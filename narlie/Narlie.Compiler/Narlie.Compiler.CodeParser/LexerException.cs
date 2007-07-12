using System;

namespace Narlie.Compiler.CodeParser
{
    public class LexerException : ApplicationException
    {
        public LexerException(string message) : base(message)
        {
        }
    }
}
