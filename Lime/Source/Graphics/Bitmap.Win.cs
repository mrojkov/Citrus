#if WIN
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using ProtoBuf;
using System.Runtime.InteropServices;
using System.Windows.Forms.VisualStyles;
using SD = System.Drawing;

namespace Lime
{
	class BitmapImplementation : IBitmapImplementation
	{
		private SD.Bitmap bitmap;

		public int GetWidth()
		{
			return bitmap == null ? 0 : bitmap.Width;
		}

		public int GetHeight()
		{
			return bitmap == null ? 0 : bitmap.Height;
		}

		public void LoadFromStream(Stream stream)
		{
			// System.Drawing.Bitmap требует, чтобы stream оставался открытым всё время существования битмапа.
			// http://stackoverflow.com/questions/336387/image-save-throws-a-gdi-exception-because-the-memory-stream-is-closed
			// Так как мы не можем быть уверены, что снаружи стрим не уничтожат, копируем его.
			Dispose();
			var streamClone = new MemoryStream();
			stream.CopyTo(streamClone);
			bitmap = new SD.Bitmap(streamClone);
		}

		public void SaveToStream(Stream stream)
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			bitmap.Save(stream, SD.Imaging.ImageFormat.Png);
		}

		public IBitmapImplementation Crop(IntRectangle cropArea)
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			var rect = new SD.Rectangle(cropArea.Left, cropArea.Top, cropArea.Width, cropArea.Height);
			var croppedBitmap = bitmap.Clone(rect, bitmap.PixelFormat);
			return new BitmapImplementation {bitmap = croppedBitmap};
		}

		public IBitmapImplementation Rescale(int newWidth, int newHeight)
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			var rescaledBitmap = new SD.Bitmap(bitmap, newWidth, newHeight);
			return new BitmapImplementation { bitmap = rescaledBitmap };
		}

		public void Dispose()
		{
			if (bitmap != null) {
				bitmap.Dispose();
				bitmap = null;
			}
			GC.SuppressFinalize(this);
		}

		public byte[] GetImageData()
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			var lockRect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
			var lockMode = System.Drawing.Imaging.ImageLockMode.ReadOnly;
			var data = bitmap.LockBits(lockRect, lockMode, bitmap.PixelFormat);		
			byte[] pixelData = new byte[data.Stride * data.Height];

			Marshal.Copy(data.Scan0, pixelData, 0, pixelData.Length);
			bitmap.UnlockBits(data);

			return pixelData;
		}

		public bool IsValid()
		{
			return (bitmap != null && (bitmap.Height > 0 && bitmap.Width > 0));
		}

		~BitmapImplementation()
		{
			Dispose();
		}
	}
}
#endif