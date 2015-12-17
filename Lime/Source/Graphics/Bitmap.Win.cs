#if WIN
using System;
using System.IO;
using System.Runtime.InteropServices;
using SD = System.Drawing;

namespace Lime
{
	class BitmapImplementation : IBitmapImplementation
	{
		private SD.Bitmap bitmap;
		private IntPtr data;

		public BitmapImplementation()
		{
		}

		public BitmapImplementation(Stream stream)
		{
			if (stream.Length == 0) {
				throw new Exception("Can not create bitmap from empty stream");
			}
			LoadFromStream(stream);
		}

		public BitmapImplementation(Color4[] data, int width, int height)
		{
			LoadFromArray(data, width, height);
		}

		public IBitmapImplementation Crop(IntRectangle cropArea)
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			var rect = new SD.Rectangle(cropArea.Left, cropArea.Top, cropArea.Width, cropArea.Height);
			var croppedBitmap = bitmap.Clone(rect, bitmap.PixelFormat);
			return new BitmapImplementation { bitmap = croppedBitmap };
		}

		public int GetHeight()
		{
			return bitmap == null ? 0 : bitmap.Height;
		}

		public Color4[] GetPixels()
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			var bmpData = bitmap.LockBits(
				new SD.Rectangle(0, 0, bitmap.Width, bitmap.Height),
				SD.Imaging.ImageLockMode.ReadOnly,
				SD.Imaging.PixelFormat.Format32bppArgb);
			if (bmpData.Stride != bmpData.Width * 4) {
				throw new Exception("Bitmap stride does not match its width");
			}
			var numBytes = bmpData.Width * bitmap.Height * 4;
			var pixelsArray = ArrayFromPointer(bmpData.Scan0, numBytes / 4);
			bitmap.UnlockBits(bmpData);
			return pixelsArray;
		}

		public int GetWidth()
		{
			return bitmap == null ? 0 : bitmap.Width;
		}

		public bool IsValid()
		{
			return !disposed && bitmap != null && (bitmap.Height > 0 && bitmap.Width > 0);
		}

		public IBitmapImplementation Rescale(int newWidth, int newHeight)
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			var rescaledBitmap = new SD.Bitmap(bitmap, newWidth, newHeight);
			return new BitmapImplementation { bitmap = rescaledBitmap };
		}

		public void SaveToStream(Stream stream)
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			bitmap.Save(stream, SD.Imaging.ImageFormat.Png);
		}

		private static Color4[] ArrayFromPointer(IntPtr data, int arraySize)
		{
			var array = new Color4[arraySize];
			unsafe
			{
				var ptr = (Color4*)data;
				for (int i = 0; i < array.Length; i++) {
					var c = *ptr++;

					// swap R and B again
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

		private void LoadFromArray(Color4[] pixels, int width, int height)
		{
			if (width * height != pixels.Length) {
				throw new Exception("Pixel data doesn't fit width and height.");
			}
			const SD.Imaging.PixelFormat Format = SD.Imaging.PixelFormat.Format32bppArgb;
			var stride = 4 * width;
			data = CreateMemoryCopy(pixels);
			bitmap = new SD.Bitmap(width, height, stride, Format, data);
		}

		private void LoadFromStream(Stream stream)
		{
			// System.Drawing.Bitmap требует, чтобы stream оставался открытым всё время существования битмапа.
			// http://stackoverflow.com/questions/336387/image-save-throws-a-gdi-exception-because-the-memory-stream-is-closed
			// Так как мы не можем быть уверены, что снаружи стрим не уничтожат, копируем его.
			var streamClone = new MemoryStream();
			stream.CopyTo(streamClone);
			bitmap = new SD.Bitmap(streamClone);
		}

		#region IDisposable Support
		private bool disposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					if (bitmap != null) {
						bitmap.Dispose();
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
