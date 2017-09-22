#if UNITY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lime
{
	public class MovieTexture : Texture2D
	{
		//FIXME: movie and alpha runs not simultaneously so we can see some artifacts
		//Posible solution: use the color and mask in the same video
		//http://answers.unity3d.com/questions/833537/how-to-play-alpha-video-in-unity.html

		//FIXME: unable to regulate play speed
		UnityEngine.MovieTexture movieTexture;
		UnityEngine.MovieTexture alphaMovieTexture;
		string path;

		public bool Looped { get; set; }
		public bool Paused { get; private set; }
		public bool Stopped { get; private set; }
		public string Path { get { return path; } set { SetPath(value); } }
		private bool opened = false;

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
			if (Path == null) {
				throw new ArgumentException();
			}
			Stop();
			movieTexture = AssetBundle.Current.LoadUnityAsset<UnityEngine.MovieTexture>(Path + ".ogv");
			movieTexture.loop = false;
			movieTexture.wrapMode = UnityEngine.TextureWrapMode.Clamp;
			movieTexture.filterMode = UnityEngine.FilterMode.Bilinear;
			foreach (var i in new string[] { "_alpha.ogv", "_Alpha.ogv" }) {
				if (AssetBundle.Current.FileExists(Path + i)) {
					alphaMovieTexture = AssetBundle.Current.LoadUnityAsset<UnityEngine.MovieTexture>(Path + i);
					alphaMovieTexture.loop = false;
					alphaMovieTexture.wrapMode = UnityEngine.TextureWrapMode.Clamp;
					alphaMovieTexture.filterMode = UnityEngine.FilterMode.Bilinear;
					break;
				}
			}
			if (alphaMovieTexture != null) {
				var alpha = new MovieTexture();
				alpha.movieTexture = alphaMovieTexture;
				AlphaTexture = alpha;
			}
			opened = true;
		}

		public void Play()
		{
			if (!opened) {
				Open();
			}
			if (Stopped) {
				Stopped = false;
			}
			//if (Stopped) {
			//	Open();
			//	Stopped = false;
			//}
			if (Paused) {
				Paused = false;
			}
			movieTexture.Play();
			alphaMovieTexture.Play();
		}

		public void Pause(bool value)
		{
			Paused = value;
			if (Paused) {
				movieTexture.Pause();
				if (alphaMovieTexture != null) {
					alphaMovieTexture.Pause();
				}
			}
		}

		public override void Dispose()
		{
			Stop();
			//UnityEngine.Object.Destroy(movieTexture);
			//if (alphaMovieTexture != null) {
				//UnityEngine.Object.Destroy(alphaMovieTexture);
			//}
			base.Dispose();
		}

		public void Stop()
		{
			Stopped = true;
			if (movieTexture != null) {
				movieTexture.Stop();
			}
			if (alphaMovieTexture != null) {
				alphaMovieTexture.Stop();
			}
		}

		public override UnityEngine.Texture GetUnityTexture ()
		{
			return movieTexture;
		}

		public void Update(float delta)
		{
			if (Paused || Stopped) {
				return;
			}
			if (!movieTexture.isPlaying) {
				if (Looped) {
					Restart();
				} else {
					Stop();
				}
			}
		}

		public void Restart()
		{
			Stop();
			Play();
		}
	}
}
#endif