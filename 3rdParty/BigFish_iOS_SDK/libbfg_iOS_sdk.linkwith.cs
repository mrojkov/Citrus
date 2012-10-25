using System;
using MonoTouch.ObjCRuntime;

[assembly: LinkWith("libbfg_iOS_sdk.a", 
   	LinkTarget.ArmV7 | LinkTarget.Simulator,
    ForceLoad = true,
	LinkerFlags = "-lz -licucore",
    Frameworks = "Security SystemConfiguration StoreKit UIKit OpenGLES MessageUI Foundation CoreGraphics",
	WeakFrameworks = "iAd GameKit")
]
