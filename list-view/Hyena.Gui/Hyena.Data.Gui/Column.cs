using System;
using System.Collections;
using System.Collections.Generic;
using Gtk;

namespace Banshee.Data.Gui
{
    public class Column : IEnumerable<ColumnCell>
    {
        private string title;
        private double width;
        private bool visible;
        private ColumnCell header_cell;
        private List<ColumnCell> cells = new List<ColumnCell>();
        
        public event EventHandler VisibilityChanged;
        
        public Column(string title, ColumnCell cell, double width) : this(null, title, cell, width)
        {
            this.header_cell = new ColumnHeaderCellText(HeaderCellDataHandler);
        }
        
        public Column(ColumnCell header_cell, string title, ColumnCell cell, double width)
        {
            this.title = title;
            this.width = width;
            this.visible = true;
            this.header_cell = header_cell;
            
            PackStart(cell);
        }
        
        private Column HeaderCellDataHandler()
        {
            return this;
        }
        
        public void PackStart(ColumnCell cell)
        {
            cells.Insert(0, cell);
        }
        
        public void PackEnd(ColumnCell cell)
        {
            cells.Add(cell);
        }
        
        public ColumnCell GetCell(int index) 
        {
            return cells[index];
        }
        
        protected virtual void OnVisibilityChanged()
        {
            EventHandler handler = VisibilityChanged;
            if(handler != null) {
                handler(this, new EventArgs());
            }
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return cells.GetEnumerator();
        }
        
        IEnumerator<ColumnCell> IEnumerable<ColumnCell>.GetEnumerator()
        {
            return cells.GetEnumerator();
        }
        
        public string Title {
            get { return title; }
            set { title = value; }
        }
        
        public double Width {
            get { return width; }
            set { width = value; }
        }
        
        public bool Visible {
            get { return visible; }
            set {
                bool old = Visible;
                visible = value;
                
                if(value != old) {
                    OnVisibilityChanged();
                }
            }
        }
        
        public ColumnCell HeaderCell {
            get { return header_cell; }
            set { header_cell = value; }
        }
    }
}
