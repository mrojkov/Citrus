#if MAC
using System;
using System.IO;
using MonoMac.CoreGraphics;
using MonoMac.AppKit;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Lime
{
	class BitmapImplementation : IBitmapImplementation
	{
		private NSImage bitmap;

		/// <summary>
		/// Constructor. Use LoadFromStream to load image data
		/// </summary>
		public BitmapImplementation() { }

		/// <summary>
		/// Returns image width or 0 (if image isn't loaded)
		/// </summary>
		public int GetWidth()
		{
			return bitmap != null ? bitmap.CGImage.Width : 0;
		}

		/// <summary>
		/// Returns image height or 0 (if image isn't loaded)
		/// </summary>
		public int GetHeight()
		{
			return bitmap != null ? bitmap.CGImage.Height : 0;
		}

		/// <summary>
		/// Loads from stream
		/// </summary>
		public void LoadFromStream(Stream stream)
		{
			bitmap = NSImage.FromStream(stream);
		}

		/// <summary>
		/// Throws NotImplementedException
		/// </summary>
		public void SaveToStream(Stream steam)
		{
			// TODO BitmapImplementation SaveToStream
			// NSImage cannot be saved in PNG format (TIF only)

			/*
			if (bitmap == null) {
				throw new InvalidOperationException("Image is not loaded. Use LoadFromStream first");
			}
			*/
			throw new NotImplementedException();
		}

		/// <summary>
		/// Crops the image
		/// </summary>
		public IBitmapImplementation Crop(IntRectangle cropArea)
		{
			if (bitmap == null) {
				throw new InvalidOperationException("Image is not loaded. Use LoadFromStream first");
			}

			var rect = new RectangleF(cropArea.Left, cropArea.Top, cropArea.Width, cropArea.Height);

			CGImage cgimage = bitmap.CGImage.WithImageInRect(rect);
			SizeF size = new SizeF(cgimage.Width, cgimage.Height);

			var result = new BitmapImplementation();
			result.bitmap = new NSImage(cgimage, size);

			return result;
		}

		/// <summary>
		/// Not implemented. Returns itself
		/// </summary>
		public IBitmapImplementation Rescale(int newWidth, int newHeight)
		{
			// TODO BitmapImplementation Rescale
			// I didn't find easy way to rescale

			if (bitmap == null) {
				throw new InvalidOperationException("Image is not loaded. Use LoadFromStream first");
			}

			return this;
		}

		/// <summary>
		/// Returns image pixel data
		/// </summary>
		public byte[] GetImageData()
		{		
			if (bitmap == null) {
				throw new InvalidOperationException("Image is not loaded. Use LoadFromStream first");
			}

			int bytesPerPixel = 4;
			byte[] imageData = new byte[GetWidth() * GetHeight() * bytesPerPixel];
			var handle = GCHandle.Alloc(imageData);

			using (var colorSpace = CGColorSpace.CreateDeviceRGB())
			{
				using (var context = new CGBitmapContext(Marshal.UnsafeAddrOfPinnedArrayElement(imageData, 0), GetWidth(), GetHeight(), 8, bytesPerPixel * GetWidth(), colorSpace, CGBitmapFlags.ByteOrder32Big | CGBitmapFlags.PremultipliedLast))
				{
					context.DrawImage(new RectangleF(0, 0, GetWidth(), GetHeight()), bitmap.CGImage);
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

		/// <summary>
		/// Releases all resource used
		/// </summary>
		public void Dispose()
		{
			if (bitmap != null)
			{
				bitmap.Dispose();
				bitmap = null;
			}
		}
	}
}

#endif