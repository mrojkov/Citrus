using System;
using System.Collections.Generic;

namespace Tangerine.Core
{
	public class ProjectPreferences
	{
		public static ProjectPreferences Instance => Project.Current.Preferences;

		private readonly List<ResolutionPreset> resolutions = new List<ResolutionPreset>();
		public IReadOnlyList<ResolutionPreset> Resolutions => resolutions;
		public ResolutionPreset DefaultResolution { get; private set; }

		public void Initialize()
		{
			InitializeResolutions();
		}

		private void InitializeResolutions()
		{
			try {
				var projectJson = Orange.The.Workspace.ProjectJson.AsDynamic;
				var resolutionMarkers = new Dictionary<string, ResolutionMarker>();
				foreach (var marker in projectJson.ResolutionSettings.Markers) {
					var name = (string)marker.Name;
					resolutionMarkers.Add(name, new ResolutionMarker((string)marker.PortraitMarker, (string)marker.LandscapeMarker));
				}
				foreach (var resolution in projectJson.ResolutionSettings.Resolutions) {
					var name = (string)resolution.Name;
					var width = (int)resolution.Width;
					var height = (int)resolution.Height;
					var usingResolutionMarkers = new List<ResolutionMarker>();
					foreach (string resolutionMarker in resolution.ResolutionMarkers) {
						usingResolutionMarkers.Add(resolutionMarkers[resolutionMarker]);
					}
					resolutions.Add(new ResolutionPreset(name, width, height, usingResolutionMarkers));
				}
				DefaultResolution = resolutions[0];
				Console.WriteLine("Resolution presets was successfully loaded.");
			} catch {
				InitializeDefaultResolutions();
			}
		}

		private void InitializeDefaultResolutions()
		{
			var resolutionMarkers = new[] { new ResolutionMarker("@Portrait", "@Landscape") };
			DefaultResolution = new ResolutionPreset("iPad", 1024, 768, resolutionMarkers);
			resolutions.Clear();
			resolutions.AddRange(new[] {
				DefaultResolution,
				new ResolutionPreset("Wide Screen", 1366, 768, resolutionMarkers),
				new ResolutionPreset("iPhone 4", 960, 640, resolutionMarkers),
				new ResolutionPreset("iPhone 5", 1136, 640, resolutionMarkers),
				new ResolutionPreset("iPhone 6, 7, 8", 1334, 750, resolutionMarkers),
				new ResolutionPreset("iPhone 6, 7, 8 Plus", 1920, 1080, resolutionMarkers),
				new ResolutionPreset("Google Nexus 9 portrait", 976, 768, resolutionMarkers),
				new ResolutionPreset("Google Nexus 9 landscape", 1024, 720, resolutionMarkers),
				new ResolutionPreset("Galaxy S8", 2960, 1440, resolutionMarkers),
				new ResolutionPreset("iPhone X", 2436, 1125, resolutionMarkers),
				new ResolutionPreset("Xperia Z4 Tablet", 2560, 1600, resolutionMarkers),
				new ResolutionPreset("LG G6", 2880, 1440, resolutionMarkers),
			});
			Console.WriteLine("Default resolution presets was loaded.");
		}
	}
}
