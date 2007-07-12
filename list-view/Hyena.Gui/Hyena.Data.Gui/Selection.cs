using System;
using System.Collections.Generic;

namespace Banshee.Data
{
    public class Selection : IEnumerable<int>
    {
        private Dictionary<int, bool> selection = new Dictionary<int, bool>();
        private bool all_selected;
        
        public event EventHandler Changed;
        
        private object owner;
        
        public Selection()
        {
        }
        
        protected virtual void OnChanged()
        {
            EventHandler handler = Changed;
            if(handler != null) {
                handler(this, EventArgs.Empty);
            }
        }
        
        public void ToggleSelect(int index)
        {
            lock(this) {
                if(selection.ContainsKey(index)) {
                    selection.Remove(index);
                } else {
                    selection.Add(index, true);
                }
                
                all_selected = false;
                OnChanged();
            }
        }
        
        public void Select(int index)
        {
            Select(index, true);
        }
        
        public void Select(int index, bool raise)
        { 
            lock(this) {
                if(!selection.ContainsKey(index)) { 
                    selection.Add(index, true);
                    all_selected = false;
                    
                    if(raise) {
                        OnChanged();
                    }
                }
            }
        }
        
        public void Unselect(int index)
        {
            lock(this) {
                if(selection.Remove(index)) {
                    all_selected = false;
                    OnChanged();
                }
            }
        }
                    
        public bool Contains(int index)
        {
            lock(this) {
                return selection.ContainsKey(index);
            }
        }
        
        public void SelectRange(int start, int end)
        {
            SelectRange(start, end, false);
        }
        
        public void SelectRange(int start, int end, bool all)
        {
            for(int i = start; i <= end; i++) {
                Select(i, false);
            }
            
            all_selected = all;
            
            OnChanged();
        }

        public void Clear() {
            Clear(true);
        }
        
        public void Clear(bool raise)
        {
            lock(this) {
                if(selection.Count > 0) {
                    selection.Clear();
                    all_selected = false;

                    if(raise) {
                        OnChanged();
                    }
                }
            }
        }
        
        public int Count {
            get { return selection.Count; }
        }
        
        public bool AllSelected {
            get { return all_selected; }
        }
        
        public object Owner {
            get { return owner; }
            set { owner = value; }
        }
        
        public IEnumerator<int> GetEnumerator()
        {
            return selection.Keys.GetEnumerator();
        }
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return selection.Keys.GetEnumerator();
        }
    }
}
