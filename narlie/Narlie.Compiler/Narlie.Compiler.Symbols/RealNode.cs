using System;

namespace Narlie.Compiler.Symbols
{
    public class RealNode : LiteralNode<double>
    {
        public RealNode(double value) : base(value)
        {
        }
    }
}
