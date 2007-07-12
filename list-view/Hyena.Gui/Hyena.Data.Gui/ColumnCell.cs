using System;
using System.Reflection;
using Gtk;

namespace Banshee.Data.Gui
{
    public abstract class ColumnCell
    {
        private bool expand;
        private int field_index;
        private object bound_object;
            
        public ColumnCell(bool expand, int fieldIndex)
        {
            this.expand = expand;
            this.field_index = fieldIndex;
        }

        public void BindListItem(object item)
        {
            if(item == null) {
                return;
            }
            
            Type type = item.GetType();
            
            bound_object = null;
            
            object [] class_attributes = type.GetCustomAttributes(typeof(ListItemSetup), true);
            if(class_attributes != null && class_attributes.Length > 0) {
                bound_object = item;
                return;
            }
            
            foreach(PropertyInfo info in type.GetProperties()) {
                object [] attributes = info.GetCustomAttributes(typeof(ListItemSetup), false);
                if(attributes == null || attributes.Length == 0) {
                    continue;
                }
            
                if(((ListItemSetup [])attributes)[0].FieldIndex != field_index) {
                    continue;
                }
                
                bound_object = info.GetValue(item, null);
                return;
            }
            
            throw new ApplicationException("Cannot bind IListItem to cell: no ListItemSetup " + 
                "attributes were found on any properties.");
        }
        
        internal Type BoundType {
            get { return bound_object.GetType(); }
        }
        
        internal object BoundObject {
            get { return bound_object; }
        }
        
        public abstract void Render(Gdk.Drawable window, Widget widget, Gdk.Rectangle background_area, 
            Gdk.Rectangle cell_area, Gdk.Rectangle expose_area, StateType state);
        
        public bool Expand {
            get { return expand; }
            set { expand = value; }
        }
        
        public int FieldIndex {
            get { return field_index; }
            set { field_index = value; }
        }
    }
}
