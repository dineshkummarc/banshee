using System;
using System.Collections;
using System.Collections.Generic;

namespace Banshee.Data.Gui
{    
    public class ColumnController : IEnumerable<Column>
    {
        private List<Column> columns = new List<Column>();
        
        public void Append(Column column)
        {
            columns.Add(column);
        }
        
        public void Insert(Column column, int index)
        {
            columns.Insert(index, column);
        }
        
        public void Remove(Column column)
        {
            columns.Remove(column);
        }
        
        public void Remove(int index)
        {
            columns.RemoveAt(index);
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return columns.GetEnumerator();
        }
        
        IEnumerator<Column> IEnumerable<Column>.GetEnumerator()
        {
            return columns.GetEnumerator();
        }
        
        public Column this[int index] {
            get { return columns[index] as Column; }
        }
        
        public int Count {
            get { return columns.Count; }
        }
    }
}