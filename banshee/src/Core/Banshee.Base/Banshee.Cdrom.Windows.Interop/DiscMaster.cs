using System;
using System.Runtime.InteropServices;

namespace Banshee.Cdrom.Windows.Interop
{	

	/// <summary>
	/// Represents the handler for a QueryCancel event.
	/// </summary>
	public delegate void QueryCancelEventHandler(object sender, QueryCancelEventArgs args);

	/// <summary>
	/// Represents the handler for a Progress event.
	/// </summary>
	public delegate void ProgressEventHandler(object sender, ProgressEventArgs args);

	/// <summary>
	/// Represents the handler for an Estimated Time operation event.
	/// </summary>
	public delegate void EstimatedTimeOperationEventHandler(object sender, EstimatedTimeOperationEventArgs args);

	/// <summary>
	/// Represents the handler for a Completion Status event.
	/// </summary>
	public delegate void CompletionStatusEventHandler(object sender, CompletionStatusEventArgs args);

	/// <summary>
	/// Summary description for DiscMaster.
	/// </summary>
	public class DiscMaster : IDisposable
	{
		private const string IID_IRedbookDiscMaster = "E3BC42CD-4E5C-11D3-9144-00104BA11C5E";
		private const string IID_IJolietDiscMaster = "E3BC42CE-4E5C-11D3-9144-00104BA11C5E";

		private bool disposed = false;
		private IDiscMaster discMaster = null;
		private DiscMasterProgressEvents progressEvents = null;
		private DiscRecorders discRecorders = null;
		private bool jolietAddDataCancel = false;

		/// <summary>
		/// Raised to request whether to cancel staging an image or
		/// burning a CD.
		/// </summary>
		public event QueryCancelEventHandler QueryCancel;
		
		/// <summary>
		/// Raised when a Plug'n'Play event occurs on this system that changes
		/// the list of available drives.
		/// </summary>
		public event EventHandler PnPActivity;

		/// <summary>
		/// Raised during staging of the disc as data is added to the staging
		/// area.
		/// </summary>
		public event ProgressEventHandler AddProgress;

		/// <summary>
		/// Raised during writing of a disc as blocks are added to the disc.
		/// </summary>
		public event ProgressEventHandler BlockProgress;

		/// <summary>
		/// Raised during writing of an audio disc as tracks are added to the
		/// disc.
		/// </summary>
		public event ProgressEventHandler TrackProgress;

		/// <summary>
		/// Raised when disc is about to be prepared for burning.
		/// </summary>
		public event EstimatedTimeOperationEventHandler PreparingBurn;

		/// <summary>
		/// Raised when the disc is about to be finalised.
		/// </summary>
		public event EstimatedTimeOperationEventHandler ClosingDisc;

		/// <summary>
		/// Raised when a burn operation has completed.
		/// </summary>
		public event CompletionStatusEventHandler BurnComplete;

		/// <summary>
		/// Raised when an erase operation has completed.
		/// </summary>
		public event CompletionStatusEventHandler EraseComplete;


		/// <summary>
		/// Constructs a new instance of this class.
		/// </summary>
		/// <exception cref="COMException">if the System's <c>ICDBurn</c> or
		/// <c>IMAPI</c> implementations cannot be instantiated</exception>
		public DiscMaster()
		{
			InitialiseIMAPI();
		}

		/// <summary>
		/// Destructor for class.
		/// </summary>
		~DiscMaster()
		{
			Dispose(false);
		}

		/// <summary>
		/// Disposes any resources associated with this class.  Note that
		/// closing an IMAPI session may take a while as the image area
		/// is cleared. It is highly recommended you call this method
		/// before your application is closed.
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
					if (discMaster != null)
					{
						if (progressEvents != null)
						{
							discMaster.ProgressUnadvise(progressEvents.Cookie);
							progressEvents.Dispose();
							progressEvents = null;
						}
						discMaster.Close();
						Marshal.ReleaseComObject(discMaster);
						discMaster = null;
					}
				}
			}
			this.disposed = true;
		}
	
		/// <summary>
		/// Gets the collection of disc recorders on this system.
		/// </summary>
		public DiscRecorders DiscRecorders
		{
			get
			{
				return discRecorders;
			}
		}

		/// <summary>
		/// Gets the Redbook (audio) disc mastering object for this
		/// system.  Getting this may change the list of available
		/// recorders on the system as some recorders may not support
		/// audio CDs.
		/// </summary>
		/// <returns>Redbook disc mastering object.</returns>
		public RedbookDiscMaster RedbookDiscMaster()
		{
			object objRedbook = null;
			Guid iidRedbook = new Guid(IID_IRedbookDiscMaster);		
			discMaster.SetActiveDiscMasterFormat(ref iidRedbook, out objRedbook);
			discRecorders.Refresh();
			RedbookDiscMaster redbookDiscMaster = new RedbookDiscMaster(this, (IRedbookDiscMaster) objRedbook);
			return redbookDiscMaster;		
		}

		/// <summary>
		/// Gets the Joliet (data) disc mastering object for this
		/// system.  Getting this may change the list of available
		/// recorders on the system.
		/// </summary>
		/// <returns>Joliet disc mastering object.</returns>
		public JolietDiscMaster JolietDiscMaster()
		{
			object objJoliet = null;
			Guid iidJoliet = new Guid(IID_IJolietDiscMaster);
			discMaster.SetActiveDiscMasterFormat(ref iidJoliet, out objJoliet);
			discRecorders.Refresh();
			JolietDiscMaster jolietDiscMaster = new JolietDiscMaster(this, (IJolietDiscMaster) objJoliet);
			return jolietDiscMaster;
		}

		/// <summary>
		/// Records the image in the staging area built up with the Joliet
		/// or Redbook disc master classes onto the disc.
		/// </summary>
		/// <param name="simulate">Whether to simulate the burn without actually
		/// recording any data to the disc.</param>
		/// <param name="ejectWhenComplete">Whether to eject the drive tray once
		/// the burn or simulated burn has completed.</param>
		public void RecordDisc(bool simulate, bool ejectWhenComplete)
		{
			if (!jolietAddDataCancel)
			{
				try
				{
					discMaster.RecordDisc((simulate ? 1 : 0), (ejectWhenComplete ? 1 : 0));
				}
				catch (COMException ex)
				{
					if ((uint) ex.ErrorCode == (uint) IMAPI_ERROR_CODES.IMAPI_E_USERABORT)
					{
						// fine
					}
					else
					{
						// rethrow
						throw ex;
					}
				}
			}
		}

		/// <summary>
		/// Clears any content placed into the staging area by the Joliet or Redbook
		/// disc master classes.
		/// </summary>
		public void ClearFormatContent()
		{
			jolietAddDataCancel = false;
			discMaster.ClearFormatContent();
		}

		/// <summary>
		/// Instantiate the <c>MsDiscMasterObj</c> implementation, open
		/// it, create progress events and the recorder collection.
		/// </summary>
		/// <exception cref="COMException">if the <c>MsDiscMasterObj</c> implementation
		/// cannot be instantiated</exception>
		private void InitialiseIMAPI()
		{
			discMaster = IMAPIObjectFactory.CreateDiscMaster();
			discMaster.Open();
			
			// Set up progress events
			progressEvents = new DiscMasterProgressEvents(this);
			IntPtr cookie = IntPtr.Zero;
			IDiscMasterProgressEvents iprgEvents = (IDiscMasterProgressEvents) progressEvents;
			discMaster.ProgressAdvise(iprgEvents, out cookie);
			progressEvents.Cookie = cookie;

			// Recorders collection
			discRecorders = new DiscRecorders(discMaster);
		}

		/// <summary>
		/// Called to request whether the burn event should be cancelled
		/// </summary>
		/// <param name="cancel">Set to <c>1</c> to cancel, otherwise
		/// set to <c>0</c>.</param>
		internal void QueryCancelRequest(out int cancel)
		{
			if (jolietAddDataCancel)
			{
				cancel = 1;
			}
			else
			{
				QueryCancelEventArgs queryCancelArgs = new QueryCancelEventArgs();
				OnQueryCancel(queryCancelArgs);
				cancel = (queryCancelArgs.Cancel ? 1 : 0);
				if (cancel == 1)
				{
					jolietAddDataCancel = true;
				}
			}
		}

		internal void ResetJolietAddDataCancel()
		{
			jolietAddDataCancel = false;
		}

		/// <summary>
		/// Raises the <see cref="QueryCancel"/> event.
		/// </summary>
		/// <param name="args">Query cancel event arguments</param>
		protected virtual void OnQueryCancel(QueryCancelEventArgs args)
		{
			if (QueryCancel != null)
			{
				QueryCancel(this, args);
			}
		}

		/// <summary>
		/// Notifies that a Plug and Play activity has occurred that has changed the list of recorders.
		/// </summary>
		internal void NotifyPnPActivity()
		{
			OnPnPActivity(new EventArgs());
		}

		/// <summary>
		/// Raises the <see cref="PnPActivity"/> event.
		/// </summary>
		/// <param name="args">Not used.</param>
		protected virtual void OnPnPActivity(EventArgs args)
		{
			if (PnPActivity != null)
			{
				PnPActivity(this, args);
			}
		}

		/// <summary>
		/// Notifies addition of data to the CD image in the stash.
		/// </summary>
		internal void NotifyAddProgress(
			int completed,
			int total)
		{
			OnAddProgress(new ProgressEventArgs(completed, total));
		}

		/// <summary>
		/// Raises the <see cref="AddProgress"/> event.
		/// </summary>
		/// <param name="args">Details of the add progress so far.</param>
		protected virtual void OnAddProgress(ProgressEventArgs args)
		{
			if (AddProgress != null)
			{
				AddProgress(this, args);
			}
		}

		/// <summary>
		/// Notifies an application of block progress whilst burning a disc.
		/// </summary>
		internal void NotifyBlockProgress(
			int currentBlock,
			int totalBlocks)
		{
			OnBlockProgress(new ProgressEventArgs(currentBlock, totalBlocks));
		}

		/// <summary>
		/// Raises the <see cref="BlockProgress"/> event.
		/// </summary>
		/// <param name="args">Details of the progress so far.</param>
		protected virtual void OnBlockProgress(ProgressEventArgs args)
		{
			if (BlockProgress != null)
			{
				BlockProgress(this, args);
			}
		}

		/// <summary>
		/// Notifies an application of track progress whilst burning an audio disc.
		/// </summary>
		internal void NotifyTrackProgress(
			int currentTrack,
			int totalTracks)
		{
			OnTrackProgress(new ProgressEventArgs(currentTrack, totalTracks));
		}

		/// <summary>
		/// Raises the <see cref="TrackProgress"/> event (if creating
		/// an audio CD).
		/// </summary>
		/// <param name="args">Details of progress so far</param>
		protected virtual void OnTrackProgress(ProgressEventArgs args)
		{
			if (TrackProgress != null)
			{
				TrackProgress(this, args);
			}
		}

		/// <summary>
		/// Notifies an application that IMAPI is preparing to burn a disc.
		/// </summary>
		internal void NotifyPreparingBurn(
			int estimatedSeconds)
		{
			OnPreparingBurn(new EstimatedTimeOperationEventArgs(estimatedSeconds));
		}

		/// <summary>
		/// Raises the <see cref="PreparingBurn"/> event.
		/// </summary>
		/// <param name="args">Details of the estimated time for the preparaion.</param>
		protected virtual void OnPreparingBurn(EstimatedTimeOperationEventArgs args)
		{
			if (PreparingBurn != null)
			{
				PreparingBurn(this, args);
			}
		}

		/// <summary>
		/// Notifies an application that IMAPI is closing a disc.
		/// </summary>
		internal void NotifyClosingDisc(
			int estimatedSeconds)
		{
			OnClosingDisc(new EstimatedTimeOperationEventArgs(estimatedSeconds));
		}

		/// <summary>
		/// Raises the <see cref="ClosingDisc"/> event.
		/// </summary>
		/// <param name="args">Details of the estimated time to close the disc.</param>
		protected virtual void OnClosingDisc(EstimatedTimeOperationEventArgs args)
		{
			if (ClosingDisc != null)
			{
				ClosingDisc(this, args);
			}
		}

		/// <summary>
		/// Notifies an application that IMAPI has completed burning a disc.
		/// </summary>
		internal void NotifyBurnComplete(
			int status)
		{
			OnBurnComplete(new CompletionStatusEventArgs(status));
		}

		/// <summary>
		/// Raises the <see cref="BurnComplete"/> event.
		/// </summary>
		/// <param name="args">Status of the burn.</param>
		protected virtual void OnBurnComplete(CompletionStatusEventArgs args)
		{
			if (BurnComplete != null)
			{
				BurnComplete(this, args);
			}
		}

		/// <summary>
		/// Notifies an application that IMAPI has completed erasing a disc.
		/// </summary>
		internal void NotifyEraseComplete(
			int status)
		{
			OnEraseComplete(new CompletionStatusEventArgs(status));
		}

		/// <summary>
		/// Raises the <see cref="EraseComplete"/> event.
		/// </summary>
		/// <param name="args">Status of the erase.</param>
		protected virtual void OnEraseComplete(CompletionStatusEventArgs args)
		{
			if (EraseComplete != null)
			{
				EraseComplete(this, args);
			}
		}

		


	}
}
