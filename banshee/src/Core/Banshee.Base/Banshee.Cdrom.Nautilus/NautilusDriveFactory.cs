/***************************************************************************
 *  NautilusDriveFactory.cs
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

using Hal;

using Banshee.Base;
using Banshee.Cdrom;
using Banshee.Cdrom.Nautilus.Interop;
using Banshee.Sources;
using Mono.Unix;

namespace Banshee.Cdrom.Nautilus
{
    public class NautilusDriveFactory : DriveFactory
    {
        private class DiskInfo
        {
            public string Udi;
            public string DeviceNode;
            public string VolumeName;

            public DiskInfo(string udi, string deviceNode, string volumeName)
            {
                Udi = udi;
                DeviceNode = deviceNode;
                VolumeName = volumeName;
            }
        }
        
        public NautilusDriveFactory()
        {
            if(HalCore.Manager == null) {
                throw new ApplicationException("HAL Core is not available");
            }

            foreach(string udi in HalCore.Manager.FindDeviceByStringMatch("storage.drive_type", "cdrom")) {
                AddDrive(new Device(udi));
            }
        
            HalCore.Manager.DeviceAdded += OnHalDeviceAdded;
            HalCore.Manager.DeviceRemoved += OnHalDeviceRemoved;

            BuildInitialList();
        }

        private void BuildInitialList()
        {
            foreach(DiskInfo hal_disk in GetHalDisks()) {
                AudioCdDisk disk = CreateDisk(hal_disk);
                if(disk != null) {
                    OnAudioCdDiskAdded(this, disk);
                }
            }

            HandleUpdated();
        }

        private IList<DiskInfo> GetHalDisks()
        {
            List<DiskInfo> list = new List<DiskInfo>();

            foreach (string udi in HalCore.Manager.FindDeviceByStringMatch("storage.drive_type", "cdrom")) {
                try {
                    DiskInfo disk = CreateHalDisk(new Device(udi));
                    if (disk != null) {
                        list.Add(disk);
                    }
                }
                catch {
                }
            }

            return list;
        }

        private DiskInfo CreateHalDisk(Device device)
        {
            string[] volumes = HalCore.Manager.FindDeviceByStringMatch("info.parent", device.Udi);

            if (volumes == null || volumes.Length < 1) {
                return null;
            }

            Device volume = new Device(volumes[0]);

            if (!volume.GetPropertyBoolean("volume.disc.has_audio")) {
                return null;
            }

            return new DiskInfo(volume.Udi, volume["block.device"] as string,
                volume["info.product"] as string);
        }

        private NautilusAudioCdDisk CreateDisk(DiskInfo hal_disk)
        {
            try {
                NautilusAudioCdDisk disk = new NautilusAudioCdDisk(hal_disk.Udi, hal_disk.DeviceNode, hal_disk.VolumeName);
                disk.Updated += OnAudioCdDiskUpdated;
                if (disk.Valid && !disks.ContainsKey(disk.Udi)) {
                    disks.Add(disk.Udi, disk);
                }
                return disk;
            }
            catch (Exception e) {
                Exception temp_e = e; // work around mcs #76642
                LogCore.Instance.PushError(Catalog.GetString("Could not Read Audio CD"),
                    temp_e.Message);
            }

            return null;
        }

        
        
        private void OnHalDeviceAdded(object o, DeviceAddedArgs args)
        {
            if(args.Device["storage.drive_type"] == "cdrom") {
                NautilusDrive drive = AddDrive(args.Device);
                if(drive != null) {
                    OnDriveAdded(drive);
                }
            }

            string udi = args.Udi;

            if (udi == null || disks.ContainsKey(udi)) {
                return;
            }

            foreach (DiskInfo hal_disk in GetHalDisks()) {
                if (hal_disk.Udi != udi) {
                    continue;
                }

                NautilusAudioCdDisk disk = CreateDisk(hal_disk);
                if (disk == null) {
                    continue;
                }

                OnAudioCdDiskAdded(this, disk);

                HandleUpdated();

                break;
            }
        }
        
        private void OnHalDeviceRemoved(object o, DeviceRemovedArgs args)
        {
            if(drives.ContainsKey(args.Udi)) {
                IDrive drive = drives[args.Udi];
                drive.MediaAdded -= OnMediaAdded;
                drive.MediaRemoved -= OnMediaRemoved;
                drives.Remove(args.Udi);
                OnDriveRemoved(drive);
            }

            string udi = args.Udi;

            if (udi == null) {
                return;
            }

            if (disks.ContainsKey(udi)) {
                disks.Remove(udi);
            }

            OnAudioCdDiskRemoved(this, udi);

            HandleUpdated();
        }
        
        private NautilusDrive AddDrive(Device device)
        {
            if(drives.ContainsKey(device.Udi)) {
                return drives[device.Udi] as NautilusDrive;
            }
            
            BurnDrive nautilus_drive = FindDriveByDeviceNode(device["block.device"]);
            if(nautilus_drive == null) {
                return null;
            }
            
            NautilusDrive drive = device.GetPropertyBoolean("storage.cdrom.cdr") ?
                new NautilusRecorder(device, nautilus_drive) :
                new NautilusDrive(device, nautilus_drive);
            
            drive.MediaAdded += OnMediaAdded;
            drive.MediaRemoved += OnMediaRemoved;
            drives.Add(device.Udi, drive);
            return drive;
        }
        
        private BurnDrive FindDriveByDeviceNode(string deviceNode)
        {
            return new BurnDrive(deviceNode);
        }
    }
}
