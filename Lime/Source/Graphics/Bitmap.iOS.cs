#if iOS
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using ProtoBuf;

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

		public IBitmapImplementation Crop(Rectangle cropArea)
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