using System;
using System.Runtime.InteropServices;

namespace Banshee.Cdrom.Windows.Interop
{
	/// <summary>
	/// A wrapper around a byte buffer which is pinned in memory
	/// for use in interop scenarios.  This class is not thread-safe.	
	/// </summary>
	internal class PinnedByteBuffer : IDisposable
	{
		private byte[] buffer;
		private GCHandle handle;
		private int currentSize = 0;
		private bool disposed = false;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="initialSize">Buffer size</param>
		public PinnedByteBuffer(int initialSize)
		{
			CreateBuffer(initialSize);
		}

		/// <summary>
		/// Disposes resources associated with this object.
		/// </summary>
		public void Dispose()
		{
			if (!disposed)
			{
				ClearBuffer();				
				disposed = true;
			}
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Gets the current size of the byte buffer, or sets the
		/// size to a larger size than it currently is.  Attempts
		/// to set the buffer to a smaller size will have no effect.
		/// </summary>
		public int Size
		{
			get
			{
				return currentSize;
			}
			set
			{
				if (value > currentSize)
				{
					CreateBuffer(value);
				}
			}
		}

		/// <summary>
		/// Unpins the buffer managed by this class.
		/// </summary>
		private void ClearBuffer()
		{
			if (handle.IsAllocated)
			{
				handle.Free();
			}	
		}

		/// <summary>
		/// Creates a pinned buffer with the specified size.
		/// </summary>
		/// <param name="size">New size of buffer</param>
		private void CreateBuffer(int size)
		{
			if (size <= 0)
			{
				throw new ArgumentException("Buffer size must be >= 0", "BufferSize");
			}
			if (size > currentSize)
			{
				ClearBuffer();
				buffer = new Byte[size];
				handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);				
				currentSize = size;
			}
		}

		/// <summary>
		/// Get the array of bytes held by this class
		/// </summary>
		public byte[] Bytes
		{
			get
			{
				return buffer;
			}
		}

		/// <summary>
		/// Get a pointer to the array of bytes held by this class, 
		/// for use with unmanaged code.
		/// </summary>
		public IntPtr BufferAddress
		{
			get
			{
				return handle.AddrOfPinnedObject();
			}
		}
	}

}
