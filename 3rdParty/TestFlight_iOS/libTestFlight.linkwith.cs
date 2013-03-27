using System;
using MonoTouch.ObjCRuntime;

[assembly: LinkWith ("libTestFlight.a", LinkTarget.ArmV7 | LinkTarget.ArmV6 | LinkTarget.Simulator, ForceLoad = true)]
