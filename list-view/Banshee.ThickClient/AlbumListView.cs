using System;

namespace Banshee.Data.Gui
{
    public class AlbumListView : ListView<AlbumInfo>
    {
        private ColumnController column_controller;
        
        public AlbumListView() : base()
        {
            column_controller = new ColumnController();
            column_controller.Append(new Column("Album", new ColumnCellAlbum(), 1.0));
            
            ColumnController = column_controller;
            
            RowHeight = ColumnCellAlbum.RowHeight;
        }
        
        public override IListModel<AlbumInfo> Model {
            get { return base.Model; }
            set { 
                base.Model = value;
                Selection.Select(0);
            }
        }
    }
}
