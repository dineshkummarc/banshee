using System;
using System.Xml;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;
using System.Reflection;

namespace MusicBrainzSharp
{
    public enum BaseIncType
    {
        ArtistRels = 0,
        LabelRels = 1,
        ReleaseRels = 2,
        TrackRels = 3,
        UrlRels = 4
    }

    internal class BaseInc : Inc
    {
        public BaseInc(BaseIncType type)
            : base((int)type)
        {
            name = EnumUtil.EnumToString(type);
        }

        public static implicit operator BaseInc(BaseIncType type)
        {
            return new BaseInc(type);
        }
    }

    internal delegate void XmlProcessingDelegate(XmlReader reader);
    
    public abstract class MusicBrainzObject
    {
        const string URLBASE = @"http://musicbrainz.org/ws/1/";

        static TimeSpan interval = new TimeSpan(0, 0, 1); // 1 second
        static DateTime last_accessed;
        static readonly object server_mutex = new object();

        protected abstract string url_extension { get; }
        protected abstract bool ProcessAttributes(XmlReader reader);
        protected abstract bool ProcessXml(XmlReader reader);
        protected abstract MusicBrainzObject ConstructObject(string mbid, params Inc[] incs);
        protected abstract Inc[] default_incs { get; }

        protected MusicBrainzObject(string mbid, Inc[] incs)
        {
            foreach(Inc inc in incs)
                switch(inc.Value) {
                case (int)BaseIncType.ArtistRels:
                    dont_attempt_artist_rels = true;
                    break;
                case (int)BaseIncType.ReleaseRels:
                    dont_attempt_release_rels = true;
                    break;
                case (int)BaseIncType.TrackRels:
                    dont_attempt_track_rels = true;
                    break;
                case (int)BaseIncType.LabelRels:
                    dont_attempt_label_rels = true;
                    break;
                case (int)BaseIncType.UrlRels:
                    dont_attempt_url_rels = true;
                    break;
                }
            
            XmlProcessingClosure(
                CreateUrl(url_extension, mbid, CreateParameters(incs)),
                delegate(XmlReader reader) {
                    reader.ReadToFollowing("metadata");
                    reader.Read();
                    CreateFromXml(reader.ReadSubtree());
                    reader.Close();
                }
            );
        }

        protected MusicBrainzObject(XmlReader reader)
        {
            CreateFromXml(reader);
        }

        void CreateFromXml(XmlReader reader)
        {
            reader.Read();
            mbid = reader.GetAttribute("id");
            string score = reader.GetAttribute("ext:score");
            if(score != null)
                this.score = byte.Parse(score);
            ProcessAttributes(reader);
            while(reader.Read() && reader.NodeType != XmlNodeType.EndElement) {
                if(reader.Name == "relation-list") {
                    switch(reader.GetAttribute("target-type")) {
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
                        url_rels = new List<UrlRelation>();
                        RelationDirection direction = RelationDirection.None;
                        string direction_string = reader.GetAttribute("direction");
                        if(direction_string != null)
                            direction = direction_string == "forward"
                                ? RelationDirection.Forward
                                : RelationDirection.Backward;
                        DateTime begin;
                        DateTime.TryParse(reader.GetAttribute("begin"), out begin);
                        DateTime end;
                        DateTime.TryParse(reader.GetAttribute("end"), out end);
                        string attributes_string = reader.GetAttribute("attributes");
                        string[] attributes = attributes_string == null
                            ? null
                            : attributes_string.Split(' ');
                        url_rels.Add(new UrlRelation(
                            reader.GetAttribute("type"),
                            reader.GetAttribute("target"),
                            direction,
                            begin,
                            end,
                            attributes));
                        break;
                    }
                } else
                    ProcessXml(reader.ReadSubtree());
            }
            reader.Close();
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
        bool dont_attempt_artist_rels;
        public List<Relation<Artist>> ArtistRelations
        {
            get
            {
                if(artist_rels == null)
                    artist_rels = dont_attempt_artist_rels
                        ? new List<Relation<Artist>>()
                        : ConstructObject(mbid, BaseIncType.ArtistRels).ArtistRelations;
                return artist_rels;
            }
        }

        List<Relation<Release>> release_rels;
        bool dont_attempt_release_rels;
        public List<Relation<Release>> ReleaseRelations
        {
            get
            {
                if(release_rels == null)
                    release_rels = dont_attempt_release_rels
                        ? new List<Relation<Release>>()
                        : ConstructObject(mbid, BaseIncType.ReleaseRels).ReleaseRelations;

                return release_rels;
            }
        }

        List<Relation<Track>> track_rels;
        bool dont_attempt_track_rels;
        public List<Relation<Track>> TrackRelations
        {
            get
            {
                if(track_rels == null)
                    track_rels = dont_attempt_track_rels
                        ? new List<Relation<Track>>()
                        : ConstructObject(mbid, BaseIncType.TrackRels).TrackRelations;
                return track_rels;
            }
        }

        List<Relation<Label>> label_rels;
        bool dont_attempt_label_rels;
        public List<Relation<Label>> LabelRelations
        {
            get
            {
                if(label_rels == null)
                    label_rels = dont_attempt_label_rels
                        ? new List<Relation<Label>>()
                        : ConstructObject(mbid, BaseIncType.LabelRels).LabelRelations;

                return label_rels;
            }
        }

        List<UrlRelation> url_rels;
        bool dont_attempt_url_rels;
        public List<UrlRelation> UrlRelations
        {
            get
            {
                if(url_rels == null)
                    url_rels = dont_attempt_url_rels
                        ? new List<UrlRelation>()
                        : ConstructObject(mbid, BaseIncType.UrlRels).UrlRelations;

                return url_rels;
            }
        }

        #endregion

        #region Static Methods

        static void CreateRelation<T>(XmlReader reader, List<Relation<T>> relations) where T : MusicBrainzObject
        {
            ConstructorInfo constructor = typeof(T).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { typeof(XmlReader) },
                null);
            while(reader.ReadToFollowing("relation")) {
                string type = reader.GetAttribute("type");
                RelationDirection direction = RelationDirection.None;
                string direction_string = reader.GetAttribute("direction");
                if(direction_string != null)
                    direction = direction_string == "forward"
                        ? RelationDirection.Forward
                        : RelationDirection.Backward;
                DateTime begin;
                DateTime.TryParse(reader.GetAttribute("begin"), out begin);
                DateTime end;
                DateTime.TryParse(reader.GetAttribute("end"), out end);
                string attributes_string = reader.GetAttribute("attributes");
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
        }
        
        static string CreateParameters(string query)
        {
            StringBuilder builder = new StringBuilder(query.Length + 7);
            builder.Append("&query=");
            builder.Append(query);
            return builder.ToString();
        }

        static string CreateParameters(Inc[] incs)
        {
            StringBuilder builder = new StringBuilder();
            bool first_inc = true;
            for(int i = 0; i < incs.Length; i++)
                if(incs[i] != null) {
                    builder.Append(first_inc ? "&inc=" : "+");
                    builder.Append(incs[i].Name);
                    first_inc = false;
                }
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

        static void XmlProcessingClosure(string url, XmlProcessingDelegate code)
        {
            Monitor.Enter(server_mutex);

            // Don't access the MB server twice within a second
            TimeSpan time = DateTime.Now.Subtract(last_accessed);
            if(interval.CompareTo(time) > 0) {
                Thread.Sleep(interval.Subtract(time).Milliseconds);
            }

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            HttpWebResponse response = null;
            try {
                response = request.GetResponse() as HttpWebResponse;
            } catch(WebException e) {
                switch(((HttpWebResponse)e.Response).StatusCode) {
                case HttpStatusCode.BadRequest:
                    throw new MusicBrainzInvalidParameterException();
                case HttpStatusCode.Unauthorized:
                    throw new MusicBrainzUnauthorizedException();
                case HttpStatusCode.NotFound:
                    throw new MusicBrainzNotFoundException();
                }
            }

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
                    string count_string = reader.GetAttribute("count");
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
