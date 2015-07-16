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
		private NSImage bitmap;

		public BitmapImplementation() {}

		public int GetWidth()
		{
			return bitmap != null ? (int)bitmap.CGImage.Width : 0;
		}

		public int GetHeight()
		{
			return bitmap != null ? (int)bitmap.CGImage.Height : 0;
		}

		public void LoadFromStream(Stream stream)
		{
			Dispose();
			bitmap = NSImage.FromStream(stream);
		}

		/// <summary>
		/// TODO: NSImage cannot be saved in PNG format (TIF only)
		/// </summary>
		public void SaveToStream(Stream steam)
		{
			throw new NotImplementedException();
		}

		public IBitmapImplementation Crop(IntRectangle cropArea)
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			var rect = new CGRect(cropArea.Left, cropArea.Top, cropArea.Width, cropArea.Height);
			CGImage cgimage = bitmap.CGImage.WithImageInRect(rect);
			var size = new CGSize(cgimage.Width, cgimage.Height);
			return new BitmapImplementation() { bitmap = new NSImage(cgimage, size) };
		}

		/// <summary>
		/// TODO: implement. I didn't find easy way to rescale
		/// </summary>
		public IBitmapImplementation Rescale(int newWidth, int newHeight)
		{
			throw new NotImplementedException();
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
					context.DrawImage(new CGRect(0, 0, GetWidth(), GetHeight()), bitmap.CGImage);
				}					
			}
			handle.Free();
			// Swap R ang B channels
			byte temp;
			for (int i = 0; i < imageData.Length; i+=4)
			{
				temp = imageData[i + 0];
				imageData[i + 0] = imageData[i + 2];
				imageData[i + 2] = temp;
			}
			return imageData;
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
			return (bitmap != null && (bitmap.Size.Height > 0 && bitmap.Size.Width >0));
		}
	}
}
#endif