using System;

namespace Narlie.Runtime
{
    public static class CompareFunctionSet
    {
        [Function(2, "compare-to")]
        public static int Compare(object a, object b)
        {
            if(a.GetType() != b.GetType()) {
                throw new ArgumentException(String.Format(
                    "arguments must be of the same type to compare; got {0} and {1}", 
                    a.GetType(), b.GetType()));
            }
            
            if(a is bool) {
                return ((double)a).CompareTo((double)b);
            } else if(a is int) {
                return ((int)a).CompareTo((int)b);
            } else if(a is double) {
                return ((double)a).CompareTo((double)b);
            } else if(a is string) {
                return ((string)a).CompareTo((string)b);
            }
            
            throw new ArgumentException("invalid type for comparison");
        }
        
        [Function(2, "less-than", "<")]
        public static bool CompareLessThan(object a, object b)
        {
            return Compare(a, b) < 0;
        }
        
        [Function(2, "greater-than", ">")]
        public static bool CompareGreaterThan(object a, object b)
        {
            return Compare(a, b) > 0;
        }
        
        [Function(2, "equal", "=")]
        public static bool CompareEqual(object a, object b)
        {
            return Compare(a, b) == 0;
        }
        
        [Function(2, "not-equal", "!=")]
        public static bool CompareNotEqual(object a, object b)
        {
            return Compare(a, b) != 0;
        }
        
        [Function(2, "less-than-or-equal", "<=")]
        public static bool CompareLessThanOrEqual(object a, object b)
        {
            return Compare(a, b) <= 0;
        }
        
        [Function(2, "greater-than-or-equal", ">=")]
        public static bool CompareGreaterThanOrEqual(object a, object b)
        {
            return Compare(a, b) >= 0;
        }
    }
}
