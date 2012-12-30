using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lime
{
	public class VideoTexture : PlainTexture
	{
		Stream rgbStream;
		Stream alphaStream;
		OgvDecoder rgbDecoder;
		OgvDecoder alphaDecoder;
		Color4[] pixels;

		public bool Looped { get; set; }
		public bool Paused { get; set; }
		public bool Stopped { get; private set; }

		public VideoTexture(string path)
		{
			rgbStream = AssetsBundle.Instance.OpenFile(path);
			rgbDecoder = new OgvDecoder(rgbStream);
			this.ImageSize = rgbDecoder.FrameSize;
			this.SurfaceSize = ImageSize;
			pixels = new Color4[ImageSize.Width * ImageSize.Height];
		}

		double gameTime;
		double videoTime;

		public void Update(float delta)
		{
			if (Paused || Stopped) {
				return;
			}
			gameTime += delta;
			if (videoTime > gameTime)
				return;
			while (true) {
				if (!rgbDecoder.DecodeFrame()) {
					if (Looped) {
						ResetDecoders();
						videoTime = 0;
						gameTime = 0;
						rgbDecoder.DecodeFrame();
					} else {
						Stopped = true;
						return;
					}
				}
				if (alphaDecoder != null) {
					alphaDecoder.DecodeFrame();
				}
				videoTime = rgbDecoder.GetPlaybackTime();
				if (videoTime >= gameTime)
					break;
			}
			rgbDecoder.FillTextureRGB8(pixels, ImageSize.Width, ImageSize.Height);
			LoadImage(pixels, ImageSize.Width, ImageSize.Height, false);
		}

		private void ResetDecoders()
		{
			rgbStream.Seek(0, SeekOrigin.Begin);
			rgbDecoder.Dispose();
			rgbDecoder = new OgvDecoder(rgbStream);
			if (alphaStream != null) {
				alphaStream.Seek(0, SeekOrigin.Begin);
				alphaDecoder.Dispose();
				alphaDecoder = new OgvDecoder(alphaStream);
			}
		}
	}
}
