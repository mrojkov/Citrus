using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.Core.Operations
{
	public static class DragRows
	{
		public static void Perform(int destination)
		{
			var nodesToDrag = Document.Current.SelectedNodes().ToList();
			var rowInsertBefore = Document.Current.Rows.FirstOrDefault(
				row => !Document.Current.SelectedRows.Contains(row) && row.Index >= destination && row.Components.Has<Core.Components.NodeRow>());
			var nodeInsertBefore = rowInsertBefore?.Components.Get<Core.Components.NodeRow>().Node;
			foreach (var node in nodesToDrag) {
				UnlinkNode.Perform(node);
			}
			var container = Document.Current.Container;
			var insertionIndex = nodeInsertBefore == null ? container.Nodes.Count : container.Nodes.IndexOf(nodeInsertBefore);
			foreach (var node in nodesToDrag) {
				InsertNode.Perform(container, insertionIndex++, node);
				SelectNode.Perform(node);
			}
		}
	}
}