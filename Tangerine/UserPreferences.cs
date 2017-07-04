using System;
using System.Linq;
using Lime;
using Yuzu;
using System.Collections.Generic;

namespace Tangerine
{
	public enum ColorThemeEnum
	{
		Light,
		Dark
	}

	// Don't use YuzuRequired or YuzuDefault here since it will trigger exception in UserPreferences classes c-tor
	public class UserPreferences
	{
		[YuzuRequired]
		public UI.DockManager.State DockState = new UI.DockManager.State();

		[YuzuRequired]
		public readonly List<string> RecentProjects = new List<string>();

		[YuzuRequired]
		public ColorThemeEnum Theme { get; set; }

		[YuzuRequired]
		public Vector2 DefaultSceneDimensions { get; set; } = new Vector2(1024, 768);

		[YuzuRequired]
		public UI.SceneView.UserPreferences SceneViewUserPreferences { get; private set; } = new UI.SceneView.UserPreferences(true);

		[YuzuRequired]
		public UI.Timeline.UserPreferences TimelineUserPreferences { get; private set; } = new UI.Timeline.UserPreferences(true);

		[YuzuRequired]
		public UI.FilesystemView.UserPreferences FilesystemViewPreferences { get; private set; } = new UI.FilesystemView.UserPreferences(true);

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
