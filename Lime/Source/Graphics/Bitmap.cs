using System;
using System.IO;

#if WIN
using NativeBitmap = System.Drawing.Bitmap;
#elif iOS || MAC
using NativeBitmap = CoreGraphics.CGImage;
#elif ANDROID
using NativeBitmap = Android.Graphics.Bitmap;
#endif

namespace Lime
{
	public enum CompressionFormat
	{
		Jpeg,
		Png
	}

	internal interface IBitmapImplementation : IDisposable
	{
		NativeBitmap Bitmap { get; }
		int Width { get; }
		int Height { get; }
		bool IsValid { get; }
		bool HasAlpha { get; }
		IBitmapImplementation Clone();
		IBitmapImplementation Crop(IntRectangle cropArea);
		IBitmapImplementation Rescale(int newWidth, int newHeight);
		Color4[] GetPixels();
		void SaveTo(Stream stream, CompressionFormat compression);
	}

	/// <summary>
	/// Wraps native bitmap and exposes unified methods to work with images.
	/// </summary>
	public class Bitmap : IDisposable
	{
		private IBitmapImplementation implementation;

		/// <summary>
		/// Initializes a new instance of <see cref="Bitmap"/> class with the specified
		/// array of pixels, width and height.
		/// </summary>
		/// <param name="data">The array of pixels of <see cref="Color4"/> type.</param>
		/// <param name="width">The width, in pixels, of the new bitmap.</param>
		/// <param name="height">The height, in pixels, of the new bitmap.</param>
		public Bitmap(Color4[] data, int width, int height)
		{
			if (width * height != data.Length) {
				throw new ArgumentException("Pixel data doesn't fit width and height.");
			}
			implementation = new BitmapImplementation(data, width, height);
			Width = width;
			Height = height;
		}

		/// <summary>
		/// Initializes a new instance of <see cref="Bitmap"/> class from the specified data stream.
		/// </summary>
		/// <param name="stream">The data stream used to load the image.</param>
		public Bitmap(Stream stream)
		{
			implementation = new BitmapImplementation(stream);
			CacheDimensions();
		}

		/// <summary>
		/// Initialized a new instance of <see cref="Bitmap"/> class with specified bitmap implementation.
		/// </summary>
		/// <param name="implementation">The native implementation of bitmap to wrap by <see cref="Bitmap"/>.</param>
		private Bitmap(IBitmapImplementation implementation)
		{
			this.implementation = implementation;
			CacheDimensions();
		}

		/// <summary>
		/// Gets the width, in pixels, of this bitmap.
		/// </summary>
		public int Width { get; private set; }

		/// <summary>
		/// Gets the height, in pixels, of this bitmap.
		/// </summary>
		public int Height { get; private set; }

		/// <summary>
		/// Gets the size this bitmap.
		/// </summary>
		public Vector2 Size
		{
			get { return new Vector2(Width, Height); }
		}

		/// <summary>
		/// Gets a value indicating whether this bitmap is valid.
		/// </summary>
		/// <returns>true if this bitmap is not null or empty; otherwise, false.</returns>
		public bool IsValid
		{
			get { return implementation != null && implementation.IsValid; }
		}

		/// <summary>
		/// Gets a value indicating whether this bitmap has at least one non-opaque pixel.
		/// </summary>
		public bool HasAlpha
		{
			get { return implementation.HasAlpha; }
		}

		/// <summary>
		/// Gets a platform specific bitmap.
		/// </summary>
		public NativeBitmap NativeBitmap
		{
			get { return implementation.Bitmap; }
		}

		/// <summary>
		/// Determines is there any non-opaque pixel in the array of colors.
		/// </summary>
		/// <param name="colors">The array of colors.</param>
		/// <returns>True if there is any non-opaque pixel, otherwise False.</returns>
		internal static bool AnyAlpha(Color4[] colors)
		{
			foreach (var color in colors) {
				if (color.A != 255) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Creates an exact copy of this bitmap.
		/// </summary>
		/// <returns>The new bitmap that this method creates.</returns>
		public Bitmap Clone()
		{
			CheckValidity();
			return new Bitmap(implementation.Clone());
		}

		/// <summary>
		/// Creates a copy of the section of this bitmap defined by <see cref="IntRectangle"/> structure.
		/// </summary>
		/// <param name="cropArea">Defines the portion of this bitmap to copy.</param>
		/// <returns>The new bitmap that this method creates.</returns>
		/// <remarks>This method can not create empty bitmap. Section should be inside of the bitmap.</remarks>
		public Bitmap Crop(IntRectangle cropArea)
		{
			if (
				cropArea.Width <= 0 || cropArea.Height <= 0 ||
				cropArea.Left < 0 || cropArea.Top < 0 ||
				cropArea.Right > Width || cropArea.Bottom > Height) {
				throw new InvalidOperationException("Bitmap: Crop rectangle should be inside the image," +
					" and resulting bitmap should not be empty.");
			}
			if (cropArea.Width == Width && cropArea.Height == Height) {
				return Clone();
			}
			CheckValidity();
			return new Bitmap(implementation.Crop(cropArea));
		}

		/// <summary>
		/// Creates a copy of this bitmap scaled to the specified size.
		/// </summary>
		/// <param name="newWidth">The width, in pixels, of the new bitmap.</param>
		/// <param name="newHeight">The height, in pixels, of the new bitmap.</param>
		/// <returns>The new bitmap that this method creates.</returns>
		/// <remarks>This method can not create empty bitmap or flip it.</remarks>
		public Bitmap Rescale(int newWidth, int newHeight)
		{
			if (newWidth == 0 || newHeight == 0) {
				throw new ArgumentException("Bitmap: Resulting bitmap should not be empty.");
			}
			if (newWidth < 0 || newHeight < 0) {
				throw new ArgumentException("Bitmap: Width and height should be positive.");
			}
			if (newWidth == Width && newHeight == Height) {
				return Clone();
			}
			CheckValidity();
			return new Bitmap(implementation.Rescale(newWidth, newHeight));
		}

		/// <summary>
		/// Gets the array of pixels from this bitmap.
		/// </summary>
		/// <returns>An <see cref="Color4"/> array of pixels.</returns>
		public Color4[] GetPixels()
		{
			CheckValidity();
			return implementation.GetPixels();
		}

		/// <summary>
		/// Saves this image to the specified stream in specified compression format (default compression is PNG).
		/// </summary>
		/// <param name="stream">The stream where the image will be saved.</param>
		/// <param name="compression">Jpeg or Png.</param>
		public void SaveTo(Stream stream, CompressionFormat compression = CompressionFormat.Png)
		{
			CheckValidity();
			implementation.SaveTo(stream, compression);
		}

		/// <summary>
		/// Saves this image to file with specified path in specified compression format (default compression is PNG).
		/// </summary>
		/// <param name="path">The path to a file.</param>
		/// <param name="compression">Jpeg or Png.</param>
		public void SaveTo(string path, CompressionFormat compression = CompressionFormat.Png)
		{
			CheckValidity();
			using (var stream = File.Create(path)) {
				SaveTo(stream, compression);
			}
		}

		private bool disposed;

		private void Dispose(bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					if (implementation != null) {
						implementation.Dispose();
						implementation = null;
					}
				}

				disposed = true;
			}
		}

		~Bitmap()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		

		private void CacheDimensions()
		{
			Width = implementation.Width;
			Height = implementation.Height;
		}

		private void CheckValidity()
		{
			if (!IsValid) {
				throw new InvalidOperationException("Bitmap is not valid.");
			}
		}
	}

	/// <summary>
	/// Serves for <see cref="Bitmap"/> serialization only. You don't need to create instance of this class.
	/// </summary>
	public class SurrogateBitmap
	{
		/// <summary>
		/// Gets or sets a byte array with png-compressed image data.
		/// </summary>
		public byte[] SerializationData { get; set; }

		public static implicit operator Bitmap(SurrogateBitmap surrogate)
		{
			if (surrogate == null) {
				return null;
			}

			using (var memoryStream = new MemoryStream(surrogate.SerializationData)) {
				return new Bitmap(memoryStream);
			}
		}

		public static implicit operator SurrogateBitmap(Bitmap bitmap)
		{
			if (bitmap == null) {
				return null;
			}

			using (var memoryStream = new MemoryStream()) {
				bitmap.SaveTo(memoryStream);
				return new SurrogateBitmap { SerializationData = memoryStream.ToArray() };
			}
		}
	}
}
