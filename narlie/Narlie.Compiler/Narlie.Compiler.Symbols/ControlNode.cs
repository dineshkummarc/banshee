using System;

using Narlie.Compiler.CodeParser;

namespace Narlie.Compiler.Symbols
{
    public class ControlNode : ListNode
    {
        private Tag tag;
    
        public ControlNode(ListNode parent, Tag tag) : base(parent)
        {
            this.tag = tag;
        }
        
        public override string ToString()
        {
            return String.Format("<@{0}>", tag);
        }
        
        public Tag Tag {
            get { return tag; }
        }
        
        public Node Condition {
            get { return Children[0]; }
        }
        
        public Node Block1 {
            get { return Children[1]; }
        }
        
        public Node Block2 {
            get { return ChildCount > 2 ? Children[2] : null; }
        }
        
        public Node Block3 {
            get { return ChildCount > 3 ? Children[3] : null; }
        }
    }
}
