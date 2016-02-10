#if iOS
using System;
using System.IO;
using CoreGraphics;
using UIKit;

namespace Lime
{
	class BitmapImplementation : IBitmapImplementation
	{
		private UIImage bitmap;

		public BitmapImplementation() { }

		public BitmapImplementation(Stream stream)
		{
			LoadFromStream(stream);
		}

		public BitmapImplementation(Color4[] pixels, int width, int height)
		{
			LoadFromArray(pixels, width, height);
		}

		public int GetWidth()
		{
			return bitmap == null ? 0 : ((float)bitmap.Size.Width).Round();
		}

		public int GetHeight()
		{
			return bitmap == null ? 0 : ((float)bitmap.Size.Height).Round();
		}

		private void LoadFromStream(Stream stream)
		{
			using (var nsData = Foundation.NSData.FromStream(stream)) {
				bitmap = UIImage.LoadFromData(nsData);
			}
		}

		private void LoadFromArray(Color4[] pixels, int width, int height)
		{
			int lengthInBytes = pixels.Length * 4;
			var data = new byte[lengthInBytes];
			int j = 0;
			for (int i = 0; i < pixels.Length; i++) {
				data[j++] = pixels[i].R;
				data[j++] = pixels[i].G;
				data[j++] = pixels[i].B;
				data[j++] = pixels[i].A;
			}
			using (var colorSpace = CGColorSpace.CreateDeviceRGB()) {
				using (var dataProvider = new CGDataProvider(data, 0, lengthInBytes)) {
					using (var img = new CGImage(width, height, 8, 32, 4 * width, colorSpace, CGBitmapFlags.Last, dataProvider, null, false,
						CGColorRenderingIntent.Default)) {
						bitmap = new UIImage(img);
					}
				}
			}
		}

		public void SaveToStream(Stream stream)
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			if (bitmap != null) {
				using (var png = bitmap.AsPNG()) {
					if (png != null) {
						using (var bitmapStream = png.AsStream()) {
							bitmapStream.CopyTo(stream);
						}
					}
				}
			}
		}

		public IBitmapImplementation Rescale(int newWidth, int newHeight)
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			var scaledImage = bitmap.Scale(new CGSize(newWidth, newHeight));
			return new BitmapImplementation { bitmap = scaledImage };
		}

		public IBitmapImplementation Crop(IntRectangle cropArea)
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			var rect = new CGRect(cropArea.Left, cropArea.Top, cropArea.Width, cropArea.Height);
			var cgimage = bitmap.CGImage;
			cgimage = cgimage.WithImageInRect(rect);
			return new BitmapImplementation { bitmap = new UIImage(cgimage) };
		}

		public Color4[] GetPixels()
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			var isColorSpaceRGB = bitmap.CGImage.ColorSpace.Model == CGColorSpaceModel.RGB;
			if (!isColorSpaceRGB && bitmap.CGImage.BitsPerPixel != 32) {
				throw new Exception("Can not return array of pixels if bitmap is not in 32 bit format or if not in RGBA format.");
			}

			var doSwap = bitmap.CGImage.BitmapInfo.HasFlag(CGBitmapFlags.ByteOrder32Little);
			var isPremultiplied = bitmap.CGImage.AlphaInfo == CGImageAlphaInfo.PremultipliedFirst ||
				bitmap.CGImage.AlphaInfo == CGImageAlphaInfo.PremultipliedLast;
			var rowLength = bitmap.CGImage.BytesPerRow / 4;
			var width = GetWidth();
			var height = GetHeight();
			var pixels = new Color4[width * height];
			using (var data = bitmap.CGImage.DataProvider.CopyData()) {
				unsafe {
					byte* pBytes = (byte*)data.Bytes;
					byte r, g, b, a;
					int index = 0;
					for (int i = 0; i < height; i++) {
						for (int j = 0; j < width; j++) {
							r = (byte)(*pBytes++);
							g = (byte)(*pBytes++);
							b = (byte)(*pBytes++);
							a = (byte)(*pBytes++);
							if (isPremultiplied && a != 255 && a != 0) {
								var fa = a / 255f;
								r = (byte)(((r / 255f) / fa) * 255f);
								g = (byte)(((g / 255f) / fa) * 255f);
								b = (byte)(((b / 255f) / fa) * 255f);
							}
							pixels[index++] = doSwap ? new Color4(b, g, r, a) : new Color4(r, g, b, a);
						}

						// Sometimes Width can be smaller then length of a row due to byte allignment.
						// It's just an empty bytes at the end of each row, so we can skip them here.
						for (int k = 0; k < rowLength - width; k++) {
							pBytes += 4;
						}
					}
				}
			}
			return pixels;
		}

		public void Dispose()
		{
			if (bitmap != null) {
				bitmap.Dispose();
			}
		}
		
		public bool IsValid()
		{
			return (bitmap != null && (bitmap.Size.Height > 0 && bitmap.Size.Width > 0));
		}
	}
}
#endif