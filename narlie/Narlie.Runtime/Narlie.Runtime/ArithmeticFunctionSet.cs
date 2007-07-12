using System;

namespace Narlie.Runtime
{
    public static class ArithmeticFunctionSet
    {
        private enum ArithmeticOperation
        {
            Add,
            Subtract,
            Multiply,
            Divide,
            Modulo
        }

        private static object PerformArithmetic(object [] args, ArithmeticOperation operation)
        {
            double result = 0.0;
            bool as_int = true;
            
            for(int i = 0; i < args.Length; i++) {
                if(args[i] is int || args[i] is double) {
                    double arg_value;
                    
                    if(args[i] is double) {
                        as_int = false;
                        arg_value = (double)args[i];
                    } else {
                        arg_value = (double)(int)args[i];
                    }
                    
                    if(i == 0) {
                        result = arg_value;
                        continue;
                    }
                    
                    switch(operation) {
                        case ArithmeticOperation.Add:
                            result += arg_value;
                            break;
                        case ArithmeticOperation.Subtract:
                            result -= arg_value;
                            break;
                        case ArithmeticOperation.Multiply:
                            result *= arg_value;
                            break;
                        case ArithmeticOperation.Divide:
                            result /= arg_value;
                            break;
                        case ArithmeticOperation.Modulo:
                            if(!(args[i] is int)) {
                                throw new ArgumentException("Modulo requires int arguments");
                            }
                            
                            result %= (int)arg_value;
                            break;
                    }       
                } else {
                    throw new ArgumentException("arguments must be double or int");
                }
            }
            
            if(as_int) {
                return Convert.ToInt32(result);
            } 
            
            return result;
        }
        
        [Function("add", "+")]
        public static object Add(object [] args)
        {
            return PerformArithmetic(args, ArithmeticOperation.Add);
        }
        
        [Function("sub", "-")]
        public static object Subtract(object [] args)
        {
            return PerformArithmetic(args, ArithmeticOperation.Subtract);
        }
        
        [Function("mul", "*")]
        public static object Multiply(object [] args)
        {
            return PerformArithmetic(args, ArithmeticOperation.Multiply);
        }
        
        [Function("div", "/")]
        public static object Divide(object [] args)
        {
            return PerformArithmetic(args, ArithmeticOperation.Divide);
        }
        
        [Function("mod", "%")]
        public static object Modulo(object [] args)
        {
            return PerformArithmetic(args, ArithmeticOperation.Modulo);
        }
    }
}
