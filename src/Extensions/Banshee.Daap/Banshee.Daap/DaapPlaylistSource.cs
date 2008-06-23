// 
// DaapPlaylistSource.cs
//
// Author:
//   Alexander Hixon <hixon.alexander@mediati.org>
//
// Copyright (C) 2008 Alexander Hixon
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using Mono.Unix;

using Hyena;
using Hyena.Data.Sqlite;

using Banshee.ServiceStack;
using Banshee.Collection;
using Banshee.Collection.Database;
using Banshee.Playlist;
using Banshee.Sources;

using DAAP = Daap;

namespace Banshee.Daap
{
    public class DaapPlaylistSource : PlaylistSource
    {
        private HyenaSqliteCommand insert_track_command = new HyenaSqliteCommand (@"
            INSERT INTO CorePlaylistEntries (PlaylistID, EntryID) 
                SELECT ?, TrackID FROM CoreTracks WHERE PrimarySourceID = ? AND ExternalID IN (?)"
        );
        
        private DaapSource parent;
        public DAAP.Database Database {
            get { return parent.Database; }
        }
        
        public DaapPlaylistSource (DAAP.Playlist playlist, int id, DaapSource parent) : base (playlist.Name, parent.DbId)
        {
            this.parent = parent;
            Save ();
            
            if (playlist.Tracks.Count > 0) {
                //IList<DAAP.Track> tracks = playlist.Tracks;
                int [] external_ids = new int [playlist.Tracks.Count];
                //for (int i = 0; i < tracks.Count; i++) {
                int i = 0;
                foreach (DAAP.Track track in playlist.Tracks) {
                    external_ids[i] = track.Id;
                    i++;
                }
                HyenaSqliteCommand.LogAll = true;
                ServiceManager.DbConnection.Execute (insert_track_command, DbId, parent.DbId, external_ids);
                HyenaSqliteCommand.LogAll = false;
            }
            SavedCount = playlist.Tracks.Count;
            OnUpdated ();
        }
    }
}
