using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using ProtoBuf;

namespace Lime
{
	interface IBitmapImplementation : IDisposable
	{
		int GetWidth();
		int GetHeight();
		void LoadFromStream(Stream stream);
		void SaveToStream(Stream stream);
		IBitmapImplementation Crop(Rectangle cropArea);
		IBitmapImplementation Rescale(int newWidth, int newHeight);
	}

	[ProtoContract]
	public class Bitmap : IDisposable
	{
		IBitmapImplementation implementation;
		public Vector2 Size { get { return new Vector2(Width, Height); } }
		public int Width { get { return implementation.GetWidth(); } }
		public int Height { get { return implementation.GetHeight(); } }

		[ProtoMember(1)]
		public byte[] AsByteArray
		{
			get { return GetByteArray(); }
			set { LoadFromByteArray(value); }
		}

		public Bitmap()
		{
			implementation = new BitmapImplementation();
		}

		private Bitmap(IBitmapImplementation implementation)
		{
			this.implementation = implementation;
		}

		public void LoadFromStream(Stream stream)
		{
			implementation.LoadFromStream(stream);
		}

		public void SaveToStream(Stream stream)
		{
			implementation.SaveToStream(stream);
		}

		public Bitmap Clone()
		{
			return Crop(new Rectangle(0, 0, Width - 1, Height - 1));
		}

		public Bitmap Rescale(int newWidth, int newHeight)
		{
			var newImplementation = implementation.Rescale(newWidth, newHeight);
			return new Bitmap(newImplementation);
		}

		public Bitmap Crop(Rectangle cropArea)
		{
			var newImplementation = implementation.Crop(cropArea);
			return new Bitmap(newImplementation);
		}

		public void Dispose()
		{
			implementation.Dispose();
		}

		private void LoadFromByteArray(byte[] data)
		{
			using (var stream = new MemoryStream(data)) {
				LoadFromStream(stream);
			}
		}

		private byte[] GetByteArray()
		{
			byte[] result;
			using (var stream = new MemoryStream()) {
				SaveToStream(stream);
				result = stream.ToArray();
			}
			return result;
		}
	}
}