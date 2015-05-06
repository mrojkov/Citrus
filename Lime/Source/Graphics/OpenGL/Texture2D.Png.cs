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
using OpenTK.Graphics.ES20;
using UIKit;
using CoreGraphics;
#elif ANDROID
using OpenTK.Graphics.ES20;
using SD = System.Drawing;
#endif

namespace Lime
{
	public partial class Texture2D : CommonTexture, ITexture
	{
#if WIN || MAC
		private void InitWithPngOrJpgBitmap(Stream stream, Stream alphaStream)
		{
			if (!Application.IsMainThread) {
				throw new NotSupportedException("Calling from non-main thread currently is not supported");
			}
			using (var bitmap = new SD.Bitmap(alphaStream ?? stream)) {
				SurfaceSize = ImageSize = new Size(bitmap.Width, bitmap.Height);
				var lockRect = new SD.Rectangle(0, 0, bitmap.Width, bitmap.Height);
				if (alphaStream != null) {
					if (bitmap.PixelFormat != SDI.PixelFormat.Format32bppArgb) {
						throw new Lime.Exception("Unexpected alpha bitmap format: " + bitmap.PixelFormat.ToString());
					}
					using (var colorBitmap = new SD.Bitmap(stream)) {
						if (colorBitmap.Width != bitmap.Width || colorBitmap.Height != bitmap.Height) {
							if (colorBitmap.Width == 1 && colorBitmap.Height == 1) {
								// For 1x1 bitmaps, propagate alpha value to red, green and blue channels.
								var imageData = bitmap.LockBits(lockRect, SDI.ImageLockMode.ReadWrite, SDI.PixelFormat.Format32bppArgb);
								CopyRedToGreenBlueAlphaChannels(imageData);
								bitmap.UnlockBits(imageData);
							} else {
								throw new Lime.Exception("Alpha bitmap size and main bitmap size must be equal");
							}
						} else {
							var alphaData = bitmap.LockBits(lockRect, SDI.ImageLockMode.ReadWrite, SDI.PixelFormat.Format32bppArgb);
							var colorData = colorBitmap.LockBits(lockRect, SDI.ImageLockMode.ReadOnly, SDI.PixelFormat.Format24bppRgb);
							MixColorAndAlphaAndSwapRB(colorData, alphaData);
							colorBitmap.UnlockBits(colorData);
							bitmap.UnlockBits(alphaData);
						}
					}
				}
				if (bitmap.PixelFormat == SDI.PixelFormat.Format24bppRgb) {
					PrepareOpenGLTexture();
					PlatformRenderer.PushTexture(handle, 0);
					var data = bitmap.LockBits(lockRect, SDI.ImageLockMode.ReadOnly, SDI.PixelFormat.Format24bppRgb);
					SwapRedAndGreen24(data);
					GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, data.Width, data.Height, 0,
						PixelFormat.Rgb, PixelType.UnsignedByte, data.Scan0);
					bitmap.UnlockBits(data);
					MemoryUsed = data.Width * data.Height * 3;
					PlatformRenderer.PopTexture(0);
				} else {
					PrepareOpenGLTexture();
					PlatformRenderer.PushTexture(handle, 0);
					var data = bitmap.LockBits(lockRect, SDI.ImageLockMode.ReadOnly, alphaStream != null ? SDI.PixelFormat.Format32bppArgb : SDI.PixelFormat.Format32bppPArgb);
					if (alphaStream == null) {
						SwapRedAndGreen32(data);
					}
					GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
						PixelFormat.Rgba, PixelType.UnsignedByte, data.Scan0);
					bitmap.UnlockBits(data);
					MemoryUsed = data.Width * data.Height * 4;
					PlatformRenderer.PopTexture(0);
				}
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

		private void MixColorAndAlphaAndSwapRB(SDI.BitmapData colorData24, SDI.BitmapData alphaData32)
		{
			// alphaData's R goes to alphaData's A
			// colorData's RGB goes to alphaData's BGR
			// alphaData is the result.
			unsafe {
				for (int j = 0; j < alphaData32.Height; j++) {
					byte* pc = (byte*)colorData24.Scan0 + colorData24.Stride * j;
					byte* pa = (byte*)alphaData32.Scan0 + alphaData32.Stride * j;
					for (int i = 0; i < alphaData32.Width; i++) {
						byte r = *pc++;
						byte g = *pc++;
						byte b = *pc++;
						byte a = *pa;
						*pa++ = b;
						*pa++ = g;
						*pa++ = r;
						*pa++ = a;
					}
				}
			}
		}

		private void CopyRedToGreenBlueAlphaChannels(SDI.BitmapData data)
		{
			unsafe {
				for (int j = 0; j < data.Height; j++) {
					byte* p = (byte*)data.Scan0 + data.Stride * j;
					for (int i = 0; i < data.Width; i++) {
						byte intensity = *p++;
						*p++ = intensity;
						*p++ = intensity;
						*p++ = intensity;
					}
				}
			}
		}

#elif iOS
		private void InitWithPngOrJpgBitmap(Stream stream, Stream alphaStream)
		{
			if (!Application.IsMainThread) {
				throw new NotSupportedException("Calling from non-main thread currently is not supported");
			}
			if (alphaStream != null) {
				throw new NotSupportedException("Separate alpha-stream is not supported on this platform");
			}
			using (var nsData = Foundation.NSData.FromStream(stream))
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
            using (var context = new CoreGraphics.CGBitmapContext(data, width, height, bitsPerComponent, bytesPerRow, colorSpace, alphaInfo)) {
                context.DrawImage(new System.Drawing.RectangleF(0, 0, width, height), imageRef);
                PrepareOpenGLTexture();
				PlatformRenderer.PushTexture(handle, 0);
                GL.TexImage2D(All.Texture2D, 0, (int)All.Rgba, width, height, 0, All.Rgba, All.UnsignedByte, data);
				PlatformRenderer.PopTexture(0);
            }
        }
#elif ANDROID
		private void InitWithPngOrJpgBitmap(Stream stream, Stream alphaStream)
		{
			if (!Application.IsMainThread) {
				throw new NotSupportedException("Calling from non-main thread currently is not supported");
			}
			using (var bitmap = Android.Graphics.BitmapFactory.DecodeStream(alphaStream ?? stream)) {
				SurfaceSize = ImageSize = new Size(bitmap.Width, bitmap.Height);
				PrepareOpenGLTexture();
				var pixels = new int[bitmap.Width * bitmap.Height];
				bitmap.GetPixels(pixels, 0, bitmap.Width, 0, 0, bitmap.Width, bitmap.Height);
				if (alphaStream != null) {
					using (var colorBitmap = Android.Graphics.BitmapFactory.DecodeStream(stream)) {
						if (colorBitmap.Width != bitmap.Width || colorBitmap.Height != bitmap.Height) {
							if (colorBitmap.Width == 1 && colorBitmap.Height == 1) {
								// For 1x1 bitmaps, propagate alpha value to red, green and blue channels.
								CopyRedToGreenBlueAlphaChannels(ref pixels);
							} else {
								throw new Lime.Exception ("Alpha bitmap size and main bitmap size must be the same");
							}
						} else {
							var colorPixels = new int[colorBitmap.Width * colorBitmap.Height];
							colorBitmap.GetPixels(colorPixels, 0, bitmap.Width, 0, 0, bitmap.Width, bitmap.Height);
							MixColorAndAlphaAndSwapRB(ref colorPixels, ref pixels);
						}
					}
				} else {
					SwapRedAndBlue(ref pixels);
					if (bitmap.HasAlpha) {
						PremultiplyAlpha(ref pixels);
					}
				}
				PlatformRenderer.PushTexture(handle, 0);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0,
					PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
				PlatformRenderer.PopTexture(0);
				MemoryUsed = bitmap.Width * bitmap.Height * 4;
			}
			PlatformRenderer.CheckErrors();
		}

		private void MixColorAndAlphaAndSwapRB(ref int[] pixelsColor, ref int[] pixelsAlpha)
		{
			// pixelsAlpha's R goes to pixelsAlpha's A
			// pixelsColor's RGB goes to pixelsAlpha's BGR
			// pixelsAlpha is the result.
			for (int i = 0; i < pixelsColor.Length; i++) {
				var colorRGB = new Color4((uint)pixelsColor[i]);
				var colorA = new Color4((uint)pixelsAlpha[i]);
				byte r = colorRGB.R;
				byte g = colorRGB.G;
				byte b = colorRGB.B;
				colorA.A = colorA.R;
				colorA.B = r;
				colorA.G = g;
				colorA.R = b;
				pixelsAlpha[i] = (int)colorA.ABGR;
			}
		}

		private void CopyRedToGreenBlueAlphaChannels(ref int[] pixels)
		{
			for (int i = 0; i < pixels.Length; i++) {
				var color = new Color4((uint)pixels[i]);
				color.A = color.R;
				color.G = color.R;
				color.B = color.R;
				pixels[i] = (int)color.ABGR;
			}
		}

		private void PremultiplyAlpha(ref int[] pixels)
		{
			for (int i = 0; i < pixels.Length; i++) {
				var color = new Color4((uint)pixels[i]);
				int a = color.A;
				if (a == 0) {
					pixels[i] = 0;
				} else if (a < 255) {
					a = (a << 8) + a;
					color.R = (byte)((color.R * a + 255) >> 16);
					color.G = (byte)((color.G * a + 255) >> 16);
					color.B = (byte)((color.B * a + 255) >> 16); 
					pixels[i] = (int)color.ABGR;
				}
			}
		}

		private void SwapRedAndBlue(ref int[] pixels)
		{
			for (int i = 0; i < pixels.Length; i++) {
				var l = (long)pixels[i];
				l = 
					(0x000000FF) & (l >> 16)
					| (0x00FF0000) & (l << 16)
					| (0xFF00FF00) & (l);
				pixels[i] = (int)l;
			}
		}

#endif
	}
}
#endif