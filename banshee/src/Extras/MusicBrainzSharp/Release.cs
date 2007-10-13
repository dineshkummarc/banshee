using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;

namespace MusicBrainzSharp
{
    # region Enums

    public enum ReleaseType
    {
        Album,
        Audiobook,
        Compilation,
        EP,
        Interview,
        Live,
        None,
        Other, 
        Remix,
        Single,
        Soundtrack,
        Spokenword,
    }

    public enum ReleaseStatus
    {
        Bootleg,
        None,
        Official,
        Promotion,
        PsudoRelease
    }

    public enum ReleaseFormat
    {
        Cartridge,
        Casette,
        CD,
        DAT,
        Digital,
        DualDisc,
        DVD,
        LaserDisc,
        MiniDisc,
        None,
        Other,
        ReelToReel,
        SACD,
        Vinyl
    }

    #endregion

    public sealed class ReleaseQueryParameters : ItemQueryParameters
    {
        string disc_id;
        public string DiscId
        {
            get { return disc_id; }
            set { disc_id = value; }
        }

        string date;
        public string Date
        {
            get { return date; }
            set { date = value; }
        }

        string asin;
        public string Asin
        {
            get { return asin; }
            set { asin = value; }
        }

        string language;
        public string Language
        {
            get { return language; }
            set { language = value; }
        }

        string script;
        public string Script
        {
            get { return script; }
            set { script = value; }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if(disc_id != null) {
                builder.Append("&discid=");
                builder.Append(disc_id);
            }
            if(date != null) {
                builder.Append("&date=");
                EncodeAndAppend(builder, date);
            }
            if(asin != null) {
                builder.Append("&asin=");
                builder.Append(asin);
            }
            if(language != null) {
                builder.Append("&language=");
                builder.Append(language);
            }
            if(script != null) {
                builder.Append("&script=");
                builder.Append(script);
            }
            AppendBaseToBuilder(builder);
            return builder.ToString();
        }
    }

    public sealed class Release : MusicBrainzItem
    {
        const string EXTENSION = "release";
        protected override string url_extension
        {
            get { return EXTENSION; }
        }

        Release(string mbid)
            : base(mbid)
        {
        }

        internal Release(XmlReader reader)
            : base(reader)
        {
        }

        protected override void HandleCreateInc(StringBuilder builder)
        {
            builder.Append("+release-events+labels+discs+tracks+track-level-rels");
            base.HandleCreateInc(builder);
        }

        public override void HandleLoadAllData()
        {
            Release release = Release.Get(MBID);
            type = release.Type;
            status = release.Status;
            language = release.Language;
            script = release.Script;
            asin = release.Asin;
            discs = release.Discs;
            events = release.Events;
            tracks = release.Tracks;
        }

        protected override bool HandleAttributes(XmlReader reader)
        {
            // How sure am I about getting the type and status in the "Type Status" format?
            // MB really ought to specify these two things seperatly.
            string type_string = reader["type"];
            if(type_string != null) {
                foreach(string token in type_string.Split(' ')) {
                    if(!this.type.HasValue) {
                        bool found = false;
                        foreach(ReleaseType type in Enum.GetValues(typeof(ReleaseType)) as ReleaseType[])
                            if(type.ToString() == token) {
                                this.type = type;
                                found = true;
                                break;
                            }
                        if(found)
                            continue;
                    }

                    foreach(ReleaseStatus status in Enum.GetValues(typeof(ReleaseStatus)) as ReleaseStatus[])
                        if(status.ToString() == token) {
                            this.status = status;
                            break;
                        }
                }
            }
            return this.type.HasValue || this.status.HasValue;
        }

        protected override bool HandleXml(XmlReader reader)
        {
            reader.Read();
            bool result = base.HandleXml(reader);
            if(!result) {
                result = true;
                switch(reader.Name) {
                case "text-representation":
                    language = reader["language"];
                    script = reader["script"];
                    break;
                case "asin":
                    reader.Read();
                    asin = reader.ReadContentAsString();
                    break;
                case "disc-list": {
                        if(reader.ReadToDescendant("disc")) {
                            discs = new List<Disc>();
                            do discs.Add(new Disc(reader.ReadSubtree()));
                            while(reader.ReadToNextSibling("disc"));
                        }
                        break;
                    }
                case "release-event-list":
                    if(reader.ReadToDescendant("event")) {
                        events = new List<Event>();
                        do events.Add(new Event(reader.ReadSubtree(), this));
                        while(reader.ReadToNextSibling("event"));
                    }
                    break;
                case "track-list": {
                        string offset = reader["offset"];
                        if(offset != null)
                            track_number = int.Parse(offset) + 1;
                        if(reader.ReadToDescendant("track")) {
                            LoadAllData(); // just to be sure
                            tracks = new List<Track>();
                            do tracks.Add(new Track(reader.ReadSubtree(), true));
                            while(reader.ReadToNextSibling("track"));
                        }
                        break;
                    }
                default:
                    result = false;
                    break;
                }
            }
            reader.Close();
            return result;
        }

        #region Properties

        ReleaseType? type;
        public ReleaseType Type
        {
            get {
                if(!type.HasValue)
                    LoadAllData();
                return type.HasValue ? type.Value : ReleaseType.None;
            }
        }

        ReleaseStatus? status;
        public ReleaseStatus Status
        {
            get {
                if(!status.HasValue)
                    LoadAllData();
                return status.HasValue ? status.Value : ReleaseStatus.None;
            }
        }

        string language;
        public string Language
        {
            get {
                if(language == null)
                    LoadAllData();
                return language;
            }
        }

        string script;
        public string Script
        {
            get {
                if(script == null)
                    LoadAllData();
                return script;
            }
        }

        string asin;
        public string Asin
        {
            get {
                if(asin == null)
                    LoadAllData();
                return asin;
            }
        }

        List<Disc> discs;
        public List<Disc> Discs
        {
            get {
                if(discs == null)
                    LoadAllData();
                return discs ?? new List<Disc>();
            }
        }

        List<Event> events;
        public List<Event> Events
        {
            get {
                if(events == null)
                    LoadAllData();
                return events ?? new List<Event>();
            }
        }

        List<Track> tracks;
        public List<Track> Tracks
        {
            get {
                if(tracks == null)
                    LoadAllData();
                return tracks ?? new List<Track>(); 
            }
        }

        int? track_number;
        internal int TrackNumber
        {
            get { return track_number.HasValue ? track_number.Value : -1; }
        }

        #endregion

        public static Release Get(string mbid)
        {
            return new Release(mbid);
        }

        #region Query

        public static Query<Release> Query(string title)
        {
            ReleaseQueryParameters parameters = new ReleaseQueryParameters();
            parameters.Title = title;
            return Query<Release>(EXTENSION, parameters);
        }

        public static Query<Release> Query(string title, ReleaseStatus status)
        {
            ReleaseQueryParameters parameters = new ReleaseQueryParameters();
            parameters.Title = title;
            parameters.ReleaseStatus = status;
            return Query<Release>(EXTENSION, parameters);
        }

        public static Query<Release> Query(string title, ReleaseType type)
        {
            ReleaseQueryParameters parameters = new ReleaseQueryParameters();
            parameters.Title = title;
            parameters.ReleaseType = type;
            return Query<Release>(EXTENSION, parameters);
        }

        public static Query<Release> Query(string title, ReleaseStatus status, ReleaseType type)
        {
            ReleaseQueryParameters parameters = new ReleaseQueryParameters();
            parameters.Title = title;
            parameters.ReleaseStatus = status;
            parameters.ReleaseType = type;
            return Query<Release>(EXTENSION, parameters);
        }

        public static Query<Release> Query(string title, string artist)
        {
            ReleaseQueryParameters parameters = new ReleaseQueryParameters();
            parameters.Title = title;
            parameters.Artist = artist;
            return Query<Release>(EXTENSION, parameters);
        }

        public static Query<Release> Query(string title, string artist, ReleaseStatus status)
        {
            ReleaseQueryParameters parameters = new ReleaseQueryParameters();
            parameters.Title = title;
            parameters.Artist = artist;
            parameters.ReleaseStatus = status;
            return Query<Release>(EXTENSION, parameters);
        }

        public static Query<Release> Query(string title, string artist, ReleaseType type)
        {
            ReleaseQueryParameters parameters = new ReleaseQueryParameters();
            parameters.Title = title;
            parameters.Artist = artist;
            parameters.ReleaseType = type;
            return Query<Release>(EXTENSION, parameters);
        }

        public static Query<Release> Query(string title, string artist, ReleaseStatus status, ReleaseType type)
        {
            ReleaseQueryParameters parameters = new ReleaseQueryParameters();
            parameters.Title = title;
            parameters.Artist = artist;
            parameters.ReleaseStatus = status;
            parameters.ReleaseType = type;
            return Query<Release>(EXTENSION, parameters);
        }

        public static Query<Release> Query(string title, Artist artist)
        {
            ReleaseQueryParameters parameters = new ReleaseQueryParameters();
            parameters.Title = title;
            parameters.ArtistId = artist.MBID;
            return Query<Release>(EXTENSION, parameters);
        }

        public static Query<Release> Query(string title, Artist artist, ReleaseStatus status)
        {
            ReleaseQueryParameters parameters = new ReleaseQueryParameters();
            parameters.Title = title;
            parameters.ArtistId = artist.MBID;
            parameters.ReleaseStatus = status;
            return Query<Release>(EXTENSION, parameters);
        }

        public static Query<Release> Query(string title, Artist artist, ReleaseType type)
        {
            ReleaseQueryParameters parameters = new ReleaseQueryParameters();
            parameters.Title = title;
            parameters.ArtistId = artist.MBID;
            parameters.ReleaseType = type;
            return Query<Release>(EXTENSION, parameters);
        }

        public static Query<Release> Query(string title, Artist artist, ReleaseStatus status, ReleaseType type)
        {
            ReleaseQueryParameters parameters = new ReleaseQueryParameters();
            parameters.Title = title;
            parameters.ArtistId = artist.MBID;
            parameters.ReleaseStatus = status;
            parameters.ReleaseType = type;
            return Query<Release>(EXTENSION, parameters);
        }

        public static Query<Release> Query(Artist artist, ReleaseStatus status)
        {
            ReleaseQueryParameters parameters = new ReleaseQueryParameters();
            parameters.ArtistId = artist.MBID;
            parameters.ReleaseStatus = status;
            return Query<Release>(EXTENSION, parameters);
        }

        public static Query<Release> Query(Artist artist, ReleaseType type)
        {
            ReleaseQueryParameters parameters = new ReleaseQueryParameters();
            parameters.ArtistId = artist.MBID;
            parameters.ReleaseType = type;
            return Query<Release>(EXTENSION, parameters);
        }

        public static Query<Release> Query(Artist artist, ReleaseStatus status, ReleaseType type)
        {
            ReleaseQueryParameters parameters = new ReleaseQueryParameters();
            parameters.ArtistId = artist.MBID;
            parameters.ReleaseStatus = status;
            parameters.ReleaseType = type;
            return Query<Release>(EXTENSION, parameters);
        }

        public static Query<Release> Query(Disc disc)
        {
            ReleaseQueryParameters parameters = new ReleaseQueryParameters();
            parameters.DiscId = disc.Id;
            return Query<Release>(EXTENSION, parameters);
        }

        public static Query<Release> Query(ReleaseQueryParameters parameters)
        {
            return Query<Release>(EXTENSION, parameters);
        }

        public static Query<Release> Query(ReleaseQueryParameters parameters, byte limit)
        {
            return Query<Release>(EXTENSION, limit, 0, parameters);
        }

        public static Query<Release> QueryLucene(string lucene_query)
        {
            return Query<Release>(EXTENSION, lucene_query);
        }

        public static Query<Release> QueryLucene(string lucene_query, byte limit)
        {
            return Query<Release>(EXTENSION, limit, 0, lucene_query);
        }

        public static Query<Release> QueryFromDisc(string device)
        {
            ReleaseQueryParameters parameters = new ReleaseQueryParameters();
            parameters.DiscId = Disc.GetFromDevice(device).Id;
            return Query<Release>(EXTENSION, parameters);
        }

        #endregion
    }
}
