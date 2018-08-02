using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.UI;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.Core.Operations;
using Node = Lime.Node;

namespace Tangerine
{
	public class GroupNodes : DocumentCommandHandler
	{
		private const string DefaultAnimationId = "<DefaultAnimationId>";

		public override void ExecuteTransaction()
		{
			var selectedNodes = Document.Current.SelectedNodes().Where(IsValidNode).ToList();
			Rectangle aabb;
			if (!UI.Utils.CalcAABB(selectedNodes, (Widget)Document.Current.Container, out aabb)) {
				return;
			}

			var container = Document.Current.Container;
			foreach (var row in Document.Current.SelectedRows()) {
				if (row.Components.Contains<BoneRow>()) {
					var boneRow = row.Components.Get<BoneRow>();
					if (!boneRow.ChildrenExpanded) {
						selectedNodes.AddRange(BoneUtils.FindBoneDescendats(boneRow.Bone, container.Nodes.OfType<Bone>()));
					}
				}
			}
			var selectedBones = selectedNodes.OfType<Bone>().ToList();

			var loc = container.RootFolder().Find(selectedNodes[0]);
			Frame group;
			try {
				group = (Frame)Core.Operations.CreateNode.Perform(container, loc, typeof(Frame));
			} catch (InvalidOperationException e) {
				AlertDialog.Show(e.Message);
				return;
			}
			group.Id = selectedNodes[0].Id + "Group";
			group.Pivot = Vector2.Half;
			group.Position = aabb.Center;
			group.Size = aabb.Size;
			var bonesExceptSelected = container.Nodes.Except(selectedNodes).OfType<Bone>().ToList();
			UntieWidgetsFromBones.Perform(bonesExceptSelected, selectedNodes.OfType<Widget>());
			UntieWidgetsFromBones.Perform(selectedBones, container.Nodes.Except(selectedNodes).OfType<Widget>());
			var nodeKeyframesDict = new Dictionary<Node, BoneAnimationData>();
			var localRoots = new List<Bone>();
			foreach (var bone in BoneUtils.SortBones(container.Nodes.OfType<Bone>())) {
				Bone localRoot;
				var delta = Vector2.Zero;
				var isSelectedBone = selectedBones.Contains(bone);
				if (isSelectedBone) {
					localRoot = BoneUtils.FindBoneRoot(bone, selectedNodes);
					delta = -aabb.A;
				} else {
					localRoot = BoneUtils.FindBoneRoot(bone, bonesExceptSelected);
				}
				if (!localRoots.Contains(localRoot)) {
					if (!isSelectedBone && localRoot.BaseIndex == 0) {
						localRoots.Add(localRoot);
						continue;
					}
					nodeKeyframesDict.Add(localRoot, EvaluateBoneAnimationUsingParent(localRoot, v => v + delta));
					localRoots.Add(localRoot);
				}
			}
			SetKeyframes(nodeKeyframesDict);
			foreach (var n in selectedNodes) {
				UnlinkFolderItem.Perform(container, n);
			}
			int i = 0;
			foreach (var node in selectedNodes) {
				InsertFolderItem.Perform(group, new FolderItemLocation(group.RootFolder(), i++), node);
				if (node is Widget) {
					TransformPropertyAndKeyframes<Vector2>(node, nameof(Widget.Position), v => v - aabb.A);
				}
				if (node is Bone) {
					TransformPropertyAndKeyframes<Vector2>(node, nameof(Bone.RefPosition), v => v - aabb.A);
				}
			}
			group.AnimationFrame = container.AnimationFrame;
			ClearRowSelection.Perform();
			SelectNode.Perform(group);
		}

		private static void SetKeyframes(Dictionary<Node, BoneAnimationData> keyframeDictionary)
		{
			foreach (var pair in keyframeDictionary) {
				if (pair.Value.NoParentKeyframes) {
					TransformPropertyAndKeyframes(pair.Key, nameof(Bone.Position), pair.Value.PositionTransformer);
				} else {
					SetProperty.Perform(pair.Key, nameof(Bone.Position), pair.Value.CurrentPosition);
					SetProperty.Perform(pair.Key, nameof(Bone.Rotation), pair.Value.CurrentRotation);
					foreach (var keyframe in pair.Value.PositionKeyframes) {
						SetKeyframe.Perform(pair.Key, nameof(Bone.Position), Document.Current.AnimationId, keyframe.Value);
					}
					foreach (var keyframe in pair.Value.RotationKeyframes) {
						SetKeyframe.Perform(pair.Key, nameof(Bone.Rotation), Document.Current.AnimationId, keyframe.Value);
					}
					SetAnimableProperty.Perform(pair.Key, nameof(Bone.BaseIndex), 0);
				}
			}
		}

		public static void TransformPropertyAndKeyframes<T>(Node node, string propertyId, Func<T, T> transformer)
		{
			var value = new Property<T>(node, propertyId).Value;
			SetProperty.Perform(node, propertyId, transformer(value));
			foreach (var animation in node.Animators) {
				if (animation.TargetProperty == propertyId) {
					foreach (var keyframe in animation.Keys.ToList()) {
						var newKeyframe = keyframe.Clone();
						newKeyframe.Value = transformer((T)newKeyframe.Value);
						SetKeyframe.Perform(node, animation.TargetProperty, animation.AnimationId, newKeyframe);
					}
				}
			}
		}

		private static BoneAnimationData EvaluateBoneAnimationUsingParent(Bone node, Func<Vector2, Vector2> positionTransformer)
		{
			var boneChain = new List<Bone>();
			var parentNode = node.Parent.AsWidget;
			var parentBone = node;
			while (parentBone != null) {
				parentBone = parentNode.Nodes.GetBone(parentBone.BaseIndex);
				if (parentBone != null) {
					boneChain.Insert(0, parentBone);
				}
			 };
			var data = new BoneAnimationData();
			var framesDict = new Dictionary<string, SortedSet<int>>();
			foreach (var bone in boneChain) {
				foreach (var a in bone.Animators) {
					if (a.TargetProperty == nameof(Bone.Position) ||
						a.TargetProperty == nameof(Bone.Length) ||
					    a.TargetProperty == nameof(Bone.Rotation)
					) {
						var id = a.AnimationId ?? DefaultAnimationId;
						if (!framesDict.ContainsKey(id)) {
							framesDict[id] = new SortedSet<int>();
						}
						foreach (var k in a.Keys.ToList()) {
							framesDict[id].Add(k.Frame);
						}
					}
				}
			}
			data.CurrentPosition = positionTransformer(GetBonePositionInSpaceOfParent(node));
			data.CurrentRotation = GetBoneRotationInSpaceOfParent(node);

			if (node.BaseIndex == 0 && (boneChain.Count == 0 || framesDict.Count == 0)) {
				data.NoParentKeyframes = true;
				data.PositionTransformer = positionTransformer;
				return data;
			}

			var curFrame = parentNode.AnimationFrame;
			boneChain.Add(node);
			foreach (var pair in framesDict) {
				foreach (var frame in pair.Value) {
					ApplyAnimationAtFrame(pair.Key, frame, boneChain);
					data.PositionKeyframes.Add(frame, new Keyframe <Vector2> {
						Frame = frame,
						Function = KeyFunction.Spline,
						Value = positionTransformer(GetBonePositionInSpaceOfParent(node))
					});
					data.RotationKeyframes.Add(frame, new Keyframe<float> {
						Frame = frame,
						Function = KeyFunction.Spline,
						Value = GetBoneRotationInSpaceOfParent(node)
					});
				}
			}

			foreach (var a in node.Animators) {
				foreach (var key in a.Keys) {
					ApplyAnimationAtFrame(a.AnimationId, key.Frame, boneChain);
					switch (a.TargetProperty) {
						case nameof(Bone.Position):
							if (!data.PositionKeyframes.ContainsKey(key.Frame)) {
								data.PositionKeyframes[key.Frame] = new Keyframe<Vector2>();
							}
							data.PositionKeyframes[key.Frame].Frame = key.Frame;
							data.PositionKeyframes[key.Frame].Function = key.Function;
							data.PositionKeyframes[key.Frame].Value = positionTransformer(GetBonePositionInSpaceOfParent(node));
							break;
						case nameof(Bone.Rotation):
							if (!data.RotationKeyframes.ContainsKey(key.Frame)) {
								data.RotationKeyframes[key.Frame] = new Keyframe<float>();
							}
							data.RotationKeyframes[key.Frame].Frame = key.Frame;
							data.RotationKeyframes[key.Frame].Function = key.Function;
							data.RotationKeyframes[key.Frame].Value = GetBoneRotationInSpaceOfParent(node);
							break;
					}
				}
			}
			ApplyAnimationAtFrame(null, curFrame, boneChain);
			return data;
		}

		private static float GetBoneRotationInSpaceOfParent(Bone bone)
		{
			var parentEntry = bone.Parent.AsWidget.BoneArray[bone.BaseIndex];
			return bone.Rotation + (parentEntry.Tip - parentEntry.Joint).Atan2Deg;
		}

		private static Vector2 GetBonePositionInSpaceOfParent(Bone node)
		{
			return node.Position * node.CalcLocalToParentWidgetTransform();
		}

		private static void ApplyAnimationAtFrame(string animationId, int frame, IEnumerable<Bone> bones)
		{
			var id = (animationId == DefaultAnimationId) ? null : animationId;
			foreach (var node in bones) {
				node.AnimationFrame = frame;
				node.Animators.Apply(node.AnimationTime, id);
				node.Update(0);
			}
		}

		public override bool GetEnabled() => Document.Current.SelectedNodes().Any(IsValidNode);

		public static bool IsValidNode(Node node) => (node is Widget) || (node is Bone) || (node is Audio) || (node is ImageCombiner);

		private class BoneAnimationData
		{
			public readonly Dictionary<int, Keyframe<Vector2>> PositionKeyframes = new Dictionary<int, Keyframe<Vector2>>();
			public readonly Dictionary<int, Keyframe<float>> RotationKeyframes = new Dictionary<int, Keyframe<float>>();
			public Vector2 CurrentPosition { get; set; }
			public float CurrentRotation { get; set; }
			public bool NoParentKeyframes { get; set; }
			public Func<Vector2, Vector2> PositionTransformer { get; set; }
		}
	}

	public class UngroupNodes : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			var groups = Document.Current?.SelectedNodes().OfType<Frame>().ToList();
			if (groups?.Count == 0) {
				return;
			}
			var container = (Widget)Document.Current.Container;
			var p = container.RootFolder().Find(groups[0]);
			ClearRowSelection.Perform();
			UntieWidgetsFromBones.Perform(Document.Current.Container.Nodes.OfType<Bone>(), groups);
			foreach (var group in groups) {
				UnlinkFolderItem.Perform(container, group);
			}

			foreach (var group in groups) {
				var flipXFactor = group.Scale.X < 0 ? -1 : 1;
				var flipYFactor = group.Scale.Y < 0 ? -1 : 1;
				var flipVector = Vector2.Right + Vector2.Down * flipXFactor * flipYFactor;
				var groupRootBones = new List<Bone>();
				var groupNodes = group.Nodes.ToList().Where(GroupNodes.IsValidNode).ToList();
				var localToParentTransform = group.CalcLocalToParentTransform();
				foreach (var node in groupNodes) {
					UnlinkFolderItem.Perform(group, node);
					InsertFolderItem.Perform(container, p, node);
					SelectNode.Perform(node);
					p.Index++;
					if (node is Widget) {
						GroupNodes.TransformPropertyAndKeyframes<Vector2>(node, nameof(Widget.Position), v => localToParentTransform * v);
						GroupNodes.TransformPropertyAndKeyframes<Vector2>(node, nameof(Widget.Scale), v => v * group.Scale);
						GroupNodes.TransformPropertyAndKeyframes<float>(node, nameof(Widget.Rotation),
							v => v * Mathf.Sign(group.Scale.X * group.Scale.Y) + group.Rotation);
						GroupNodes.TransformPropertyAndKeyframes<Color4>(node, nameof(Widget.Color), v => group.Color * v);
					} else if (node is Bone) {
						var root = BoneUtils.FindBoneRoot((Bone) node, groupNodes);
						if (!groupRootBones.Contains(root)) {
							GroupNodes.TransformPropertyAndKeyframes<Vector2>(node, nameof(Bone.Position), v => localToParentTransform * v);
							GroupNodes.TransformPropertyAndKeyframes<float>(node, nameof(Bone.Rotation),
								v => (Matrix32.Rotation(v * Mathf.DegToRad) * localToParentTransform).ToTransform2().Rotation);
							groupRootBones.Add(root);
						} else if (flipVector != Vector2.One) {
							GroupNodes.TransformPropertyAndKeyframes<Vector2>(node, nameof(Bone.Position), v => v * flipVector);
							GroupNodes.TransformPropertyAndKeyframes<float>(node, nameof(Bone.Rotation), v => -v);
						}
						GroupNodes.TransformPropertyAndKeyframes<Vector2>(node, nameof(Bone.RefPosition), v => localToParentTransform * v);
						GroupNodes.TransformPropertyAndKeyframes<float>(node, nameof(Bone.RefRotation),
							v => (Matrix32.Rotation(v * Mathf.DegToRad) * localToParentTransform).ToTransform2().Rotation);
					}
				}
			}
		}

		public override bool GetEnabled() => Core.Document.Current.SelectedNodes().Any(i => i is Frame);
	}

	public class InsertTimelineColumn : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			TimelineHorizontalShift.Perform(UI.Timeline.Timeline.Instance.CurrentColumn, 1);
		}
	}

	public class RemoveTimelineColumn : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			if (UI.Timeline.Timeline.Instance.CurrentColumn == 0) {
				TimelineHorizontalShift.Perform(UI.Timeline.Timeline.Instance.CurrentColumn, -1);
			} else {
				TimelineColumnRemove.Perform(UI.Timeline.Timeline.Instance.CurrentColumn);
			}
		}
	}

	public class GroupContentsToMorphableMeshes : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			//var nodes = Document.Current?.SelectedNodes().Editable().ToList();
			//var container = Document.Current.Container;
			//Core.Operations.ClearRowSelection.Perform();
			//foreach (var node in nodes) {
			//	var clone = node.Clone();
			//	var loc = container.RootFolder().Find(node);
			//	Core.Operations.UnlinkFolderItem.Perform(container, node);
			//	Core.Operations.InsertFolderItem.Perform(container, loc, clone);
			//	new MorphableMeshBuilder().BuildNodeContents(clone, MorphableMeshBuilder.Options.None);
			//	Core.Operations.SelectNode.Perform(clone);
			//}
		}
	}

	public class ExportScene : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			var nodes = Document.Current?.SelectedNodes().Editable().ToList();
			if (nodes.Count != 1) {
				AlertDialog.Show("Please, select a single node");
				return;
			}
			Export(nodes[0]);
		}

		public static void Export(Node node)
		{
			var dlg = new FileDialog {
				AllowedFileTypes = new string[] { Document.Current.GetFileExtension() },
				Mode = FileDialogMode.Save,
				InitialDirectory = Project.Current.GetSystemDirectory(Document.Current.Path),
			};
			if (dlg.RunModal()) {
				string assetPath;
				if (!Project.Current.TryGetAssetPath(dlg.FileName, out assetPath)) {
					AlertDialog.Show("Can't save the document outside the project directory");
				} else {
					try {
						Document.WriteNodeToFile(assetPath, DocumentFormat.Tan, node);
					} catch (System.Exception e) {
						AlertDialog.Show(e.Message);
					}
				}
			}
		}
	}

	public class UpsampleAnimationTwice : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			UpsampleNodeAnimation(Document.Current.RootNode);
		}

		private void UpsampleNodeAnimation(Node node)
		{
			foreach (var a in node.Animations) {
				foreach (var m in a.Markers) {
					SetProperty.Perform(m, "Frame", m.Frame * 2);
				}
			}
			foreach (var a in node.Animators) {
				foreach (var k in a.Keys) {
					SetProperty.Perform(k, "Frame", k.Frame * 2);
				}
			}
			foreach (var n in node.Nodes) {
				UpsampleNodeAnimation(n);
			}
		}
	}
}
