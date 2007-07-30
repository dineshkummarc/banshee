/***************************************************************************
 *  IDriveFactory.cs
 *
 *  Copyright (C) 2006 Novell, Inc.
 *  Written by Aaron Bockover <aaron@abock.org>
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
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
using System.Collections;
using System.Collections.Generic;

using Banshee.Base;
using Banshee.Sources;

namespace Banshee.Cdrom
{
    public delegate void DriveHandler(object o, DriveArgs args);
    public delegate void MediaHandler(object o, MediaArgs args);

    public class DriveArgs : EventArgs
    {
        private IDrive recorder;

        public DriveArgs(IDrive recorder)
        {
            this.recorder = recorder;
        }

        public IDrive Drive
        {
            get { return recorder; }
        }
    }

    public sealed class MediaArgs : DriveArgs
    {
        private bool available;

        public MediaArgs(IDrive recorder, bool available)
            : base(recorder)
        {
            this.available = available;
        }

        public bool Available
        {
            get { return available; }
        }
    }
    
    public delegate void AudioCdDiskAddedHandler(object o, AudioCdDiskAddedArgs args);
    public delegate void AudioCdDiskRemovedHandler(object o, AudioCdDiskRemovedArgs args);

    public class AudioCdDiskRemovedArgs : EventArgs
    {
        public string Udi;
    }

    public class AudioCdDiskAddedArgs : EventArgs
    {
        public AudioCdDisk Disk;
    }
    
    public abstract class DriveFactory : IEnumerable<IDrive>
    {
        public event DriveHandler DriveAdded;
        public event DriveHandler DriveRemoved;
        public event MediaHandler MediaAdded;
        public event MediaHandler MediaRemoved;
        public event EventHandler Updated;
        public event AudioCdDiskAddedHandler AudioCdDiskAdded;
        public event AudioCdDiskRemovedHandler AudioCdDiskRemoved;

        protected Dictionary<string, IDrive> drives;
        protected Dictionary<string, AudioCdDisk> disks = new Dictionary<string, AudioCdDisk>();

        protected DriveFactory()
        {
        }
        
        protected virtual void OnAudioCdDiskUpdated(object o, EventArgs args)
        {
            HandleUpdated();
        }
        
        protected virtual void HandleUpdated()
        {
            EventHandler handler = Updated;
            if(handler != null) {
                handler(this, new EventArgs());
            }
        }

        protected virtual void OnDriveAdded(IDrive drive)
        {
            DriveHandler handler = DriveAdded;
            if(handler != null) {
                handler(this, new DriveArgs(drive));
            }
        }

        protected virtual void OnDriveRemoved(IDrive drive)
        {
            DriveHandler handler = DriveRemoved;
            if(handler != null) {
                handler(this, new DriveArgs(drive));
            }
        }

        protected virtual void OnMediaAdded(object o, MediaArgs args)
        {
            MediaHandler handler = MediaAdded;
            if(handler != null) {
                handler(o, args);
            }
        }

        protected virtual void OnMediaRemoved(object o, MediaArgs args)
        {
            MediaHandler handler = MediaRemoved;
            if(handler != null) {
                handler(o, args);
            }
        }

        protected virtual void OnAudioCdDiskAdded(object o, AudioCdDisk disk)
        {
            AudioCdDiskAddedHandler handler = AudioCdDiskAdded;
            if(handler != null) {
                AudioCdDiskAddedArgs args = new AudioCdDiskAddedArgs();
                args.Disk = disk;
                handler(o, args);
            }

            SourceManager.AddSource(new AudioCdSource(disk));
        }

        protected virtual void OnAudioCdDiskRemoved(object o, string udi)
        {
            AudioCdDiskRemovedHandler handler = AudioCdDiskRemoved;
            if(handler != null) {
                AudioCdDiskRemovedArgs args = new AudioCdDiskRemovedArgs();
                args.Udi = udi;
                handler(o, args);
            }

            foreach(Source source in SourceManager.Sources) {
                AudioCdSource audio_cd_source = source as AudioCdSource;
                if(audio_cd_source != null && audio_cd_source.Disk.Udi == udi) {
                    SourceManager.RemoveSource(source);
                    break;
                }
            }
        }

        public virtual IEnumerator<IDrive> GetEnumerator()
        {
            return drives.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return drives.Values.GetEnumerator();
        }

        public virtual ICollection<AudioCdDisk> Disks
        {
            get { return disks.Values; }
        }

        public virtual int DriveCount
        {
            get { return drives.Count; }
        }

        public int RecorderCount
        {
            get {
                int count = 0;

                foreach(IDrive drive in drives.Values) {
                    if(drive is IRecorder) {
                        count++;
                    }
                }

                return count;
            }
        }
    }
}
