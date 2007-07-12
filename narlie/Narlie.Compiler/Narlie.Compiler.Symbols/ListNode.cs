using System;
using System.Collections.Generic;

namespace Narlie.Compiler.Symbols
{
    public class ListNode : Node
    {
        private List<Node> children = new List<Node>();
        private SymbolTable symbol_table;
    
        public ListNode() : base()
        {
        }
        
        public ListNode(ListNode parent) : base(parent)
        {
        }
        
        public void AddChild(Node child)
        {
            child.Parent = this;
            children.Add(child);
        }
        
        public void RemoveChild(Node child)
        {
            child.Parent = null;
            children.Remove(child);
        }
        
        public void ReplaceChild(Node old_child, Node new_child)
        {
            int index = children.IndexOf(old_child);
            if(index < 0) {
                throw new ApplicationException("old_child does not exist");
            }
            
            children.RemoveAt(index);
            children.Insert(index, new_child);
        }
        
        public void InsertChild(int index, Node child)
        {
            child.Parent = this;
            children.Insert(index, child);
        }
        
        internal override void Dump(int depth)
        {
            PrintIndent(depth);
            
            if(this is IdNode && IsEmpty) {
                Console.WriteLine(this);
                return;
            }
            
            Console.WriteLine("{0}{{", (this is IdNode || this is ControlNode) ? this + " " : String.Empty);
            
            foreach(Node child in children) {
                child.Dump(depth + 1);
            }
            
            PrintIndent(depth);
            Console.WriteLine("}");
        }
        
        public SymbolTable SymbolTable {
            get { return symbol_table ?? (Parent != null ? Parent.SymbolTable : null); }
            set { symbol_table = value; }
        }
        
        public bool IsEmpty {
            get { return ChildCount == 0; }
        }
        
        public List<Node> Children {
            get { return children; }
        }
        
        public int ChildCount {
            get { return children.Count; }
        }
    }
}
