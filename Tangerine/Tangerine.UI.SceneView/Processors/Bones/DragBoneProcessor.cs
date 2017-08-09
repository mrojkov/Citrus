using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class DragBoneProcessor : ITaskProvider
	{
		SceneView sv => SceneView.Instance;

		public IEnumerator<object> Task()
		{
			while (true) {
				var bones = Document.Current.SelectedNodes().Editable().OfType<Bone>().ToList();
				if (bones.Count == 1) {
					var entry = bones.First().Parent.AsWidget.BoneArray[bones[0].Index];
					var t = Document.Current.Container.AsWidget.CalcTransitionToSpaceOf(sv.Scene);
					if (sv.HitTestControlPoint(t * entry.Joint)) {
						Utils.ChangeCursorIfDefault(MouseCursor.Hand);
						if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
							yield return Drag(bones.First(), entry, false);
						}
					}

					if (sv.HitTestControlPoint(t * entry.Tip, 6)) {
						Utils.ChangeCursorIfDefault(MouseCursor.Hand);
						if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
							yield return Drag(bones.First(), entry, true);
						}
					}
				}
				yield return null;
			}
		}

		private IEnumerator<object> Drag(Bone bone, BoneArray.Entry entry, bool dragTip)
		{
			Document.Current.History.BeginTransaction();

			try {
				sv.Input.CaptureMouse();
				var iniMousePos = sv.MousePosition;
				var worldToLocal = sv.Scene.CalcTransitionToSpaceOf(Document.Current.Container.AsWidget);
				while (sv.Input.IsMousePressed()) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					var b = bone.Parent.AsWidget.BoneArray[bone.BaseIndex];
					var dragDelta = sv.MousePosition * worldToLocal - iniMousePos * worldToLocal;
					if (dragTip) {
						var boneChain = IKSolver.SolveFor(bone, entry.Tip + dragDelta);
						foreach (var pair in boneChain) {
							Core.Operations.SetAnimableProperty.Perform(pair.Item1, nameof(Bone.Rotation), pair.Item2);
						}
					} else {
						var position = bone.WorldToLocalTransform * (entry.Joint + dragDelta - b.Tip);
						Core.Operations.SetAnimableProperty.Perform(bone, nameof(Bone.Position), position);
					}
					bone.Parent.Update(0);
					yield return null;
				}
			} finally {
				sv.Input.ReleaseMouse();
				sv.Input.ConsumeKey(Key.Mouse0);
				Document.Current.History.EndTransaction();
				Window.Current.Invalidate();
			}
		}
	}
}
