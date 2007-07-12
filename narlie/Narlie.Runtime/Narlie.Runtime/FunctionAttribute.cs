using System;

namespace Narlie.Runtime
{
    public enum FunctionArgs {
        Empty,
        Fixed,
        Range,
        Variable
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class FunctionAttribute : Attribute
    {
        private string [] names;
        private FunctionArgs args_restriction;
        private int args_min, args_max;
        
        public FunctionAttribute(params string [] names) : this(FunctionArgs.Variable, 0, 0, names)
        {
        }
        
        public FunctionAttribute(FunctionArgs args_restriction,  params string [] names) 
            : this(args_restriction, 0, 0, names)
        {
        }
        
        public FunctionAttribute(int args_fixed,  params string [] names) : this(FunctionArgs.Fixed, args_fixed, 
            args_fixed, names)
        {
        }
                
        public FunctionAttribute(int args_min, int args_max,  params string [] names) : this(FunctionArgs.Range, 
            args_min, args_max, names)
        {
        }
        
        public FunctionAttribute(FunctionArgs args_restriction, int args_min, int args_max,  params string [] names)
        {
            this.args_restriction = args_restriction;
            this.args_min = args_min;
            this.args_max = args_max;
            this.names = names;
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
        
        public string [] Names {
            get { return names; }
        }
    }
}
