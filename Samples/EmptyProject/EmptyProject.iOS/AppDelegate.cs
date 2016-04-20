using AVFoundation;
using Foundation;
using UIKit;

namespace EmptyProject.iOS
{
	[Register ("EmptyProjectAppDelegate")]
	public class EmptyProjectAppDelegate : UIApplicationDelegate
	{

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			AVAudioSession.SharedInstance ().SetCategory (AVAudioSessionCategory.Ambient);
			Lime.Serialization.Serializer = new Serializer ();
			Lime.Application.Initialize (new Lime.ApplicationOptions {
				DecodeAudioInSeparateThread = false,
				UsingDeferredHitTest = true
			});
			return true;
		}

		public static void Main(string[] args)
		{
			UIKit.UIApplication.Main (args, null, "EmptyProjectAppDelegate");
		}
	}
}


