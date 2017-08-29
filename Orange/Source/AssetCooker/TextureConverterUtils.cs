using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Lime;

namespace Orange
{
	public static class TextureConverterUtils
	{
		public static Bitmap BleedAlpha(Bitmap bitmap)
		{
			if (!bitmap.HasAlpha || (bitmap.Width == 1 && bitmap.Height == 1)) {
				return bitmap;
			}
			var pixels = bitmap.GetPixels();
			BleedAlpha(pixels, bitmap.Width, bitmap.Height);
			return new Bitmap(pixels, bitmap.Width, bitmap.Height);
		}

		private static void BleedAlpha(Color4[] image, int width, int height, int radius = 8)
		{
			var processed = new bool[image.Length];
			var pending = new List<int>(image.Length);
			var pendingNext = new List<int>(image.Length);
			var hOffsets = new[] { -1, 0, 1, -1, 1, -1, 0, 1 };
			var vOffsets = new[] { -1, -1, -1, 0, 0, 1, 1, 1 };
			for (int i = 0; i < image.Length; i++) {
				if (image[i].A != 0) {
					processed[i] = true;
				} else {
					// Pend transparent pixel if it has any non-transparent adjacent pixel.
					int x = i % width;
					int y = i / width;
					for (int j = 0; j < hOffsets.Length; j++) {
						int hOffset = hOffsets[j];
						int vOffset = vOffsets[j];
						if (x + hOffset >= 0 && x + hOffset < width && y + vOffset >= 0 && y + vOffset < height) {
							int index = i + (vOffset * width + hOffset);
							if (image[index].A != 0) {
								pending.Add(i);
								break;
							}
						}
					}
				}
			}
			while (pending.Count > 0) {
				pendingNext.Clear();
				foreach (var i in pending) {
					if (!processed[i]) {
						processed[i] = true;
						int x = i % width;
						int y = i / width;
						int r, g, b, count;
						r = g = b = count = 0;
						for (int j = 0; j < hOffsets.Length; j++) {
							int hOffset = hOffsets[j];
							int vOffset = vOffsets[j];
							if (x + hOffset >= 0 && x + hOffset < width && y + vOffset >= 0 && y + vOffset < height) {
								int index = i + (vOffset * width + hOffset);
								if (processed[index]) {
									Color4 color = image[index];
									r += color.R;
									g += color.G;
									b += color.B;
									count++;
								} else {
									pendingNext.Add(index);
								}
							}
						}
						if (count == 0) {
							throw new InvalidOperationException("Pending pixel has no non-transparent adjacent pixel.");
						}
						if (radius > 0) {
							image[i] = new Color4((byte) (r / count), (byte) (g / count), (byte) (b / count), 0);
						} else {
							image[i] = Color4.Zero;
						}
					}
				}
				Lime.Toolbox.Swap(ref pending, ref pendingNext);
				radius--;
			}
		}

		public static void SaveToTGA(Bitmap bitmap, string path, bool swapRedAndBlue)
		{
			using (var stream = new FileStream(path, FileMode.Create)) {
				using (var writer = new BinaryWriter(stream)) {
					int width = bitmap.Width;
					int height = bitmap.Height;
					bool hasAlpha = bitmap.HasAlpha;
					writer.Write((byte) 0); // size of ID field that follows 18 byte header(0 usually)
					writer.Write((byte) 0); // type of color map 0 = none, 1 = has palette
					writer.Write((byte) 2); // type of image 0 = none, 1 = indexed, 2 = rgb, 3 = grey, +8 = rle packed
					writer.Write((short) 0); // first color map entry in palette
					writer.Write((short) 0); // number of colors in palette
					writer.Write((byte) 0); // number of bits per palette entry 15,16,24,32
					writer.Write((short) 0); // image x origin
					writer.Write((short) 0); // image y origin
					writer.Write((short) width); // image width in pixels
					writer.Write((short) height); // image height in pixels
					writer.Write((byte) (hasAlpha ? 32 : 24)); // image bits per pixel 8,16,24,32
					writer.Write((byte) 0); // descriptor
					Color4[] pixels = bitmap.GetPixels();
					var bytes = new byte[hasAlpha ? pixels.Length * 4 : pixels.Length * 3];
					int bi = 0;
					for (int y = height - 1; y >= 0; y--) {
						int rowsOffset = y * width;
						for (int x = 0; x < width; x++) {
							Color4 pixel = pixels[x + rowsOffset];
							if (swapRedAndBlue) {
								bytes[bi++] = pixel.B;
								bytes[bi++] = pixel.G;
								bytes[bi++] = pixel.R;
							}
							else {
								bytes[bi++] = pixel.R;
								bytes[bi++] = pixel.G;
								bytes[bi++] = pixel.B;
							}
							if (hasAlpha) {
								bytes[bi++] = pixel.A;
							}
						}
					}
					writer.Write(bytes, 0, bytes.Length);
				}
			}
		}

		public static bool GetPngFileInfo(string path, out int width, out int height, out bool hasAlpha, bool fromAssetBundle)
		{
			width = height = 0;
			hasAlpha = false;
			using (var stream = fromAssetBundle ? AssetBundle.Instance.OpenFile(path) : new FileStream(path, FileMode.Open)) {
				using (var reader = new BinaryReader(stream)) {
					byte[] sign = reader.ReadBytes(8); // PNG signature
					if (sign[1] != 'P' || sign[2] != 'N' || sign[3] != 'G')
						return false;
					reader.ReadBytes(4);
					reader.ReadBytes(4); // 'IHDR'
					width = IPAddress.NetworkToHostOrder(reader.ReadInt32());
					height = IPAddress.NetworkToHostOrder(reader.ReadInt32());
					reader.ReadByte(); // color depth
					int colorType = reader.ReadByte();
					hasAlpha = (colorType == 4) || (colorType == 6);
				}
			}
			return true;
		}

		public static int GetNearestPowerOf2(int x, int min, int max)
		{
			int y = GetNearestPowerOf2Helper(x);
			x = (y - x < x - y / 2) ? y : y / 2;
			x = Math.Max(Math.Min(max, x), min);
			return x;
		}

		static int GetNearestPowerOf2Helper(int value)
		{
			if (!IsPowerOf2(value)) {
				int i = 1;
				while (i < value)
					i *= 2;
				return i;
			}
			return value;
		}

		public static bool IsPowerOf2(int value)
		{
			return value == 1 || (value & (value - 1)) == 0;
		}
	}
}
