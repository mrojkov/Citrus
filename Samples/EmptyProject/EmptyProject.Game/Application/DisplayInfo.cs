using System;
using System.Collections.Generic;
using Lime;

namespace EmptyProject.Application
{
	public enum Orientation
	{
		Portrait,
		Landscape
	}

	public static partial class DisplayInfo
	{
		private const float ReferenceDpi = 330f;

		public static readonly List<Display> Displays = new List<Display>() {
			new Display("iPhone 4", 960, 640, 326),
			new Display("iPhone 5", 1136, 640, 326),
			new Display("iPhone 6", 1334, 750, 326),
			new Display("iPhone 6 Plus", 1980, 1080, 401),
			new Display("iPad", 1024, 768, 132),
			new Display("iPad 3", 2048, 1536, 264),
			new Display("Galaxy Nexus 4", 1280, 768, 318),
		};

		public static event Action BeforeOrientationOrResolutionChanged;
		public static event Action OrientationOrResolutionChanged;

		public static Orientation Orientation => IsLandscapeOrientation() ? Orientation.Landscape : Orientation.Portrait;

		public static float GetInterfaceScale()
		{
			var d = Display.DPI / ReferenceDpi;
			d = (d * 4.0f).Round() * 0.25f;
			if (IsTabletDisplay()) {
				d *= 2;
			}
			return d;
		}

		public static bool IsPhoneDisplay()
		{
			return !Display.IsTablet;
		}

		/// <summary>
		/// Check for iPhone 5 and more lengthy displays
		/// </summary>
		public static bool IsLengthyDisplay()
		{
			var res = GetLandscapeResolution();
			return res.X > 1.75f * res.Y;
		}

		public static bool IsTabletDisplay()
		{
			return Display.IsTablet;
		}

		public static Vector2 GetLandscapeResolution()
		{
			return IsLandscapeOrientation() ? GetResolution() : new Vector2(GetResolution().Y, GetResolution().X);
		}

		public static bool IsLandscapeOrientation()
		{
			return GetDeviceOrientation().IsLandscape();
		}

		public static bool IsPortraitOrientation()
		{
			return GetDeviceOrientation().IsPortrait();
		}

		public static bool IsIPad()
		{
			var res = GetLandscapeResolution();

			return Mathf.Abs(res.Y / res.X - 0.75f) < Mathf.ZeroTolerance;
		}

		public static float GetOnscreenKeyboardHeight()
		{
#if iOS
			return 0;//Application.Instance.OnscreenKeyboardHeight;
#else
			var pointsToPixelsMultiplier = 2;
			// Keyboard dimensions:
			// iPhone 4S (and earlier):
			//   Portrait Keyboard (English)	 320 x 216 pts
			//   Landscape Keyboard (English)	 480 x 162 pts
			// iPhone 5
			//   Portrait Keyboard (English)	 320 x 216 pts
			//   Landscape Keyboard (English)	 568 x 162 pts
			// iPad
			//   Portrait Keyboard (English)	 height: 264
			//   Landscape Keyboard (English)	 height: 352

			if (IsIPad()) {
				return IsLandscapeOrientation() ? 352 : 264;
			}
			else {
				return IsLandscapeOrientation() ? 162 * pointsToPixelsMultiplier : 216 * pointsToPixelsMultiplier;
			}
#endif
		}

		public static void HandleOrientationOrResolutionChange()
		{
			BeforeOrientationOrResolutionChanged?.Invoke();
			The.World.Size = CalcWorldSize();
#if !iOS && !ANDROID
			The.Window.ClientSize = GetResolution();
#endif
			OrientationOrResolutionChanged?.Invoke();

#if WIN
			The.Window.Center();
#endif
		}

		private static Vector2 CalcWorldSize()
		{
			var result = GetResolution();
			var windowSize = (Size)GetResolution();
			if (windowSize.Width > 0 && windowSize.Height > 0) {
				if (IsPortraitOrientation()) {
					result.X = Application.DefaultWorldSize.Y;
					result.Y = Application.DefaultWorldSize.Y * ((float)windowSize.Height / (float)windowSize.Width);
				}
				else {
					result.X = Application.DefaultWorldSize.Y * ((float)windowSize.Width / (float)windowSize.Height);
					result.Y = Application.DefaultWorldSize.Y;
				}
			}
			return result;
		}

	}
}