using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	class ChangeBoneLengthProcessor : ITaskProvider
	{
		SceneView sv => SceneView.Instance;

		public IEnumerator<object> Task()
		{
			while (true) {
				var bones = Document.Current.SelectedNodes().Editable().OfType<Bone>().ToList();
				if (bones.Count == 1) {
					var t = Document.Current.Container.AsWidget.CalcTransitionToSpaceOf(sv.Scene);
					var hull = BonePresenter.CalcHull(bones.First());
					for (int i = 0; i < 4; i++) {
						hull[i] = t * hull[i];
					}
					if (hull.Contains(sv.MousePosition) && sv.Input.IsKeyPressed(Key.Control)) {
						Utils.ChangeCursorIfDefault(MouseCursor.SizeWE);
						if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
							yield return Resize(bones.First());
						}
					}
				}
				yield return null;
			}
		}

		private IEnumerator<object> Resize(Bone bone)
		{
			sv.Input.CaptureMouse();
			Document.Current.History.BeginTransaction();
			try {
				var iniMousePos = sv.MousePosition;
				var initBoneLength = bone.Length;
				while (sv.Input.IsMousePressed()) {
					Utils.ChangeCursorIfDefault(MouseCursor.SizeWE);
					var dragDelta = sv.MousePosition - iniMousePos;
					Core.Operations.SetAnimableProperty.Perform(bone, nameof(Bone.Length), initBoneLength + dragDelta.X);
					yield return null;
				}
			} finally {
				sv.Input.ReleaseMouse();
				sv.Input.ConsumeKey(Key.Mouse0);
				Document.Current.History.EndTransaction();
			}
		}
	}
}
