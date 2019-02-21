using System;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Orange;

namespace Tangerine.Core.Operations
{
	public static class Copy
	{
		public static void CopyToClipboard()
		{
			if (Document.Current == null) {
				return;
			}
			Clipboard.Text = CopyToString();
		}

		public static string CopyToString()
		{
			var stream = new System.IO.MemoryStream();
			if (Document.Current.Animation.IsCompound) {
				CopyAnimationTracks(stream);
			} else {
				CopyNodes(stream);
			}
			var text = System.Text.Encoding.UTF8.GetString(stream.ToArray());
			return text;
		}

		private static void CopyAnimationTracks(System.IO.MemoryStream stream)
		{
			var animation = new Animation { IsCompound = true };
			foreach (var row in Document.Current.TopLevelSelectedRows()) {
				if (row.Components.Get<AnimationTrackRow>()?.Track is AnimationTrack track) {
					animation.Tracks.Add(track.Clone());
				}
			}
			TangerineYuzu.Instance.Value.WriteObject(Document.Current.Path, stream, animation, Serialization.Format.JSON);
		}

		private static void CopyNodes(System.IO.MemoryStream stream)
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
				var animator = row.Components.Get<PropertyRow>()?.Animator.Clone();
				if (animator != null) {
					frame.Animators.Add(animator);
				}
			}
			frame.SyncFolderDescriptorsAndNodes();
			TangerineYuzu.Instance.Value.WriteObject(Document.Current.Path, stream, frame, Serialization.Format.JSON);
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
				if (Document.Current.Animation.IsCompound) {
					PasteAnimationTracks(data, loc);
				} else {
					PasteNodes(data, loc, pasteAtMouse);
				}
			}
			foreach (var node in Document.Current.SelectedNodes()) {
				node.LoadExternalScenes();
			}
		}

		private static void PasteAnimationTracks(string data, RowLocation loc)
		{
			try {
				var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(data));
				var animation = TangerineYuzu.Instance.Value.ReadObject<Animation>(string.Empty, stream);
				ClearRowSelection.Perform();
				var tracks = animation.Tracks.ToList();
				animation.Tracks.Clear();
				int i = Document.Current.Animation.Tracks.Count == 0 ? 0 : loc.Index + 1;
				foreach (var t in tracks) {
					InsertIntoList<AnimationTrackList, AnimationTrack>.Perform(Document.Current.Animation.Tracks, i++, t);
					SelectRow.Perform(Document.Current.GetRowForObject(t));
				}
			} catch (System.Exception e) {
				Debug.Write(e);
				return;
			}
		}

		public static bool PasteNodes(string data, RowLocation location, bool pasteAtMouse = false)
		{
			bool CanPaste()
			{
				// We are support only paste into folders for now.
				return location.ParentRow.Components.Contains<FolderRow>() ||
					   location.ParentRow.Components.Contains<BoneRow>();
			}

			if (!CanPaste()) {
				return false;
			}
			Frame frame;
			try {
				var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(data));
				frame = TangerineYuzu.Instance.Value.ReadObject<Frame>(Document.Current.Path, stream);
			} catch (System.Exception e) {
				Debug.Write(e);
				try {
					var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(data));
					frame = (Frame)new HotSceneImporter(isTangerine: true).Import(stream, new Frame(), null);
				} catch (System.Exception e2) {
					Debug.Write(e2);
					return false;
				}
			}
			var animators = frame.Animators;
			var items = frame.RootFolder().Items.Where(item => NodeCompositionValidator.IsCopyPasteAllowed(item.GetType())).ToList();
			if (items.Count == 0) {
				if (animators.Count != 0) {
					foreach (var row in Document.Current.TopLevelSelectedRows().ToList()) {
						if (!(row.Components.Get<NodeRow>()?.Node is IAnimationHost animable)) {
							continue;
						}
						Document.Current.History.DoTransaction(() => {
							foreach (var animator in animators) {
								if (animable.GetType().GetProperty(animator.TargetPropertyPath) == null) {
									continue;
								}
								foreach (var keyframe in animator.Keys) {
									SetKeyframe.Perform(animable, animator.TargetPropertyPath, animator.AnimationId, keyframe);
								}
							}
						});
					}
				}
				return true;
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
			var doc = Document.Current;
			foreach (var row in doc.TopLevelSelectedRows().ToList()) {
				if (!row.IsCopyPasteAllowed()) {
					continue;
				}
				if (row.Components.Get<PropertyRow>()?.Animator is IAnimator animator) {
					doc.History.DoTransaction(() => {
						foreach (var keyframe in animator.Keys.ToList()) {
							RemoveKeyframe.Perform(animator, keyframe.Frame);
						}
					});
				} else if (row.Components.Get<AnimationTrackRow>()?.Track is AnimationTrack track) {
					doc.History.DoTransaction(() => {
						RemoveFromList<AnimationTrackList, AnimationTrack>.Perform(doc.Animation.Tracks, track);
					});
				} else if (row.Components.Get<BoneRow>()?.Bone is Bone currentBone) {
					var bones = doc.Container.Nodes.OfType<Bone>().ToList();
					var dependentBones = BoneUtils.FindBoneDescendats(currentBone, bones).ToList();
					dependentBones.Insert(0, currentBone);
					UntieWidgetsFromBones.Perform(dependentBones, doc.Container.Nodes.OfType<Widget>());
					foreach (var bone in dependentBones) {
						UnlinkFolderItem.Perform(doc.Container, bone);
						doc.Container.AsWidget.BoneArray[bone.Index] = default;
					}
				} else if (Row.GetFolderItem(row) is IFolderItem item) {
					UnlinkFolderItem.Perform(doc.Container, item);
				}
			}
		}
	}
}
