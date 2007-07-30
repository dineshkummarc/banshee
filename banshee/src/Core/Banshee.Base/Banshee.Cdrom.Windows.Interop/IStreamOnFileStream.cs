using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Banshee.Cdrom.Windows.Interop
{
	/// <summary>
	/// Wrapper around a managed FileStream to convert it into an IStream.
	/// </summary>
	[ComVisible(true)]
	internal class IStreamOnFileStream : IStreamOnStream
	{
		public IStreamOnFileStream(FileStream stream, string streamName) : base(stream , streamName)
		{
		}

		/// <summary>
		/// Retrieves statistical information about this stream
		/// </summary>
		/// <param name="pstatstg">Structure containing statisical information</param>
		/// <param name="grfStatFlag">Flags</param>
		/// <returns>COM hResult</returns>
		public override uint Stat(
			ref STATSTG pstatstg,
			STATFLAG grfStatFlag)
		{
			uint hRes = (uint) GENERIC_ERROR_CODES.S_OK;
			try
			{
				if ((grfStatFlag == STATFLAG.STATFLAG_DEFAULT) ||
					(grfStatFlag == STATFLAG.STATFLAG_NONAME))
				{
					pstatstg.type = STGTY.STGTY_STREAM;
					pstatstg.cbSize = stream.Length;
					FileStream fileStream = (FileStream) stream ;

					pstatstg.mtime = File.GetLastWriteTime(fileStream.Name).ToFileTime();
					pstatstg.ctime = File.GetCreationTime(fileStream.Name).ToFileTime();
					pstatstg.atime = File.GetLastAccessTime(fileStream.Name).ToFileTime();
					if (grfStatFlag != STATFLAG.STATFLAG_NONAME)
					{
						pstatstg.pwcsName = Marshal.StringToCoTaskMemUni(StreamName);
					}
				}			
			}
			catch (Exception)
			{
				hRes = (uint) GENERIC_ERROR_CODES.E_UNEXPECTED;
			}
			return hRes;
		}

	}
}
