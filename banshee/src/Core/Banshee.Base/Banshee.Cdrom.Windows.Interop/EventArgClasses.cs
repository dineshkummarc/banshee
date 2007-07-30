using System;

namespace Banshee.Cdrom.Windows.Interop
{

	/// <summary>
	/// Event arguments for a QueryCancel event,
	/// fired whilst the disc is being staged
	/// and burnt.
	/// </summary>
	public class QueryCancelEventArgs : EventArgs
	{
		private bool cancel = false;

		/// <summary>
		/// Constructor.
		/// </summary>
		public QueryCancelEventArgs() : base()
		{
		}

		/// <summary>
		/// Get/sets whether the operation should be cancelled.
		/// </summary>
		public bool Cancel
		{
			get
			{
				return cancel;
			}
			set
			{
				cancel = value;
			}
		}		
	}

	/// <summary>
	/// Progress event information.  Provides the currently
	/// completed versus total amount to perform for various
	/// progress operations during staging and burning.
	/// </summary>
	public class ProgressEventArgs : EventArgs
	{
		private readonly int completed = 0;
		private readonly int total = 0;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="completed">Amount completed for this operation</param>
		/// <param name="total">Total amount for this operation</param>
		public ProgressEventArgs(int completed, int total)
		{
			this.completed = completed;
			this.total = total;
		}

		/// <summary>
		/// Gets the amount completed for this operation.
		/// </summary>
		public int Completed
		{
			get
			{
				return completed;
			}
		}

		/// <summary>
		/// Gets the total for this operation.
		/// </summary>
		public int Total
		{
			get
			{
				return total;
			}
		}
	}

	/// <summary>
	/// Information about an event in which only an estimated
	/// time to complete can be provided (Preparing and
	/// Finalising the disc).
	/// </summary>
	public class EstimatedTimeOperationEventArgs : EventArgs
	{
		private readonly int estimatedSeconds = 0;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="estimatedSeconds">Estimated time for this operation.</param>
		public EstimatedTimeOperationEventArgs(int estimatedSeconds)
		{
			this.estimatedSeconds = estimatedSeconds;
		}

		/// <summary>
		/// Gets the estimated length of this operation in seconds.
		/// </summary>
		public int EstimatedSeconds
		{
			get
			{
				return estimatedSeconds;
			}
		}
	}

	/// <summary>
	/// Completion status event information.
	/// </summary>
	public class CompletionStatusEventArgs : EventArgs
	{
		private readonly int status = 0;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="status">Status of the operation that has just completed.</param>
		public CompletionStatusEventArgs(int status)
		{
			this.status = status;
		}

		/// <summary>
		/// Gets the status of this operation.
		/// </summary>
		public int Status
		{
			get
			{
				return status;
			}
		}
	}	

}
