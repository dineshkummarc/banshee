using System;
using System.Collections;

namespace Banshee.Cdrom.Windows.Interop
{
	/// <summary>
	/// Represents the collection of disc recorders on the system.
	/// The available list of disc recorders is dependent on the
	/// current chosen mastering format as well as what is plugged
	/// into the system.
	/// </summary>
	public class DiscRecorders : ReadOnlyCollectionBase
	{
		private IDiscMaster discMaster;
		private bool disposed = false;

		/// <summary>
		/// Prevent instantiation from outside the library
		/// </summary>
		internal DiscRecorders(IDiscMaster discMaster)
		{
			this.discMaster = discMaster;
			Refresh();
		}

		/// <summary>
		/// Disposes any resources associated with this object.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(true);
		}

		/// <summary>
		/// Disposes any resources associated with this object.
		/// </summary>
		/// <param name="disposing"><c>true</c> if disposing from the <c>Dispose</c>
		/// method, <c>false</c> otherwise.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				InnerList.Clear();
				if (disposing)
				{					
					discMaster = null;
				}
				disposed = true;
			}
		}

		/// <summary>
		/// Returns the <c>DiscRecorder</c> at the specified 0-based index.
		/// </summary>
		public DiscRecorder this[int index]
		{
			get
			{
				return (DiscRecorder) InnerList[index];
			}
		}

		/// <summary>
		/// Gets/sets the active disc recorder on the system.
		/// </summary>
		public DiscRecorder ActiveDiscRecorder
		{
			get
			{
				DiscRecorder activeRecorder = null;

				IDiscRecorder recorder = null;				
				discMaster.GetActiveDiscRecorder(out recorder);
				string path;
				recorder.GetPath(out path);
				foreach (DiscRecorder compareRecorder in InnerList)
				{
					if (path.Equals(compareRecorder.OsPath))
					{
						activeRecorder = compareRecorder;		
					}
				}
				return activeRecorder;
			}
			set
			{
				IDiscRecorder discRecorder = value.IDiscRecorder;
				discMaster.SetActiveDiscRecorder(discRecorder);
			}
		}



		/// <summary>
		/// Refreshes the cached list of recorders.
		/// </summary>
		public void Refresh()
		{
			InnerList.Clear();

			IEnumDiscRecorders enumDiscRecorders = null;
			discMaster.EnumDiscRecorders(out enumDiscRecorders);
			if (enumDiscRecorders != null)
			{
				int fetched = 0;
				IDiscRecorder recorder = null;
				while ((!ComUtility.Failed(enumDiscRecorders.Next(1, out recorder, out fetched))) 
					&& (fetched > 0))
				{
					DiscRecorder discRecorder = new DiscRecorder(recorder);
					InnerList.Add(discRecorder);
				}
			}

		}



	}
}
