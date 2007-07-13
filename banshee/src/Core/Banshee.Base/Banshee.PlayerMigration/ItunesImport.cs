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
                
        }
        private static ItunesImport instance;
        public static PlayerImport Instance
        {
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
        
        public override string Name
        {
            get { return Catalog.GetString ("iTunes"); }
        }
        public static bool CanImport
        {
            get { return true; }
        }
        
        private volatile bool canceled;
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
                canceled = true;
            };

            ThreadAssist.Spawn(delegate {
                DoImport();
                Done();
            });
        }

        protected override void DoImport()
        {
            canceled = false;
            CheckDatabase();

            // TODO check version
            XmlDocument xml_document = new XmlDocument();
            xml_document.Load(data.library_uri);
            XmlNode node = xml_document.FirstChild.NextSibling.NextSibling.FirstChild.FirstChild;
            while (node.Name != "dict")
            {
                switch (node.InnerText) {
                    case "Major Version":
                        goto case "Version";
                    case "Minor Version":
                        goto case "Version";
                    case "Version":
                        if (node.NextSibling.InnerText != "1") {
                            // TODO prompt user to continue anyway
                            Banshee.Sources.ImportErrorsSource.Instance.AddError(data.library_uri,
                                "Unsupported version", null);
                            return;
                        }
                        break;
                    case "Music Folder":
                        if(data.local_library) {
                            break;
                        }
                        string[] itunes_music_uri_parts = ConvertToLocalUriFormat(node.NextSibling.InnerText).
                            Split(Path.DirectorySeparatorChar);
                        string[] library_uri_parts = Path.GetDirectoryName(data.library_uri).Split(Path.DirectorySeparatorChar);

                        string itunes_dir_name = library_uri_parts[library_uri_parts.Length - 1];
                        int i = 0;
                        bool found = false;
                        for (i = itunes_music_uri_parts.Length - 1; i >= 0; i--) {
                            if (itunes_music_uri_parts[i] == itunes_dir_name) {
                                found = true;
                                break;
                            }
                        }
                        if (!found) {
                            Banshee.Sources.ImportErrorsSource.Instance.AddError(data.library_uri,
                              "Unable to locate iTunes directory from iTunes URI", null);
                            return;
                        }

                        string[] local_prefix_parts = new string[library_uri_parts.Length + (itunes_music_uri_parts.Length - i) - 1];
                        for (int j = 0; j < library_uri_parts.Length - 1; j++) {
                            local_prefix_parts[j] = library_uri_parts[j];
                        }
                        for (int j = i; j < itunes_music_uri_parts.Length; j++) {
                            local_prefix_parts[local_prefix_parts.Length - (itunes_music_uri_parts.Length - j)] = itunes_music_uri_parts[j];
                        }

                        string[] tmp_query_dirs = new string[itunes_music_uri_parts.Length];
                        string upstream_uri;
                        string tmp_upstream_uri = null;
                        int step = 0;
                        do {
                            upstream_uri = tmp_upstream_uri;
                            tmp_upstream_uri = Path.GetPathRoot(data.library_uri);
                            for (int j = 0; j < library_uri_parts.Length - step - 1; j++) {
                                tmp_upstream_uri = Path.Combine(tmp_upstream_uri, library_uri_parts[j]);
                            }
                            tmp_upstream_uri = Path.Combine(tmp_upstream_uri, itunes_music_uri_parts[i - step]);
                            data.fallback_dir = tmp_query_dirs[step] = itunes_music_uri_parts[i - step];
                            step++;
                        }
                        while (IOProxy.Directory.Exists(tmp_upstream_uri));
                        if (upstream_uri == null) {
                            Banshee.Sources.ImportErrorsSource.Instance.AddError(data.library_uri,
                              "Unable to reslove iTunes URIs to local URIs", null);
                            return;
                        }
                        data.query_dirs = new string[step - 2];
                        data.default_query = "";

                        for (int j = step - 2; j >= 0; j--) {
                            if (j > 0) {
                                data.query_dirs[j - 1] = tmp_query_dirs[j];
                            }
                            data.default_query += tmp_query_dirs[j] + Path.DirectorySeparatorChar;

                        }

                        data.local_prefix = "";
                        for (int j = 0; j < step; j++) {
                            data.local_prefix += local_prefix_parts[j] + Path.DirectorySeparatorChar;
                        }
                        break;
                }
                node = node.NextSibling;
            }
            data.total_songs = (uint)node.ChildNodes.Count / 2;
            foreach (XmlNode dict in node.ChildNodes) {
                if (dict.Name == "dict") {
                    ProcessSong(dict.ChildNodes);
                    if (canceled) {
                        return;
                    }
                }
            }
            user_event.Header = Catalog.GetString("Importing Playlists");
            user_event.Progress = 0;
            if (data.get_playlists || data.get_smart_playlists) {
                XmlNode playlist_array = node.NextSibling.NextSibling;
                XmlNodeList playlist_dicts = playlist_array.ChildNodes;
                for (int i = 1; i < playlist_dicts.Count; i++) {
                    ProcessPlaylist(playlist_dicts[i]);
                    if (canceled) {
                        return;
                    }
                }
            }
        }

        private void Done()
        {
            user_event.Dispose();
            user_event = null;
        }

        private bool PromptUser()
        {
            ItunesImportDialog import_dialog = new ItunesImportDialog();
            if(import_dialog.Run() == (int)Gtk.ResponseType.Ok) {
                data.library_uri = import_dialog.LibraryUri;
                data.get_ratings = import_dialog.Ratings;
                data.get_stats = import_dialog.Stats;
                data.get_playlists = import_dialog.Playliststs;
                data.get_smart_playlists = import_dialog.SmartPlaylists;
                data.local_library = import_dialog.LocalLibrary;
            }
            import_dialog.Destroy();

            if(data.library_uri == null || data.library_uri.Length == 0) {
                return false;
            }
            if(data.get_smart_playlists) {
                data.partial_smart_playlists = new List<ItunesSmartPlaylist>();
                data.failed_smart_playlists = new List<ItunesSmartPlaylist>();
            }
            return true;
        }

        private void CheckDatabase()
        {
            try {
                Globals.Library.Db.Execute("SELECT ItunesSynced FROM Tracks");
            } catch(Exception e) {
                if (e.Message != "no such column: ItunesSynced") {
                    throw e;
                }
                Globals.Library.Db.Execute("ALTER TABLE Tracks ADD ItunesSynced INTEGER");
                Globals.Library.Db.Execute("UPDATE Tracks SET ItunesSynced = 0");
            }
            try {
                Globals.Library.Db.Execute("SELECT ItunesID FROM Tracks");
            } catch(Exception e) {
                if (e.Message != "no such column: ItunesID") {
                    throw e;
                }
                Globals.Library.Db.Execute("ALTER TABLE Tracks ADD ItunesID INTEGER");
            } finally {
                Globals.Library.Db.Execute("UPDATE Tracks SET ItunesID = 0");
            }
        }
            
        private void ProcessSong(XmlNodeList keys)
        {
            string location = null;
            int itunes_id = 0;
            byte rating = 0;
            uint play_count = 0;
            DateTime last_played = new DateTime();
            bool match = false;

            foreach(XmlNode key in keys) {
                if(key.Name != "key") {
                    continue;
                }
                switch(key.InnerText) {
                case "Track ID":
                    itunes_id = int.Parse(key.NextSibling.InnerText);
                    break;
                case "Play Count":
                    play_count = uint.Parse(key.NextSibling.InnerText);
                    match = true;
                    break;
                case "Play Date UTC":
                    last_played = DateTime.Parse(key.NextSibling.InnerText);
                    match = true;
                    break;
                case "Rating":
                    rating = byte.Parse(key.NextSibling.InnerText);
                    match = true;
                    break;
                case "Location":
                    location = key.NextSibling.InnerText;
                    break;
                }
            }

            data.total_processed++;
            if(location == null) {
                return;
            }

            location = ConvertToLocalUriFormat(location);

            string local_uri = null;
            if(!data.local_library) {
                int index = location.IndexOf(data.default_query);
                if(index == -1 && data.query_dirs.Length > 0) {
                    int count = 0;
                    string path = data.query_dirs[data.query_dirs.Length - 1];
                    do {
                        for(int k = data.query_dirs.Length - 2; k >= count; k--) {
                            path = Path.Combine(path, data.query_dirs[k]);
                        }
                        index = location.IndexOf(path);
                        count++;
                    } while(index == -1 && count < data.query_dirs.Length);
                    if(index == -1) {
                        index = location.IndexOf(data.fallback_dir);
                        if(index != -1) {
                            index += data.fallback_dir.Length + 1;
                        }
                    }
                }
                if(index == -1) {
                    Banshee.Sources.ImportErrorsSource.Instance.AddError(location,
                        "Unable to map iTunes URI to local URI", null);
                    return;
                }
                local_uri = location.Substring(index, location.Length - index);
                local_uri = Path.Combine(data.local_prefix, local_uri);
            } else {
                local_uri = location.Substring(17); // 17 is the length of "file://localhost/"
            }

            SafeUri safe_uri = null;
            try {
                safe_uri = new SafeUri(local_uri);
            } catch {
                Banshee.Sources.ImportErrorsSource.Instance.AddError(local_uri,
                    "URI is not a local file path", null);
                return;
            }
            
            if(!IOProxy.File.Exists(safe_uri)) {
                Banshee.Sources.ImportErrorsSource.Instance.AddError(local_uri,
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
                Banshee.Sources.ImportErrorsSource.Instance.AddError(local_uri, e.Message, e);
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
            
        private void ProcessPlaylist(XmlNode node)
        {
            string name = "";
            bool skip = false;
            byte[] smart_info = null;
            byte[] smart_criteria = null;
            XmlNode array = null;

            foreach(XmlNode key in node.ChildNodes) {
                if(key.Name=="key") {
                    switch(key.InnerText) {
                    case "Name":
                       name = key.NextSibling.InnerText;
                       //if(name == "Music Videos")
                           //skip = true;
                       break;
                    case "Smart Info":
                       smart_info = Convert.FromBase64String(key.NextSibling.InnerText);
                       break;
                    case "Smart Criteria":
                        smart_criteria = Convert.FromBase64String(key.NextSibling.InnerText);
                        break;
                    default:
                        if (key.InnerText == "Audiobooks" ||
                            key.InnerText == "Music" ||
                            key.InnerText == "Movies" ||
                            key.InnerText == "Party Shuffle" ||
                            key.InnerText == "Podcasts" ||
                            key.InnerText == "Purchased Music" ||
                            key.InnerText == "TV Shows") {
                            skip = true;
                        }
                        break;
                    }
                    if(skip) {
                        break;
                    }
                } else if(key.Name == "array") {
                    array = key;
                }
            }
            if(skip || array == null) {
                return;
            } else if(data.get_playlists && smart_info == null) {
                user_event.Header = Catalog.GetString("Importing Playlist") + " " + name;
                ProcessRegularPlaylist(name, array);
            } else if(data.get_smart_playlists && smart_info != null && smart_criteria != null) {
                user_event.Header = Catalog.GetString("Importing Playlist") + " " + name;
                ProcessSmartPlaylist(name, smart_info, smart_criteria, array);
            }
        }

        private void ProcessRegularPlaylist(string name, XmlNode array)
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
            uint total_songs = (uint)array.ChildNodes.Count;
            uint processed_songs = 0;
            foreach(XmlNode dict in array.ChildNodes) {
               if(canceled) {
                    break;
                }
                user_event.Progress = (double)++processed_songs / (double)total_songs;
                int itunes_id = int.Parse(dict.ChildNodes[1].InnerText);
                int track_id;
                TrackInfo track;
                try {
                    track_id = (int)Globals.Library.Db.QuerySingle(String.Format(
                        "SELECT TrackID FROM Tracks WHERE ItunesID = {0}", itunes_id));
                    track = Globals.Library.GetTrack(track_id);
                } catch {
                   continue;
                }
                playlist_source.AddTrack(track);
                user_event.Message = String.Format("{0} - {1}", track.Artist, track.Title);
            }
            user_event.Message = "";
            user_event.Progress = 0;
            playlist_source.Commit();
            data.playlists_count++;
        }

        private void ProcessSmartPlaylist(string name, byte[] info, byte[] criteria, XmlNode array)
        {
            ItunesSmartPlaylist smart_playlist = new SmartPlaylistParser().Parse(info, criteria);
            smart_playlist.Name = name;
            if(!(smart_playlist.Query == "" && smart_playlist.Ignore != "") || smart_playlist.LimitNumber != 0) {
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
                    (smart_playlist.Query == "") ? null : " " + smart_playlist.Query,
                    smart_playlist.OrderBy,
                    smart_playlist.LimitNumber.ToString(),
                    smart_playlist.LimitMethod
                );
                if(!SourceManager.ContainsSource(smart_playlist_source) &&
                    SourceManager.ContainsSource(LibrarySource.Instance)) {
                    LibrarySource.Instance.AddChildSource(smart_playlist_source);
                }
            }
            if (smart_playlist.Ignore != "") {
                if (smart_playlist.Query != "") {
                    data.partial_smart_playlists.Add(smart_playlist);
                } else {
                    data.failed_smart_playlists.Add(smart_playlist);
                }
                ProcessRegularPlaylist(name, array);
            } else {
                data.smart_playlists_count++;
            }
        }
            
        private static string ConvertToLocalUriFormat(string input)
        {
            // URIs are UTF-8 percent-encoded. Deconding with System.Web.HttpServerUtility
            // involves too much overhead, so we do it cheap here.
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
