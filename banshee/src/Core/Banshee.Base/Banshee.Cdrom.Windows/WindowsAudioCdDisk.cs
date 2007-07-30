using System;
using System.Collections.Generic;

using Banshee.Cdrom.Windows;
using Banshee.Cdrom.Windows.Interop;

namespace Banshee.Base
{
	class WindowsAudioCdDisk : AudioCdDisk
	{
        private WindowsDrive drive;

        public WindowsAudioCdDisk(WindowsDrive drive)
            : base (drive.Device, drive.Device, drive.Name)
        {
            this.drive = drive;
        }

        protected override bool DoEject(bool open)
        {
            if(open) {
                return drive.Drive.EjectCD();
            } else {
                if(!drive.Drive.IsOpened) {
                    return false;
                } else {
                    drive.Drive.Close();
                    return true;
                }
            }
        }

        protected override bool DoLockDrive()
        {
            return drive.Drive.LockCD();
        }

        protected override bool DoUnlockDrive()
        {
            return drive.Drive.UnLockCD();
        }

        public override bool Valid
        {
            get { return true; }
        }
    }
}
