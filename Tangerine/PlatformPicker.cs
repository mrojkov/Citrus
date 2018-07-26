using Lime;
using Orange;

namespace Tangerine
{
	public class PlatformPicker : ThemedDropDownList
	{
		public PlatformPicker()
		{
			Reload();
		}

		public void Reload()
		{
			var savedIndex = Index;
			Index = -1;
			Items.Clear();
			foreach (var target in The.Workspace.Targets) {
				Items.Add(new Item(target.Name, target));
			}
			if (savedIndex >= 0 && savedIndex < Items.Count) {
				Index = savedIndex;
			} else {
				Index = 0;
			}
		}

		public Target SelectedTarget => (Target)Items[Index].Value;
	}
}
