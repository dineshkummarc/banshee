using System;

namespace Narlie.Compiler.CodeParser
{
    public class RealToken : Token
    {
        private double value;

        public RealToken(double value) : base(Tag.Real)
        {
            this.value = value;
        }
        
        public override string ToString()
        {
            return value.ToString();
        }

        public double Value {
            get { return value; }
        }
    }
}
