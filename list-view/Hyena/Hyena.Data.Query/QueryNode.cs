using System;

namespace Banshee.Data.Query
{
    public class QueryNode
    {
        private QueryListNode parent;
        private int source_column;
        private int source_line;
        
        public QueryNode()
        {
        }
        
        public QueryNode(QueryListNode parent)
        {
            Parent = parent;
            Parent.AddChild(this);
        }
        
        protected void PrintIndent(int depth)
        {
            Console.Write(String.Empty.PadLeft(depth * 2, ' '));
        }
        
        public void Dump()
        {
            Dump(0);
        }
        
        internal virtual void Dump(int depth)
        {
            PrintIndent(depth);
            Console.WriteLine(this);
        }
        
        public QueryListNode Parent {
            get { return parent; }
            set { parent = value; }
        }
        
        public int SourceColumn {
            get { return source_column; }
            set { source_column = value; }
        }
        
        public int SourceLine {
            get { return source_line; }
            set { source_line = value; }
        }
    }
}
