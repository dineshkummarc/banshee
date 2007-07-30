using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Banshee.Cdrom.Windows.Interop
{
	/// <summary>
	/// Implementation of IStorage for the JolietDiscMaster interface.
	/// </summary>
	[Guid("CD79B02B-DB7C-4bed-B169-C0A71804089E")]
	[ComVisible(true)]
	internal class JolietIStorage : IStorage, IDisposable
	{
		private readonly JolietDiscMasterStorage owner;
		private readonly string name;
		private JolietIEnumStatStg enumStatStg = null;
		
		public JolietIStorage(JolietDiscMasterStorage owner, string name)
		{
			this.owner = owner;
			this.name = name;
		}

		public void Dispose()
		{
			if (enumStatStg != null)
			{
				enumStatStg.Dispose();
				enumStatStg = null;
			}
		}

		public uint CreateStream(
			string pwcsName,
			STGM grfMode,
			int reserved1,
			int reserved2,
			out IStream ppstm)
		{
			uint hRes = (uint) GENERIC_ERROR_CODES.E_NOTIMPL;
			ppstm = null;
			return hRes;
		}

		public uint OpenStream(
			string pwcsName,
			int reserved1,
			STGM grfMode,
			int reserved2,
			out IStream ppstm)
		{
			uint hRes = (uint) GENERIC_ERROR_CODES.E_FAIL;
			ppstm = null;

			// Request the IStream for this name:
			Debug.WriteLine(String.Format("Requesting stream for {0}", pwcsName));
			IStream stream = owner.RequestIStream(pwcsName);
			if (stream != null)
			{
				ppstm = stream;
				hRes = (uint) GENERIC_ERROR_CODES.S_OK;
			}			

			return hRes;
		}

		public uint CreateStorage(
			string pwcsName,
			STGM grfMode,
			int reserved1,
			int reserved2,
			out IStorage ppstg)
		{
			uint hRes = (uint) GENERIC_ERROR_CODES.E_NOTIMPL;
			ppstg = null;
			return hRes;
		}

		public uint OpenStorage(
			string pwcsName,
			int pstgPriority,
			STGM grfMode,
			int snbExclude,
			int reserved,
			out IStorage ppstg)
		{
			uint hRes = (uint) GENERIC_ERROR_CODES.E_FAIL;
			ppstg = null;

			// Request the IStorage for this name:
			Debug.WriteLine(String.Format("Requesting IStorage for {0}", pwcsName));
			IStorage storage = owner.RequestIStorage(pwcsName);
			if (storage != null)
			{
				ppstg = storage;
				hRes = (uint) GENERIC_ERROR_CODES.S_OK;
			}			

			return hRes;
		}

		public uint CopyTo(
			int ciidExclude,
			ref Guid rgiidExclude,
			int snbExclude,			
			ref IStorage pstgDest)
		{
			uint hRes = (uint) GENERIC_ERROR_CODES.E_NOTIMPL;
			return hRes;
		}

		public uint MoveElementTo(
			string pwcsName,
			ref IStorage pstgDest,
			string pwcsNewName,
			STGMOVE grfFlags)
		{
			uint hRes = (uint) GENERIC_ERROR_CODES.E_NOTIMPL;
			return hRes;
		}

		public uint Commit(
			STGC grfCommitFlags)
		{
			uint hRes = (uint) GENERIC_ERROR_CODES.E_NOTIMPL;
			return hRes;
		}

		public uint Revert()
		{
			uint hRes = (uint) GENERIC_ERROR_CODES.E_NOTIMPL;
			return hRes;
		}

		public uint EnumElements(
			int reserved1,
			int reserved2,
			int reserved3,
			out IEnumSTATSTG ppenum)
		{
			Debug.WriteLine(String.Format("Requesting storage enumerator for {0}", name));
			uint hRes = (uint) GENERIC_ERROR_CODES.S_OK;
			enumStatStg = new JolietIEnumStatStg(owner);
			ppenum = enumStatStg;
			return hRes;
		}

		public uint DestroyElement(
			string pwcsName)
		{
			uint hRes = (uint) GENERIC_ERROR_CODES.E_NOTIMPL;
			return hRes;
		}

		public uint RenameElement(
			string pwcsOldName,
			string pwcsNewName)
		{
			uint hRes = (uint) GENERIC_ERROR_CODES.E_NOTIMPL;
			return hRes;
		}

		public uint SetElementTimes(
			string pwcsName,
			ref long pctime,
			ref long patime,
			ref long pmtime)
		{
			uint hRes = (uint) GENERIC_ERROR_CODES.E_NOTIMPL;
			return hRes;
		}

		public uint SetClass(
			ref Guid clsid)
		{
			uint hRes = (uint) GENERIC_ERROR_CODES.E_NOTIMPL;
			return hRes;
		}

		public uint SetStateBits(
			int grfStateBits,
			int grfMask)
		{
			uint hRes = (uint) GENERIC_ERROR_CODES.E_NOTIMPL;
			return hRes;
		}

		public uint Stat(
			out STATSTG pstatstg,
			STATFLAG grfStatFlag)
		{
			uint hRes = (uint) GENERIC_ERROR_CODES.S_OK;
			
			pstatstg = new STATSTG();
			if (grfStatFlag != STATFLAG.STATFLAG_NONAME )
			{
				pstatstg.pwcsName = Marshal.StringToCoTaskMemUni(name);
			}
			pstatstg.type = STGTY.STGTY_STORAGE;
			pstatstg.cbSize = Marshal.SizeOf(typeof(STATSTG));			

			return hRes;
		}

		public override string ToString()
		{
			return String.Format("{0} {1}", GetType().FullName, name);
		}

	}
}
