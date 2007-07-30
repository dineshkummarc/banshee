using System;

using Banshee.Base;
using Banshee.Cdrom;
using Banshee.Cdrom.Windows.Interop;

namespace Banshee.Cdrom.Windows
{
	class WindowsRecorder : IRecorder
	{
        public event ActionChangedHandler ActionChanged;
        public event ProgressChangedHandler ProgressChanged;
        public event InsertMediaRequestHandler InsertMediaRequest;
        public event EventHandler WarnDataLoss;

        public event MediaHandler MediaAdded;
        public event MediaHandler MediaRemoved;

        public void ClearTracks()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void AddTrack(RecorderTrack track)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void RemoveTrack(RecorderTrack track)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public RecorderResult WriteTracks(int speed, bool eject)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool CancelWrite(bool skipIfDangerous)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool IsWriting
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public string Name
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public string Device
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool HaveMedia
        {
            get { throw new Exception("The method or operation is not implemented."); }
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
    }
}
