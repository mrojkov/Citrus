using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	class ChangeBoneRadiusProcessor : ITaskProvider
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
					if (hull.Contains(sv.MousePosition) && sv.Input.IsKeyPressed(Key.Shift)) {
						Utils.ChangeCursorIfDefault(MouseCursor.SizeNS);
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
			Document.Current.History.BeginTransaction();
			try {
				var iniMousePos = sv.MousePosition;
				var initEffectiveRadius = bone.EffectiveRadius;
				var initFadeoutZone = bone.FadeoutZone;
				while (sv.Input.IsMousePressed()) {
					Document.Current.History.RevertActiveTransaction();
					
					Utils.ChangeCursorIfDefault(MouseCursor.SizeNS);
					var dragDelta = sv.MousePosition - iniMousePos;
					Core.Operations.SetAnimableProperty.Perform(bone, nameof(Bone.EffectiveRadius), initEffectiveRadius + dragDelta.X, CoreUserPreferences.Instance.AutoKeyframes);
					Core.Operations.SetAnimableProperty.Perform(bone, nameof(Bone.FadeoutZone), initFadeoutZone + dragDelta.Y, CoreUserPreferences.Instance.AutoKeyframes);
					yield return null;
				}
			} finally {
				sv.Input.ConsumeKey(Key.Mouse0);
				Document.Current.History.EndTransaction();
			}
		}
	}
}
