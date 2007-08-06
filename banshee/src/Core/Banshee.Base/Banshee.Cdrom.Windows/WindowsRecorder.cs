using System;
using System.IO;

using Banshee.Base;
using Banshee.Cdrom;
using Banshee.Cdrom.Windows.Interop;
using System.Collections.Generic;

namespace Banshee.Cdrom.Windows
{
	class WindowsRecorder : WindowsDrive, IRecorder
	{
        public event ActionChangedHandler ActionChanged;
        public event ProgressChangedHandler ProgressChanged;
        public event InsertMediaRequestHandler InsertMediaRequest;
        public event EventHandler WarnDataLoss;

        // Some of this is duplicate code from NautilusRecorder. Is there anyway to architect
        // around that w/o multiple inheritence?

        private List<RecorderTrack> tracks = new List<RecorderTrack>();
        private object burner_mutex = new object();
        private DiscMaster disc_master;
        private volatile bool is_writing, cancel;
        private int max_write_speed, min_write_speed;
        
        internal WindowsRecorder(char c) : base (c)
        {
            DiscRecorderClosure(delegate(DiscRecorder disc_recorder) {
                foreach(Property property in disc_recorder.Properties) {
                    if(property.Name == "WriteSpeed") {
                        min_write_speed = (int)property.Value;
                    } else if(property.Name == "MaxWriteSpeed") {
                        max_write_speed = (int)property.Value;
                    }
                }
            });
        }

        // The COM objects MUST be disposed. To make sure of that, we only instantiate them for
        // just as long as we need them. It makes things a little more cumbersome, but it's safer.
        private delegate T DiscRecorderDelegate<T>(DiscRecorder disc_recorder);
        private delegate void DiscRecorderDelegate(DiscRecorder disc_recorder);
        private void DiscRecorderClosure(DiscRecorderDelegate del)
        {
            DiscRecorderClosure<bool>(delegate(DiscRecorder disc_recorder) {
                del(disc_recorder);
                return true;
            });
        }
        private T DiscRecorderClosure<T>(DiscRecorderDelegate<T> del) where T : new()
        {
            T result = new T();
            using(disc_master = new DiscMaster()) {
                foreach(DiscRecorder disc_recorder in disc_master.DiscRecorders) {
                    if(disc_recorder.DriveLetter[0] == DriveLetter) {
                        result = del(disc_recorder);
                        break;
                    }
                }
            }
            disc_master = null;
            return result;
        }

        public void ClearTracks()
        {
            tracks.Clear();
        }

        public void AddTrack(RecorderTrack track)
        {
            tracks.Add(track);
        }

        public void RemoveTrack(RecorderTrack track)
        {
            tracks.Remove(track);
        }

        public RecorderResult WriteTracks(int speed, bool eject)
        {
            try {
                return WriteTracks(eject);
            } finally {
                is_writing = false;
            }
        }
        private RecorderResult WriteTracks(bool eject)
        {
            lock(burner_mutex) {
                cancel = false;
                // FIXME handle recorders that don't do audio CDs
                return DiscRecorderClosure<RecorderResult>(delegate(DiscRecorder disc_recorder) {
                    disc_master.DiscRecorders.ActiveDiscRecorder = disc_recorder;
                    using(RedbookDiscMaster redbook = disc_master.RedbookDiscMaster()) {
                        OnActionChanged(RecorderAction.PreparingWrite);
                        foreach(RecorderTrack track in tracks) {
                            if(track.Type == RecorderTrackType.Audio && !cancel) {
                                redbook.AddAudioTrackFromStream(new FileStream(track.FileName, FileMode.Open));
                            }
                        }

                        bool simulate = Environment.GetEnvironmentVariable("BURNER_SIMULATE") != null;
                        if(simulate) {
                            Console.Error.WriteLine("** Simulating CD Record **");
                        }

                        disc_master.BlockProgress += disc_master_BlockProgress;
                        disc_master.ClosingDisc += delegate {
                            OnActionChanged(RecorderAction.Fixating);
                            OnProgressChanged(0.0);
                        };

                        is_writing = true;
                        if(cancel) {
                            return RecorderResult.Canceled;
                        }
                        OnActionChanged(RecorderAction.Writing);
                        disc_master.RecordDisc(simulate, eject);
                        return RecorderResult.Finished;
                    }
                });
            }
        }

        void disc_master_BlockProgress(object sender, ProgressEventArgs args)
        {
            OnProgressChanged((double)(args.Completed) / (double)(args.Total));
        }

        private void OnProgressChanged(double fraction)
        {
            if(ProgressChanged != null) {
                ProgressChanged(this, new ProgressChangedArgs(fraction));
            }
        }

        private void OnActionChanged(RecorderAction action)
        {
            if(ActionChanged != null) {
                ActionChanged(this, new ActionChangedArgs(action, DriveMediaType.CD));
            }
        }

        public bool CancelWrite(bool skipIfDangerous)
        {
            if(!is_writing) {
                cancel = true;
                return true;
            }
            return false;
        }

        public bool IsWriting
        {
            get { return is_writing; }
        }

        public override int MaxWriteSpeed
        {
            get { return max_write_speed; }
        }

        public override int MinWriteSpeed
        {
            get { return min_write_speed; }
        }

        public override long MediaCapacity
        {
            get {
                return DiscRecorderClosure<long>(delegate(DiscRecorder disc_recorder) {
                    try {
                        disc_recorder.OpenExclusive();
                        MediaDetails media = disc_recorder.GetMediaDetails();
                        long result = 0;
                        if (media.MediaPresent) {
                            result = media.FreeBlocks * 2352;
                        }
                        disc_recorder.CloseExclusive();
                        return result;
                    } catch {
                        return 0;
                    }
                });
            }
        }
    }
}
