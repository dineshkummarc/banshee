using System;

using Narlie.Compiler;

namespace Narlie.Compiler.Narc
{
    public static class NarcFrontend
    {
        public static int Main()
        {
            NarlieCompiler compiler = new NarlieCompiler(true);
            compiler.SetInput(Console.OpenStandardInput());
            if(compiler.Compile()) {
                compiler.ResultType.GetMethod("Main").Invoke(null, null);
                return 0;
            } else {
                Console.WriteLine("ERROR: {0}", compiler.ErrorMessage);
                return 1;
            }
        }
    }
}
