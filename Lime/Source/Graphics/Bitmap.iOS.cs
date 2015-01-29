#if iOS
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using ProtoBuf;
using UIKit;
using CoreGraphics;
using System.Drawing;

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
			using (var nsData = Foundation.NSData.FromStream(stream)) {
				bitmap = UIImage.LoadFromData(nsData);
			}
		}

		public void SaveToStream(Stream stream)
		{
			if (bitmap != null && bitmap.AsPNG() != null) {
				using (var bitmapStream = bitmap.AsPNG().AsStream()) { 
					Toolbox.CopyStream(bitmapStream, stream);
				}
			}
		}

		public IBitmapImplementation Rescale(int newWidth, int newHeight)
		{
			var scaledImage = bitmap.Scale(new SizeF(newWidth, newHeight));
			var newImplementation = new BitmapImplementation();
			newImplementation.bitmap = scaledImage;
			return newImplementation;
		}

		public IBitmapImplementation Crop(IntRectangle cropArea)
		{
			var rect = new RectangleF(cropArea.Left, cropArea.Top, cropArea.Width, cropArea.Height);
			var cgimage = bitmap.CGImage;
			cgimage = cgimage.WithImageInRect(rect);
			var cropped = new BitmapImplementation();
			cropped.bitmap = new UIImage(cgimage);
			return cropped;
		}

		public void Dispose()
		{
			if (bitmap != null) {
				bitmap.Dispose();
			}
		}
	}
}
#endif