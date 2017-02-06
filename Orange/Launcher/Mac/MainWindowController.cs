using System;
using Foundation;
using AppKit;

namespace Launcher
{
	public partial class MainWindowController : NSWindowController
	{
		private LoggingWindowController loggingWindowConroller;

		public LogWriter LogWriter;

		public MainWindowController(IntPtr handle) : base(handle)
		{
		}

		[Export("initWithCoder:")]
		public MainWindowController(NSCoder coder) : base(coder)
		{
		}

		public MainWindowController() : base("MainWindow")
		{
			loggingWindowConroller = new LoggingWindowController();
			loggingWindowConroller.Window.OrderOut(this);
			LogWriter = new LogWriter (loggingWindowConroller.Log);
		}

		public void SetBuildStatus(string status)
		{
			loggingWindowConroller.SetBuildStatus(status);
		}

		public void ShowLog()
		{
			InvokeOnMainThread(() => {
				loggingWindowConroller.Window.MakeKeyAndOrderFront(this);
				loggingWindowConroller.Window.WillClose += (sender, e) => NSApplication.SharedApplication.Terminate(this);
				Close();
			});
		}

		public override void KeyUp(NSEvent e)
		{
			if (e.KeyCode == (ushort)NSKey.F1) {
				ShowLog();
			}
			base.KeyUp(e);
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();
		}

		public new MainWindow Window => (MainWindow)base.Window;
	}
}
