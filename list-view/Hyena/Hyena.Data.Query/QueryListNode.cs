using System;
using System.Collections.Generic;

namespace Banshee.Data.Query
{
    public class QueryListNode : QueryNode
    {
        private List<QueryNode> children = new List<QueryNode>();
    
        public QueryListNode() : base()
        {
        }
        
        public QueryListNode(QueryListNode parent) : base(parent)
        {
        }
        
        public void AddChild(QueryNode child)
        {
            child.Parent = this;
            children.Add(child);
        }
        
        public void RemoveChild(QueryNode child)
        {
            child.Parent = null;
            children.Remove(child);
        }
        
        public void ReplaceChild(QueryNode old_child, QueryNode new_child)
        {
            int index = children.IndexOf(old_child);
            if(index < 0) {
                throw new ApplicationException("old_child does not exist");
            }
            
            children.RemoveAt(index);
            children.Insert(index, new_child);
        }
        
        public void InsertChild(int index, QueryNode child)
        {
            child.Parent = this;
            children.Insert(index, child);
        }
        
        public int IndexOfChild(QueryNode child)
        {
            return children.IndexOf(child);
        }
        
        internal override void Dump(int depth)
        {
            PrintIndent(depth);
            Console.WriteLine("{");
            
            foreach(QueryNode child in children) {
                child.Dump(depth + 1);
            }
            
            PrintIndent(depth);
            Console.WriteLine("}");
        }
        
        public QueryNode GetLeftSibling(QueryNode node)
        {
            int index = IndexOfChild(node);
            if(index >= 1) {
                return Children[index - 1];
            }
            
            return null;
        }
        
        public QueryNode GetRightSibling(QueryNode node)
        {
            int index = IndexOfChild(node);
            if(index < 0 || index >= ChildCount - 2) {
                return null;
            }
            return Children[index + 1];
        }
        
        public bool IsEmpty {
            get { return ChildCount == 0; }
        }
        
        public List<QueryNode> Children {
            get { return children; }
        }
        
        public QueryNode LastChild {
            get { return ChildCount > 0 ? children[ChildCount - 1] : null; }
        }
        
        public int ChildCount {
            get { return children.Count; }
        }
    }
}
