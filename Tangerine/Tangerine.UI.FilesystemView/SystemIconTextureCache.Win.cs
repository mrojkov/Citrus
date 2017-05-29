#if WIN
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Lime;

namespace Tangerine.UI.FilesystemView
{
	public class SystemIconTextureProvider : ISystemIconTextureProvider
	{
		private static readonly Dictionary<string, ITexture> textureCache = new Dictionary<string, ITexture>();
		private static ITexture directoryTexture;
		public static SystemIconTextureProvider Instance { get; set; } = new SystemIconTextureProvider();

		public ITexture GetTexture(string path)
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
			IntPtr r = WinAPI.SHGetFileInfo(path, 0, out shInfo, (uint)Marshal.SizeOf(shInfo), WinAPI.SHGFI.SHGFI_ICON | WinAPI.SHGFI.SHGFI_SMALLICON);
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
	}
}
#endif