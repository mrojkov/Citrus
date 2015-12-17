using System;
using System.IO;

using ProtoBuf;

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

	[ProtoContract]
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

		public int Height
		{
			get { return implementation.GetHeight(); }
		}

		[ProtoMember(1)]
		public byte[] SerializationData
		{
			get
			{
				return IsValid() ? GetByteArray() : null;
			}

			set
			{
				if (value != null) {
					LoadFromByteArray(value);
				} else {
					Dispose();
				}
			}
		}

		public Vector2 Size
		{
			get { return new Vector2(Width, Height); }
		}

		public int Width
		{
			get { return implementation.GetWidth(); }
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

		private byte[] GetByteArray()
		{
			using (var stream = new MemoryStream()) {
				SaveToStream(stream);
				return stream.ToArray();
			}
		}

		private void LoadFromByteArray(byte[] data)
		{
			using (var stream = new MemoryStream(data)) {
				implementation = new BitmapImplementation(stream);
			}
		}
	}
}