using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Banshee.Cdrom.Windows.Interop
{
	/// <summary>
	/// Wrapper for an IMAPI <c>IDiscRecorder</c> object for use in
	/// managed code.  This class allows information about the
	/// recorder and any media present to be obtained.  It also
	/// allows media to be erased and the disc drawer to be ejected.
	/// </summary>
	public class DiscRecorder : IDisposable
	{
		#region Unmanaged Code
		[DllImport("kernel32", CharSet=CharSet.Auto)]
		private extern static int QueryDosDevice(
			[MarshalAs(UnmanagedType.LPTStr)]
			string lpDeviceName,
			[MarshalAs(UnmanagedType.LPTStr)]
			string lpTargetPath,
			int ucchMax);
		#endregion

		private IDiscRecorder recorder = null;
		private bool disposed = false;
		private readonly string vendor;
		private readonly string product;
		private readonly string revision;

		/// <summary>
		/// Internal constructor; this class should only be constructed
		/// by an instance of the <see cref="DiscMaster"/> class.
		/// </summary>
		/// <param name="recorder">IMAPI recorder object to wrap</param>
		internal DiscRecorder(IDiscRecorder recorder)
		{
			this.recorder = recorder;
			recorder.GetDisplayNames(ref vendor, ref product, ref revision);			

		}

		/// <summary>
		/// Disposes any resources associated with this class.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Disposes any resources associated with this class.
		/// </summary>
		/// <param name="disposing"><c>true</c> if the method is being
		/// called from the <c>Dispose</c> method, otherwise <c>false</c>.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					if (recorder != null)
					{
						Marshal.ReleaseComObject(recorder);
					}
					recorder = null;
				}
			}
			this.disposed = true;
		}

		internal IDiscRecorder IDiscRecorder
		{
			get
			{
				return recorder;
			}
		}

		
		/// <summary>
		/// Gets the Plug and Play ID of this recorder. Typically this
		/// is a concatenation of the Vendor, Product and Revision
		/// information.
		/// </summary>
		public string PnPID
		{
			get
			{
				string pnpId;
				recorder.GetBasePnPID(out pnpId);
				return pnpId;
			}
		}

		/// <summary>
		/// Gets the product name of this recorder.
		/// </summary>
		public string Product
		{
			get
			{
				return product;				
			}
		}

		/// <summary>
		/// Gets the vendor name for this recorder.
		/// </summary>
		public string Vendor
		{
			get
			{
				return vendor;
			}
		}

		/// <summary>
		/// Gets the revision number of this recorder.
		/// </summary>
		public string Revision
		{
			get
			{
				return revision;
			}
		}

		/// <summary>
		/// Gets the OS path for this recorder.
		/// </summary>
		public string OsPath
		{
			get
			{
				string path;
				recorder.GetPath(out path);
				return path;
			}
		}

		/// <summary>
		/// Returns the drive letter for this recorder.
		/// </summary>
		public string DriveLetter
		{
			get
			{
				string driveLetter = "";
				string osPath = OsPath;
				foreach (string drive in Directory.GetLogicalDrives())
				{
					string driveTest = drive.Substring(0, drive.Length - 1);
					string deviceName = new String('\0', 260);
					int result = QueryDosDevice(driveTest, deviceName, 260);
					deviceName = deviceName.Substring(0, result - 2); // two trailing nulls
					if (deviceName.Equals(osPath))
					{
						driveLetter = drive;
						break;
					}
				}
				return driveLetter;
			}
		}

		/// <summary>
		/// Gets the type of this recorder.
		/// </summary>
		public RECORDER_TYPES RecorderType
		{
			get
			{
				int type = -1;
				recorder.GetRecorderType(out type);
				return (RECORDER_TYPES) type;
			}
		}

		/// <summary>
		/// Gets the current state of this recorder.
		/// </summary>
		public RECORDER_STATE RecorderState
		{
			get
			{
				int state = -1;
				recorder.GetRecorderState(out state);
				return (RECORDER_STATE) state;
			}
		}

		/// <summary>
		/// Erases CD-RW media.
		/// </summary>
		/// <param name="fullErase"><c>true</c> to fully erase the
		/// media, <c>false</c> otherwise.</param>
		public void EraseCDRW(bool fullErase)
		{
			recorder.Erase(fullErase ? 1 : 0);
		}

		/// <summary>
		/// Ejects the CD tray.
		/// </summary>
		public void Eject()
		{
			recorder.Eject();
		}

		/// <summary>
		/// Opens the recorder for exclusive access.  Required to determine
		/// the media contained within the recorder.
		/// </summary>
		public void OpenExclusive()
		{
			recorder.OpenExclusive();
		}

		/// <summary>
		/// Closes the recorder if previously opened using <see cref="OpenExclusive"/>.
		/// </summary>
		public void CloseExclusive()
		{
			recorder.Close();
		}

		/// <summary>
		/// Gets an object describing the media in the recorder.
		/// </summary>
		/// <returns>object describing media details</returns>
		public MediaDetails GetMediaDetails()
		{
			MediaDetails details = new MediaDetails(recorder);
			return details;
		}

		/// <summary>
		/// Gets the properties collection for this disc recorder.
		/// </summary>
		public DiscRecorderProperties Properties
		{
			get
			{
				IPropertyStorage storage = null;
				recorder.GetRecorderProperties(out storage);
				DiscRecorderProperties properties = new DiscRecorderProperties(storage);
				return properties;
			}
		}

	}
}
