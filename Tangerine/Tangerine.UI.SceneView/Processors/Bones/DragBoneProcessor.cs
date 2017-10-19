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
				var bone = Document.Current.SelectedNodes().Editable().OfType<Bone>().FirstOrDefault();
				if (bone != null) {
					var entry = bone.Parent.AsWidget.BoneArray[bone.Index];
					var t = Document.Current.Container.AsWidget.CalcTransitionToSpaceOf(sv.Scene);
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
			Document.Current.History.BeginTransaction();

			try {
				var iniMousePos = sv.MousePosition;
				var transform = sv.Scene.CalcTransitionToSpaceOf(Document.Current.Container.AsWidget);
				var transformInversed = transform.CalcInversed();
				int index = 0;
				while (sv.Input.IsMousePressed()) {
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
					var dragDelta = sv.MousePosition * transform - iniMousePos * transform;
					var position = bone.WorldToLocalTransform *
						(entry.Joint - b.Tip + (index != 0 && index != bone.Index && snapEnabled ? items[index].Tip - entry.Joint : dragDelta));
					Core.Operations.SetAnimableProperty.Perform(bone, nameof(Bone.Position), position);

					bone.Parent.Update(0);
					yield return null;
				}
				if (index != 0 && index != bone.Index && sv.Input.IsKeyPressed(Key.Alt)) {
					Core.Operations.SetAnimableProperty.Perform(bone, nameof(Bone.Position), Vector2.Zero);
					Core.Operations.SetAnimableProperty.Perform(bone, nameof(Bone.BaseIndex), index);
					Core.Operations.SortBonesInChain.Perform(bone);
				}
			} finally {
				SceneView.Instance.Components.Remove<CreateBoneHelper>();
				sv.Input.ConsumeKey(Key.Mouse0);
				Document.Current.History.EndTransaction();
				Window.Current.Invalidate();
			}
		}

		private IEnumerator<object> DragTip(Bone bone, BoneArray.Entry entry)
		{
			Document.Current.History.BeginTransaction();

			try {
				var iniMousePos = sv.MousePosition;
				var transform = sv.Scene.CalcTransitionToSpaceOf(Document.Current.Container.AsWidget);
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
							Core.Operations.SetAnimableProperty.Perform(bone, nameof(Bone.Rotation), angle);
						}
						Core.Operations.SetAnimableProperty.Perform(bone, nameof(Bone.Length), dir.Length);
					} else {
						var dragDelta = sv.MousePosition * transform - iniMousePos * transform;
						var boneChain = IKSolver.SolveFor(bone, entry.Tip + dragDelta);
						foreach (var pair in boneChain) {
							Core.Operations.SetAnimableProperty.Perform(pair.Item1, nameof(Bone.Rotation), pair.Item2);
						}
					}
					bone.Parent.Update(0);
					yield return null;
				}
			} finally {
				sv.Input.ConsumeKey(Key.Mouse0);
				Document.Current.History.EndTransaction();
				Window.Current.Invalidate();
			}
		}
	}
}
