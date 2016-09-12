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
			foreach (var i in Document.Current.SelectedNodes().ToList()) {
				UnlinkNode.Perform(i);
			}
		}
	}
}