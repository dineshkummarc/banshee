using System;

namespace Banshee.Cdrom.Windows.Interop
{
	/// <summary>
	/// Describes media (if available) in a recorder's drive.
	/// This class is thread-safe.
	/// </summary>
	public class MediaDetails
	{
		private readonly byte sessions;
		private readonly byte lastTrack;
		private readonly int startAddress;
		private readonly int nextWritable;
		private readonly int freeBlocks;
		private readonly MEDIA_TYPES mediaType;
		private readonly MEDIA_FLAGS mediaFlags;
		private readonly bool noMedia = false;

		/// <summary>
		/// Internal constructor; instances of this class should be
		/// obtained from a <see cref="DiscRecorder"/> object.
		/// </summary>
		/// <param name="recorder">IMAPI disc recorder object to
		/// obtain media information for.  This must have been
		/// opened for exclusive access otherwise the information
		/// will not be retreived.</param>
		internal MediaDetails(IDiscRecorder recorder)
		{
			int intMediaType = 0;
			int intMediaFlags = 0;
			recorder.QueryMediaType(out intMediaType, out intMediaFlags);
			if ((intMediaType == 0) || (intMediaFlags == 0))
			{
				noMedia = true;
			}
			else
			{
				mediaType = (MEDIA_TYPES) intMediaType;
				mediaFlags = (MEDIA_FLAGS) intMediaFlags;
				recorder.QueryMediaInfo(
					out sessions, out lastTrack, out startAddress, out nextWritable, out freeBlocks);
			}
		}

		/// <summary>
		/// Gets whether there is media present in the drive.
		/// </summary>
		public bool MediaPresent
		{
			get
			{
				return (!noMedia);
			}
		}

		/// <summary>
		/// Gets the number of sessions on the media.
		/// </summary>
		public byte Sessions
		{
			get
			{
				return sessions;
			}
		}

		/// <summary>
		/// Gets the number of tracks on the media if it is an audio CD
		/// </summary>
		public byte LastTrack
		{
			get
			{
				return lastTrack;
			}
		}

		/// <summary>
		/// Gets the start address for the media.
		/// </summary>
		public int StartAddress
		{
			get
			{
				return startAddress;			
			}
		}

		/// <summary>
		/// Gets the next writable address on the media.
		/// </summary>
		public int NextWritable
		{
			get
			{
				return nextWritable;
			}
		}

		/// <summary>
		/// Gets the number of free blocks on the media.
		/// </summary>
		public int FreeBlocks
		{
			get
			{
				return freeBlocks;
			}
		}

		/// <summary>
		/// Gets the type of media.
		/// </summary>
		public MEDIA_TYPES MediaType
		{
			get
			{
				return mediaType;
			}
		}

		/// <summary>
		/// Gets information about the media such as whether it is blank,
		/// writable and so on.
		/// </summary>
		public MEDIA_FLAGS MediaFlags
		{
			get
			{
				return mediaFlags;
			}
		}

		/// <summary>
		/// Gets a string representation of this object for debuging purposes
		/// </summary>
		/// <returns>string representation of the object</returns>
		public override string ToString()
		{
			if (noMedia)
			{
				return "MediaDetails: No Media Present";
			}
			else
			{
				return String.Format("MediaDetails: {0} {1} {2} {3} {4} {5} {6}", 
					sessions, lastTrack , startAddress, nextWritable,
					freeBlocks, mediaType, mediaFlags);
			}
		}

	}
}
