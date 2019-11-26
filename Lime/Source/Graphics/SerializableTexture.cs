using Yuzu;
using Lime.Graphics.Platform;

namespace Lime
{
	public class SerializableTexture : CommonTexture, ITexture
	{
		private string path;
		private ITexture texture;

		public SerializableTexture(string path)
		{
			SetSerializationPath(path);
		}

		public SerializableTexture()
			: this("")
		{
		}

		public SerializableTexture(string format, params object[] args)
			: this(string.Format(format, args))
		{
		}

		[YuzuMember]
		public string SerializationPath {
			get => GetSerializationPath();
			set => SetSerializationPath(value);
		}

		private string GetSerializationPath()
		{
			if (!string.IsNullOrEmpty(path) && path[0] == '#') {
				return path;
			} else {
				return InternalPersistence.Current?.ShrinkPath(path) ?? path;
			}
		}

		private void SetSerializationPath(string value)
		{
			path = value;
			if (!string.IsNullOrEmpty(value) && value[0] != '#') {
				path = InternalPersistence.Current?.ExpandPath(value) ?? path;
			}
			texture = null;
		}

		public TextureParams TextureParams
		{
			get
			{
				EnsureTexture();
				return texture.TextureParams;
			}
			set
			{
				EnsureTexture();
				texture.TextureParams = value;
			}
		}

		public bool IsStubTexture
		{
			get
			{
				EnsureTexture();
				return texture.IsStubTexture;
			}
		}

		public Size ImageSize {
			get
			{
				EnsureTexture();
				return texture.ImageSize;
			}
		}

		public Size SurfaceSize {
			get
			{
				EnsureTexture();
				return texture.SurfaceSize;
			}
		}

		public ITexture AtlasTexture {
			get {
				EnsureTexture();
				return texture.AtlasTexture;
			}
		}

		public Rectangle AtlasUVRect {
			get {
				EnsureTexture();
				return texture.AtlasUVRect;
			}
		}

		public void TransformUVCoordinatesToAtlasSpace(ref Vector2 uv)
		{
			EnsureTexture();
			texture.TransformUVCoordinatesToAtlasSpace(ref uv);
		}

		public IPlatformTexture2D GetPlatformTexture()
		{
			EnsureTexture();
			return texture.GetPlatformTexture();
		}

		public void SetAsRenderTarget()
		{
			EnsureTexture();
			texture.SetAsRenderTarget();
		}

		public void RestoreRenderTarget()
		{
			EnsureTexture();
			texture.RestoreRenderTarget();
		}

		public bool IsTransparentPixel(int x, int y)
		{
			EnsureTexture();
			return texture.IsTransparentPixel(x, y);
		}

		public override string ToString()
		{
			return path;
		}

		public Color4[] GetPixels()
		{
			EnsureTexture();
			return texture.GetPixels();
		}

		private void EnsureTexture()
		{
			if (texture == null || texture.IsDisposed) {
				texture = TexturePool.Instance.GetTexture(path);
			}
		}
	}
}
