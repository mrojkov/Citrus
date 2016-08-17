using System;
using System.Linq;
using Lime;
using ProtoBuf;
using System.Collections.Generic;

namespace Tangerine.UI
{
	public class UserPreferences
	{
		[YuzuMember]
		public UI.DockManager.State DockState = new UI.DockManager.State();

		[YuzuMember]
		public readonly List<string> RecentProjects = new List<string>();

		[YuzuMember]
		public bool AutoKeyframes;

		[YuzuMember]
		public bool AnimationMode;

		public static UserPreferences Instance { get; private set; }

		public static void Initialize()
		{
			if (Instance != null) {
				throw new InvalidOperationException();
			}
			Instance = new UserPreferences();
		}

		private UserPreferences()
		{
			if (System.IO.File.Exists(GetPath())) {
				try {
					Serialization.ReadObjectFromFile<UserPreferences>(GetPath(), this);
				} catch (System.Exception e) {
					Debug.Write($"Failed to load the user preferences: {e}");
				}
			}
		}

		public void Save()
		{
			Serialization.WriteObjectToFile(GetPath(), this);
		}

		public static string GetPath()
		{
			return System.IO.Path.Combine(Lime.Environment.GetDataDirectory("Tangerine"), "UserPreferences");
		}
	}
}
