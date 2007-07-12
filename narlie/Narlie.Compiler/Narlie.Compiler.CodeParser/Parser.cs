using System;
using System.Reflection;

using Narlie.Compiler.Symbols;
using Narlie.Runtime;

namespace Narlie.Compiler.CodeParser
{
    public class Parser
    {
        private Lexer lexer;
        private TypeCache type_cache;
        
        private ListNode root_node;
        private ListNode current_parent;
        private int scope_depth = 0;
        private Token rewind_token = null;
        
        public Parser(Lexer lexer)
        {
            this.lexer = lexer;
        }
        
        public void RegisterFunction(string id, FunctionHandler handler)
        {
            root_node.SymbolTable.Register(id, new FunctionNode(handler));
        }
        
        public void RegisterFunctionSet(Type type)
        {
            foreach(MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public)) {
                FunctionAttribute fn_attr = null;
               
                foreach(Attribute attr in method.GetCustomAttributes(false)) {
                    if(attr is FunctionAttribute) {
                        fn_attr = (FunctionAttribute)attr;
                        break;
                    }
                }
               
                if(fn_attr == null || fn_attr.Names == null || fn_attr.Names.Length == 0) {
                    continue;
                }
               
                FunctionNode node = new FunctionNode(fn_attr.ArgsRestriction, fn_attr.ArgsMin, 
                    fn_attr.ArgsMax, method);
               
                foreach(string name in fn_attr.Names) {
                    root_node.SymbolTable.Register(name, node);
                }
            }
        }
        
        public void Reset()
        {
            root_node = current_parent = new ListNode();
            root_node.SymbolTable = new SymbolTable();
            
            type_cache = new TypeCache(new AssemblyCache());
            
            scope_depth = 0;
            rewind_token = null;
            
            lexer.Reset();
        }
        
        public Node Parse()
        {
            ParseLoop(false, 0);
            
            if(scope_depth != 0) {
                throw new ParserException(current_parent, "Scope does not pop back to zero");
            }
            
            return current_parent;
        }
        
        private void ParseLoop(bool break_on_scope_pop, int break_depth)
        {
            while(true) {
                Token token = rewind_token;
                if(rewind_token == null) {
                    token = lexer.Scan();
                } else {
                    rewind_token = null;
                }
                
                if(token.Tag == Tag.Unknown) {
                    break;
                }
                
                if(ParseToken(token) && break_on_scope_pop && break_depth == scope_depth) {
                    break;
                }
            }
        }
        
        private void ScopePush()
        {
            ScopePush(new ListNode(current_parent));
        }
        
        private void ScopePush(ListNode node)
        {
            SymbolTable symbol_table = new SymbolTable(current_parent.SymbolTable);
            current_parent = node;
            current_parent.SymbolTable = symbol_table;
            scope_depth++;
        }
        
        private void ScopePop()
        {
            current_parent = current_parent.Parent;
            scope_depth--;
        }
        
        private void ChildPush(Token token, Node node)
        {
            node.SourceLine = token.SourceLine;
            node.SourceColumn = token.SourceColumn;
            
            if(node is IdNode) {
                ((IdNode)node).SymbolTable = new SymbolTable(current_parent.SymbolTable);
            }
            
            current_parent.AddChild(node);
        }
        
        private void ScopeTrim(ListNode node)
        {
            // OPTIMIZATION: Removes unnecessary extra parent scope
            if(node.ChildCount > 0) {
                ListNode parent = node.Parent;
                ListNode grandparent = parent.Parent;
                
                if(grandparent != null) {
                    grandparent.ReplaceChild(parent, node);
                }
            }        
        }
        
        private bool ParseToken(Token token)
        {
            if(token is ScopeToken) {
                if(((ScopeToken)token).Action == ScopeAction.Push) {
                    ScopePush();
                } else {
                    ScopePop();
                    return true;
                }
            } else if(token is IdToken) {
                ParseId((IdToken)token);
            } else if(token is DefineToken) {
                ParseDefine();
            } else if(token is StringToken) {
                ChildPush(token, new StringNode(((StringToken)token).Value));
            } else if(token is RealToken) {
                ChildPush(token, new RealNode(((RealToken)token).Value));
            } else if(token is IntegerToken) {
                ChildPush(token, new IntegerNode(((IntegerToken)token).Value));
            } else if(token is BooleanToken) {
                ChildPush(token, new BooleanNode(((BooleanToken)token).Value));
            } else if(token is NilToken) {
                ChildPush(token, new NilNode());
            } else if(token is KeywordToken) {
                switch(token.Tag) {
                    case Tag.If:
                        ParseIf();
                        break;
                    case Tag.For:
                        ParseFor();
                        break;
                    case Tag.While:
                        ParseWhile();
                        break;
                    default:
                        break;
                }
            }
            
            return false;
        }
        
        private void ParseId(IdToken token)
        {
            Node id_node = current_parent.SymbolTable.Lookup(token.Value);
            FunctionNode function_node = id_node as FunctionNode;
            bool have_virtual_function = false;
            
            foreach(string vf in Lexer.VirtualFunctions) {
                if(token.Value == vf) {
                    have_virtual_function = true;
                    break;
                }
            }
            
            if(!have_virtual_function) {
                if(function_node == null) {
                    if(current_parent.ChildCount != 0 || !ParseNetMethod(token)) {
                        ChildPush(token, new IdNode(token.Value));
                    }
                    return;
                } else if(function_node.ArgsRestriction == FunctionArgs.Empty) {
                    ChildPush(token, new IdNode(token.Value));
                    return;
                }
            }
            
            if(current_parent.ChildCount != 0) {
                throw new ParserException(current_parent, "Functions accepting arguments must be the first child in a scope");
            }
            
            IdNode function_body = new IdNode(current_parent, token.Value);
            ScopePush(function_body);
            
            ParseLoop(true, scope_depth - 1);
            
            if(!have_virtual_function) {
                ValidateFunctionArguments(token.Value, function_node, function_body);
            } else {
                ValidateVirtualFunction(token.Value, function_body);
            }
            
            ScopeTrim(function_body);
            ScopePop();
        }
        
        private void ValidateFunctionArguments(string id, FunctionNode function_node, IdNode function_body)
        { 
            if(function_node.ArgsRestriction == FunctionArgs.Fixed && 
                function_body.ChildCount != function_node.ArgsMin) {
                throw new ParserException(function_body, "Function '{0}' expects {1} arguments",
                    id, function_node.ArgsMin);
            }
            
            if(function_node.ArgsRestriction == FunctionArgs.Range &&
                (function_body.ChildCount < function_node.ArgsMin || 
                function_body.ChildCount > function_node.ArgsMax)) {
                throw new ParserException(function_body, "Function '{0}' expects {1} through {2} arguments",
                    id, function_node.ArgsMin, function_node.ArgsMax);
            } 
        }
        
        // "virtual" functions do not resolve to anything in a symbol 
        // table and are handled specially by the code generator
        private void ValidateVirtualFunction(string id, IdNode function_body)
        {
            if(id == "using") {
                if(function_body.ChildCount < 1) {
                    throw new ParserException(function_body, "`using' takes at least one ID");
                }
            
                foreach(Node arg_node in function_body.Children) {
                    if(!(arg_node is IdNode)) {
                        throw new ParserException(arg_node, "`using' takes a list of namespaces as IDs");
                    }
                    
                    type_cache.Using(((IdNode)arg_node).Id);
                }
            } else if(id == "let") {
                ParseLet(function_body);
            } else if(id == "set" || id == "setf") {
                ParseSet(function_body);
            }
        }
        
        private bool ParseNetMethod(IdToken method_token)
        {
            Token lex_token = lexer.Scan();
            
            if(lex_token.Tag == Tag.Unknown || !(lex_token is IdToken)) {
                rewind_token = lex_token;
                return false;
            }
            
            IdToken type_token = (IdToken)lex_token;
            
            Type method_type = type_cache.FindType(type_token.Value);
            if(method_type == null) {
                rewind_token = lex_token;
                return false;
            }
            
            MethodInfo method_info = null;
            bool found = false;
            
            try {
                method_info = method_type.GetMethod(method_token.Value,
                    BindingFlags.Public | 
                    BindingFlags.Static |
                    BindingFlags.IgnoreCase);
                if(method_info != null) {
                    found = true;
                }
            } catch(AmbiguousMatchException) {
                found = true;
            } 
            
            if(!found) {
                rewind_token = lex_token;
                return false;
            }
            
            NetMethodNode function_body = new NetMethodNode(current_parent, method_type, 
                method_info, method_token.Value);
            ScopePush(function_body);
            
            ParseLoop(true, scope_depth - 1);
            
            ScopeTrim(function_body);
            ScopePop();
            
            return true;
        }
        
        private ControlNode ControlPushParse(ListNode parent, Tag tag)
        {
            ControlNode node = new ControlNode(parent, tag);
            ScopePush(node);
            
            ParseLoop(true, scope_depth - 1);
            
            return node;
        }
        
        private void ControlPop(ControlNode node)
        {
            ScopeTrim(node);
            ScopePop();
        }
        
        private void ParseIf()
        {
            ControlNode node = ControlPushParse(current_parent, Tag.If);
            
            if(node.ChildCount < 2 || node.ChildCount > 3) {
                throw new ParserException(node, "Invalid `if' statement; (if <condition> <true-block> <false-block>)");
            }
            
            ControlPop(node);
        }
        
        private void ParseFor()
        {
            ControlNode node = ControlPushParse(current_parent, Tag.For);
            
            if(node.ChildCount != 4) {
                throw new ParserException(node, "Invalid `for' statement; (for <pre-block> <condition> <post-block> <true-block>)");
            }
            
            // re-arrange so that the AST satisfies design of ControlNode
            Node condition = node.Children[1];
            node.Children[1] = node.Children[0];
            node.Children[0] = condition;
            
            ControlPop(node);
        }
        
        private void ParseWhile()
        {
            ControlNode node = ControlPushParse(current_parent, Tag.While);
            
            if(node.ChildCount != 2) {
                throw new ParserException(node, "Invalid `while' statement; (while <condition> <true-block>)");
            }
            
            ControlPop(node);
        }
        
        private void ParseDefine()
        {
            throw new NotImplementedException("define");
        }
        
        private void ParseLet(IdNode node)
        {
            if(node.ChildCount == 2 && node.Children[0] is IdNode) {
                node.Parent.SymbolTable.Register(((IdNode)node.Children[0]).Id, node.Children[1], true);
                return;
            }
        
            foreach(Node child in node.Children) {
                if(!(child is ListNode)) {
                    throw new ParserException(child, "Invalid `let' statement; (let (<id0> <val0>) ... (<idN> <valN>))");
                }
                
                ListNode tuple = (ListNode)child;
                
                if(tuple.ChildCount != 2 || !(tuple.Children[0] is IdNode)) {
                    throw new ParserException(tuple, "Invalid tuple or identifier in `let' statement");
                }
                
                IdNode id = (IdNode)tuple.Children[0];
                Node value = tuple.Children[1];
                
                node.Parent.SymbolTable.Register(id.Id, value, true);
            }
        }
        
        private void ParseSet(IdNode node)
        {
            if(node.ChildCount != 2 || !(node.Children[0] is IdNode)) {
                throw new ParserException(node, "Invalid `set' statement; (set <id> <val>)");
            }
            
            IdNode id = (IdNode)node.Children[0];
            if(node.Parent.SymbolTable.Lookup(id.Id) == null) {
                throw new ParserException(id, "Id `{0}' is undefined in this scope", id.Id);
            }
            
            node.Id = "set";
        }
    }
}
