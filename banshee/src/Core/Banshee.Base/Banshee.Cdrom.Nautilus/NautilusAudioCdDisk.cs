/***************************************************************************
 *  NautilusAudioCdDisk.cs
 *
 *  Copyright (C) 2005-2006 Novell, Inc.
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
using System.Runtime.InteropServices;
using System.Threading;
using Mono.Unix;

namespace Banshee.Base
{
    public class NautilusAudioCdDisk : AudioCdDisk
    {
        [DllImport("libc")]
        private static extern int ioctl(int device, EjectOperation request);

        [DllImport("libc")]
        private static extern int ioctl(int device, IoctlOperation request, bool lockdoor);

        public NautilusAudioCdDisk(string udi, string deviceNode, string driveName)
            : base(udi, deviceNode, driveName)
        {
        }

        private enum EjectOperation
        {
            Open = 0x5309,
            Close = 0x5319
        }

        protected override bool DoEject(bool open)
        {
            try {
                Hal.Device device = new Hal.Device(udi);
                if (device.GetPropertyBoolean("volume.is_mounted")) {
                    if (!Utilities.UnmountVolume(device_node)) {
                        return false;
                    }
                }

                using (UnixStream stream = (new UnixFileInfo(device_node)).Open(
                    Mono.Unix.Native.OpenFlags.O_RDONLY |
                    Mono.Unix.Native.OpenFlags.O_NONBLOCK)) {
                    return ioctl(stream.Handle, open
                        ? EjectOperation.Open
                        : EjectOperation.Close) == 0;
                }
            } catch {
                return false;
            }
        }

        protected override bool DoLockDrive()
        {
            return LockDrive(device_node, true);
        }

        protected override bool DoUnlockDrive()
        {
            return LockDrive(device_node, false);
        }

        private enum IoctlOperation
        {
            LockDoor = 0x5329
        }

        private static bool LockDrive(string device, bool lockdoor)
        {
            using(UnixStream stream = (new UnixFileInfo(device)).Open(
                Mono.Unix.Native.OpenFlags.O_RDONLY |
                Mono.Unix.Native.OpenFlags.O_NONBLOCK)) {
                return ioctl(stream.Handle, IoctlOperation.LockDoor, lockdoor) == 0;
            }
        }

        public override bool Valid
        {
            get { return true; }
        }
    }
}