using System.Linq;
using Lime;

namespace Orange
{
	public class PlatformPicker : ThemedDropDownList
	{
		public PlatformPicker()
		{
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
		}

		public Target SelectedTarget => (Target)Items[Index].Value;
	}
}