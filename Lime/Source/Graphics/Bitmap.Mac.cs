#if MAC || MONOMAC
using System;
using System.IO;
using AppKit;
using CoreGraphics;

namespace Lime
{
	class BitmapImplementation : IBitmapImplementation
	{
		private NSImage image;

		public BitmapImplementation() {}

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
			return image != null ? (int)image.Size.Width : 0;
		}

		public int GetHeight()
		{
			return image != null ? (int)image.Size.Height : 0;
		}

		private void LoadFromStream(Stream stream)
		{
			image = NSImage.FromStream(stream);
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
						image = new NSImage(img, new CGSize(width, height));
					}
				}
			}
		}

		public void SaveToStream(Stream stream)
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			if (image != null) {
				using (var cgImage = image.CGImage)
				using (var rep = new NSBitmapImageRep(cgImage)) {
					rep.Size = image.Size;
					using (var pngData = rep.RepresentationUsingTypeProperties(NSBitmapImageFileType.Png, null))
					using (var bitmapStream = pngData.AsStream()) {
						bitmapStream.CopyTo(stream);
					}
				}
			}
		}

		public IBitmapImplementation Crop(IntRectangle cropArea)
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			var rect = new CGRect(cropArea.Left, cropArea.Top, cropArea.Width, cropArea.Height);
			var cgimage = image.CGImage.WithImageInRect(rect);
			var size = new CGSize(cgimage.Width, cgimage.Height);
			return new BitmapImplementation() { image = new NSImage(cgimage, size) };
		}

		public IBitmapImplementation Rescale(int newWidth, int newHeight)
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			var newImage = new NSImage(new CGSize(newWidth, newHeight));
			newImage.LockFocus();
			var ctx = NSGraphicsContext.CurrentContext;
			ctx.ImageInterpolation = NSImageInterpolation.High;
			image.DrawInRect(
				new CGRect(0, 0, newWidth, newHeight), 
				new CGRect(0, 0, image.Size.Width, image.Size.Height), 
				NSCompositingOperation.Copy, 1); 
			newImage.UnlockFocus();
			return new BitmapImplementation() { image = newImage };
		}

		public Color4[] GetPixels()
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			nint bitsPerPixel = image.CGImage.BitsPerPixel;
			bool isColorSpaceRGB = image.CGImage.ColorSpace.Model == CGColorSpaceModel.RGB;
			if (!isColorSpaceRGB && bitsPerPixel != 32) {
				throw new Exception("Can not return array of pixels if bitmap is not in 32 bit format or if not in RGBA format.");
			}
			int arraySize = GetWidth() * GetHeight();
			var pixels = new Color4[arraySize];
			using (var data = image.CGImage.DataProvider.CopyData()) {				
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
			if (image != null) {
				image.Dispose();
				image = null;
			}
			GC.SuppressFinalize(this);
		}

		~BitmapImplementation() 
		{
			Dispose();
		}

		public bool IsValid() 
		{
			return image != null && (image.Size.Height > 0 && image.Size.Width > 0);
		}
	}
}
#endif