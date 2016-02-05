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

	public class Bitmap : IDisposable
	{
		private IBitmapImplementation implementation;

		public Bitmap()
		{
			implementation = new BitmapImplementation();
		}

		public Bitmap(Color4[] data, int width, int height)
		{
			implementation = new BitmapImplementation(data, width, height);
		}

		public Bitmap(Stream stream)
		{
			implementation = new BitmapImplementation(stream);
		}

		private Bitmap(IBitmapImplementation implementation)
		{
			this.implementation = implementation;
		}

		public int Width
		{
			get { return implementation.GetWidth(); }
		}

		public int Height
		{
			get { return implementation.GetHeight(); }
		}

		public Vector2 Size
		{
			get { return new Vector2(Width, Height); }
		}

		public Bitmap Clone()
		{
			return Crop(new IntRectangle(0, 0, Width, Height));
		}

		public Bitmap Crop(IntRectangle cropArea)
		{
			var newImplementation = implementation.Crop(cropArea);
			return new Bitmap(newImplementation);
		}

		public void Dispose()
		{
			if (implementation != null) {
				implementation.Dispose();
			}
		}

		public Color4[] GetPixels()
		{
			return implementation.GetPixels();
		}

		public bool IsValid()
		{
			return implementation != null && implementation.IsValid();
		}

		public Bitmap Rescale(int newWidth, int newHeight)
		{
			var newImplementation = implementation.Rescale(newWidth, newHeight);
			return new Bitmap(newImplementation);
		}

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