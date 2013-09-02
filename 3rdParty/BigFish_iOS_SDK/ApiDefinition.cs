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

		[Static, Export("reset")]
		void Reset();

		[Static, Export("mainMenuRateApp")]
		void MainMenuRateApp();

		[Static, Export("immediateTrigger")]
		void ImmediateTrigger();
	}

	[BaseType(typeof(NSObject), Name = "bfgSettings")]
	interface BfgSettings
	{
		[Static, Export("set:value:")]
		void Set(string key, NSObject value);

		[Static, Export("plistPath")]
		string GetPlistPath();

		[Static, Export("get")]
		NSObject Get(string key);
	}

	[BaseType(typeof(NSObject), Name = "bfgPurchaseObject")]
	interface BfgPurchaseObject
	{
		[Export("productId")]
		string ProductId { get; }

		[Export("productInfo")]
		NSDictionary ProductInfo { get; }
	}

	[BaseType(typeof(NSObject), Name = "bfgPurchase")]
	interface BfgPurchase
	{
		[Static, Export("acquireProductInformationForProducts:")]
		void AcquireProductInformationForProducts(NSSet products);

		[Static, Export("canStartPurchase:")]
		bool CanStartPurchase(string productId);

		[Static, Export("finishPurchase:")]
		void FinishPurchase(string productId);

		[Static, Export("isPurchaseActive:")]
		void IsPurchaseActive(string productId);

		[Static, Export("productInformation:")]
		NSDictionary ProductInformation(string productId);

		[Static, Export("restorePurchases")]
		void RestorePurchases();

		[Static, Export("startPurchase:")]
		void StartPurchase(string productId);

		[Static, Export("startService")]
		bool StartService();
	}

	[BaseType(typeof(NSObject), Name = "bfgGameReporting")]
	interface BfgGameReporting
	{
		[Static, Export("logMainMenuShown")]
		void LogMainMenuShown();
	
		[Static, Export("logRateMainMenuCancelled")]
		void LogRateMainMenuCanceled();
	
		[Static, Export("logOptionsShown")]
		void LogOptionsShown();
	
		[Static, Export("logPurchaseSuccessful")]
		void LogPurchaseSuccessful();

		[Static, Export("logPurchaseSuccessful:")]
		void LogPurchaseSuccessful(string purchaseID);
	
		[Static, Export("logPurchaseMainMenuShown")]
		void LogPurchaseMainMenuShown();

		[Static, Export("logPurchasePayWallShown:")]
		void LogPurchasePayWallShown(string paywallID);

		[Static, Export("logLevelStart:")]
		void LogLevelStart(string levelID);

		[Static, Export("logLevelFinished:")]
		void LogLevelFinished(string levelID);

		[Static, Export("logMiniGameStart:")]
		void LogMiniGameStart(string miniGameID);

		[Static, Export("logMiniGameSkipped:")]
		void LogMiniGameSkipped(string miniGameID);

		[Static, Export("logMiniGameFinished:")]
		void LogMiniGameFinished(string miniGameID);

		[Static, Export("logAchievementEarned:")]
		void LogAchievementEarned(string achievementID);

		[Static, Export("logTellAFriendTapped")]
		void LogTellAFriendTapped();

		[Static, Export("logGameCompleted")]
		void LogGameCompleted();

		[Static, Export("logCustomEvent:value:level:details1:details2:details3:additionalDetails:")]
		void LogCustomEvent(string name, int value, int level, string details1, string details2, string details3, NSDictionary additionalDetails);
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

	[BaseType(typeof(NSObject), Name = "bfgutils")]
	interface BfgUtils
	{
		[Static, Export("bfgUDID")]
		string BfgUDID { get; }

		[Static, Export("getMACAddress")]
		string GetMACAddress { get; }
	}
}

