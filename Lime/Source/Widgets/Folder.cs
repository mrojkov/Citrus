using System;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	public interface IFolderContext
	{
		List<Folder.Descriptor> Folders { get; set; }
		NodeList Nodes { get; }
	}

	public interface IFolderItem { }

	public class Folder : IFolderItem
	{
		public string Id { get; set; }
		public bool Expanded { get; set; }
		public readonly List<IFolderItem> Items = new List<IFolderItem>();

		public static Folder BuildTree(IFolderContext context)
		{
			var root = new Folder { Expanded = true };
			if (context.Folders == null || context.Folders.Count == 0) {
				root.Items.AddRange(context.Nodes);
			} else {
				int descIndex = 0;
				int nodeIndex = 0;
				BuildTree(context, root, int.MaxValue, ref descIndex, ref nodeIndex);
			}
			return root;
		}

		public FolderItemLocation Find(IFolderItem item)
		{
			int j = 0;
			foreach (var i in Items) {
				if (i == item) {
					return new FolderItemLocation(this, j);
				}
				if (i is Folder) {
					var l = (i as Folder).Find(item);
					if (l.Folder != null) {
						return l;
					}
				}
				j++;
			}
			return new FolderItemLocation(null, -1);
		}

		public bool Contains(IFolderItem item)
		{
			return Find(item).Folder != null;
		}

		static void BuildTree(IFolderContext context, Folder folder, int maxItems, ref int descIndex, ref int nodeIndex)
		{
			var nodes = context.Nodes;
			var descriptors = context.Folders;
			while (folder.Items.Count < maxItems) {
				var d = descIndex < descriptors.Count ? descriptors[descIndex] : null;
				if (nodeIndex < nodes.Count && (d == null || nodeIndex < d.Index)) {
					folder.Items.Add(nodes[nodeIndex++]);
				} else if (d != null) {
					descIndex++;
					var subFolder = new Folder { Id = d.Id, Expanded = d.Expanded };
					folder.Items.Add(subFolder);
					BuildTree(context, subFolder, d.ItemCount, ref descIndex, ref nodeIndex);
				} else {
					break;
				}
			}
		}

		public void SyncDescriptorsAndNodes(IFolderContext context)
		{
			context.Nodes.Clear();
			context.Folders = new List<Descriptor>();
			SyncDescriptorsAndNodes(context, this);
			if (context.Folders.Count == 0) {
				context.Folders = null;
			}
		}

		private static void SyncDescriptorsAndNodes(IFolderContext context, Folder folder)
		{
			foreach (var i in folder.Items) {
				if (i is Node) {
					context.Nodes.Add((Node)i);
				} else if (i is Folder) {
					var subFolder = (Folder)i;
					context.Folders.Add(new Descriptor {
						Id = subFolder.Id,
						Expanded = subFolder.Expanded,
						Index = context.Nodes.Count,
						ItemCount = subFolder.Items.Count
					});
					SyncDescriptorsAndNodes(context, subFolder);
				}
			}
		}

		[YuzuDontGenerateDeserializer]
		public class Descriptor
		{
			[YuzuMember]
			public string Id { get; set; }

			[YuzuMember]
			public bool Expanded { get; set; }

			[YuzuMember]
			public int Index { get; set; }

			[YuzuMember]
			public int ItemCount { get; set; }
		}
	}

	public struct FolderItemLocation
	{
		public Folder Folder;
		public int Index;

		public FolderItemLocation(Folder parent, int index)
		{
			Folder = parent;
			Index = index;
		}

		public static FolderItemLocation operator + (FolderItemLocation location, int indexDelta)
		{
			location.Index += indexDelta;
			return location;
		}
	}
}
