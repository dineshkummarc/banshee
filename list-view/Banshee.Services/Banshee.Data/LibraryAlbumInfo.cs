using System;
using System.Data;

namespace Banshee.Data
{    
    public class LibraryAlbumInfo : AlbumInfo
    {
        private int dbid;

        private enum Column : int {
            AlbumID,
            Title,
            ArtistName
        }

        public LibraryAlbumInfo(IDataReader reader) : base(null)
        {
            LoadFromReader(reader);
        }

        private void LoadFromReader(IDataReader reader)
        {
            dbid = Convert.ToInt32(reader[(int)Column.AlbumID]);
            Title = (string)reader[(int)Column.Title];
            ArtistName = (string)reader[(int)Column.ArtistName];
        }

        public int DbId {
            get { return dbid; }
        }
    }
}
