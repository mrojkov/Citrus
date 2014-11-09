using System;
using ProtoBuf;

namespace Lime
{
	public class TextureAtlasElement : CommonTexture, ITexture
	{
		[ProtoContract]
		public struct Params
		{
			[ProtoMember(1)]
			public string AtlasPath;

			[ProtoMember(2)]
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
				Serialization.WriteObjectToFile<Params>(path, texParams);
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

		public ITexture AlphaTexture
		{
			get { return atlasTexture.AlphaTexture; }
		}

		public void TransformUVCoordinatesToAtlasSpace(ref Vector2 uv0, ref Vector2 uv1)
		{
			float width = AtlasUVRect.B.X - AtlasUVRect.A.X;
			float height = AtlasUVRect.B.Y - AtlasUVRect.A.Y;
			uv0.X = AtlasUVRect.Left + width * uv0.X;
			uv0.Y = AtlasUVRect.Top + height * uv0.Y;
			uv1.X = AtlasUVRect.Left + width * uv1.X;
			uv1.Y = AtlasUVRect.Top + height * uv1.Y;
		}

		public uint GetHandle()
		{
			return atlasTexture.GetHandle();
		}

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

		public void Unload() { }

		public bool IsStubTexture
		{
			get { return atlasTexture.IsStubTexture; }
		}

		public string SerializationPath
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
	}
}
