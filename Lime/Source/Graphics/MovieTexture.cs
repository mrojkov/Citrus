using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class MovieTexture : Texture2D
	{
		Stream rgbStream;
		Stream alphaStream;
#if !UNITY
		OgvDecoder rgbDecoder;
		OgvDecoder alphaDecoder;
#endif
		Color4[] pixels;
		double gameTime;
		double videoTime;
		string path;

		public bool Looped { get; set; }
		public bool Paused { get; private set; }
		public bool Stopped { get; private set; }
		public string Path { get { return path; } set { SetPath(value); } }

		public MovieTexture()
		{
			Stopped = true;
		}

		private void SetPath(string value)
		{
			path = value;
			Stop();
		}

		private void Open()
		{
#if UNITY || MAC
#else
			if (Path == null) {
				throw new ArgumentException();
			}
			Stop();
			rgbStream = AssetsBundle.Instance.OpenFile(Path + ".ogv");
			rgbDecoder = new OgvDecoder(rgbStream);
			if (AssetsBundle.Instance.FileExists(Path + "_alpha.ogv")) {
				alphaStream = AssetsBundle.Instance.OpenFile(Path + "_alpha.ogv");
				alphaDecoder = new OgvDecoder(alphaStream);
			}
			this.ImageSize = rgbDecoder.FrameSize;
			this.SurfaceSize = ImageSize;
			pixels = new Color4[ImageSize.Width * ImageSize.Height];
#endif
		}

		public void Play()
		{
			if (Stopped) {
				Open();
				Stopped = false;
			}
			if (Paused) {
				Paused = false;
			}
		}

		public void Pause(bool value)
		{
			Paused = value;
		}

		public override void Dispose()
		{
			Stop();
			base.Dispose();
		}

		public void Stop()
		{
#if UNITY || MAC
#else
			DisposeOpenGLTexture();
			Stopped = true;
			videoTime = 0;
			gameTime = 0;
			if (rgbDecoder != null) {
				rgbDecoder.Dispose();
				rgbDecoder = null;
			}
			if (rgbStream != null) {
				rgbStream.Dispose();
				rgbStream = null;
			}
			if (alphaDecoder != null) {
				alphaDecoder.Dispose();
				alphaDecoder = null;
			}
			if (alphaStream != null) {
				alphaStream.Dispose();
				alphaStream = null;
			}
#endif
		}

		public void Update(float delta)
		{
#if UNITY || MAC
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
						Restart();
						rgbDecoder.DecodeFrame();
					} else {
						Stop();
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
			rgbDecoder.FillTextureRGBX8(pixels, ImageSize.Width, ImageSize.Height);
			if (alphaDecoder != null) {
				alphaDecoder.FillTextureAlpha(pixels, ImageSize.Width, ImageSize.Height);
			}
			LoadImage(pixels, ImageSize.Width, ImageSize.Height, false);
#endif
		}

		public void Restart()
		{
			Stop();
			Play();
		}
	}
}