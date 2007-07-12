using System;

namespace Narlie.Compiler.Symbols
{
    public class LiteralNode<T> : LiteralBaseNode
    {
        private T value;
        
        public LiteralNode(T value) : base()
        {
            this.value = value;
        }
        
        public override string ToString()
        {
            return Value.ToString();
        }
        
        public T Value {
            get { return value; }
            set { this.value = value; }
        }
        
        public override object ObfuscatedValue {
            get { return value; }
        }
    }
}
