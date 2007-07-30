using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Banshee.Cdrom.Windows.Interop
{
	/// <summary>
	/// Object for constructing a Redbook (audio) CD image
	/// in the staging area in preparation for a CD burn.
	/// </summary>
	public class RedbookDiscMaster : IDisposable
	{
		private const int BLOCK_MULTIPLE = 20;

		private DiscMaster owner = null;
		private IRedbookDiscMaster redbookMaster = null;
		private bool disposed = false;
		PinnedByteBuffer buffer = null;		
		private int track = 0;

		/// <summary>
		/// Internal constructor: instances of this object should
		/// be obtained from the <see cref="DiscMaster"/> object.
		/// </summary>
		/// <param name="owner">Disc master object which owns this object.</param>
		/// <param name="redbookMaster">IMAPI redbook disc mastering object to wrap.</param>
		internal RedbookDiscMaster(DiscMaster owner, IRedbookDiscMaster redbookMaster)
		{
			this.owner = owner;
			this.redbookMaster = redbookMaster;
		}

		/// <summary>
		/// Clears up any resources associated with this class.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Clears up any resources associated with this class.
		/// </summary>
		/// <param name="disposing"><c>true</c> if the method is called
		/// from the <c>Dispose</c> method, otherwise <c>false</c>.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					Marshal.ReleaseComObject(redbookMaster);
					redbookMaster = null;
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
		/// Gets the number of available track blocks remaining on the disc.
		/// </summary>
		public int AvailableTrackBlocks
		{
			get
			{
				int blocks = 0;
				redbookMaster.GetAvailableAudioTrackBlocks(out blocks);
				return blocks;
			}
		}

		/// <summary>
		/// Gets the size of a single audio block (2352 bytes)
		/// </summary>
		public int AudioBlockSize
		{
			get
			{
				int blockSize = 0;
				redbookMaster.GetAudioBlockSize(out blockSize);
				return blockSize;
			}
		}

		/// <summary>
		/// Gets the total number of audio blocks on the disc.
		/// </summary>
		public int TotalAudioBlocks
		{
			get
			{
				int blocks = 0;
				redbookMaster.GetTotalAudioBlocks(out blocks);
				return blocks;
			}
		}

		/// <summary>
		/// Gets the used number of audio blocks on the disc.
		/// </summary>
		public int UsedAudioBlocks
		{
			get
			{
				int blocks = 0;
				redbookMaster.GetUsedAudioBlocks(out blocks);
				return blocks;
			}
		}

		/// <summary>
		/// Gets the total number of audio tracks on the disc.
		/// </summary>
		public int TotalAudioTracks
		{
			get
			{
				int tracks = 0;
				redbookMaster.GetTotalAudioTracks(out tracks);
				return tracks;
			}
		}

		/// <summary>
		/// Creates a new audio track with the specified number of blocks.
		/// Audio CDs may have between 1 and 99 tracks.
		/// </summary>
		/// <param name="blocks">Number of audio blocks for this track.</param>
		public void CreateAudioTrack(int blocks)
		{
			owner.NotifyTrackProgress(track, track+1);
			redbookMaster.CreateAudioTrack(blocks);
			if (buffer == null)
			{
				buffer = new PinnedByteBuffer(AudioBlockSize * BLOCK_MULTIPLE);
			}
		}

		/// <summary>
		/// Closes an audio track previously opened with <c>CreateAudioTrack</c>.
		/// Call this method after you have added all of the data to the track.
		/// </summary>
		public void CloseAudioTrack()
		{
			redbookMaster.CloseAudioTrack();
			track++;
			owner.NotifyTrackProgress(track, track);
		}

		/// <summary>
		/// Adds raw audio data to the track from a <c>Stream</c>.  Raw
		/// audio data must be presented as stereo, 16 bit signed L-R
		/// pairs of data, with a sampling frequency of 44.1kHz.
		/// The track must have been created using <see cref="CreateAudioTrack"/>.
		/// </summary>
		/// <param name="rawAudioStream">Stream containing raw audio
		/// data.</param>
		/// <param name="bytes">Number of bytes to add from the stream.
		/// This must be a multiple of the <c>AudioBlockSize</c>; if
		/// not this class will add padding 0s to the unused bytes
		/// (for the end of a track).</param>
		public void AddAudioTrackBlocks(Stream rawAudioStream, int bytes)
		{
			if (bytes > buffer.Size)
			{
				buffer.Size = bytes;
			}
			
			// Read bytes in:
			rawAudioStream.Read(buffer.Bytes, 0, bytes);
			
			bytes = ZeroTrailingBufferBytes(bytes);
			
			// Write the data to the staging area:
			redbookMaster.AddAudioTrackBlocks(buffer.BufferAddress, bytes);
		}

		private int ZeroTrailingBufferBytes(int bytes)
		{
			// If we didn't read an even multiple of block size, then
			// zero out any bytes from the last read byte to the end:
			int blockSize = AudioBlockSize;
			if ((bytes % blockSize) != 0)
			{
				int end = (int) Math.Ceiling(((double) bytes) / AudioBlockSize) * AudioBlockSize;
				for (int zeroByte = bytes; zeroByte < end; zeroByte++)
				{
					buffer.Bytes[zeroByte] = 0;
				}
				bytes = end;
			}
			return bytes;
		}

		/// <summary>
		/// Adds a new audio track from a <c>Stream</c>.  Raw
		/// audio data must be presented as stereo, 16 bit signed L-R
		/// pairs of data, with a sampling frequency of 44.1kHz.
		/// </summary>
		/// <param name="rawAudioStream">Stream containing raw audio
		/// data.</param>
		public void AddAudioTrackFromStream(Stream rawAudioStream)
		{
			int cancel = 0;
			int completed = 0;
			owner.QueryCancelRequest(out cancel);
			if (cancel == 0)
			{

				if (buffer == null)
				{
					buffer = new PinnedByteBuffer(AudioBlockSize * BLOCK_MULTIPLE);
				}

				int blocks = (int) Math.Ceiling(((double) rawAudioStream.Length) / AudioBlockSize);
				CreateAudioTrack(blocks);
				owner.QueryCancelRequest(out cancel);
				if (cancel == 0)
				{
					int size = 0;
					while ((size = rawAudioStream.Read(buffer.Bytes, 0, buffer.Size)) > 0)
					{
						size = ZeroTrailingBufferBytes(size);
						redbookMaster.AddAudioTrackBlocks(buffer.BufferAddress, size);
						completed += size / AudioBlockSize;
						owner.QueryCancelRequest(out cancel);
						if (cancel != 0)
						{
							break;
						}
					}

					CloseAudioTrack();
				}
			}
		}

	}

}
