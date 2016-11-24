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
			var selectedNodes = Document.Current.SelectedNodes().ToList();
			var rowInsertBefore = Document.Current.Rows.FirstOrDefault(
				row => !row.Selected && row.Index >= destination && row.Components.Has<Components.NodeRow>());
			var nodeInsertBefore = rowInsertBefore?.Components.Get<Components.NodeRow>().Node;
			foreach (var node in selectedNodes) {
				UnlinkNode.Perform(node);
			}
			var container = Document.Current.Container;
			var insertionIndex = nodeInsertBefore == null ? container.Nodes.Count : container.Nodes.IndexOf(nodeInsertBefore);
			foreach (var node in selectedNodes) {
				InsertNode.Perform(container, insertionIndex++, node);
				SelectNode.Perform(node);
			}
		}
	}
}