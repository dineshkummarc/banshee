using System;

namespace Narlie.Compiler.Symbols
{
    public class LiteralBaseNode : Node
    {
        public LiteralBaseNode() : base()
        {
        }
        
        public virtual object ObfuscatedValue {
            get { return null; }
        }
    }
}
