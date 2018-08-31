#if WIN
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Tangerine.UI.FilesystemView
{
	internal static class WinAPI
	{
		[ComImport()]
		[Guid("000214F2-0000-0000-C000-000000000046")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		public interface IEnumIDList
		{
			[PreserveSig()]
			int Next(int celt, ref IntPtr rgelt, ref int pceltFetched);

			void Skip(int celt);
			void Reset();
			void Clone(ref IEnumIDList ppenum);
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0, CharSet = CharSet.Auto)]
		public struct STRRET_CSTR
		{
			public ESTRRET uType;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 520)]
			public byte[] cStr;
		}

		[ComImport(), Guid("00000000-0000-0000-C000-000000000046")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		public interface IUnknown
		{
			[PreserveSig()]
			IntPtr QueryInterface(ref Guid riid, ref IntPtr pVoid);

			[PreserveSig()]
			IntPtr AddRef();

			[PreserveSig()]
			IntPtr Release();
		}

		[Flags]
		public enum SFGAO : uint
		{
			SFGAO_CANCOPY = 0x1,
			SFGAO_CANMOVE = 0x2,
			SFGAO_CANLINK = 0x4,
			SFGAO_STORAGE = 0x00000008,
			SFGAO_CANRENAME = 0x00000010,
			SFGAO_CANDELETE = 0x00000020,
			SFGAO_HASPROPSHEET = 0x00000040,
			SFGAO_DROPTARGET = 0x00000100,
			SFGAO_CAPABILITYMASK = 0x00000177,
			SFGAO_ENCRYPTED = 0x00002000,
			SFGAO_ISSLOW = 0x00004000,
			SFGAO_GHOSTED = 0x00008000,
			SFGAO_LINK = 0x00010000,
			SFGAO_SHARE = 0x00020000,
			SFGAO_READONLY = 0x00040000,
			SFGAO_HIDDEN = 0x00080000,
			SFGAO_DISPLAYATTRMASK = 0x000FC000,
			SFGAO_FILESYSANCESTOR = 0x10000000,
			SFGAO_FOLDER = 0x20000000,
			SFGAO_FILESYSTEM = 0x40000000,
			SFGAO_HASSUBFOLDER = 0x80000000,
			SFGAO_CONTENTSMASK = 0x80000000,
			SFGAO_VALIDATE = 0x01000000,
			SFGAO_REMOVABLE = 0x02000000,
			SFGAO_COMPRESSED = 0x04000000,
			SFGAO_BROWSABLE = 0x08000000,
			SFGAO_NONENUMERATED = 0x00100000,
			SFGAO_NEWCONTENT = 0x00200000,
			SFGAO_CANMONIKER = 0x00400000,
			SFGAO_HASSTORAGE = 0x00400000,
			SFGAO_STREAM = 0x00400000,
			SFGAO_STORAGEANCESTOR = 0x00800000,
			SFGAO_STORAGECAPMASK = 0x70C50008
		}

		[Flags]
		public enum SHGFI
		{
			SHGFI_ICON = 0x000000100,
			SHGFI_DISPLAYNAME = 0x000000200,
			SHGFI_TYPENAME = 0x000000400,
			SHGFI_ATTRIBUTES = 0x000000800,
			SHGFI_ICONLOCATION = 0x000001000,
			SHGFI_EXETYPE = 0x000002000,
			SHGFI_SYSICONINDEX = 0x000004000,
			SHGFI_LINKOVERLAY = 0x000008000,
			SHGFI_SELECTED = 0x000010000,
			SHGFI_ATTR_SPECIFIED = 0x000020000,
			SHGFI_LARGEICON = 0x000000000,
			SHGFI_SMALLICON = 0x000000001,
			SHGFI_OPENICON = 0x000000002,
			SHGFI_SHELLICONSIZE = 0x000000004,
			SHGFI_PIDL = 0x000000008,
			SHGFI_USEFILEATTRIBUTES = 0x000000010,
			SHGFI_ADDOVERLAYS = 0x000000020,
			SHGFI_OVERLAYINDEX = 0x000000040
		}

		[Flags]
		public enum SHGDNF
		{
			SHGDN_NORMAL = 0x0000,
			SHGDN_INFOLDER = 0x0001,
			SHGDN_FOREDITING = 0x1000,
			SHGDN_FORADDRESSBAR = 0x4000,
			SHGDN_FORPARSING = 0x8000,
		}

		[Flags]
		public enum SHCONTF
		{
			SHCONTF_CHECKING_FOR_CHILDREN = 0x00010,
			SHCONTF_FOLDERS = 0x00020,
			SHCONTF_NONFOLDERS = 0x00040,
			SHCONTF_INCLUDEHIDDEN = 0x00080,
			SHCONTF_INIT_ON_FIRST_NEXT = 0x00100,
			SHCONTF_NETPRINTERSRCH = 0x00200,
			SHCONTF_SHAREABLE = 0x00400,
			SHCONTF_STORAGE = 0x00800,
			SHCONTF_NAVIGATION_ENUM = 0x01000,
			SHCONTF_FASTITEMS = 0x02000,
			SHCONTF_FLATLIST = 0x04000,
			SHCONTF_ENABLE_ASYNC = 0x08000,
			SHCONTF_INCLUDESUPERHIDDEN = 0x10000
		}

		[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("000214E6-0000-0000-C000-000000000046")]
		public interface IShellFolder
		{
			[PreserveSig]
			int ParseDisplayName(IntPtr hwnd, IntPtr pbc, string pszDisplayName, ref uint pchEaten, out IntPtr ppidl,
				ref SFGAO pdwAttributes);

			[PreserveSig]
			int EnumObjects(IntPtr hwnd, SHCONTF grfFlags, out IEnumIDList ppenumIDList);

			[PreserveSig]
			int BindToObject(IntPtr pidl, IntPtr pbc, [In] ref Guid riid, out IntPtr ppv);

			[PreserveSig]
			int BindToStorage(IntPtr pidl, IntPtr pbc, [In] ref Guid riid, out IntPtr ppv);

			[PreserveSig]
			int CompareIDs(IntPtr lParam, IntPtr pidl1, IntPtr pidl2);

			[PreserveSig]
			int CreateViewObject(IntPtr hwndOwner, [In] ref Guid riid, out IntPtr ppv);

			[PreserveSig]
			int GetAttributesOf(UInt32 cidl, IntPtr apidl, ref SFGAO rgfInOut);

			[PreserveSig]
			int GetUIObjectOf(IntPtr hwndOwner, UInt32 cidl, ref IntPtr apidl, [In] ref Guid riid, UInt32 rgfReserved,
				out IntPtr ppv);

			[PreserveSig]
			int GetDisplayNameOf(IntPtr pidl, SHGDNF uFlags, out STRRET pName);

			[PreserveSig]
			int SetNameOf(IntPtr hwnd, IntPtr pidl, String pszName, SHGDNF uFlags, out IntPtr ppidlOut);
		}

		public enum ESFGAO : uint
		{
			SFGAO_CANCOPY = 0x00000001,
			SFGAO_CANMOVE = 0x00000002,
			SFGAO_CANLINK = 0x00000004,
			SFGAO_LINK = 0x00010000,
			SFGAO_SHARE = 0x00020000,
			SFGAO_READONLY = 0x00040000,
			SFGAO_HIDDEN = 0x00080000,
			SFGAO_FOLDER = 0x20000000,
			SFGAO_FILESYSTEM = 0x40000000,
			SFGAO_HASSUBFOLDER = 0x80000000,
		}

		public enum ESHCONTF
		{
			SHCONTF_FOLDERS = 0x0020,
			SHCONTF_NONFOLDERS = 0x0040,
			SHCONTF_INCLUDEHIDDEN = 0x0080,
			SHCONTF_INIT_ON_FIRST_NEXT = 0x0100,
			SHCONTF_NETPRINTERSRCH = 0x0200,
			SHCONTF_SHAREABLE = 0x0400,
			SHCONTF_STORAGE = 0x0800
		}

		public enum ESHGDN
		{
			SHGDN_NORMAL = 0x0000,
			SHGDN_INFOLDER = 0x0001,
			SHGDN_FOREDITING = 0x1000,
			SHGDN_FORADDRESSBAR = 0x4000,
			SHGDN_FORPARSING = 0x8000,
		}

		public enum ESTRRET : int
		{
			eeRRET_WSTR = 0x0000,
			STRRET_OFFSET = 0x0001,
			STRRET_CSTR = 0x0002
		}

		[StructLayout(LayoutKind.Explicit, Size = 520)]
		public struct STRRETinternal
		{
			[FieldOffset(0)]
			public IntPtr pOleStr;

			[FieldOffset(0)]
			public IntPtr pStr;

			[FieldOffset(0)]
			public uint uOffset;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct STRRET
		{
			public uint uType;
			public STRRETinternal data;
		}

		public class Guid_IShellFolder
		{
			public static Guid IID_IShellFolder = new Guid("{000214E6-0000-0000-C000-000000000046}");
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct SHITEMID
		{
			public ushort cb;
			public byte[] abID;
		}

		public enum CSIDL
		{
			CSIDL_DESKTOP = 0x0000, // <desktop>
			CSIDL_INTERNET = 0x0001, // Internet Explorer (icon on desktop)
			CSIDL_PROGRAMS = 0x0002, // Start Menu\Programs
			CSIDL_CONTROLS = 0x0003, // My Computer\Control Panel
			CSIDL_PRINTERS = 0x0004, // My Computer\Printers
			CSIDL_PERSONAL = 0x0005, // My Documents
			CSIDL_FAVORITES = 0x0006, // <user name>\Favorites
			CSIDL_STARTUP = 0x0007, // Start Menu\Programs\Startup
			CSIDL_RECENT = 0x0008, // <user name>\Recent
			CSIDL_SENDTO = 0x0009, // <user name>\SendTo
			CSIDL_BITBUCKET = 0x000a, // <desktop>\Recycle Bin
			CSIDL_STARTMENU = 0x000b, // <user name>\Start Menu
			CSIDL_MYDOCUMENTS = 0x000c, // logical "My Documents" desktop icon
			CSIDL_MYMUSIC = 0x000d, // "My Music" folder
			CSIDL_MYVIDEO = 0x000e, // "My Videos" folder
			CSIDL_DESKTOPDIRECTORY = 0x0010, // <user name>\Desktop
			CSIDL_DRIVES = 0x0011, // My Computer
			CSIDL_NETWORK = 0x0012, // Network Neighborhood (My Network Places)
			CSIDL_NETHOOD = 0x0013, // <user name>\nethood
			CSIDL_FONTS = 0x0014, // windows\fonts
			CSIDL_TEMPLATES = 0x0015,
			CSIDL_COMMON_STARTMENU = 0x0016, // All Users\Start Menu
			CSIDL_COMMON_PROGRAMS = 0X0017, // All Users\Start Menu\Programs
			CSIDL_COMMON_STARTUP = 0x0018, // All Users\Startup
			CSIDL_COMMON_DESKTOPDIRECTORY = 0x0019, // All Users\Desktop
			CSIDL_APPDATA = 0x001a, // <user name>\Application Data
			CSIDL_PRINTHOOD = 0x001b, // <user name>\PrintHood
			CSIDL_LOCAL_APPDATA = 0x001c, // <user name>\Local Settings\Applicaiton Data (non roaming)
			CSIDL_ALTSTARTUP = 0x001d, // non localized startup
			CSIDL_COMMON_ALTSTARTUP = 0x001e, // non localized common startup
			CSIDL_COMMON_FAVORITES = 0x001f,
			CSIDL_INTERNET_CACHE = 0x0020,
			CSIDL_COOKIES = 0x0021,
			CSIDL_HISTORY = 0x0022,
			CSIDL_COMMON_APPDATA = 0x0023, // All Users\Application Data
			CSIDL_WINDOWS = 0x0024, // GetWindowsDirectory()
			CSIDL_SYSTEM = 0x0025, // GetSystemDirectory()
			CSIDL_PROGRAM_FILES = 0x0026, // C:\Program Files
			CSIDL_MYPICTURES = 0x0027, // C:\Program Files\My Pictures
			CSIDL_PROFILE = 0x0028, // USERPROFILE
			CSIDL_SYSTEMX86 = 0x0029, // x86 system directory on RISC
			CSIDL_PROGRAM_FILESX86 = 0x002a, // x86 C:\Program Files on RISC
			CSIDL_PROGRAM_FILES_COMMON = 0x002b, // C:\Program Files\Common
			CSIDL_PROGRAM_FILES_COMMONX86 = 0x002c, // x86 Program Files\Common on RISC
			CSIDL_COMMON_TEMPLATES = 0x002d, // All Users\Templates
			CSIDL_COMMON_DOCUMENTS = 0x002e, // All Users\Documents
			CSIDL_COMMON_ADMINTOOLS = 0x002f, // All Users\Start Menu\Programs\Administrative Tools
			CSIDL_ADMINTOOLS = 0x0030, // <user name>\Start Menu\Programs\Administrative Tools
			CSIDL_CONNECTIONS = 0x0031, // Network and Dial-up Connections
			CSIDL_COMMON_MUSIC = 0x0035, // All Users\My Music
			CSIDL_COMMON_PICTURES = 0x0036, // All Users\My Pictures
			CSIDL_COMMON_VIDEO = 0x0037, // All Users\My Video
			CSIDL_CDBURN_AREA = 0x003b // USERPROFILE\Local Settings\Application Data\Microsoft\CD Burning
		}

		[Flags]
		public enum GCS : uint
		{
			GCS_VERBA = 0x00000000,
			GCS_HELPTEXTA = 0x00000001,
			GCS_VALIDATEA = 0x00000002,
			GCS_VERBW = 0x00000004,
			GCS_HELPTEXTW = 0x00000005,
			GCS_VALIDATEW = 0x00000006,
			GCS_VERBICONW = 0x00000014,
			GCS_UNICODE = 0x00000004
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct ITEMIDLIST
		{
			[MarshalAs(UnmanagedType.Struct)]
			public SHITEMID mkid;
		}

		[Flags]
		public enum CMF : uint
		{
			CMF_NORMAL = 0x00000000,
			CMF_DEFAULTONLY = 0x00000001,
			CMF_VERBSONLY = 0x00000002,
			CMF_EXPLORE = 0x00000004,
			CMF_NOVERBS = 0x00000008,
			CMF_CANRENAME = 0x00000010,
			CMF_NODEFAULT = 0x00000020,
			CMF_INCLUDESTATIC = 0x00000040,
			CMF_ITEMMENU = 0x00000080,
			CMF_EXTENDEDVERBS = 0x00000100,
			CMF_DISABLEDVERBS = 0x00000200,
			CMF_ASYNCVERBSTATE = 0x00000400,
			CMF_OPTIMIZEFORINVOKE = 0x00000800,
			CMF_SYNCCASCADEMENU = 0x00001000,
			CMF_DONOTPICKDEFAULT = 0x00002000,
			CMF_RESERVED = 0xFFFF0000
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SHFILEINFO
		{
			public IntPtr hIcon;
			public int iIcon;
			public uint dwAttributes;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szDisplayName;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		}

		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("000214e4-0000-0000-c000-000000000046")]
		public interface IContextMenu
		{
			[PreserveSig]
			int QueryContextMenu(IntPtr hMenu, uint indexMenu, int idCmdFirst, int idCmdLast, CMF uFlags);

			[PreserveSig]
			int InvokeCommand(ref CMINVOKECOMMANDINFO pici);

			[PreserveSig]
			int GetCommandString(int idcmd, GCS uflags, int reserved, StringBuilder commandstring, int cch);
		}

		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("000214f4-0000-0000-c000-000000000046")]
		public interface IContextMenu2 : IContextMenu
		{
			#region IContextMenu overrides

			new int QueryContextMenu(IntPtr hMenu, uint indexMenu, int idCmdFirst, int idCmdLast, CMF uFlags);
			new int GetCommandString(int idcmd, GCS uflags, int reserved, StringBuilder commandstring, int cch);

			#endregion

			[PreserveSig]
			int InvokeCommand(IntPtr pici);

			[PreserveSig]
			int HandleMenuMsg(uint uMsg, IntPtr wParam, IntPtr lParam);
		}

		[Flags]
		public enum SEE : uint
		{
			SEE_MASK_DEFAULT = 0x00000000,
			SEE_MASK_CLASSNAME = 0x00000001,
			SEE_MASK_CLASSKEY = 0x00000003,
			SEE_MASK_IDLIST = 0x00000004,
			SEE_MASK_INVOKEIDLIST = 0x0000000C,
			SEE_MASK_ICON = 0x00000010,
			SEE_MASK_HOTKEY = 0x00000020,
			SEE_MASK_NOCLOSEPROCESS = 0x00000040,
			SEE_MASK_CONNECTNETDRV = 0x00000080,
			SEE_MASK_NOASYNC = 0x00000100,
			SEE_MASK_FLAG_DDEWAIT = 0x00000100,
			SEE_MASK_DOENVSUBST = 0x00000200,
			SEE_MASK_FLAG_NO_UI = 0x00000400,
			SEE_MASK_UNICODE = 0x00004000,
			SEE_MASK_NO_CONSOLE = 0x00008000,
			SEE_MASK_ASYNCOK = 0x00100000,
			SEE_MASK_NOQUERYCLASSSTORE = 0x01000000,
			SEE_MASK_HMONITOR = 0x00200000,
			SEE_MASK_NOZONECHECKS = 0x00800000,
			SEE_MASK_WAITFORINPUTIDLE = 0x02000000,
			SEE_MASK_FLAG_LOG_USAGE = 0x04000000,
			SEE_MASK_FLAG_HINST_IS_SITE = 0x08000000,
		}

		public enum KNOWN_FOLDER_FLAG : uint
		{
			KF_NO_FLAGS = 0,
			KF_FLAG_SIMPLE_IDLIST = 0x00000100,
			KF_FLAG_NOT_PARENT_RELATIVE = 0x00000200,
			KF_FLAG_DEFAULT_PATH = 0x00000400,
			KF_FLAG_INIT = 0x00000800,
			KF_FLAG_NO_ALIAS = 0x00001000,
			KF_FLAG_DONT_UNEXPAND = 0x00002000,
			KF_FLAG_DONT_VERIFY = 0x00004000,
			KF_FLAG_CREATE = 0x00008000,
			KF_FLAG_NO_APPCONTAINER_REDIRECTION = 0x00010000,
			KF_FLAG_ALIAS_ONLY = 0x80000000
		}

		public enum WM : uint
		{
			NULL = 0x0000,
			CREATE = 0x0001,
			DESTROY = 0x0002,
			MOVE = 0x0003,
			SIZE = 0x0005,
			ACTIVATE = 0x0006,
			SETFOCUS = 0x0007,
			KILLFOCUS = 0x0008,
			ENABLE = 0x000A,
			SETREDRAW = 0x000B,
			SETTEXT = 0x000C,
			GETTEXT = 0x000D,
			GETTEXTLENGTH = 0x000E,
			PAINT = 0x000F,
			CLOSE = 0x0010,
			QUERYENDSESSION = 0x0011,
			QUERYOPEN = 0x0013,
			ENDSESSION = 0x0016,
			QUIT = 0x0012,
			ERASEBKGND = 0x0014,
			SYSCOLORCHANGE = 0x0015,
			SHOWWINDOW = 0x0018,
			WININICHANGE = 0x001A,
			SETTINGCHANGE = WININICHANGE,
			DEVMODECHANGE = 0x001B,
			ACTIVATEAPP = 0x001C,
			FONTCHANGE = 0x001D,
			TIMECHANGE = 0x001E,
			CANCELMODE = 0x001F,
			SETCURSOR = 0x0020,
			MOUSEACTIVATE = 0x0021,
			CHILDACTIVATE = 0x0022,
			QUEUESYNC = 0x0023,
			GETMINMAXINFO = 0x0024,
			PAINTICON = 0x0026,
			ICONERASEBKGND = 0x0027,
			NEXTDLGCTL = 0x0028,
			SPOOLERSTATUS = 0x002A,
			DRAWITEM = 0x002B,
			MEASUREITEM = 0x002C,
			DELETEITEM = 0x002D,
			VKEYTOITEM = 0x002E,
			CHARTOITEM = 0x002F,
			SETFONT = 0x0030,
			GETFONT = 0x0031,
			SETHOTKEY = 0x0032,
			GETHOTKEY = 0x0033,
			QUERYDRAGICON = 0x0037,
			COMPAREITEM = 0x0039,
			GETOBJECT = 0x003D,
			COMPACTING = 0x0041,

			[Obsolete]
			COMMNOTIFY = 0x0044,
			WINDOWPOSCHANGING = 0x0046,
			WINDOWPOSCHANGED = 0x0047,

			[Obsolete]
			POWER = 0x0048,
			COPYDATA = 0x004A,
			CANCELJOURNAL = 0x004B,
			NOTIFY = 0x004E,
			INPUTLANGCHANGEREQUEST = 0x0050,
			INPUTLANGCHANGE = 0x0051,
			TCARD = 0x0052,
			HELP = 0x0053,
			USERCHANGED = 0x0054,
			NOTIFYFORMAT = 0x0055,
			CONTEXTMENU = 0x007B,
			STYLECHANGING = 0x007C,
			STYLECHANGED = 0x007D,
			DISPLAYCHANGE = 0x007E,
			GETICON = 0x007F,
			SETICON = 0x0080,
			NCCREATE = 0x0081,
			NCDESTROY = 0x0082,
			NCCALCSIZE = 0x0083,
			NCHITTEST = 0x0084,
			NCPAINT = 0x0085,
			NCACTIVATE = 0x0086,
			GETDLGCODE = 0x0087,
			SYNCPAINT = 0x0088,
			NCMOUSEMOVE = 0x00A0,
			NCLBUTTONDOWN = 0x00A1,
			NCLBUTTONUP = 0x00A2,
			NCLBUTTONDBLCLK = 0x00A3,
			NCRBUTTONDOWN = 0x00A4,
			NCRBUTTONUP = 0x00A5,
			NCRBUTTONDBLCLK = 0x00A6,
			NCMBUTTONDOWN = 0x00A7,
			NCMBUTTONUP = 0x00A8,
			NCMBUTTONDBLCLK = 0x00A9,
			NCXBUTTONDOWN = 0x00AB,
			NCXBUTTONUP = 0x00AC,
			NCXBUTTONDBLCLK = 0x00AD,
			INPUT_DEVICE_CHANGE = 0x00FE,
			INPUT = 0x00FF,
			KEYFIRST = 0x0100,
			KEYDOWN = 0x0100,
			KEYUP = 0x0101,
			CHAR = 0x0102,
			DEADCHAR = 0x0103,
			SYSKEYDOWN = 0x0104,
			SYSKEYUP = 0x0105,
			SYSCHAR = 0x0106,
			SYSDEADCHAR = 0x0107,
			UNICHAR = 0x0109,
			KEYLAST = 0x0108,
			IME_STARTCOMPOSITION = 0x010D,
			IME_ENDCOMPOSITION = 0x010E,
			IME_COMPOSITION = 0x010F,
			IME_KEYLAST = 0x010F,
			INITDIALOG = 0x0110,
			COMMAND = 0x0111,
			SYSCOMMAND = 0x0112,
			TIMER = 0x0113,
			HSCROLL = 0x0114,
			VSCROLL = 0x0115,
			INITMENU = 0x0116,
			INITMENUPOPUP = 0x0117,
			MENUSELECT = 0x011F,
			MENUCHAR = 0x0120,
			ENTERIDLE = 0x0121,
			MENURBUTTONUP = 0x0122,
			MENUDRAG = 0x0123,
			MENUGETOBJECT = 0x0124,
			UNINITMENUPOPUP = 0x0125,
			MENUCOMMAND = 0x0126,
			CHANGEUISTATE = 0x0127,
			UPDATEUISTATE = 0x0128,
			QUERYUISTATE = 0x0129,
			CTLCOLORMSGBOX = 0x0132,
			CTLCOLOREDIT = 0x0133,
			CTLCOLORLISTBOX = 0x0134,
			CTLCOLORBTN = 0x0135,
			CTLCOLORDLG = 0x0136,
			CTLCOLORSCROLLBAR = 0x0137,
			CTLCOLORSTATIC = 0x0138,
			MOUSEFIRST = 0x0200,
			MOUSEMOVE = 0x0200,
			LBUTTONDOWN = 0x0201,
			LBUTTONUP = 0x0202,
			LBUTTONDBLCLK = 0x0203,
			RBUTTONDOWN = 0x0204,
			RBUTTONUP = 0x0205,
			RBUTTONDBLCLK = 0x0206,
			MBUTTONDOWN = 0x0207,
			MBUTTONUP = 0x0208,
			MBUTTONDBLCLK = 0x0209,
			MOUSEWHEEL = 0x020A,
			XBUTTONDOWN = 0x020B,
			XBUTTONUP = 0x020C,
			XBUTTONDBLCLK = 0x020D,
			MOUSEHWHEEL = 0x020E,
			MOUSELAST = 0x020E,
			PARENTNOTIFY = 0x0210,
			ENTERMENULOOP = 0x0211,
			EXITMENULOOP = 0x0212,
			NEXTMENU = 0x0213,
			SIZING = 0x0214,
			CAPTURECHANGED = 0x0215,
			MOVING = 0x0216,
			POWERBROADCAST = 0x0218,
			DEVICECHANGE = 0x0219,
			MDICREATE = 0x0220,
			MDIDESTROY = 0x0221,
			MDIACTIVATE = 0x0222,
			MDIRESTORE = 0x0223,
			MDINEXT = 0x0224,
			MDIMAXIMIZE = 0x0225,
			MDITILE = 0x0226,
			MDICASCADE = 0x0227,
			MDIICONARRANGE = 0x0228,
			MDIGETACTIVE = 0x0229,
			MDISETMENU = 0x0230,
			ENTERSIZEMOVE = 0x0231,
			EXITSIZEMOVE = 0x0232,
			DROPFILES = 0x0233,
			MDIREFRESHMENU = 0x0234,
			IME_SETCONTEXT = 0x0281,
			IME_NOTIFY = 0x0282,
			IME_CONTROL = 0x0283,
			IME_COMPOSITIONFULL = 0x0284,
			IME_SELECT = 0x0285,
			IME_CHAR = 0x0286,
			IME_REQUEST = 0x0288,
			IME_KEYDOWN = 0x0290,
			IME_KEYUP = 0x0291,
			MOUSEHOVER = 0x02A1,
			MOUSELEAVE = 0x02A3,
			NCMOUSEHOVER = 0x02A0,
			NCMOUSELEAVE = 0x02A2,
			WTSSESSION_CHANGE = 0x02B1,
			TABLET_FIRST = 0x02c0,
			TABLET_LAST = 0x02df,
			CUT = 0x0300,
			COPY = 0x0301,
			PASTE = 0x0302,
			CLEAR = 0x0303,
			UNDO = 0x0304,
			RENDERFORMAT = 0x0305,
			RENDERALLFORMATS = 0x0306,
			DESTROYCLIPBOARD = 0x0307,
			DRAWCLIPBOARD = 0x0308,
			PAINTCLIPBOARD = 0x0309,
			VSCROLLCLIPBOARD = 0x030A,
			SIZECLIPBOARD = 0x030B,
			ASKCBFORMATNAME = 0x030C,
			CHANGECBCHAIN = 0x030D,
			HSCROLLCLIPBOARD = 0x030E,
			QUERYNEWPALETTE = 0x030F,
			PALETTEISCHANGING = 0x0310,
			PALETTECHANGED = 0x0311,
			HOTKEY = 0x0312,
			PRINT = 0x0317,
			PRINTCLIENT = 0x0318,
			APPCOMMAND = 0x0319,
			THEMECHANGED = 0x031A,
			CLIPBOARDUPDATE = 0x031D,
			DWMCOMPOSITIONCHANGED = 0x031E,
			DWMNCRENDERINGCHANGED = 0x031F,
			DWMCOLORIZATIONCOLORCHANGED = 0x0320,
			DWMWINDOWMAXIMIZEDCHANGE = 0x0321,
			GETTITLEBARINFOEX = 0x033F,
			HANDHELDFIRST = 0x0358,
			HANDHELDLAST = 0x035F,
			AFXFIRST = 0x0360,
			AFXLAST = 0x037F,
			PENWINFIRST = 0x0380,
			PENWINLAST = 0x038F,
			APP = 0x8000,
			USER = 0x0400,
			CPL_LAUNCH = USER + 0x1000,
			CPL_LAUNCHED = USER + 0x1001,
			SYSTIMER = 0x118,
			HSHELL_ACCESSIBILITYSTATE = 11,
			HSHELL_ACTIVATESHELLWINDOW = 3,
			HSHELL_APPCOMMAND = 12,
			HSHELL_GETMINRECT = 5,
			HSHELL_LANGUAGE = 8,
			HSHELL_REDRAW = 6,
			HSHELL_TASKMAN = 7,
			HSHELL_WINDOWCREATED = 1,
			HSHELL_WINDOWDESTROYED = 2,
			HSHELL_WINDOWACTIVATED = 4,
			HSHELL_WINDOWREPLACED = 13
		}

#pragma warning disable CS0649
		public struct SHELLEXECUTEINFO
		{
			public int cbSize;
			public SEE fMask;
			public IntPtr hwnd;
			public string lpVerb;
			public string lpFile;
			public string lpParameters;
			public string lpDirectory;
			public int nShow;
			public IntPtr hInstApp;
			public IntPtr lpIDList;
			public string lpClass;
			public IntPtr hkeyClass;
			public int dwHotKey;
			public IntPtr hIcon;
			public IntPtr hProcess;
		}
#pragma warning restore CS0649

		[Flags]
		public enum ClassStyles : uint
		{
			ByteAlignClient = 0x1000,
			ByteAlignWindow = 0x2000,
			ClassDC = 0x40,
			DoubleClicks = 0x8,
			DropShadow = 0x20000,
			GlobalClass = 0x4000,
			HorizontalRedraw = 0x2,
			NoClose = 0x200,
			OwnDC = 0x20,
			ParentDC = 0x80,
			SaveBits = 0x800,
			VerticalRedraw = 0x1
		}

		public delegate IntPtr WndProc(IntPtr hWnd, WM msg, IntPtr wParam, IntPtr lParam);

		[System.Runtime.InteropServices.StructLayout(
			System.Runtime.InteropServices.LayoutKind.Sequential,
			CharSet = System.Runtime.InteropServices.CharSet.Unicode
			)]
		public struct WNDCLASS
		{
			public uint style;
			public IntPtr lpfnWndProc;
			public int cbClsExtra;
			public int cbWndExtra;
			public IntPtr hInstance;
			public IntPtr hIcon;
			public IntPtr hCursor;
			public IntPtr hbrBackground;

			[System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
			public string lpszMenuName;

			[System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
			public string lpszClassName;
		}

		[System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
		public static extern System.UInt16 RegisterClassW(
			[System.Runtime.InteropServices.In] ref WNDCLASS lpWndClass
			);

		[StructLayout(LayoutKind.Sequential)]
		public struct WNDCLASSEX
		{
			public uint cbSize;
			public ClassStyles style;

			[MarshalAs(UnmanagedType.FunctionPtr)]
			public WndProc lpfnWndProc;

			public int cbClsExtra;
			public int cbWndExtra;
			public IntPtr hInstance;
			public IntPtr hIcon;
			public IntPtr hCursor;
			public IntPtr hbrBackground;
			public string lpszMenuName;
			public string lpszClassName;
			public IntPtr hIconSm;
		}

		[DllImport("user32.dll")]
		public static extern IntPtr CreatePopupMenu();

		[DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern uint DragQueryFile(IntPtr hDrop, uint iFile, [Out] StringBuilder lpszFile, uint cch);

		[DllImport("shell32.dll", EntryPoint = "ShellExecuteEx", SetLastError = true)]
		public static extern int ShellExecuteEx(ref SHELLEXECUTEINFO pExecInfo);

		[DllImport("shell32.dll", SetLastError = true)]
		public static extern int SHGetKnownFolderIDList([MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
			KNOWN_FOLDER_FLAG dwFlags, IntPtr hToken, out IntPtr ppidl);

		[DllImport("shell32.dll")]
		public static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
			KNOWN_FOLDER_FLAG dwFlags, IntPtr hToken, out string pszPath);

		[DllImport("shell32.dll")]
		public static extern void ILFree(IntPtr pidl);

		[DllImport("shell32.dll")]
		public static extern int SHGetDesktopFolder(out IShellFolder ppshf);

		[DllImport("shell32.dll")]
		public static extern int SHGetFolderLocation(IntPtr hwndOwner, CSIDL nFolder, IntPtr hToken, uint dwReserved,
			out IntPtr ppidl);

		[DllImport("shell32.dll", EntryPoint = "SHGetPathFromIDListW")]
		public static extern bool SHGetPathFromIDList(IntPtr pidl, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder pszPath);

		[DllImport("shlwapi.dll", EntryPoint = "SHStrDupW")]
		public static extern int SHStrDup([MarshalAs(UnmanagedType.LPWStr)] string pszSource, out IntPtr ppwsz);

		[DllImport("shell32.dll")]
		public static extern bool ILIsEqual(IntPtr pidl1, IntPtr pidl2);

		[DllImport("shell32.dll")]
		public static extern IntPtr ILCombine(IntPtr pidl1, IntPtr pidl2);

		[DllImport("shell32.dll")]
		public static extern IntPtr ILClone(IntPtr pidl);

		[DllImport("shell32.dll")]
		public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttribs, out SHFILEINFO psfi, uint cbFileInfo,
			SHGFI uFlags);

		[DllImport("shell32.dll")]
		public static extern Int32 SHGetSpecialFolderLocation(IntPtr hwndOwner, CSIDL nFolder, ref IntPtr ppidl);

		[DllImport("shell32.dll")]
		public static extern int SHCreateDataObject(IntPtr pidlFolder, uint cidl, IntPtr apidl, IDataObject pdtInner,
			Guid riid,
			out IntPtr ppv);

		[DllImport("user32.dll")]
		public static extern IntPtr DefWindowProc(IntPtr hWnd, WM uMsg, IntPtr wParam, IntPtr lParam);

		public enum GWL
		{
			GWL_WNDPROC = (-4),
			GWL_HINSTANCE = (-6),
			GWL_HWNDPARENT = (-8),
			GWL_STYLE = (-16),
			GWL_EXSTYLE = (-20),
			GWL_USERDATA = (-21),
			GWL_ID = (-12)
		}

		public abstract class WindowStyles
		{
			public const uint WS_OVERLAPPED = 0x00000000;
			public const uint WS_POPUP = 0x80000000;
			public const uint WS_CHILD = 0x40000000;
			public const uint WS_MINIMIZE = 0x20000000;
			public const uint WS_VISIBLE = 0x10000000;
			public const uint WS_DISABLED = 0x08000000;
			public const uint WS_CLIPSIBLINGS = 0x04000000;
			public const uint WS_CLIPCHILDREN = 0x02000000;
			public const uint WS_MAXIMIZE = 0x01000000;
			public const uint WS_CAPTION = 0x00C00000; /* WS_BORDER | WS_DLGFRAME  */
			public const uint WS_BORDER = 0x00800000;
			public const uint WS_DLGFRAME = 0x00400000;
			public const uint WS_VSCROLL = 0x00200000;
			public const uint WS_HSCROLL = 0x00100000;
			public const uint WS_SYSMENU = 0x00080000;
			public const uint WS_THICKFRAME = 0x00040000;
			public const uint WS_GROUP = 0x00020000;
			public const uint WS_TABSTOP = 0x00010000;
			public const uint WS_MINIMIZEBOX = 0x00020000;
			public const uint WS_MAXIMIZEBOX = 0x00010000;
			public const uint WS_TILED = WS_OVERLAPPED;
			public const uint WS_ICONIC = WS_MINIMIZE;
			public const uint WS_SIZEBOX = WS_THICKFRAME;
			public const uint WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW;

			public const uint WS_OVERLAPPEDWINDOW =
				(WS_OVERLAPPED |
				 WS_CAPTION |
				 WS_SYSMENU |
				 WS_THICKFRAME |
				 WS_MINIMIZEBOX |
				 WS_MAXIMIZEBOX);

			public const uint WS_POPUPWINDOW =
				(WS_POPUP |
				 WS_BORDER |
				 WS_SYSMENU);

			public const uint WS_CHILDWINDOW = WS_CHILD;
			public const uint WS_EX_DLGMODALFRAME = 0x00000001;
			public const uint WS_EX_NOPARENTNOTIFY = 0x00000004;
			public const uint WS_EX_TOPMOST = 0x00000008;
			public const uint WS_EX_ACCEPTFILES = 0x00000010;
			public const uint WS_EX_TRANSPARENT = 0x00000020;
			public const uint WS_EX_MDICHILD = 0x00000040;
			public const uint WS_EX_TOOLWINDOW = 0x00000080;
			public const uint WS_EX_WINDOWEDGE = 0x00000100;
			public const uint WS_EX_CLIENTEDGE = 0x00000200;
			public const uint WS_EX_CONTEXTHELP = 0x00000400;
			public const uint WS_EX_RIGHT = 0x00001000;
			public const uint WS_EX_LEFT = 0x00000000;
			public const uint WS_EX_RTLREADING = 0x00002000;
			public const uint WS_EX_LTRREADING = 0x00000000;
			public const uint WS_EX_LEFTSCROLLBAR = 0x00004000;
			public const uint WS_EX_RIGHTSCROLLBAR = 0x00000000;
			public const uint WS_EX_CONTROLPARENT = 0x00010000;
			public const uint WS_EX_STATICEDGE = 0x00020000;
			public const uint WS_EX_APPWINDOW = 0x00040000;
			public const uint WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE);
			public const uint WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);
			public const uint WS_EX_LAYERED = 0x00080000;
			public const uint WS_EX_NOINHERITLAYOUT = 0x00100000; // Disable inheritence of mirroring by children
			public const uint WS_EX_LAYOUTRTL = 0x00400000; // Right to left mirroring
			public const uint WS_EX_COMPOSITED = 0x02000000;
			public const uint WS_EX_NOACTIVATE = 0x08000000;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct CREATESTRUCT
		{
			public IntPtr lpCreateParams;
			public IntPtr hInstance;
			public IntPtr hMenu;
			public IntPtr hwndParent;
			public int cy;
			public int cx;
			public int y;
			public int x;
			public int style;
			public IntPtr lpszName;
			public IntPtr lpszClass;
			public int dwExStyle;
		}

		[DllImport("user32.dll", EntryPoint = "GetWindowLong")]
		private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, GWL nIndex);

		[DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
		private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, GWL nIndex);

		public static IntPtr GetWindowLongPtr(IntPtr hWnd, GWL nIndex)
		{
			if (IntPtr.Size == 8) {
				return GetWindowLongPtr64(hWnd, nIndex);
			} else {
				return GetWindowLongPtr32(hWnd, nIndex);
			}
		}

		[Flags]
		public enum CMIC : uint
		{
			CMIC_MASK_ICON = 0x00000010,
			CMIC_MASK_HOTKEY = 0x00000020,
			CMIC_MASK_NOASYNC = 0x00000100,
			CMIC_MASK_FLAG_NO_UI = 0x00000400,
			CMIC_MASK_UNICODE = 0x00004000,
			CMIC_MASK_NO_CONSOLE = 0x00008000,
			CMIC_MASK_ASYNCOK = 0x00100000,
			CMIC_MASK_NOZONECHECKS = 0x00800000,
			CMIC_MASK_FLAG_LOG_USAGE = 0x04000000,
			CMIC_MASK_SHIFT_DOWN = 0x10000000,
			CMIC_MASK_PTINVOKE = 0x20000000,
			CMIC_MASK_CONTROL_DOWN = 0x40000000,
			/*/// <summary>
			CMIC_MASK_SEP_VDM = 0*/
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct CMINVOKECOMMANDINFO
		{
			public uint cbSize;
			public CMIC fMask;
			public IntPtr hwnd;
			public IntPtr verb;

			[MarshalAs(UnmanagedType.LPStr)]
			public string parameters;

			[MarshalAs(UnmanagedType.LPStr)]
			public string directory;

			public int nShow;
			public uint dwHotKey;
			public IntPtr hIcon;
		}

		public static IntPtr SetWindowLongPtr(IntPtr hWnd, GWL nIndex, IntPtr dwNewLong)
		{
			if (IntPtr.Size == 8) {
				return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
			} else {
				return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
			}
		}

		[DllImport("user32.dll", EntryPoint = "SetWindowLong")]
		private static extern int SetWindowLong32(IntPtr hWnd, GWL nIndex, int dwNewLong);

		[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
		private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, GWL nIndex, IntPtr dwNewLong);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.U2)]
		public static extern short RegisterClassEx([In] ref WNDCLASSEX lpwcx);

		[System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr CreateWindowEx(
			UInt32 dwExStyle,
			string lpClassName,
			string lpWindowName,
			UInt32 dwStyle,
			Int32 x,
			Int32 y,
			Int32 nWidth,
			Int32 nHeight,
			IntPtr hWndParent,
			IntPtr hMenu,
			IntPtr hInstance,
			IntPtr lpParam
			);

		public static uint TPM_LEFTALIGN = 0x0000;
		public static uint TPM_CENTERALIGN = 0x0004;
		public static uint TPM_RIGHTALIGN = 0x0008;
		public static uint TPM_TOPALIGN = 0x0000;
		public static uint TPM_VCENTERALIGN = 0x0010;
		public static uint TPM_BOTTOMALIGN = 0x0020;
		public static uint TPM_HORIZONTAL = 0x0000;
		public static uint TPM_VERTICAL = 0x0040;
		public static uint TPM_RETURNCMD = 0x0100;
		public static uint TPM_LEFTBUTTON = 0x0000;
		public static uint TPM_RIGHTBUTTON = 0x0002;

		[DllImport("user32.dll")]
		public static extern int TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

		public static Guid IID_IQueryAssociations = new Guid("{c46ca590-3c3f-11d2-bee6-0000f805ca57}");
		public static Guid IID_IShellFolder = new Guid("000214E6-0000-0000-C000-000000000046");
		public static Guid IID_IImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
		public static Guid IID_ExtractIconW = new Guid("{000214FA-0000-0000-C000-000000000046}");
		public static Guid IID_IDataObject = new Guid("{0000010E-0000-0000-C000-000000000046}");
		public static Guid IID_ICo = new Guid("000214e4-0000-0000-c000-000000000046");
		public static Guid IID_IShellBrowser = new Guid("000214E2-0000-0000-C000-000000000046");
		public static Guid IID_IFolderView = new Guid("cde725b0-ccc9-4519-917e-325d72fab4ce");
		public static Guid IID_IContextMenu = new Guid("000214e4-0000-0000-c000-000000000046");
		public static Guid IID_IContextMenu2 = new Guid("000214f4-0000-0000-c000-000000000046");

		[DllImport("ole32.dll")]
		public static extern void CoTaskMemFree(IntPtr pv);

		[DllImport("user32.dll")]
		public static extern bool DestroyWindow(IntPtr hostWindowHandle);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern uint DestroyMenu(IntPtr hMenu);

		[DllImport("user32.dll")]
		public static extern int DestroyIcon(IntPtr hIcon);
	}
}
#endif
