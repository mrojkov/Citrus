using System;

using Foundation;
using AppKit;

namespace Launcher
{
	public partial class LoggingWindow : NSWindow
	{
		public LoggingWindow(IntPtr handle) : base(handle)
		{
		}

		[Export("initWithCoder:")]
		public LoggingWindow(NSCoder coder) : base(coder)
		{
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();
		}
	}
}
