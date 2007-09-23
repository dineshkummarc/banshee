using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;

namespace MusicBrainzSharp
{
    public abstract class ItemQueryParameters : QueryParameters
    {
        string title;
        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        string artist;
        public string Artist
        {
            get { return artist; }
            set { artist = value; }
        }

        string artist_id;
        public string ArtistId
        {
            get { return artist_id; }
            set { artist_id = value; }
        }

        ReleaseType? release_type;
        public ReleaseType? ReleaseType
        {
            get { return release_type; }
            set { release_type = value; }
        }

        ReleaseStatus? release_status;
        public ReleaseStatus? ReleaseStatus
        {
            get { return release_status; }
            set { release_status = value; }
        }

        int? count;
        public int? TrackCount
        {
            get { return count; }
            set { count = value; }
        }

        protected void AppendBaseToBuilder(StringBuilder builder)
        {
            if(title != null) {
                builder.Append("&title=");
                AppendStringToBuilder(builder, title);
            }
            if(artist != null) {
                builder.Append("&artist=");
                AppendStringToBuilder(builder, artist);
            }
            if(artist_id != null) {
                builder.Append("&artistid=");
                builder.Append(artist_id);
            }
            if(release_type.HasValue) {
                builder.Append("&releasetypes=");
                builder.Append(EnumUtil.EnumToString(release_type));
            }
            if(release_status.HasValue) {
                builder.Append(release_type.HasValue ? "+" : "&releasetypes=");
                builder.Append(release_status);
            }
            if(count.HasValue) {
                builder.Append("&count=");
                builder.Append(count.Value);
            }
        }
    }
    
    // The item-like product of an artist, such as a track or a release.
    public abstract class MusicBrainzItem : MusicBrainzObject
    {
        protected MusicBrainzItem(string mbid, params Inc[] incs)
            : base(mbid, incs)
        {
            foreach(Inc inc in incs)
                if(inc.Value == (int)ReleaseIncType.Artist)
                    dont_attempt_artist = true;
        }

        protected MusicBrainzItem(XmlReader reader)
            : base(reader)
        {
        }

        protected override bool ProcessXml(XmlReader reader)
        {
            bool result = true;
            switch(reader.Name) {
            case "title":
                reader.Read();
                title = reader.ReadContentAsString();
                break;
            case "artist":
                artist = new Artist(reader.ReadSubtree());
                break;
            default:
                result = false;
                break;
            }
            return result;
        }

        string title = string.Empty;
        public string Title
        {
            get { return title; }
        }

        Artist artist;
        bool dont_attempt_artist;
        public Artist Artist
        {
            get {
                if(artist == null && !dont_attempt_artist)
                    artist = ((MusicBrainzItem)ConstructObject(
                        MBID, new ReleaseInc(ReleaseIncType.Artist))).Artist;
                return artist;
            }
        }
    }
}
