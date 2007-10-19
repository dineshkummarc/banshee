using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace MusicBrainzSharp
{
    public enum LabelType
    {
        Distributor,
        Holding,
        OriginalProduction,
        BootlegProduction,
        ReissueProduction,
        Unspecified
    }
    
    public sealed class Label : MusicBrainzEntity
    {
        const string EXTENSION = "label";
        protected override string url_extension
        {
            get { return EXTENSION; }
        }

        Label(string mbid)
            : base(mbid)
        {
        }

        Label(string mbid, string parameters)
            : base(mbid, parameters)
        {
        }

        internal Label(XmlReader reader)
            : base(reader)
        {
        }

        public override void HandleLoadAllData()
        {
            Label label = new Label(MBID, CreateInc());
            type = label.Type;
            base.HandleLoadAllData(label);
        }

        protected override bool HandleAttributes(XmlReader reader)
        {
            string type_string = reader["type"];
            foreach(LabelType type in Enum.GetValues(typeof(LabelType)) as LabelType[])
                if(Utilities.EnumToString(type) == type_string) {
                    this.type = type;
                    break;
                }
            return this.type.HasValue;
        }

        protected override bool HandleXml(XmlReader reader)
        {
            reader.Read();
            bool result = base.HandleXml(reader);
			if(!result) {
				if(reader.Name == "country") {
					result = true;
					reader.Read();
					if(reader.NodeType == XmlNodeType.Text)
						country = reader.ReadContentAsString();
				} else
					reader.Skip(); // FIXME this is a workaround for Mono bug 334752
			}
            reader.Close();
            return result;
        }
		
		string country;
		public string Country
		{
			get {
				if(country == null)
					LoadAllData();
				return country;
			}
		}

        LabelType? type;
        public LabelType Type
        {
            get {
                if(!type.HasValue)
                    LoadAllData();
                return type.HasValue ? type.Value : LabelType.Unspecified;
            }
        }

        public static Label Get(string mbid)
        {
            return new Label(mbid);
        }

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
