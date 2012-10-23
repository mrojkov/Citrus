using System;
using MonoTouch.ObjCRuntime;

[assembly: LinkWith ("libLemon.a", LinkTarget.ArmV6 | LinkTarget.ArmV7, ForceLoad = true)]
