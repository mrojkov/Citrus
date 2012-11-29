using System;
using MonoTouch.ObjCRuntime;

[assembly: LinkWith ("libLemonUniversal.a", LinkTarget.ArmV6 | LinkTarget.ArmV7 | LinkTarget.Simulator, ForceLoad = true)]
