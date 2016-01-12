#if ANDROID
using System;

namespace Lime
{
	public class ClipboardImplementation : IClipboardImplementation
	{
		public string Text
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
	}
}
#endif
