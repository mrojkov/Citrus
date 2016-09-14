using AVFoundation;
using Foundation;
using UIKit;

namespace EmptyProject.iOS
{
	[Register ("EmptyProjectAppDelegate")]
	public class EmptyProjectAppDelegate : Lime.AppDelegate
	{
		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.Ambient);
			Lime.Application.Initialize(new Lime.ApplicationOptions {
				DecodeAudioInSeparateThread = false,
				UsingDeferredHitTest = true
			});

			new EmptyProject.Application.Application();
			return true;
		}

		public static void Main(string[] args)
		{
			UIApplication.Main(args, null, "EmptyProjectAppDelegate");
		}
	}
}


