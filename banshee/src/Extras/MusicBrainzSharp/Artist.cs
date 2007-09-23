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
    
    public enum ArtistIncType
    {
        // Object
        ArtistRels = 0,
        LabelRels = 1,
        ReleaseRels = 2,
        TrackRels = 3,
        UrlRels = 4,
        
        // Entity
        Aliases = 5
    }

    public enum ArtistReleasesIncType
    {
        [StringValue("sa-")] SingleArtist,
        [StringValue("va-")] VariousArtists
    }

    public sealed class ArtistInc : Inc
    {
        public ArtistInc(ArtistIncType type)
            : base((int)type)
        {
            name = EnumUtil.EnumToString(type);
        }

        public ArtistInc(ArtistReleasesIncType type, ReleaseType release_type)
            : base(-1)
        {
            if(release_type == ReleaseType.None)
                throw new ArgumentException("You cannot use ReleaseType.None in an inc parameter");
            name = EnumUtil.EnumToString(type) + EnumUtil.EnumToString(release_type);
        }

        public ArtistInc(ArtistReleasesIncType type, ReleaseStatus release_status)
            : base(-1)
        {
            if(release_status == ReleaseStatus.None)
                throw new ArgumentException("You cannot use ReleaseStatus.None in an inc parameter");
            name = EnumUtil.EnumToString(type) + EnumUtil.EnumToString(release_status);
        }

        public static implicit operator ArtistInc(ArtistIncType type)
        {
            return new ArtistInc(type);
        }
    }
    
    public sealed class Artist : MusicBrainzEntity
    {
        const string extension = "artist";
        protected override string url_extension { get { return extension; } }

        public static ArtistInc[] DefaultIncs = new ArtistInc[] { };
        protected override Inc[] default_incs
        {
            get { return DefaultIncs; }
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
        
        Artist(string mbid, Inc[] incs, Inc[] release_incs)
            : base(mbid, incs)
        {
            foreach(Inc inc in incs) {
                if(inc.Value == -1) {
                    dont_attempt_releases = true;
                    break;
                }
            }
            this.release_incs = release_incs;
        }

        Artist(string mbid, Inc[] incs)
            : base(mbid, incs)
        {
            dont_attempt_releases = true;
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

        protected override bool ProcessAttributes(XmlReader reader)
        {
            switch(reader.GetAttribute("type")) {
            case "Group":
                type = ArtistType.Group;
                break;
            case "Person":
                type = ArtistType.Person;
                break;
            }
            return type != ArtistType.Unspecified;
        }

        protected override bool ProcessXml(XmlReader reader)
        {
            reader.Read();
            bool result = base.ProcessXml(reader);
            if(!result) {
                result = true;
                switch(reader.Name) {
                case "release-list":
                    if(reader.ReadToDescendant("release")) {
                        releases = new List<Release>();
                        do releases.Add(new Release(reader.ReadSubtree()));
                        while(reader.ReadToNextSibling("release"));
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

        ArtistType type = ArtistType.Unspecified;
        public ArtistType Type
        {
            get { return type; }
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
                        releases = new Artist(MBID, release_incs).Releases;
                return releases;
            }
        }

        #region Get

        public static Artist Get(string mbid)
        {
            return GetAdvanced(mbid, DefaultIncs, DefaultReleaseIncs);
        }

        public static Artist Get(string mbid, params ArtistInc[] incs)
        {
            return GetAdvanced(mbid, incs, DefaultReleaseIncs);
        }

        public static Artist Get(string mbid, ArtistInc[] incs, ArtistInc[] release_incs)
        {
            return GetAdvanced(mbid, incs, release_incs);
        }

        public static Artist GetAdvanced(string mbid, params Inc[] incs)
        {
            return GetAdvanced(mbid, incs, DefaultReleaseIncs);
        }

        public static Artist GetAdvanced(string mbid, Inc[] incs, Inc[] release_incs)
        {
            return new Artist(mbid, incs, release_incs);
        }

        protected override MusicBrainzObject ConstructObject(string mbid, params Inc[] incs)
        {
            return GetAdvanced(mbid, incs, DefaultReleaseIncs);
        }

        #endregion

        #region Query

        public static Query<Artist> Query(string name)
        {
            EntityQueryParameters parameters = new EntityQueryParameters();
            parameters.Name = name;
            return Query<Artist>(extension, parameters, DefaultIncs);
        }

        public static Query<Artist> Query(string name, params ArtistInc[] release_incs)
        {
            EntityQueryParameters parameters = new EntityQueryParameters();
            parameters.Name = name;
            Query<Artist> result = Query<Artist>(extension, parameters, release_incs);
            result.ArtistReleaseIncs = release_incs;
            return result;
        }

        public static Query<Artist> QueryLucene(string lucene_query)
        {
            return Query<Artist>(extension, lucene_query, DefaultIncs);
        }

        public static Query<Artist> QueryLucene(string lucene_query, params ArtistInc[] release_incs)
        {
            Query<Artist> result = Query<Artist>(extension, lucene_query, release_incs);
            result.ArtistReleaseIncs = release_incs;
            return result;
        }

        public static Query<Artist> QueryAdvanced(EntityQueryParameters parameters, int limit, Inc[] incs, Inc[] release_incs)
        {
            Query<Artist> result = Query<Artist>(extension, limit, 0, parameters, incs);
            result.ArtistReleaseIncs = release_incs;
            return result;
        }

        public static Query<Artist> QueryLuceneAdvanced(string lucene_query, int limit, Inc[] incs, Inc[] release_incs)
        {
            Query<Artist> result = Query<Artist>(extension, limit, 0, lucene_query, incs);
            result.ArtistReleaseIncs = release_incs;
            return result;
        }

        #endregion
    }
}
