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
			protected set {
				unityTexture.width = value.Width;
				unityTexture.height = value.Height;
			}
		}

		public Size SurfaceSize
		{
			get { return ImageSize; }
			set { ImageSize = value; }
		}

		public virtual UnityEngine.Texture GetUnityTexture()
		{
			return unityTexture;
		}

		public string SerializationPath
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public Rectangle AtlasUVRect
		{
			get { return new Rectangle(0, 0, 1, 1); }
		}

		public void TransformUVCoordinatesToAtlasSpace(ref Vector2 uv) { }

		public Texture2D()
		{
			unityTexture = new UnityEngine.Texture2D(4, 4);
			SetTextureDefaultParameters();
		}

		public virtual bool IsStubTexture { get { return false; } }

		public void LoadImage(string path)
		{
			Dispose();
			unityTexture = UnityAssetBundle.Instance.LoadUnityAsset<UnityEngine.Texture2D>(path);
			SetTextureDefaultParameters();
		}

		public void LoadImage(Bitmap bitmap)
		{
			using (var stream = new MemoryStream()) {
				bitmap.SaveTo(stream);
				stream.Position = 0;
				LoadImage(stream);
			}
		}

		void SetTextureDefaultParameters()
		{
			unityTexture.wrapMode = UnityEngine.TextureWrapMode.Clamp;
			unityTexture.filterMode = UnityEngine.FilterMode.Bilinear;
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
		/// Load subtexture from pixel array
		/// Warning: this method doesn't support automatic texture reload after restoring graphics context
		/// </summary>
		public void LoadSubImage(Color4[] pixels, int x, int y, int width, int height)
		{
			// This mindblowing code:
			// 1) Rearranges pixels in bottom to top layout;
			// 2) Clips pixels outside the texture rectangle.
			y = unityTexture.height - y - height;
			var x0 = Math.Max(0, x);
			var y0 = Math.Max(0, y);
			var x1 = Math.Min(unityTexture.width, x + width);
			var y1 = Math.Min(unityTexture.height, y + height);
			if (x1 <= x0 || y1 <= y0) {
				return;
			}
			var stride = width;
			width = x1 - x0;
			height = y1 - y0;
			var c = new UnityEngine.Color32[width * height];
			int k = 0;
			for (var i = y1 - 1; i >= y0; i--) {
				var p = (i - y) * stride + (x0 - x);
				for (var j = x0; j < x1; j++) {
					c[k].a = pixels[p].A;
					c[k].r = pixels[p].R;
					c[k].g = pixels[p].G;
					c[k].b = pixels[p].B;
					k++;
					p++;
				}
			}
			unityTexture.SetPixels32(x0, y0, width, height, c);
			unityTexture.Apply();
		}

		/// <summary>
		/// Create texture from pixel array
		/// </summary>
		public void LoadImage(Color4[] pixels, int width, int height, bool generateMips = false)
		{
			Dispose();
			unityTexture = new UnityEngine.Texture2D(width, height);
			SetTextureDefaultParameters();
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

		public void Discard() {}
		public void MaybeDiscardUnderPressure() {}

		public int MemoryUsed { get { return 0; } }

		public virtual void Dispose()
		{
			//UnityAssetBundle.Instance.UnloadUnityAsset(unityTexture);
		}

		/// <summary>
		/// Returns native texture handle
		/// </summary>
		/// <returns></returns>
		public virtual uint GetHandle()
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