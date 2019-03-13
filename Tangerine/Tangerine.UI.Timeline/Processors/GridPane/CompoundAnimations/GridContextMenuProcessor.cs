using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime;
using Tangerine.Core;
using Tangerine.UI.Timeline.Operations.CompoundAnimations;

namespace Tangerine.UI.Timeline.CompoundAnimations
{
	public class GridContextMenuProcessor : ITaskProvider
	{
		public IEnumerator<object> Task()
		{
			while (true) {
				if (Document.Current.Animation.IsCompound &&
					Timeline.Instance.Grid.RootWidget.Input.WasMouseReleased(1) &&
					Timeline.Instance.Grid.IsMouseOverRow())
				{
					var c = Timeline.Instance.Grid.CellUnderMouse();
					Document.Current.History.DoTransaction(() => {
						Operations.SetCurrentColumn.Perform(c.X);
						Core.Operations.ClearRowSelection.Perform();
						Core.Operations.SelectRow.Perform(Document.Current.Rows[c.Y]);
					});
					var menu = new Menu {
						new Command("Add", () => AddAnimationClip.Perform(c)) { Enabled = AddAnimationClip.IsEnabled() },
						new Command("Split", () => SplitAnimationClip.Perform(c)) { Enabled = SplitAnimationClip.IsEnabled(c) },
					};
					menu.Popup();
				}
				yield return null;
			}
		}
	}
}
