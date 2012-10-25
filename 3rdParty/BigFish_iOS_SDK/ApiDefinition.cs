using System;
using System.Drawing;

using MonoTouch.ObjCRuntime;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;

namespace BigFish_iOS_SDK
{
	[BaseType(typeof(NSObject), Name="bfgManager")]
	interface BfgManager
	{
		[Static, Export("sharedInstance")]
		BfgManager SharedInstance { get; }

		[Static, Export("initializeWithViewController:")]
		void InitializeWithViewController(UIViewController controller);

		[Static, Export("initializeWithWindow:")]
		void InitializeWithWindow(UIWindow rootWindow);

		[Static, Export("showMoreGames")]
		void ShowMoreGames();

		[Static, Export("removeMoreGames")]
		void RemoveMoreGames();

		[Static, Export("sendContinueMessage")]
		void SendContinueMessage();

		[Static, Export("sendMainMenuMessage")]
		void SendMainMenuMessage();

		[Static, Export("isInitialized")]
		bool IsInitialized { get; }

		[Static, Export("isWindowed")]
		bool IsWindowed { get; }

		[Static, Export("setWindowed:")]
		void SetWindowed(bool bWindowed);

		[Static, Export("isFirstTime")]
		bool IsFirstTime { get; }

		[Static, Export("isSkinned")]
		bool IsSkinned { get; }

		[Static, Export("getGameScreenShot")]
		UIImage GetGameScreenShot();

		[Static, Export("getGameScreenShotTransform")]
		CGAffineTransform GetGameScreenShotTransform();

		[Static, Export("currentOrientation")]
		UIInterfaceOrientation CurrentOrientation { get; }

		[Static, Export("navigateToLinkShareURL:")]
		bool NavigateToLinkShareURL(NSString url);

		[Static, Export("currentUIType")]
		int CurrentUIType { get; }

		[Static, Export("currentUITypeAsString")]
		NSString CurrentUITypeAsString { get; }
		//
		// setParentViewController
		///
		/// \details  The view controller to use when showing the GDN,
		/// for both startup and resume UI. If you change your root ViewController
		/// for your application, you should update bfgManager
		[Static, Export("setParentViewController:")]
		void SetParentViewController(UIViewController parent);

		[Static, Export("getParentViewController")]
		UIViewController GetParentViewController();

		[Static, Export("getServerWithDomain:")]
		NSString GetServerWithDomain(NSString server);
		///
		/// \details Returns true if ads are currently running. 
		///
		[Export("adsRunning")]
		bool AdsRunning { get; }
		///
		/// \details  Starts ads based on the origin.
		///
		[Export("startAds:")]
		bool StartAds(AdsOrigin origin);

		[Export("stopAds")]
		bool StopAds();
		/// \details Stops displaying ads but keeps the ad controller alive.  This method should be used when the user is temporarily navigating away from the screen showing the ads and will return to it soon.  
		///
		[Export("pauseAds")]
		void PauseAds();
		///
		/// \details Makes an already created and started ad controller show ads again. Use this method if you have paused the ad controller before.
		///
		[Export("resumeAds")]
		void ResumeAds();

		[Export("startBranding")]
		bool StartBranding();			

		[Export("stopBranding")]
		void StopBranding();
	}

	//	+(void) initializeWithViewController:(UIViewController*) controller;
	//
	//	+(void) showMoreGames;
	//	+(void) removeMoreGames;
	//
	//	+(void) sendContinueMessage;
	//	+(void) sendMainMenuMessage;
	//
	//	+(bool) isInitialized;
	//	+(bool) isWindowed;
	//	+(void) setWindowed:(bool)bWindowed;
	//	+(bool) isFirstTime;
	//	+(bool) isSkinned;
	//
	//	+(UIImage *) getGameScreenShot;
	//	+(CGAffineTransform) getGameScreenShotTransform;
	//
	//	+(UIInterfaceOrientation) currentOrientation;
	//
	//	+(bool) navigateToLinkShareURL:(NSString *) url;
	//
	//	+(int) currentUIType;
	//	+(NSString *) currentUITypeAsString;
	//
	//	//
	//	// setParentViewController
	//	///
	//	/// \details  The view controller to use when showing the GDN,
	//	/// for both startup and resume UI. If you change your root ViewController
	//	/// for your application, you should update bfgManager
	//	///
	//	///
	//	+ (void) setParentViewController:(UIViewController *) parent;
	//	+ (UIViewController *) getParentViewController;
	//
	//	+ (NSString *) getServerWithDomain:(NSString *) server;
	//
	//	@end

		// The first step to creating a binding is to add your native library ("libNativeLibrary.a")
		// to the project by right-clicking (or Control-clicking) the folder containing this source
		// file and clicking "Add files..." and then simply select the native library (or libraries)
		// that you want to bind.
		//
		// When you do that, you'll notice that MonoDevelop generates a code-behind file for each
		// native library which will contain a [LinkWith] attribute. MonoDevelop auto-detects the
		// architectures that the native library supports and fills in that information for you,
		// however, it cannot auto-detect any Frameworks or other system libraries that the
		// native library may depend on, so you'll need to fill in that information yourself.
		//
		// Once you've done that, you're ready to move on to binding the API...
		//
		//
		// Here is where you'd define your API definition for the native Objective-C library.
		//
		// For example, to bind the following Objective-C class:
		//
		//     @interface Widget : NSObject {
		//     }
		//
		// The C# binding would look like this:
		//
		//     [BaseType (typeof (NSObject))]
		//     interface Widget {
		//     }
		//
		// To bind Objective-C properties, such as:
		//
		//     @property (nonatomic, readwrite, assign) CGPoint center;
		//
		// You would add a property definition in the C# interface like so:
		//
		//     [Export ("center")]
		//     PointF Center { get; set; }
		//
		// To bind an Objective-C method, such as:
		//
		//     -(void) doSomething:(NSObject *)object atIndex:(NSInteger)index;
		//
		// You would add a method definition to the C# interface like so:
		//
		//     [Export ("doSomething:atIndex:")]
		//     void DoSomething (NSObject object, int index);
		//
		// Objective-C "constructors" such as:
		//
		//     -(id)initWithElmo:(ElmoMuppet *)elmo;
		//
		// Can be bound as:
		//
		//     [Export ("initWithElmo:")]
		//     IntPtr Constructor (ElmoMuppet elmo);
		//
		// For more information, see http://docs.xamarin.com/ios/advanced_topics/binding_objective-c_types
		//
}

