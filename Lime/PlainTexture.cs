using System;
using System.Diagnostics;
using System.IO;
#if iOS
using System.Runtime.InteropServices;
using MonoTouch.UIKit;
using OpenTK.Graphics.ES11;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using System.Drawing;
#elif MAC
using MonoMac.OpenGL;
using System.Drawing;
using MonoMac.CoreGraphics;
#else
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;
#endif

namespace Lime
{
	/// <summary>
	/// Represents 2D texture.
	/// </summary>
	public class PlainTexture : ITexture, IDisposable
	{
		uint id;
		Size imgSize = new Size (0, 0);
		Size surSize = new Size (0, 0);
		Rectangle uvRect;
		
		/// <summary>
		/// Constructor.
		/// </summary>
		public PlainTexture ()
		{
		}

		/// <summary>
		/// Size of texture.
		/// </summary>
		public Size ImageSize {
			get { return imgSize; }
		}
		
		/// <summary>
		/// Size of texture surface.
		/// </summary>		
		public Size SurfaceSize { 
			get {
				return surSize;
			} 
		}
		
		public Rectangle UVRect { get { return uvRect; } }
		
		/// <summary>
		/// Loads an image from file.
		/// </summary>
		public void LoadImage (string path)
		{
			using (Stream stream = AssetsBundle.Instance.OpenFile (path)) {
				LoadImage (stream);
			}
		}
		
#if iOS || MAC
		private void InitWithCGImage (CGImage image)
		{
			CGImageAlphaInfo info = image.AlphaInfo;
			bool hasAlpha = (info == CGImageAlphaInfo.PremultipliedLast) || 
				(info == CGImageAlphaInfo.PremultipliedFirst) || 
				(info == CGImageAlphaInfo.Last) || 
				(info == CGImageAlphaInfo.First);
			
			if (image.ColorSpace == null)
				throw new Exception ("Unsupported image format");

			imgSize = new Size (image.Width, image.Height);
			CGAffineTransform transform = CGAffineTransform.MakeIdentity ();
			
			surSize.Width = Utils.NearestPowerOf2 (imgSize.Width);	
			surSize.Height = Utils.NearestPowerOf2 (imgSize.Height);

			while (surSize.Width > 1024 || surSize.Height > 1024) {
				surSize.Width /= 2;
				surSize.Height /= 2;
				transform = CGAffineTransform.MakeScale (0.5f, 0.5f);
				imgSize.Width /= 2;
				imgSize.Height /= 2;
			}
			
			using (CGColorSpace colorSpace = CGColorSpace.CreateDeviceRGB ()) {
				if (hasAlpha) {
					byte[] pixels = new byte [surSize.Height * surSize.Width * 4];
					var context = new CGBitmapContext (pixels, 
						surSize.Width, surSize.Height, 8, 4 * surSize.Width, 
						colorSpace, CGImageAlphaInfo.PremultipliedLast);
					using (context) {
						context.ClearRect (new RectangleF (0, 0, surSize.Width, surSize.Height));
						context.TranslateCTM (0, surSize.Height - imgSize.Height);
						
						if (!transform.IsIdentity)
							context.ConcatCTM (transform);
						
						context.DrawImage (new RectangleF (0, 0, image.Width, image.Height), image);	
						
						short[] pixels2 = new short [surSize.Width * surSize.Height];
						int j = 0;
						int c = surSize.Width * surSize.Height * 4;
						for (int i = 0; i < c; i += 4) {
							short R = (short)((pixels [i] >> 4) << 12);
							short G = (short)((pixels [i + 1] >> 4) << 8);
							short B = (short)((pixels [i + 2] >> 4) << 4);
							short A = (short)((pixels [i + 3] >> 4));
							pixels2 [j++] = (short)(R | G | B | A);
						}

#if MAC
						GL.TexImage2D (TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, surSize.Width, surSize.Height, 0, 
							PixelFormat.Rgba, PixelType.UnsignedShort4444, pixels2);
#else
						GL.TexImage2D (All.Texture2D, 0, (int)All.Rgba, surSize.Width, surSize.Height, 0, All.Rgba, All.UnsignedShort4444, pixels2);
#endif
					}
				} else {
					byte[] pixels = new byte [surSize.Height * surSize.Width * 4];
					var context = new CGBitmapContext (pixels, 
						surSize.Width, surSize.Height, 8, 4 * surSize.Width, 
						colorSpace, CGImageAlphaInfo.NoneSkipLast);
					using (context) {
						context.ClearRect (new RectangleF (0, 0, surSize.Width, surSize.Height));
						context.TranslateCTM (0, surSize.Height - imgSize.Height);
						
						if (!transform.IsIdentity)
							context.ConcatCTM (transform);
						
						context.DrawImage (new RectangleF (0, 0, image.Width, image.Height), image);
						
						short[] pixels2 = new short [surSize.Width * surSize.Height];
						int j = 0;
						int c = surSize.Width * surSize.Height * 4;
						for (int i = 0; i < c; i += 4) {
							short R = (short)((pixels [i] >> 3) << 11);
							short G = (short)((pixels [i + 1] >> 2) << 5);
							short B = (short)(pixels [i + 2] >> 3);
							pixels2 [j++] = (short)(R | G | B);
						}
#if MAC
						GL.TexImage2D (TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, surSize.Width, surSize.Height, 0, 
							PixelFormat.Rgb, PixelType.UnsignedShort565, pixels2);
#else
						GL.TexImage2D (All.Texture2D, 0, (int)All.Rgb, surSize.Width, surSize.Height, 0, All.Rgb, All.UnsignedShort565, pixels2);
#endif
						Renderer.Instance.CheckErrors ();
					}
				}
			}
		}
#endif	
		/// <summary>
		/// Loads an image from stream.
		/// </summary>
		public void LoadImage (Stream stream)
		{
#if GLES11 || MAC
			// Discards current texture.
			Dispose ();

#if GLES11			
			// Generate a new texture.
			GL.GenTextures (1, ref id);
			GL.BindTexture (All.Texture2D, id);
			GL.TexParameter (All.Texture2D, All.TextureMinFilter, (int)All.Linear);
			GL.TexParameter (All.Texture2D, All.TextureMagFilter, (int)All.Linear);
			GL.Hint (All.PerspectiveCorrectionHint, All.Fastest);
#else			
			// Generate a new texture.
			id = (uint)GL.GenTexture ();
			GL.BindTexture (TextureTarget.Texture2D, id);
			GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.Hint (HintTarget.PerspectiveCorrectionHint, HintMode.Fastest);
#endif				
			byte[] sign = new byte [4];
			stream.Read (sign, 0, 4);
			if (sign [1] == 'P' && sign [2] == 'N' && sign [3] == 'G') {
				stream.Seek (0, SeekOrigin.Begin);				
				int length = (int)stream.Length;
				byte[] buffer = new byte [length];
				if (stream.Read (buffer, 0, length) != length)
					throw new Exception ("Unable to read stream");

				CGDataProvider provider = new CGDataProvider (buffer, 0, length);
				using (CGImage img = CGImage.FromPNG (provider, null, false, CGColorRenderingIntent.Default)) {
					InitWithCGImage (img);
				}
			} else if (sign [0] == 'R' && sign [1] == 'A' && sign [2] == 'W' && sign [3] == ' ') {
				using (BinaryReader reader = new BinaryReader (stream)) {
					imgSize = new Size (reader.ReadInt32 (), reader.ReadInt32 ());
					reader.ReadInt32 (); // surface width
					reader.ReadInt32 (); // surface height
					BitmapFormat format = (BitmapFormat)reader.ReadInt32();
					reader.ReadInt32 (); // image size in bytes
					int bytesPerPixel = 4;
					if (format == BitmapFormat.R5G6B5 || format == BitmapFormat.RGBA4)
						bytesPerPixel = 2;
					surSize.Width = Utils.NearestPowerOf2 (imgSize.Width);
					surSize.Height = Utils.NearestPowerOf2 (imgSize.Height);
					byte[] buffer = new byte [surSize.Width * surSize.Height * bytesPerPixel];
					for (int i = 0; i < imgSize.Height; i++) {
						reader.Read (buffer, i * surSize.Width * bytesPerPixel, imgSize.Width * bytesPerPixel);
					}
					switch (format)
					{
#if GLES11
					case BitmapFormat.RGBA4:						
						GL.TexImage2D (All.Texture2D, 0, (int)All.Rgba, surSize.Width, surSize.Height, 0, 
							All.Rgba, All.UnsignedShort4444, buffer);
						break;
					case BitmapFormat.R5G6B5:
						GL.TexImage2D (All.Texture2D, 0, (int)All.Rgb, surSize.Width, surSize.Height, 0, 
							All.Rgb, All.UnsignedShort565, buffer);
						break;
#else
					case BitmapFormat.RGBA4:
						GL.TexImage2D (TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, surSize.Width, surSize.Height, 0, 
								PixelFormat.Rgba, PixelType.UnsignedShort4444, buffer);
						break;
					case BitmapFormat.R5G6B5:
						GL.TexImage2D (TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, surSize.Width, surSize.Height, 0, 
							PixelFormat.Rgb, PixelType.UnsignedShort565, buffer);
						break;
#endif
					default:
						throw new NotImplementedException ();
					}
				}
			}
			Renderer.Instance.CheckErrors ();
#else
			// Discards current texture.
			Dispose ();
			
			Gtk.Application.Init ();
			
			// Generate a new texture.
			id = (uint)GL.GenTexture ();
			GL.BindTexture (TextureTarget.Texture2D, id);
			GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.Hint (HintTarget.PerspectiveCorrectionHint, HintMode.Fastest);
			
			using (var pb = new Gdk.Pixbuf (stream)) {
				if (pb.HasAlpha) {
					GL.TexImage2D (TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, pb.Width, pb.Height, 0,
						OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, pb.Pixels);
				} else {
					GL.TexImage2D (TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, pb.Width, pb.Height, 0,
						OpenTK.Graphics.OpenGL.PixelFormat.Rgb, PixelType.UnsignedByte, pb.Pixels);
				}
				imgSize = new Size (pb.Width, pb.Height);
				surSize = imgSize;
				//surSize.Width = NearestPowerOf2 (imgSize.Width);
				//surSize.Height = NearestPowerOf2 (imgSize.Height);
			}
			/*
			// Load the texture from stream.
			System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap (stream);

			BitmapData data = bitmap.LockBits (new Rectangle (0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			GL.TexImage2D (TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

			bitmap.UnlockBits (data);
			*/
#endif
			uvRect = new Rectangle (Vector2.Zero, (Vector2)ImageSize / (Vector2)SurfaceSize);
		}

		/// <summary>
		/// Create texture from pixel array.
		/// </summary>
		public void LoadImage (Color4[] pixels, int width, int height)
		{		
#if GLES11
			// Discards current texture.
			Dispose ();
			
			// Generate a new texture.
			GL.GenTextures (1, ref id);
			
			GL.BindTexture (All.Texture2D, id);
			GL.TexParameter (All.Texture2D, All.TextureMinFilter, (int)All.Linear);
			GL.TexParameter (All.Texture2D, All.TextureMagFilter, (int)All.Linear);
			GL.Hint (All.PerspectiveCorrectionHint, All.Fastest);
			
			GL.TexImage2D (All.Texture2D, 0, (int)All.Rgba, width, height, 0,
            	All.Rgba, All.UnsignedByte, pixels);

			imgSize = new Size (width, height);
			surSize = imgSize;
#else
			// Discards current texture.
			Dispose ();

			// Generate a new texture.
			id = (uint)GL.GenTexture ();
			GL.BindTexture (TextureTarget.Texture2D, id);
			GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.Hint (HintTarget.PerspectiveCorrectionHint, HintMode.Fastest);

			GL.TexImage2D (TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
            	PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

			imgSize = new Size (width, height);
			surSize = imgSize;
#endif
			Renderer.Instance.CheckErrors ();
			uvRect = new Rectangle (Vector2.Zero, (Vector2)ImageSize / (Vector2)SurfaceSize);
		}

		private void Dispose (bool manual)
		{
			if (id != 0) {
				if (manual) {
#if GLES11
					GL.DeleteTextures (1, new uint[] { id });
#else						
					GL.DeleteTexture (id);
#endif
					Renderer.Instance.CheckErrors ();
				} else
					Console.WriteLine ("[Warning] {0} leaked.", this);
				id = 0;
			}
		}

		/// <summary>
		/// Frees OpenGL texture
		/// </summary>
		public void Dispose ()
		{
			this.Dispose (true);
		}

		/// <summary>
		/// Returns native texture handle.
		/// </summary>
		/// <returns></returns>
		public uint GetHandle ()
		{
			return id;
		}

		/// <summary>
		/// Sets texture as a render target.
		/// </summary>
		public void SetAsRenderTarget ()
		{
		}

		/// <summary>
		/// Restores default render target (backbuffer).
		/// </summary>
		public void RestoreRenderTarget ()
		{
		}

		/// <summary>
		/// Checks pixel transparency at given coordinates.
		/// </summary>
		/// <param name="x">x-coordinate of pixel</param>
		/// <param name="y">y-coordinate of pixel</param>
		/// <returns></returns>
		public bool IsTransparentPixel (int x, int y)
		{
			return false;
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~PlainTexture ()
		{
			Dispose (false);
		}
	}
}