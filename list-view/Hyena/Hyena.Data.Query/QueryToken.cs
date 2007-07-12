using System;

namespace Banshee.Data
{
    public enum TokenID {
        Unknown,
        OpenParen,
        CloseParen,
        Not,
        Or,
        Range,
        Term
    }
    
    public class QueryToken
    {
        private TokenID id;
        private int line;
        private int column;
        private string term;

        public QueryToken()
        {
        }

        public QueryToken(string term)
        {
            this.id = TokenID.Term;
            this.term = term;
        }

        public QueryToken(TokenID id)
        {
            this.id = id;
        }

        public QueryToken(TokenID id, int line, int column)
        {
            this.id = id;
            this.line = line;
            this.column = column;
        }

        public TokenID ID {
            get { return id; }
            set { id = value; }
        }

        public int Line {
            get { return line; }
            set { line = value; }
        }

        public int Column {
            get { return column; }
            set { column = value; }
        }

        public string Term {
            get { return term; }
            set { term = value; }
        }
    }
}