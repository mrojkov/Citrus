using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;

namespace Tangerine.Core.Operations
{
	public static class Copy
	{
		public static void Perform()
		{
			using (var frame = new Frame()) {
				foreach (var row in Document.Current.TopLevelSelectedRows()) {
					var nr = row.Components.Get<NodeRow>();
					var fr = row.Components.Get<FolderRow>();
					// TODO: Handle PropertyRow
					if (nr != null) {
						frame.AddNode(Document.CreateCloneForSerialization(nr.Node));
					}
					if (fr != null) {
						foreach (var n in fr.GetNodes().ToList()) {
							frame.AddNode(Document.CreateCloneForSerialization(n));
						}
					}
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
			Frame frame = null;
			try {
				var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));
				frame = Serialization.ReadObject<Frame>(Document.Current.Path, stream);
			} catch (System.Exception e) {
				Debug.Write(e);
				return;
			}
			var nodeInsertBefore = Document.Current.TopLevelSelectedRows().
				Select(i => i.Components.Get<NodeRow>()?.Node ?? i.Components.Get<FolderRow>()?.Node).
				Where(i => i != null)?.FirstOrDefault();
			var insertionIndex = nodeInsertBefore != null ? nodeInsertBefore.CollectionIndex() : 0;
			ClearRowSelection.Perform();
			var nodes = frame.Nodes.ToList();
			FreezeRows.Perform();
			foreach (var node in nodes) {
				node.Unlink();
				InsertNode.Perform(Document.Current.Container, insertionIndex++, node);
			}
			UnfreezeRows.Perform();
			SelectNode.Perform(nodes.FirstOrDefault());
		}
	}

	public static class Delete
	{
		public static void Perform()
		{
			var t = Document.Current.SelectedRows().First().Index;
			foreach (var row in Document.Current.TopLevelSelectedRows().ToList()) {
				DeleteRow.Perform(row);
			}
			t = t.Clamp(0, Document.Current.Rows.Count - 1);
			if (t >= 0) {
				SelectRow.Perform(Document.Current.Rows[t]);
			}
		}
	}

	public class DeleteRow : Operation
	{
		public readonly Row Row;

		public override bool IsChangingDocument => true;

		public static void Perform(Row row)
		{
			Document.Current.History.Perform(new DeleteRow(row));
		}

		private DeleteRow(Row row)
		{
			Row = row;
		}

		public class Processor : OperationProcessor<DeleteRow>
		{
			protected override void InternalDo(DeleteRow op)
			{
				var nr = op.Row.Components.Get<NodeRow>();
				var fr = op.Row.Components.Get<FolderRow>();
				if (nr != null) {
					UnlinkNode.Perform(nr.Node);
				}
				if (fr != null) {
					FreezeRows.Perform();
					foreach (var n in fr.GetNodes().ToList()) {
						UnlinkNode.Perform(n);
					}
					UnfreezeRows.Perform();
				}
				// TODO: Handle PropertyRow
			}

			protected override void InternalRedo(DeleteRow op) { }
			protected override void InternalUndo(DeleteRow op) { }
		}
	}
}