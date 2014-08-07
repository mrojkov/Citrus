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

        [Static, Export("hasRatedApp")]
        bool HasRatedApp();

        [Static, Export("canShowMainMenuRateButton")]
        bool CanShowMainMenuRateButton();
		
		[Static, Export("userDidSignificantEvent:")]
		void UserDidSignificantEvent(bool canPromptForRating);
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

		[Export("quantity")]
		int Quantity { get; }

		[Export("receipt")]
		NSData Receipt { get; }

		[Export("sandbox")]
		bool Sandbox { get; }

		[Export("restore")]
		bool Restore { get; }

		[Export("success")]
		bool Success { get; }
	
		[Export("canceled")]
		bool Canceled { get; }
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

        [Static, Export("startService:")]
        bool StartService(out NSError error);
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

		[Static, Export("logGameCompleted")]
		void LogGameCompleted();

		[Static, Export("logCustomEvent:value:level:details1:details2:details3:additionalDetails:")]
		void LogCustomEvent(string name, int value, int level, string details1, string details2, string details3, NSDictionary additionalDetails);
		
		[Static, Export("logCustomPlacement:")]
		void LogCustomPlacement(string placementName);
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
        [Static, Export("initializeWithViewController:")]
		void InitializeWithViewController(UIViewController controller);

    	[Static, Export("showMoreGames")]
		void ShowMoreGames();

		[Static, Export("removeMoreGames")]
		void RemoveMoreGames();

		[Static, Export("isInitialized")]
		bool IsInitialized { get; }

		[Static, Export("isFirstTime")]
		bool IsFirstTime { get; }

    	[Static, Export("currentUIType")]
		int CurrentUIType { get; }

		[Static, Export("setParentViewController:")]
		void SetParentViewController(UIViewController parent);

        [Static, Export("adsRunning")]
		bool AdsRunning { get; }

        [Static, Export("startAds:")]
		bool StartAds(AdsOrigin origin);

        [Static, Export("stopAds")]
        void StopAds();

        [Static, Export("startBranding")]
		bool StartBranding();

        [Static, Export("stopBranding")]
		void StopBranding();

        [Static, Export("showSupport")]
		void ShowSupport();

        [Static, Export("showTerms")]
		void ShowTerms();

        [Static, Export("showPrivacy")]
		void ShowPrivacy();

        [Static, Export("launchSDKByURLScheme:")]
        bool LaunchSDKByURLScheme(string urlScheme);

		[Static, Export("userID")]
		NSNumber UserId { get; }
	}

	[BaseType(typeof(NSObject), Name = "bfgutils")]
	interface BfgUtils
	{
		[Static, Export("bfgUDID")]
		string BfgUDID { get; }

		[Static, Export("getMACAddress")]
		string GetMACAddress { get; }
	}
	
	[BaseType(typeof(NSObject), Name = "bfgPushNotificationManager")]
	interface BfgPushNotificationManager
	{
		[Static, Export("registerForPlayHavenPushNotifications")]
		void RegisterForPlayHavenPushNotifications();
		
		[Static, Export("handleRemoteNotificationWithLaunchOption:")]
        void HandleRemoteNotificationWithLaunchOption([NullAllowed] NSDictionary launchOptions);
		
		[Static, Export("registerDeviceToken:")]
		void RegisterDeviceToken(NSData deviceToken);
		
		[Static, Export("didFailToRegisterWithError:")]
		void DidFailToRegisterWithError(NSError error);
		
		[Static, Export("handlePush:")]
		void HandlePush(NSDictionary userInfo);
		
		[Static, Export("setIconBadgeNumber:")]
		void SetIconBadgeNumber(int newNumber);
		
		[Static, Export("pushRegistrationInProgress")]
		bool PushRegistrationInProgress();
	}
}

