using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if WIN
using OpenTK.Graphics.OpenGL;
using SDI = System.Drawing.Imaging;
using SD = System.Drawing;
using System.IO;
#elif iOS
using OpenTK.Graphics.ES11;
using MonoTouch.UIKit;
#endif

namespace Lime
{
	public partial class Texture2D : ITexture
	{
#if WIN
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
			var nsData = MonoTouch.Foundation.NSData.FromStream(stream);
			UIImage image = UIImage.LoadFromData(nsData);
			if (image == null) {
				throw new Lime.Exception("Error loading texture from stream");
			}
		}
#endif
	}
}
