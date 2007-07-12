using System;

namespace Narlie.Compiler.CodeParser
{
    public class NilToken : Token
    {
        public NilToken() : base(Tag.Nil)
        {
        }

        public override string ToString()
        {
            return "'nil";
        }
    }
}
