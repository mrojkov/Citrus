using System;
using System.Collections.Generic;
using Lime;
using Yuzu;

namespace Tangerine.UI.FilesystemView
{
	public class UserPreferences : Component
	{
		[YuzuRequired]
		public bool ShowCookingRulesEditor = true;

		[YuzuRequired]
		public bool ShowSelectionPreview = true;
	}
}