using System;
using System.Data;

namespace Banshee.Data
{    
    public class LibraryTrackInfo : TrackInfo
    {
        private int dbid;

        private enum Column : int {
            TrackID,
            ArtistID,
            AlbumID,
            TagSetID,
            MusicBrainzID,
            RelativeUri,
            MimeType,
            Title,
            TrackNumber,
            TrackCount,
            Duration,
            Year,
            Rating,
            PlayCount,
            LastPlayedStamp,
            DateAddedStamp,
            
            Artist,
            AlbumTitle
        }

        public LibraryTrackInfo(IDataReader reader) : base()
        {
            LoadFromReader(reader);
        }

        private void LoadFromReader(IDataReader reader)
        {
            dbid = ReaderGetInt32(reader, Column.TrackID);
            
            Uri = new Uri(ReaderGetString(reader, Column.RelativeUri));
            
            ArtistName = ReaderGetString(reader, Column.Artist);
            AlbumTitle = ReaderGetString(reader, Column.AlbumTitle);
            TrackTitle = ReaderGetString(reader, Column.Title);

            TrackNumber = ReaderGetInt32(reader, Column.TrackNumber);
            TrackCount = ReaderGetInt32(reader, Column.TrackCount);
            Year = ReaderGetInt32(reader, Column.Year);
            Rating = ReaderGetInt32(reader, Column.Rating);

            Duration = ReaderGetTimeSpan(reader, Column.Duration);
        }

        private string ReaderGetString(IDataReader reader, Column column)
        {
            int column_id = (int)column;
            return !reader.IsDBNull(column_id) 
                ? String.Intern(reader.GetString(column_id)) 
                : null;
        }

        private int ReaderGetInt32(IDataReader reader, Column column)
        {
            return reader.GetInt32((int)column);
        }

        private TimeSpan ReaderGetTimeSpan(IDataReader reader, Column column)
        {
            long raw = reader.GetInt64((int)column);
            return new TimeSpan(raw * TimeSpan.TicksPerSecond);
        }

        public int DbId {
            get { return dbid; }
        }
    }
}