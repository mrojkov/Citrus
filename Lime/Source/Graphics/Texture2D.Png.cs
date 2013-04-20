using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

#if WIN
using OpenTK.Graphics.OpenGL;
#elif MAC
using MonoMac.OpenGL;
using OGL = MonoMac.OpenGL.GL;
#endif


#if WIN || MAC
using SDI = System.Drawing.Imaging;
using SD = System.Drawing;
#elif iOS
using OpenTK.Graphics.ES11;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
#endif

namespace Lime
{
	public partial class Texture2D : ITexture
	{
#if WIN || MAC
		private void InitWithPngOrJpgBitmap(Stream stream)
		{
			var bitmap = new SD.Bitmap(stream);
			SurfaceSize = ImageSize = new Size(bitmap.Width, bitmap.Height);
			var lockRect = new SD.Rectangle(0, 0, bitmap.Width, bitmap.Height);
			var lockMode = SDI.ImageLockMode.ReadOnly;
			if (bitmap.PixelFormat == SDI.PixelFormat.Format24bppRgb) {
				var data = bitmap.LockBits(lockRect, lockMode, SDI.PixelFormat.Format24bppRgb);
				OGL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, data.Width, data.Height, 0,
					PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
				bitmap.UnlockBits(data);
			} else {
				var data = bitmap.LockBits(lockRect, lockMode, SDI.PixelFormat.Format32bppPArgb);
				OGL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
					PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
				bitmap.UnlockBits(data);
			}
			Renderer.CheckErrors();
		}
#elif iOS
		private void InitWithPngOrJpgBitmap(Stream stream)
		{
			using (var nsData = MonoTouch.Foundation.NSData.FromStream(stream))
			using (UIImage image = UIImage.LoadFromData(nsData)) {
				if (image == null) {
					throw new Lime.Exception("Error loading texture from stream");
				}
				CGImage imageRef = image.CGImage;
				int width = (int)image.Size.Width;
				int height = (int)image.Size.Height;
				SurfaceSize = ImageSize = new Size(width, height);

				int bitsPerComponent = 8;
				int bytesPerPixel = 4;
				int bytesPerRow = bytesPerPixel * width;
				byte[] data = new byte[height * bytesPerRow];

				CGImageAlphaInfo alphaInfo = imageRef.AlphaInfo;
				if (alphaInfo == CGImageAlphaInfo.None) {
					alphaInfo = CGImageAlphaInfo.NoneSkipLast;
				}
				using (var colorSpace = CGColorSpace.CreateDeviceRGB())
				using (var context = new MonoTouch.CoreGraphics.CGBitmapContext(data, width, height, bitsPerComponent, bytesPerRow, colorSpace, alphaInfo)) {
					context.DrawImage(new System.Drawing.RectangleF(0, 0, width, height), imageRef);
					GL.TexImage2D(All.Texture2D, 0, (int)All.Rgba, width, height, 0, All.Rgba, All.UnsignedByte, data);
				}
			}
		}
#endif
	}
}
