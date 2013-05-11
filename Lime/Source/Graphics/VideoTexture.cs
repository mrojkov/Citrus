#if !iOS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lime
{
	public class VideoTexture : Texture2D
	{
		Stream rgbStream;
		Stream alphaStream = null;
#if !UNITY
		OgvDecoder rgbDecoder;
		OgvDecoder alphaDecoder;
#endif
		Color4[] pixels;

		public bool Looped { get; set; }
		public bool Paused { get; set; }
		public bool Stopped { get; private set; }

		public VideoTexture(string path)
		{
#if UNITY
			throw new NotImplementedException();
#else
			rgbStream = PackedAssetsBundle.Instance.OpenFile(path);
			rgbDecoder = new OgvDecoder(rgbStream);
			this.ImageSize = rgbDecoder.FrameSize;
			this.SurfaceSize = ImageSize;
			pixels = new Color4[ImageSize.Width * ImageSize.Height];
#endif
		}

		double gameTime;
		double videoTime;

		public void Update(float delta)
		{
#if UNITY
			throw new NotImplementedException();
#else
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
#endif
		}

		private void ResetDecoders()
		{
#if UNITY
			throw new NotImplementedException();
#else
			rgbStream.Seek(0, SeekOrigin.Begin);
			rgbDecoder.Dispose();
			rgbDecoder = new OgvDecoder(rgbStream);
			if (alphaStream != null) {
				alphaStream.Seek(0, SeekOrigin.Begin);
				alphaDecoder.Dispose();
				alphaDecoder = new OgvDecoder(alphaStream);
			}
#endif
		}
	}
}
#endif