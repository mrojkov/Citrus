using System;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;

namespace Tangerine.Core.Operations
{
	public static class Copy
	{
		public static void CopyToClipboard()
		{
			Clipboard.Text = CopyToString();
		}

		public static string CopyToString()
		{
			using (var frame = CreateInMemoryCopy()) {
				var stream = new System.IO.MemoryStream();
				Serialization.WriteObject(Document.Current.Path, stream, frame, Serialization.Format.JSON);
				var text = System.Text.Encoding.UTF8.GetString(stream.ToArray());
				return text;
			}
		}

		static Frame CreateInMemoryCopy()
		{
			var frame = new Frame();
			foreach (var row in Document.Current.TopLevelSelectedRows()) {
				var node = row.Components.Get<NodeRow>()?.Node;
				if (node != null) {
					frame.RootFolder().Items.Add(Document.CreateCloneForSerialization(node));
				}
				var folder = row.Components.Get<FolderRow>()?.Folder;
				if (folder != null) {
					frame.RootFolder().Items.Add(CloneFolder(folder));
				}
			}
			frame.SyncFolderDescriptorsAndNodes();
			return frame;
		}

		static Folder CloneFolder(Folder folder)
		{
			var clone = new Folder { Id = folder.Id, Expanded = folder.Expanded };
			foreach (var i in folder.Items) {
				if (i is Folder) {
					clone.Items.Add(CloneFolder(i as Folder));
				} else if (i is Node) {
					clone.Items.Add(Document.CreateCloneForSerialization(i as Node));
				}
			}
			return clone;
		}
	}

	public static class Cut
	{
		public static void Perform()
		{
			Copy.CopyToClipboard();
			Delete.Perform();
		}
	}

	public static class Paste
	{
		public static void Perform()
		{
			var row = Document.Current.SelectedRows().FirstOrDefault();
			var loc = row == null ?
				new RowLocation(Document.Current.RowTree, 0) :
				new RowLocation(row.Parent, row.Parent.Rows.IndexOf(row));
			var data = Clipboard.Text;
			if (!string.IsNullOrEmpty(data)) {
				Perform(data, loc);
			}
		}

		public static bool CanPaste(string data, RowLocation location)
		{
			var parentFolder = location.ParentRow.Components.Get<FolderRow>()?.Folder;
			// We are support only paste into folders for now.
			return parentFolder != null;
		}

		public static bool Perform(string data, RowLocation location)
		{
			if (!CanPaste(data, location)) {
				return false;
			}
			Frame frame;
			try {
				var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(data));
				frame = Serialization.ReadObject<Frame>(Document.Current.Path, stream);
			} catch (System.Exception e) {
				Debug.Write(e);
				return false;
			}
			var parentFolder = location.ParentRow.Components.Get<FolderRow>()?.Folder;
			if (!parentFolder.Expanded) {
				SetProperty.Perform(parentFolder, nameof(Folder.Expanded), true);
			}
			var items = frame.RootFolder().Items.ToList();
			foreach (var n in items.OfType<Node>()) {
				Document.Current.Decorate(n);
			}
			frame.RootFolder().Items.Clear();
			frame.RootFolder().SyncDescriptorsAndNodes(frame);
			ClearRowSelection.Perform();
			foreach (var i in items) {
				InsertFolderItem.Perform(Document.Current.Container, new FolderItemLocation(parentFolder, location.Index), i);
				location.Index++;
				SelectRow.Perform(Document.Current.GetRowForObject(i));
			}
			return true;
		}
	}

	public static class Delete
	{
		public static void Perform()
		{
			foreach (var row in Document.Current.TopLevelSelectedRows().ToList()) {
				var item = (row.Components.Get<NodeRow>()?.Node as IFolderItem) ?? row.Components.Get<FolderRow>()?.Folder;
				if (item != null) {
					UnlinkFolderItem.Perform(Document.Current.Container, item);
				}
				var root = row.Components.Get<BoneRow>()?.Bone;
				if (root != null) {
					var bones = Document.Current.Container.Nodes.OfType<Bone>().ToList();
					foreach (var bone in BoneUtils.FindBoneDescendats(root, bones)) {
						UnlinkFolderItem.Perform(Document.Current.Container, bone);
					}
				}
			}
		}
	}
}