using System;
using System.Collections.Generic;

namespace Banshee.Data
{
    public class TrackListModel : IListModel<TrackInfo>
    {
        public event EventHandler Cleared;
        public event EventHandler Reloaded;
        
        protected virtual void OnCleared()
        {
            EventHandler handler = Cleared;
            if(handler != null) {
                handler(this, EventArgs.Empty);
            }
        }
        
        protected virtual void OnReloaded()
        {
            EventHandler handler = Reloaded;
            if(handler != null) {
                handler(this, EventArgs.Empty);
            }
        }
        
        public virtual void Clear()
        {
            throw new NotImplementedException();
        }
        
        public virtual void Reload()
        {
            throw new NotImplementedException();
        }
    
        public virtual TrackInfo GetValue(int index)
        {
            throw new NotImplementedException();
        }
        
        public virtual IEnumerable<ArtistInfo> ArtistInfoFilter {
            set { throw new NotImplementedException(); }
        }
        
        public virtual IEnumerable<AlbumInfo> AlbumInfoFilter {
            set { throw new NotImplementedException(); }
        }

        public virtual int Rows { 
            get { throw new NotImplementedException(); } 
        }
    }
}
