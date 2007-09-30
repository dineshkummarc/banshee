using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;

namespace MusicBrainzSharp
{
    public enum TrackIncType
    {
        // Object
        ArtistRels = 0,
        LabelRels = 1,
        ReleaseRels = 2,
        TrackRels = 3,
        UrlRels = 4,

        // Item
        Artist = 6,
        TrackLevelRels = 7,
        
        // Tracks
        Puids = 13,
        Releases = 14
    }

    public sealed class TrackInc : Inc
    {
        public TrackInc(TrackIncType type)
            : base((int)type)
        {
            name = EnumUtil.EnumToString(type);
        }

        public static implicit operator TrackInc(TrackIncType type)
        {
            return new TrackInc(type);
        }
    }

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
                AppendStringToBuilder(builder, release);
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
        protected override string url_extension { get { return EXTENSION; } }

        public static TrackInc[] DefaultIncs = new TrackInc[] { };
        protected override Inc[] default_incs
        {
            get { return DefaultIncs; }
        }
        
        Track(string mbid, params Inc[] incs)
            : base(mbid, incs)
        {
            foreach(Inc inc in incs)
                if(inc.Value == (int)TrackIncType.Releases)
                    dont_attempt_releases = true;
                else if(inc.Value == (int)TrackIncType.Puids)
                    dont_attempt_puids = true;
        }

        internal Track(XmlReader reader)
            : base(reader)
        {
        }

        protected override bool ProcessAttributes(XmlReader reader)
        {
            return true;
        }

        protected override bool ProcessXml(XmlReader reader)
        {
            reader.Read();
            bool result = base.ProcessXml(reader);
            if(!result) {
                result = true;
                switch(reader.Name) {
                case "duration":
                    reader.Read();
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
                        do {
                            reader.Read();
                            puids.Add(reader.ReadContentAsString());
                        } while(reader.ReadToNextSibling("puid"));
                    }
                    break;
                default:
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
        bool dont_attempt_releases;
        public List<Release> Releases
        {
            get {
                if(releases == null) {
                    releases = dont_attempt_releases
                        ? new List<Release>()
                        : new Track(MBID, TrackIncType.Releases).Releases;
                }
                return releases;
            }
        }

        List<string> puids;
        bool dont_attempt_puids;
        public List<string> Puids
        {
            get {
                if(puids == null) {
                    puids = dont_attempt_puids
                        ? new List<string>()
                        : new Track(MBID, TrackIncType.Puids).Puids;
                }
                return puids;
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

        #region Get

        public static Track Get(string mbid)
        {
            return Get(mbid, (Inc[])DefaultIncs);
        }

        public static Track Get(string mbid, params TrackInc[] incs)
        {
            return Get(mbid, (Inc[])incs);
        }

        static Track Get(string mbid, params Inc[] incs)
        {
            return new Track(mbid, incs);
        }

        protected override MusicBrainzObject ConstructObject(string mbid, params Inc[] incs)
        {
            return Get(mbid, incs);
        }

        #endregion

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
