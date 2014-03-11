using System;
using MonoTouch.ObjCRuntime;

[assembly: LinkWith("libbfg_iOS_sdk.a", 
   	LinkTarget.ArmV7 | LinkTarget.Simulator,
    ForceLoad = true,
	LinkerFlags = "-lz -licucore",
    Frameworks = "CoreGraphics CoreText CoreTelephony Foundation GameKit iAd MessageUI OpenGLES Security StoreKit SystemConfiguration UIKit MobileCoreServices",
    WeakFrameworks = "AdSupport")
]
