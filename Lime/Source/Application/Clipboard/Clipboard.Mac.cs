#if MAC
using System;
using AppKit;
using ObjCRuntime;
using Foundation;
using System.Runtime.InteropServices;


namespace Lime
{
	public static partial class Clipboard
	{
		[DllImport("/usr/lib/libobjc.dylib", EntryPoint="objc_msgSend")]
		private extern static global::System.IntPtr
		SendMessageAndGetIntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint="objc_msgSend")]
		private extern static bool
		SendMessageAndGetBool(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

		private static string GetTextImpl()
		{
			var pasteBoard = NSPasteboard.GeneralPasteboard;
			var classArray = NSArray.FromObjects(new Class("NSString"));

			bool hasText = SendMessageAndGetBool(
				pasteBoard.Handle,
				Selector.GetHandle("canReadObjectForClasses:options:"),
				classArray.Handle,
				IntPtr.Zero);
			
			string text = String.Empty;

			if (hasText) {
				NSObject[] objectsToPaste = NSArray.ArrayFromHandle<NSObject>(SendMessageAndGetIntPtr(
					pasteBoard.Handle,
					Selector.GetHandle("readObjectsForClasses:options:"),
					classArray.Handle,
					IntPtr.Zero));
				text = objectsToPaste[0].ToString();
			}
			return text;
		}

		private static void PutTextImpl(string text)
		{
			//Do not put empty strings into clipboard
			if (text == null || text == String.Empty) { return; }
			var pasteBoard = NSPasteboard.GeneralPasteboard;
			pasteBoard.ClearContents();
			pasteBoard.WriteObjects(new NSString[] {(NSString)text});
		}
	}
}
#endif

