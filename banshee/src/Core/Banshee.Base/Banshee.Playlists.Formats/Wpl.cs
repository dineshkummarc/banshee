using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Mono.Unix;

using Banshee.Base;
using Banshee.Sources;
using Banshee.Playlists;

namespace Banshee.Playlists.Formats
{
    public class Wpl : PlaylistFile
    {
        
        public Wpl()
        {
        }

        public override string Extension
        {
            get { return "wpl"; }
        }

        public override string Name
        {
            get { return Catalog.GetString("Windows Media Player Playlist (*.wpl)"); }
        }

        public override string GetPlaylistName(string playlist_uri)
        {
            XmlReader reader = new XmlTextReader(playlist_uri);
            reader.ReadToFollowing("title");
            reader.Read();
            return reader.Value;
        }

        public override void Export(string uri, Source source)
        {
            try {
                bool use_relative_paths = true;
                string path = UpdateExtension(uri);
                string save_directory = Path.GetDirectoryName(path);

                if(save_directory == null) {
                    use_relative_paths = false;
                }

                XmlTextWriter writer = new XmlTextWriter(path, null);
                writer.Formatting = Formatting.Indented;
                writer.IndentChar = ' ';
                writer.Indentation = 4;
                
                // Header
                writer.WriteRaw("<?wpl version=\"1.0\"?>");
                writer.WriteStartElement("smil");
                writer.WriteStartElement("head");

                writer.WriteStartElement("meta");
                writer.WriteAttributeString("name", "Generator");
                writer.WriteAttributeString("content", "Banshee -- " + ConfigureDefines.VERSION);
                writer.WriteEndElement();

                writer.WriteElementString("title", source.Name);

                writer.WriteEndElement();
                writer.WriteStartElement("body");
                writer.WriteStartElement("seq");

                // Songs
                foreach(TrackInfo ti in source.Tracks) {
                    writer.WriteStartElement("media");
                    writer.WriteAttributeString("src", use_relative_paths
                        ? AbsoluteToRelative(ti.Uri.AbsolutePath, save_directory)
                        : ti.Uri.AbsolutePath);
                    writer.WriteEndElement();
                }

                // Footer
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.Flush();
                writer.Close();
            } catch(Exception e) {
                LogCore.Instance.PushError(Catalog.GetString("Exception: "), e.Message);
            }
        }

        public override string[] Import(string uri)
        {
            List<string> list = new List<string>();
            XmlReader reader = null;
            try {
                reader = new XmlTextReader(uri);

                bool validFile = false;
                string line = null;
                while(reader.ReadToFollowing("media")) {
                    reader.MoveToAttribute(0);
                    line = reader.Value;

                    string fullPath = IsValidFile(uri, line);
                    if(fullPath != null) {
                        list.Add(fullPath);
                        validFile = true;
                    }
                }

                if(!validFile) {
                    throw new InvalidPlaylistException(Catalog.GetString("Not a valid WPL file."));
                }

            } finally {
                if(reader != null) {
                    reader.Close();
                }
            }

            return list.ToArray();
        }
    }
}
