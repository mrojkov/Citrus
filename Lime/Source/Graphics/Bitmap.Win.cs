#if WIN
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using ProtoBuf;
using System.Runtime.InteropServices;

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
			// System.Drawing.Bitmap �������, ����� stream ��������� �������� �� ����� ������������� �������.
			// http://stackoverflow.com/questions/336387/image-save-throws-a-gdi-exception-because-the-memory-stream-is-closed
			// ��� ��� �� �� ����� ���� �������, ��� ������� ����� �� ���������, �������� ���.
			var streamClone = new MemoryStream();
			Toolbox.CopyStream(stream, streamClone);

			InitWithPngOrJpgBitmap(streamClone);
		}

		public void SaveToStream(Stream stream)
		{
			if (bitmap != null) {
				bitmap.Save(stream, SD.Imaging.ImageFormat.Png);
			}
		}

		public IBitmapImplementation Crop(IntRectangle cropArea)
		{
			var rect = new SD.Rectangle(cropArea.Left, cropArea.Top, cropArea.Width, cropArea.Height);
			var croppedBitmap = bitmap.Clone(rect, bitmap.PixelFormat);
			var result = new BitmapImplementation();
			result.bitmap = croppedBitmap;
			return result;
		}

		public IBitmapImplementation Rescale(int newWidth, int newHeight)
		{
			var rescaledBitmap = new SD.Bitmap(bitmap, newWidth, newHeight);
			var result = new BitmapImplementation();
			result.bitmap = rescaledBitmap;
			return result;
		}

		public void Dispose()
		{
			if (bitmap != null) {
				bitmap.Dispose();
			}
		}

		public byte[] GetImageData()
		{		
			var lockRect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
			var lockMode = System.Drawing.Imaging.ImageLockMode.ReadOnly;
			var data = bitmap.LockBits(lockRect, lockMode, bitmap.PixelFormat);		
			byte[] pixelData = new byte[data.Stride * data.Height];

			Marshal.Copy(data.Scan0, pixelData, 0, pixelData.Length);
			bitmap.UnlockBits(data);

			return pixelData;
		}

		private void InitWithPngOrJpgBitmap(Stream stream)
		{
			try {
				bitmap = new SD.Bitmap(stream);
			} catch (System.Exception e) {
				Logger.Write("Bitmap.Win: failed to load bitmap. {0}", e.Message);
			}

		}
	}
}
#endif