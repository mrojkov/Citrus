#if WIN || MAC
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using ProtoBuf;

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
			// System.Drawing.Bitmap требует, чтобы stream оставалс€ открытым всЄ врем€ существовани€ битмапа.
			// http://stackoverflow.com/questions/336387/image-save-throws-a-gdi-exception-because-the-memory-stream-is-closed
			// “ак как мы не можем быть уверены, что снаружи стрим не уничтожат, копируем его.
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

		private void InitWithPngOrJpgBitmap(Stream stream)
		{
			bitmap = new SD.Bitmap(stream);
		}
	}
}
#endif