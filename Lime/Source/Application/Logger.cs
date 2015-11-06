using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

#if iOS
using ObjCRuntime;
using Foundation;
using UIKit;
#endif

namespace Lime
{
	public class Logger
	{
		public static event Action<string> OnWrite;
#if iOS
		[DllImport(Constants.FoundationLibrary)]
		private extern static void NSLog(IntPtr message);

		public static void Write(string msg)
		{
			using (var nss = new NSString(msg)) {
				NSLog(nss.Handle);
			}
			if (OnWrite != null) {
				OnWrite(msg);
			}
		}

		public static void Write(string msg, params object[] args)
		{
			Write(string.Format(msg, args));
		}
#else
		public static void Write(string msg)
		{
#if UNITY
			UnityEngine.Debug.Log(msg);
#else
			Console.WriteLine(msg);
#endif
			if (OnWrite != null) {
				OnWrite(msg);
			}
		}

		public static void Write(string format, params object[] args)
		{
			Write(string.Format(format, args));
		}
#endif
	}
}
