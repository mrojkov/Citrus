using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core.Components;

namespace Tangerine.Core
{
	public class RowsSynchronizer : SymmetricOperationProcessor
	{
		readonly List<Row> rows = new List<Row>();

		public override void Process(IOperation op)
		{
			var doc = Document.Current;
			rows.Clear();
			doc.RowTree = GetFolderRow(doc.Container.RootFolder());
			doc.RowTree.Rows.Clear();
			AddFolderContent(doc.RowTree);
			// Use temporary row list to avoid 'Collection was modified' exception during row batch processing.
			if (!rows.SequenceEqual(Document.Current.Rows)) {
				doc.Rows.Clear();
				doc.Rows.AddRange(rows);
			}
		}

		void AddFolderContent(Row parentRow)
		{
			var parentFolder = parentRow.Components.Get<FolderRow>()?.Folder;
			var bones = parentFolder.Items.OfType<Bone>().ToList();
			foreach (var i in parentFolder.Items) {
				var node = i as Node;
				var folder = i as Folder;
				var bone = i as Bone;
				if (bone != null) {
					if (bone.BaseIndex == 0) {
						AddBoneContent(bone, null, parentRow, bones);
					}
				} else if (node != null) {
					var nodeRow = AddNodeRow(parentRow, node);
					if (node.EditorState().PropertiesExpanded) {
						foreach (var animator in node.Animators) {
							AddAnimatorRow(nodeRow, node, animator);
						}
					}
				} else if (folder != null) {
					var folderRow = AddFolderRow(parentRow, folder);
					if (folder.Expanded) {
						AddFolderContent(folderRow);
					}
				}
			}
		}

		private void AddBoneContent(Bone bone, Bone parentBone, Row parentRow, List<Bone> bones)
		{
			var row = AddBoneRow(parentRow, bone, parentBone);
			if (bone.EditorState().PropertiesExpanded) {
				foreach (var animator in bone.Animators) {
					AddAnimatorRow(row, bone, animator);
				}
			}
			if (bone.EditorState().ChildrenExpanded) {
				AddBoneContent(row, bone, bones);
			}
		}

		private void AddBoneContent(Row parentRow, Bone parentBone, List<Bone> bones)
		{
			foreach (var bone in bones.Where(b => b.BaseIndex == parentBone.Index)) {
				AddBoneContent(bone, parentBone, parentRow, bones);
			}
		}

		Row AddAnimatorRow(Row parent, Node node, IAnimator animator)
		{
			if (animator.IsZombie) {
				return null;
			}
			var row = Document.Current.GetRowForObject(animator);
			if (!row.Components.Contains<PropertyRow>()) {
				row.Components.Add(new PropertyRow(node, animator));
			}
			AddRow(parent, row);
			return row;
		}

		Row AddNodeRow(Row parent, Node node)
		{
			var row = Document.Current.GetRowForObject(node);
			if (!row.Components.Contains<NodeRow>()) {
				row.Components.Add(new NodeRow(node));
			}
			AddRow(parent, row);
			return row;
		}

		Row AddBoneRow(Row parent, Bone bone, Bone parentBone)
		{
			var row = Document.Current.GetRowForObject(bone);
			if (!row.Components.Contains<NodeRow>()) {
				row.Components.Add(new NodeRow(bone));
				row.CanHaveChildren = true;
			}
			var boneRow = row.Components.Get<BoneRow>();
			if (boneRow == null) {
				row.Components.Add(boneRow = new BoneRow(bone));
			}
			boneRow.HaveChildren = bone.Parent?.AsWidget.Nodes
				.OfType<Bone>()
				.Any(b => b.BaseIndex == bone.Index) ?? false;
			AddRow(parent, row);
			return row;
		}

		Row AddFolderRow(Row parent, Folder folder)
		{
			var row = GetFolderRow(folder);
			AddRow(parent, row);
			return row;
		}

		Row GetFolderRow(Folder folder)
		{
			var row = Document.Current.GetRowForObject(folder);
			if (!row.Components.Contains<FolderRow>()) {
				row.Components.Add(new FolderRow(folder));
				row.CanHaveChildren = true;
			}
			return row;
		}

		void AddRow(Row parent, Row row)
		{
			row.Index = rows.Count;
			row.Parent = parent;
			row.Rows.Clear();
			rows.Add(row);
			parent.Rows.Add(row);
		}
	}
}
