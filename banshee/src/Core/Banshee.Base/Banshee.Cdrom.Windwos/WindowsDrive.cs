using System;
using MusicBrainz;

using Banshee.Base;
using Banshee.Cdrom;
using Banshee.Cdrom.Windows.Interop;

namespace Banshee.Cdrom.Windows
{
    class WindowsDrive : IDrive
	{
        public event MediaHandler MediaAdded;
        public event MediaHandler MediaRemoved;

        public event EventHandler Updated;
        
        private CDDrive drive;
        private char drive_letter;

        internal WindowsDrive(char c)
        {
            drive_letter = c;
            drive = new CDDrive();
            drive.CDInserted += new EventHandler(drive_CDInserted);
            drive.CDRemoved += new EventHandler(drive_CDRemoved);
            drive.Open(drive_letter);

            CheckForAudioCdDisk();
        }

        public bool CheckForAudioCdDisk()
        {
            if(drive.IsCDReady() && drive.Refresh()) {
                int tracks = drive.GetNumTracks();
                for (int i = 1; i <= tracks; i++) {
                    if(drive.IsAudioTrack(i)) {
                        return true;
                    }
                }
            }
            return false;
        }

        private void drive_CDInserted(object sender, EventArgs e)
        {
            if(MediaAdded != null) {
                MediaAdded(this, new MediaArgs(this, true));
            }
        }

        private void drive_CDRemoved(object sender, EventArgs e)
        {
            if (MediaRemoved != null) {
                MediaRemoved(this, new MediaArgs(this, false));
            }
        }

        public string Name
        {
            get { return drive_letter.ToString(); }
        }

        public string Device
        {
            get { return drive_letter.ToString() + @":\"; }
        }

        public bool HaveMedia
        {
            get { return drive.IsCDReady(); }
        }

        public int MaxReadSpeed
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public int MaxWriteSpeed
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public int MinWriteSpeed
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public long MediaCapacity
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public CDDrive Drive
        {
            get { return drive; }
        }
    }
}
