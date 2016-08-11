using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Operations
{
	static class Copy
	{
		public static void Perform()
		{
			Timeline.Clipboard.Nodes.Clear();
			Timeline.Clipboard.Nodes.AddRange(Document.Current.SelectedNodes.Select(i => {
				var clone = i.Clone();
				clone.UserData = null;
				return clone;
			}));
		}
	}

	static class Cut
	{
		public static void Perform()
		{
			Copy.Perform();
			Delete.Perform();
		}
	}

	static class Paste
	{
		public static void Perform()
		{
			var nodeInsertBefore = Document.Current.SelectedNodes.FirstOrDefault();
			var insertionIndex = nodeInsertBefore != null ? Document.Current.Container.Nodes.IndexOf(nodeInsertBefore) : 0;
			if (Timeline.Clipboard.Nodes.Count > 0) {
				Operations.ClearRowSelection.Perform();
				foreach (var node in Timeline.Clipboard.Nodes) {
					var clone = node.Clone();
					Core.Operations.InsertNode.Perform(Document.Current.Container, insertionIndex++, clone);
					var row = Timeline.Instance.GetCachedRow(clone.EditorState().Uid);
					Operations.SelectRow.Perform(row);
				}
			}
		}
	}

	static class Delete
	{
		public static void Perform()
		{
			foreach (var i in Document.Current.SelectedNodes.ToList()) {
				Core.Operations.UnlinkNode.Perform(i);
			}
		}
	}
}