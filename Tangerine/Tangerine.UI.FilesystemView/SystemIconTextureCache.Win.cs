#if WIN
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Lime;

namespace Tangerine.UI.FilesystemView
{
	public static class SystemIconTextureCache
	{
		private static readonly Dictionary<string, ITexture> textureCache = new Dictionary<string, ITexture>();
		private static ITexture directoryTexture;

		public static ITexture GetTexture(string path)
		{
			if (path.IsNullOrWhiteSpace()) {
				return TexturePool.Instance.GetTexture(null);
			}
			FileAttributes attr = File.GetAttributes(path);
			bool isDirectory = (attr & FileAttributes.Directory) == FileAttributes.Directory;
			if (isDirectory && directoryTexture != null) {
				return directoryTexture;
			}
			var ext = Path.GetExtension(path);
			if (ext == null) {
				return TexturePool.Instance.GetTexture(null);
			}
			if (textureCache.ContainsKey(ext)) {
				return textureCache[ext];
			}
			var shInfo = new WinAPI.SHFILEINFO();
			IntPtr r = WinAPI.SHGetFileInfo(path, 0, ref shInfo, (uint)Marshal.SizeOf(shInfo), WinAPI.SHGFI_ICON | WinAPI.SHGFI_SMALLICON);
			if (r == IntPtr.Zero) {
				return TexturePool.Instance.GetTexture(null);
			}
			var b = System.Drawing.Bitmap.FromHicon(shInfo.hIcon);
			WinAPI.DestroyIcon(shInfo.hIcon);
			var t = new Texture2D();
			using (var s = new MemoryStream()) {
				b.Save(s, ImageFormat.Png);
				t.LoadImage(s);
			}
			if (isDirectory) {
				directoryTexture = t;
			} else {
				textureCache.Add(ext, t);
			}
			return t;
		}	

		class WinAPI
		{
			[StructLayout(LayoutKind.Sequential)]
			public struct SHFILEINFO
			{
				public IntPtr hIcon;
				public IntPtr iIcon;
				public uint dwAttributes;
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
				public string szDisplayName;
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
				public string szTypeName;
			};

			public const uint SHGFI_ICON = 0x100;
			public const uint SHGFI_LARGEICON = 0x0;	// 'Large icon
			public const uint SHGFI_SMALLICON = 0x1;	// 'Small icon
			public const uint SHGFI_ADDOVERLAYS = 0x000000020;

			[DllImport("shell32.dll")]
			public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

			[DllImport("user32")]
			public static extern int DestroyIcon(IntPtr hIcon);
		}
	}
}
#endif