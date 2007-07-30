using System;
using System.Collections.Generic;
using System.Threading;
using MusicBrainz;

using Banshee.Widgets;
using Banshee.AudioProfiles;
using Mono.Unix;

namespace Banshee.Base
{
    public enum AudioCdLookupStatus
    {
        ReadingDisk,
        SearchingMetadata,
        SearchingCoverArt,
        Success,
        ErrorNoConnection,
        ErrorLookup
    }
    
    public abstract class AudioCdDisk
	{
        public event EventHandler Updated;
        
        protected abstract bool DoEject(bool open);
        protected abstract bool DoLockDrive();
        protected abstract bool DoUnlockDrive();

        protected readonly string drive_name;
        protected readonly string device_node;
        protected readonly string udi;
        
        protected string title;
        protected string album_title;
        protected bool is_ripping;
        protected List<TrackInfo> tracks = new List<TrackInfo>();
        protected volatile AudioCdLookupStatus status;
        protected volatile bool mb_querying = false;
        protected volatile bool mb_queried = false;
        
        private object lock_mutex = new object();

        protected AudioCdDisk(string udi, string device_node, string drive_name)
        {
            this.udi = udi;
            this.device_node = device_node;
            this.drive_name = drive_name;

            Globals.Network.StateChanged += OnNetworkStateChanged;

            Status = AudioCdLookupStatus.ReadingDisk;
            LoadDiskInfo();
        }

        protected virtual void LoadDiskInfo()
        {
            tracks.Clear();
            SimpleDisc mb_disc = new SimpleDisc(device_node);
            //mb_disc.Client.Debug = true;

            foreach(SimpleTrack mb_track in mb_disc) {
                AudioCdTrackInfo track = new AudioCdTrackInfo(this);
                track.Duration = new TimeSpan(mb_track.Length * TimeSpan.TicksPerSecond);
                track.TrackIndex = mb_track.Index;
                track.Artist = Catalog.GetString("Unknown Artist");
                track.Album = Catalog.GetString("Unknown Album");
                track.Title = String.Format(Catalog.GetString("Track {0}"), mb_track.Index);

                tracks.Add(track);
            }

            album_title = Catalog.GetString("Audio CD");

            QueryMetadata(mb_disc);
        }
        
        public bool Eject()
        {
            return Eject(true);
        }

        public bool Eject(bool open)
        {
            if(IsRipping) {
                LogCore.Instance.PushWarning(Catalog.GetString("Cannot Eject CD"),
                    Catalog.GetString("The CD cannot be ejected while it is importing. Stop the import first."));
                return false;
            }

            AudioCdTrackInfo track = PlayerEngineCore.CurrentTrack as AudioCdTrackInfo;
            if(track != null && track.DeviceNode == DeviceNode) {
                PlayerEngineCore.Close();
            }

            return DoEject(open);
        }

        public void LockDrive()
        {
            lock(lock_mutex) {
                if(!DoLockDrive()) {
                    LogCore.Instance.PushWarning("Could not lock CD-ROM drive", device_node, false);
                }
            }
        }

        public void UnlockDrive()
        {
            lock(lock_mutex) {
                if(!DoUnlockDrive()) {
                    LogCore.Instance.PushWarning("Could not unlock CD-ROM drive", device_node, false);
                }
            }
        }

        public virtual void QueryMetadata()
        {
            QueryMetadata(null);
        }

        protected virtual void QueryMetadata(SimpleDisc disc)
        {
            ThreadPool.QueueUserWorkItem(QueryMusicBrainz, disc);
        }

        protected virtual void QueryMusicBrainz(object o)
        {
            if(mb_querying) {
                return;
            }

            mb_querying = true;

            if(!Globals.Network.Connected) {
                Status = AudioCdLookupStatus.ErrorNoConnection;
                mb_querying = false;
                return;
            }

            Status = AudioCdLookupStatus.SearchingMetadata;

            SimpleDisc mb_disc;

            if(o == null) {
                mb_disc = new SimpleDisc(device_node);
            } else {
                mb_disc = o as SimpleDisc;
            }

            try {
                mb_disc.QueryCDMetadata();
            } catch {
                Status = AudioCdLookupStatus.ErrorLookup;
                mb_querying = false;
                return;
            }

            int min = tracks.Count < mb_disc.Tracks.Length
                ? tracks.Count : mb_disc.Tracks.Length;

            if(mb_disc.AlbumName != null) {
                album_title = mb_disc.AlbumName;
            }

            for(int i = 0; i < min; i++) {
                tracks[i].Duration = new TimeSpan(mb_disc[i].Length * TimeSpan.TicksPerSecond);
                (tracks[i] as AudioCdTrackInfo).TrackIndex = mb_disc[i].Index;

                if(mb_disc[i].Artist != null) {
                    tracks[i].Artist = mb_disc[i].Artist;
                }

                if(mb_disc.AlbumName != null) {
                    tracks[i].Album = mb_disc.AlbumName;
                }

                if(mb_disc[i].Title != null) {
                    tracks[i].Title = mb_disc[i].Title;
                }

                tracks[i].Asin = mb_disc.AmazonAsin;
                tracks[i].RemoteLookupStatus = RemoteLookupStatus.Success;
            }

            string asin = mb_disc.AmazonAsin;

            if(asin == null || asin == String.Empty) {
                HandleUpdated();

                // sometimes ASINs aren't associated with a CD disc ID, but they are associated
                // with file track metadata. If no ASIN was returned for the CD lookup, use the
                // first track on the CD to attempt a file lookup
                try {
                    SimpleTrack mb_track = SimpleQuery.FileLookup(mb_disc.Client,
                        tracks[0].Artist, tracks[0].Album, tracks[0].Title, 0, 0);
                    asin = mb_track.Asin;
                    for(int i = 0; i < min; i++) {
                        tracks[i].Asin = asin;
                    }
                } catch {
                }
            }

            mb_queried = true;
            mb_disc.Dispose();
            HandleUpdated();

            string path = Paths.GetCoverArtPath(asin);
            if(System.IO.File.Exists(path)) {
                Status = AudioCdLookupStatus.Success;
                mb_querying = false;
                return;
            }

            Status = AudioCdLookupStatus.SearchingCoverArt;

            Banshee.Metadata.MusicBrainz.MusicBrainzQueryJob cover_art_job =
                new Banshee.Metadata.MusicBrainz.MusicBrainzQueryJob(tracks[0],
                    Banshee.Metadata.MetadataService.Instance.Settings, asin);
            if(cover_art_job.Lookup()) {
                HandleUpdated();
            }

            Status = AudioCdLookupStatus.Success;
            mb_querying = false;
        }

        protected virtual void HandleUpdated()
        {
            ThreadAssist.ProxyToMain(delegate {
                EventHandler handler = Updated;
                if(handler != null) {
                    handler(this, new EventArgs());
                }
            });
        }

        protected virtual void OnNetworkStateChanged(object o, NetworkStateChangedArgs args)
        {
            if(!mb_queried && args.Connected) {
                QueryMetadata();
            }
        }

        public AudioCdLookupStatus Status
        {
            get { return status; }

            private set
            {
                status = value;
                HandleUpdated();
            }
        }
        
        public string Udi
        {
            get { return udi; }
        }

        public string DeviceNode
        {
            get { return device_node; }
        }

        public string DriveName
        {
            get { return drive_name; }
        }

        public bool IsRipping
        {
            get { return is_ripping; }
            set { is_ripping = value; }
        }

        public string Title
        {
            get { return album_title; }
        }

        public int TrackCount
        {
            get { return tracks.Count; }
        }

        public IEnumerable<TrackInfo> Tracks
        {
            get { return tracks; }
        }

        public abstract bool Valid { get; }
	}
}
