using System;

namespace Narlie.Compiler.Symbols
{
    public class BooleanNode : LiteralNode<bool>
    {
        public BooleanNode(bool value) : base(value)
        {
        }
    }
}
