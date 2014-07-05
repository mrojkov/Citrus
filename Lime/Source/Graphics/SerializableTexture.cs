using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class SerializableTexture : ITexture
	{
		SerializableTextureCore core;

		public SerializableTexture()
		{
			core = TexturePool.Instance.GetSerializableTextureCore("");
		}

		public SerializableTexture(string path)
		{
			core = TexturePool.Instance.GetSerializableTextureCore(path);
		}

		public SerializableTexture(string format, params object[] args)
		{
			var path = string.Format(format, args);
			core = TexturePool.Instance.GetSerializableTextureCore(path);
		}

		public string SerializationPath {
			get {
				return GetSerializationPath();
			}
			set {
				SetSerializationPath(value);
			}
		}

		private string GetSerializationPath()
		{
			var path = core.Path;
			if (!string.IsNullOrEmpty(path) && path[0] == '#') {
				return path;
			} else {
				return Serialization.ShrinkPath(path);
			}
		}

		private void SetSerializationPath(string value)
		{
			string path = value;
			if (!string.IsNullOrEmpty(value) && value[0] != '#') {
				path = Serialization.ExpandPath(value);
			}
			core = TexturePool.Instance.GetSerializableTextureCore(path);
		}

		public bool IsStubTexture
		{
			get {
				core.GetMainTexture();
				return core.IsStubTexture;
			}
		}

		public Size ImageSize {
			get {
				core.GetMainTexture();
				return core.ImageSize; 
			}
		}

		public Size SurfaceSize {
			get {
				core.GetMainTexture();
				return core.SurfaceSize; 
			}
		}

		public Rectangle AtlasUVRect {
			get {
				core.GetMainTexture();
				return core.UVRect; 
			}
		}

		public void TransformUVCoordinatesToAtlasSpace(ref Vector2 uv0, ref Vector2 uv1)
		{
			core.TransformUVCoordinates(ref uv0, ref uv1);
		}

		public uint GetHandle()
		{
			return core.GetHandle();
		}

#if UNITY
		public UnityEngine.Texture GetUnityTexture()
		{
			return core.GetInstance().GetUnityTexture();
		}
#endif

		public void SetAsRenderTarget()
		{
			core.GetMainTexture().SetAsRenderTarget();
		}

		public void RestoreRenderTarget()
		{
			core.GetMainTexture().RestoreRenderTarget();
		}

		public bool IsTransparentPixel(int x, int y)
		{
			var texture = core.GetMainTexture();
			var size = (Size)(AtlasUVRect.Size * (Vector2)texture.SurfaceSize);
			if (x < 0 || y < 0 || x >= size.Width || y >= size.Height) {
				return false;
			}
			var offset = (IntVector2)(AtlasUVRect.A * (Vector2)texture.SurfaceSize);
			return texture.IsTransparentPixel(x + offset.X, y + offset.Y);
		}

		public override string ToString()
		{
			return core.Path;
		}

		public void Dispose() {}
	}

	class SerializableTextureCore
	{
		public readonly string Path;
		public Rectangle UVRect;
		public Size ImageSize;
		public Size SurfaceSize;

		private int usedAtRenderCycle = 0;
		private uint cachedHandle;
		private ITexture mainTexture;

		public bool IsStubTexture { get; private set; }

		public SerializableTextureCore(string path)
		{
			Path = path;
		}

		~SerializableTextureCore()
		{
			Discard();
		}
		
		private static Texture2D CreateStubTexture()
		{
			var texture = new Texture2D();
			var pixels = new Color4[128 * 128];
			for (int i = 0; i < 128; i++)
				for (int j = 0; j < 128; j++)
					pixels[i * 128 + j] = (((i + (j & ~7)) & 8) == 0) ? Color4.Blue : Color4.White;
			texture.LoadImage(pixels, 128, 128, false);
			return texture;
		}

		/// <summary>
		/// Discards texture, free graphics resources.
		/// </summary>
		public void Discard()
		{
			if (mainTexture == null) {
				return;
			}
			mainTexture.Dispose();
			mainTexture = null;
			cachedHandle = 0;
		}

		public uint GetHandle()
		{
			usedAtRenderCycle = Renderer.RenderCycle;
			if (cachedHandle == 0) {
				cachedHandle = GetMainTexture().GetHandle();
			}
			return cachedHandle;
		}

		/// <summary>
		/// Discards texture if it has not been used 
		/// for given number of game cycles.
		/// </summary>
		public void DiscardIfNotUsed(int numCycles)
		{
			if ((Renderer.RenderCycle - usedAtRenderCycle) >= numCycles)
				Discard();
		}

		public void TransformUVCoordinates(ref Vector2 uv0, ref Vector2 uv1)
		{
			float width = UVRect.B.X - UVRect.A.X;
			float height = UVRect.B.Y - UVRect.A.Y;
			uv0.X = UVRect.Left + width * uv0.X;
			uv0.Y = UVRect.Top + height * uv0.Y;
			uv1.X = UVRect.Left + width * uv1.X;
			uv1.Y = UVRect.Top + height * uv1.Y;
		}
		
		private bool TryCreateRenderTarget(string path)
		{
			if (Path.Length <= 0 || Path[0] != '#') {
				return false;
			}
			switch(Path) {
				case "#a":
				case "#b":
					mainTexture = new RenderTexture(256, 256);
					break;
				case "#c":
					mainTexture = new RenderTexture(512, 512);
					break;
				case "#d":
					mainTexture = new RenderTexture(1024, 1024);
					break;
				default:
					mainTexture = CreateStubTexture();
					IsStubTexture = true;
					break;
			}
			UVRect.A = Vector2.Zero;
			UVRect.B = Vector2.One;
			ImageSize = mainTexture.ImageSize;
			SurfaceSize = mainTexture.SurfaceSize;
			return true;
		}
		
		private bool TryLoadTextureAtlasPart(string path)
		{
			if (!AssetsBundle.Instance.FileExists(path)) {
				return false;
			}
			var texParams = TextureAtlasPart.ReadFromBundle(path);
			mainTexture = new SerializableTexture(texParams.AtlasTexture);
			UVRect.A = (Vector2)texParams.AtlasRect.A / (Vector2)mainTexture.SurfaceSize;
			UVRect.B = (Vector2)texParams.AtlasRect.B / (Vector2)mainTexture.SurfaceSize;
			ImageSize = (Size)texParams.AtlasRect.Size;
			SurfaceSize = mainTexture.SurfaceSize;
			return true;
		}
		
		private bool TryLoadImage(string path)
		{
			if (!AssetsBundle.Instance.FileExists(path)) {
				return false;
			}
			mainTexture = new Texture2D();
			(mainTexture as Texture2D).LoadImage(path);
			UVRect.A = Vector2.Zero;
			UVRect.B = (Vector2)mainTexture.ImageSize / (Vector2)mainTexture.SurfaceSize;
			ImageSize = mainTexture.ImageSize;
			SurfaceSize = mainTexture.SurfaceSize;
            AudioSystem.Update();
			return true;
		}

		public ITexture GetMainTexture()
		{
			usedAtRenderCycle = Renderer.RenderCycle;
			if (mainTexture != null) {
				return mainTexture;
			}
			bool loaded = !string.IsNullOrEmpty(Path) && (TryCreateRenderTarget(Path) ||
				TryLoadTextureAtlasPart(Path + ".atlasPart") ||
#if iOS
				TryLoadImage(Path + ".pvr")
#elif UNITY
				TryLoadImage(Path + ".png")
#else
				TryLoadImage(Path + ".dds") ||
				TryLoadImage(Path + ".png")
#endif
			);
			if (!loaded) {
				if (!string.IsNullOrEmpty(Path)) {
					Console.WriteLine("Missing texture '{0}'", Path);
				}
				mainTexture = CreateStubTexture();
				IsStubTexture = true;
				UVRect = mainTexture.AtlasUVRect;
				ImageSize = mainTexture.ImageSize;
				SurfaceSize = mainTexture.SurfaceSize;
			}
			return mainTexture;
		}
	}
}