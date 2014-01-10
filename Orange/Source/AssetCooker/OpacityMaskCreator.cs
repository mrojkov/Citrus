using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lime;
using System.IO;

namespace Orange
{
	static class OpacityMaskCreator
	{
		const byte OpacityTheshold = 12;

		struct RGBA
		{
			public byte R, G, B, A;
		}

		public static void CreateMask(AssetsBundle assetsBundle, string pngPath, string maskPath)
		{
			int width, height;
			bool hasAlpha;
			if (!TextureConverterUtils.GetPngFileInfo(pngPath, out width, out height, out hasAlpha)) {
				throw new Lime.Exception("Wrong png file: " + pngPath);
			}
			if (!hasAlpha) {
				return;
			}
			using (var pixbuf = new Gdk.Pixbuf(pngPath)) {
				int newWidth = Math.Max(pixbuf.Width / 2, 1);
				int newHeight = Math.Max(pixbuf.Height / 2, 1);
				using (var pixbufDownscaled = pixbuf.ScaleSimple(newWidth, newHeight, Gdk.InterpType.Bilinear)) {
					bool bundled = assetsBundle.FileExists(maskPath);
					Console.WriteLine((bundled ? "* " : "+ ") + maskPath);
					WriteMask(assetsBundle, maskPath, pixbufDownscaled);
				}
			}
		}

		private static void WriteMask(AssetsBundle assetsBundle, string maskPath, Gdk.Pixbuf pixbuf)
		{
			var mask = CreateMaskHelper(pixbuf);
			using (var ms = new MemoryStream()) {
				using (var bw = new BinaryWriter(ms)) {
					bw.Write((UInt32)pixbuf.Width);
					bw.Write((UInt32)pixbuf.Height);
					bw.Write(mask, 0, mask.Length);
					bw.Flush();
					ms.Seek(0, SeekOrigin.Begin);
					assetsBundle.ImportFile(maskPath, ms, 0, compress: true);
				}
			};
		}

		private static byte[] CreateMaskHelper(Gdk.Pixbuf pixbuf)
		{
			byte[] mask = new byte[pixbuf.Height * ((pixbuf.Width + 7) / 8)];
			unsafe {
				int t = 0;
				RGBA* pixels = (RGBA*)pixbuf.Pixels;
				int width = pixbuf.Width;
				for (int i = 0; i < pixbuf.Height; i++) {
					byte v = 0;
					for (int j = 0; j < width; j++) {
						RGBA c = *pixels;
						if (c.A > OpacityTheshold) {
							v |= 1;
						}
						if (((j + 1) & 7) == 0) {
							mask[t++] = v;
							v = 0;
						}
						v <<= 1;
						pixels++;
					}
					if (width % 8 != 0) {
						mask[t++] = v;
					}
					pixels += pixbuf.Rowstride / 4 - width;
				}
				if (t != mask.Length) {
					throw new Lime.Exception();
				}
			}
			return mask;
		}
	}
}
