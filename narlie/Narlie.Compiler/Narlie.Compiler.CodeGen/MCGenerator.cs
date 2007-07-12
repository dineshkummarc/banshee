using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using Mono.CodeGeneration;
using Narlie.Compiler.Symbols;
using Narlie.Compiler.CodeParser;
using Narlie.Runtime;

namespace Narlie.Compiler.CodeGen
{
    public class MCGenerator
    {
        private CodeBuilder builder;
        private Stack<List<CodeExpression>> expression_stack = new Stack<List<CodeExpression>>();
        
        public MCGenerator()
        {
        }
        
        public void Generate(CodeClass cls, CodeMethod method, Node node)
        {
            builder = method.CodeBuilder;
            Visit(node);
        }
        
        private void PushExpressionBlock()
        {
            expression_stack.Push(new List<CodeExpression>());
        }
        
        private void EmitExpressionBlock()
        {
            foreach(CodeExpression expression in expression_stack.Pop()) {
                builder += expression;
            }
        }

        private void Visit(Node node)
        {
            if(node is IdNode) {
                VisitId((IdNode)node);
            } else if(node is ControlNode) {
                EmitControl((ControlNode)node);
            } else if(node is ListNode) {
                foreach(Node child in ((ListNode)node).Children) {
                    Visit(child);
                }
            } else if(node is LiteralBaseNode) {
                List<CodeExpression> args_list = expression_stack.Peek();
                if(args_list != null) {
                    args_list.Add(new CodeLiteral(((LiteralBaseNode)node).ObfuscatedValue));
                }
            }
        }
        
        private void VisitId(IdNode node)
        {
            if(node is NetMethodNode) {
                EmitNetMethodCall((NetMethodNode)node);
                return;
            }
        
            Node id_node = node.Parent.SymbolTable.Lookup(node.Id);
            
            if(id_node is FunctionNode) {
                EmitFunctionCall((FunctionNode)id_node, node);
            } else if(node.Id == "let") {
                EmitLet(node, node.Parent.SymbolTable);
            } else if(node.Id == "set") {
                EmitSet((IdNode)node.Children[0], node.Children[1], node.Parent.SymbolTable);
            } else {
                EmitCodeVariableReference(node);
            }
        }
        
        private void EmitNetMethodCall(NetMethodNode node)
        {
            MethodInfo method_info = node.MethodInfo;
            
            PushExpressionBlock();
            
            for(int i = 0; i < node.ChildCount; i++) {
                Visit(node.Children[i]);
            }
            
            if(method_info == null) {
                List<CodeExpression> method_args = expression_stack.Peek();
                Type [] types = new Type[method_args.Count];
                for(int i = 0; i < method_args.Count; i++) {
                    types[i] = method_args[i].GetResultType();
                }
            
                method_info = node.MethodType.GetMethod(node.MethodName, 
                    BindingFlags.Public | 
                    BindingFlags.Static |
                    BindingFlags.IgnoreCase,
                    null, types, null);
            
                if(method_info == null) {
                    throw new ApplicationException(String.Format("Unknown CLR method `{0}::{1}'", 
                        node.MethodType.FullName, node.MethodName));
                }
            }
            
            EmitFunctionCallByMethodInfo(null, method_info);
        }
        
        private void EmitFunctionCall(FunctionNode fn_node, IdNode ast_node)
        {
            PushExpressionBlock();
            
            foreach(Node arg_node in ast_node.Children) {
                Visit(arg_node);
            }
            
            if(fn_node.Handler is MethodInfo) {
                EmitFunctionCallByMethodInfo(fn_node, (MethodInfo)fn_node.Handler);
                return;
            }
            
            throw new ApplicationException(String.Format(
                "Unsupported method handler type: {0}", fn_node.Handler.GetType()));
        }
        
        private void EmitFunctionCallByMethodInfo(FunctionNode fn_node, MethodInfo method_info)
        {
            List<CodeExpression> method_args = expression_stack.Pop();
            CodeMethodCall method_call;
        
            // runtime functions that are variable are basicall "params", so we have to box the
            // function arguments into an object[] array and pass that to the function instead
            if(fn_node != null && fn_node.ArgsRestriction == FunctionArgs.Variable) {
                CodeVariableDeclaration array_decl = new CodeVariableDeclaration(typeof(object []), "arg");
                CodeNewArray array = new CodeNewArray(typeof(object), Exp.Literal(method_args.Count));
                
                builder.CurrentBlock.Add(array_decl);
                builder.Assign(array_decl.Variable, array);
                
                for(int i = 0; i < method_args.Count; i++) {
                    CodeArrayItem arg = new CodeArrayItem(array_decl.Variable, Exp.Literal(i));
                    builder.Assign(arg, method_args[i]);
                }
                
                method_call = new CodeMethodCall(method_info, array_decl.Variable);
            } else {
                method_call = new CodeMethodCall(method_info, method_args.ToArray());
            }
            
            if(expression_stack.Count > 0) {
                expression_stack.Peek().Add(method_call);
            } else {
                builder += method_call;
            }
        }
        
        private void EmitControl(ControlNode node)
        {
            if(node.Tag == Tag.If) {
                EmitIf(node);
            } else if(node.Tag == Tag.For) {
                EmitFor(node);
            } else if(node.Tag == Tag.While) {
                EmitWhile(node);
            }
        }
       
        private CodeExpression VisitPopSingleExpression(Node node)
        {
            PushExpressionBlock();
            Visit(node);
            
            List<CodeExpression> condition_list = expression_stack.Pop();
            if(condition_list == null || condition_list.Count != 1) {
                throw new InvalidOperationException("requires exactly one valid expression");
            }
            
            return condition_list[0];
        }
        
        private void EmitIf(ControlNode node)
        {
            CodeExpression condition = VisitPopSingleExpression(node.Condition);
            
            if(condition.GetResultType() != typeof(bool)) {
                // TODO: inject a call to cast-bool
            }
            
            builder.If(condition);
            
            PushExpressionBlock();
            Visit(node.Block1);
            EmitExpressionBlock();
            
            if(node.Block2 != null) {
                builder.Else();
                
                PushExpressionBlock();
                Visit(node.Block2);
                EmitExpressionBlock();
            }
            
            builder.EndIf();
        }
        
        private void EmitFor(ControlNode node)
        {
            CodeExpression condition = VisitPopSingleExpression(node.Condition);
            CodeExpression pre = null;
            CodeExpression post = null;
        
            if(condition.GetResultType() != typeof(bool)) {
                // TODO: inject a call to cast-bool
            }
        
            if(node.Block1 != null && !(node.Block1 is NilNode)) {
                pre = VisitPopSingleExpression(node.Block1);
            }
        
            if(node.Block2 != null && !(node.Block2 is NilNode)) {
                post = VisitPopSingleExpression(node.Block2);
            }
            
            builder.For(pre, condition, post);
            
            PushExpressionBlock();
            Visit(node.Block3);
            EmitExpressionBlock();
            
            builder.EndFor();
        }
        
        private void EmitWhile(ControlNode node)
        {
            CodeExpression condition = VisitPopSingleExpression(node.Condition);
                    
            if(condition.GetResultType() != typeof(bool)) {
                // TODO: inject a call to cast-bool
            }
            
            builder.While(condition);
            
            PushExpressionBlock();
            Visit(node.Block1);
            EmitExpressionBlock();
            
            builder.EndWhile();
        }
        
        private void EmitLet(IdNode node, SymbolTable symbol_table)
        {
            if(node.ChildCount == 2 && node.Children[0] is IdNode) {
                EmitLet((IdNode)node.Children[0], node.Children[1], symbol_table);
                return;
            }
        
            foreach(Node child in node.Children) {
                ListNode child_list = (ListNode)child;
                EmitLet((IdNode)child_list.Children[0], child_list.Children[1], symbol_table);
            }
        }
        
        private void EmitLet(IdNode id, Node value, SymbolTable symbol_table)
        {
            CodeVariableReference reference = builder.DeclareVariable(typeof(object), VisitPopSingleExpression(value));
            symbol_table.SetGeneratorReference(id.Id, reference);
        }
        
        private void EmitSet(IdNode id, Node value, SymbolTable symbol_table)
        {
            CodeVariableReference reference = symbol_table.LookupGeneratorReference(id.Id);
            if(Object.ReferenceEquals(reference, null)) {
                throw new CompilerException("Cannot find codegen reference to `{0}'", id);
            }
            
            builder.Assign(reference, VisitPopSingleExpression(value));
        }
        
        private void EmitCodeVariableReference(IdNode id)
        {
            CodeVariableReference reference = id.SymbolTable.LookupGeneratorReference(id.Id);
            if(Object.ReferenceEquals(reference, null)) {
                throw new CompilerException("Cannot find codegen reference to `{0}'", id);
            }
            
            expression_stack.Peek().Add(reference);
        }
    }
}
