#if ANDROID
using System.IO;
using Android.Graphics;
using Android.OS;
using AndroidBitmap = Android.Graphics.Bitmap;

namespace Lime
{
	internal class BitmapImplementation : IBitmapImplementation
	{
		public BitmapImplementation(Stream stream)
		{
			var options = new BitmapFactory.Options();
			if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat) {
				options.InPremultiplied = false;
			}
			Bitmap = BitmapFactory.DecodeStream(stream, null, options);
		}

		public BitmapImplementation(Color4[] data, int width, int height)
		{
			var colors = new int[data.Length];
			for (int i = 0; i < data.Length; i++) {
				var pixel = data[i];
				colors[i] = Color4.CreateArgb(pixel.A, pixel.R, pixel.G, pixel.B);
			}
			Bitmap = AndroidBitmap.CreateBitmap(colors, width, height, AndroidBitmap.Config.Argb8888);
			Bitmap.HasAlpha = Lime.Bitmap.AnyAlpha(data);
		}

		private BitmapImplementation(AndroidBitmap bitmap)
		{
			Bitmap = bitmap;
			Bitmap.HasAlpha = Lime.Bitmap.AnyAlpha(GetPixels());
		}

		public AndroidBitmap Bitmap { get; private set; }

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
					Bitmap != null &&
					!Bitmap.IsRecycled &&
					Bitmap.Height > 0 &&
					Bitmap.Width > 0;
			}
		}

		public bool HasAlpha
		{
			get { return Bitmap.HasAlpha; }
		}

		public IBitmapImplementation Clone()
		{
			return new BitmapImplementation(Bitmap.Copy(
				AndroidBitmap.Config.Argb8888,
				isMutable: false));
		}

		public IBitmapImplementation Rescale(int newWidth, int newHeight)
		{
			return new BitmapImplementation(
				AndroidBitmap.CreateScaledBitmap(
					Bitmap,
					newWidth,
					newHeight,
					filter: true));
		}

		public IBitmapImplementation Crop(IntRectangle cropArea)
		{
			return new BitmapImplementation(
				AndroidBitmap.CreateBitmap(
					Bitmap,
					cropArea.Left,
					cropArea.Top,
					cropArea.Width,
					cropArea.Height));
		}

		public Color4[] GetPixels()
		{
			int length = Bitmap.Width * Bitmap.Height;
			var colors = new int[length];
			Bitmap.GetPixels(colors, 0, Bitmap.Width, 0, 0, Bitmap.Width, Bitmap.Height);
			var pixels = new Color4[length];
			for (int i = 0; i < length; i++) {
				pixels[i] = new Color4(
					Color4.GetRedComponent(colors[i]),
					Color4.GetGreenComponent(colors[i]),
					Color4.GetBlueComponent(colors[i]),
					Color4.GetAlphaComponent(colors[i]));
			}
			return pixels;
		}

		public void SaveTo(Stream stream, CompressionFormat compression)
		{
			switch (compression) {
				case CompressionFormat.Jpeg:
					Bitmap.Compress(AndroidBitmap.CompressFormat.Jpeg, 80, stream);
					break;
				case CompressionFormat.Png:
					Bitmap.Compress(AndroidBitmap.CompressFormat.Png, 100, stream);
					break;
			}
		}

		public void Dispose()
		{
			if (Bitmap != null && !Bitmap.IsRecycled) {
				Bitmap.Recycle();
			}
		}
	}
}
#endif
