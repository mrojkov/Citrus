#if iOS
using UIKit;

namespace Lime
{
	public class ClipboardImplementation : IClipboardImplementation
	{
		public string Text
		{
			get
			{
				return UIPasteboard.General.String ?? string.Empty;
			}

			set
			{
				UIPasteboard.General.String = value;
			}
		}
	}
}
#endif
