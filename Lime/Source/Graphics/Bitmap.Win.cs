#if WIN
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SD = System.Drawing;

namespace Lime
{
	internal class BitmapImplementation : IBitmapImplementation
	{
		private IntPtr data;

		public BitmapImplementation(Stream stream)
		{
			// System.Drawing.Bitmap требует, чтобы поток оставался открытым всё время существования битмапа.
			// http://goo.gl/oBBW6G
			// Так как мы не можем быть уверены, что снаружи поток не уничтожат, копируем его.
			var streamClone = new MemoryStream();
			stream.CopyTo(streamClone);
			Bitmap = new SD.Bitmap(streamClone);
			HasAlpha = IsReallyHasAlpha(Bitmap);
		}

		public BitmapImplementation(Color4[] colors, int width, int height)
		{
			const SD.Imaging.PixelFormat Format = SD.Imaging.PixelFormat.Format32bppArgb;
			var stride = 4 * width;
			data = CreateMemoryCopy(colors);
			Bitmap = new SD.Bitmap(width, height, stride, Format, data);
			HasAlpha = colors.Any(color => color.A != 255);
		}

		private BitmapImplementation(SD.Bitmap bitmap)
		{
			Bitmap = bitmap;
		}

		public SD.Bitmap Bitmap { get; private set; }

		public int Width
		{
			get { return Bitmap == null ? 0 : Bitmap.Width; }
		}

		public int Height
		{
			get { return Bitmap == null ? 0 : Bitmap.Height; }
		}

		public bool IsValid
		{
			get
			{
				return
					!disposed &&
					Bitmap != null &&
					Bitmap.Height > 0 &&
					Bitmap.Width > 0;
			}
		}

		public bool HasAlpha
		{
			get; private set;
		}

		public IBitmapImplementation Clone()
		{
			return new BitmapImplementation((SD.Bitmap)Bitmap.Clone());
		}

		public IBitmapImplementation Crop(IntRectangle cropArea)
		{
			var rect = new SD.Rectangle(cropArea.Left, cropArea.Top, cropArea.Width, cropArea.Height);
			return new BitmapImplementation(Bitmap.Clone(rect, Bitmap.PixelFormat));
		}

		public IBitmapImplementation Rescale(int newWidth, int newHeight)
		{
			return new BitmapImplementation(new SD.Bitmap(Bitmap, newWidth, newHeight));
		}

		public Color4[] GetPixels()
		{
			var bmpData = Bitmap.LockBits(
				new SD.Rectangle(0, 0, Bitmap.Width, Bitmap.Height),
				SD.Imaging.ImageLockMode.ReadOnly,
				SD.Imaging.PixelFormat.Format32bppArgb);
			if (bmpData.Stride != bmpData.Width * 4) {
				throw new FormatException("Bitmap stride does not match its width");
			}

			var numBytes = bmpData.Width * Bitmap.Height * 4;
			var pixelsArray = ArrayFromPointer(bmpData.Scan0, numBytes / 4);
			Bitmap.UnlockBits(bmpData);
			return pixelsArray;
		}

		public void SaveTo(Stream stream)
		{
			Bitmap.Save(stream, SD.Imaging.ImageFormat.Png);
		}

		private static Color4[] ArrayFromPointer(IntPtr data, int arraySize)
		{
			var array = new Color4[arraySize];
			unsafe
			{
				var ptr = (Color4*)data;
				for (int i = 0; i < array.Length; i++) {
					var c = *ptr++;

					// Swap R and B again
					array[i] = new Color4(c.B, c.G, c.R, c.A);
				}
			}
			return array;
		}

		private static IntPtr CreateMemoryCopy(Color4[] pixels)
		{
			var lengthInBytes = pixels.Length * 4;
			var data = Marshal.AllocHGlobal(lengthInBytes);

			// Copy pixels to data, swap r & b
			unsafe
			{
				var pixelsPtr = (Color4*)data;
				foreach (var c in pixels) {
					*pixelsPtr++ = new Color4(c.B, c.G, c.R, c.A);
				}
			}
			return data;
		}

		private static bool IsReallyHasAlpha(SD.Bitmap bitmap)
		{
			var bmpData = bitmap.LockBits(
				new SD.Rectangle(0, 0, bitmap.Width, bitmap.Height),
				SD.Imaging.ImageLockMode.ReadOnly,
				bitmap.PixelFormat);
			var bytes = new byte[bmpData.Height * Math.Abs(bmpData.Stride)];
			Marshal.Copy(bmpData.Scan0, bytes, 0, bytes.Length);
			for (int p = 3; p < bytes.Length; p += 4) {
				if (bytes[p] != 255) {
					bitmap.UnlockBits(bmpData);
					return true;
				}
			}
			bitmap.UnlockBits(bmpData);
			return false;
		}

		#region IDisposable Support
		private bool disposed;

		private void Dispose(bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					if (Bitmap != null) {
						Bitmap.Dispose();
					}
				}

				if (data != IntPtr.Zero) {
					Marshal.FreeHGlobal(data);
				}

				disposed = true;
			}
		}

		~BitmapImplementation()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
#endif
