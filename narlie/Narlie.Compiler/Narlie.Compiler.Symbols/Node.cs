using System;

namespace Narlie.Compiler.Symbols
{
    public class Node
    {
        private ListNode parent;
        private int source_column;
        private int source_line;
        
        public Node()
        {
        }
        
        public Node(ListNode parent)
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
        
        public ListNode Parent {
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
        
        public virtual bool BooleanTest {
            get { 
                if(this is BooleanNode) {
                    return ((BooleanNode)this).Value;
                } else {
                    return false;
                }
            }
        }
    }
}
