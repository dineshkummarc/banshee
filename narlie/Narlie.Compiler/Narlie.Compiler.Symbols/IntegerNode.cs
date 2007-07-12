using System;

namespace Narlie.Compiler.Symbols
{
    public class IntegerNode : LiteralNode<int>
    {
        public IntegerNode(int value) : base(value)
        {
        }
    }
}
