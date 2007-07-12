using System;

using Narlie.Runtime;

namespace Narlie.Compiler.Symbols
{
    public delegate Node FunctionHandler(Node [] args);

    public class FunctionNode : Node
    {
        private FunctionArgs args_restriction;
        private int args_min, args_max;
        private object handler;
        
        public FunctionNode(object handler) : this(FunctionArgs.Variable, 0, 0, handler)
        {
        }
        
        public FunctionNode(FunctionArgs args_restriction, object handler) 
            : this(args_restriction, 0, 0, handler)
        {
        }
        
        public FunctionNode(int args_fixed, object handler) : this(FunctionArgs.Fixed, args_fixed, 
            args_fixed, handler)
        {
        }
                
        public FunctionNode(int args_min, int args_max, object handler) : this(FunctionArgs.Range, 
            args_min, args_max, handler)
        {
        }
        
        public FunctionNode(FunctionArgs args_restriction, int args_min, int args_max, object handler)
        {
            this.args_restriction = args_restriction;
            this.args_min = args_min;
            this.args_max = args_max;
            this.handler = handler;
        }
        
        public int ArgsMin {
            get { return args_min; }
        }
        
        public int ArgsMax {
            get { return args_max; }
        }
        
        public FunctionArgs ArgsRestriction {
            get { return args_restriction; }
        }
        
        public object Handler {
            get { return handler; }
        }
    }
}
