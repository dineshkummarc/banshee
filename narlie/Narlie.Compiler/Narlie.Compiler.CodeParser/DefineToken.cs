using System;

namespace Narlie.Compiler.CodeParser
{
    public class DefineToken : Token
    {
        public DefineToken() : base(Tag.Define)
        {
        }

        public override string ToString()
        {
            return "[define]";
        }
    }
}
