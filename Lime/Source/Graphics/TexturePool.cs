using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;

namespace Lime
{
	public sealed class TexturePool
	{
		private ConcurrentDictionary<string, WeakReference> textures;

		public readonly static TexturePool Instance = new TexturePool();

		private TexturePool()
		{
			textures = new ConcurrentDictionary<string, WeakReference>();
		}

		public IEnumerable<ITexture> PreloadAll(string pathPrefix)
		{
			var textures = new List<ITexture>();
			foreach (var i in AssetsBundle.Instance.EnumerateFiles()) {
				if (i.StartsWith(pathPrefix)) {
					var ext = Path.GetExtension(i);
					if (ext == ".atlasPart" || ext == ".pvr" || ext == ".dds" || ext == ".png") {
						var texturePath = Path.ChangeExtension(i, null);
						var texture = Preload(texturePath);
						textures.Add(texture);
					}
				}
			}
			return textures;
		}

		public ITexture Preload(string path)
		{
			var texture = new SerializableTexture(path);
			texture.GetHandle();
			return texture;
		}

		/// <summary>
		/// Discards textures which have not been used 
		/// for given number of render cycles.
		/// </summary>
		public void DiscardUnusedTextures(int numCycles)
		{
			foreach (WeakReference r in textures.Values) {
				var target = r.Target as SerializableTextureCore;
				if (target != null) {
					target.DiscardIfNotUsed(numCycles);
				}
			}
#if !UNITY
			Texture2D.DeleteScheduledTextures();
#endif
		}

		public void DiscardAllTextures()
		{
			foreach (WeakReference r in textures.Values) {
				var target = r.Target as SerializableTextureCore;
				if (target != null) {
					target.Discard();
				}
			}
#if !UNITY
			Texture2D.DeleteScheduledTextures();
#endif
		}

		internal SerializableTextureCore GetSerializableTextureCore(string path)
		{
			SerializableTextureCore core;
			WeakReference r;
			if (!textures.TryGetValue(path, out r)) {
				core = new SerializableTextureCore(path);
				textures[path] = new WeakReference(core);
				return core;
			}
			core = r.Target as SerializableTextureCore;
			if (core == null) {
				core = new SerializableTextureCore(path);
				textures[path] = new WeakReference(core);
				return core;
			}
			return core;
		}
	}
}
