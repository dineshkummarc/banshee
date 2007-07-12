using System;

namespace Banshee.Data
{
    public class TrackInfo
    {
        private Uri uri;

        private string artist_name;
        private string album_title;
        private string track_title;
        private string genre;

        private int track_number;
        private int track_count;
        private int year;
        private int rating;

        private TimeSpan duration;

        public TrackInfo()
        {
        }

        public override string ToString()
        {
            return String.Format("{0} - {1} (on {2}) <{3}> [{4}]", ArtistName, TrackTitle, 
                AlbumTitle, Duration, Uri.AbsoluteUri);
        }

        public Uri Uri {
            get { return uri; }
            set { uri = value; }
        }

        [ListItemSetup(FieldIndex=1)]
        public string ArtistName {
            get { return artist_name; }
            set { artist_name = value; }
        }

        [ListItemSetup(FieldIndex=2)]
        public string AlbumTitle {
            get { return album_title; }
            set { album_title = value; }
        }

        [ListItemSetup(FieldIndex=3)]
        public string TrackTitle {
            get { return track_title; }
            set { track_title = value; }
        }

        public string Genre {
            get { return genre; }
            set { genre = value; }
        }

        [ListItemSetup(FieldIndex=0)]
        public int TrackNumber {
            get { return track_number; }
            set { track_number = value; }
        }

        public int TrackCount {
            get { return track_count; }
            set { track_count = value; }
        }

        public int Year {
            get { return year; }
            set { year = value; }
        }

        public int Rating {
            get { return rating; }
            set { rating = value; }
        }

        [ListItemSetup(FieldIndex=4)]
        public TimeSpan Duration {
            get { return duration; }
            set { duration = value; }
        }
    }
}
