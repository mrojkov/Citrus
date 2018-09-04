using System;
using Foundation;
using AppKit;
using Lime;
using Tangerine.Core;
using System.Collections.Generic;

namespace Tangerine
{
	public class AppDelegate : NSApplicationDelegate
	{
		public override void DidFinishLaunching(NSNotification notification)
		{
			TangerineApp.Initialize();
		}
	}
}