using System;
using System.Collections.Generic;
using Yuzu;

namespace Tangerine.UI.FilesystemView
{
	public class UserPreferences
	{
		[YuzuRequired]
		public bool ShowCookingRulesEditor = true;

		[YuzuRequired]
		public bool ShowSelectionPreview = true;

		[YuzuRequired]
		public Dictionary<string, List<float>> Splitters { get; } = new Dictionary<string, List<float>>();

		public static UserPreferences Instance;

		public UserPreferences(bool makeInstance)
		{
			if (makeInstance) {
				if (Instance != null) {
					throw new InvalidOperationException();
				}
				Instance = this;
			}
		}
	}
}