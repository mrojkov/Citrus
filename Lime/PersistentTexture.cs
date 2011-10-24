using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using ProtoBuf;

namespace Lime
{
    [ProtoContract]
	public class PersistentTexture : ITexture
	{
		private PersistentTextureCore core;

		public PersistentTexture ()
		{
			core = TexturePool.Instance.GetPersistentTextureCore ("");
		}

		public PersistentTexture (string path)
		{
			core = TexturePool.Instance.GetPersistentTextureCore (path);
		}
		
		[ProtoMember(1)]
		public string SerializationPath {
			get {
				var path = Serialization.ShrinkPath (Path);
				return path;
			}
			set {
				var path = Serialization.ExpandPath (value);
				core = TexturePool.Instance.GetPersistentTextureCore (path);
			}
		}

		public string Path { get { return core.Path; } }

		public Size ImageSize { get { return Instance.ImageSize; } }

		public Size SurfaceSize { get { return Instance.SurfaceSize; } }
		
		public Rectangle UVRect { get { return core.UVRect; } }
				
		public uint GetHandle ()
		{
			return Instance.GetHandle ();
		}

		public void SetAsRenderTarget ()
		{
			Instance.SetAsRenderTarget ();
		}

		public void RestoreRenderTarget ()
		{
			Instance.RestoreRenderTarget ();
		}

		public bool IsTransparentPixel (int x, int y)
		{
			return Instance.IsTransparentPixel (x, y);
		}

		private ITexture Instance { get { return core.Instance; } }
	}

	internal class PersistentTextureCore
	{
		public readonly string Path;
		int usedAtRenderCycle = 0;
		
		ITexture instance;	
		Rectangle uvRect;
		
		public Rectangle UVRect { get { return uvRect; } }
						
		public PersistentTextureCore (string path)
		{
			Path = path;			
		}

		~PersistentTextureCore ()
		{
			Discard ();
		}
		
		static PlainTexture CreateStubTexture ()
		{
			var stubTexture = new PlainTexture ();
			Color4[] pixels = new Color4[128 * 128];
			for (int i = 0; i < 128; i++)
				for (int j = 0; j < 128; j++)
					pixels [i * 128 + j] = (((i + (j & ~7)) & 8) == 0) ? Color4.Blue : Color4.White;
			stubTexture.LoadImage (pixels, 128, 128, true);
			return stubTexture;
		}

		/// <summary>
		/// Discards texture, frees graphics resources.
		/// </summary>
		public void Discard ()
		{
			if (instance != null) {
				if (instance is IDisposable)
					(instance as IDisposable).Dispose ();
				instance = null;
			}
		}

		/// <summary>
		/// Discards texture if it has not been used 
		/// for given number of game cycles.
		/// </summary>
		public void DiscardIfNotUsed (int numCycles)
		{
			if ((TexturePool.Instance.gameRenderCycle - usedAtRenderCycle) >= numCycles)
				Discard ();
		}
		
		private bool TryCreateRenderTarget (string path)
		{
			if (Path.Length > 0 && Path [0] == '#') {
				switch (Path) {
				case "#a":
				case "#b":
					instance = new RenderTexture (256, 256);
					break;
				case "#c":
					instance = new RenderTexture (512, 512);
					break;
				case "#d":
					instance = new RenderTexture (1024, 1024);
					break;
				default:
					instance = CreateStubTexture ();
					break;
				}
				uvRect.A = Vector2.Zero;
				uvRect.B = (Vector2)instance.SurfaceSize;
			}
			return false;
		}
		
		private bool TryLoadTextureAtlasPart (string path)
		{
			if (AssetsBundle.Instance.FileExists (path)) {
				var texParams = TextureAtlasPart.ReadFromBundle (path);
				instance = new PersistentTexture (texParams.AtlasTexture);
				uvRect.A = (Vector2)texParams.AtlasRect.A / (Vector2)instance.SurfaceSize;
				uvRect.B = (Vector2)texParams.AtlasRect.B / (Vector2)instance.SurfaceSize;
				return true;
			}
			return false;
		}
		
		private bool TryLoadImage (string path)
		{
			if (AssetsBundle.Instance.FileExists (path)) {
				instance = new PlainTexture ();
				(instance as PlainTexture).LoadImage (path);
				uvRect.A = Vector2.Zero;
				uvRect.B = (Vector2)instance.ImageSize / (Vector2)instance.SurfaceSize;
				return true;
			}
			Console.WriteLine ("Missing texture: {0}", path);
			return false;
		}

		public ITexture Instance {
			get {
				if (instance == null) {
					bool loaded = !string.IsNullOrEmpty (Path)  && (TryCreateRenderTarget (Path) ||
						TryLoadTextureAtlasPart (Path + ".atlasPart") ||
#if iOS
						TryLoadImage (Path + ".pvr")
#else
						TryLoadImage (Path + ".dds")
#endif
					);
					if (!loaded) {
						instance = CreateStubTexture ();
						uvRect = instance.UVRect;
					}
				}
				usedAtRenderCycle = TexturePool.Instance.gameRenderCycle;
				return instance;
			}
		}
	}

	/// Container for texture assets.
	/// </summary>
	public sealed class TexturePool
	{
		internal int gameRenderCycle = 1;
		Dictionary<string, WeakReference> items;
		static readonly TexturePool instance = new TexturePool ();

		/// <summary>
		/// Global singleton.
		/// </summary>
		public static TexturePool Instance { get { return instance; } }

		private TexturePool ()
		{
			items = new Dictionary<string, WeakReference> ();
		}

		/// <summary>
		/// Discards textures wich have not been used 
		/// for given number of game cycles.
		/// </summary>
		public void DiscardUnused (int numCycles)
		{
			foreach (WeakReference r in items.Values) {
				if (r.IsAlive)
					(r.Target as PersistentTextureCore).DiscardIfNotUsed (numCycles);
			}
		}

		/// <summary>
		/// Discards all textures.
		/// </summary>
		public void DiscardAll ()
		{
			foreach (WeakReference r in items.Values) {
				if (r.IsAlive)
					(r.Target as PersistentTextureCore).Discard ();
			}
		}

		/// <summary>
		/// Increases current game render cycle. 
		/// Usually this function must be called at the end of frame.
		/// </summary>
		public void AdvanceGameRenderCycle ()
		{
			gameRenderCycle++;
		}

		internal PersistentTextureCore GetPersistentTextureCore (string path)
		{
			WeakReference r;
			if (!items.TryGetValue (path, out r) || !r.IsAlive) {
				r = new WeakReference (new PersistentTextureCore (path));
				items [path] = r;
			}
			return r.Target as PersistentTextureCore;
		}
	}
}