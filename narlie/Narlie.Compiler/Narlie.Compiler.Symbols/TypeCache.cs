using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Narlie.Compiler.Symbols
{
    public class TypeCache
    {
        private AssemblyCache assembly_cache;
        private List<string> namespaces = new List<string>();
        private Dictionary<string, Type> types = new Dictionary<string, Type>();
        
        public TypeCache(AssemblyCache assembly_cache)
        {
            this.assembly_cache = assembly_cache;
            
            Using("System");
            Using("Narlie.Runtime");
        }
        
        public void Using(string @namespace)
        {
            namespaces.Add(@namespace);
        }
        
        public Type FindType(string name)
        {
            string lname = name.ToLower();
            
            if(types.ContainsKey(lname)) {
                return types[lname];
            }
            
            Type type = _FindType(name);
            if(type != null) {
                types.Add(lname, type);
                return type;
            }
            
            return null;
        }
        
        private Type _FindType(string name)
        {
            Type type = Type.GetType(name, false, true);
            if(type != null) {
                return type;
            }
            
            foreach(Assembly asm in assembly_cache) {
                type = asm.GetType(name, false, true);
                if(type != null) {
                    return type;
                }
            }
            
            foreach(string @namespace in namespaces) {
                foreach(Assembly asm in assembly_cache) {
                    type = asm.GetType(String.Format("{0}.{1}", @namespace, name), false, true);
                    if(type != null) {
                        return type;
                    }
                    
                    type = Type.GetType(String.Format("{0}.{1}", @namespace, name), false, true);
                    if(type != null) {
                        return type;
                    }
                }
            }
            
            return null;
        }
    }
}
