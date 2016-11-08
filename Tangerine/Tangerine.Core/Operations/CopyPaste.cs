using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.Core.Operations
{
	public static class Copy
	{
		public static void Perform()
		{
			using (var frame = new Frame()) {
				foreach (var node in Document.Current.SelectedNodes()) {
					var clone = Document.CreateCloneForSerialization(node);
					frame.AddNode(clone);
				}
				var stream = new System.IO.MemoryStream();
				Serialization.WriteObject(Document.Current.Path, stream, frame, Serialization.Format.JSON);
				var text = System.Text.Encoding.UTF8.GetString(stream.ToArray());
				Clipboard.Text = text;
			}
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
			var text = Clipboard.Text;
			if (string.IsNullOrEmpty(text)) {
				return;
			}
			try {
				var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));
				var frame = Serialization.ReadObject<Frame>(Document.Current.Path, stream);
				var nodeInsertBefore = Document.Current.SelectedNodes().FirstOrDefault();
				var insertionIndex = nodeInsertBefore != null ? Document.Current.Container.Nodes.IndexOf(nodeInsertBefore) : 0;
				ClearRowSelection.Perform();
				foreach (var node in frame.Nodes.ToList()) {
					node.Unlink();
					InsertNode.Perform(Document.Current.Container, insertionIndex++, node);
					SelectNode.Perform(node);
				}
			} catch (System.Exception) { }
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