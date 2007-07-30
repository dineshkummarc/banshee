using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Banshee.Cdrom.Windows.Interop
{
	/// <summary>
	/// Wrapper for an IMAPI <c>IJolietDiscMaster</c> object for use in
	/// managed code.  This class allows a data (Joliet) disc image to
	/// be staged which can be later burnt using the <see cref="DiscMaster"/>
	/// object.
	/// </summary>
	public class JolietDiscMaster : IDisposable
	{
		private bool disposed = false;
		private DiscMaster owner = null;
		private IJolietDiscMaster jolietDiscMaster = null;
		private JolietDiscMasterStorage rootStorage = null;

		/// <summary>
		/// Internal constructor.  Instances of this class should only
		/// be obtained from the <see cref="DiscMaster"/> object.
		/// </summary>
		/// <param name="owner">Disc master object which owns this object.</param>
		/// <param name="jolietDiscMaster">IMAPI Joliet Disc</param>
		internal JolietDiscMaster(DiscMaster owner, IJolietDiscMaster jolietDiscMaster)
		{
			this.owner = owner;
			this.jolietDiscMaster = jolietDiscMaster;
			this.rootStorage = new JolietDiscMasterStorage(owner);
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~JolietDiscMaster()
		{
			Dispose(false);
		}

		/// <summary>
		/// Disposes any resources associated with this object.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Disposes any resources associated with this object
		/// </summary>
		/// <param name="disposing"><c>true</c> if disposing from the
		/// <see cref="Dispose"/> method, otherwise <c>false</c>.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					rootStorage.Dispose();
					Marshal.ReleaseComObject(jolietDiscMaster);
					jolietDiscMaster = null;
				}
				disposed = true;
			}
		}

		/// <summary>
		/// Gets the storage class which holds the files and folders
		/// to be written to the CD.
		/// </summary>
		public JolietDiscMasterStorage RootStorage
		{
			get
			{
				return rootStorage;
			}
		}

		/// <summary>
		/// Gets the size of data block in the image in bytes (2048 bytes).
		/// </summary>
		public int DataBlockSize
		{
			get
			{
				int blockSize;
				jolietDiscMaster.GetDataBlockSize(out blockSize);
				return blockSize;
			}
		}

		/// <summary>
		/// Gets the total number of data blocks on the disc.
		/// </summary>
		public int TotalDataBlocks
		{
			get
			{
				int totalBlocks;
				jolietDiscMaster.GetTotalDataBlocks(out totalBlocks);
				return totalBlocks;
			}
		}

		/// <summary>
		/// Gets the number of used data blocks on the disc.
		/// </summary>
		public int UsedDataBlocks
		{
			get
			{
				int dataBlocks;
				jolietDiscMaster.GetUsedDataBlocks(out dataBlocks);
				return dataBlocks;
			}
		}

		/// <summary>
		/// Gets the properties associated with the Joliet Disc Master
		/// </summary>
		public JolietDiscMasterProperties Properties
		{
			get
			{
				IPropertyStorage storage;
				jolietDiscMaster.GetJolietProperties(out storage);
				JolietDiscMasterProperties props = new JolietDiscMasterProperties(storage);
				return props;
			}
		}

		/// <summary>
		/// Adds data to the Joliet Disc Master cache from a JolietDiscMasterStorage
		/// instance.
		/// </summary>
		/// <param name="overwrite"><c>true</c> if overwriting should occur, <c>false</c>
		/// otherwise.</param>
		public void AddData(bool overwrite)
		{
			owner.ResetJolietAddDataCancel();
			int cancel = 0;
			owner.QueryCancelRequest(out cancel);
			if (cancel == 0)
			{
				IStorage istorage = rootStorage.GetIStorage();
				Debug.WriteLine(String.Format("Adding data to cache for storage {0}", istorage));
				jolietDiscMaster.AddData(istorage, (overwrite ? 1 : 0));
				Debug.WriteLine(String.Format("Completed adding data to cache for storage {0}", istorage));
			}
		}

		/// <summary>
		/// Updates properties for this disc.
		/// </summary>
		/// <param name="properties">Properties collection previously obtained
		/// from the <see cref="Properties"/> accessor.</param>
		public void SetProperties(JolietDiscMasterProperties properties)
		{
			IPropertyStorage propStorage = properties.GetIPropertyStorage();
			jolietDiscMaster.SetJolietProperties(ref propStorage);
		}

	}
}
