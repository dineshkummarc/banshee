using System;

namespace Narlie.Compiler.CodeParser
{
    public enum Tag {
        Scope,
        Keyword,
        Define,
        Id,
        Nil,
        Boolean,
        Integer,
        Real,
        String,
        If,
        For,
        While,
        Unknown
    }

    public class Token
    {
        private Tag tag;
        private char unknown_token = Char.MaxValue;

        private int line;
        private int column;

        public Token(Tag tag)
        {
            this.tag = tag;
        }

        public Token(char token)
        {
            tag = Tag.Unknown;
            unknown_token = token;
        }

        public override string ToString()
        {
            return tag == Tag.Unknown ? unknown_token.ToString() : tag.ToString();
        }

        public Tag Tag {
            get { return tag; }
        }

        public int SourceLine {
            get { return line; }
            set { line = value; }
        }

        public int SourceColumn {
            get { return column; }
            set { column = value; }
        }
    }
}
