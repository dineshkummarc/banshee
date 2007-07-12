using System;
using System.Reflection;
using System.Reflection.Emit;

using Mono.CodeGeneration;
using Narlie.Compiler.Symbols;

namespace Narlie.Compiler.CodeGen
{
    public class AssemblyGenerator
    {        
        private AssemblyBuilder asm_builder;
        private ModuleBuilder entry_module;
        private bool verbose = false;
        
        public Type Generate(Node node)
        {
            return Generate(node, AssemblyBuilderAccess.Run, null);
        }
        
        public Type Generate(Node node, string outputPath)
        {
            return Generate(node, outputPath == null ? 
                AssemblyBuilderAccess.Run : 
                AssemblyBuilderAccess.RunAndSave, outputPath);
        }
        
        public Type Generate(Node node, AssemblyBuilderAccess access)
        {
            return Generate(node, access, null);
        }
        
        public Type Generate(Node node, AssemblyBuilderAccess access, string outputPath)
        {
            AssemblyName asm_name = new AssemblyName();
            asm_name.Name = "Narlie.Generated";
            
            asm_builder = AppDomain.CurrentDomain.DefineDynamicAssembly(asm_name, access);
            
            entry_module = asm_builder.DefineDynamicModule(outputPath == null
                ? asm_name.Name 
                : System.IO.Path.GetFileName(outputPath));

            CodeClass entry_class_gen  = new CodeClass(entry_module, "__narlie_generated_root", 
                TypeAttributes.Public | 
                TypeAttributes.Sealed |
                TypeAttributes.Abstract |
                TypeAttributes.BeforeFieldInit,
                typeof(object)
            );
            
            CodeMethod entry_method_gen = entry_class_gen.CreateStaticMethod("Main", typeof(void));
            
            MCGenerator ig = new MCGenerator();
            ig.Generate(entry_class_gen, entry_method_gen, node);
            
            if(verbose) {
                Console.WriteLine(entry_class_gen.PrintCode());
            }
            
            Type entry_type_build = entry_class_gen.CreateType();
            
            if(outputPath != null) {
                asm_builder.SetEntryPoint(entry_method_gen.MethodInfo, PEFileKinds.ConsoleApplication);
                asm_builder.Save(outputPath);
            }
            
            return entry_type_build;
        }
        
        public AssemblyBuilder AssemblyBuilder {
            get { return asm_builder; }
        }
        
        public bool Verbose {
            get { return verbose; }
            set { verbose = value; }
        }
    }
}
