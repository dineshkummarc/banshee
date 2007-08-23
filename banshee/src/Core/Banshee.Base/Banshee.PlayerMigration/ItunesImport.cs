/*
 *  Copyright (c) 2007 Scott Peterson <lunchtimemama@gmail.com> 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using Mono.Unix;
using Gtk;

using Banshee.Base;
using Banshee.IO;
using Banshee.Widgets;
using Banshee.Sources;
using Banshee.SmartPlaylist;

namespace Banshee.PlayerMigration
{
    public class ItunesImport : PlayerImport
    {
        // This is its own class so that we don't always load this stuff into memory
        private class ItunesImportData
        {
            public string library_uri, default_query, local_prefix, fallback_dir;
            public string [] query_dirs;
            public bool get_ratings, get_stats, get_playlists, get_smart_playlists, local_library;
            public uint total_songs, total_processed, total_count, ratings_count, stats_count, playlists_count, smart_playlists_count;
            public List<ItunesSmartPlaylist> partial_smart_playlists, failed_smart_playlists;
            public volatile bool canceled;
                
        }

        private static ItunesImport instance;
        public static PlayerImport Instance {
            get {
                if(instance == null) {
                    instance = new ItunesImport();
                }
                return instance;
            }
        }
        
        private ItunesImport ()
        {
        }
        
        public override string Name {
            get { return Catalog.GetString ("iTunes"); }
        }

        public static bool CanImport {
            get { return true; }
        }

        private ItunesImportData data;

        public override void Import()
        {
            data = new ItunesImportData();
            if (!PromptUser()) {
                data = null;
                return;
            }

            CreateUserEvent();
            user_event.CancelRequested += delegate {
                data.canceled = true;
            };

            ThreadAssist.Spawn(delegate {
                DoImport();
                user_event.Dispose();
                user_event = null;
                data = null;
            });
        }

        private bool PromptUser()
        {
            ItunesImportDialog import_dialog = new ItunesImportDialog();
            if(import_dialog.Run() == (int)ResponseType.Ok) {
                data.library_uri = import_dialog.LibraryUri;
                data.get_ratings = import_dialog.Ratings;
                data.get_stats = import_dialog.Stats;
                data.get_playlists = import_dialog.Playliststs;
                data.get_smart_playlists = import_dialog.SmartPlaylists;
                data.local_library = import_dialog.LocalLibrary;
            }
            import_dialog.Destroy();
            import_dialog.Dispose();

            if(data.library_uri == null || data.library_uri.Length == 0) {
                return false;
            }

            // Make sure the library version is supported (version 1.1)
            string message = null;
            bool prompt = false;
            XmlReader xml_reader = new XmlTextReader(data.library_uri);
            xml_reader.ReadToFollowing("key");
            do {
                xml_reader.Read();
                string key = xml_reader.ReadContentAsString();
                if(key == "Major Version" || key == "Minor Version") {
                    xml_reader.Read();
                    xml_reader.Read();
                    if(xml_reader.ReadContentAsString() != "1") {
                        message = Catalog.GetString(String.Format(
                            "{0} is not familiar with this version of the iTunes library format." +
                            " Importing may or may not work as expected, or at all. Would you like to attempt to import anyway?",
                            Branding.ApplicationName));
                        prompt = true;
                        break;
                    }
                }
            } while(xml_reader.ReadToNextSibling("key"));
            xml_reader.Close();

            if(prompt) {
                bool proceed = false;
                using(MessageDialog dialog = new MessageDialog(null, 0, MessageType.Question, ButtonsType.YesNo, message)) {
                    if(dialog.Run() == (int)ResponseType.Yes) {
                        proceed = true;
                    }
                    dialog.Destroy();
                }
                if(!proceed) {
                    Banshee.Sources.ImportErrorsSource.Instance.AddError(data.library_uri,
                        "Unsupported version", null);
                    return false;
                }
            }

            if(data.get_smart_playlists) {
                data.partial_smart_playlists = new List<ItunesSmartPlaylist>();
                data.failed_smart_playlists = new List<ItunesSmartPlaylist>();
            }
            return true;
        }

        protected override void DoImport()
        {
            data.canceled = false;
            CheckDatabase();
            CountSongs();
            XmlReader xml_reader = new XmlTextReader(data.library_uri);
            ProcessLibraryXml(xml_reader);
            xml_reader.Close();
        }

        private void CheckDatabase()
        {
            try {
                Globals.Library.Db.Execute("SELECT ItunesSynced FROM Tracks");
            } catch(Exception e) {
                if(e.Message != "no such column: ItunesSynced") {
                    throw e;
                }
                Globals.Library.Db.Execute("ALTER TABLE Tracks ADD ItunesSynced INTEGER");
                Globals.Library.Db.Execute("UPDATE Tracks SET ItunesSynced = 0");
            }

            try {
                Globals.Library.Db.Execute("SELECT ItunesID FROM Tracks");
            } catch(Exception e) {
                if(e.Message != "no such column: ItunesID") {
                    throw e;
                }
                Globals.Library.Db.Execute("ALTER TABLE Tracks ADD ItunesID INTEGER");
            } finally {
                Globals.Library.Db.Execute("UPDATE Tracks SET ItunesID = 0");
            }
        }

        private void CountSongs()
        {
            XmlTextReader xml_reader = new XmlTextReader(data.library_uri);
            xml_reader.ReadToDescendant("dict");
            xml_reader.ReadToDescendant("dict");
            xml_reader.ReadToDescendant("dict");
            do {
                data.total_songs++;
            } while(xml_reader.ReadToNextSibling("dict"));
            xml_reader.Close();
        }
        
        private void ProcessLibraryXml(XmlReader xml_reader)
        {
            while(xml_reader.ReadToFollowing("key") && !data.canceled) {
                xml_reader.Read();
                string key = xml_reader.ReadContentAsString();
                xml_reader.Read();
                xml_reader.Read();

                switch(key) {
                case "Music Folder":
                    if(!data.local_library && !ProcessMusicFolderPath(xml_reader.ReadContentAsString())) {
                        return;
                    }
                    break;
                case "Tracks":
                    ProcessSongs(xml_reader.ReadSubtree());
                    break;
                case "Playlists":
                    if(data.get_playlists || data.get_smart_playlists) {
                        ProcessPlaylists(xml_reader.ReadSubtree());
                    }
                    break;
                }
            }
        }

        private bool ProcessMusicFolderPath(string path)
        {
            string[] itunes_music_uri_parts = ConvertToLocalUriFormat(path).Split(Path.DirectorySeparatorChar);
            string[] library_uri_parts = Path.GetDirectoryName(data.library_uri).Split(Path.DirectorySeparatorChar);

            string itunes_dir_name = library_uri_parts[library_uri_parts.Length - 1];
            int i = 0;
            bool found = false;
            for(i = itunes_music_uri_parts.Length - 1; i >= 0; i--) {
                if(itunes_music_uri_parts[i] == itunes_dir_name) {
                    found = true;
                    break;
                }
            }
            if(!found) {
                Banshee.Sources.ImportErrorsSource.Instance.AddError(data.library_uri,
                  "Unable to locate iTunes directory from iTunes URI", null);
                return false;
            }

            string[] local_prefix_parts = new string[library_uri_parts.Length + (itunes_music_uri_parts.Length - i) - 1];
            for(int j = 0; j < library_uri_parts.Length - 1; j++) {
                local_prefix_parts[j] = library_uri_parts[j];
            }
            for(int j = i; j < itunes_music_uri_parts.Length; j++) {
                local_prefix_parts[local_prefix_parts.Length - (itunes_music_uri_parts.Length - j)] = itunes_music_uri_parts[j];
            }

            string[] tmp_query_dirs = new string[itunes_music_uri_parts.Length];
            string upstream_uri;
            string tmp_upstream_uri = null;
            int step = 0;
            do {
                upstream_uri = tmp_upstream_uri;
                tmp_upstream_uri = Path.GetPathRoot(data.library_uri);
                for(int j = 0; j < library_uri_parts.Length - step - 1; j++) {
                    tmp_upstream_uri = Path.Combine(tmp_upstream_uri, library_uri_parts[j]);
                }
                tmp_upstream_uri = Path.Combine(tmp_upstream_uri, itunes_music_uri_parts[i - step]);
                data.fallback_dir = tmp_query_dirs[step] = itunes_music_uri_parts[i - step];
                step++;
            } while(IOProxy.Directory.Exists(tmp_upstream_uri));
            if(upstream_uri == null) {
                Banshee.Sources.ImportErrorsSource.Instance.AddError(data.library_uri,
                  "Unable to reslove iTunes URIs to local URIs", null);
                return false;
            }
            data.query_dirs = new string[step - 2];
            data.default_query = string.Empty;

            for(int j = step - 2; j >= 0; j--) {
                if(j > 0) {
                    data.query_dirs[j - 1] = tmp_query_dirs[j];
                }
                data.default_query += tmp_query_dirs[j] + Path.DirectorySeparatorChar;

            }

            data.local_prefix = string.Empty;
            for(int j = 0; j < step; j++) {
                data.local_prefix += local_prefix_parts[j] + Path.DirectorySeparatorChar;
            }

            return true;
        }

        private void ProcessSongs(XmlReader xml_reader)
        {
            xml_reader.ReadToFollowing("dict");
            while(xml_reader.ReadToFollowing("dict") && !data.canceled) {
                ProcessSong(xml_reader.ReadSubtree());
            }
            xml_reader.Close();
        }

        private void ProcessPlaylists(XmlReader xml_reader)
        {
            user_event.Header = Catalog.GetString("Importing Playlists");
            user_event.Progress = 0;
            while(xml_reader.ReadToFollowing("dict") && !data.canceled) {
                ProcessPlaylist(xml_reader.ReadSubtree());
            }
            xml_reader.Close();
        }

        private void ProcessSong(XmlReader xml_reader)
        {
            string location = null;
            int itunes_id = 0;
            byte rating = 0;
            uint play_count = 0;
            DateTime last_played = new DateTime();

            while(xml_reader.ReadToFollowing("key")) {
                xml_reader.Read();
                string key = xml_reader.ReadContentAsString();
                xml_reader.Read();
                xml_reader.Read();

                switch (key) {
                case "Track ID":
                    itunes_id = int.Parse(xml_reader.ReadContentAsString());
                    break;
                case "Play Count":
                    play_count = uint.Parse(xml_reader.ReadContentAsString());
                    break;
                case "Play Date UTC":
                    last_played = DateTime.Parse(xml_reader.ReadContentAsString());
                    break;
                case "Rating":
                    rating = byte.Parse(xml_reader.ReadContentAsString());
                    break;
                case "Location":
                    location = xml_reader.ReadContentAsString();
                    break;
                }
            }
            xml_reader.Close();

            data.total_processed++;
            location = ConvertToLocalUri(location);
            if(location == null) {
                return;
            }
            SafeUri safe_uri = null;
            try {
                safe_uri = new SafeUri(location);
            } catch {
                Banshee.Sources.ImportErrorsSource.Instance.AddError(location,
                    "URI is not a local file path", null);
                return;
            }
            
            if(!IOProxy.File.Exists(safe_uri)) {
                Banshee.Sources.ImportErrorsSource.Instance.AddError(location,
                    "File does not exist", null);
                return;
            }
            
            // Update the song if it is already in the library
            int track_id = LibraryTrackInfo.GetId(safe_uri);
            bool update = track_id != 0;
            TrackInfo track_info = null;
            try {
                track_info = update
                    ? Globals.Library.GetTrack(track_id)
                    : new LibraryTrackInfo(safe_uri.AbsoluteUri);
            } catch (Exception e) {
                Banshee.Sources.ImportErrorsSource.Instance.AddError(location, e.Message, e);
                return;
            }

            // Rating
            if(data.get_ratings && rating != 0 && track_info.Rating == 0) {
                track_info.Rating = (uint)rating / 20;
                data.ratings_count++;
            }
            bool stats_updated = false;

            // Play count
            while(data.get_stats && play_count != 0) {
                uint previous_sync = 0;
                if(update) {
                    try {
                        previous_sync = (uint)Globals.Library.Db.QuerySingle(String.Format(
                            "SELECT ItunesSynced FROM Tracks WHERE TrackID = {0}", track_info.TrackId));
                    } catch {
                        previous_sync = 0;
                    }
                }
                if(play_count <= previous_sync) {
                    break;
                }
                Globals.Library.Db.Execute(String.Format(
                    "UPDATE Tracks SET ItunesSynced = {0} WHERE TrackID = {1}", play_count, track_info.TrackId));
                play_count -= previous_sync;
                track_info.PlayCount += play_count;
                data.stats_count++;
                stats_updated = true;
                break;
            }

            // Last played
            if(data.get_stats && last_played != new DateTime () && DateTime.Compare (last_played, track_info.LastPlayed) > 0) {
                track_info.LastPlayed = last_played;
                if(!stats_updated) {
                    data.stats_count++;
                }
            }

            // iTunesID
            Globals.Library.Db.Execute(String.Format(
                "UPDATE Tracks SET ItunesID = {0} WHERE TrackID = {1}", itunes_id, track_info.TrackId));

            track_info.Save();
            data.total_count++;
            UpdateUserEvent((int)data.total_processed, (int)data.total_songs, track_info.Artist, track_info.Title);
        }

        private void ProcessPlaylist(XmlReader xml_reader)
        {
            string name = string.Empty;
            bool skip = false;
            bool processed = false;
            byte[] smart_info = null;
            byte[] smart_criteria = null;

            while(xml_reader.ReadToFollowing("key")) {
                xml_reader.Read();
                string key = xml_reader.ReadContentAsString();
                xml_reader.Read();

                switch (key) {
                case "Name":
                    xml_reader.Read();
                    name = xml_reader.ReadContentAsString();
                    if(name == "Library") {
                        skip = true;
                    }
                    //if(name == "Music Videos")
                    //skip = true;
                    break;
                case "Audiobooks":
                    goto case "skip";
                case "Music":
                    goto case "skip";
                case "Movies":
                    goto case "skip";
                case "Party Shuffle":
                    goto case "skip";
                case "Podcasts":
                    goto case "skip";
                case "Purchased Music":
                    goto case "skip";
                case "TV Shows":
                    goto case "skip";
                case "skip":
                    if(xml_reader.Name == "true") {
                        skip = true;
                    }
                    break;
                case "Smart Info":
                    xml_reader.Read();
                    xml_reader.Read();
                    smart_info = Convert.FromBase64String(xml_reader.ReadContentAsString());
                    break;
                case "Smart Criteria":
                    xml_reader.Read();
                    xml_reader.Read();
                    smart_criteria = Convert.FromBase64String(xml_reader.ReadContentAsString());
                    break;
                case "Playlist Items":
                    xml_reader.Read();
                    if(!skip) {
                        ProcessPlaylist(name, smart_info, smart_criteria, xml_reader.ReadSubtree());
                        processed = true;
                    }
                    break;
                }
            }
            xml_reader.Close();

            // Empty playlist
            if(!processed && !skip) {
                ProcessPlaylist(name, smart_info, smart_criteria, null);
            }
        }

        private void ProcessPlaylist(string name, byte[] smart_info, byte[] smart_criteria, XmlReader xml_reader)
        {
            user_event.Header = Catalog.GetString("Importing Playlist ") + name;

            if(data.get_playlists && smart_info == null) {
                ProcessRegularPlaylist(name, xml_reader);
            } else if(data.get_smart_playlists && smart_info != null && smart_criteria != null) {
                ProcessSmartPlaylist(name, smart_info, smart_criteria, xml_reader);
            }
            if(xml_reader != null) {
                xml_reader.Close();
            }
        }

        private void ProcessRegularPlaylist(string name, XmlReader xml_reader)
        {
            // Should we do this?
            /*foreach(PlaylistSource playlist in PlaylistSource.Playlists) {
                if(playlist.Name == name) {
                    playlist.Unmap();
                    Console.WriteLine("Replacing playlist " + name);
                    break;
                }
            }*/
            PlaylistSource playlist_source = new PlaylistSource();
            playlist_source.Rename(name);
            LibrarySource.Instance.AddChildSource(playlist_source);

            // Get the songs in the playlists
            if(xml_reader != null) {
                while(xml_reader.ReadToFollowing("integer") && !data.canceled) {
                    xml_reader.Read();
                    int itunes_id = int.Parse(xml_reader.ReadContentAsString());
                    TrackInfo track;
                    try {
                        int track_id = (int)Globals.Library.Db.QuerySingle(String.Format(
                            "SELECT TrackID FROM Tracks WHERE ItunesID = {0}", itunes_id));
                        track = Globals.Library.GetTrack(track_id);
                    }
                    catch {
                        continue;
                    }
                    playlist_source.AddTrack(track);
                    user_event.Message = String.Format("{0} - {1}", track.Artist, track.Title);
                }
            }
            user_event.Message = string.Empty;
            playlist_source.Commit();
            data.playlists_count++;
        }

        private void ProcessSmartPlaylist(string name, byte[] info, byte[] criteria, XmlReader xml_reader)
        {
            ItunesSmartPlaylist smart_playlist = new SmartPlaylistParser().Parse(info, criteria);
            smart_playlist.Name = name;

            if(!(smart_playlist.Query.Length == 0 && smart_playlist.Ignore.Length != 0) || smart_playlist.LimitNumber != 0) {
                
                // Is there a collection of Smart Playlists?
                /*ChildSource[] temp_array = new ChildSource[LibrarySource.Instance.Children.Count];
                LibrarySource.Instance.Children.CopyTo(temp_array, 0);
                // Should we do this?
                for(int i = 0; i < temp_array.Length; i++) {
                    if(temp_array[i].Name == name && temp_array[i].GenericName == "Smart Playlist") {
                        temp_array[i].Unmap();
                        Console.WriteLine("Replacing smart playlist " + name);
                        break;
                    }
                }*/

                SmartPlaylistSource smart_playlist_source = new SmartPlaylistSource(
                    name,
                    (smart_playlist.Query.Length == 0) ? null : " " + smart_playlist.Query,
                    smart_playlist.OrderBy,
                    smart_playlist.LimitNumber.ToString(),
                    smart_playlist.LimitMethod
                );

                if(!SourceManager.ContainsSource(smart_playlist_source) &&
                    SourceManager.ContainsSource(LibrarySource.Instance)) {
                    LibrarySource.Instance.AddChildSource(smart_playlist_source);
                }
            }

            if (smart_playlist.Ignore.Length != 0) {
                if (smart_playlist.Query.Length != 0) {
                    data.partial_smart_playlists.Add(smart_playlist);
                } else {
                    data.failed_smart_playlists.Add(smart_playlist);
                }
                ProcessRegularPlaylist(name, xml_reader);
            } else {
                data.smart_playlists_count++;
            }
        }

        private string ConvertToLocalUri(string uri)
        {
            if(uri == null) {
                return null;
            }

            string local_uri = null;
            uri = ConvertToLocalUriFormat(uri);
            if(!data.local_library) {
                int index = uri.IndexOf(data.default_query);
                if(index == -1 && data.query_dirs.Length > 0) {
                    int count = 0;
                    string path = data.query_dirs[data.query_dirs.Length - 1];
                    do {
                        for(int k = data.query_dirs.Length - 2; k >= count; k--) {
                            path = Path.Combine(path, data.query_dirs[k]);
                        }
                        index = uri.IndexOf(path);
                        count++;
                    } while(index == -1 && count < data.query_dirs.Length);
                    if(index == -1) {
                        index = uri.IndexOf(data.fallback_dir);
                        if(index != -1) {
                            index += data.fallback_dir.Length + 1;
                        }
                    }
                }
                if(index == -1) {
                    Banshee.Sources.ImportErrorsSource.Instance.AddError(uri,
                        "Unable to map iTunes URI to local URI", null);
                    return null;
                }
                local_uri = uri.Substring(index, uri.Length - index);
                local_uri = Path.Combine(data.local_prefix, local_uri);
            } else {
                local_uri = uri.Substring(17); // 17 is the length of "file://localhost/"
            }
            return local_uri;
        }

        // URIs are UTF-8 percent-encoded. Deconding with System.Web.HttpServerUtility
        // involves too much overhead, so we do it cheap here.
        private static string ConvertToLocalUriFormat(string input)
        {
            StringBuilder builder = new StringBuilder(input.Length);
            byte [] buffer = new byte [2];
            bool using_buffer = false;
            for(int i = 0; i < input.Length; i++) {
                // If it's a '%', treat the two subsiquent characters as a UTF-8 byte in hex.
                if(input[i] == '%') {
                    byte code = byte.Parse(input.Substring(i + 1, 2),
                        System.Globalization.NumberStyles.HexNumber);
                    // If it's a non-ascii character, or there are already some non-ascii
                    // characters in the buffer, then queue it for UTF-8 decoding.
                    if(using_buffer || (code & 0x80) != 0) {
                        if(using_buffer) {
                            if(buffer[1] == 0) {
                                buffer[1] = code;
                            } else {
                                byte [] new_buffer = new byte [buffer.Length + 1];
                                for (int j = 0; j < buffer.Length; j++) {
                                    new_buffer[j] = buffer[j];
                                }
                                buffer = new_buffer;
                                buffer[buffer.Length - 1] = code;
                            }
                        } else {
                            buffer[0] = code;
                            using_buffer = true;
                        }
                    }
                    // If it's a lone ascii character, there's no need for fancy UTF-8 decoding.
                    else {
                        builder.Append((char)code);
                    }
                    i += 2;
                } else {
                    // If we have something in the buffer, decode it.
                    if(using_buffer) {
                        builder.Append(Encoding.UTF8.GetString(buffer));
                        if(buffer.Length > 2) {
                            buffer = new byte [2];
                        } else {
                            buffer[1] = 0;
                        }
                        using_buffer = false;
                    }
                    // And add our regular characters and convert to local directory separator char.
                    if(input[i] == '/') {
                        builder.Append(Path.DirectorySeparatorChar);
                    } else {
                        builder.Append(input[i]);
                    }
                }
            }
            return builder.ToString();
        }
    }
}
