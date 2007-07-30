using System;
using System.Runtime.InteropServices;

namespace Banshee.Cdrom.Windows.Interop
{
	/// <summary>
	/// Implementation of IDiscMasterProgressEvents.  This implementation
	/// simply transfers the event calls to the owning <see cref="DiscMaster"/>
	/// object from where they are raised as events.  The cookie is also
	/// stored.  Effectively, this class is immutable, however, the cookie
	/// has to be written after construction and therefore there is  set
	/// method.
	/// </summary>
	[Guid("0E817968-4B3F-42d5-B2F8-51E0113D12CE")]
	[ComVisible(true)]
	internal class DiscMasterProgressEvents : IDiscMasterProgressEvents, IDisposable
	{
		private IntPtr cookie = IntPtr.Zero;
		private DiscMaster owner;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">DiscMaster which will be called to receive
		/// events.</param>
		public DiscMasterProgressEvents(DiscMaster owner)
		{
			this.owner = owner;
		}

		/// <summary>
		/// Clears up any resources associated with this class.
		/// </summary>
		public void Dispose()
		{
			owner = null;
			cookie = IntPtr.Zero;
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Called to request whether the burn event should be cancelled
		/// </summary>
		/// <param name="pbCancel"></param>
		public void QueryCancel(out int pbCancel)
		{
			owner.QueryCancelRequest(out pbCancel);
		}

		/// <summary>
		/// Notifies that a Plug and Play activity has occurred that has changed the list of recorders.
		/// </summary>
		public void NotifyPnPActivity()
		{
			owner.NotifyPnPActivity();
		}

		/// <summary>
		/// Notifies addition of data to the CD image in the stash.
		/// </summary>
		public void NotifyAddProgress(
			int nCompleted,
			int nTotal)
		{
			owner.NotifyAddProgress(nCompleted, nTotal);
		}

		/// <summary>
		/// Notifies an application of block progress whilst burning a disc.
		/// </summary>
		public void NotifyBlockProgress(
			int nCurrentBlock,
			int nTotalBlocks)
		{
			owner.NotifyBlockProgress(nCurrentBlock, nTotalBlocks);
		}

		/// <summary>
		/// Notifies an application of track progress whilst burning an audio disc.
		/// </summary>
		public void NotifyTrackProgress(
			int nCurrentTrack,
			int nTotalTracks)
		{
			owner.NotifyTrackProgress(nCurrentTrack, nTotalTracks);
		}

		/// <summary>
		/// Notifies an application that IMAPI is preparing to burn a disc.
		/// </summary>
		public void NotifyPreparingBurn(
			int nEstimatedSeconds)
		{
			owner.NotifyPreparingBurn(nEstimatedSeconds);
		}

		/// <summary>
		/// Notifies an application that IMAPI is closing a disc.
		/// </summary>
		public void NotifyClosingDisc(
			int nEstimatedSeconds)
		{
			owner.NotifyClosingDisc(nEstimatedSeconds);
		}

		/// <summary>
		/// Notifies an application that IMAPI has completed burning a disc.
		/// </summary>
		public void NotifyBurnComplete(
			int status)
		{
			owner.NotifyBurnComplete(status);
		}

		/// <summary>
		/// Notifies an application that IMAPI has completed erasing a disc.
		/// </summary>
		public void NotifyEraseComplete(
			int status)
		{
			owner.NotifyEraseComplete(status);
		}

		/// <summary>
		/// Gets/sets the cookie associated with this implementation.  A
		/// cookie is provided by IMAPI whenever an IDiscMasterProgressEvents
		/// implementation is associated with an IDiscMaster object.  In
		/// order to release the implementation again, the cookie must
		/// be provided.  The cookie can only be set once in the lifetime
		/// of this object.
		/// </summary>
		public IntPtr Cookie
		{
			get
			{
				return cookie;
			}
			set
			{
				if (!cookie.Equals(IntPtr.Zero))
				{
					throw new InvalidOperationException("Attempt to set cookie when already set.");
				}
				cookie = value;
			}
		}

	}
}
