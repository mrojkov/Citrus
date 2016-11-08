#if MAC
using System;
using System.Runtime.InteropServices;
using AppKit;
using Foundation;
using ObjCRuntime;

namespace Lime
{
	public class ClipboardImplementation : IClipboardImplementation
	{
		private const string Tag = "Clipboard";

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		private extern static global::System.IntPtr
		SendMessageAndGetIntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		private extern static bool
		SendMessageAndGetBool(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

		public string Text
		{
			get
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
			set
			{
				if (value == null || value == String.Empty) { return; }
				var pasteBoard = NSPasteboard.GeneralPasteboard;
				pasteBoard.ClearContents();
				pasteBoard.WriteObjects(new NSString[] { (NSString)value });
			}
		}
	}
}
#endif
