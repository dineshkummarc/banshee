using System;

namespace Narlie.Compiler.CodeParser
{
    public class StringToken : Token
    {
        private string value;
        
        public StringToken(string value) : base(Tag.String)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value;
        }

        public string Value {
            get { return value; }
        }
    }
}
