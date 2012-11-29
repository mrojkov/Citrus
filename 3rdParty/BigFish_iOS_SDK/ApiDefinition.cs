using System;
using System.Drawing;

using MonoTouch.ObjCRuntime;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;

namespace BigFish_iOS_SDK
{
	[BaseType(typeof(NSObject), Name = "bfgRating")]
	interface BfgRating
	{
		[Static, Export("initialize")]
		void Initialize();

		[Static, Export("mainMenuRateApp")]
		void MainMenuRateApp();

		[Static, Export("immediateTrigger")]
		void ImmediateTrigger();
	}

	[BaseType(typeof(NSObject), Name = "bfgGameReporting")]
	interface BfgGameReporting
	{
		[Static, Export("sharedInstance")]
		BfgGameReporting SharedInstance { get; }

		[Export("logMainMenuShown")]
		void LogMainMenuShown();
	
		[Export("logRateMainMenuCancelled")]
		void LogRateMainMenuCanceled();
	
		[Export("logOptionsShown")]
		void LogOptionsShown();
	
		[Export("logPurchaseSuccessful")]
		void LogPurchaseSuccessful();

		[Export("logPurchaseSuccessful:")]
		void LogPurchaseSuccessful(string purchaseID);
	
		[Export("logPurchaseMainMenuShown")]
		void LogPurchaseMainMenuShown();

		[Export("logPurchasePayWallShown:")]
		void LogPurchasePayWallShown(string paywallID);

		[Export("logLevelStart:")]
		void LogLevelStart(string levelID);

		[Export("logLevelFinished:")]
		void LogLevelFinished(string levelID);

		[Export("logMiniGameStart:")]
		void LogMiniGameStart(string miniGameID);

		[Export("logMiniGameSkipped:")]
		void LogMiniGameSkipped(string miniGameID);

		[Export("logMiniGameFinished:")]
		void LogMiniGameFinished(string miniGameID);

		[Export("logAchievementEarned:")]
		void LogAchievementEarned(string achievementID);

		[Export("logTellAFriendTapped")]
		void LogTellAFriendTapped();

		[Export("logGameCompleted")]
		void LogGameCompleted();
	}

	[BaseType(typeof(NSObject), Name = "bfgSplash")]
	interface BfgSplash
	{
		[Static, Export("getNewsletterSent")]
		bool NewsletterSent { get; }

		[Static, Export("setNewsletterSent:")]
		void SetNewsletterSent(bool sent);

		[Static, Export("displayNewsletter:")]
		void DisplayNewsletter(UIViewController baseController);
	}

	[BaseType(typeof(NSObject), Name = "bfgManager")]
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
		bool NavigateToLinkShareURL(string url);

		[Static, Export("currentUIType")]
		int CurrentUIType { get; }

		[Static, Export("currentUITypeAsString")]
		string CurrentUITypeAsString { get; }

		[Static, Export("setParentViewController:")]
		void SetParentViewController(UIViewController parent);

		[Static, Export("getParentViewController")]
		UIViewController GetParentViewController();

		[Static, Export("getServerWithDomain:")]
		string GetServerWithDomain(string server);

		[Export("adsRunning")]
		bool AdsRunning { get; }

		[Export("startAds:")]
		bool StartAds(AdsOrigin origin);

		[Export("stopAds")]
		bool StopAds();

		[Export("pauseAds")]
		void PauseAds();

		[Export("resumeAds")]
		void ResumeAds();

		[Export("startBranding")]
		bool StartBranding();

		[Export("stopBranding")]
		void StopBranding();

		[Export("showSupport")]
		void ShowSupport();

		[Export("showTerms")]
		void ShowTerms();

		[Export("showPrivacy")]
		void ShowPrivacy();
	}
}

