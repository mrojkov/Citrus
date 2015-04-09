#if UNITY
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using ProtoBuf;
using System.Runtime.InteropServices;

namespace Lime
{
	class BitmapImplementation : IBitmapImplementation
	{
		public int GetWidth()
		{
			return 0;
		}

		public int GetHeight()
		{
			return 0;
		}

		public void LoadFromStream(Stream stream)
		{
			throw new NotImplementedException();
		}

		public void SaveToStream(Stream stream)
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

		public void Dispose() {}

		public byte[] GetImageData()
		{		
			throw new NotImplementedException();
		}

		private void InitWithPngOrJpgBitmap(Stream stream)
		{
			throw new NotImplementedException();
		}

		public bool IsValid()
		{
			throw new NotImplementedException();
		}
	}
}
#endif