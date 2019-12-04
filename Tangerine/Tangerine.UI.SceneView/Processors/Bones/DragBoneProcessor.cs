using System;
using Lime;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class DragBoneProcessor : ITaskProvider
	{
		SceneView sv => SceneView.Instance;

		public IEnumerator<object> Task()
		{
			while (true) {
				if (!SceneView.Instance.InputArea.IsMouseOverThisOrDescendant()) {
					yield return null;
					continue;
				}
				var bone = Document.Current.SelectedNodes().Editable().OfType<Bone>().FirstOrDefault();
				if (bone != null) {
					var entry = bone.Parent.AsWidget.BoneArray[bone.Index];
					var t = Document.Current.Container.AsWidget.LocalToWorldTransform;
					if (sv.HitTestControlPoint(t * entry.Joint, 20)) {
						Utils.ChangeCursorIfDefault(MouseCursor.Hand);
						if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
							yield return Drag(bone, entry);
						}
					} else if (sv.HitTestControlPoint(t * entry.Tip, 20)) {
						Utils.ChangeCursorIfDefault(MouseCursor.Hand);
						if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
							yield return DragTip(bone, entry);
						}
					}
				}
				yield return null;
			}
		}

		private IEnumerator<object> Drag(Bone bone, BoneArray.Entry entry)
		{
			using (Document.Current.History.BeginTransaction()) {
				var iniMousePos = sv.MousePosition;
				var transform = Document.Current.Container.AsWidget.LocalToWorldTransform.CalcInversed();
				var transformInversed = transform.CalcInversed();
				int index = 0;
				var dragDelta = Vector2.Zero;
				while (sv.Input.IsMousePressed()) {
					Document.Current.History.RollbackTransaction();

					var snapEnabled = sv.Input.IsKeyPressed(Key.Alt);
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					var items = Document.Current.Container.AsWidget.BoneArray.items;
					index = 0;
					SceneView.Instance.Components.GetOrAdd<CreateBoneHelper>().HitTip = default(Vector2);
					if (items != null && snapEnabled) {
						for (var i = 0; i < items.Length; i++) {
							if (sv.HitTestControlPoint(transformInversed * items[i].Tip)) {
								index = i;
								break;
							}
						}
						if (bone.Index != index) {
							SceneView.Instance.Components.GetOrAdd<CreateBoneHelper>().HitTip =
							index != 0 ? items[index].Tip : default(Vector2);
						}
					}
					var b = bone.Parent.AsWidget.BoneArray[bone.BaseIndex];
					dragDelta = sv.MousePosition * transform - iniMousePos * transform;
					var parentToLocalTransform = bone.CalcLocalToParentWidgetTransform().CalcInversed();
					parentToLocalTransform.T = Vector2.Zero;
					var position = parentToLocalTransform *
						(entry.Joint - b.Tip + (index != 0 && index != bone.Index && snapEnabled ? items[index].Tip - entry.Joint : dragDelta));
					Core.Operations.SetAnimableProperty.Perform(bone, nameof(Bone.Position), position, CoreUserPreferences.Instance.AutoKeyframes);

					bone.Parent.Update(0);
					yield return null;
				}
				if (index != bone.Index && sv.Input.IsKeyPressed(Key.Alt)) {
					Core.Operations.SetAnimableProperty.Perform(bone, nameof(Bone.Position), index == 0 ? entry.Joint + dragDelta : Vector2.Zero, CoreUserPreferences.Instance.AutoKeyframes);
					var parentEntry = bone.Parent.AsWidget.BoneArray[index];
					float parentAngle = (parentEntry.Tip - parentEntry.Joint).Atan2Deg;
					var boneEntry = bone.Parent.AsWidget.BoneArray[bone.Index];
					float boneAngle = (boneEntry.Tip - boneEntry.Joint).Atan2Deg;
					Core.Operations.SetAnimableProperty.Perform(bone, nameof(Bone.Rotation), index == 0 ? boneAngle : boneAngle - parentAngle, CoreUserPreferences.Instance.AutoKeyframes);
					Core.Operations.SetAnimableProperty.Perform(bone, nameof(Bone.BaseIndex), index, CoreUserPreferences.Instance.AutoKeyframes);
					Core.Operations.SortBonesInChain.Perform(bone);
				}
				SceneView.Instance.Components.Remove<CreateBoneHelper>();
				sv.Input.ConsumeKey(Key.Mouse0);
				Window.Current.Invalidate();
				Document.Current.History.CommitTransaction();
			}
		}

		private IEnumerator<object> DragTip(Bone bone, BoneArray.Entry entry)
		{
			using (Document.Current.History.BeginTransaction()) {
				var iniMousePos = sv.MousePosition;
				var transform = Document.Current.Container.AsWidget.LocalToWorldTransform.CalcInversed();

				var accumulativeRotationsHelpersByBones = new Dictionary<Bone, AccumulativeRotationHelper>();

				while (sv.Input.IsMousePressed()) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					if (sv.Input.IsKeyPressed(Key.Control)) {
						var parent = bone.Parent.AsWidget.BoneArray[bone.BaseIndex];
						var dir = (sv.MousePosition * transform -
							bone.Parent.AsWidget.BoneArray[bone.Index].Joint).Snap(Vector2.Zero);
						var angle = dir.Atan2Deg;
						if (bone.BaseIndex != 0) {
							var prentDir = parent.Tip - parent.Joint;
							angle = Vector2.AngleDeg(prentDir, dir);
						}
						if (!sv.Input.IsKeyPressed(Key.Alt)) {
							Core.Operations.SetAnimableProperty.Perform(bone, nameof(Bone.Rotation),
								GetRotationByBone(accumulativeRotationsHelpersByBones, bone, angle),
								CoreUserPreferences.Instance.AutoKeyframes
							);
						}
						Core.Operations.SetAnimableProperty.Perform(bone, nameof(Bone.Length), dir.Length, CoreUserPreferences.Instance.AutoKeyframes);
					} else {
						var dragDelta = sv.MousePosition * transform - iniMousePos * transform;
						var boneChain = IKSolver.SolveFor(bone, entry.Tip + dragDelta);
						foreach (Tuple<Bone, float> pair in boneChain) {
							Core.Operations.SetAnimableProperty.Perform(pair.Item1, nameof(Bone.Rotation),
								GetRotationByBone(accumulativeRotationsHelpersByBones, pair.Item1, pair.Item2),
								CoreUserPreferences.Instance.AutoKeyframes
							);
						}
					}
					bone.Parent.Update(0);
					yield return null;
				}
				sv.Input.ConsumeKey(Key.Mouse0);
				Window.Current.Invalidate();
				Document.Current.History.CommitTransaction();
			}
		}

		private static float GetRotationByBone(IDictionary<Bone, AccumulativeRotationHelper> accumulativeRotationsHelpersByBones, Bone bone, float rotation)
		{
			if (!accumulativeRotationsHelpersByBones.ContainsKey(bone)) {
				accumulativeRotationsHelpersByBones[bone] = new AccumulativeRotationHelper(bone.Rotation, rotation);
			}
			AccumulativeRotationHelper accumulativeRotationHelper = accumulativeRotationsHelpersByBones[bone];
			accumulativeRotationHelper.Rotate(rotation);
			return accumulativeRotationHelper.Rotation;
		}

	}

}
