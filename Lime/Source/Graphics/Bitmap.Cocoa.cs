#if iOS || MAC || MONOMAC
using System;
using System.IO;
#if iOS
using Foundation;
using CoreGraphics;
using CocoaBitmap = UIKit.UIImage;
#elif MAC
using AppKit;
using CoreGraphics;
using Foundation;
using CocoaBitmap = AppKit.NSImage;
#else
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
using CocoaBitmap = MonoMac.AppKit.NSImage;
#endif

namespace Lime
{
	class BitmapImplementation : IBitmapImplementation
	{
		public BitmapImplementation(Stream stream)
		{
#if iOS
			using (var nsData = NSData.FromStream(stream)) {
				Bitmap = CocoaBitmap.LoadFromData(nsData);
			}
#elif MAC || MONOMAC
			Bitmap = CocoaBitmap.FromStream(stream);
#endif
			var alphaInfo = Bitmap.CGImage.AlphaInfo;
			HasAlpha =
				alphaInfo != CGImageAlphaInfo.None &&
				alphaInfo != CGImageAlphaInfo.NoneSkipFirst &&
				alphaInfo != CGImageAlphaInfo.NoneSkipLast;
		}

		public BitmapImplementation(Color4[] pixels, int width, int height)
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
			var alphaInfo = Lime.Bitmap.AnyAlpha(pixels) ? CGBitmapFlags.Last : CGBitmapFlags.NoneSkipLast;
			var img = new CGImage(
				width,
				height,
				8,
				32,
				4 * width,
				CGColorSpace.CreateDeviceRGB(),
				alphaInfo,
				new CGDataProvider(data, 0, lengthInBytes),
				null,
				false,
				CGColorRenderingIntent.Default);
#if iOS
			Bitmap = new CocoaBitmap(img);
#elif MAC || MONOMAC
			Bitmap = new CocoaBitmap(img, new CGSize(width, height));
#endif
			HasAlpha = alphaInfo == CGBitmapFlags.Last;
		}

		private BitmapImplementation(CocoaBitmap bitmap)
		{
			Bitmap = bitmap;
			HasAlpha = Lime.Bitmap.AnyAlpha(GetPixels());
		}

		public CocoaBitmap Bitmap { get; private set; }

		public int Width
		{
			get { return Bitmap != null ? (int)Bitmap.CGImage.Width : 0; }
		}

		public int Height
		{
			get { return Bitmap != null ? (int)Bitmap.CGImage.Height : 0; }
		}

		public bool IsValid
		{
			get { return (Bitmap != null && (Width > 0 && Height > 0)); }
		}

		public bool HasAlpha
		{
			get; private set;
		}

		public IBitmapImplementation Clone()
		{
#if iOS
			return Crop(new IntRectangle(0, 0, Width, Height));
#elif MAC || MONOMAC
			return new BitmapImplementation((CocoaBitmap)Bitmap.Copy());
#endif
		}

		public IBitmapImplementation Rescale(int newWidth, int newHeight)
		{
#if iOS
			return new BitmapImplementation(Bitmap.Scale(new CGSize(newWidth, newHeight)));
#elif MAC || MONOMAC
			var newImage = new CocoaBitmap(new CGSize(newWidth, newHeight));
			newImage.LockFocus();
			var ctx = NSGraphicsContext.CurrentContext;
			ctx.ImageInterpolation = NSImageInterpolation.High;
			Bitmap.DrawInRect(
				new CGRect(0, 0, newWidth, newHeight),
				new CGRect(0, 0, Bitmap.Size.Width, Bitmap.Size.Height),
				NSCompositingOperation.Copy,
				1);
			newImage.UnlockFocus();
			return new BitmapImplementation(newImage);
#endif
		}

		public IBitmapImplementation Crop(IntRectangle cropArea)
		{
			var rect = new CGRect(cropArea.Left, cropArea.Top, cropArea.Width, cropArea.Height);
#if iOS
			return new BitmapImplementation(new CocoaBitmap(Bitmap.CGImage.WithImageInRect(rect)));
#elif MAC || MONOMAC
			var size = new CGSize(cropArea.Width, cropArea.Height);
			return new BitmapImplementation(new CocoaBitmap(Bitmap.CGImage.WithImageInRect(rect), size));
#endif
		}

		public Color4[] GetPixels()
		{
			var isColorSpaceRGB = Bitmap.CGImage.ColorSpace.Model == CGColorSpaceModel.RGB;
			var isMonochrome = Bitmap.CGImage.ColorSpace.Model == CGColorSpaceModel.Monochrome;
			var bpp = Bitmap.CGImage.BitsPerPixel;
			if (!((isColorSpaceRGB && (bpp == 32 || bpp == 64)) || (isMonochrome && bpp == 8))) {
				throw new FormatException("Can not return array of pixels: bitmap should be either 32/64 bpp RGBA or 8 bpp monochrome");
			}

			var doSwap = Bitmap.CGImage.BitmapInfo.HasFlag(CGBitmapFlags.ByteOrder32Little);
			var isPremultiplied = Bitmap.CGImage.AlphaInfo == CGImageAlphaInfo.PremultipliedFirst ||
				Bitmap.CGImage.AlphaInfo == CGImageAlphaInfo.PremultipliedLast;
			var rowLength = Bitmap.CGImage.BytesPerRow / (bpp / 8);
			var width = Width;
			var height = Height;
			var pixels = new Color4[width * height];
			if (isColorSpaceRGB) {
				if (bpp == 32) {
					using (var data = Bitmap.CGImage.DataProvider.CopyData()) {
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
										r = (byte)(255 * r / a);
										g = (byte)(255 * g / a);
										b = (byte)(255 * b / a);
									}
									pixels[index++] = doSwap ? new Color4(b, g, r, a) : new Color4(r, g, b, a);
								}

								// Sometimes Width can be smaller then length of a row due to byte alignment.
								// It's just an empty bytes at the end of each row, so we can skip them here.
								for (int k = 0; k < rowLength - width; k++) {
									pBytes += 4;
								}
							}
						}
					}
				} else if (bpp == 64) {
					using (var data = Bitmap.CGImage.DataProvider.CopyData()) {
						unsafe {
							ushort* pBytes = (ushort*)data.Bytes;
							ushort r, g, b, a;
							int index = 0;
							for (int i = 0; i < height; i++) {
								for (int j = 0; j < width; j++) {
									r = (ushort)(*pBytes++);
									g = (ushort)(*pBytes++);
									b = (ushort)(*pBytes++);
									a = (ushort)(*pBytes++);
									if (isPremultiplied && a != 65536 && a != 0) {
										r = (ushort)(65536 * r / a);
										g = (ushort)(65536 * g / a);
										b = (ushort)(65536 * b / a);
									}
									pixels[index++] = doSwap ? new Color4((byte)(b / 255), (byte)(g / 255), (byte)(r / 255), (byte)(a / 255))
										: new Color4((byte)(r / 255), (byte)(g / 255), (byte)(b / 255), (byte)(a / 255));
								}

								// Sometimes Width can be smaller then length of a row due to byte alignment.
								// It's just an empty bytes at the end of each row, so we can skip them here.
								for (int k = 0; k < rowLength - width; k++) {
									pBytes += bpp / 8;
								}
							}
						}
					}
				}

			} else if (isMonochrome) {
				using (var data = Bitmap.CGImage.DataProvider.CopyData()) {
					unsafe {
						byte* pBytes = (byte*)data.Bytes;
						byte v;
						int index = 0;
						for (int i = 0; i < height; i++) {
							for (int j = 0; j < width; j++) {
								v = (*pBytes++);
								pixels[index++] = new Color4(v, v, v, 255);
							}

							// Sometimes Width can be smaller then length of a row due to byte alignment.
							// It's just an empty bytes at the end of each row, so we can skip them here.
							for (int k = 0; k < rowLength - width; k++) {
								pBytes += bpp / 8;
							}
						}
					}
				}

			}
			return pixels;
		}

		public void SaveTo(Stream stream, CompressionFormat compression)
		{
#if iOS
			NSData data = null;
			switch (compression) {
				case CompressionFormat.Jpeg:
					data = Bitmap.AsJPEG(0.8f);
					break;
				case CompressionFormat.Png:
					data = Bitmap.AsPNG();
					break;
			}
			if (data != null) {
				using (var bitmapStream = data.AsStream()) {
					bitmapStream.CopyTo(stream);
				}
				data.Dispose();
			}
#elif MAC || MONOMAC
			using (var representation = new NSBitmapImageRep(Bitmap.CGImage)) {
				NSData data = null;
				NSDictionary parameters = null;
				switch (compression) {
					case CompressionFormat.Jpeg:
						parameters = NSDictionary.FromObjectAndKey(
							NSNumber.FromFloat(0.8f), NSBitmapImageRep.CompressionFactor);
						data = representation.RepresentationUsingTypeProperties(NSBitmapImageFileType.Jpeg, parameters);
						break;
					case CompressionFormat.Png:
						parameters = new NSDictionary();
						data = representation.RepresentationUsingTypeProperties(NSBitmapImageFileType.Png, parameters);
						break;
				}
				using (var bitmapStream = data.AsStream()) {
					bitmapStream.CopyTo(stream);
				}
				data.Dispose();
			}
#endif
		}

		public void Dispose()
		{
			if (Bitmap != null) {
				Bitmap.Dispose();
			}
		}
	}
}
#endif