using System;
using System.IO;

#if WIN
using NativeBitmap = System.Drawing.Bitmap;
#elif MAC
using NativeBitmap = AppKit.NSImage;
#elif iOS
using NativeBitmap = UIKit.UIImage;
#elif ANDROID
using NativeBitmap = Android.Graphics.Bitmap;
#endif

namespace Lime
{
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
		}

		/// <summary>
		/// Initializes a new instance of <see cref="Bitmap"/> class from the specified data stream.
		/// </summary>
		/// <param name="stream">The data stream used to load the image.</param>
		public Bitmap(Stream stream)
		{
			implementation = new BitmapImplementation(stream);
		}

		/// <summary>
		/// Initialized a new instance of <see cref="Bitmap"/> class with specified bitmap implementation.
		/// </summary>
		/// <param name="implementation">The native implementation of bitmap to wrap by <see cref="Bitmap"/>.</param>
		private Bitmap(IBitmapImplementation implementation)
		{
			this.implementation = implementation;
		}

		/// <summary>
		/// Gets the width, in pixels, of this bitmap.
		/// </summary>
		public int Width
		{
			get { return implementation.Width; }
		}

		/// <summary>
		/// Gets the height, in pixels, of this bitmap.
		/// </summary>
		public int Height
		{
			get { return implementation.Height; }
		}

		/// <summary>
		/// Gets the size this bitmap.
		/// </summary>
		public Vector2 Size
		{
			get { return new Vector2(Width, Height); }
		}

		/// <summary>
		/// Determines whether this bitmap is valid.
		/// </summary>
		/// <returns>true if this bitmap is not null or empty; otherwise, false.</returns>
		public bool IsValid
		{
			get { return implementation != null && implementation.IsValid; }
		}

		/// <summary>
		/// Gets a platform specific bitmap.
		/// </summary>
		public NativeBitmap NativeBitmap
		{
			get { return implementation.Bitmap; }
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
				cropArea.Right > Width || cropArea.Bottom > Height
				) {
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
		/// Saves this image to the specified stream in PNG format.
		/// </summary>
		/// <param name="stream">The stream where the image will be saved.</param>
		public void SaveTo(Stream stream)
		{
			CheckValidity();
			implementation.SaveTo(stream);
		}

		private void CheckValidity()
		{
			if (!IsValid) {
				throw new InvalidOperationException("Bitmap is not valid.");
			}
		}

		/// <summary>
		/// Releases all resources used by this bitmap.
		/// </summary>
		public void Dispose()
		{
			if (implementation != null) {
				implementation.Dispose();
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

	interface IBitmapImplementation : IDisposable
	{
		NativeBitmap Bitmap { get; }
		int Width { get; }
		int Height { get; }
		bool IsValid { get; }
		IBitmapImplementation Clone();
		IBitmapImplementation Crop(IntRectangle cropArea);
		IBitmapImplementation Rescale(int newWidth, int newHeight);
		Color4[] GetPixels();
		void SaveTo(Stream stream);
	}
}