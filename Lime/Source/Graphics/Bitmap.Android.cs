#if ANDROID
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using ProtoBuf;
using System.Drawing;

namespace Lime
{
	class BitmapImplementation : IBitmapImplementation
	{
		public int GetWidth()
		{
			throw new NotImplementedException();
		}

		public int GetHeight()
		{
			throw new NotImplementedException();
		}

		public void LoadFromStream(Stream stream)
		{
			throw new NotImplementedException();		
		}

		public void SaveToStream(Stream stream)
		{
			throw new NotImplementedException();
		}

		public IBitmapImplementation Rescale(int newWidth, int newHeight)
		{
			throw new NotImplementedException();
		}

		public IBitmapImplementation Crop(IntRectangle cropArea)
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