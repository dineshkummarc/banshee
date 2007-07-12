using System;

namespace Narlie.Runtime
{
    public static class UtilityFunctionSet
    {
        [Function("print")]
        public static void Print(object [] args)
        {
            if(args.Length > 1) {
                if(args[0] is string) {
                    object [] slice_args = new object[args.Length - 1];
                    Array.Copy(args, 1, slice_args, 0, args.Length - 1);
                    Console.WriteLine((string)args[0], slice_args);
                } else {
                    throw new ArgumentException("First argument to print must be a string");
                }
            } else {
                Console.WriteLine(args[0]);
            }
        }
        
        [Function(1, "print-type")]
        public static void PrintType(object arg)
        {
            Console.WriteLine(arg.GetType());
        }
    }
}
