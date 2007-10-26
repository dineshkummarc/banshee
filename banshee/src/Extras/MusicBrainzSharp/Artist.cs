using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;

namespace MusicBrainzSharp
{
    public enum ArtistType
    {
        Group,
        Person,
        Unspecified
    }

    // FIXME this stuff needs to be cleaned up!
    public enum ArtistReleasesIncType
    {
        [StringValue("sa-")] SingleArtist,
        [StringValue("va-")] VariousArtists
    }

    public sealed class ArtistInc : Inc
    {
        public ArtistInc(ArtistReleasesIncType type, ReleaseType release_type)
            : base(-1)
        {
            if(release_type == ReleaseType.None)
                throw new ArgumentException("You cannot use ReleaseType.None in an inc parameter");
            name = Utilities.EnumToString(type) + Utilities.EnumToString(release_type);
        }

        public ArtistInc(ArtistReleasesIncType type, ReleaseStatus release_status)
            : base(-1)
        {
            if(release_status == ReleaseStatus.None)
                throw new ArgumentException("You cannot use ReleaseStatus.None in an inc parameter");
            name = Utilities.EnumToString(type) + Utilities.EnumToString(release_status);
        }
    }
    
    public sealed class Artist : MusicBrainzEntity
    {
        const string EXTENSION = "artist";
        protected override string url_extension
        {
            get { return EXTENSION; }
        }

        public static ArtistInc[] DefaultReleaseIncs = new ArtistInc[] {
            new ArtistInc(ArtistReleasesIncType.SingleArtist, ReleaseStatus.Official)
        };

        Inc[] release_incs;
        public Inc[] ReleaseIncs
        {
            get { return release_incs; }
            set {
                releases = null;
                release_incs = value;
                dont_attempt_releases = false;
            }
        }

        Artist(string mbid)
            : base(mbid)
        {
        }

        Artist(string mbid, string parameters)
            : base(mbid, parameters)
        {
        }

        Artist(string mbid, string parameters, Inc[] release_incs)
            : base(mbid, parameters)
        {
            dont_attempt_releases = true;
            this.release_incs = release_incs;
        }

        internal Artist(XmlReader reader)
            : this(reader, DefaultReleaseIncs)
        {
        }

        internal Artist(XmlReader reader, Inc[] release_incs)
            : base(reader)
        {
            this.release_incs = release_incs;
        }

        public override void HandleLoadMissingData()
        {
            Artist artist = new Artist(MBID, CreateInc());
            type = artist.Type;
            base.HandleLoadAllData(artist);
        }

        protected override bool HandleAttributes(XmlReader reader)
        {
            switch(reader["type"]) {
            case "Group":
                type = ArtistType.Group;
                break;
            case "Person":
                type = ArtistType.Person;
                break;
            }
            return type != ArtistType.Unspecified;
        }

        protected override bool HandleXml(XmlReader reader)
        {
            reader.Read();
            bool result = base.HandleXml(reader);
            if(!result) {
                result = true;
                switch(reader.Name) {
                case "release-list":
                    if(reader.ReadToDescendant("release")) {
                        dont_attempt_releases = true;
                        releases = new List<Release>();
                        do releases.Add(new Release(reader.ReadSubtree()));
                        while(reader.ReadToNextSibling("release"));
                    }
                    break;
                default:
					reader.Skip(); // FIXME this is a workaround for Mono bug 334752                  
					result = false;
                    break;
                }
            }
            reader.Close();
            return result;
        }

        #region Properties

        ArtistType? type;
        public ArtistType Type
        {
            get {
                if(!type.HasValue)
                    LoadMissingData();
                return type.HasValue ? type.Value : ArtistType.Unspecified;
            }
        }

        List<Release> releases;
        bool dont_attempt_releases;
        public List<Release> Releases
        {
            get {
                if(releases == null)
                    if(dont_attempt_releases)
                        releases = new List<Release>();
                    else
                        releases = new Artist(MBID, MakeInc(release_incs), release_incs).Releases;
                return releases;
            }
        }

        #endregion

        static string MakeInc(Inc[] incs)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("&inc=");
            foreach(ArtistInc inc in incs)
                builder.Append(inc.Name);
            return builder.ToString();
        }

        public static Artist Get(string mbid)
        {
            return new Artist(mbid);
        }

        public static Artist Get(string mbid, params ArtistInc[] release_incs)
        {
            return new Artist(mbid, MakeInc(release_incs), release_incs);
        }

        #region Query

        public static Query<Artist> Query(string name)
        {
            EntityQueryParameters parameters = new EntityQueryParameters();
            parameters.Name = name;
            return Query<Artist>(EXTENSION, parameters);
        }

        public static Query<Artist> Query(string name, byte limit)
        {
            EntityQueryParameters parameters = new EntityQueryParameters();
            parameters.Name = name;
            return Query<Artist>(EXTENSION, limit, 0, parameters);
        }

        public static Query<Artist> Query(string name, params ArtistInc[] release_incs)
        {
            EntityQueryParameters parameters = new EntityQueryParameters();
            parameters.Name = name;
            Query<Artist> result = Query<Artist>(EXTENSION, parameters);
            result.ArtistReleaseIncs = release_incs;
            return result;
        }

        public static Query<Artist> Query(string name, byte limit, params ArtistInc[] release_incs)
        {
            EntityQueryParameters parameters = new EntityQueryParameters();
            parameters.Name = name;
            Query<Artist> result = Query<Artist>(EXTENSION, limit, 0, parameters);
            result.ArtistReleaseIncs = release_incs;
            return result;
        }

        public static Query<Artist> QueryLucene(string lucene_query)
        {
            return Query<Artist>(EXTENSION, lucene_query);
        }

        public static Query<Artist> QueryLucene(string lucene_query, params ArtistInc[] release_incs)
        {
            Query<Artist> result = Query<Artist>(EXTENSION, lucene_query);
            result.ArtistReleaseIncs = release_incs;
            return result;
        }

        public static Query<Artist> QueryLucene(string lucene_query, byte limit, params ArtistInc[] release_incs)
        {
            Query<Artist> result = Query<Artist>(EXTENSION, limit, 0, lucene_query);
            result.ArtistReleaseIncs = release_incs;
            return result;
        }

        #endregion
    }
}
