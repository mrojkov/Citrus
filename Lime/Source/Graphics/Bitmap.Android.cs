#if ANDROID
using System;
using Android.Graphics;
using System.IO;
using Android.Graphics.Drawables;
using AndroidBitmap = Android.Graphics.Bitmap;

namespace Lime
{
	class BitmapImplementation : IBitmapImplementation
	{
		AndroidBitmap bitmap;

		public BitmapImplementation() { }

		public BitmapImplementation(Stream stream)
		{
			LoadFromStream(stream);
		}

		public BitmapImplementation(Color4[] data, int width, int height)
		{
			LoadFromArray(data, width, height);
		}

		public int GetWidth()
		{
			return bitmap == null ? 0 : bitmap.Width;
		}

		public int GetHeight()
		{
			return bitmap == null ? 0 : bitmap.Height;
		}

		private void LoadFromStream(Stream stream)
		{
			var options = new BitmapFactory.Options();
			if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat) {
				options.InPremultiplied = false;
			}
			bitmap = BitmapFactory.DecodeStream(stream, null, options);
		}

		private void LoadFromArray(Color4[] pixels, int width, int height)
		{
			if (width * height != pixels.Length) {
				throw new Exception("Pixel data doesn't fit width and height.");
			}
			var colors = new int[pixels.Length];
			for (int i = 0; i < pixels.Length; i++) {
				var pixel = pixels[i];
				colors[i] = Color.Argb(pixel.A, pixel.R, pixel.G, pixel.B).ToArgb();
			}
			bitmap = AndroidBitmap.CreateBitmap(colors, width, height, AndroidBitmap.Config.Argb8888);
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
			if (cropArea.Width == GetWidth() && cropArea.Height == GetHeight()) {
				return new BitmapImplementation {
					bitmap = bitmap.Copy(AndroidBitmap.Config.Argb8888, isMutable: false)
				};
			}
			var croppedBitmap = Android.Graphics.Bitmap.CreateBitmap(bitmap, cropArea.Left, cropArea.Top, cropArea.Width, cropArea.Height);
			return new BitmapImplementation { bitmap = croppedBitmap };
		}

		public Color4[] GetPixels()
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			int length = bitmap.Width * bitmap.Height;
			var colors = new int[length];
			bitmap.GetPixels(colors, 0, bitmap.Width, 0, 0, bitmap.Width, bitmap.Height);
			var pixels = new Color4[length];
			int r, g, b, a;
			for (int i = 0; i < length; i++) {
				r = Color.GetRedComponent(colors[i]);
				g = Color.GetGreenComponent(colors[i]);
				b = Color.GetBlueComponent(colors[i]);
				a = Color.GetAlphaComponent(colors[i]);
				pixels[i] = new Color4((byte)r, (byte)g, (byte)b, (byte)a);
			}
			return pixels;
		}

		public void Dispose()
		{
			if (bitmap != null && !bitmap.IsRecycled) {
				bitmap.Recycle();
			}
		}

		public bool IsValid()
		{
			return (bitmap != null && !bitmap.IsRecycled && (bitmap.Height > 0 && bitmap.Width > 0));
		}
	}
}
#endif