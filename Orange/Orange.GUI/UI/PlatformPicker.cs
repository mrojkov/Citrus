using System.Linq;
using Lime;

namespace Orange
{
	public class PlatformPicker : ThemedDropDownList
	{
		public PlatformPicker()
		{
			Changed += args => SelectedTarget = (Target) args.Value;
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
			SelectedTarget = (Target) Items.First().Value;
		}

		public Target SelectedTarget { get; private set; }
	}
}