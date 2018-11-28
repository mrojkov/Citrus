#if OPENGL
using System;
using System.IO;
using System.Runtime.InteropServices;
#if iOS || ANDROID || WIN
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
#endif

#pragma warning disable 0618

namespace Lime
{
	/// <summary>
	/// Represents 2D texture
	/// </summary>
	[YuzuDontGenerateDeserializer]
	public partial class Texture2D : CommonTexture, ITexture, IGLObject
	{
		#region TextureReloader
		abstract class TextureReloader
		{
			public abstract void Reload(Texture2D texture);
		}

		class TextureStubReloader : TextureReloader
		{
			private readonly bool transparent;

			public TextureStubReloader(bool transparent)
			{
				this.transparent = transparent;
			}

			public override void Reload(Texture2D texture)
			{
				texture.LoadStubImage(transparent);
			}
		}

		class TextureBundleReloader : TextureReloader
		{
			public string path;

			public TextureBundleReloader(string path)
			{
				this.path = path;
			}

			public override void Reload(Texture2D texture)
			{
				texture.LoadImage(path);
			}
		}

		class TexturePixelArrayReloader : TextureReloader
		{
			Color4[] pixels;
			int width;
			int height;

			public TexturePixelArrayReloader(Color4[] pixels, int width, int height)
			{
				this.pixels = pixels;
				this.width = width;
				this.height = height;
			}

			public override void Reload(Texture2D texture)
			{
				texture.LoadImage(pixels, width, height);
			}
		}

		class TextureStreamReloader : TextureReloader
		{
			MemoryStream stream;

			public TextureStreamReloader(Stream stream)
			{
				this.stream = new MemoryStream();
				stream.CopyTo(this.stream);
				stream.Seek(0, SeekOrigin.Begin);
			}

			public override void Reload(Texture2D texture)
			{
				texture.LoadImage(stream);
			}
		}
		#endregion

		public delegate void TextureMissingDelegate(string path);

		public static bool IsStubTextureTransparent;

		private static readonly string[] AffordableTextureFileExtensions = {
#if iOS || ANDROID
			".pvr", ".jpg"
#else
			".dds", ".pvr", ".jpg", ".png"
#endif
		};

		private uint handle;
		public OpacityMask OpacityMask { get; private set; }
		public Size ImageSize { get; protected set; }
		public Size SurfaceSize { get; protected set; }
		private Rectangle uvRect;
		private TextureReloader reloader;
		private int usedOnRenderCycle;

		public ITexture AtlasTexture => this;

		public Rectangle AtlasUVRect { get { return uvRect; } }

		public void TransformUVCoordinatesToAtlasSpace(ref Vector2 uv) { }

		public bool IsStubTexture { get; private set; }

		private void SetTextureParameter(TextureParameterName name, int value)
		{
			if (handle == 0) {
				return;
			}
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, handle);
			GL.TexParameter(TextureTarget.Texture2D, name, value);
			PlatformRenderer.MarkTextureSlotAsDirty(0);
		}

		private TextureParams textureParams = TextureParams.Default;
		public TextureParams TextureParams {
			get
			{
				return textureParams;
			}
			set
			{
				if (textureParams == value) {
					return;
				}
				if (textureParams.WrapModeU != value.WrapModeU) SetTextureParameter(TextureParameterName.TextureWrapS, value.WrapModeU.ToInt());
				if (textureParams.WrapModeV != value.WrapModeV) SetTextureParameter(TextureParameterName.TextureWrapT, value.WrapModeV.ToInt());
				if (textureParams.MinFilter != value.MinFilter) SetTextureParameter(TextureParameterName.TextureMinFilter, value.MinFilter.ToInt());
				if (textureParams.MagFilter != value.MagFilter) SetTextureParameter(TextureParameterName.TextureMagFilter, value.MagFilter.ToInt());
				textureParams = value;
			}
		}

		public Texture2D()
		{
			GLObjectRegistry.Instance.Add(this);
		}

		private void LoadTextureParams(string path)
		{
			var textureParamsPath = path + ".texture";
			if (AssetBundle.Current.FileExists(textureParamsPath)) {
				using (var stream = AssetBundle.Current.OpenFile(textureParamsPath)) {
					TextureParams = Serialization.ReadObject<TextureParams>(textureParamsPath, stream);
				}
			} else {
				TextureParams = TextureParams.Default;
			}
		}

		public void LoadImage(string path, TextureMissingDelegate onTextureMissing = null)
		{
			IsStubTexture = false;

			try {
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
						LoadImageHelper(stream, createReloader: false);
					}

					LoadTextureParams(path);

					var maskPath = path + ".mask";
					if (AssetBundle.Current.FileExists(maskPath)) {
						OpacityMask = new OpacityMask(maskPath);
					}

					// Update audio buffers if the audio system performs in the main thread.
					AudioSystem.Update();

					return;
				}

				Console.WriteLine("Missing texture '{0}'", path);
				onTextureMissing?.Invoke(path);
				LoadStubImage(IsStubTextureTransparent && !path.IsNullOrWhiteSpace());
			} finally {
				reloader = new TextureBundleReloader(path);
			}
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
			reloader = new TextureStubReloader(transparent);
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

			LoadImageHelper(stream, createReloader: true);
		}

		public void LoadImage(Bitmap bitmap)
		{
			IsStubTexture = false;

			LoadImage(bitmap.GetPixels(), bitmap.Width, bitmap.Height);
		}

		private void LoadImageHelper(Stream stream, bool createReloader)
		{
			reloader = createReloader ? new TextureStreamReloader(stream) : null;
			Discard();
			if (!stream.CanSeek) {
				stream = new RewindableStream(stream);
			}
			using (var reader = new BinaryReader(stream)) {
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

		private void PrepareOpenGLTexture()
		{
			// Generate a new texture.
			if (handle == 0) {
				var t = new int[1];
				GL.GenTextures(1, t);
				handle = (uint)t[0];
			}
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, handle);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, textureParams.MinFilter.ToInt());
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, textureParams.MagFilter.ToInt());
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, textureParams.WrapModeU.ToInt());
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, textureParams.WrapModeV.ToInt());
			PlatformRenderer.MarkTextureSlotAsDirty(0);
			PlatformRenderer.CheckErrors();
		}

		/// <summary>
		/// Create texture from pixel array
		/// </summary>
		public void LoadImage(Color4[] pixels, int width, int height)
		{
			IsStubTexture = false;

			reloader = new TexturePixelArrayReloader(pixels, width, height);

			MemoryUsed = 4 * width * height;
			ImageSize = new Size(width, height);
			SurfaceSize = ImageSize;
			uvRect = new Rectangle(0, 0, 1, 1);

			Window.Current.InvokeOnRendering(() => {
				var pinnedArray = GCHandle.Alloc(pixels, GCHandleType.Pinned);
				var pointer = pinnedArray.AddrOfPinnedObject();

				PrepareOpenGLTexture();
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, handle);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pointer);
				PlatformRenderer.MarkTextureSlotAsDirty(0);
				PlatformRenderer.CheckErrors();

				pinnedArray.Free();
			});
		}

		public void LoadImage(IntPtr pixels, int width, int height)
		{
			LoadImage(pixels, width, height, PixelFormat.Rgba);
		}

		internal void LoadImage(IntPtr pixels, int width, int height, PixelFormat pixelFormat)
		{
			LoadImage(pixels, width, height, PixelInternalFormat.Rgba, pixelFormat);
		}

		internal void LoadImage(IntPtr pixels, int width, int height, PixelInternalFormat pixelInternalFormat, PixelFormat pixelFormat)
		{
			MemoryUsed = 4 * width * height;
			ImageSize = new Size(width, height);
			SurfaceSize = ImageSize;
			uvRect = new Rectangle(0, 0, 1, 1);

			Window.Current.InvokeOnRendering(() => {
				PrepareOpenGLTexture();
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, handle);
				GL.TexImage2D(TextureTarget.Texture2D, 0, pixelInternalFormat, width, height, 0, pixelFormat, PixelType.UnsignedByte, pixels);
				PlatformRenderer.MarkTextureSlotAsDirty(0);
				PlatformRenderer.CheckErrors();
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
				PrepareOpenGLTexture();
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, handle);
				GL.TexSubImage2D(TextureTarget.Texture2D, 0, x, y, width, height,
					PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
				PlatformRenderer.CheckErrors();
				PlatformRenderer.MarkTextureSlotAsDirty(0);
			});
		}

		~Texture2D()
		{
			Dispose();
		}

		public override void Dispose()
		{
			Discard();
			base.Dispose();
		}

		/// <summary>
		/// Returns native texture handle
		/// </summary>
		/// <returns></returns>
		public virtual uint GetHandle()
		{
			if (IsDisposed) {
				throw new ObjectDisposedException(GetType().Name);
			}
			if (handle == 0) {
				Reload();
			}
			usedOnRenderCycle = Renderer.RenderCycle;
			return handle;
		}

		public void Discard()
		{
			MemoryUsed = 0;
			if (handle != 0) {
				var capturedHandle = handle;
				Window.Current.InvokeOnRendering(() => {
					GL.DeleteTextures(1, new uint[] { capturedHandle });
				});
				handle = 0;
			}
		}

		public override void MaybeDiscardUnderPressure()
		{
			if (Renderer.RenderCycle - usedOnRenderCycle > 3) {
				Discard();
			}
		}

		private void Reload()
		{
			Window.Current.InvokeOnRendering(() => {
				if (reloader != null) {
					reloader.Reload(this);
				} else {
					PrepareOpenGLTexture();
					PlatformRenderer.CheckErrors();
				}
			});
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

		public Color4[] GetPixels()
		{
			throw new NotSupportedException();
		}
	}
}
#endif
