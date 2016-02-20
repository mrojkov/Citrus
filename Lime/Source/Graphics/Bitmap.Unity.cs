#if UNITY
using System;
using System.IO;

namespace Lime
{
	class BitmapImplementation : IBitmapImplementation
	{
		public int Width
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public int Height
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public bool IsValid
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public IBitmapImplementation Clone()
		{
			throw new NotImplementedException();
		}

		public IBitmapImplementation Crop(IntRectangle cropArea)
		{
			throw new NotImplementedException();
		}

		public IBitmapImplementation Rescale(int newWidth, int newHeight)
		{
			throw new NotImplementedException();
		}

		public Color4[] GetPixels()
		{
			throw new NotImplementedException();
		}

		public void SaveTo(Stream stream)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}
}
#endif