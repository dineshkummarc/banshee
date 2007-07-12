using System;
using System.Reflection;

using Narlie.Runtime;

namespace Narlie.Compiler.Symbols
{
    public class NetMethodNode : IdNode
    {
        private Type method_type;
        private MethodInfo method_info;
        private string method_name;
        
        public NetMethodNode(ListNode parent, Type method_type, MethodInfo method_info, string method_name) 
            : base(parent, String.Format("{0}::{1}", method_type.FullName, method_name))
        {
            this.method_type = method_type;
            this.method_info = method_info;
            this.method_name = method_name;
        }
        
        public override string ToString()
        {
            return String.Format("<${0}::{1}$>", method_type.FullName, 
                method_info == null ? method_name : method_info.Name);
        }
        
        public Type MethodType {
            get { return method_type; }
        }
        
        public MethodInfo MethodInfo {
            get { return method_info; }
        }
        
        public string MethodName {
            get { return method_name; }
        }
    }
}
