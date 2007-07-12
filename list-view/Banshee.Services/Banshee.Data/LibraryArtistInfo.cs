using System;
using System.Data;

namespace Banshee.Data
{    
    public class LibraryArtistInfo : ArtistInfo
    {
        private int dbid;

        private enum Column : int {
            ArtistID,
            Name
        }

        public LibraryArtistInfo(IDataReader reader) : base(null)
        {
            LoadFromReader(reader);
        }

        private void LoadFromReader(IDataReader reader)
        {
            dbid = Convert.ToInt32(reader[(int)Column.ArtistID]);
            Name = (string)reader[(int)Column.Name];
        }

        public int DbId {
            get { return dbid; }
        }
    }
}
