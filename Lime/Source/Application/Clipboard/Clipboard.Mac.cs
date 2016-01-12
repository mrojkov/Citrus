#if MAC || MONOMAC
using AppKit;
using ObjCRuntime;
using Foundation;

namespace Lime
{
	public class ClipboardImplementation : IClipboardImplementation
	{
		public string Text
		{
			get
			{
				var pasteBoard = NSPasteboard.GeneralPasteboard;
				Class[] classArray = { new Class("NSString") };
				var hasText = pasteBoard.CanReadObjectForClasses(classArray, null);
				if (hasText) {
					NSObject[] objectsToPaste = pasteBoard.ReadObjectsForClasses(classArray, null);
					retutn objectsToPaste[0].ToString();
				}
				return string.Empty;
			}

			set
			{
				if (!string.IsNullOrEmpty(value)) {
					var pasteBoard = NSPasteboard.GeneralPasteboard;
					pasteBoard.ClearContents();
					pasteBoard.WriteObjects(new NSString[] { (NSString)value });
				}
			}
		}
	}
}
#endif

