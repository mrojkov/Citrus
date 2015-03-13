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
			bitmap = BitmapFactory.DecodeStream(stream);
		}

		public void SaveToStream(Stream stream)
		{
			bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, stream);
		}

		public IBitmapImplementation Rescale(int newWidth, int newHeight)
		{
			var scaledBitmap = Android.Graphics.Bitmap.CreateScaledBitmap(bitmap, newWidth, newHeight, true);
			var result = new BitmapImplementation();
			result.bitmap = scaledBitmap;
			return result;
		}

		public IBitmapImplementation Crop(IntRectangle cropArea)
		{
			var croppedBitmap = Android.Graphics.Bitmap.CreateBitmap(bitmap, cropArea.Left, cropArea.Top, cropArea.Width, cropArea.Height);
			var result = new BitmapImplementation();
			result.bitmap = croppedBitmap;
			return result;
		}

		public void Dispose()
		{
			if (bitmap != null) {
				bitmap.Dispose();
				bitmap = null;
			}
		}
	}
}
#endif