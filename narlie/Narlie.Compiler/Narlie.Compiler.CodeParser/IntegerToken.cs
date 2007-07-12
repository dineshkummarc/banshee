using System;

namespace Narlie.Compiler.CodeParser
{
    public class IntegerToken : Token
    {
        private int value;
        
        public IntegerToken(int value) : base(Tag.Integer)
        {
            this.value = value;
        }
        
        public override string ToString()
        {
            return value.ToString();
        }

        public int Value {
            get { return value; }
        }
    }
}
