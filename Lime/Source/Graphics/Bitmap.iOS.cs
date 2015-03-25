#if iOS
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using ProtoBuf;
using UIKit;
using CoreGraphics;
using System.Drawing;
using Foundation;

namespace Lime
{
	class BitmapImplementation : IBitmapImplementation
	{
		private UIImage bitmap;

		public int GetWidth()
		{
			return bitmap == null ? 0 : ((float)bitmap.Size.Width).Round();
		}

		public int GetHeight()
		{
			return bitmap == null ? 0 : ((float)bitmap.Size.Height).Round();
		}

		public void LoadFromStream(Stream stream)
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			Dispose();
			using (var nsData = Foundation.NSData.FromStream(stream)) {
				bitmap = UIImage.LoadFromData(nsData);
			}
		}

		public void SaveToStream(Stream stream)
		{
			if (!IsValid()) {
				throw new InvalidOperationException();
			}
			if (bitmap != null && bitmap.AsPNG() != null) {
				using (var bitmapStream = bitmap.AsPNG().AsStream()) {
					bitmapStream.CopyTo(stream);
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