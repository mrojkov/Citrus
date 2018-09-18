#if WIN
using System;
using System.Drawing.Imaging;
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
			HasAlpha = SD.Image.IsAlphaPixelFormat(Bitmap.PixelFormat) && IsReallyHasAlpha(Bitmap);

		}

		public BitmapImplementation(Color4[] colors, int width, int height)
		{
			const PixelFormat Format = PixelFormat.Format32bppArgb;
			var stride = 4 * width;
			data = CreateMemoryCopy(colors);
			Bitmap = new SD.Bitmap(width, height, stride, Format, data);
			HasAlpha = Lime.Bitmap.AnyAlpha(colors);
		}

		private BitmapImplementation(SD.Bitmap bitmap)
		{
			Bitmap = bitmap;
			HasAlpha = SD.Image.IsAlphaPixelFormat(Bitmap.PixelFormat) && IsReallyHasAlpha(Bitmap);
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
				ImageLockMode.ReadOnly,
				PixelFormat.Format32bppArgb);
			if (bmpData.Stride != bmpData.Width * 4) {
				throw new FormatException("Bitmap stride does not match its width");
			}

			var numBytes = bmpData.Width * Bitmap.Height * 4;
			var pixelsArray = ArrayFromPointer(bmpData.Scan0, numBytes / 4);
			Bitmap.UnlockBits(bmpData);
			return pixelsArray;
		}

		public void SaveTo(Stream stream, CompressionFormat compression)
		{
			switch (compression) {
				case CompressionFormat.Jpeg:
					ImageCodecInfo codec = ImageCodecInfo.GetImageEncoders().First(enc => enc.MimeType == "image/jpeg");
					var parameters = new EncoderParameters();
					parameters.Param[0] = new EncoderParameter(Encoder.Quality, 80L);
					Bitmap.Save(stream, codec, parameters);
					break;
				case CompressionFormat.Png:
					Bitmap.Save(stream, ImageFormat.Png);
					break;
			}
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

		private static unsafe bool IsReallyHasAlpha(SD.Bitmap bitmap)
		{
			var bmpData = bitmap.LockBits(
				new SD.Rectangle(0, 0, bitmap.Width, bitmap.Height),
				SD.Imaging.ImageLockMode.ReadOnly,
				bitmap.PixelFormat);
			try {
				int lengthInBytes = bmpData.Height * Math.Abs(bmpData.Stride);
				var pointer = (byte*)bmpData.Scan0 + 3;
				for (int i = 3; i < lengthInBytes; i += 4) {
					if (*pointer != 255) {
						return true;
					}
					pointer += 4;
				}
				return false;
			} finally {
				bitmap.UnlockBits(bmpData);
			}
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
