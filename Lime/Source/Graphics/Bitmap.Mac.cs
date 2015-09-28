#if MAC || MONOMAC
using System;
using System.IO;
using AppKit;
using CoreGraphics;
using System.Runtime.InteropServices;

namespace Lime
{
	class BitmapImplementation : IBitmapImplementation
	{
		private NSImage image;

		public BitmapImplementation() {}

		public int GetWidth()
		{
			return image != null ? (int)image.Size.Width : 0;
		}

		public int GetHeight()
		{
			return image != null ? (int)image.Size.Height : 0;
		}

		public void LoadFromStream(Stream stream)
		{
			if (image != null) {
				image.Dispose();
				image = null;
			}
			image = NSImage.FromStream(stream);
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
			CGImage cgimage = image.CGImage.WithImageInRect(rect);
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

		public byte[] GetImageData()
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			int bytesPerPixel = 4;
			byte[] imageData = new byte[GetWidth() * GetHeight() * bytesPerPixel];
			var handle = GCHandle.Alloc(imageData);
			using (var colorSpace = CGColorSpace.CreateDeviceRGB()) {
				using (
					var context = new CGBitmapContext(Marshal.UnsafeAddrOfPinnedArrayElement(imageData, 0), GetWidth(), GetHeight(), 8,
						bytesPerPixel * GetWidth(), colorSpace, CGBitmapFlags.ByteOrder32Big | CGBitmapFlags.PremultipliedLast)) 
				{
					context.DrawImage(new CGRect(0, 0, GetWidth(), GetHeight()), image.CGImage);
				}					
			}
			handle.Free();
			// Swap R ang B channels
			byte temp;
			for (int i = 0; i < imageData.Length; i += 4)
			{
				temp = imageData[i + 0];
				imageData[i + 0] = imageData[i + 2];
				imageData[i + 2] = temp;
			}
			return imageData;
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