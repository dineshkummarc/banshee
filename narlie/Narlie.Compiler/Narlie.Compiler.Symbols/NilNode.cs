using System;

namespace Narlie.Compiler.Symbols
{
    public class NilNode : LiteralBaseNode
    {
        public NilNode() : base()
        {
        }
        
        public override string ToString()
        {
            return "[nil]";
        }
    }
}
