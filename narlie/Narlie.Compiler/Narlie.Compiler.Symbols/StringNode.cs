using System;

namespace Narlie.Compiler.Symbols
{
    public class StringNode : LiteralNode<string>
    {
        public StringNode(string value) : base(value)
        {
        }
        
        public override string ToString()
        {
            return String.Format("\"{0}\"", Value);
        }
    }
}
