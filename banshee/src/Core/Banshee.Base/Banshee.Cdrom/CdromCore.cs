using System;

namespace Banshee.Cdrom
{
	class CdromCore
	{
        private static DriveFactory drive_factory;
        private static IDiscDuplicator disc_duplicator;

        public static DriveFactory DriveFactory {
            get {
                if(drive_factory == null) {
                    drive_factory = Environment.OSVersion.Platform == PlatformID.Unix
                        ? (DriveFactory) new Nautilus.NautilusDriveFactory()
                        : new Windows.WindowsDriveFactory();
                }
                return drive_factory;
            }
        }

        public static IDiscDuplicator DiscDuplicator {
            get
            {
                if (disc_duplicator == null)
                {
                    disc_duplicator = Environment.OSVersion.Platform == PlatformID.Unix
                        ? (IDiscDuplicator) new Nautilus.NautilusDiscDuplicator()
                        : new Windows.WindowsDiscDuplicator();
                }
                return disc_duplicator;
            }
        }
	}
}
