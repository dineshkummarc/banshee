using System;

namespace Narlie.Runtime
{
    public static class LogicFunctionSet
    {
        [Function(1, "not", "!")]
        public static bool Not(object arg)
        {
            if(!(arg is bool)) {
                throw new ArgumentException("can only not a boolean");
            }
            
            return !((bool)arg);
        }
        
        [Function("or", "||")]
        public static bool Or(object [] args)
        {
            return AndOr(args, false);
        }
        
        [Function("and", "&&")]
        public static bool And(object [] args)
        {
            return AndOr(args, true);
        }
        
        private static bool AndOr(object [] args, bool and)
        {
            if(args.Length < 2) {
                throw new ArgumentException("must have two or more boolean arguments");
            }
            
            bool result = false;
            
            for(int i = 0; i < args.Length; i++) {
                object node = args[i];
                if(!(node is bool)) {
                    throw new ArgumentException("arguments must be boolean");
                }
                
                bool arg = (bool)node;
                
                if(i == 0) {
                    result = arg;
                    continue;
                }
                
                if(and) {
                    result &= arg;
                } else {
                    result |= arg;
                }
            }
            
            return result;
        }
    }
}
