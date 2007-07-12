using System;

namespace Narlie.Compiler.CodeParser
{
    public class BooleanToken : Token
    {
        private bool value;

        public BooleanToken(bool value) : base(Tag.Boolean)
        {
            this.value = value;
        }
        
        public override string ToString()
        {
            return value.ToString();
        }

        public bool Value {
            get { return value; }
        }
    }
}
