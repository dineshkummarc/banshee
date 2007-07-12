using System;

namespace Banshee.Data.Query
{
    public enum Keyword {
        Not,
        Or,
        And
    }
    
    public class QueryKeywordNode : QueryNode
    {
        private Keyword keyword;
        
        public QueryKeywordNode(Keyword keyword) : base()
        {
            this.keyword = keyword;
        }
        
        public override string ToString()
        {
            return String.Format("<{0}>", Keyword);
        }
        
        public Keyword Keyword {
            get { return keyword; }
            set { keyword = value; }
        }
    }
}
