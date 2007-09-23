using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;

namespace MusicBrainzSharp
{
    public class EntityQueryParameters : QueryParameters
    {
        string name;
        public string Name
        {
            get { return name; }
            set
            {
                if(value == null)
                    throw new NullReferenceException("You cannot specify a null name string.");
                name = value;
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(name.Length);
            builder.Append("&name=");
            AppendStringToBuilder(builder, name);
            return builder.ToString();
        }
    }
    
    // A person-like entity, such as an artist or a label.
    public abstract class MusicBrainzEntity : MusicBrainzObject
    {
        protected MusicBrainzEntity(string mbid, params Inc[] incs)
            : base(mbid, incs)
        {
            foreach(Inc inc in incs)
                if(inc.Value == (int)ArtistIncType.Aliases) {
                    dont_attempt_aliases = true;
                    break;
                }
        }

        protected MusicBrainzEntity(XmlReader reader)
            : base(reader)
        {
        }
        
        protected override bool ProcessXml(XmlReader reader)
        {
            bool result = true;
            switch(reader.Name) {
            case "name":
                reader.Read();
                name = reader.ReadContentAsString();
                break;
            case "sort-name":
                reader.Read();
                sort_name = reader.ReadContentAsString();
                break;
            case "disambiguation":
                reader.Read();
                disambiguation = reader.ReadContentAsString();
                break;
            case "life-span":
                string begin = reader.GetAttribute("begin");
                if(begin != null)
                    DateTime.TryParse(begin, out begin_date);
                string end = reader.GetAttribute("end");
                if(end != null)
                    DateTime.TryParse(end, out end_date);
                break;
            case "alias-list":
                if(reader.ReadToDescendant("alias")) {
                    aliases = new List<string>();
                    do {
                        reader.Read();
                        aliases.Add(reader.ReadContentAsString());
                    } while(reader.ReadToNextSibling("alias"));
                }
                break;
            default:
                result = false;
                break;
            }
            return result;
        }

        string name = string.Empty;
        public string Name
        {
            get { return name; }
        }

        string sort_name = string.Empty;
        public string SortName
        {
            get { return sort_name; }
        }

        string disambiguation = string.Empty;
        public string Disambiguation
        {
            get { return disambiguation; }
        }

        DateTime begin_date;
        public DateTime BeginDate
        {
            get { return begin_date; }
        }

        DateTime end_date;
        public DateTime EndDate
        {
            get { return end_date; }
        }

        List<string> aliases;
        bool dont_attempt_aliases;
        public List<string> Aliases
        {
            get {
                if(aliases == null)
                    aliases = dont_attempt_aliases
                        ? new List<string>()
                        : ((MusicBrainzEntity)ConstructObject(
                            MBID, ArtistIncType.Aliases)).Aliases;

                return aliases;
            }
        }
    }
}
