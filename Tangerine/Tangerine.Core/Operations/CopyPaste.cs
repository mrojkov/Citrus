using System;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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
				if (!row.IsCopyPasteAllowed()) {
					continue;
				}
				var bone = row.Components.Get<BoneRow>()?.Bone;
				if (bone != null) {
					var c = (Bone)Document.CreateCloneForSerialization(bone);
					c.BaseIndex = 0;
					frame.RootFolder().Items.Add(c);
					if (!bone.EditorState().ChildrenExpanded) {
						var children = BoneUtils.FindBoneDescendats(bone, Document.Current.Container.Nodes.OfType<Bone>());
						foreach (var b in children) {
							c = (Bone)Document.CreateCloneForSerialization(b);
							frame.RootFolder().Items.Add(c);
						}
					}
					continue;
				}
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
		public static void Perform(bool pasteAtMouse = false)
		{
			var row = Document.Current.SelectedRows().LastOrDefault();
			var loc = row == null ?
				new RowLocation(Document.Current.RowTree, 0) :
				new RowLocation(row.Parent, row.Parent.Rows.IndexOf(row));
			var data = Clipboard.Text;
			if (!string.IsNullOrEmpty(data)) {
				Perform(data, loc, pasteAtMouse);
			}

			foreach (var node in Document.Current.SelectedNodes()) {
				Document.Current.RefreshExternalScenes(node);
			}
		}

		public static bool CanPaste(string data, RowLocation location)
		{
			// We are support only paste into folders for now.
			return location.ParentRow.Components.Contains<FolderRow>() ||
				   location.ParentRow.Components.Contains<BoneRow>();
		}

		public static bool Perform(string data, RowLocation location, bool pasteAtMouse = false)
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
			FolderItemLocation folderLocation;
			if (location.ParentRow.Rows.Count > 0) {
				folderLocation = Row.GetFolderItemLocation(location.ParentRow.Rows[location.Index]);
				folderLocation.Index++;
			} else {
				folderLocation = new FolderItemLocation { Index = 0, Folder = location.ParentRow.Components.Get<FolderRow>().Folder };
			}
			if (!folderLocation.Folder.Expanded) {
				SetProperty.Perform(folderLocation.Folder, nameof(Folder.Expanded), true);
			}
			var items = frame.RootFolder().Items.Where(item => NodeCompositionValidator.IsCopyPasteAllowed(item.GetType())).ToList();
			if (items.Count == 0) {
				return true;
			}
			var mousePosition = Document.Current.Container.AsWidget?.LocalMousePosition();
			var shift = mousePosition - items.OfType<Widget>().FirstOrDefault()?.Position;
			foreach (var n in items.OfType<Node>()) {
				Document.Current.Decorate(n);
			}
			if (shift.HasValue && pasteAtMouse) {
				foreach (var w in items.OfType<Widget>()) {
					w.Position += shift.Value;
				}
			}
			frame.RootFolder().Items.Clear();
			frame.RootFolder().SyncDescriptorsAndNodes(frame);
			ClearRowSelection.Perform();
			while (items.Count > 0) {
				var item = items.First();
				var bone = item as Bone;
				if (bone != null) {
					if (bone.BaseIndex != 0) {
						continue;
					}
					var newIndex = 1;
					var bones = Document.Current.Container.Nodes.OfType<Bone>();
					if (bones.Any()) {
						newIndex = bones.Max(b => b.Index) + 1;
					}
					var children = BoneUtils.FindBoneDescendats(bone, items.OfType<Bone>()).ToList();
					var map = new Dictionary<int, int>();
					map.Add(bone.Index, newIndex);
					bone.BaseIndex = location.ParentRow.Components.Get<BoneRow>()?.Bone.Index ?? 0;
					bone.Index = newIndex;
					InsertFolderItem.Perform(
						Document.Current.Container,
						folderLocation, bone);
					folderLocation.Index++;
					foreach (var b in children) {
						b.BaseIndex = map[b.BaseIndex];
						map.Add(b.Index, b.Index = ++newIndex);
						InsertFolderItem.Perform(
							Document.Current.Container,
							folderLocation, b);
						folderLocation.Index++;
						items.Remove(b);
					}
					Document.Current.Container.RootFolder().SyncDescriptorsAndNodes(Document.Current.Container);
					SortBonesInChain.Perform(bone);
					SelectRow.Perform(Document.Current.GetRowForObject(item));
				} else {
					if (!location.ParentRow.Components.Contains<BoneRow>()) {
						InsertFolderItem.Perform(
							Document.Current.Container,
							folderLocation, item);
						folderLocation.Index++;
						SelectRow.Perform(Document.Current.GetRowForObject(item));
					}
				}
				items.Remove(item);
			}
			return true;
		}
	}

	public static class Delete
	{
		public static void Perform()
		{
			foreach (var row in Document.Current.TopLevelSelectedRows().ToList()) {
				if (!row.IsCopyPasteAllowed()) {
					continue;
				}
				var item = Row.GetFolderItem(row);
				var currentBone = row.Components.Get<BoneRow>()?.Bone;
				if (currentBone != null) {
					var bones = Document.Current.Container.Nodes.OfType<Bone>().ToList();
					var dependentBones = BoneUtils.FindBoneDescendats(currentBone, bones).ToList();
					dependentBones.Insert(0, currentBone);
					UntieWidgetsFromBones.Perform(dependentBones, Document.Current.Container.Nodes.OfType<Widget>());
					foreach (var bone in dependentBones) {
						UnlinkFolderItem.Perform(Document.Current.Container, bone);
						Document.Current.Container.AsWidget.BoneArray[bone.Index] = default(BoneArray.Entry);
					}
				} else if (item != null) {
					UnlinkFolderItem.Perform(Document.Current.Container, item);
				}
			}
		}
	}
}
