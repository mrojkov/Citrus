namespace Lime
{
	public class Icon
	{
		private Bitmap bitmap;
		public Bitmap AsBitmap =>
			bitmap ?? (bitmap = new Bitmap(texture.GetPixels(), texture.ImageSize.Width, texture.ImageSize.Height));

		private ITexture texture;
		public ITexture AsTexture => texture ?? (texture = CreateTexture(bitmap));

		private object nativeIcon;
		public object AsNativeIcon
		{
			get {
#if WIN
				return nativeIcon ?? (nativeIcon = AsBitmap.NativeBitmap);
#else
				throw new System.NotImplementedException();
#endif
			}
		}

		public Icon(Bitmap bitmap)
		{
			this.bitmap = bitmap;
		}

		public Icon(ITexture texture)
		{
			this.texture = texture;
		}

		private static ITexture CreateTexture(Bitmap bitmap)
		{
			var texture = new Texture2D();
			texture.LoadImage(bitmap);
			return texture;
		}
	}
}
