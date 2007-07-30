using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Banshee.Cdrom.Windows.Interop
{
	/// <summary>
	/// Wrapper around a managed <c>Stream</c> to convert it to an <c>IStream</c>.
	/// </summary>
	[ComVisible(true)]
	internal class IStreamOnStream : IStream, IDisposable
	{
		/// <summary>
		/// The underlying stream passed in to the constructor
		/// </summary>
		protected Stream stream;

		private string streamName;
		private PinnedByteBuffer buffer;
		private bool disposed = false;

		/// <summary>
		/// Constructs a new wrapper for the specified stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="streamName"></param>
		public IStreamOnStream(Stream stream, string streamName)
		{
			this.stream = stream;
			this.streamName = streamName;
		}

		/// <summary>
		/// Disposes any resources associated with this stream
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Disposes any resources associated with this stream
		/// </summary>
		/// <param name="disposing"><c>true</c> if called from the <c>Dispose</c>
		/// method, <c>false</c> otherwise.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					if (buffer != null)
					{
						buffer.Dispose();
						buffer = null;
					}
				}
				disposed = true;
			}
		}

		/// <summary>
		/// Gets/sets the stream's name
		/// </summary>
		public string StreamName
		{
			get
			{
				return streamName;
			}
			set
			{
				streamName = value;
			}
		}

		private void EnsureBuffer(int size)
		{
			if (buffer == null)
			{
				buffer = new PinnedByteBuffer(size);
			}
			else
			{
				buffer.Size = size;
			}
		}

		/// <summary>
		/// Reads from the stream.
		/// </summary>
		/// <param name="pv">Pointer to buffer to put the results into</param>
		/// <param name="cb">Number of bytes to read</param>
		/// <param name="pcbRead">Number of bytes actually read</param>
		/// <returns>COM hResult</returns>
		public virtual uint Read(
			IntPtr pv,
			int cb,
			ref int pcbRead)
		{
			Debug.WriteLine(String.Format("Reading {0} bytes from stream", cb));
			uint hRes = (uint) GENERIC_ERROR_CODES.S_OK;
			if (pv != IntPtr.Zero)
			{
				EnsureBuffer(cb);
				try
				{
					pcbRead = stream.Read(buffer.Bytes, 0, cb);
					Marshal.Copy(buffer.Bytes, 0, pv, pcbRead);
				}
				catch (Exception)
				{
					hRes = (uint) GENERIC_ERROR_CODES.S_FALSE;
				}
			}
			else
			{
				hRes = (uint) STG_ERROR_CONSTANTS.STG_E_INVALIDPOINTER;
			}
			return hRes;
		}

		/// <summary>
		/// Write to the stream
		/// </summary>
		/// <param name="pv">Pointer to buffer to read from</param>
		/// <param name="cb">Number of bytes to write</param>
		/// <param name="pcbWritten">Number of bytes actually written</param>
		/// <returns>COM hResult</returns>
		public virtual uint Write(
			IntPtr pv,
			int cb,
			ref int pcbWritten)
		{
			uint hRes = (uint) GENERIC_ERROR_CODES.S_OK;
			if (pv != IntPtr.Zero)
			{
				EnsureBuffer(cb);
				try
				{
					Marshal.Copy(pv, buffer.Bytes, 0, cb);
					stream.Write(buffer.Bytes, 0, cb);
					pcbWritten = cb;
				}
				catch (Exception)
				{
					hRes = (uint) GENERIC_ERROR_CODES.S_FALSE;
				}
			}
			else
			{
				hRes = (uint) STG_ERROR_CONSTANTS.STG_E_INVALIDPOINTER;
			}
			return hRes;
		}

		/// <summary>
		/// Seek to the specified point in the stream
		/// </summary>
		/// <param name="dlibMove">Position to move to</param>
		/// <param name="dwOrigin">Seek origin</param>
		/// <param name="plibNewPosition">New position set</param>
		/// <returns>COM hResult</returns>
		public virtual uint Seek(
			long dlibMove,
			STREAM_SEEK dwOrigin,
			ref long plibNewPosition)
		{
			uint hRes;
			SeekOrigin seekOrigin = SeekOrigin.Current;
			switch (dwOrigin)
			{
				case STREAM_SEEK.STREAM_SEEK_SET:
					seekOrigin = SeekOrigin.Begin;
					break;
				case STREAM_SEEK.STREAM_SEEK_END:
					seekOrigin = SeekOrigin.End;
					break;
			}
			try
			{
				plibNewPosition = stream.Seek(dlibMove, seekOrigin);
				hRes = (uint) GENERIC_ERROR_CODES.S_OK;
			}
			catch (Exception)
			{
				hRes = (uint) GENERIC_ERROR_CODES.S_FALSE;
			}
			return hRes;
		}

		/// <summary>
		/// Set the size of the stream
		/// </summary>
		/// <param name="libNewSize">New size</param>
		/// <returns>COM hResult</returns>
		public virtual uint SetSize(
			long libNewSize)
		{
			uint hRes;
			try
			{
				stream.SetLength(libNewSize);
				if (stream.Length == libNewSize)
				{
					hRes = (uint) GENERIC_ERROR_CODES.S_OK;
				}
				else
				{
					hRes = (uint) GENERIC_ERROR_CODES.E_FAIL;
				}
			}
			catch (Exception)
			{
				hRes = (uint) GENERIC_ERROR_CODES.E_FAIL;
			}
			return hRes;
		}

		public virtual uint CopyTo(
			[In()]
			ref IStream pstm,
			long cb,
			ref long pcbRead,
			ref long pcbWritten)
		{
			return (uint) GENERIC_ERROR_CODES.E_NOTIMPL;
		}

		/// <summary>
		/// Commit changes.
		/// </summary>
		/// <param name="grfCommitFlags">Flags describing change to commit</param>
		/// <returns>COM hResult</returns>
		public virtual uint Commit(
			ref STGC grfCommitFlags)
		{
			return (uint) GENERIC_ERROR_CODES.S_OK;
		}

		/// <summary>
		/// Revert any changes.
		/// </summary>
		/// <returns>COM hResult</returns>
		public virtual uint Revert()
		{
			return (uint) GENERIC_ERROR_CODES.S_OK;
		}

		/// <summary>
		/// Lock a region of the stream
		/// </summary>
		/// <param name="libOffset">Start point to lock from</param>
		/// <param name="cb">Number of bytes to lock</param>
		/// <param name="dwLockType">Type of lock</param>
		/// <returns>COM hResult</returns>
		public virtual uint LockRegion(
			long libOffset,
			long cb,
			int dwLockType)
		{
			return (uint) STG_ERROR_CONSTANTS.STG_E_INVALIDFUNCTION;
		}

		/// <summary>
		/// Unlock a region 
		/// </summary>
		/// <param name="libOffset">Start point to unlock</param>
		/// <param name="cb">Number of bytes to unlock</param>
		/// <param name="dwLockType">Type of lock to unlock</param>
		/// <returns>COM hResult</returns>
		public virtual uint UnlockRegion(
			long libOffset,
			long cb,
			int dwLockType)
		{
			return (uint) STG_ERROR_CONSTANTS.STG_E_INVALIDFUNCTION;
		}

		/// <summary>
		/// Retrieves statistical information about this stream
		/// </summary>
		/// <param name="pstatstg">Structure containing statisical information</param>
		/// <param name="grfStatFlag">Flags</param>
		/// <returns>COM hResult</returns>
		public virtual uint Stat(
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
					pstatstg.mtime = DateTime.Now.ToFileTime();
					pstatstg.ctime = DateTime.Now.ToFileTime();
					pstatstg.atime = DateTime.Now.ToFileTime();
					if (grfStatFlag != STATFLAG.STATFLAG_NONAME)
					{
						pstatstg.pwcsName = Marshal.StringToCoTaskMemUni(streamName);
					}
				}			
			}
			catch (Exception)
			{
				hRes = (uint) GENERIC_ERROR_CODES.E_UNEXPECTED;
			}
			return hRes;
		}

		/// <summary>
		/// Clone this stream
		/// </summary>
		/// <param name="ppstm">Cloned stream</param>
		/// <returns>COM hResult</returns>
		public virtual uint Clone(
			out IStream ppstm)
		{
			ppstm = null;
			return (uint) GENERIC_ERROR_CODES.E_NOTIMPL;
		}

	}
	

}
