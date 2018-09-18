using System;

namespace Lzma
{
	public class LzmaException : Exception
	{
		public LzmaException(string msg) : base(msg) { }
	}
}