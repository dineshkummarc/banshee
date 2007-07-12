using System;

namespace Narlie.Compiler
{
    public class CompilerException : ApplicationException
    {
        public CompilerException(string message) : base(message)
        {
        }
        
        public CompilerException(string message, params object [] args) : base(String.Format(message, args))
        {
        }
    }
}