using System;

namespace Narlie.Compiler.CodeParser
{
    public class IdToken : Token
    {
        private string value;
        
        public IdToken(string value) : base(Tag.Id)
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
