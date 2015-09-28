#if ANDROID
using System;
using Android.Graphics;
using System.IO;
using Android.Graphics.Drawables;

namespace Lime
{
	class BitmapImplementation : IBitmapImplementation
	{
		Android.Graphics.Bitmap bitmap;

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
			// For some reason bitmap.Dispose() causes NRE on Android L.
			//if (bitmap != null) {
			//	bitmap.Dispose();
			//	bitmap = null;
			//}
			bitmap = BitmapFactory.DecodeStream(stream);
		}

		public void SaveToStream(Stream stream)
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, stream);
		}

		public IBitmapImplementation Rescale(int newWidth, int newHeight)
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			var scaledBitmap = Android.Graphics.Bitmap.CreateScaledBitmap(bitmap, newWidth, newHeight, true);
			return new BitmapImplementation() { bitmap = scaledBitmap };
		}

		public IBitmapImplementation Crop(IntRectangle cropArea)
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			var croppedBitmap = Android.Graphics.Bitmap.CreateBitmap(bitmap, cropArea.Left, cropArea.Top, cropArea.Width, cropArea.Height);
			return new BitmapImplementation { bitmap = croppedBitmap };
		}

		public void Dispose()
		{
			// For some reason bitmap.Dispose() causes NRE on Android L.
		}

		public bool IsValid()
		{
			return (bitmap != null && (bitmap.Height > 0 && bitmap.Width > 0));
		}
	}
}
#endif