#if UNITY
using System;
using System.IO;
using System.Linq;

namespace Lime
{
	class BitmapImplementation : IBitmapImplementation
	{
		public BitmapImplementation (Color4[] data, int width, int height):this(ToUnityColors(data), width, height)
		{
		}

		private BitmapImplementation (UnityEngine.Color32[] data, int width, int height)
		{
			Bitmap = new UnityEngine.Texture2D(width, height);
			Bitmap.SetPixels32(data);
		}

		public BitmapImplementation (Stream stream)
		{
			Bitmap = new UnityEngine.Texture2D(0, 0);

			byte[] imageData = new byte[stream.Length];
			stream.Read(imageData, 0, (int)stream.Length);
			Bitmap.LoadImage(imageData);
		}

		private BitmapImplementation (UnityEngine.Texture2D bitmap)
		{
			Bitmap = bitmap;
		}

		public UnityEngine.Texture2D Bitmap  { get; private set; }

		public int Width
		{
			get { return Bitmap == null ? 0 : Bitmap.width; }
		}

		public int Height
		{
			get { return Bitmap == null ? 0 : Bitmap.height; }
		}

		public bool IsValid
		{
			get { return (Bitmap != null && (Width > 0 && Height > 0)); }
		}

		private static UnityEngine.Color32[] ToUnityColors(Color4[] colors)
		{
			var res = new UnityEngine.Color32[colors.Length];
			for (int i = 0; i < colors.Length; ++i) {
				res[i] = new UnityEngine.Color32(colors[i].R, colors[i].G, colors[i].B, colors[i].A);
			}
			return res;
		}

		public IBitmapImplementation Clone()
		{
			return new BitmapImplementation(UnityEngine.Object.Instantiate(Bitmap));
		}

		public IBitmapImplementation Crop(IntRectangle cropArea)
		{
			var pixels = Bitmap.GetPixels(cropArea.A.X, cropArea.A.Y, cropArea.Width, cropArea.Height);
			var res = new UnityEngine.Texture2D(cropArea.Width, cropArea.Height);
			res.SetPixels(pixels);
			return new BitmapImplementation(res);
		}

		public IBitmapImplementation Rescale(int newWidth, int newHeight)
		{
			var res = UnityEngine.Object.Instantiate(Bitmap);
			res.Resize(newWidth, newHeight);
			return new BitmapImplementation(res);
		}

		public Color4[] GetPixels()
		{
			var original = Bitmap.GetPixels32();
			var res = new Color4[Width * Height];
			for (int i = 0; i < original.Length; ++i) {
				res[i] = new Color4(original[i].r, original[i].g, original[i].b, original[i].a);
			}
			return res;
		}

		public void SaveTo(Stream stream)
		{
			var png = Bitmap.EncodeToPNG();
			if (png != null) {
				using (var bitmapStream = new MemoryStream(png)) {
					bitmapStream.CopyTo(stream);
				}
			}
		}

		public void Dispose()
		{
			UnityEngine.Object.Destroy(Bitmap);
		}
	}
}
#endif