using System;
using MonoTouch;
using MonoTouch.Foundation;
using System.Runtime.InteropServices;

namespace TestFlight
{
	public partial class TestFlight : NSObject
	{
		[DllImport("__Internal", EntryPoint = "TFLog")]
		private extern static void WrapperTfLog(IntPtr handle);
		
		public static void Log (string msg, params object [] args)
		{
			using (var nss = new NSString (string.Format (msg, args)))
				WrapperTfLog (nss.Handle);
		}
	}
}
