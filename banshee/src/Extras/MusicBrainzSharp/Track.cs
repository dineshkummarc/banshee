using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;

namespace MusicBrainzSharp
{
    public sealed class TrackQueryParameters : ItemQueryParameters
    {
        string release;
        public string Release
        {
            get { return release; }
            set { release = value; }
        }

        string release_id;
        public string ReleaseId
        {
            get { return release_id; }
            set { release_id = value; }
        }

        uint? duration;
        public uint? Duration
        {
            get { return duration; }
            set { duration = value; }
        }

        int? track_number;
        public int? TrackNumber
        {
            get { return track_number; }
            set { track_number = value; }
        }

        string puid;
        public string Puid
        {
            get { return puid; }
            set { puid = value; }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if(release != null) {
                builder.Append("&release=");
                EncodeAndAppend(builder, release);
            }
            if(release_id != null) {
                builder.Append("&releaseid=");
                builder.Append(release_id);
            }
            if(duration.HasValue) {
                builder.Append("&duration=");
                builder.Append(duration.Value);
            }
            if(track_number.HasValue) {
                builder.Append("&tracknumber=");
                builder.Append(track_number.Value);
            }
            if(puid != null) {
                builder.Append("&puid=");
                builder.Append(puid);
            }
            AppendBaseToBuilder(builder);
            return builder.ToString();
        }
    }

    public sealed class Track : MusicBrainzItem
    {
        const string EXTENSION = "track";
        protected override string url_extension
        {
            get { return EXTENSION; }
        }
        
        Track(string mbid)
            : base(mbid)
        {
        }

        internal Track(XmlReader reader)
            : base(reader)
        {
        }

        internal Track(XmlReader reader, bool all_rels_loaded)
            : base(reader, all_rels_loaded)
        {
        }

        protected override void HandleCreateInc(StringBuilder builder)
        {
            builder.Append("+releases+puids");
            base.HandleCreateInc(builder);
        }

        public override void HandleLoadAllData()
        {
            Track track = Track.Get(MBID);
            duration = track.Duration;
            releases = track.Releases;
            puids = track.Puids;
            base.HandleLoadAllData(track);
        }

        protected override bool HandleAttributes(XmlReader reader)
        {
            return true;
        }

        protected override bool HandleXml(XmlReader reader)
        {
            reader.Read();
            bool result = base.HandleXml(reader);
            if(!result) {
                result = true;
                switch(reader.Name) {
                case "duration":
					reader.Read();
					if(reader.NodeType == XmlNodeType.Text)
						duration = uint.Parse(reader.ReadContentAsString());
                    break;
                case "release-list":
                    if(reader.ReadToDescendant("release")) {
                        releases = new List<Release>();
                        do releases.Add(new Release(reader.ReadSubtree()));
                        while(reader.ReadToNextSibling("release"));
                    }
                    break;
                case "puid-list":
                    if(reader.ReadToDescendant("puid")) {
                        puids = new List<string>();
                        do puids.Add(reader["id"]);
                        while(reader.ReadToNextSibling("puid"));
                    }
                    break;
                default:
					reader.Skip(); // FIXME this is a workaround for a Mono bug :(
					result = false;
                    break;
                }
            }
            reader.Close();
            return result;
        }

        #region Properties

        uint duration;
        public uint Duration
        {
            get { return duration; }
        }

        List<Release> releases;
        public List<Release> Releases
        {
            get {
                if(releases == null)
                    LoadAllData();
                return releases ?? new List<Release>();
            }
        }

        List<string> puids;
        public List<string> Puids
        {
            get {
                if(puids == null)
                    LoadAllData();
                return puids ?? new List<string>();
            }
        }

        public int GetTrackNumber(Release release)
        {
            foreach(Release r in Releases)
                if(r.Equals(release))
                    return r.TrackNumber;
            return -1;
        }

        #endregion

        public static Track Get(string mbid)
        {
            return new Track(mbid);
        }

        #region Query

        public static Query<Track> Query(string title)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, ReleaseStatus status)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.ReleaseStatus = status;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, ReleaseType type)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.ReleaseType = type;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, ReleaseStatus status, ReleaseType type)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.ReleaseStatus = status;
            parameters.ReleaseType = type;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, string artist)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.Artist = artist;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, string artist, ReleaseStatus status)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.Artist = artist;
            parameters.ReleaseStatus = status;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, string artist, ReleaseType type)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.Artist = artist;
            parameters.ReleaseType = type;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, string artist, ReleaseStatus status, ReleaseType type)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.Artist = artist;
            parameters.ReleaseStatus = status;
            parameters.ReleaseType = type;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, Artist artist)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.ArtistId = artist.MBID;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, Artist artist, ReleaseStatus status)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.ArtistId = artist.MBID;
            parameters.ReleaseStatus = status;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, Artist artist, ReleaseType type)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.ArtistId = artist.MBID;
            parameters.ReleaseType = type;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, Artist artist, ReleaseStatus status, ReleaseType type)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.ArtistId = artist.MBID;
            parameters.ReleaseStatus = status;
            parameters.ReleaseType = type;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(Artist artist)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.ArtistId = artist.MBID;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(Artist artist, ReleaseStatus status)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.ArtistId = artist.MBID;
            parameters.ReleaseStatus = status;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(Artist artist, ReleaseType type)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.ArtistId = artist.MBID;
            parameters.ReleaseType = type;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(Artist artist, ReleaseStatus status, ReleaseType type)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.ArtistId = artist.MBID;
            parameters.ReleaseStatus = status;
            parameters.ReleaseType = type;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, string artist, string release)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.Artist = artist;
            parameters.Release = release;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, string artist, string release, ReleaseStatus status)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.Artist = artist;
            parameters.Release = release;
            parameters.ReleaseStatus = status;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, string artist, string release, ReleaseType type)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.Artist = artist;
            parameters.Release = release;
            parameters.ReleaseType = type;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, string artist, string release, ReleaseStatus status, ReleaseType type)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.Artist = artist;
            parameters.Release = release;
            parameters.ReleaseStatus = status;
            parameters.ReleaseType = type;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, Artist artist, string release)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.ArtistId = artist.MBID;
            parameters.Release = release;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, Artist artist, string release, ReleaseStatus status)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.ArtistId = artist.MBID;
            parameters.Release = release;
            parameters.ReleaseStatus = status;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, Artist artist, string release, ReleaseType type)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.ArtistId = artist.MBID;
            parameters.Release = release;
            parameters.ReleaseType = type;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, Artist artist, string release, ReleaseStatus status, ReleaseType type)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.ArtistId = artist.MBID;
            parameters.Release = release;
            parameters.ReleaseStatus = status;
            parameters.ReleaseType = type;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, Release release)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.ReleaseId = release.MBID;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, Release release, ReleaseStatus status)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.ReleaseId = release.MBID;
            parameters.ReleaseStatus = status;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, Release release, ReleaseType type)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.ReleaseId = release.MBID;
            parameters.ReleaseType = type;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, Release release, ReleaseStatus status, ReleaseType type)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.ReleaseId = release.MBID;
            parameters.ReleaseStatus = status;
            parameters.ReleaseType = type;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(Release release)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.ReleaseId = release.MBID;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(Release release, ReleaseStatus status)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.ReleaseId = release.MBID;
            parameters.ReleaseStatus = status;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(Release release, ReleaseType type)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.ReleaseId = release.MBID;
            parameters.ReleaseType = type;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(Release release, ReleaseStatus status, ReleaseType type)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.ReleaseId = release.MBID;
            parameters.ReleaseStatus = status;
            parameters.ReleaseType = type;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(Artist artist, Release release)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.ArtistId = artist.MBID;
            parameters.ReleaseId = release.MBID;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(Artist artist, Release release, ReleaseStatus status)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.ArtistId = artist.MBID;
            parameters.ReleaseId = release.MBID;
            parameters.ReleaseStatus = status;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(Artist artist, Release release, ReleaseType type)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.ArtistId = artist.MBID;
            parameters.ReleaseId = release.MBID;
            parameters.ReleaseType = type;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(Artist artist, Release release, ReleaseStatus status, ReleaseType type)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.ArtistId = artist.MBID;
            parameters.ReleaseId = release.MBID;
            parameters.ReleaseStatus = status;
            parameters.ReleaseType = type;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, string artist, Release release)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.Artist = artist;
            parameters.ReleaseId = release.MBID;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, string artist, Release release, ReleaseStatus status)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.Artist = artist;
            parameters.ReleaseId = release.MBID;
            parameters.ReleaseStatus = status;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, string artist, Release release, ReleaseType type)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.Artist = artist;
            parameters.ReleaseId = release.MBID;
            parameters.ReleaseType = type;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, string artist, Release release, ReleaseStatus status, ReleaseType type)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.Artist = artist;
            parameters.ReleaseId = release.MBID;
            parameters.ReleaseStatus = status;
            parameters.ReleaseType = type;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, Artist artist, Release release)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.ArtistId = artist.MBID;
            parameters.ReleaseId = release.MBID;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, Artist artist, Release release, ReleaseStatus status)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.ArtistId = artist.MBID;
            parameters.ReleaseId = release.MBID;
            parameters.ReleaseStatus = status;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, Artist artist, Release release, ReleaseType type)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.ArtistId = artist.MBID;
            parameters.ReleaseId = release.MBID;
            parameters.ReleaseType = type;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(string title, Artist artist, Release release, ReleaseStatus status, ReleaseType type)
        {
            TrackQueryParameters parameters = new TrackQueryParameters();
            parameters.Title = title;
            parameters.ArtistId = artist.MBID;
            parameters.ReleaseId = release.MBID;
            parameters.ReleaseStatus = status;
            parameters.ReleaseType = type;
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(TrackQueryParameters parameters)
        {
            return Query<Track>(EXTENSION, parameters);
        }

        public static Query<Track> Query(TrackQueryParameters parameters, byte limit)
        {
            return Query<Track>(EXTENSION, limit, 0, parameters);
        }

        public static Query<Track> QueryLucene(string lucene_query)
        {
            return Query<Track>(EXTENSION, lucene_query);
        }

        public static Query<Track> QueryLucene(string lucene_query, byte limit)
        {
            return Query<Track>(EXTENSION, limit, 0, lucene_query);
        }

        #endregion
    }
}
