#if OPENGL
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

#if WIN
using OpenTK.Graphics;
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
	public partial class Texture2D : CommonTexture, ITexture
	{
#if WIN || MAC
		private void InitWithPngOrJpgBitmap(Stream stream)
		{
			if (!Application.IsMainThread) {
				throw new NotSupportedException("Calling from non-main thread currently is not supported");
			}
			var bitmap = new SD.Bitmap(stream);
			SurfaceSize = ImageSize = new Size(bitmap.Width, bitmap.Height);
			var lockRect = new SD.Rectangle(0, 0, bitmap.Width, bitmap.Height);
			var lockMode = SDI.ImageLockMode.ReadOnly;
			if (bitmap.PixelFormat == SDI.PixelFormat.Format24bppRgb) {
				PrepareOpenGLTexture();
				var data = bitmap.LockBits(lockRect, lockMode, SDI.PixelFormat.Format24bppRgb);
				SwapRedAndGreen24(data);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, data.Width, data.Height, 0,
					PixelFormat.Rgb, PixelType.UnsignedByte, data.Scan0);
				bitmap.UnlockBits(data);
				MemoryUsed = data.Width * data.Height * 3;
			} else {
				PrepareOpenGLTexture();
				var data = bitmap.LockBits(lockRect, lockMode, SDI.PixelFormat.Format32bppPArgb);
				SwapRedAndGreen32(data);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
					PixelFormat.Rgba, PixelType.UnsignedByte, data.Scan0);
				bitmap.UnlockBits(data);
				MemoryUsed = data.Width * data.Height * 4;
			}
			PlatformRenderer.CheckErrors();
		}

		private void SwapRedAndGreen24(SDI.BitmapData data)
		{
			unsafe {
				for (int j = 0; j < data.Height; j++) {
					byte* p = (byte*)data.Scan0 + data.Stride * j;
					for (int i = 0; i < data.Width; i++) {
						byte r = *p;
						byte g = *(p + 1);
						byte b = *(p + 2);
						*p++ = b;
						*p++ = g;
						*p++ = r;
					}
				}
			}
		}

		private void SwapRedAndGreen32(SDI.BitmapData data)
		{
			unsafe {
				for (int j = 0; j < data.Height; j++) {
					byte* p = (byte*)data.Scan0 + data.Stride * j;
					for (int i = 0; i < data.Width; i++) {
						byte r = *p;
						byte g = *(p + 1);
						byte b = *(p + 2);
						byte a = *(p + 3);
						*p++ = b;
						*p++ = g;
						*p++ = r;
						*p++ = a;
					}
				}
			}
		}

#elif iOS
		private void InitWithPngOrJpgBitmap(Stream stream)
		{
			if (!Application.IsMainThread) {
				throw new NotSupportedException("Calling from non-main thread currently is not supported");
			}
			using (var nsData = MonoTouch.Foundation.NSData.FromStream(stream))
			using (UIImage image = UIImage.LoadFromData(nsData)) {
				if (image == null) {
					throw new Lime.Exception("Error loading texture from stream");
				}
                InitWithUIImage(image);
			}
		}

        public void InitWithUIImage(UIImage image)
        {
            CGImage imageRef = image.CGImage;
            int width = (int)image.Size.Width;
            int height = (int)image.Size.Height;
            SurfaceSize = ImageSize = new Size(width, height);

            int bitsPerComponent = 8;
            int bytesPerPixel = 4;
            int bytesPerRow = bytesPerPixel * width;
            byte[] data = new byte[height * bytesPerRow];
			MemoryUsed = data.Length;

            CGImageAlphaInfo alphaInfo = imageRef.AlphaInfo;
            if (alphaInfo == CGImageAlphaInfo.None) {
                alphaInfo = CGImageAlphaInfo.NoneSkipLast;
            }
            using (var colorSpace = CGColorSpace.CreateDeviceRGB())
            using (var context = new MonoTouch.CoreGraphics.CGBitmapContext(data, width, height, bitsPerComponent, bytesPerRow, colorSpace, alphaInfo)) {
                context.DrawImage(new System.Drawing.RectangleF(0, 0, width, height), imageRef);
                PrepareOpenGLTexture();
                GL.TexImage2D(All.Texture2D, 0, (int)All.Rgba, width, height, 0, All.Rgba, All.UnsignedByte, data);
            }
        }
#endif
	}
}
#endif