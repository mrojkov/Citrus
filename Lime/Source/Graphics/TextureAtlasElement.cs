using System;
using Yuzu;

namespace Lime
{
	public class TextureAtlasElement : CommonTexture, ITexture
	{
		public struct Params
		{
			[YuzuMember]
			public string AtlasPath;

			[YuzuMember]
			public IntRectangle AtlasRect;

			public static Params ReadFromBundle(string path)
			{
				return Serialization.ReadObject<Params>(path);
			}

			public static Params ReadFromFile(string path)
			{
				return Serialization.ReadObjectFromFile<Params>(path);
			}

			public static void WriteToFile(Params texParams, string path)
			{
				Serialization.WriteObjectToFile<Params>(path, texParams, Serialization.Format.Binary);
			}
		}

		private ITexture atlasTexture;
		public Rectangle AtlasUVRect { get; private set; }
		public Size ImageSize { get; private set; }

		public TextureAtlasElement(Params @params)
		{
			atlasTexture = TexturePool.Instance.GetTexture(@params.AtlasPath);
			AtlasUVRect = new Rectangle(
				(Vector2)@params.AtlasRect.A / (Vector2)atlasTexture.SurfaceSize,
				(Vector2)@params.AtlasRect.B / (Vector2)atlasTexture.SurfaceSize
			);
			ImageSize = (Size)@params.AtlasRect.Size;
		}

		public Size SurfaceSize
		{
			get { return atlasTexture.SurfaceSize; }
		}

		public void TransformUVCoordinatesToAtlasSpace(ref Vector2 uv)
		{
			float width = AtlasUVRect.B.X - AtlasUVRect.A.X;
			float height = AtlasUVRect.B.Y - AtlasUVRect.A.Y;
			uv.X = AtlasUVRect.Left + width * uv.X;
			uv.Y = AtlasUVRect.Top + height * uv.Y;
		}

		public uint GetHandle()
		{
			return atlasTexture.GetHandle();
		}

#if UNITY
		public UnityEngine.Texture GetUnityTexture()
		{
			return atlasTexture.GetUnityTexture();
		}
#endif

		public void SetAsRenderTarget()
		{
			throw new NotSupportedException();
		}

		public void RestoreRenderTarget()
		{
			throw new NotSupportedException();
		}

		public bool IsTransparentPixel(int x, int y)
		{
			var size = (Size)(AtlasUVRect.Size * (Vector2)atlasTexture.SurfaceSize);
			if (x < 0 || y < 0 || x >= size.Width || y >= size.Height) {
				return false;
			}
			var offset = (IntVector2)(AtlasUVRect.A * (Vector2)atlasTexture.SurfaceSize);
			return atlasTexture.IsTransparentPixel(x + offset.X, y + offset.Y);
		}

		public void Discard() { }

		public Color4[] GetPixels()
		{
			throw new NotSupportedException();
		}

		public bool IsStubTexture
		{
			get { return atlasTexture.IsStubTexture; }
		}

		public string SerializationPath
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public TextureWrapMode WrapModeU
		{
			get { return atlasTexture.WrapModeU; }
			set { atlasTexture.WrapModeU = value; }
		}

		public TextureWrapMode WrapModeV
		{
			get { return atlasTexture.WrapModeV; }
			set { atlasTexture.WrapModeV = value; }
		}

		public TextureFilter MinFilter
		{
			get { return atlasTexture.MinFilter; }
			set { atlasTexture.MinFilter = value; }
		}

		public TextureFilter MagFilter
		{
			get { return atlasTexture.MagFilter; }
			set { atlasTexture.MagFilter = value; }
		}
	}
}
