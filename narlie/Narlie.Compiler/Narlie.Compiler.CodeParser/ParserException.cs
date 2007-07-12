using System;

using Narlie.Compiler.Symbols;

namespace Narlie.Compiler.CodeParser
{
    public class ParserException : Narlie.Compiler.CompilerException
    {
        public ParserException(string message) : this(null, message)
        {
        }
        
        public ParserException(Node node, string message) : this(node, message, null)
        {
        }
        
        public ParserException(Node node, string message, params object [] args) : base(String.Format(
            "Parser error: {0}{1}", (args == null ? message : String.Format(message, args)), node != null ? 
                String.Format("; token `{0}' ({1}:{2})", node.ToString(), node.SourceLine, node.SourceColumn) :
                String.Empty))
        {
        }
    }
}
