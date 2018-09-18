using System;
using Lime;
using System.IO;

namespace Orange
{
	static class OpacityMaskCreator
	{
		const byte OpacityTheshold = 12;

		public static void CreateMask(AssetBundle assetBundle, string srcPath, string maskPath)
		{
			using (var stream = File.OpenRead(srcPath)) {
				using (var bitmap = new Bitmap(stream)) {
					CreateMask(assetBundle, bitmap, maskPath);
				}
			}
		}

		public static void CreateMask(AssetBundle assetBundle, Bitmap bitmap, string maskPath)
		{
			if (!bitmap.HasAlpha) {
				return;
			}
			int newWidth = Math.Max(bitmap.Width / 2, 1);
			int newHeight = Math.Max(bitmap.Height / 2, 1);
			using (var scaledBitmap = bitmap.Rescale(newWidth, newHeight)) {
				bool bundled = assetBundle.FileExists(maskPath);
				Console.WriteLine((bundled ? "* " : "+ ") + maskPath);
				WriteMask(assetBundle, maskPath, scaledBitmap);
			}
		}

		private static void WriteMask(AssetBundle assetBundle, string maskPath, Bitmap bitmap)
		{
			byte[] mask = CreateMaskHelper(bitmap);
			using (var stream = new MemoryStream()) {
				using (var writer = new BinaryWriter(stream)) {
					writer.Write((uint) bitmap.Width);
					writer.Write((uint) bitmap.Height);
					writer.Write(mask, 0, mask.Length);
					writer.Flush();
					stream.Seek(0, SeekOrigin.Begin);
					assetBundle.ImportFile(maskPath, stream, 0, "", AssetAttributes.Zipped, null);
				}
			}
		}

		private static byte[] CreateMaskHelper(Bitmap bitmap)
		{
			var mask = new byte[bitmap.Height * ((bitmap.Width + 7) / 8)];
			var pi = 0;
			var mi = 0;
			int width = bitmap.Width;
			int height = bitmap.Height;
			Color4[] pixels = bitmap.GetPixels();
			for (int y = 0; y < height; y++) {
				byte value = 0;
				for (int x = 0; x < width; x++) {
					if (pixels[pi].A > OpacityTheshold) {
						value |= 1;
					}
					if (((x + 1) & 7) == 0) {
						mask[mi++] = value;
						value = 0;
					}
					value <<= 1;
					pi++;
				}
				if (width % 8 != 0) {
					mask[mi++] = value;
				}
			}
			if (mi != mask.Length) {
				throw new Lime.Exception("Opacity mask is not full.");
			}
			return mask;
		}
	}
}
