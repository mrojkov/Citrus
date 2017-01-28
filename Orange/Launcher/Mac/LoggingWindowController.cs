using System;

using Foundation;
using AppKit;

namespace Launcher
{
	public partial class LoggingWindowController : NSWindowController
	{
		private NSTextField buildStatus;
		private NSTextView buildLog;

		public LoggingWindowController(IntPtr handle) : base(handle)
		{
		}

		[Export("initWithCoder:")]
		public LoggingWindowController(NSCoder coder) : base(coder)
		{
		}

		public LoggingWindowController() : base("LoggingWindow")
		{
			var copyButton = (NSButton)Window.ContentView.Subviews[0];
			buildStatus = (NSTextField)Window.ContentView.Subviews[2];
			buildLog = (NSTextView)(((NSScrollView)Window.ContentView.Subviews[3]).DocumentView);
			// TODO
			//copyButton.Activated += ;
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();
		}

		public void Log(string line)
		{
			InvokeOnMainThread(() => 
				{
					buildLog.Value += line + Environment.NewLine; 
					buildLog.ScrollRangeToVisible(new NSRange(buildLog.Value.Length, 0));
				}
			);
		}

		public void SetBuildStatus(string status)
		{
			InvokeOnMainThread(() => buildStatus.AttributedStringValue = new NSAttributedString(status));
		}

		public new LoggingWindow Window => (LoggingWindow)base.Window;
	}
}
