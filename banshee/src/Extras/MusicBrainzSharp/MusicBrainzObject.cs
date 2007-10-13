using System;
using System.Xml;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Net.Cache;

namespace MusicBrainzSharp
{
    public delegate void XmlRequestHandler(string url, bool from_cache);
    internal delegate void XmlProcessingDelegate(XmlReader reader);
    
    public abstract class MusicBrainzObject
    {
        const string URLBASE = @"http://musicbrainz.org/ws/1/";

        static TimeSpan min_interval = new TimeSpan(0, 0, 1); // 1 second
        static DateTime last_accessed;
        static readonly object server_mutex = new object();

        bool all_data_loaded;
        bool all_rels_loaded;
        protected abstract string url_extension { get; }
        
        protected MusicBrainzObject(string mbid)
        {
            all_data_loaded = true;
            CreateFromMbid(mbid, CreateInc());
        }

        protected MusicBrainzObject(string mbid, string parameters)
        {
            CreateFromMbid(mbid, parameters);
        }

        protected MusicBrainzObject(XmlReader reader)
            : this(reader, false)
        {
        }

        protected MusicBrainzObject(XmlReader reader, bool all_rels_loaded)
        {
            this.all_rels_loaded = all_rels_loaded;
            CreateFromXml(reader);
        }

        protected abstract void HandleCreateInc(StringBuilder builder);
        string CreateInc()
        {
            StringBuilder builder = new StringBuilder(70);
            builder.Append("&inc=artist-rels+release-rels+track-rels+label-rels+url-rels");
            HandleCreateInc(builder);
            return builder.ToString();
        }

        void CreateFromMbid(string mbid, string parameters)
        {
            XmlProcessingClosure(
                CreateUrl(url_extension, mbid, parameters),
                delegate(XmlReader reader) {
                    reader.ReadToFollowing("metadata");
                    reader.Read();
                    CreateFromXml(reader.ReadSubtree());
                    reader.Close();
                }
            );
        }
        
        protected abstract bool HandleAttributes(XmlReader reader);
        protected abstract bool HandleXml(XmlReader reader);
        void CreateFromXml(XmlReader reader)
        {
            reader.Read();
            mbid = reader["id"];
            string score = reader["ext:score"];
            if(score != null)
                this.score = byte.Parse(score);
            HandleAttributes(reader);
            while(reader.Read() && reader.NodeType != XmlNodeType.EndElement)
                if(reader.Name == "relation-list")
                    switch(reader["target-type"]) {
                    case "Artist":
                        artist_rels = new List<Relation<Artist>>();
                        CreateRelation<Artist>(reader.ReadSubtree(), artist_rels);
                        break;
                    case "Release":
                        release_rels = new List<Relation<Release>>();
                        CreateRelation<Release>(reader.ReadSubtree(), release_rels);
                        break;
                    case "Track":
                        track_rels = new List<Relation<Track>>();
                        CreateRelation<Track>(reader.ReadSubtree(), track_rels);
                        break;
                    case "Label":
                        label_rels = new List<Relation<Label>>();
                        CreateRelation<Label>(reader.ReadSubtree(), label_rels);
                        break;
                    case "Url":
                        if(!reader.ReadToDescendant("relation"))
                            break;
                        url_rels = new List<UrlRelation>();
                        do {
                            RelationDirection direction = RelationDirection.Forward;
                            string direction_string = reader["direction"];
                            if(direction_string != null && direction_string == "backward")
                                direction = RelationDirection.Backward;
                            string attributes_string = reader["attributes"];
                            string[] attributes = attributes_string == null
                                ? null
                                : attributes_string.Split(' ');
                            url_rels.Add(new UrlRelation(
                                reader["type"],
                                reader["target"],
                                direction,
                                reader["begin"],
                                reader["end"],
                                attributes));
                        } while(reader.ReadToNextSibling("relation"));
                        break;
                    }
                else
                    HandleXml(reader.ReadSubtree());
            reader.Close();
        }

        protected void LoadAllData()
        {
            if(!all_data_loaded) {
                HandleLoadAllData();
                all_data_loaded = true;
            }
        }

        public abstract void HandleLoadAllData();
        protected void HandleLoadAllData(MusicBrainzObject obj)
        {
            artist_rels = obj.ArtistRelations;
            release_rels = obj.ReleaseRelations;
            track_rels = obj.TrackRelations;
            label_rels = obj.LabelRelations;
            url_rels = obj.UrlRelations;
        }

        #region Properties

        string mbid;
        public string MBID
        {
            get { return mbid; }
        }

        byte score;
        public byte Score
        {
            get { return score; }
        }

        List<Relation<Artist>> artist_rels;
        public List<Relation<Artist>> ArtistRelations
        {
            get {
                if(artist_rels == null && !all_rels_loaded)
                    LoadAllData();
                return artist_rels ?? new List<Relation<Artist>>();
            }
        }

        List<Relation<Release>> release_rels;
        public List<Relation<Release>> ReleaseRelations
        {
            get {
                if(release_rels == null && !all_rels_loaded)
                    LoadAllData();
                return release_rels ?? new List<Relation<Release>>();
            }
        }

        List<Relation<Track>> track_rels;
        public List<Relation<Track>> TrackRelations
        {
            get {
                if(track_rels == null && !all_rels_loaded)
                    LoadAllData();
                return track_rels ?? new List<Relation<Track>>();
            }
        }

        List<Relation<Label>> label_rels;
        public List<Relation<Label>> LabelRelations
        {
            get {
                if(label_rels == null && !all_rels_loaded)
                    LoadAllData();
                return label_rels ?? new List<Relation<Label>>();
            }
        }

        List<UrlRelation> url_rels;
        public List<UrlRelation> UrlRelations
        {
            get {
                if(url_rels == null && !all_rels_loaded)
                    LoadAllData();
                return url_rels ?? new List<UrlRelation>();
            }
        }

        public override bool Equals(object obj)
        {
            MusicBrainzObject mbobj = obj as MusicBrainzObject;
            return mbobj != null && mbobj.GetType() == GetType() && mbobj.MBID == MBID;
        }

        public override int GetHashCode()
        {
            return (GetType().Name + MBID).GetHashCode();
        }

        #endregion

        #region Static Methods

        static bool CreateRelation<T>(XmlReader reader, List<Relation<T>> relations) where T : MusicBrainzObject
        {
            ConstructorInfo constructor = typeof(T).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { typeof(XmlReader) },
                null);

            bool found = false;
            while(reader.ReadToFollowing("relation")) {
                found = true;
                string type = reader["type"];
                RelationDirection direction = RelationDirection.Forward;
                string direction_string = reader["direction"];
                if(direction_string != null && direction_string == "backward")
                    direction = RelationDirection.Backward;
                string begin = reader["begin"];
                string end = reader["end"];
                string attributes_string = reader["attributes"];
                string[] attributes = attributes_string == null
                    ? null
                    : attributes_string.Split(' ');

                reader.Read();
                relations.Add(new Relation<T>(
                    type,
                    (T)constructor.Invoke(new object[] { reader.ReadSubtree() }),
                    direction,
                    begin,
                    end,
                    attributes));
            }
            reader.Close();
            return found;
        }
        
        static string CreateParameters(string query)
        {
            StringBuilder builder = new StringBuilder(query.Length + 7);
            builder.Append("&query=");
            builder.Append(query);
            return builder.ToString();
        }
        
        static string CreateUrl(string url_extension, int limit, int offset, string parameters)
        {
            StringBuilder builder = new StringBuilder();
            if(limit != 25) {
                builder.Append("&limit=");
                builder.Append(limit);
            }
            if(offset != 0) {
                builder.Append("&offset=");
                builder.Append(offset);
            }
            builder.Append(parameters);
            return CreateUrl(url_extension, string.Empty, builder.ToString());
        }

        static string CreateUrl(string url_extension, string mbid, string parameters)
        {
            StringBuilder builder = new StringBuilder(URLBASE.Length + mbid.Length + parameters.Length + 9);
            builder.Append(URLBASE);
            builder.Append(url_extension);
            builder.Append('/');
            builder.Append(mbid);
            builder.Append("?type=xml");
            builder.Append(parameters);
            return builder.ToString();
        }

        public static RequestCachePolicy CachePolicy;
        public static event XmlRequestHandler XmlRequest;
        static void XmlProcessingClosure(string url, XmlProcessingDelegate code)
        {
            Monitor.Enter(server_mutex);

            // Don't access the MB server twice within a second
            TimeSpan time = DateTime.Now.Subtract(last_accessed);
            if(min_interval.CompareTo(time) > 0)
                Thread.Sleep(min_interval.Subtract(time).Milliseconds);

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            if(CachePolicy != null)
                request.CachePolicy = CachePolicy;
            HttpWebResponse response = null;
            try {
                response = request.GetResponse() as HttpWebResponse;
            } catch(WebException e) {
                response = (HttpWebResponse)e.Response;
            }

            switch(response.StatusCode) {
            case HttpStatusCode.BadRequest:
                Monitor.Exit(server_mutex);
                throw new MusicBrainzInvalidParameterException();
            case HttpStatusCode.Unauthorized:
                Monitor.Exit(server_mutex);
                throw new MusicBrainzUnauthorizedException();
            case HttpStatusCode.NotFound:
                Monitor.Exit(server_mutex);
                throw new MusicBrainzNotFoundException();
            }

            if(XmlRequest != null)
                XmlRequest(url, response.IsFromCache);

            if(response.IsFromCache)
                Monitor.Exit(server_mutex);

            // Should we read the stream into a memory stream and run the XmlReader off of that?
            code(new XmlTextReader(response.GetResponseStream()));
            response.Close();

            if(!response.IsFromCache) {
                last_accessed = DateTime.Now;
                Monitor.Exit(server_mutex);
            }
        }

        #endregion

        #region Query

        protected static Query<T> Query<T>(string url_extension, QueryParameters parameters) where T : MusicBrainzObject
        {
            return Query<T>(url_extension, MusicBrainzSharp.Query.DefaultLimit, 0, parameters);
        }

        protected static Query<T> Query<T>(string url_extension, byte limit, int offset, QueryParameters parameters) where T : MusicBrainzObject
        {
            return new Query<T>(url_extension, limit, offset, parameters.ToString());
        }

        protected static Query<T> Query<T>(string url_extension, string query) where T : MusicBrainzObject
        {
            return Query<T>(url_extension, MusicBrainzSharp.Query.DefaultLimit, 0, query);
        }

        protected static Query<T> Query<T>(string url_extension, byte limit, int offset, string query) where T : MusicBrainzObject
        {
            return new Query<T>(url_extension, limit, offset, CreateParameters(query));
        }

        internal static List<T> DoQuery<T>(string url_extension, byte limit, int offset, string parameters, Inc[] artist_release_incs, out int? count) where T : MusicBrainzObject
        {
            int count_value = 0;
            List<T> results = new List<T>();
            XmlProcessingClosure(
                CreateUrl(url_extension, limit, offset, parameters),
                delegate(XmlReader reader) {
                    reader.ReadToFollowing("metadata");
                    if(!reader.Read())
                        return;
                    string count_string = reader["count"];
                    if(count_string != null)
                        count_value = int.Parse(count_string);
                    reader.Read();
                    do results.Add(GetResult<T>(reader.ReadSubtree(), artist_release_incs));
                    while(reader.Read() && reader.NodeType == XmlNodeType.Element);
                    reader.Close();
                }
            );
            count = count_value == 0 ? results.Count : count_value;
            return results;
        }

        static T GetResult<T>(XmlReader reader, Inc[] artist_release_incs) where T : MusicBrainzObject
        {
            if(typeof(T) == typeof(Artist))
                return artist_release_incs == null
                    ? new Artist(reader) as T
                    : new Artist(reader, artist_release_incs) as T;
            else if(typeof(T) == typeof(Release))
                return new Release(reader) as T;
            else if(typeof(T) == typeof(Track))
                return new Track(reader) as T;
            else if(typeof(T) == typeof(Label))
                return new Label(reader) as T;
            else
                return null;
        }

        #endregion
    }
}
