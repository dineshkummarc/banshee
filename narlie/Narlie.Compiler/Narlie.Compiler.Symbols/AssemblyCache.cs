using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace Narlie.Compiler.Symbols
{
    public class AssemblyCache : IEnumerable<Assembly>
    {
        private Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>(); 
    
        public AssemblyCache()
        {
            foreach(Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
                Add(asm);
            }
        }
        
        public void Add(Assembly asm)
        {
            assemblies.Add(asm.FullName, asm);
        }
        
        public void Remove(Assembly asm)
        {
            assemblies.Remove(asm.FullName);
        }
        
        public Assembly Load(string name)
        {
            if(assemblies.ContainsKey(name)) {
                return assemblies[name];
            }
            
            Assembly asm = Path.IsPathRooted(name) ?
                Assembly.LoadFrom(name) :
                Assembly.Load(name);
            
            assemblies.Add(name, asm);
            
            return asm;
        }
        
        public IEnumerator<Assembly> GetEnumerator()
        {
            return assemblies.Values.GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return assemblies.Values.GetEnumerator();
        }
    }
}
