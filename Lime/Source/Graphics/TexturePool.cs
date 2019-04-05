using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Concurrent;

namespace Lime
{
	public sealed class TexturePool
	{

		public static event Texture2D.TextureMissingDelegate TextureMissing;

		private readonly Dictionary<string, WeakReference> textures = new Dictionary<string, WeakReference>();

		public readonly static TexturePool Instance = new TexturePool();

		private TexturePool() {}

		[Obsolete("Use DiscardTexturesUnderPressure()")]
		public void DiscardUnusedTextures(int numCycles)
		{
			DiscardTexturesUnderPressure();
		}

		public void DiscardTexturesUnderPressure()
		{
			lock (textures) {
				foreach (WeakReference r in textures.Values) {
					var texture = r.Target as ITexture;
					if (texture != null && !texture.IsDisposed) {
						texture.MaybeDiscardUnderPressure();
					}
				}
			}
		}

		public void DiscardAllTextures()
		{
			lock (textures) {
				foreach (WeakReference r in textures.Values) {
					var texture = r.Target as ITexture;
					if (texture != null && !texture.IsDisposed) {
						texture.Dispose();
					}
				}
			}
		}

		public void DiscardAllStubTextures()
		{
			lock (textures) {
				foreach (WeakReference r in textures.Values) {
					var target = r.Target as ITexture;
					if (target != null && target.IsStubTexture && !target.IsDisposed) {
						target.Dispose();

						//TODO: Вместо следующей строки, нужно реализовать нормальный Discard у StubTexture
						r.Target = null;
					}
				}
			}
		}

		public ITexture GetTexture(string path)
		{
			lock (textures) {
				ITexture texture;
				WeakReference r;
				if (path == null) {
					path = string.Empty;
				}
				if (!textures.TryGetValue(path, out r)) {
					texture = CreateTexture(path);
					textures[path] = new WeakReference(texture);
					return texture;
				}
				texture = r.Target as ITexture;
				if (texture == null || texture.IsDisposed) {
					texture = CreateTexture(path);
					textures[path] = new WeakReference(texture);
					return texture;
				}
				return texture;
			}
		}

		private static ITexture CreateTexture(string path)
		{
			ITexture texture;
			
			if (string.IsNullOrEmpty(path)) {
				texture = new Texture2D();
				((Texture2D) texture).LoadStubImage(false);
				return texture;
			}
			
			texture = TryCreateRenderTarget(path) ?? TryLoadTextureAtlasPart(path + ".atlasPart");
			
			if (texture == null) {
				texture = new Texture2D();
				((Texture2D) texture).LoadImage(path, TextureMissing);
			}
			
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
			if (!AssetBundle.Current.FileExists(path)) {
				return null;
			}
			var data = TextureAtlasElement.Params.ReadFromBundle(path);
			var texture = new TextureAtlasElement(data);
			return texture;
		}
	}
}
