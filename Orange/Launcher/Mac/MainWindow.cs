using AppKit;
using System;
using Foundation;

namespace Launcher
{
	public partial class MainWindow : NSWindow
	{
		public MainWindow(IntPtr handle) : base(handle) { }

		[Export("initWithCoder:")]
		public MainWindow(NSCoder coder) : base(coder) { }


		public override bool CanBecomeKeyWindow {
			get { return true; }
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();
		}
	}
}
