using System.Linq;
using Lime;

namespace Orange
{
	public class PlatformPicker : DropDownList
	{
		private Target selected;

		public PlatformPicker()
		{
			Theme.Current.Apply(this, typeof(DropDownList));
			Changed += args => selected = (Target) args.Value;
			Reload();
		}

		public void Reload()
		{
			Index = -1;
			Items.Clear();
			foreach (var target in The.Workspace.Targets) {
				Items.Add(new Item(target.Name, target));
			}
			Index = 0;
			selected = (Target) Items.First().Value;
		}

		public Target SelectedTarget => selected;

		public TargetPlatform? SelectedPlatform => selected?.Platform;
	}
}