using System;

namespace Banshee.Data.Gui
{
    public class ArtistListView : ListView<ArtistInfo>
    {
        private ColumnController column_controller;
        
        public ArtistListView() : base()
        {
            column_controller = new ColumnController();
            column_controller.Append(new Column("Artist", new ColumnCellText(true, 0), 1.0));
            
            ColumnController = column_controller;
        }
        
        public override IListModel<ArtistInfo> Model {
            get { return base.Model; }
            set { 
                base.Model = value;
                Selection.Select(0);
            }
        }
    }
}
