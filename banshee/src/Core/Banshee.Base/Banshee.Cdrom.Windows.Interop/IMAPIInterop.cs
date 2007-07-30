using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Banshee.Cdrom.Windows.Interop
{

	#region Enumerations used within IMAPI and associated interfaces
	/// <summary>
	/// Property Set Flags
	/// </summary>
	[Flags]
    internal enum PROPSETFLAG : int 
	{
        PROPSETFLAG_DEFAULT    = 0,
        PROPSETFLAG_NONSIMPLE  = 1,
        PROPSETFLAG_ANSI       = 2,
        PROPSETFLAG_UNBUFFERED = 4
    }

	/// <summary>
	/// Property ID types
	/// </summary>
    internal enum PRPSPEC : uint 
	{
        PRSPEC_LPWSTR  = 0,
        PRSPEC_PROPID  = 1,
        PRSPEC_INVALID = 0xffffffff
    }

	/// <summary>
	/// Contexts for CoCreateInstance
	/// </summary>
    internal enum CLSCTX : int
    {
		CLSCTX_INPROC_SERVER   = 1, 
		CLSCTX_INPROC_HANDLER  = 2,     
		CLSCTX_LOCAL_SERVER    = 4, 
		CLSCTX_REMOTE_SERVER   = 16
	} ;

	/// <summary>
	/// CD Media Types
	/// </summary>
    public enum MEDIA_TYPES : int
    {
		/// <summary>
		/// CDDA CDROM media
		/// </summary>
		MEDIA_CDDA_CDROM	= 1,
		/// <summary>
		/// CD ROM XA media
		/// </summary>
		MEDIA_CD_ROM_XA		= 2,
		/// <summary>
		/// CD_I media
		/// </summary>
		MEDIA_CD_I			= 3,
		/// <summary>
		/// CD Extra media
		/// </summary>
		MEDIA_CD_EXTRA		= 4,
		/// <summary>
		/// CD Other media
		/// </summary>
		MEDIA_CD_OTHER		= 5,
		/// <summary>
		/// Special media
		/// </summary>
		MEDIA_SPECIAL		= 6		
    } 

	/// <summary>
	/// Flags describing media
	/// </summary>
	[Flags]
	public enum MEDIA_FLAGS : int
	{
		/// <summary>
		/// Blank media
		/// </summary>
    	MEDIA_BLANK	= 0x1,
		/// <summary>
		/// Read/Write media
		/// </summary>
		MEDIA_RW	= 0x2,
		/// <summary>
		/// Writable media
		/// </summary>
		MEDIA_WRITABLE	= 0x4,
		/// <summary>
		/// Unusable media
		/// </summary>
		MEDIA_FORMAT_UNUSABLE_BY_IMAPI	= 0x8
	}

	/// <summary>
	/// CD Recorder types
	/// </summary>
	public enum RECORDER_TYPES : int
	{
		/// <summary>
		/// CDR recorder
		/// </summary>
		RECORDER_CDR	= 0x1,
		/// <summary>
		/// CDRW recorder
		/// </summary>
		RECORDER_CDRW	= 0x2
	}

	/// <summary>
	/// State of a recorder on the system
	/// </summary>
	public enum RECORDER_STATE : int 
	{
		/// <summary>
		/// Recorder is idle.
		/// </summary>
		RECORDER_DOING_NOTHING = 0,
		/// <summary>
		/// Recorder is opened for exclusive access.
		/// </summary>
		RECORDER_OPENED = 0x1,
		/// <summary>
		/// Recorder is burning.
		/// </summary>
		RECORDER_BURNING = 0x2
	}

	/// <summary>
	/// Flags for IStorage
	/// </summary>
	[Flags]
	internal enum STGC  : int
	{
		STGC_DEFAULT = 0,
		STGC_OVERWRITE = 1,
		STGC_ONLYIFCURRENT = 2,
		STGC_DANGEROUSLYCOMMITMERELYTODISKCACHE = 4,
		STGC_CONSOLIDATE = 8
	}

	/// <summary>
	/// Storage instantiation modes
	/// </summary>
	[Flags]
	internal enum STGM : int
	{
		STGM_DIRECT = 0x00000000,
		STGM_TRANSACTED = 0x00010000,
		STGM_SIMPLE = 0x08000000,

		STGM_READ = 0x00000000,
		STGM_WRITE = 0x00000001,
		STGM_READWRITE = 0x00000002,

		STGM_SHARE_DENY_NONE = 0x00000040,
		STGM_SHARE_DENY_READ = 0x00000030,
		STGM_SHARE_DENY_WRITE = 0x00000020,
		STGM_SHARE_EXCLUSIVE = 0x00000010,

		STGM_PRIORITY = 0x00040000,
		STGM_DELETEONRELEASE = 0x04000000,
		STGM_NOSCRATCH = 0x00100000,

		STGM_CREATE = 0x00001000,
		STGM_CONVERT = 0x00020000,
		STGM_FAILIFTHERE = 0x00000000,

		STGM_NOSNAPSHOT = 0x00200000,
		STGM_DIRECT_SWMR = 0x00400000,
	}

	/// <summary>
	/// IStorage move and copy 
	/// </summary>
	internal enum STGMOVE : int
	{
		STGMOVE_MOVE = 0,
		STGMOVE_COPY = 1,
		STGMOVE_SHALLOWCOPY = 2
	}

	/// <summary>
	/// Status flags
	/// </summary>
	[Flags]
	internal enum STATFLAG : int
	{
	    STATFLAG_DEFAULT = 0,
		STATFLAG_NONAME = 1,
		STATFLAG_NOOPEN = 2
	}

	/// <summary>
	/// Storage type
	/// </summary>
	internal enum STGTY : int
	{
		STGTY_STORAGE = 1,
		STGTY_STREAM = 2,
		STGTY_LOCKBYTES = 3,
		STGTY_PROPERTY = 4
	}

	/// <summary>
	/// Lock types
	/// </summary>
	[Flags]
	internal enum LOCKTYPE : int
	{
		LOCK_WRITE = 1,
		LOCK_EXCLUSIVE = 2,
		LOCK_ONLYONCE = 4
	}

	/// <summary>
	/// Stream seeking options
	/// </summary>
	internal enum STREAM_SEEK : int
	{
		STREAM_SEEK_SET = 0,
		STREAM_SEEK_CUR = 1,
		STREAM_SEEK_END = 2
	}

	internal enum GENERIC_ERROR_CODES : uint
	{
		S_OK = 0,
		S_FALSE = 1,
		E_NOTIMPL = 0x80004001,
		E_FAIL = 0x80004005,
		E_UNEXPECTED = 0x8000FFFF
	}

	internal enum STG_ERROR_CONSTANTS : uint
	{
		STG_E_INVALIDFUNCTION = 0x80030001,
		STG_E_FILENOTFOUND = 0x80030002,
		STG_E_INVALIDPOINTER = 0x80030009,
		STG_E_INVALIDFLAG = 0x800300FF
	}

	/// <summary>
	/// Error codes returned by IMAPI.  These are thrown as <c>COMException</c>
	/// errors on calling IMAPI methods; the <c>ErrorCode</c> property of the 
	/// exception can be compared to these values. Note that the framework
	/// will fill in an arbitrary error description which is frequently
	/// inappropriate as these codes appear to be the same as error codes
	/// used elsewhere in COM.
	/// </summary>	
	public enum IMAPI_ERROR_CODES : uint
	{		
		/// <summary>
		/// An unknown property was passed in a property set and it was ignored.
		/// </summary>
		IMAPI_S_PROPERTIESIGNORED = 0x80040200,
		/// <summary>
		/// Buffer too small
		/// </summary>
		IMAPI_S_BUFFER_TOO_SMALL = 0x80040201,
		/// <summary>
		/// A call to IDiscMaster::Open has not been made.
		/// </summary>
		IMAPI_E_NOTOPENED = 0x8004020B,
		/// <summary>
		/// A recorder object has not been initialized.
		/// </summary>
		IMAPI_E_NOTINITIALIZED = 0x8004020C,
		/// <summary>
		/// The user canceled the operation.
		/// </summary>
		IMAPI_E_USERABORT = 0x8004020D,
		/// <summary>
		/// A generic error occurred.
		/// </summary>
		IMAPI_E_GENERIC = 0x8004020E,
		/// <summary>
		/// There is no disc in the device.
		/// </summary>
		IMAPI_E_MEDIUM_NOTPRESENT = 0x8004020F,
		/// <summary>
		/// The media is not a type that can be used.
		/// </summary>
		IMAPI_E_MEDIUM_INVALIDTYPE = 0x80040210,
		/// <summary>
		/// The recorder does not support any properties.
		/// </summary>
		IMAPI_E_DEVICE_NOPROPERTIES = 0x80040211,
		/// <summary>
		/// The device cannot be used or is already in use.
		/// </summary>
		IMAPI_E_DEVICE_NOTACCESSIBLE = 0x80040212,
		/// <summary>
		/// The device is not present or has been removed.
		/// </summary>
		IMAPI_E_DEVICE_NOTPRESENT = 0x80040213,
		/// <summary>
		/// The recorder does not support an operation.
		/// </summary>
		IMAPI_E_DEVICE_INVALIDTYPE = 0x80040214,
		/// <summary>
		/// The drive interface could not be initialized for writing.
		/// </summary>
		IMAPI_E_INITIALIZE_WRITE = 0x80040215,
		/// <summary>
		/// The drive interface could not be initialized for closing.
		/// </summary>
		IMAPI_E_INITIALIZE_ENDWRITE = 0x80040216,
		/// <summary>
		/// "An error occurred while enabling/disabling file system access or during auto-insertion detection.
		/// </summary>
		IMAPI_E_FILESYSTEM = 0x80040217,
		/// <summary>
		/// An error occurred while writing the image file.
		/// </summary>
		IMAPI_E_FILEACCESS = 0x80040218,
		/// <summary>
		/// An error occurred while trying to read disc data from the device.
		/// </summary>
		IMAPI_E_DISCINFO = 0x80040219,
		/// <summary>
		/// An audio track is not open for writing.
		/// </summary>
		IMAPI_E_TRACKNOTOPEN = 0x8004021A,
		/// <summary>
		/// An open audio track is already being staged.
		/// </summary>
		IMAPI_E_TRACKOPEN = 0x8004021B,
		/// <summary>
		/// The disc cannot hold any more data.
		/// </summary>
		IMAPI_E_DISCFULL = 0x8004021C,
		/// <summary>
		/// The application tried to add a badly named element to a disc.
		/// </summary>
		IMAPI_E_BADJOLIETNAME = 0x8004021D,
		/// <summary>
		/// The staged image is not suitable for a burn. It has been 
		/// corrupted or cleared and has no usable content.
		/// </summary>
		IMAPI_E_INVALIDIMAGE = 0x8004021E,
		/// <summary>
		/// An active format master has not been selected using 
		/// IDiscMaster::SetActiveDiscMasterFormat.
		/// </summary>
		IMAPI_E_NOACTIVEFORMAT = 0x8004021F,
		/// <summary>
		/// An active disc recorder has not been selected using 
		/// IDiscMaster::SetActiveDiscRecorder.
		/// </summary>
		IMAPI_E_NOACTIVERECORDER = 0x80040220,
		/// <summary>
		/// A call to IJolietDiscMaster has been made when IRedbookDiscMaster is 
		/// the active format, or vice versa. To use a different format, change 
		/// the format and clear the image file contents.
		/// </summary>
		IMAPI_E_WRONGFORMAT = 0x80040221,
		/// <summary>
		/// A call to IDiscMaster::Open has already been made against this 
		/// object by your application.
		/// </summary>
		IMAPI_E_ALREADYOPEN = 0x80040222,
		/// <summary>
		/// The IMAPI multi-session disc has been removed from the active recorder.
		/// </summary>
		IMAPI_E_WRONGDISC = 0x80040223,
		/// <summary>
		/// The file to add is already in the image file and the overwrite 
		/// flag was not set.
		/// </summary>
		IMAPI_E_FILEEXISTS = 0x80040224,
		/// <summary>
		/// Another application is already using the IMAPI stash file required to 
		/// stage a disc image. Try again later.
		/// </summary>
		IMAPI_E_STASHINUSE = 0x80040225,
		/// <summary>
		/// Another application is already using this device, so IMAPI cannot 
		/// access the device.
		/// </summary>
		IMAPI_E_DEVICE_STILL_IN_USE = 0x80040226,
		/// <summary>
		/// Content streaming was lost; a buffer under-run may have occurred.
		/// </summary>
		IMAPI_E_LOSS_OF_STREAMING = 0x80040227,
		/// <summary>
		/// The stash is located on a compressed volume and cannot be read.
		/// </summary>
		IMAPI_E_COMPRESSEDSTASH = 0x80040228,
		/// <summary>
		/// The stash is located on an encrypted volume and cannot be read.
		/// </summary>
		IMAPI_E_ENCRYPTEDSTASH = 0x80040229,
		/// <summary>
		/// There is not enough free space to create the stash file on the specified volume.
		/// </summary>
		IMAPI_E_NOTENOUGHDISKFORSTASH = 0x8004022A,
		/// <summary>
		/// The selected stash location is on a removable media.
		/// </summary>
		IMAPI_E_REMOVABLESTASH = 0x8004022B,
		/// <summary>
		/// Cannot write to the media.
		/// </summary>
		IMAPI_E_CANNOT_WRITE_TO_MEDIA = 0x8004022C,
		/// <summary>
		/// Track was too short.
		/// </summary>
		IMAPI_E_TRACK_NOT_BIG_ENOUGH = 0x8004022D,
		/// <summary>
		/// Attempt to create a bootable image on a non-blank disc.
		/// </summary>
		IMAPI_E_BOOTIMAGE_AND_NONBLANK_DISC = 0x8004022E
	}

	#endregion

	#region Interop Structures
	/// <summary>
	/// STATPROPSTG
	/// </summary>
	[StructLayoutAttribute(LayoutKind.Sequential)]
	internal struct STATPROPSTG 
	{
        public IntPtr lpwstrName;
        public int propid;
        public short vt;
	}

	/// <summary>
	/// PROPSPEC
	/// </summary>
	[StructLayoutAttribute(LayoutKind.Sequential)]
    internal struct PROPSPEC 
	{
        public PRPSPEC ulKind;
        public IntPtr ID_or_LPWSTR;
    }

	/// <summary>
	/// STATPROPSETSTG
	/// </summary>
	[StructLayoutAttribute(LayoutKind.Sequential)]
    internal struct STATPROPSETSTG 
	{
        public Guid fmtid;
        public Guid clsid;
        public int grfFlags;
        public long mtime;
        public long ctime;
        public long atime;
        public int dwOSVersion;
    }

	/// <summary>
	/// STATSTG
	/// </summary>
	[StructLayoutAttribute(LayoutKind.Sequential)]
	internal struct STATSTG
	{
		public IntPtr pwcsName;
		public STGTY type;
		public long cbSize;
		public long mtime;
		public long ctime;
		public long atime;
		public STGM grfMode;
		public LOCKTYPE grfLocksSupported;
		public Guid clsid;
		public int grfStateBits;
		public int reserved;
	}
	#endregion

	#region Com Interop for IUnknown	
	/// <summary>
	/// IUnknown Interface 
	/// </summary>
	[ComImport, Guid("00000000-0000-0000-C000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IUnknown
	{
		[PreserveSig]
		IntPtr QueryInterface(ref Guid riid, out IntPtr pVoid);
		
		[PreserveSig]
		IntPtr AddRef();

		[PreserveSig]
		IntPtr Release();
	}
	#endregion

	#region COM Interop for IMalloc
	[ComImportAttribute()]
	[GuidAttribute("00000002-0000-0000-C000-000000000046")]
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	//helpstring("IMalloc interface")
	internal interface IMalloc
	{
		[PreserveSig]
		IntPtr Alloc(int cb);

		[PreserveSig]
		IntPtr Realloc(
			IntPtr pv,
			int cb);
	
		[PreserveSig]
		void Free(IntPtr pv);

		[PreserveSig]
		int GetSize(IntPtr pv);

		[PreserveSig]
		int DidAlloc(IntPtr pv);

		[PreserveSig]
		void  HeapMinimize();
	};
	#endregion

	#region COM Interop for IEnumSTATPROPSTG
	/// <summary>
	/// IEnumSTATPROPSTG interface
	/// </summary>
	[ComImportAttribute()]
	[GuidAttribute("00000139-0000-0000-C000-000000000046")]
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IEnumSTATPROPSTG 
	{
		[PreserveSig]
		int Next(
			int celt, 
			ref STATPROPSTG pSTATPROPSTG,
			out int pceltFetched);

		void Skip(
			int celt);

		void Reset();

		void Clone(
			ref IEnumSTATPROPSTG ppenum);
	}
	#endregion

	#region COM Interop for IPropertyStorage
	/// <summary>
	/// IPropertyStorage interface
	/// </summary>
	[ComImportAttribute()]
	[GuidAttribute("00000138-0000-0000-C000-000000000046")]
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPropertyStorage 
	{
        void ReadMultiple(
            int cpspec,
            ref PROPSPEC rgpspec,
			[MarshalAs(UnmanagedType.Struct)]
            out object rgpropvar);

        void WriteMultiple(
            int cpspec,
            ref PROPSPEC rgpspec,
			[MarshalAs(UnmanagedType.Struct)]
            ref object rgPropvar,
            int propidNameFirst);

        void DeleteMultiple(
            int cpspec,
            ref PROPSPEC rgpspec);

        void ReadPropertyNames(
            int cpropid,
            [In()]
			ref int rgpropidp,
            [Out(), MarshalAs(UnmanagedType.LPWStr)] 
			out string rglpwstrName);

        void WritePropertyNames(
            int cpropid,
            ref int rgpropid,
			[In(), MarshalAs(UnmanagedType.LPWStr)]
            ref string rglpwstrName);

        void DeletePropertyNames(
            int cpropid,
			[In()]
            ref int rgpropid);

        void Commit(
            ref STGC grfCommitFlags);

        void Revert();

        void Enum(
            ref IEnumSTATPROPSTG ppenum);

        void SetTimes(
            [In()]
			ref long pctime,
			[In()]
            ref long patime,
			[In()]
            ref long pmtime);

        void SetClass(
            ref Guid clsid);

        void Stat(
            out STATPROPSETSTG pstatpsstg);
    }
	#endregion

	#region COM Interop for IStream
	/// <summary>
	/// IStream interface
	/// </summary>
	[ComImportAttribute()]
	[GuidAttribute("0000000c-0000-0000-C000-000000000046")]
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	interface IStream 
	{

		[PreserveSig()]
        uint Read(
            IntPtr pv,
            int cb,
            ref int pcbRead);

		[PreserveSig()]
        uint Write(
            IntPtr pv,
            int cb,
            ref int pcbWritten);

		[PreserveSig()]
        uint Seek(
            long dlibMove,
            STREAM_SEEK dwOrigin,
            ref long plibNewPosition);

		[PreserveSig()]
        uint SetSize(
            long libNewSize);

		[PreserveSig()]
        uint CopyTo(
			[In()]
            ref IStream pstm,
            long cb,
            ref long pcbRead,
            ref long pcbWritten);

		[PreserveSig()]
        uint Commit(
            ref STGC grfCommitFlags);

		[PreserveSig()]
        uint Revert();

		[PreserveSig()]
        uint LockRegion(
            long libOffset,
            long cb,
            int dwLockType);

		[PreserveSig()]
        uint UnlockRegion(
            long libOffset,
            long cb,
            int dwLockType);

		[PreserveSig()]
        uint Stat(
            ref STATSTG pstatstg,
            STATFLAG grfStatFlag);
		
		[PreserveSig()]
        uint Clone(
            out IStream ppstm);
    }
	#endregion

	#region COM Interop for IEnumSTATSTG
	/// <summary>
	/// IEnumSTATSTG interface.
	/// </summary>
	[ComImportAttribute()]
	[GuidAttribute("0000000d-0000-0000-C000-000000000046")]
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IEnumSTATSTG 
	{

		[PreserveSig()]
		uint Next(
			int celt,
			ref STATSTG rgelt,
			out int pceltFetched);

		[PreserveSig()]
		uint Skip(
			int celt);

		[PreserveSig()]
		uint Reset();

		[PreserveSig()]
		uint Clone(
			out IEnumSTATSTG ppenum);
	}
	#endregion

	#region COM Interop for IStorage
	/// <summary>
	/// IStorage interface
	/// </summary>
	[ComImportAttribute()]
	[GuidAttribute("0000000b-0000-0000-c000-000000000046")]
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IStorage 
	{

		[PreserveSig()]
        uint CreateStream(
			[MarshalAs(UnmanagedType.LPWStr)]
            string pwcsName,
            STGM grfMode,
            int reserved1,
            int reserved2,
            out IStream ppstm);

		[PreserveSig()]
        uint OpenStream(
			[MarshalAs(UnmanagedType.LPWStr)]
            string pwcsName,
            int reserved1,
            STGM grfMode,
            int reserved2,
            out IStream ppstm);

		[PreserveSig()]
        uint CreateStorage(
			[MarshalAs(UnmanagedType.LPWStr)]
            string pwcsName,
            STGM grfMode,
            int reserved1,
            int reserved2,
            out IStorage ppstg);

		[PreserveSig()]
        uint OpenStorage(
			[MarshalAs(UnmanagedType.LPWStr)]
            string pwcsName,
            int pstgPriority,
            STGM grfMode,
            int snbExclude,
            int reserved,
            out IStorage ppstg);

		[PreserveSig()]
        uint CopyTo(
            int ciidExclude,
            ref Guid rgiidExclude,
            int snbExclude,			
			[In()]
            ref IStorage pstgDest);

		[PreserveSig()]
        uint MoveElementTo(
			[MarshalAs(UnmanagedType.LPWStr)]
            string pwcsName,
			[In()]
            ref IStorage pstgDest,
			[MarshalAs(UnmanagedType.LPWStr)]
            string pwcsNewName,
            STGMOVE grfFlags);

		[PreserveSig()]
        uint Commit(
            STGC grfCommitFlags);

		[PreserveSig()]
        uint Revert();

		[PreserveSig()]
        uint EnumElements(
            int reserved1,
            int reserved2,
            int reserved3,
            out IEnumSTATSTG ppenum);

		[PreserveSig()]
        uint DestroyElement(
			[MarshalAs(UnmanagedType.LPWStr)]
            string pwcsName);

		[PreserveSig()]
        uint RenameElement(
			[MarshalAs(UnmanagedType.LPWStr)]
            string pwcsOldName,
			[MarshalAs(UnmanagedType.LPWStr)]
            string pwcsNewName);

		[PreserveSig()]
        uint SetElementTimes(
			[MarshalAs(UnmanagedType.LPWStr)]
            string pwcsName,
			[In()]
            ref long pctime,
			[In()]
            ref long patime,
			[In()]
            ref long pmtime);

		[PreserveSig()]
        uint SetClass(
            [In()]
			ref Guid clsid);

		[PreserveSig()]
        uint SetStateBits(
            int grfStateBits,
            int grfMask);

		[PreserveSig()]
        uint Stat(
            out STATSTG pstatstg,
            STATFLAG grfStatFlag);
    }
	#endregion

	/// <summary>
	/// IDiscRecorder 
	/// </summary>
	[ComImportAttribute()]
	[GuidAttribute("85AC9776-CA88-4cf2-894E-09598C078A41")]
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IDiscRecorder
	{
		/// <summary>
		/// Initializes the object for an underlying device.  Used internally only.
		/// </summary>				
		void Init(
			ref IntPtr pbyUniqueID,
			int nulIDSize,
			int nulDriveNumber);

		/// <summary>
		/// Retrieves the underlying device GUID.
		/// </summary>
		/// <param name="pbyUniqueID"></param>
		/// <param name="ulBufferSize"></param>
		/// <param name="pulReturnSizeRequired"></param>
		void GetRecorderGUID(
			ref IntPtr pbyUniqueID,
			int ulBufferSize,
			out int pulReturnSizeRequired);

		/// <summary>
		/// Identifies the device as CD-R or CD-RW
		/// </summary>
		/// <param name="fTypeCode"></param>
		void GetRecorderType(
			out int fTypeCode);

		/// <summary>
		/// Retrieves a name suitable for GUI display
		/// </summary>
		void GetDisplayNames(
			[MarshalAs(UnmanagedType.BStr)] ref string pbstrVendorID,
			[MarshalAs(UnmanagedType.BStr)] ref string pbstrProductID,
			[MarshalAs(UnmanagedType.BStr)] ref string pbstrRevision);

		/// <summary>
		/// Returns an identifier unique to the device class
		/// </summary>
		/// <param name="pbstrPath"></param>
		void GetBasePnPID(
			[MarshalAs(UnmanagedType.BStr)] out string pbstrPath);

		/// <summary>
		/// Returns an OS Path to the device
		/// </summary>
		/// <param name="pbstrPath"></param>
		void GetPath(
			[MarshalAs(UnmanagedType.BStr)] out string pbstrPath);

		/// <summary>
		/// Retrieves a pointer to the IPropertyStorage interface for the recorder
		/// </summary>
		/// <param name="ppPropStg"></param>
		void GetRecorderProperties(
			out IPropertyStorage ppPropStg);

		/// <summary>
		/// Sets properties for the recorder
		/// </summary>
		void SetRecorderProperties(
			[In()]
			ref IPropertyStorage ppPropStg);

		/// <summary>
		/// Checks if the recorder is ready to burn
		/// </summary>
		void GetRecorderState(
			out int pulDevStateFlags);

		/// <summary>
		/// Opens a device for exclusive use
		/// </summary>
		void OpenExclusive();

		/// <summary>
		/// Identifies the type of media in the recorder
		/// </summary>
		void QueryMediaType(
			out int fMediaType,
			out int fMediaFlags);

		/// <summary>
		/// Retrieves the media properties
		/// </summary>
		void QueryMediaInfo(
			out byte pbSessions,
			out byte pbLastTrack,
			out int ulStartAddress,
			out int ulNextWritable,
			out int ulFreeBlocks);

		/// <summary>
		/// Ejects a recorder's tray, if possible
		/// </summary>
		void Eject();

		/// <summary>
		/// Erases CD-RW media, if possible
		/// </summary>
		void Erase(
			int bFulLErase);

		/// <summary>
		/// Closes a recorder after exclusive access
		/// </summary>
		void Close();

	}

	/// <summary>
	/// IEnumDiscMasterFormats interface 
	/// </summary>
	[ComImportAttribute()]
	[GuidAttribute("DDF445E1-54BA-11d3-9144-00104BA11C5E")]
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IEnumDiscMasterFormats
	{
		[PreserveSig()]
		int Next(
			int cFormats,
			out Guid lpiidFormatID,
			out int pcFetched);

		void Skip(
			int cFormats);

		void Reset();

		void Clone(
			out IEnumDiscMasterFormats ppEnum);
	}

	/// <summary>
	/// IEnumDiscRecorders interface
	/// </summary>
	[ComImportAttribute()]
	[GuidAttribute("9B1921E1-54AC-11d3-9144-00104BA11C5E")]
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IEnumDiscRecorders
	{
		[PreserveSig()]
		int Next(
			int cRecorders,
			out IDiscRecorder ppRecorder,
			out int pcFetched);

		void Skip(
			int cRecorders);

		void Reset();

		void Clone(
			out IEnumDiscRecorders ppEnum);
	}

	/// <summary>
	/// IDiscMasterProgressEvents interface
	/// </summary>
	[ComImportAttribute()]
	[GuidAttribute("EC9E51C1-4E5D-11D3-9144-00104BA11C5E")]	
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IDiscMasterProgressEvents
	{
		/// <summary>
		/// Called to request whether the burn event should be cancelled
		/// </summary>
		void QueryCancel(
			out int pbCancel);

		/// <summary>
		/// Notifies that a Plug and Play activity has occurred that has changed the list of recorders.
		/// </summary>
		void NotifyPnPActivity();

		/// <summary>
		/// Notifies addition of data to the CD image in the stash.
		/// </summary>
		void NotifyAddProgress(
			int nCompleted,
			int nTotal);

		/// <summary>
		/// Notifies an application of block progress whilst burning a disc.
		/// </summary>
		void NotifyBlockProgress(
			int nCurrentBlock,
			int nTotalBlocks);

		/// <summary>
		/// Notifies an application of track progress whilst burning an audio disc.
		/// </summary>
		void NotifyTrackProgress(
			int nCurrentTrack,
			int nTotalTracks);

		/// <summary>
		/// Notifies an application that IMAPI is preparing to burn a disc.
		/// </summary>
		void NotifyPreparingBurn(
			int nEstimatedSeconds);

		/// <summary>
		/// Notifies an application that IMAPI is closing a disc.
		/// </summary>
		void NotifyClosingDisc(
			int nEstimatedSeconds);

		/// <summary>
		/// Notifies an application that IMAPI has completed burning a disc.
		/// </summary>
		void NotifyBurnComplete(
			int status);

		/// <summary>
		/// Notifies an application that IMAPI has completed erasing a disc.
		/// </summary>
		void NotifyEraseComplete(
			int status);
	}

	/// <summary>
	/// IDiscMaster interface
	/// </summary>
	[ComImportAttribute()]
	[GuidAttribute("520CCA62-51A5-11D3-9144-00104BA11C5E")]
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IDiscMaster
	{
		/// <summary>
		/// Opens an IMAPI object
		/// </summary>
		void Open();

		/// <summary>
		/// Retrieves a format enumerator
		/// </summary>
		void EnumDiscMasterFormats(
			out IEnumDiscMasterFormats ppEnum);

		/// <summary>
		/// Retrieves the currently selected recorder format
		/// </summary>
		void GetActiveDiscMasterFormat(
			out Guid lpiid);

		/// <summary>
		/// Sets a new active recorder format
		/// </summary>
		void SetActiveDiscMasterFormat(
			[In()]
			ref Guid riid,
			[MarshalAs(UnmanagedType.Interface)]
			out object ppUnk);

		/// <summary>
		/// Retrieves a recorder enumerator
		/// </summary>
		void EnumDiscRecorders(
			out IEnumDiscRecorders ppEnum);

		/// <summary>
		/// Gets the active disc recorder
		/// </summary>
		void GetActiveDiscRecorder(
			out IDiscRecorder ppRecorder);

		/// <summary>
		/// Sets the active disc recorder
		/// </summary>
		void SetActiveDiscRecorder(
			IDiscRecorder pRecorder);

		/// <summary>
		/// Clears the contents of an unburnt image
		/// </summary>
		void ClearFormatContent();

		/// <summary>
		/// Registers for progress notifications
		/// </summary>
		void ProgressAdvise(
			IDiscMasterProgressEvents pEvents,
			out IntPtr pvCookie);

		/// <summary>
		/// Cancels progress notifications
		/// </summary>
		void ProgressUnadvise(
			IntPtr vCookie);

		/// <summary>
		/// Burns the staged image to the active recorder
		/// </summary>
		void RecordDisc(
			int bSimulate,
			int bEjectAfterBurn);

		/// <summary>
		/// Closes the interface
		/// </summary>
		void Close();

	}

	/// <summary>
	/// IRedbookDiscMaster interface
	/// </summary>
	[ComImportAttribute()]
	[GuidAttribute("E3BC42CD-4E5C-11D3-9144-00104BA11C5E")]
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IRedbookDiscMaster
	{
		/// <summary>
		/// Gets the total number of audio tracks
		/// </summary>
		void GetTotalAudioTracks(
			out int pnTracks);

		/// <summary>
		/// Gets the total number of audio blocks
		/// </summary>
		void GetTotalAudioBlocks(
			out int pnBlocks);

		/// <summary>
		/// Gets the used number of audio blocks
		/// </summary>
		void GetUsedAudioBlocks(
			out int pnBlocks);

		/// <summary>
		/// Gets the number of available audio track blocks
		/// </summary>
		void GetAvailableAudioTrackBlocks(
			out int pnBlocks);

		/// <summary>
		/// Gets the size of an audio block in bytes
		/// </summary>
		void GetAudioBlockSize(
			out int pnBlockBytes);

		/// <summary>
		/// Creates a new audio track in the staging area
		/// </summary>
		void CreateAudioTrack(
			int nBlocks);

		/// <summary>
		/// Adds a block to the current audio track
		/// </summary>
		void AddAudioTrackBlocks(
			IntPtr cb,
			int pby);

		/// <summary>
		/// Closes the current audio track
		/// </summary>
		void CloseAudioTrack();
	}
	
	/// <summary>
	/// IJolietDiscMaster interface
	/// </summary>
	[ComImportAttribute()]
	[GuidAttribute("E3BC42CE-4E5C-11D3-9144-00104BA11C5E")]
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IJolietDiscMaster
	{
		/// <summary>
		/// Gets the total number of data blocks
		/// </summary>
		void GetTotalDataBlocks(
			out int pnBlocks);

		/// <summary>
		/// Gets the number of used data blocks
		/// </summary>
		void GetUsedDataBlocks(
			out int pnBlocks);

		/// <summary>
		/// Gets the size of a data block
		/// </summary>
		void GetDataBlockSize(
			out int pnBlockBytes);

		/// <summary>
		/// Adds data from an IStorage interface
		/// </summary>
		void AddData(
			IStorage pStorage,
			int lFileOverwrite);

		/// <summary>
		/// Gets the properties of the Joliet writer
		/// </summary>
		void GetJolietProperties(
			out IPropertyStorage ppPropStg);

		/// <summary>
		/// Sets the properties of the Joliet writer
		/// </summary>
		void SetJolietProperties(
			[In()]
			ref IPropertyStorage pPropStg);
	}


	#region IMAPIObjectFactory class
	/// <summary>
	/// A factory for instantiating IMAPI and ICDBurn objects.
	/// </summary>
	internal class IMAPIObjectFactory
	{
		private const string CLSID_CDBURN = "fbeb8a05-beee-4442-804e-409d6c4515e9";
		private const string IID_CDBURN   = "3d73a659-e5d0-4d42-afc0-5121ba425c8d";

		private const string CLSID_MSDiscMasterObj = "520CCA63-51A5-11D3-9144-00104BA11C5E";
		private const string IID_IDiscMaster = "520CCA62-51A5-11D3-9144-00104BA11C5E";

			
		[DllImport("ole32", CharSet = CharSet.Unicode)]
		private extern static int CoCreateInstance(
			ref Guid CLSID,
			IntPtr pUnkOuter,
			CLSCTX dwClsContext,
			ref Guid IID,
			[MarshalAs(UnmanagedType.Interface)]
			out object ppv);

		/// <summary>
		/// Should not be able to instantiate this class
		/// </summary>
		private IMAPIObjectFactory()
		{
		}

		/// <summary>
		/// Creates a new instance of the <c>IDiscMaster</c> implementation
		/// on this system, if the system supports it.
		/// </summary>
		/// <returns>Implementating intance of <c>IDiscMaster</c></returns>
		/// <exception cref="COMException">if the system does not have an
		/// <c>IDiscMaster</c> implementation.</exception>
		public static IDiscMaster CreateDiscMaster()
		{
			object discMaster = null;
			Guid clsIdDiscMaster = new Guid(CLSID_MSDiscMasterObj);
			Guid iidDiscMaster = new Guid(IID_IDiscMaster);
			int hResult = IMAPIObjectFactory.CoCreateInstance(
				ref clsIdDiscMaster, IntPtr.Zero, CLSCTX.CLSCTX_INPROC_SERVER | CLSCTX.CLSCTX_LOCAL_SERVER, 
				ref iidDiscMaster, out discMaster);
			if (ComUtility.Failed(hResult))
			{
				throw new COMException("Failed to instantiate the IDiscMaster implementation", 
					hResult);
			}
			return (IDiscMaster) discMaster;
		}

	}
	#endregion

	[StructLayout(LayoutKind.Explicit)]
	internal struct Variant
	{
		[FieldOffset(0)]
		public short vt;
		[FieldOffset(8)]
		public IntPtr ptr;
		[FieldOffset(8)]
		public byte Byte;
		[FieldOffset(8)]
		public long Long;

		/// <summary>
		/// Converts an object to a variant in this structure
		/// </summary>
		/// <param name="o">Object to convert</param>
		public void ForObject(object o)
		{
			if (o is string)
			{
				vt = (short) VarEnum.VT_LPWSTR;
				ptr = Marshal.StringToHGlobalUni((string) o);
			}
			else if (o is DateTime)
			{
				vt = (short) VarEnum.VT_DATE;
				Long = ((DateTime) o).ToFileTime();
			}
			else if (o is bool)
			{
				vt = (short) VarEnum.VT_BOOL;
				Byte = (byte) ((bool) o ? 1 : 0);
			}
			else if (o is byte)
			{
				vt = (short) VarEnum.VT_UI1;
				Byte = (byte) o;
			}
			else if (o is short)
			{
				vt = (short) VarEnum.VT_UI2;
				ptr = (IntPtr) o;
			}
			else if (o is int)
			{
				vt = (short) VarEnum.VT_I4;
				ptr = (IntPtr) o;
			}
			else if (o is long)
			{
				vt = (short) VarEnum.VT_I8;
				Long = (long) o;
			}
			else
			{
				throw new ArgumentException(
					String.Format("Unsupported variant type {0}", vt));
			}
		}

		/// <summary>
		/// Converts a variant to an object
		/// </summary>
		/// <returns></returns>
		public object ToObject()
		{
			object ret = null;

			switch ((VarEnum) vt)
			{
				case VarEnum.VT_BSTR:
					ret = Marshal.PtrToStringBSTR(ptr);
					Marshal.FreeCoTaskMem(ptr);
					break;
				case VarEnum.VT_UI1:
					ret = Byte;
					break;
				case VarEnum.VT_UI2:
					ret = (short) Long;
					break;
				case VarEnum.VT_I4:
					ret = (int) ptr;
					break;
				case VarEnum.VT_UI8:
					ret = Long;
					break;
				case VarEnum.VT_INT:
					ret = (short) Long;
					break;
				case VarEnum.VT_LPSTR:
					ret = Marshal.PtrToStringAnsi(ptr);
					Marshal.FreeCoTaskMem(ptr);
					break;
				case VarEnum.VT_LPWSTR:
					ret = Marshal.PtrToStringUni(ptr);
					Marshal.FreeCoTaskMem(ptr);
					break;
				case VarEnum.VT_FILETIME:
					ret = DateTime.FromFileTime(Long);
					break;
				case VarEnum.VT_BOOL:
					ret = (Byte == 0 ? false : true);
					break;
				case VarEnum.VT_NULL:
				case VarEnum.VT_EMPTY:
					break;
				default:
					throw new ArgumentException(
						String.Format("Unsupported variant type {0}", vt));
			}

			return ret;
		}

		/// <summary>
		/// Clears up any resources associated with the structure
		/// </summary>
		public void Clear()
		{
			if ((vt == (short) VarEnum.VT_LPSTR) 
				|| (vt == (short) VarEnum.VT_LPWSTR) 
				|| (vt == (short) VarEnum.VT_BSTR))
			{
				Marshal.FreeHGlobal(ptr);
			}
			else
			{
				ComUtility.VariantClear(this);
			}
		}
	}

	internal class ComUtility
	{
		private const uint FAIL_BIT = 0x80000000;

		private ComUtility()
		{
		}

		[DllImport("ole32")]
		public extern static int VariantClear(Variant vt);

		public static bool Failed(int hResult)
		{
			return (((uint) hResult & FAIL_BIT) == FAIL_BIT);
		}

	}	

}
