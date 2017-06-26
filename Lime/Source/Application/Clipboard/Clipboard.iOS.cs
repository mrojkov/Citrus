#if iOS
using System;

namespace Lime
{
	public class ClipboardImplementation : IClipboardImplementation
	{
		public string Text
		{
			// TODO: Properly implement (Not implemented exception removed because of crunch)
			get;
			set;
		}
	}
}
#endif
