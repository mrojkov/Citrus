using System;

namespace BigFish_iOS_SDK
{
	public static class BfgConsts
	{
		public const string NotificationPurchaseFailed = "NOTIFICATION_PURCHASE_FAILED";
		public const string NotificationPurchaseSucceeded = "NOTIFICATION_PURCHASE_SUCCEEDED";
		public const string NotificationRestoreFailed = "NOTIFICATION_RESTORE_FAILED";
		public const string NotificationRestoreSucceeded = "NOTIFICATION_RESTORE_SUCCEEDED";
		public const string NotificationPurchaseProductionInformation = "NOTIFICATION_PURCHASE_PRODUCTINFORMATION";
        public const string NotificationPlacementContentStartPurchase = "BFG_NOTIFICATION_PLACEMENT_CONTENT_START_PURCHASE";
		public const string BrandingNotificationCompleted = "BFGBRANDING_NOTIFICATION_COMPLETED";
		public const string SplashNotificationCompleted = "BFGSPLASH_NOTIFICATION_COMPLETED";
		public const string PromoDashboardNotificationMoreGamesClosed = "BFGPROMODASHBOARD_NOTIFICATION_MOREGAMES_CLOSED";
	}

	public enum AdsOrigin
	{
		Default = 0,
		Top = 1,
		Bottom = 2
	}
}

