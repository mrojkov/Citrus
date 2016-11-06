using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.Core.Operations
{
	public static class Clipboard
	{
		public static List<Node> Nodes = new List<Node>();
	}

	public static class Copy
	{
		public static void Perform()
		{
			Clipboard.Nodes.Clear();
			Clipboard.Nodes.AddRange(Document.Current.SelectedNodes().Select(i => {
				var clone = i.Clone();
				clone.UserData = null;
				foreach (var n in clone.Descendants) {
					n.UserData = null;
				}
				return clone;
			}));
		}
	}

	public static class Cut
	{
		public static void Perform()
		{
			Copy.Perform();
			Delete.Perform();
		}
	}

	public static class Paste
	{
		public static void Perform()
		{
			var nodeInsertBefore = Document.Current.SelectedNodes().FirstOrDefault();
			var insertionIndex = nodeInsertBefore != null ? Document.Current.Container.Nodes.IndexOf(nodeInsertBefore) : 0;
			if (Clipboard.Nodes.Count > 0) {
				ClearRowSelection.Perform();
				foreach (var node in Clipboard.Nodes) {
					var clone = node.Clone();
					InsertNode.Perform(Document.Current.Container, insertionIndex++, clone);
					SelectNode.Perform(clone);
				}
			}
		}
	}

	public static class Delete
	{
		public static void Perform()
		{
			var nodes = Document.Current.SelectedNodes().ToList();
			if (nodes.Count == 0) {
				return;
			}
			var container = Document.Current.Container;
			var t = container.Nodes.IndexOf(nodes[0]);
			foreach (var i in nodes) {
				UnlinkNode.Perform(i);
			}
			t = t.Clamp(0, container.Nodes.Count - 1);
			if (t >= 0) {
				SelectNode.Perform(container.Nodes[t]);
			}
		}
	}
}