using System.Collections.Generic;
using Lime;

namespace Orange
{
	public class PlatformPicker : DropDownList
	{
		private List<Target> targets = new List<Target>();
		private Target selected;

		public PlatformPicker()
		{
			Theme.Current.Apply(this, typeof(DropDownList));
			Changed += i => selected = targets[i];
			Reload();
		}

		public void Reload()
		{
			Index = -1;
			Items.Clear();
			targets = new List<Target> {
				new Target {
#if WIN
					Platform = TargetPlatform.Win,
#elif MAC
					Platform = TargetPlatform.Mac,
#endif
					Name = "Desktop (PC, Mac, Linux)"
				},
				new Target {
					Platform = TargetPlatform.iOS,
					Name = "iPhone/iPad"
				},
				new Target {
					Platform = TargetPlatform.Android,
					Name = "Android"
				},
				new Target {
					Platform = TargetPlatform.Unity,
					Name = "Unity"
				},
			};

			if (The.Workspace.SubTargets != null) {
				foreach (var target in The.Workspace.SubTargets)
					targets.Add(new Target {
						Name = target.Name,
						SubTarget = target,
						Platform = target.Platform
					});
			}

			foreach (var platform in targets) {
				Items.Add(new Item(platform.Name, platform));
			}
			Index = 0;
			selected = targets[0];
		}

		public TargetPlatform? SelectedPlatform => selected?.Platform;

		public SubTarget SelectedSubTarget => selected?.SubTarget;

		private class Target
		{
			public TargetPlatform Platform { get; set; }
			public SubTarget SubTarget { get; set; }
			public string Name { get; set; }
		}
	}
}