using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace MusicBrainzSharp
{
    #region Enums

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

    #endregion

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
        const string EXTENSION = "label";
        protected override string url_extension { get { return EXTENSION; } }

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
            string type_string = reader.GetAttribute("type");
            foreach(LabelType type in Enum.GetValues(typeof(LabelType)) as LabelType[])
                if(EnumUtil.EnumToString(type) == type_string) {
                    this.type = type;
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
            return Get(mbid, (Inc[])DefaultIncs);
        }

        public static Label Get(string mbid, params LabelInc[] incs)
        {
            return Get(mbid, (Inc[])incs);
        }

        static Label Get(string mbid, params Inc[] incs)
        {
            return new Label(mbid, incs);
        }

        protected override MusicBrainzObject ConstructObject(string mbid, params Inc[] incs)
        {
            return Get(mbid, incs);
        }

        #endregion

        #region Query

        public static Query<Label> Query(string name)
        {
            EntityQueryParameters parameters = new EntityQueryParameters();
            parameters.Name = name;
            return Query<Label>(EXTENSION, parameters);
        }

        public static Query<Label> Query(string name, byte limit)
        {
            EntityQueryParameters parameters = new EntityQueryParameters();
            parameters.Name = name;
            return Query<Label>(EXTENSION, limit, 0, parameters);
        }

        public static Query<Label> QueryLucene(string lucene_query)
        {
            return Query<Label>(EXTENSION, lucene_query);
        }

        public static Query<Label> QueryLucene(string lucene_query, byte limit)
        {
            return Query<Label>(EXTENSION, limit, 0, lucene_query);
        }

        #endregion
    }
}
