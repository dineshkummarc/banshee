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
        ReleaseFormat format = ReleaseFormat.None;

        internal Event(XmlReader reader, Release release)
        {
            reader.Read();
            date = reader["date"];
            country = reader["country"];
            catalog_number = reader["catalog-number"];
            barcode = reader["barcode"];
            string format_string = reader["format"];
            if(format_string != null)
                foreach(ReleaseFormat format in Enum.GetValues(typeof(ReleaseFormat)))
                    if(format.ToString() == format_string) {
                        this.format = format;
                        break;
                    }
            if(reader.ReadToDescendant("label"))
                label = new Label(reader.ReadSubtree());
            reader.Close();
            this.release = release;
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
