using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

namespace Banshee.Cdrom.Windows.Interop
{
	/// <summary>
	/// A JolietDiscMasterStorage manages a single directory to be added
	/// to the disc.  A directory can contain files and sub-directories;
	/// the top level or root directory of the CD has a blank folder name.
	/// </summary>
	public class JolietDiscMasterStorage : IDisposable
	{
		private JolietIStorage storage;
		private DiscMaster owner;

		// Folder name.  Root folder is ""
		private string folderName = "";
		// Hashtable of folders; key is name, value is folder
		private Hashtable subFolders = new Hashtable();
		// Hashtable of files; key is name, value is source file
		private Hashtable files = new Hashtable();

		/// <summary>
		/// Constructor
		/// </summary>
		internal JolietDiscMasterStorage(DiscMaster owner)
		{			
			this.owner = owner;
			storage = new JolietIStorage(this, "");
		}

		internal JolietDiscMasterStorage(DiscMaster owner, string folder) 
		{
			this.owner = owner;
			folderName = folder;
			storage = new JolietIStorage(this, folderName);
		}

		/// <summary>
		/// Clears up any resources associated with this class.
		/// </summary>
		public void Dispose()
		{
			if (storage != null)
			{
				foreach (JolietDiscMasterStorage subStorage in subFolders.Values)
				{
					subStorage.Dispose();
				}
				storage.Dispose();
				storage = null;
			}
		}

		/// <summary>
		/// Clears any files associated with the storage
		/// </summary>
		public void Clear()
		{
			if (storage != null)
			{
				foreach (JolietDiscMasterStorage subStorage in subFolders.Values)
				{
					subStorage.Dispose();
				}
			}
			subFolders = new Hashtable();
			files = new Hashtable();
		}

		/// <summary>
		/// Gets the name of this folder.
		/// </summary>
		public String FolderName
		{
			get
			{
				return folderName;
			}
		}

		/// <summary>
		/// Create a sub folder
		/// </summary>
		/// <returns>Sub folder</returns>
		public JolietDiscMasterStorage CreateSubFolder(string folderName)
		{
			JolietDiscMasterStorage subFolder = new JolietDiscMasterStorage(owner, folderName);
			subFolders.Add(folderName, subFolder);
			return subFolder;
		}

		/// <summary>
		/// Add a file
		/// </summary>
		/// <param name="sourceFileName">Source file name</param>
		/// <param name="outputFileName">output file name</param>
		public void AddFile(string sourceFileName, string outputFileName)
		{
			files.Add(outputFileName, sourceFileName);
		}

		/// <summary>
		/// Gets an <c>IEnumerator</c> instance for the files contained
		/// within this storage instance.  The name returned is
		/// the name of the file on the disc.
		/// </summary>
		public IEnumerator Files
		{
			get
			{
				return files.Keys.GetEnumerator();
			}
		}

		/// <summary>
		/// Gets an <c>IEnumerator</c> instance for the sub-folders contained
		/// within this storage instance.  The name returned is
		/// the name of the subfolder on the disc.
		/// </summary>
		public IEnumerator Folders
		{
			get
			{
				return subFolders.Keys.GetEnumerator();
			}
		}

		internal IStorage GetIStorage()
		{
			return storage;
		}

		internal IStream RequestIStream(string name)
		{
			IStream returnValue = null;
			int cancel = 0;
			owner.QueryCancelRequest(out cancel);
			if (cancel == 0)
			{
				if (files.Contains(name))
				{
					string fileName = (string) files[name];
					try
					{
						FileStream fileStream = new FileStream(
							fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
						IStreamOnFileStream istream = new IStreamOnFileStream(fileStream, name);
						returnValue = istream;
					}
					catch (Exception ex)
					{
						Debug.WriteLine(String.Format("Exception trying to read file {0}: {1}", fileName, ex));						
					}
				}
			}
			return returnValue;
		}

		internal IStorage RequestIStorage(string name)
		{
			IStorage returnValue = null;
			int cancel = 0;
			owner.QueryCancelRequest(out cancel);
			if (cancel == 0)
			{
				if (subFolders.ContainsKey(name))
				{
					JolietDiscMasterStorage folder = (JolietDiscMasterStorage) subFolders[name];
					returnValue = folder.GetIStorage();
				}
				else if (name.Equals(folderName))
				{
					returnValue = storage;
				}
			}
			return returnValue;
		}


	}

}
