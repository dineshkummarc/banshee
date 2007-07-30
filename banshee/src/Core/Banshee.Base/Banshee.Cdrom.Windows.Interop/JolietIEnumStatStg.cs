using System;
using System.Runtime.InteropServices;
using System.Collections;

namespace Banshee.Cdrom.Windows.Interop
{
	/// <summary>
	/// Summary description for JolietIEnumStatStg.
	/// </summary>
	[Guid("90E97780-F7E9-4b46-9EF4-27E773459B92")]
	[ComVisible(true)]
	internal class JolietIEnumStatStg : IEnumSTATSTG, IDisposable
	{
		private readonly JolietDiscMasterStorage owner;
		private IEnumerator files;
		private IEnumerator subFolders;

		public JolietIEnumStatStg(JolietDiscMasterStorage owner)
		{
			this.owner = owner;
			files = owner.Files;
			subFolders = owner.Folders;
		}

		public void Dispose()
		{
			// nothing to do
		}

		public uint Next(
			int celt,
			ref STATSTG rgelt,
			out int pceltFetched)
		{
			uint hRes = (uint) GENERIC_ERROR_CODES.S_FALSE;
			int returned = 0;
			
			if (celt == 1)
			{
				if (files.MoveNext())
				{
					string name = (string) files.Current;
					IStream stream = owner.RequestIStream(name);
					stream.Stat(ref rgelt, STATFLAG.STATFLAG_DEFAULT);
					returned++;
					hRes = (uint) GENERIC_ERROR_CODES.S_OK;
				}
				else if (subFolders.MoveNext())
				{
					string name = (string) subFolders.Current;
					IStorage storage = owner.RequestIStorage(name);
					storage.Stat(out rgelt, STATFLAG.STATFLAG_DEFAULT);
					returned++;
					hRes = (uint) GENERIC_ERROR_CODES.S_OK;
				}
			}
			pceltFetched = returned;

			return hRes;
		}

		public uint Skip(
			int celt)
		{
			uint hRes = (uint) GENERIC_ERROR_CODES.S_FALSE;
			int skipped = 0;
			while (skipped < celt)
			{
				if (!files.MoveNext())
				{
					if (!subFolders.MoveNext())
					{
						break;
					}
				}
				skipped++;
			}
			
			if (skipped == celt)
			{
				hRes = (uint) GENERIC_ERROR_CODES.S_OK;
			}
			return hRes;
		}

		public uint Reset()
		{
			files = owner.Files;
			subFolders = owner.Folders;

			return (uint) GENERIC_ERROR_CODES.S_OK;
		}

		public uint Clone(
			out IEnumSTATSTG ppenum)
		{
			JolietIEnumStatStg clone = new JolietIEnumStatStg(owner);
			ppenum = clone;
			return (uint) GENERIC_ERROR_CODES.S_OK;
		}

	}
}
