using System;

namespace Narlie.Compiler.Symbols
{
    public class IdNode : ListNode
    {
        private string id;
    
        public IdNode(string id) : base()
        {
            this.id = id;
        }    
        
        public IdNode(ListNode parent, string id) : base(parent)
        {
            this.id = id;
        }
        
        public override string ToString()
        {
            return String.Format("<{0}>", Id);
        }
        
        public string Id {
            get { return id; }
            set { id = value; }
        }
    }
}
