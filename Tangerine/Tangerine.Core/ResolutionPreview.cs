using System.Collections.Generic;
using System.Linq;
using Lime;

namespace Tangerine.Core
{
	public struct ResolutionPreview
	{
		public bool Enabled;
		public ResolutionPreset Preset;
		public bool IsPortrait;
	}

	public class ResolutionPreset
	{
		private readonly IReadOnlyList<ResolutionMarker> resolutionMarkers;

		public string Name { get; }
		public Vector2 LandscapeValue { get; }
		public Vector2 PortraitValue { get; }

		public ResolutionPreset(string name, float width, float height, IReadOnlyList<ResolutionMarker> resolutionMarkers)
		{
			Name = name;
			LandscapeValue = new Vector2(width, height);
			PortraitValue = new Vector2(height, width);
			this.resolutionMarkers = resolutionMarkers;
		}

		public List<string> GetAnimations(bool isPortrait)
		{
			return resolutionMarkers
				.Select(resolutionMarker => isPortrait ? resolutionMarker.Portrait : resolutionMarker.Landscape)
				.ToList();
		}

		public string GetDescription(bool isPortrait)
		{
			var resolution = isPortrait ? PortraitValue : LandscapeValue;
			return $"{Name} ({(int)resolution.X} x {(int)resolution.Y})";
		}
	}

	public class ResolutionMarker
	{
		public string Portrait { get; }
		public string Landscape { get; }

		public ResolutionMarker(string portrait, string landscape)
		{
			Portrait = portrait;
			Landscape = landscape;
		}
	}
}
