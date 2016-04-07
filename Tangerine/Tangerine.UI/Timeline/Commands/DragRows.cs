using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline.Commands
{
	public class DragRows : InteractiveCommand
	{
		Timeline timeline => Timeline.Instance;
		readonly int destination;

		public DragRows(int destination)
		{
			this.destination = destination;
		}

		public override void Do()
		{
			var nodesToDrag = timeline.SelectedRows.Select(i => i.Components.Get<NodeRow>()?.Node).Where(i => i != null).ToList();
			var rowInsertBefore = timeline.Rows.FirstOrDefault(
				row => !timeline.SelectedRows.Contains(row) && row.Index >= destination && row.Components.Has<NodeRow>());
			var nodeInsertBefore = rowInsertBefore?.Components.Get<NodeRow>().Node;
			foreach (var node in nodesToDrag) {
				Execute(new Core.Commands.UnlinkNode(node));
			}
			var container = timeline.Container;
			var insertionIndex = nodeInsertBefore == null ? container.Nodes.Count : container.Nodes.IndexOf(nodeInsertBefore);
			foreach (var node in nodesToDrag) {
				Execute(new Core.Commands.InsertNode(container, insertionIndex++, node));
			}
		}
	}
}