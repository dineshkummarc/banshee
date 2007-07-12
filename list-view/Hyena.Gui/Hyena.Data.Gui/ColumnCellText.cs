using System;
using Gtk;

namespace Banshee.Data.Gui
{
    public class ColumnCellText : ColumnCell
    {
        public delegate string DataHandler();
    
        private Pango.Layout layout;
        private DataHandler data_handler;
        
        public ColumnCellText(bool expand, DataHandler data_handler) : base(expand, -1)
        {
            this.data_handler = data_handler;
        }
    
        public ColumnCellText(bool expand, int fieldIndex) : base(expand, fieldIndex)
        {
        }
    
        public override void Render(Gdk.Drawable window, Widget widget, Gdk.Rectangle background_area, 
            Gdk.Rectangle cell_area, Gdk.Rectangle expose_area, StateType state)
        {
            if(data_handler == null && BoundObject == null) {
                return;
            }
            
            if(layout == null) {
                layout = new Pango.Layout(widget.PangoContext);
            }
        
            string object_str = data_handler == null ? BoundObject.ToString() : data_handler();
            int text_height, text_width;
            
            layout.SetText(object_str);
            layout.GetPixelSize(out text_width, out text_height);
            
            Style.PaintLayout(widget.Style, window, state, true, expose_area, widget, "column",
                cell_area.X + 4, cell_area.Y + ((cell_area.Height - text_height) / 2), layout);
        }
    }
}
