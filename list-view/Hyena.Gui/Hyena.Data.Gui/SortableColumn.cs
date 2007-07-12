using System;
using System.Collections;
using System.Collections.Generic;
using Gtk;

namespace Banshee.Data.Gui
{
    public class SortableColumn : Column, ISortableColumn
    {
        private string sort_key;
        private SortType sort_type;
        
        public SortableColumn(string title, ColumnCell cell, double width, string sort_key) : 
            base(title, cell, width)
        {
            this.sort_key = sort_key;
        }
        
        public SortableColumn(ColumnCell header_cell, string title, ColumnCell cell, double width, string sort_key) :
            base(header_cell, title, cell, width)
        {
            this.sort_key = sort_key;
        }
        
        public string SortKey {
            get { return sort_key; }
        }
        
        public SortType SortType {
            get { return sort_type; }
            set { sort_type = value; }
        }
    }
}