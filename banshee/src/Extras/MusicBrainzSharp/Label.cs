using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace MusicBrainzSharp
{
    public enum LabelIncType
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

    public enum LabelType
    {
        Distributor,
        Holding,
        OriginalProduction,
        BootlegProduction,
        ReissueProduction,
        Unspecified
    }

    public sealed class LabelInc : Inc
    {
        public LabelInc(LabelIncType type)
            : base((int)type)
        {
            name = EnumUtil.EnumToString(type);
        }

        public static implicit operator LabelInc(LabelIncType type)
        {
            return new LabelInc(type);
        }
    }
    
    public sealed class Label : MusicBrainzEntity
    {
        const string extension = "label";
        protected override string url_extension { get { return extension; } }

        public static LabelInc[] DefaultIncs = new LabelInc[] { };
        protected override Inc[] default_incs
        {
            get { return DefaultIncs; }
        }
        
        Label(string mbid, params Inc[] incs)
            : base(mbid, incs)
        {
        }

        internal Label(XmlReader reader)
            : base(reader)
        {
        }

        protected override bool ProcessAttributes(XmlReader reader)
        {
            string type = reader.GetAttribute("type");
            foreach(LabelType enumeration in Enum.GetValues(typeof(LabelType)) as LabelType[])
                if(EnumUtil.EnumToString(enumeration) == type) {
                    this.type = enumeration;
                    break;
                }
            return this.type != LabelType.Unspecified;
        }

        protected override bool ProcessXml(XmlReader reader)
        {
            reader.Read();
            bool result = base.ProcessXml(reader);
            reader.Close();
            return result;
        }

        LabelType type = LabelType.Unspecified;
        public LabelType Type
        {
            get { return type; }
        }

        #region Get

        public static Label Get(string mbid)
        {
            return GetAdvanced(mbid, DefaultIncs);
        }

        public static Label Get(string mbid, params LabelInc[] incs)
        {
            return GetAdvanced(mbid, incs);
        }

        public static Label GetAdvanced(string mbid, params Inc[] incs)
        {
            return new Label(mbid, incs);
        }

        protected override MusicBrainzObject ConstructObject(string mbid, params Inc[] incs)
        {
            return GetAdvanced(mbid, incs);
        }

        #endregion

        #region Query

        public static Query<Label> Query(string name)
        {
            EntityQueryParameters parameters = new EntityQueryParameters();
            parameters.Name = name;
            return Query<Label>(extension, parameters, DefaultIncs);
        }

        public static Query<Label> QueryLucene(string lucene_query)
        {
            return Query<Label>(extension, lucene_query, DefaultIncs);
        }

        public static Query<Label> QueryAdvanced(EntityQueryParameters parameters, int limit, params Inc[] incs)
        {
            return Query<Label>(extension, limit, 0, parameters, incs);
        }

        public static Query<Label> QueryLuceneAdvanced(string lucene_query, int limit, params Inc[] incs)
        {
            return Query<Label>(extension, limit, 0, lucene_query, incs);
        }

        #endregion
    }
}
