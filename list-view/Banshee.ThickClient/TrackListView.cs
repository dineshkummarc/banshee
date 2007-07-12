using System;

namespace Banshee.Data.Gui
{
    public class TrackListView : ListView<TrackInfo>
    {
        private ColumnController column_controller;
        
        public TrackListView() : base()
        {
            column_controller = new ColumnController();
            column_controller.Append(new Column("Track", new ColumnCellText(true, 0), 0.10));
            column_controller.Append(new SortableColumn("Artist", new ColumnCellText(true, 1), 0.25, "artist"));
            column_controller.Append(new SortableColumn("Album", new ColumnCellText(true, 2), 0.25, "album"));
            column_controller.Append(new SortableColumn("Title", new ColumnCellText(true, 3), 0.25, "title"));
            column_controller.Append(new Column("Duration", new ColumnCellText(true, 4), 0.15));
            
            ColumnController = DefaultColumnController;
        }
        
        public ColumnController DefaultColumnController {
            get { return column_controller; }
        }
    }
}
