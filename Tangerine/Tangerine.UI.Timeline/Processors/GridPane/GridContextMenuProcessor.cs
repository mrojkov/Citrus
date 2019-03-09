using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class GridContextMenuProcessor : ITaskProvider
	{
		public IEnumerator<object> Task()
		{
			while (true) {
				if (!Document.Current.Animation.IsCompound && Timeline.Instance.Grid.RootWidget.Input.WasMouseReleased(1)) {
					bool enabled = Document.Current.Rows.Count > 0;
					if (enabled) {
						var cell = Timeline.Instance.Grid.CellUnderMouse();
						var row = Document.Current.Rows[cell.Y];
						var spans = row.Components.Get<Core.Components.GridSpanListComponent>()?.Spans;
						if (!row.Selected || !spans.Any(i => i.Contains(cell.X))) {
							Document.Current.History.DoTransaction(() => {
								Core.Operations.ClearRowSelection.Perform();
								Operations.ClearGridSelection.Perform();
								Core.Operations.SelectRow.Perform(row);
								Operations.SelectGridSpan.Perform(cell.Y, cell.X, cell.X + 1);
							});
						}
					}
					var menu = new Menu {
						TimelineCommands.CutKeyframes,
						TimelineCommands.CopyKeyframes,
						TimelineCommands.PasteKeyframes,
						Command.MenuSeparator,
						TimelineCommands.ReverseKeyframes,
						Command.MenuSeparator,
						GenericCommands.InsertTimelineColumn,
						GenericCommands.RemoveTimelineColumn,
						TimelineCommands.DeleteKeyframes,
						Command.MenuSeparator,
						TimelineCommands.NumericMove,
						TimelineCommands.NumericScale
					};
					foreach (var i in menu) {
						i.Enabled = enabled;
					}
					menu.Popup();
				}
				yield return null;
			}
		}
	}
}
