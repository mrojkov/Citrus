#if WIN
using System;
using System.IO;
using System.Runtime.InteropServices;
using SD = System.Drawing;

namespace Lime
{
	class BitmapImplementation : IBitmapImplementation
	{
		private IntPtr data;
		public SD.Bitmap Bitmap;

		public BitmapImplementation(Stream stream)
		{
			// System.Drawing.Bitmap требует, чтобы stream оставался открытым всё время существования битмапа.
			// http://stackoverflow.com/questions/336387/image-save-throws-a-gdi-exception-because-the-memory-stream-is-closed
			// Так как мы не можем быть уверены, что снаружи стрим не уничтожат, копируем его.
			var streamClone = new MemoryStream();
			stream.CopyTo(streamClone);
			Bitmap = new SD.Bitmap(streamClone);
		}

		public BitmapImplementation(Color4[] colors, int width, int height)
		{
			const SD.Imaging.PixelFormat Format = SD.Imaging.PixelFormat.Format32bppArgb;
			var stride = 4 * width;
			data = CreateMemoryCopy(colors);
			Bitmap = new SD.Bitmap(width, height, stride, Format, data);
		}

		private BitmapImplementation(SD.Bitmap bitmap)
		{
			Bitmap = bitmap;
		}

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
