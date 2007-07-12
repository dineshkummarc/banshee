using System;

namespace Narlie.Compiler.CodeParser
{
    public class KeywordToken : Token
    {
        public KeywordToken(Tag tag) : base(tag)
        {
        }
        
        public override string ToString()
        {
            return Tag.ToString();
        }
    }
}
