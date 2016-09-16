#if !iOS && !ANDROID
using Lime;

namespace EmptyProject.Application
{
	public static partial class DisplayInfo
	{
		public static Display Display
		{
			get { return The.AppData.SimulateDisplay; }
			set
			{
				The.AppData.SimulateDisplay = value;
				The.AppData.Save();
				HandleOrientationOrResolutionChange();
			}
		}

		public static Vector2 GetResolution()
		{
			var resolution = (Vector2) The.AppData.SimulateDisplay.Resolution;
			if (GetDeviceOrientation().IsPortrait()) {
				resolution = new Vector2(resolution.Y, resolution.X);
			}
			return resolution;
		}

		public static void SetResolution(Vector2 newResolution)
		{
			Display = new Display("Custom", newResolution.X.Round(), newResolution.Y.Round(), 400);
		}

		public static DeviceOrientation GetDeviceOrientation()
		{
			return The.AppData.SimulateDeviceOrientation;
		}

		private static void SetDeviceOrientation(DeviceOrientation value)
		{
			The.AppData.SimulateDeviceOrientation = value;
			The.AppData.Save();
			HandleOrientationOrResolutionChange();
		}

		public static void SetNextDeviceOrientation()
		{
			SetDeviceOrientation(GetDeviceOrientation().IsLandscape()
				? DeviceOrientation.Portrait
				: DeviceOrientation.LandscapeLeft);
		}

		public static void SetNextDisplay()
		{
			var index = Displays.IndexOf(The.AppData.SimulateDisplay);
			index = (index >= 0) ? (index + 1)%Displays.Count : 0;
			Display = Displays[index];
#if WIN
			The.Window.Title = $"{Application.ApplicationName} (Display: {Display.Name})";
#endif
		}
	}
}

#endif