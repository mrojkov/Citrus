#if WIN
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Lime;

namespace Tangerine.UI.FilesystemView
{
	// TODO: known bug - second mouse1 click after context menu has been handled doesn't come through
	public class SystemShellContextMenu : ISystemShellContextMenu
	{
		public static ISystemShellContextMenu Instance { get; set; } = new SystemShellContextMenu();

		public void Show(string path)
		{
			Show(new[] { path });
		}

		public void Show(IEnumerable<string> paths)
		{
			// TODO: multiple paths support
			ShowShellContextMenu(paths.First(), CommonWindow.Current.Form.Handle, (IntVector2)Application.DesktopMousePosition);
		}

		public void Show(string path, Vector2 position)
		{
			ShowShellContextMenu(path, CommonWindow.Current.Form.Handle, (IntVector2)position);
		}

		public void Show(IEnumerable<string> paths, Vector2 position)
		{
			ShowShellContextMenu(paths.First(), CommonWindow.Current.Form.Handle, (IntVector2)position);
		}

		private static unsafe IntPtr MenuCallback(IntPtr wnd, WinAPI.WM msg, IntPtr wparam, IntPtr lparam)
		{
			WinAPI.IContextMenu2 contextMenu2;
			IntPtr result;
			switch (msg) {
				case WinAPI.WM.CREATE: {
					WinAPI.SetWindowLongPtr(wnd, WinAPI.GWL.GWL_USERDATA, ((WinAPI.CREATESTRUCT*)lparam)->lpCreateParams);
					result = WinAPI.DefWindowProc(wnd, msg, wparam, lparam);
				}
					break;
				case WinAPI.WM.INITMENUPOPUP: {
					contextMenu2 =
						(WinAPI.IContextMenu2)Marshal.GetObjectForIUnknown(WinAPI.GetWindowLongPtr(wnd, WinAPI.GWL.GWL_USERDATA));
					contextMenu2.HandleMenuMsg((uint)msg, wparam, lparam);
					result = IntPtr.Zero;
				}
					break;
				case WinAPI.WM.DRAWITEM:
				case WinAPI.WM.MEASUREITEM: {
					contextMenu2 =
						(WinAPI.IContextMenu2)Marshal.GetObjectForIUnknown(WinAPI.GetWindowLongPtr(wnd, WinAPI.GWL.GWL_USERDATA));
					contextMenu2.HandleMenuMsg((uint)msg, wparam, lparam);
					result = IntPtr.Zero + 1;
				}
					break;
				default:
					result = WinAPI.DefWindowProc(wnd, msg, wparam, lparam);
					break;
			}
			return result;
		}

		private static readonly WinAPI.WndProc MenuCallbackDelegate = MenuCallback;

		private static IntPtr CreateMenuCallbackWnd(WinAPI.IContextMenu2 contextMenu)
		{
			string icmCallbackWnd = "ICMCALLBACKWND";
			WinAPI.WNDCLASS wclass = new WinAPI.WNDCLASS();
			wclass.lpszClassName = icmCallbackWnd;
			wclass.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(MenuCallbackDelegate);
			wclass.hInstance = IntPtr.Zero;
			int r = WinAPI.RegisterClassW(ref wclass);
			IntPtr result = WinAPI.CreateWindowEx(0, icmCallbackWnd, icmCallbackWnd, WinAPI.WindowStyles.WS_POPUPWINDOW,
				0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, Marshal.GetIUnknownForObject(contextMenu));
			return result;
		}

		private static void ShowShellContextMenu(string path, IntPtr windowHandle, IntVector2 mousePos)
		{
			string dir = Path.GetDirectoryName(path) ?? "";
			string filename = Path.GetFileNameWithoutExtension(path);
			string fileExt = Path.GetExtension(path);
			string pathRoot = Path.GetPathRoot(path);
			if (!dir.StartsWith(pathRoot)) {
				dir = Path.Combine(pathRoot, dir);
			}
			if (!string.IsNullOrEmpty(filename) && !string.IsNullOrEmpty(fileExt)) {
				filename += fileExt;
			}
			int r = 0;
			WinAPI.IShellFolder desktop = null;
			var pathPIDL = IntPtr.Zero;
			WinAPI.IShellFolder shellFolder = null;
			uint pchEaten = 0;
			WinAPI.SFGAO attr = 0;
			IntPtr ppv;
			var filePIDL = IntPtr.Zero;
			WinAPI.IContextMenu iCMenu = null;
			var shellContextMenu = IntPtr.Zero;
			var callbackWnd = IntPtr.Zero;
			WinAPI.IContextMenu2 iCMenu2 = null;
			Action Finalize = () => {
				if (iCMenu2 != null) {
					Marshal.FinalReleaseComObject(iCMenu2);
				}
				if (iCMenu != null) {
					Marshal.FinalReleaseComObject(iCMenu);
				}
				if (shellFolder != null) {
					Marshal.FinalReleaseComObject(shellFolder);
				}
				if (desktop != null) {
					Marshal.FinalReleaseComObject(desktop);
				}
				if (filePIDL != IntPtr.Zero) {
					WinAPI.CoTaskMemFree(filePIDL);
				}
				if (pathPIDL != IntPtr.Zero) {
					WinAPI.CoTaskMemFree(pathPIDL);
				}
				if (shellContextMenu != IntPtr.Zero) {
					WinAPI.DestroyMenu(shellContextMenu);
				}
				if (callbackWnd != IntPtr.Zero) {
					WinAPI.DestroyWindow(callbackWnd);
				}
			};
			Action<int> checkHResult = (result) => {
				if (result != 0) {
					Finalize();
					Marshal.ThrowExceptionForHR(result);
				}
			};
			r = WinAPI.SHGetDesktopFolder(out desktop);
			checkHResult(r);
			if (string.IsNullOrEmpty(filename)) {
				r = WinAPI.SHGetSpecialFolderLocation(IntPtr.Zero, WinAPI.CSIDL.CSIDL_DRIVES, ref pathPIDL);
				checkHResult(r);
				r = desktop.BindToObject(pathPIDL, IntPtr.Zero, ref WinAPI.Guid_IShellFolder.IID_IShellFolder, out ppv);
				checkHResult(r);
				shellFolder = (WinAPI.IShellFolder)Marshal.GetObjectForIUnknown(ppv);
				r = shellFolder.ParseDisplayName(windowHandle, IntPtr.Zero, dir, ref pchEaten, out filePIDL, ref attr);
				checkHResult(r);
			} else {
				r = desktop.ParseDisplayName(windowHandle, IntPtr.Zero, dir, ref pchEaten, out pathPIDL, ref attr);
				checkHResult(r);
				r = desktop.BindToObject(pathPIDL, IntPtr.Zero, ref WinAPI.IID_IShellFolder, out ppv);
				checkHResult(r);
				shellFolder = (WinAPI.IShellFolder)Marshal.GetObjectForIUnknown(ppv);
				r = shellFolder.ParseDisplayName(windowHandle, IntPtr.Zero, filename, ref pchEaten, out filePIDL, ref attr);
				checkHResult(r);
			}
			r = shellFolder.GetUIObjectOf(windowHandle, 1, ref filePIDL, ref WinAPI.IID_IContextMenu, 0, out ppv);
			checkHResult(r);
			iCMenu = (WinAPI.IContextMenu)Marshal.GetObjectForIUnknown(ppv);
			shellContextMenu = WinAPI.CreatePopupMenu();
			r = iCMenu.QueryContextMenu(shellContextMenu, 0, 1, 0x7FFF, WinAPI.CMF.CMF_EXPLORE);
			if ((r & 0x80000000) == 0) {
				var ppv2 = IntPtr.Zero;
				Marshal.QueryInterface(ppv, ref WinAPI.IID_IContextMenu2, out ppv2);
				iCMenu2 = (WinAPI.IContextMenu2)Marshal.GetObjectForIUnknown(ppv2);
				callbackWnd = CreateMenuCallbackWnd(iCMenu2);
				var popupMenuResult = WinAPI.TrackPopupMenuEx(shellContextMenu,
					WinAPI.TPM_LEFTALIGN | WinAPI.TPM_LEFTBUTTON | WinAPI.TPM_RIGHTBUTTON | WinAPI.TPM_RETURNCMD,
					mousePos.X, mousePos.Y, callbackWnd, IntPtr.Zero);
				if (popupMenuResult > 0) {
					var iCmd = popupMenuResult - 1;
					var CMD = new WinAPI.CMINVOKECOMMANDINFO {
						cbSize = (uint)Marshal.SizeOf(typeof (WinAPI.CMINVOKECOMMANDINFO)),
						hwnd = windowHandle,
						verb = (IntPtr)iCmd,
						nShow = 1, // SW_SHOWNORMAL
					};
					r = iCMenu.InvokeCommand(ref CMD);
					checkHResult(r);
				}
			}
			Finalize();
		}
	}
}
#endif