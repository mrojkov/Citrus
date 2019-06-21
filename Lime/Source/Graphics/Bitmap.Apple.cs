#if iOS || MAC
using System;
using System.IO;
using System.Runtime.InteropServices;
using CoreGraphics;
using ImageIO;
using Foundation;

namespace Lime
{
	class BitmapImplementation : IBitmapImplementation
	{
		[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
		internal static extern void CFRelease(System.IntPtr obj);

		private CGImage cgImage;

		public CGImage Bitmap { get { return cgImage; } private set { cgImage = value; } }

		public BitmapImplementation(Stream stream)
		{
			byte[] data;
			using (var ms = new MemoryStream()) {
				stream.CopyTo(ms);
				data = ms.ToArray();
			}
			cgImage = CGImage.FromPNG(new CGDataProvider(data), null, false, CGColorRenderingIntent.Default);
            if (cgImage == null) {
                cgImage = CGImage.FromJPEG(new CGDataProvider(data), null, false, CGColorRenderingIntent.Default);
            }
			if (cgImage == null) {
				throw new FailedToCreateCGImageFromGivenStreamException();
			} 
			var alphaInfo = cgImage.AlphaInfo;
			HasAlpha =
				alphaInfo != CGImageAlphaInfo.None &&
				alphaInfo != CGImageAlphaInfo.NoneSkipFirst &&
				alphaInfo != CGImageAlphaInfo.NoneSkipLast;
		}

		public class FailedToCreateCGImageFromGivenStreamException : Lime.Exception
		{

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
			cgImage = new CGImage(
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

			HasAlpha = alphaInfo == CGBitmapFlags.Last;
		}

		private BitmapImplementation(CGImage cgImage)
		{
			this.cgImage = cgImage;
			var alphaInfo = cgImage.AlphaInfo;
			HasAlpha =
				alphaInfo != CGImageAlphaInfo.None &&
				alphaInfo != CGImageAlphaInfo.NoneSkipFirst &&
				alphaInfo != CGImageAlphaInfo.NoneSkipLast;
		}

		public int Width
		{
			get { return cgImage != null ? (int)cgImage.Width : 0; }
		}

		public int Height
		{
			get { return cgImage != null ? (int)cgImage.Height : 0; }
		}

		public bool IsValid
		{
			get { return (cgImage != null && (Width > 0 && Height > 0)); }
		}

		public bool HasAlpha
		{
			get; private set;
		}

		public IBitmapImplementation Clone()
		{
#if iOS
			return Crop(new IntRectangle(0, 0, Width, Height));
#elif MAC
			return new BitmapImplementation(GetPixels(), Width, Height);
#endif
		}

		public IBitmapImplementation Rescale(int newWidth, int newHeight)
		{
			CGBitmapContext bctx = new CGBitmapContext(null, newWidth, newHeight,
				cgImage.BitsPerComponent, 0, CGColorSpace.CreateDeviceRGB(), CGImageAlphaInfo.PremultipliedLast);
			bctx.InterpolationQuality = CGInterpolationQuality.High;
			bctx.DrawImage(new CGRect(0, 0, newWidth, newHeight), cgImage);
			var img = bctx.ToImage();
			bctx.Dispose();
			return new BitmapImplementation(img);
		}

		public IBitmapImplementation Crop(IntRectangle cropArea)
		{
			var rect = new CGRect(cropArea.Left, cropArea.Top, cropArea.Width, cropArea.Height);
			return new BitmapImplementation(cgImage.WithImageInRect(rect));
		}

		public Color4[] GetPixels()
		{
			var isColorSpaceRGB = cgImage.ColorSpace.Model == CGColorSpaceModel.RGB;
			var isMonochrome = cgImage.ColorSpace.Model == CGColorSpaceModel.Monochrome;
			var bpp = cgImage.BitsPerPixel;
			if (!((isColorSpaceRGB && (bpp == 32 || bpp == 64)) || (isMonochrome && (bpp == 8 || bpp == 16)))) {
				throw new FormatException("Can not return array of pixels: bitmap should be either 32/64 bpp RGBA or 8/16 bpp monochrome");
			}

			var doSwap = cgImage.BitmapInfo.HasFlag(CGBitmapFlags.ByteOrder32Little);
			var isPremultiplied = cgImage.AlphaInfo == CGImageAlphaInfo.PremultipliedFirst ||
				cgImage.AlphaInfo == CGImageAlphaInfo.PremultipliedLast;
			var rowLength = cgImage.BytesPerRow / (bpp / 8);
			var width = Width;
			var height = Height;
			var pixels = new Color4[width * height];
			if (isColorSpaceRGB) {
				if (bpp == 32) {
					using (var data = cgImage.DataProvider.CopyData()) {
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
								if (rowLength > width) {
									pBytes += 4 * (rowLength - width);
								}
							}
						}
					}
				} else if (bpp == 64) {
					using (var data = cgImage.DataProvider.CopyData()) {
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
									if (isPremultiplied && a != 65535 && a != 0) {
										r = (ushort)(65535 * r / a);
										g = (ushort)(65535 * g / a);
										b = (ushort)(65535 * b / a);
									}
									pixels[index++] = doSwap ? new Color4((byte)(b / 255), (byte)(g / 255), (byte)(r / 255), (byte)(a / 255))
										: new Color4((byte)(r / 255), (byte)(g / 255), (byte)(b / 255), (byte)(a / 255));
								}
								// Sometimes Width can be smaller then length of a row due to byte alignment.
								// It's just an empty bytes at the end of each row, so we can skip them here.
								if (rowLength > width) {
									pBytes += 8 * (rowLength - width);
								}
							}
						}
					}
				}
			} else if (isMonochrome) {
				if (bpp == 8) {
					using (var data = cgImage.DataProvider.CopyData()) {
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
								if (rowLength > width) {
									pBytes += rowLength - width;
								}
							}
						}
					}
				} else if (bpp == 16) {
					using (var data = cgImage.DataProvider.CopyData()) {
						unsafe {
							byte* pBytes = (byte*)data.Bytes;
							byte v, a;
							int index = 0;
							for (int i = 0; i < height; i++) {
								for (int j = 0; j < width; j++) {
									v = (*pBytes++);
									a = (*pBytes++);
									pixels[index++] = new Color4(v, v, v, a);
								}
								// Sometimes Width can be smaller then length of a row due to byte alignment.
								// It's just an empty bytes at the end of each row, so we can skip them here.
								if (rowLength > width) {
									pBytes += 2 * (rowLength - width);
								}
							}
						}
					}
				}
			}
			return pixels;
		}

		public void SaveTo(Stream stream, CompressionFormat compression)
		{
			var data = new NSMutableData();
			CGImageDestination dest = null;

			switch (compression) {
				case CompressionFormat.Jpeg:
					dest = CGImageDestination.Create(data, MobileCoreServices.UTType.JPEG, imageCount: 1);
					break;
				case CompressionFormat.Png:
					dest = CGImageDestination.Create(data, MobileCoreServices.UTType.PNG, imageCount: 1);
					break;
			}
			if (dest != null) {
				dest.AddImage(cgImage);
				dest.Close(); 
				using (var bitmapStream = data.AsStream()) {
					bitmapStream.CopyTo(stream);
				}
				data.Dispose();
			}
		}

		private bool disposed;

		public void Dispose()
		{
			if (!disposed) {
				if (cgImage != null) {
					cgImage.Dispose();
				}
				disposed = true;
			}
		}	
	}
}
#endif
