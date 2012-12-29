using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class VideoTexture : PlainTexture
	{
		OgvDecoder decoder;
		Color4[] pixels;

		public VideoTexture(string path)
		{
			var stream = AssetsBundle.Instance.OpenFile(path);
			decoder = new OgvDecoder(stream);
			this.ImageSize = decoder.FrameSize;
			this.SurfaceSize = ImageSize;
			pixels = new Color4[ImageSize.Width * ImageSize.Height];
		}

		public void Update(float delta)
		{
			decoder.DecodeFrame();
			Console.WriteLine(decoder.GetPlaybackTime());
			decoder.FillTextureRGB8(pixels, ImageSize.Width, ImageSize.Height);
			//decoder.FillTextureRGB8(pixels, ImageSize.Width, ImageSize.Height);
			//decoder.FillTextureRGB8(pixels, ImageSize.Width, ImageSize.Height);
			LoadImage(pixels, ImageSize.Width, ImageSize.Height, false);
		}
	}
}
