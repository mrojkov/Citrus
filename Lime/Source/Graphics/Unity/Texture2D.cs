#if UNITY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lime
{
	public class Texture2D : ITexture
	{
		UnityEngine.Texture2D unityTexture;

		public Size ImageSize
		{
			get { return new Size(unityTexture.width, unityTexture.height); }
		}

		public Size SurfaceSize
		{
			get { return ImageSize; }
		}

		public UnityEngine.Texture GetUnityTexture()
		{
			return unityTexture;
		}

		public string SerializationPath
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public Rectangle UVRect
		{
			get { return new Rectangle(0, 0, 1, 1); }
		}

		public Texture2D()
		{
			unityTexture = new UnityEngine.Texture2D(4, 4);
		}

		public bool IsStubTexture { get { return false; } }

		public void LoadImage(string path)
		{
			Dispose();
			unityTexture = UnityAssetsBundle.Instance.LoadUnityAsset<UnityEngine.Texture2D>(path);
		}

		public void LoadImage(byte[] data)
		{
			using (var stream = new MemoryStream(data)) {
				LoadImage(stream);
			}
		}

		public void LoadImage(Stream stream)
		{
			using (var mstream = new MemoryStream()) {
				stream.CopyTo(mstream);
				var data = mstream.GetBuffer();
				unityTexture.LoadImage(data);
			}
		}

		/// <summary>
		/// Create texture from pixel array
		/// </summary>
		public void LoadImage(Color4[] pixels, int width, int height, bool generateMips)
		{
			Dispose();
			unityTexture = new UnityEngine.Texture2D(width, height);
			var c = new UnityEngine.Color32[pixels.Length];
			for (int i = 0; i < pixels.Length; i++) {
				c[i].a = pixels[i].A;
				c[i].r = pixels[i].R;
				c[i].g = pixels[i].G;
				c[i].b = pixels[i].B;
			}
			unityTexture.SetPixels32(c);
			unityTexture.Apply(generateMips);
		}

		~Texture2D()
		{
			Dispose();
		}

		public void Dispose()
		{
			//UnityAssetsBundle.Instance.UnloadUnityAsset(unityTexture);
		}

		/// <summary>
		/// Returns native texture handle
		/// </summary>
		/// <returns></returns>
		public uint GetHandle()
		{
			return (uint)unityTexture.GetNativeTextureID();
		}

		/// <summary>
		/// Sets texture as a render target
		/// </summary>
		public void SetAsRenderTarget()
		{
		}

		/// <summary>
		/// Restores default render target(backbuffer).
		/// </summary>
		public void RestoreRenderTarget()
		{
		}

		/// <summary>
		/// Checks pixel transparency at given coordinates
		/// </summary>
		/// <param name="x">x-coordinate of pixel</param>
		/// <param name="y">y-coordinate of pixel</param>
		/// <returns></returns>
		public bool IsTransparentPixel(int x, int y)
		{
			return false;
		}
	}
}
#endif