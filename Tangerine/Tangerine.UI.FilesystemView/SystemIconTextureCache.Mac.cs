#if MAC
using System;
using System.Collections.Generic;
using System.IO;
using AppKit;
using Foundation;
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
			var icon = NSWorkspace.SharedWorkspace.IconForFileType(isDirectory ? NSFileTypeForHFSTypeCode.FinderIcon : ext);
			using (var stream = new MemoryStream())
			using (var representation = new NSBitmapImageRep(icon.CGImage)) {
				NSData data = null;
				data = representation.RepresentationUsingTypeProperties(NSBitmapImageFileType.Png);//, new NSDictionary());
				using (var bitmapStream = data.AsStream()) {
					bitmapStream.CopyTo(stream);
				}
				data.Dispose();
				var texture = new Texture2D();
				texture.LoadImage(stream);
				if (isDirectory) {
					directoryTexture = texture;
				} else {
					textureCache.Add(ext, texture);
				}
				return texture;
			}
		}
	}
}
#endif