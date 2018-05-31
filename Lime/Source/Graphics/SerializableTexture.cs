using System;

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

		public string SerializationPath {
			get { return GetSerializationPath(); }
			set { SetSerializationPath(value); }
		}

		private string GetSerializationPath()
		{
			if (!string.IsNullOrEmpty(path) && path[0] == '#') {
				return path;
			} else {
				return Serialization.ShrinkPath(path);
			}
		}

		private void SetSerializationPath(string value)
		{
			path = value;
			if (!string.IsNullOrEmpty(value) && value[0] != '#') {
				path = Serialization.ExpandPath(value);
			}
			texture = null;
		}

		public TextureParams TextureParams
		{
			get
			{
				if (texture == null) {
					texture = LoadTexture();
				}
				return texture.TextureParams;
			}
			set
			{
				if (texture == null) {
					texture = LoadTexture();
				}
				texture.TextureParams = value;
			}
		}

		public bool IsStubTexture
		{
			get
			{
				if (texture == null) {
					texture = LoadTexture();
				}
				return texture.IsStubTexture;
			}
		}

		public Size ImageSize {
			get
			{
				if (texture == null) {
					texture = LoadTexture();
				}
				return texture.ImageSize;
			}
		}

		public Size SurfaceSize {
			get
			{
				if (texture == null) {
					texture = LoadTexture();
				}
				return texture.SurfaceSize;
			}
		}
		
		public ITexture AtlasTexture {
			get {
				if (texture == null) {
					texture = LoadTexture();
				}
				return texture.AtlasTexture;
			}
		}

		public Rectangle AtlasUVRect {
			get {
				if (texture == null) {
					texture = LoadTexture();
				}
				return texture.AtlasUVRect;
			}
		}

		public void TransformUVCoordinatesToAtlasSpace(ref Vector2 uv)
		{
			if (texture == null) {
				texture = LoadTexture();
			}
			texture.TransformUVCoordinatesToAtlasSpace(ref uv);
		}

		public uint GetHandle()
		{
			if (texture == null) {
				texture = LoadTexture();
			}
			return texture.GetHandle();
		}

		public void Discard() { }

		public void SetAsRenderTarget()
		{
			if (texture == null) {
				texture = LoadTexture();
			}
			texture.SetAsRenderTarget();
		}

		public void RestoreRenderTarget()
		{
			if (texture == null) {
				texture = LoadTexture();
			}
			texture.RestoreRenderTarget();
		}

		public bool IsTransparentPixel(int x, int y)
		{
			if (texture == null) {
				texture = LoadTexture();
			}
			return texture.IsTransparentPixel(x, y);
		}

		public override string ToString()
		{
			return path;
		}

		private ITexture LoadTexture()
		{
			return TexturePool.Instance.GetTexture(path);
		}

		public Color4[] GetPixels()
		{
			if (texture == null) {
				texture = LoadTexture();
			}
			return texture.GetPixels();
		}
	}
}
