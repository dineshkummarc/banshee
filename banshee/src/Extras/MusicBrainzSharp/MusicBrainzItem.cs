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
                EncodeAndAppend(builder, title);
            }
            if(artist != null) {
                builder.Append("&artist=");
                EncodeAndAppend(builder, artist);
            }
            if(artist_id != null) {
                builder.Append("&artistid=");
                builder.Append(artist_id);
            }
            if(release_type.HasValue) {
                builder.Append("&releasetypes=");
                builder.Append(Utilities.EnumToString(release_type.Value));
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
        protected MusicBrainzItem(string mbid)
            : base(mbid)
        {
        }

        protected MusicBrainzItem(XmlReader reader)
            : base(reader)
        {
        }

        protected MusicBrainzItem(XmlReader reader, bool all_rels_loaded)
            : base(reader, all_rels_loaded)
        {
        }

        protected override void HandleCreateInc(StringBuilder builder)
        {
            builder.Append("+artist");
        }

        public void HandleLoadAllData(MusicBrainzItem item)
        {
            title = item.Title;
            artist = item.Artist;
            base.HandleLoadAllData(item);
        }

        protected override bool HandleXml(XmlReader reader)
        {
            bool result = true;
            switch(reader.Name) {
            case "title":
				reader.Read();
				if(reader.NodeType == XmlNodeType.Text)
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

        string title;
        public string Title
        {
            get {
                if(title == null)
                    LoadAllData();
                return title;
            }
        }

        Artist artist;
        public Artist Artist
        {
            get {
                if(artist == null)
                    LoadAllData();
                return artist;
            }
        }

        public override string ToString()
        {
            return title;
        }
    }
}
