using System;
using System.IO;

using Narlie.Compiler.Symbols;
using Narlie.Compiler.CodeParser;
using Narlie.Compiler.CodeGen;

using Narlie.Runtime;

namespace Narlie.Compiler
{
    public class NarlieCompiler
    {
        private static Type [] narlie_runtime_types = {
            typeof(ArithmeticFunctionSet),
            typeof(LogicFunctionSet),
            typeof(UtilityFunctionSet), 
            typeof(CompareFunctionSet)
        };
    
        private Lexer lexer;
        private Parser parser;
        private bool verbose = false;
        
        private Type result_type;
        private Node root_node;
        private CompilerException compiler_exception;

        public NarlieCompiler(bool verbose) : this()
        {
            this.verbose = verbose;
        }
        
        public NarlieCompiler()
        {
            lexer = new Lexer();
            parser = new Parser(lexer);
        }
        
        public void SetInput(StreamReader reader)
        {
            lexer.SetInput(reader);
        }
        
        public void SetInput(Stream stream)
        {
            lexer.SetInput(stream);
        }
        
        public void SetInput(string input)
        {
            lexer.SetInput(input);
        }
        
        public bool Compile()
        {
            return Compile(null);
        }
        
        public bool Compile(string assembly_path)
        {
            parser.Reset();
            compiler_exception = null;
            result_type = null;
            root_node = null;
            
            foreach(Type type in narlie_runtime_types) {
                parser.RegisterFunctionSet(type);
            }
            
            try {
                root_node = parser.Parse();
                
                if(verbose) {
                    root_node.Dump();
                }
                
                AssemblyGenerator code_gen = new AssemblyGenerator();
                code_gen.Verbose = verbose;
                
                result_type = code_gen.Generate(root_node, assembly_path);
                
                return true;
            } catch(CompilerException e) {
                compiler_exception = e;
            }
            
            return false;
        }
        
        public Type ResultType {
            get { return result_type; }
        }
        
        public Node RootAstNode {
            get { return root_node; }
        }
        
        public CompilerException Error {
            get { return compiler_exception; }
        }
        
        public string ErrorMessage {
            get { return compiler_exception.Message; }
        }
    }
}
