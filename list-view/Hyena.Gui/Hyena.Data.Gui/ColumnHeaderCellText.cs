using System;
using Gtk;

namespace Banshee.Data.Gui
{
    public class ColumnHeaderCellText : ColumnCell
    {
        public delegate Column DataHandler();
        
        private Pango.Layout layout;
        private DataHandler data_handler;
        private bool has_sort;
        
        public ColumnHeaderCellText(DataHandler data_handler) : base(true, -1)
        {
            this.data_handler = data_handler;
        }
    
        public override void Render(Gdk.Drawable window, Widget widget, Gdk.Rectangle background_area, 
            Gdk.Rectangle cell_area, Gdk.Rectangle expose_area, StateType state)
        {
            if(data_handler == null) {
                return;
            }
            
            if(layout == null) {
                layout = new Pango.Layout(widget.PangoContext);
                layout.FontDescription = widget.PangoContext.FontDescription.Copy();
                layout.FontDescription.Weight = Pango.Weight.Bold;
            }
        
            Column column = data_handler();
            int text_height, text_width, arrow_size;
            
            layout.SetText(column.Title);
            layout.GetPixelSize(out text_width, out text_height);
            
            Style.PaintLayout(widget.Style, window, state, true, expose_area, widget, "column",
                cell_area.X + 4, cell_area.Y + ((cell_area.Height - text_height) / 2), layout);
   
            if(has_sort && column is ISortableColumn) {
                arrow_size = (int)((double)cell_area.Height / 2.5);
                Style.PaintArrow(widget.Style, window, state, ShadowType.In, expose_area, widget, "arrow", 
                    ((ISortableColumn)column).SortType == Gtk.SortType.Ascending ? ArrowType.Up : ArrowType.Down, true,
                    cell_area.X + text_width + 8, cell_area.Y + ((cell_area.Height - arrow_size) / 2), arrow_size, arrow_size);
            }
        }
        
        public bool HasSort {
            get { return has_sort; }
            set { has_sort = value; }
        }
    }
}
