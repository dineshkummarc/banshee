using System;
using System.Collections.Generic;

using Mono.CodeGeneration;

namespace Narlie.Compiler.Symbols
{
    public class SymbolTable 
    {
        private class Reference
        {
            public Reference(Node node)
            {
                this.node = node;
            }
        
            public Node node;
            public CodeVariableReference gen_reference;
        }
    
        private Dictionary<string, Reference> symbols = new Dictionary<string, Reference>();
        private SymbolTable parent;
        
        public SymbolTable()
        {
        }
        
        public SymbolTable(SymbolTable parent)
        {
            Parent = parent;
        }
        
        public void Register(string id, Node value)
        {
            Register(id, value, false);
        }
        
        public void Register(string id, Node value, bool on_parent)
        {
            SymbolTable table = on_parent && Parent != null ? Parent : this;
        
            lock(table.symbols) {
                if(table.symbols.ContainsKey(id)) {
                    table.symbols[id].node = value;
                } else {
                    table.symbols.Add(id, new Reference(value));
                }
            }
        }
        
        public void Unregister(string id)
        {
            lock(symbols) {
                if(symbols.ContainsKey(id)) {
                    symbols.Remove(id);
                }
            }
        }
        
        private Reference LookupReference(string id)
        {
            SymbolTable table = this;
            
            while(table != null) {
                if(table.symbols.ContainsKey(id)) {
                    return table.symbols[id];
                }
                
                table = table.Parent;
            }
            
            return null;
        }
        
        public Node Lookup(string id)
        {
            Reference reference = LookupReference(id);
            return reference == null ? null : reference.node;
        }
        
        public CodeVariableReference LookupGeneratorReference(string id)
        {
            Reference reference = LookupReference(id);
            return reference == null ? null : reference.gen_reference;
        }
        
        public void SetGeneratorReference(string id, CodeVariableReference gen_reference)
        {
            Reference reference = LookupReference(id);
            if(reference != null) {
                reference.gen_reference = gen_reference;
                return;
            }
            
            throw new CompilerException("Cannot find codegen reference to `{0}'", id);
        }
        
        public void Dump()
        {
            Dump(0);
        }
        
        private void Dump(int depth)
        {
            Console.WriteLine("--- Symbol Table {0} ---", depth);
            foreach(string id in symbols.Keys) {
                Console.WriteLine(id);
            }
            
            if(Parent != null) {
                Parent.Dump(depth + 1);
            }
        }

        public SymbolTable Parent {
            get { return parent; }
            set { parent = value; }
        }
    }
}
