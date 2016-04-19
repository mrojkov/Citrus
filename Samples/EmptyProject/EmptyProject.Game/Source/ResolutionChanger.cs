using System.Collections.Generic;
using Lime;
using System;
using System.IO;
using ProtoBuf;
using System.ComponentModel;
using System.Reflection;
using System.Linq;

namespace EmptyProject
{
	public class ResolutionChanger
	{
#if !iOS && !ANDROID
		private static readonly Size[] resolutionPixelValues = new Size[] {
			new Size(1024, 768), // iPad
			new Size(1366, 768), // Wide Screen
			new Size(480, 320),  // iPhone 3
			new Size(960, 640),  // iPhone 4
			new Size(1136, 640), // iPhone 5
			new Size(976, 768),  // half of Google Nexus 9 portrait (with Android Lollipop) minus status bar size (48px)
			new Size(1024, 720),  // half of Google Nexus 9 landscape (with Android Lollipop) minus status bar size (48px)
		};

		private static readonly string[] resolutionNames = new string[] {
			"iPad",
			"Wide Screen",
			"iPhone 3",
			"iPhone 4",
			"iPhone 5",
			"Google Nexus 9 portrait",
			"Google Nexus 9 landscape",
		};
#endif

		private int resolutionIdx = 0;
		private float worldWidth;
		private float worldHeight;
		private float worldScale;
		private Vector2 worldSize;
		private bool isPortraitOrientation;
		private bool isUpdatedOnFirstRender;

		public Size Current { get { return GetCurrentAdjustedResolution(); } }
		public bool IsPortraitOrientation {
			get { return isPortraitOrientation; }
			set { SetOrientation(isPortait: value); }
		}
		public float WorldScale { get { return worldScale; } }

		public ResolutionChanger(Vector2 worldSize)
		{
			this.worldSize = worldSize;
		}

		public void UpdateWorldSize()
		{
#if !iOS && !ANDROID
			Size windowSize = The.Application.WindowSize;
			isPortraitOrientation = windowSize.Height > windowSize.Width;
#else
			Size windowSize = Current;
#endif
			if (windowSize.Width > 0 && windowSize.Height > 0) {
				if (isPortraitOrientation) {
					worldWidth = worldSize.Y;
					worldHeight = worldSize.Y * ((float)windowSize.Height / (float)windowSize.Width);
					worldScale = (float)windowSize.Width / worldSize.Y;
				} else {
					worldWidth = worldSize.Y * ((float)windowSize.Width / (float)windowSize.Height);
					worldHeight = worldSize.Y;
					worldScale = (float)windowSize.Height / worldSize.Y;
				}
				The.World.Size = new Vector2(worldWidth, worldHeight);
			}
		}

		public void OnRenderFrame()
		{
			if (!isUpdatedOnFirstRender) {
				isUpdatedOnFirstRender = true;
				UpdateWorldSize();
			}

			Size windowSize = Current;
			Renderer.SetOrthogonalProjection(0, 0, worldWidth, worldHeight);
			Input.ScreenToWorldTransform =
				Matrix32.Scaling(
					worldWidth / windowSize.Width, 
					worldHeight / windowSize.Height
				);
		}

		private Size GetCurrentAdjustedResolution()
		{
			Size result = The.Application.WindowSize;
			if (isPortraitOrientation && result.Height < result.Width) {
				result = new Size(result.Height, result.Width);
			}
			if (!isPortraitOrientation && result.Height > result.Width) {
				result = new Size(result.Height, result.Width);
			}
			return result;
		}

		private void SetOrientation(bool isPortait)
		{
#if !iOS && !ANDROID
			if (!The.Application.FullScreen) {
				Size windowSize = The.Application.WindowSize;
				int max = Math.Max(windowSize.Width, windowSize.Height);
				int min = Math.Min(windowSize.Width, windowSize.Height);
				if (isPortait)
					The.Application.WindowSize = new Size(min, max);
				else
					The.Application.WindowSize = new Size(max, min);
			}
#else
			isPortraitOrientation = isPortait;
#endif
		}

#if !iOS && !ANDROID
		public void SetCurrentPresetResolution()
		{
			Size result = resolutionPixelValues[resolutionIdx];
			if (isPortraitOrientation) {
				result = new Size(result.Height, result.Width);
			}
			The.Application.WindowSize = result;
		}

		public void SwitchToNextPresetResolution()
		{
			if (!The.Application.FullScreen) {
				resolutionIdx = (resolutionIdx + 1) % resolutionPixelValues.Length;
				Logger.Write("Current resolution: " + resolutionNames[resolutionIdx]);
				SetCurrentPresetResolution();
			}
		}

		public void ToggleFullScreen()
		{
			The.Application.FullScreen = !The.Application.FullScreen;
			if (!The.Application.FullScreen) {
				SetCurrentPresetResolution();
			}
		}
#endif

	}
}