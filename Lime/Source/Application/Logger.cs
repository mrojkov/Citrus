using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;


#if iOS
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif

namespace Lime
{
	public class Logger
	{
#if iOS
		[DllImport(MonoTouch.Constants.FoundationLibrary)]
		private extern static void NSLog(IntPtr message);

		public static void Write(string msg)
		{
			using (var nss = new NSString(msg)) {
				NSLog(nss.Handle);
			}
		}

		public static void Write(string msg, params object[] args)
		{
			Write(string.Format(msg, args));
		}
#else
		public static void Write(string msg)
		{
			Console.WriteLine(msg);
		}

		public static void Write(string msg, params object[] args)
		{
			Console.WriteLine(msg, args);
		}
#endif
	}
}
