using System;
using System.Linq;
using Lime;
using Yuzu;
using System.Collections.Generic;

namespace Tangerine.UI
{
	public enum ColorThemeEnum
	{
		Light,
		Dark
	}

	public class UserPreferences
	{
		[YuzuMember]
		public UI.DockManager.State DockState = new UI.DockManager.State();

		[YuzuMember]
		public readonly List<string> RecentProjects = new List<string>();

		[YuzuRequired]
		public bool AutoKeyframes { get; set; }

		[YuzuRequired]
		public bool AnimationMode { get; set; }

		[YuzuRequired]
		public ColorThemeEnum Theme { get; set; }

		[YuzuRequired]
		public Vector2 DefaultSceneDimensions { get; set; } = new Vector2(1024, 768);

		[YuzuOptional]
		public bool ShowOverlays { get; set; }

		[YuzuOptional]
		public float TimelineColWidth { get; set; } = 15;

		[YuzuOptional]
		public Dictionary<string, List<float>> Splitters { get; } = new Dictionary<string, List<float>>();

		public static UserPreferences Instance { get; private set; }

		public static void Initialize()
		{
			if (Instance != null) {
				throw new InvalidOperationException();
			}
			Instance = new UserPreferences();
			Instance.Load();
		}

		public void Load()
		{
			var path = GetPath();
			if (System.IO.File.Exists(path)) {
				try {
					Serialization.ReadObjectFromFile<UserPreferences>(path, this);
				} catch (System.Exception e) {
					Debug.Write($"Failed to load the user preferences ({path}): {e}");
				}
			}
		}

		public void Save()
		{
			Serialization.WriteObjectToFile(GetPath(), this, Serialization.Format.JSON);
		}

		public static string GetPath()
		{
			return System.IO.Path.Combine(Lime.Environment.GetDataDirectory("Tangerine"), "UserPreferences");
		}
	}
}
