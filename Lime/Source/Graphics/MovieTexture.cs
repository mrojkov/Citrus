#if !iOS
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

		public bool Looped { get; set; }
		public bool Paused { get; private set; }
		public bool Stopped { get; private set; }
		public string Path { get; private set; }

		public MovieTexture()
		{
			Stopped = true;
		}

		public MovieTexture(string path)
		{
			Open(path);
		}

		public void Open(string path)
		{
#if UNITY
			throw new NotImplementedException();
#else
			if (path == null) {
				throw new ArgumentException();
			}
			videoTime = 0;
			gameTime = 0;
			Stop();
			Path = path;
			rgbStream = AssetsBundle.Instance.OpenFile(path + ".ogv");
			if (AssetsBundle.Instance.FileExists(path + "_alpha.ogv")) {
				alphaStream = AssetsBundle.Instance.OpenFile(path + "_alpha.ogv");
			}
			rgbDecoder = new OgvDecoder(rgbStream);
			this.ImageSize = rgbDecoder.FrameSize;
			this.SurfaceSize = ImageSize;
			pixels = new Color4[ImageSize.Width * ImageSize.Height];
#endif
		}

		public void Play()
		{
			if (Stopped) {
				Open(Path);
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
#if UNITY
			throw new NotImplementedException();
#else
			Stopped = true;
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
						Restart();
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

		public void Restart()
		{
			Stop();
			Play();
		}
	}
}
#endif