#if iOS || ANDROID
using System;
using System.Collections.Generic;
using Lime;

namespace EmptyProject.Application
{
	public static partial class DisplayInfo
	{
		public static readonly Display Display = InitializeDisplay();

		static Display InitializeDisplay()
		{
			var display = new Display("Unknown device",
				The.Window.ClientSize.X.Round(), The.Window.ClientSize.Y.Round(),
				(int)Lime.Application.ScreenDPI.X);
			return display;
		}

		public static DeviceOrientation GetDeviceOrientation()
		{
			return Lime.Application.CurrentDeviceOrientation;
		}

		public static Vector2 GetResolution()
		{
			return (Vector2)The.Window.ClientSize;
		}
	}
}
#endif