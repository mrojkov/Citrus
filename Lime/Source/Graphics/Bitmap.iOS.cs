#if iOS
using System;
using System.Drawing;
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
			if (stream.Length == 0) {
				throw new Exception("Can not create bitmap from empty stream");
			}
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
			var scaledImage = bitmap.Scale(new SizeF(newWidth, newHeight));
			return new BitmapImplementation { bitmap = scaledImage };
		}

		public IBitmapImplementation Crop(IntRectangle cropArea)
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			var rect = new RectangleF(cropArea.Left, cropArea.Top, cropArea.Width, cropArea.Height);
			var cgimage = bitmap.CGImage;
			cgimage = cgimage.WithImageInRect(rect);
			return new BitmapImplementation { bitmap = new UIImage(cgimage) };
		}

		public Color4[] GetPixels()
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			nint bitsPerPixel = bitmap.CGImage.BitsPerPixel;
			bool isColorSpaceRGB = bitmap.CGImage.ColorSpace.Model == CGColorSpaceModel.RGB;
			if (!isColorSpaceRGB && bitsPerPixel != 32) {
				throw new Exception("Can not return array of pixels if bitmap is not in 32 bit format or if not in RGBA format.");
			}
			int arraySize = GetWidth() * GetHeight();
			var pixels = new Color4[arraySize];
			using (var data = bitmap.CGImage.DataProvider.CopyData()) {				
				unsafe {
					byte* pBytes = (byte*)data.Bytes;
					byte r, g, b, a;
					for (int i = 0; i < arraySize; i++) {
						r = (byte)(*pBytes++);
						g = (byte)(*pBytes++);
						b = (byte)(*pBytes++);
						a = (byte)(*pBytes++);
						pixels[i] = new Color4(r, g, b, a);
					}
				}
			}
			return pixels;
		}

		public void Dispose()
		{
			if (bitmap != null) {
				bitmap.Dispose();
				bitmap = null;
			}
			GC.SuppressFinalize(this);
		}

		~BitmapImplementation() 
		{
			Dispose();
		}
		
		public bool IsValid()
		{
			return (bitmap != null && (bitmap.Size.Height > 0 && bitmap.Size.Width > 0));
		}
	}
}
#endif