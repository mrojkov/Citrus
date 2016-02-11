using System;
using System.IO;

namespace Lime
{
	interface IBitmapImplementation : IDisposable
	{
		int GetWidth();
		int GetHeight();
		void SaveToStream(Stream stream);
		IBitmapImplementation Crop(IntRectangle cropArea);
		IBitmapImplementation Rescale(int newWidth, int newHeight);
		bool IsValid();
		Color4[] GetPixels();
	}

	/// <summary>
	/// Wraps native bitmap and exposes unified methods to work with images.
	/// </summary>
	public class Bitmap : IDisposable
	{
		private IBitmapImplementation implementation;

		/// <summary>
		/// Initializes a new instance of <see cref="Bitmap"/> class with the specified array of pixels, width and height.
		/// </summary>
		/// <param name="data">The array of pixels of <see cref="Color4"/> type.</param>
		/// <param name="width">The width, in pixels, of the new bitmap.</param>
		/// <param name="height">The height, in pixels, of the new bitmap.</param>
		public Bitmap(Color4[] data, int width, int height)
		{

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
			get { return implementation.GetWidth(); }
		}

		/// <summary>
		/// Gets the height, in pixels, of this bitmap.
		/// </summary>
		public int Height
		{
			get { return implementation.GetHeight(); }
		}

		/// <summary>
		/// Gets the size this bitmap.
		/// </summary>
		public Vector2 Size
		{
			get { return new Vector2(Width, Height); }
		}

		/// <summary>
		/// Creates an exact copy of this bitmap.
		/// </summary>
		/// <returns>The new bitmap that this method creates.</returns>
		public Bitmap Clone()
		{
			return Crop(new IntRectangle(0, 0, Width, Height));
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
				cropArea.Right > this.Width || cropArea.Bottom > this.Height
				) {
				throw new InvalidOperationException("Bitmap: Crop rectangle should be inside the image," +
					" and resulting bitmap should not be empty.");
			}

			var newImplementation = implementation.Crop(cropArea);
			return new Bitmap(newImplementation);
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

		/// <summary>
		/// Gets the array of pixels from this bitmap.
		/// </summary>
		/// <returns>An <see cref="Color4"/> array of pixels.</returns>
		public Color4[] GetPixels()
		{
			return implementation.GetPixels();
		}

		/// <summary>
		/// Determines whether this bitmap is valid.
		/// </summary>
		/// <returns>true if this bitmap is not null or empty; otherwise, false.</returns>
		public bool IsValid()
		{
			return implementation != null && implementation.IsValid();
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

			var newImplementation = implementation.Rescale(newWidth, newHeight);
			return new Bitmap(newImplementation);
		}

		/// <summary>
		/// Saves this image to the specified stream in PNG format.
		/// </summary>
		/// <param name="stream">The stream where the image will be saved.</param>
		public void SaveToStream(Stream stream)
		{
			implementation.SaveToStream(stream);
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
				bitmap.SaveToStream(memoryStream);
				return new SurrogateBitmap { SerializationData = memoryStream.ToArray() };
			}
		}
	}
}