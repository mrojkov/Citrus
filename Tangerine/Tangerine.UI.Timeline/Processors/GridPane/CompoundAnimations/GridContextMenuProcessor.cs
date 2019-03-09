using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.CompoundAnimations
{
	public class GridContextMenuProcessor : ITaskProvider
	{
		public IEnumerator<object> Task()
		{
			while (true) {
				if (Document.Current.Animation.IsCompound && Timeline.Instance.Grid.RootWidget.Input.WasMouseReleased(1)) {
					Timeline.Instance.Grid.CellUnderMouseOnContextMenuPopup = Timeline.Instance.Grid.CellUnderMouse(clampRow: false);
					var menu = new Menu {
						TimelineCommands.AddAnimationClip,
						TimelineCommands.SplitAnimationClip,
					};
					menu.Popup();
				}
				yield return null;
			}
		}
	}
}
