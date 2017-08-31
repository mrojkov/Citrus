using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
#if !UNITY
using System.Collections.Concurrent;
#endif

namespace Lime
{
	public sealed class TexturePool
	{
		private class StubTexture : Texture2D
		{
			public override bool IsStubTexture
			{
				get { return true; }
			}

			public StubTexture()
			{
				var pixels = new Color4[128 * 128];
				for (int i = 0; i < 128; i++)
					for (int j = 0; j < 128; j++)
						pixels[i * 128 + j] = (((i + (j & ~7)) & 8) == 0) ? Color4.Blue : Color4.White;
				Application.InvokeOnMainThread(() =>
					LoadImage(pixels, 128, 128)
				);
			}
		}

		public delegate void TextureMissingDelegate(string path);
		public static event TextureMissingDelegate TextureMissing;

#if UNITY
		private readonly Dictionary<string, WeakReference> textures = new Dictionary<string, WeakReference>();
#else
		private readonly ConcurrentDictionary<string, WeakReference> textures = new ConcurrentDictionary<string, WeakReference>();
#endif
		public readonly static TexturePool Instance = new TexturePool();

		private TexturePool() {}

		[Obsolete("Use DiscardTexturesUnderPressure()")]
		public void DiscardUnusedTextures(int numCycles)
		{
			DiscardTexturesUnderPressure();
		}

		public void DiscardTexturesUnderPressure()
		{
			foreach (WeakReference r in textures.Values) {
				var texture = r.Target as ITexture;
				if (texture != null) {
					texture.MaybeDiscardUnderPressure();
				}
			}
		}

		public void DiscardAllTextures()
		{
			foreach (WeakReference r in textures.Values) {
				var texture = r.Target as ITexture;
				if (texture != null) {
					texture.Discard();
				}
			}
		}

		public void DiscardAllStubTextures()
		{
			foreach (WeakReference r in textures.Values) {
				var target = r.Target as ITexture;
				if (target != null && target.IsStubTexture) {
					target.Discard();

					//TODO: Вместо следующей строки, нужно реализовать нормальный Discard у StubTexture
					r.Target = null;
				}
			}
		}

		public ITexture GetTexture(string path)
		{
			ITexture texture;
			WeakReference r;
			if (!textures.TryGetValue(path, out r)) {
				texture = CreateTexture(path);
				textures[path] = new WeakReference(texture);
				return texture;
			}
			texture = r.Target as ITexture;
			if (texture == null) {
				texture = CreateTexture(path);
				textures[path] = new WeakReference(texture);
				return texture;
			}
			return texture;
		}

		private static ITexture CreateTexture(string path)
		{
			if (string.IsNullOrEmpty(path)) {
				return new StubTexture();
			}
			var texture = TryCreateRenderTarget(path) ??
				TryLoadTextureAtlasPart(path + ".atlasPart") ??
#if iOS || ANDROID
				TryLoadImage(path + ".pvr") ??
				TryLoadImage(path + ".jpg");
#else
				TryLoadImage(path + ".dds") ??
				TryLoadImage(path + ".pvr") ??
				TryLoadImage(path + ".jpg") ??
				TryLoadImage(path + ".png");
#endif
			if (texture == null) {
				Console.WriteLine("Missing texture '{0}'", path);
				if (TextureMissing != null) {
					TextureMissing(path);
				}
				texture = new StubTexture();
			}
			return texture;
		}

		private static ITexture TryLoadImage(string path)
		{
			if (!AssetBundle.Instance.FileExists(path)) {
				return null;
			}
			var texture = new Texture2D();
			texture.LoadTextureParams(path);
			texture.LoadImage(path);
			AudioSystem.Update();
			return texture;
		}

		private static ITexture TryCreateRenderTarget(string path)
		{
			if (path.Length <= 0 || path[0] != '#') {
				return null;
			}
			switch (path) {
				case "#a":
				case "#b":
					return new RenderTexture(256, 256);
				case "#c":
					return new RenderTexture(512, 512);
				case "#d":
				case "#e":
				case "#f":
				case "#g":
					return new RenderTexture(1024, 1024);
				default:
					return null;
			}
		}

		private static ITexture TryLoadTextureAtlasPart(string path)
		{
			if (!AssetBundle.Instance.FileExists(path)) {
				return null;
			}
			var data = TextureAtlasElement.Params.ReadFromBundle(path);
			var texture = new TextureAtlasElement(data);
			return texture;
		}
	}
}
