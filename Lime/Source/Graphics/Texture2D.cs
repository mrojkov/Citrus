using System;
using System.IO;
using Lime.Graphics.Platform;

namespace Lime
{
	/// <summary>
	/// Represents 2D texture
	/// </summary>
	[YuzuDontGenerateDeserializer]
	public partial class Texture2D : CommonTexture, ITexture
	{
		public delegate void TextureMissingDelegate(string path);

		public static bool IsStubTextureTransparent;

		private static readonly string[] AffordableTextureFileExtensions = {
#if iOS || ANDROID
			".pvr", ".jpg"
#else
			".dds", ".pvr", ".jpg", ".png"
#endif
		};

		private IPlatformTexture2D platformTexture;
		public OpacityMask OpacityMask { get; private set; }
		public Size ImageSize { get; protected set; }
		public Size SurfaceSize { get; protected set; }
		private Rectangle uvRect;
		private int usedOnRenderCycle;

		public ITexture AtlasTexture => this;

		public Rectangle AtlasUVRect { get { return uvRect; } }

		public void TransformUVCoordinatesToAtlasSpace(ref Vector2 uv) { }

		public bool IsStubTexture { get; private set; }

		private TextureParams textureParams = TextureParams.Default;
		public TextureParams TextureParams {
			get
			{
				return textureParams;
			}
			set
			{
				if (textureParams != value) {
					textureParams = value;
					if (platformTexture != null) {
						platformTexture.SetTextureParams(textureParams);
					}
				}
			}
		}

		private void LoadTextureParams(string path)
		{
			var textureParamsPath = path + ".texture";
			if (AssetBundle.Current.FileExists(textureParamsPath)) {
				using (var stream = AssetBundle.Current.OpenFile(textureParamsPath)) {
					TextureParams = InternalPersistence.Instance.ReadObject<TextureParams>(textureParamsPath, stream);
				}
			} else {
				TextureParams = TextureParams.Default;
			}
		}

		public void LoadImage(string path, TextureMissingDelegate onTextureMissing = null)
		{
			IsStubTexture = false;

			foreach (string textureFileExtension in AffordableTextureFileExtensions) {
				string tryPath = path + textureFileExtension;
				if (!AssetBundle.Current.FileExists(tryPath)) {
					continue;
				}

				Stream stream;
				try {
					stream = AssetBundle.Current.OpenFileLocalized(tryPath);
				} catch (System.Exception e) {
					Console.WriteLine("Can not open file '{0}':\n{1}", path, e);
					continue;
				}

				using (stream) {
					LoadImageHelper(stream);
				}

				LoadTextureParams(path);

				var maskPath = path + ".mask";
				if (AssetBundle.Current.FileExists(maskPath)) {
					OpacityMask = new OpacityMask(maskPath);
				}

				if (Application.IsMain(Application.CurrentThread)) {
					AudioSystem.Update();
				}

				return;
			}

			Console.WriteLine("Missing texture '{0}'", path);
			onTextureMissing?.Invoke(path);
			LoadStubImage(IsStubTextureTransparent && !path.IsNullOrWhiteSpace());
		}

		internal void LoadStubImage(bool transparent)
		{
			const int side = 128;

			TextureParams = TextureParams.Default;
			OpacityMask = transparent? new OpacityMask(side, side) : null;

			var pixels = new Color4[side * side];
			for (int i = 0; i < side; i++) {
				for (int j = 0; j < side; j++) {
					pixels[i * side + j] =
						transparent
							? Color4.Transparent
							: ((i + (j & ~7)) & 8) == 0
								? Color4.Blue
								: Color4.White;
				}
			}
			LoadImage(pixels, side, side);

			IsStubTexture = true;
		}

		public void LoadImage(byte[] data)
		{
			IsStubTexture = false;

			using (var stream = new MemoryStream(data)) {
				LoadImage(stream);
			}
		}

		public void LoadImage(Stream stream)
		{
			IsStubTexture = false;

			LoadImageHelper(stream);
		}

		public void LoadImage(Bitmap bitmap)
		{
			IsStubTexture = false;

			LoadImage(bitmap.GetPixels(), bitmap.Width, bitmap.Height);
		}

		private void LoadImageHelper(Stream stream)
		{
			if (!stream.CanSeek) {
				var memoryStream = new MemoryStream();
				stream.CopyTo(memoryStream);
				stream = memoryStream;
			}
			using (var reader = new BinaryReader(stream)) {
				stream.Seek(0, SeekOrigin.Begin);
				int sign = reader.ReadInt32();
				stream.Seek(0, SeekOrigin.Begin);
				if (sign == PVRMagic) {
					InitWithPVRTexture(reader);
				} else if (sign == KTXMagic) {
					InitWithKTXTexture(reader);
				}
#if WIN || MAC
				else if (sign == DDSMagic) {
					InitWithDDSBitmap(reader);
				}
#endif
				else {
					InitWithPngOrJpg(stream);
				}
			}
			uvRect = new Rectangle(Vector2.Zero, (Vector2)ImageSize / (Vector2)SurfaceSize);
		}

		private void InitWithPngOrJpg(Stream stream)
		{
			using (var bitmap = new Bitmap(stream)) {
				LoadImage(bitmap);
			}
		}

		private void EnsurePlatformTexture(Format format, int width, int height, bool mipmaps)
		{
			if (platformTexture == null ||
				platformTexture.Format != format ||
				platformTexture.Width != width ||
				platformTexture.Height != height ||
				platformTexture.LevelCount > 1 != mipmaps
			) {
				if (platformTexture != null) {
					platformTexture.Dispose();
				}
				platformTexture = PlatformRenderer.Context.CreateTexture2D(format, width, height, mipmaps, textureParams);
				PlatformRenderer.RebindTexture(this);
			}
		}

		/// <summary>
		/// Create texture from pixel array
		/// </summary>
		public void LoadImage(Color4[] pixels, int width, int height)
		{
			IsStubTexture = false;

			MemoryUsed = 4 * width * height;
			ImageSize = new Size(width, height);
			SurfaceSize = ImageSize;
			uvRect = new Rectangle(0, 0, 1, 1);

			Window.Current.InvokeOnRendering(() => {
				EnsurePlatformTexture(Format.R8G8B8A8_UNorm, width, height, false);
				platformTexture.SetData(0, pixels);
			});
		}

		public void LoadImage(IntPtr pixels, int width, int height)
		{
			LoadImage(pixels, width, height, Format.R8G8B8A8_UNorm);
		}

		internal void LoadImage(IntPtr pixels, int width, int height, Format format)
		{
			MemoryUsed = 4 * width * height;
			ImageSize = new Size(width, height);
			SurfaceSize = ImageSize;
			uvRect = new Rectangle(0, 0, 1, 1);

			Window.Current.InvokeOnRendering(() => {
				EnsurePlatformTexture(format, width, height, false);
				platformTexture.SetData(0, pixels);
			});
		}

		/// <summary>
		/// Load subtexture from pixel array
		/// Warning: this method doesn't support automatic texture reload after restoring graphics context
		/// </summary>
		public void LoadSubImage(Color4[] pixels, int x, int y, int width, int height)
		{
			IsStubTexture = false;

			Window.Current.InvokeOnRendering(() => {
				platformTexture.SetData(0, x, y, width, height, pixels);
			});
		}

		~Texture2D()
		{
			Dispose();
		}

		public override void Dispose()
		{
			MemoryUsed = 0;
			if (platformTexture != null) {
				var platformTextureCopy = platformTexture;
				Window.Current.InvokeOnRendering(() => {
					platformTextureCopy.Dispose();
				});
				platformTexture = null;
			}
			base.Dispose();
		}

		public virtual IPlatformTexture2D GetPlatformTexture()
		{
			if (IsDisposed) {
				throw new ObjectDisposedException(GetType().Name);
			}
			usedOnRenderCycle = Renderer.RenderCycle;
			return platformTexture;
		}

		public override void MaybeDiscardUnderPressure()
		{
			if (Renderer.RenderCycle - usedOnRenderCycle > 3) {
				Dispose();
			}
		}

		/// <summary>
		/// Sets texture as a render target
		/// </summary>
		public void SetAsRenderTarget()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Restores default render target(backbuffer).
		/// </summary>
		public void RestoreRenderTarget()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Checks pixel transparency at given coordinates
		/// </summary>
		/// <param name="x">x-coordinate of pixel</param>
		/// <param name="y">y-coordinate of pixel</param>
		/// <returns></returns>
		public bool IsTransparentPixel(int x, int y)
		{
			if (OpacityMask != null) {
				int x1 = x * OpacityMask.Width / ImageSize.Width;
				int y1 = y * OpacityMask.Height / ImageSize.Height;
				if (TextureParams.WrapModeU == TextureWrapMode.Repeat) {
					x1 = Math.Abs(x1 % OpacityMask.Width);
				} else if (TextureParams.WrapModeU == TextureWrapMode.MirroredRepeat) {
					x1 = (x1 / OpacityMask.Width) % 2 == 0
						? Math.Abs(x1 % OpacityMask.Width)
						: OpacityMask.Width - Math.Abs(x1 % OpacityMask.Width);
				}
				if (TextureParams.WrapModeV == TextureWrapMode.Repeat) {
					y1 = Math.Abs(y1 % OpacityMask.Height);
				} else if (TextureParams.WrapModeV == TextureWrapMode.MirroredRepeat) {
					y1 = (y1 / OpacityMask.Height) % 2 == 0
						? Math.Abs(y1 % OpacityMask.Height)
						: OpacityMask.Height - Math.Abs(y1 % OpacityMask.Height);
				}
				return !OpacityMask.TestPixel(x1, y1);
			}
			return false;
		}

		private byte[] ReadTextureData(BinaryReader reader, int length)
		{
			MemoryUsed += length;
			return reader.ReadBytes(length);
		}

		public virtual Color4[] GetPixels()
		{
			throw new NotSupportedException();
		}
	}
}
