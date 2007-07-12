using System;

namespace Banshee.Data
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public class ListItemSetup : Attribute
    {
        private int field_index;
        
        public int FieldIndex {
            get { return field_index; }
            set { field_index = value; }
        }
    }
}
