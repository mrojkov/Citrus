using System;
using Yuzu;
using Lime.Graphics.Platform;

namespace Lime
{
	public class TextureAtlasElement : CommonTexture, ITexture
	{
		public struct Params
		{
			private string atlasPath;

			[YuzuMember]
			public string AtlasPath
			{
				get
				{
					return atlasPath;
				}
				set
				{
					atlasPath = string.Intern(value);
				}
			}

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

		public ITexture AtlasTexture { get; private set; }
		public Rectangle AtlasUVRect { get; private set; }
		public Size ImageSize { get; private set; }

		public TextureAtlasElement(Params @params)
		{
			AtlasTexture = TexturePool.Instance.GetTexture(@params.AtlasPath);
			AtlasUVRect = new Rectangle(
				(Vector2)@params.AtlasRect.A / (Vector2)AtlasTexture.SurfaceSize,
				(Vector2)@params.AtlasRect.B / (Vector2)AtlasTexture.SurfaceSize
			);
			ImageSize = (Size)@params.AtlasRect.Size;
		}

		public Size SurfaceSize
		{
			get { return AtlasTexture.SurfaceSize; }
		}

		public void TransformUVCoordinatesToAtlasSpace(ref Vector2 uv)
		{
			if (uv.X < 0) uv.X = 0;
			if (uv.X > 1) uv.X = 1;
			if (uv.Y < 0) uv.Y = 0;
			if (uv.Y > 1) uv.Y = 1;
			float width = AtlasUVRect.BX - AtlasUVRect.AX;
			float height = AtlasUVRect.BY - AtlasUVRect.AY;
			uv.X = AtlasUVRect.Left + width * uv.X;
			uv.Y = AtlasUVRect.Top + height * uv.Y;
		}

		public IPlatformTexture2D GetPlatformTexture()
		{
			return AtlasTexture.GetPlatformTexture();
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
			var size = (Size)(AtlasUVRect.Size * (Vector2)AtlasTexture.SurfaceSize);
			if (x < 0 || y < 0 || x >= size.Width || y >= size.Height) {
				return false;
			}
			var offset = (IntVector2)(AtlasUVRect.A * (Vector2)AtlasTexture.SurfaceSize);
			return AtlasTexture.IsTransparentPixel(x + offset.X, y + offset.Y);
		}

		public Color4[] GetPixels()
		{
			throw new NotSupportedException();
		}

		public bool IsStubTexture
		{
			get { return AtlasTexture.IsStubTexture; }
		}

		public TextureParams TextureParams
		{
			get { return AtlasTexture.TextureParams;}
			set { AtlasTexture.TextureParams = value; }
		}
	}
}
