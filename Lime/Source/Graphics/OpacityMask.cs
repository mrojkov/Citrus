using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lime
{
	public class OpacityMask
	{
		public int Width { get; private set; }
		public int Height { get; private set; }

		public int Stride { get; private set; }

		public byte[] Data { get; private set; }
		public OpacityMask(string path)
		{
			using (var stream = AssetBundle.Current.OpenFileLocalized(path)) {
				using (var reader = new BinaryReader(stream)) {
					Width = reader.ReadInt32();
					Height = reader.ReadInt32();
					Stride = (Width + 7) / 8;
					Data = reader.ReadBytes(Stride * Height);
				}
			}
		}

		public bool TestPixel(int x, int y)
		{
			if (x < 0 || y < 0 || x >= Width || y >= Height) {
				return false;
			}
			var b = Data[y * Stride + x / 8];
			bool result = (b & (1 << (7 - x % 8))) != 0;
			return result;
		}
	}
}