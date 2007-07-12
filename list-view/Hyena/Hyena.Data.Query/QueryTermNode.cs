using System;

namespace Banshee.Data.Query
{
    public class QueryTermNode : QueryNode
    {
        private string field;
        private string value;
        
        public QueryTermNode(string value) : base()
        {
            int field_separator = value.IndexOf(':');
            if(field_separator > 0) {
                field = value.Substring(0, field_separator);
                this.value = value.Substring(field_separator + 1);
            } else {
                this.value = value;
            }
        }
        
        public override string ToString()
        {
            if(field != null) {
                return String.Format("[{0}]=\"{1}\"", field, value);
            } else {
                return String.Format("\"{0}\"", Value);
            }
        }
        
        public string Value {
            get { return value; }
        }
        
        public string Field {
            get { return field; }
        }
    }
}
