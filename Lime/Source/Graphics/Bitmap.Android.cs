#if ANDROID
using System;
using System.IO;

using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using AndroidBitmap = Android.Graphics.Bitmap;

namespace Lime
{
	class BitmapImplementation : IBitmapImplementation
	{
		public AndroidBitmap bitmap;

		public BitmapImplementation(Stream stream)
		{
			var options = new BitmapFactory.Options();
			if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat) {
				options.InPremultiplied = false;
			}
			bitmap = BitmapFactory.DecodeStream(stream, null, options);
		}

		public BitmapImplementation(Color4[] data, int width, int height)
		{
			var colors = new int[data.Length];
			for (int i = 0; i < data.Length; i++) {
				var pixel = data[i];
				colors[i] = Color.Argb(pixel.A, pixel.R, pixel.G, pixel.B).ToArgb();
			}
			bitmap = AndroidBitmap.CreateBitmap(colors, width, height, AndroidBitmap.Config.Argb8888);
		}

		private BitmapImplementation(AndroidBitmap bitmap)
		{
			this.bitmap = bitmap;
		}

		public int Width
		{
			get { return bitmap == null ? 0 : bitmap.Width; }
		}

		public int Height
		{
			get { return bitmap == null ? 0 : bitmap.Height; }
		}

		public bool IsValid
		{
			get
			{
				return
					bitmap != null &&
					!bitmap.IsRecycled &&
					bitmap.Height > 0 &&
					bitmap.Width > 0;
			}
		}

		public IBitmapImplementation Clone()
		{
			return new BitmapImplementation(bitmap.Copy(
				AndroidBitmap.Config.Argb8888,
				isMutable: false));
		}

		public IBitmapImplementation Rescale(int newWidth, int newHeight)
		{
			return new BitmapImplementation(
				AndroidBitmap.CreateScaledBitmap(
					bitmap,
					newWidth,
					newHeight,
					filter: true));
		}

		public IBitmapImplementation Crop(IntRectangle cropArea)
		{
			return new BitmapImplementation(
				AndroidBitmap.CreateBitmap(
					bitmap,
					cropArea.Left,
					cropArea.Top,
					cropArea.Width,
					cropArea.Height));
		}

		public Color4[] GetPixels()
		{
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

		public void SaveTo(Stream stream)
		{
			bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, stream);
		}

		public void Dispose()
		{
			if (bitmap != null && !bitmap.IsRecycled) {
				bitmap.Recycle();
			}
		}
	}
}
#endif