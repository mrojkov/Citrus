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
	public partial class Texture2D : CommonTexture, ITexture, IWidgetMaterialListHolder, IGLObject
	{
		#region TextureReloader
		abstract class TextureReloader
		{
			public abstract void Reload(Texture2D texture);
		}

		class TextureStubReloader : TextureReloader
		{
			public override void Reload(Texture2D texture)
			{
				texture.LoadStubImage();
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

		public virtual string SerializationPath
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		
		WidgetMaterialList IWidgetMaterialListHolder.WidgetMaterials { get; } = new WidgetMaterialList();

		public Rectangle AtlasUVRect { get { return uvRect; } }

		public void TransformUVCoordinatesToAtlasSpace(ref Vector2 uv) { }

		public bool IsStubTexture { get; private set; }

		private void SetTextureParameter(TextureParameterName name, int value)
		{
			if (handle == 0) {
				return;
			}
			PlatformRenderer.PushTexture(handle, 0);
			GL.TexParameter(TextureTarget.Texture2D, name, value);
			PlatformRenderer.PopTexture(0);
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
					} catch (Exception e) {
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
				LoadStubImage();
			} finally {
				reloader = new TextureBundleReloader(path);
			}
		}

		internal void LoadStubImage()
		{
			TextureParams = TextureParams.Default;
			OpacityMask = null;

			var pixels = new Color4[128 * 128];
			for (int i = 0; i < 128; i++)
				for (int j = 0; j < 128; j++)
					pixels[i * 128 + j] = (((i + (j & ~7)) & 8) == 0) ? Color4.Blue : Color4.White;

			LoadImage(pixels, 128, 128);
			
			IsStubTexture = true;
			reloader = new TextureStubReloader();
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
			PlatformRenderer.PushTexture(handle, 0);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, textureParams.MinFilter.ToInt());
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, textureParams.MagFilter.ToInt());
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, textureParams.WrapModeU.ToInt());
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, textureParams.WrapModeV.ToInt());
			PlatformRenderer.PopTexture(0);
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
				PlatformRenderer.PushTexture(handle, 0);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pointer);
				PlatformRenderer.PopTexture(0);
				PlatformRenderer.CheckErrors();
				
				pinnedArray.Free();
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
				PlatformRenderer.PushTexture(handle, 0);
				GL.TexSubImage2D(TextureTarget.Texture2D, 0, x, y, width, height,
					PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
				PlatformRenderer.CheckErrors();
				PlatformRenderer.PopTexture(0);
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
					PlatformRenderer.InvalidateTexture(capturedHandle);
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
