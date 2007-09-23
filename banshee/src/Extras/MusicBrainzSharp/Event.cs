using System;
using System.Xml;

namespace MusicBrainzSharp
{
    public class Event
    {
        DateTime date;
        string country;
        string catalog_number;
        string barcode;
        Label label;
        Release release;

        internal Event(XmlReader reader, Release release)
        {
            this.release = release;
            reader.Read();
            string date = reader.GetAttribute("date");
            if(date != null)
                DateTime.TryParse(date, out this.date);
            country = reader.GetAttribute("country") ?? string.Empty;
            catalog_number = reader.GetAttribute("catalog-number") ?? string.Empty;
            barcode = reader.GetAttribute("barcode") ?? string.Empty;
            if(reader.ReadToDescendant("label"))
                label = new Label(reader.ReadSubtree());
            reader.Close();
        }

        public DateTime Date
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
    }
}
