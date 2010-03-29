//
// PaasTrackInfo.cs
//
// Authors:
//   Gabriel Burt <gburt@novell.com>
//   Mike Urbanski <michael.c.urbanski@gmail.com>
//
// Copyright (C) 2009 Michael C. Urbanski
// Copyright (C) 2008 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;

using Banshee.Collection;
using Banshee.Collection.Database;

namespace Banshee.Paas.Data
{
    public class PaasTrackInfo
    {
        private static readonly object sync = new object ();

        public static PaasTrackInfo From (TrackInfo track)
        {
            if (track != null) {
                PaasTrackInfo pi = track.ExternalObject as PaasTrackInfo;
                PaasItem item = (pi != null) ? pi.Item : null;

                if (pi != null && item != null) {
                    track.ReleaseDate = item.PubDate;
                }

                return pi;
            }

            return null;
        }

        public static IEnumerable<PaasTrackInfo> From (IEnumerable<TrackInfo> tracks)
        {
            lock (sync) {
                foreach (TrackInfo track in tracks) {
                    PaasTrackInfo pi = From (track);

                    if (pi != null) {
                        yield return pi;
                    }
                }
            }
        }

        private int position;
        private DatabaseTrackInfo track;

        private PaasItem item;

        public DatabaseTrackInfo Track {
            get { return track; }
        }

        public PaasChannel Channel {
            get { return Item.Channel; }
        }

        public PaasItem Item {
            get {
                if (item == null && track.ExternalId > 0) {
                    item = PaasItem.Provider.FetchSingle (track.ExternalId);
                }

                return item ?? PaasItem.Empty;
            }

            set {
                item = value;
                track.ExternalId = value.DbId;
            }
        }

        public DateTime PubDate {
            get { return Item.PubDate; }
        }

        public string Description {
            get { return Item.StrippedDescription; }
        }

        public bool IsNew {
            get { return IsDownloaded && Item.IsNew; }
        }

        public bool IsDownloaded {
            get { return !String.IsNullOrEmpty (Item.LocalPath); }
        }

        public int Position {
            get { return position; }
            set { position = value; }
        }

        public DateTime ReleaseDate {
            get { return Item.PubDate; }
        }

        public PaasTrackInfo (DatabaseTrackInfo track) : base ()
        {
            this.track = track;
        }

        public PaasTrackInfo (DatabaseTrackInfo track, PaasItem item) : this (track)
        {
            Item = item;
            SyncWithItem ();
        }

        static PaasTrackInfo ()
        {
            TrackInfo.PlaybackFinished += OnPlaybackFinished;
        }

        private static void OnPlaybackFinished (TrackInfo track, double percentCompleted)
        {
            if (percentCompleted > 0.5 && track.PlayCount > 0) {
                PaasTrackInfo pi = PaasTrackInfo.From (track);
                if (pi != null && pi.Item != PaasItem.Empty && pi.Item.IsNew) {
                    pi.Item.IsNew = false;
                    pi.Item.Save ();
                }
            }
        }

        public void SyncWithItem ()
        {
            PaasItem item = Item;

            if (item == null || item == PaasItem.Empty || item.Channel == PaasChannel.Empty) {
                return;
            }

            if (track.ExternalId != item.DbId) {
                throw new Exception (String.Format (
                    "PLEASE REPORT!  Track.TrackID:  {0} - Track.ExternalID:  {1} - Track.CacheEntryID:  {2} - Item.DbId = {3} - Item.CacheEntryID = {4}",
                    track.TrackId, track.ExternalId, track.CacheEntryId, item.DbId, item.CacheEntryId
                ));
            }

            track.ArtistName = item.Channel.Publisher;
            track.AlbumTitle = item.Channel.Name;
            track.TrackTitle = item.Name;
            track.Year = item.PubDate.Year;
            track.CanPlay = true;
            track.Genre = track.Genre ?? "Podcast";
            track.ReleaseDate = item.PubDate;
            track.MimeType = item.MimeType;
            track.Duration = item.Duration;
            track.FileSize = item.Size;
            track.LicenseUri = item.Channel.License;

            track.Uri = new Banshee.Base.SafeUri (item.LocalPath ?? item.Url);

            if (!String.IsNullOrEmpty (item.LocalPath)) {
                try {
                    TagLib.File file = Banshee.Streaming.StreamTagger.ProcessUri (track.Uri);
                    Banshee.Streaming.StreamTagger.TrackInfoMerge (track, file, true);
                } catch {}
            }

            track.MediaAttributes |= TrackMediaAttributes.Podcast;
        }
    }
}
