using System;
using System.Xml;

namespace MusicBrainzSharp
{
    public class Event
    {
        string date;
        string country;
        string catalog_number;
        string barcode;
        Label label;
        Release release;
        ReleaseFormat format;

        internal Event(XmlReader reader, Release release)
        {
            this.release = release;
            reader.Read();
            date = reader.GetAttribute("date") ?? string.Empty;
            country = reader.GetAttribute("country") ?? string.Empty;
            catalog_number = reader.GetAttribute("catalog-number") ?? string.Empty;
            barcode = reader.GetAttribute("barcode") ?? string.Empty;
            string format_string = reader.GetAttribute("format");
            if(format_string != null)
                foreach(ReleaseFormat format in Enum.GetValues(typeof(ReleaseFormat)))
                    if(Enum.GetName(typeof(ReleaseFormat), format) == format_string) {
                        this.format = format;
                        break;
                    }
            if(reader.ReadToDescendant("label"))
                label = new Label(reader.ReadSubtree());
            reader.Close();
        }

        public string Date
        {
            get { return date; }
        }

        public string Country
        {
            get { return country; }
        }

        public string CatalogNumber
        {
            get { return catalog_number; }
        }

        public string Barcode
        {
            get { return barcode; }
        }

        public Label Label
        {
            get {
                if(label == null)
                    release.GetEventLabel();
                return label;
            }
            internal set { label = value; }
        }

        public ReleaseFormat Format
        {
            get { return format; }
        }
    }
}
