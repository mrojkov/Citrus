using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class CreateBoneProcessor : ITaskProvider
	{
		SceneView sv => SceneView.Instance;

		public IEnumerator<object> Task()
		{
			while (true) {
				if (CreateNodeRequestComponent.Consume<Bone>(SceneView.Instance.Components)) {
					yield return CreateBoneTask();
				}
				yield return null;
			}
		}

		IEnumerator<object> CreateBoneTask()
		{
			while (true) {
				if (sv.InputArea.IsMouseOver()) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
				}
				CreateNodeRequestComponent.Consume<Node>(sv.Components);
				if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
					try {
						var bone = (Bone)Core.Operations.CreateNode.Perform(typeof(Bone), aboveSelected: false);
						var container = (Widget)Document.Current.Container;
						if (!container.BoneArray.Equals(default(BoneArray))) {
							bone.Index = container.BoneArray.items.Length + 1;
						} else {
							bone.Index = 1;
						}
						var t = sv.Scene.CalcTransitionToSpaceOf(container);
						var pos = Vector2.Zero;
						if (container.Width.Abs() > Mathf.ZeroTolerance && container.Height.Abs() > Mathf.ZeroTolerance) {
							pos = sv.MousePosition * t;
						}
						Core.Operations.SetProperty.Perform(bone, nameof(Bone.Position), pos);
						sv.Input.CaptureMouse();
						Document.Current.History.BeginTransaction();
						while (sv.Input.IsMousePressed()) {
							var dir = (sv.MousePosition * t - bone.Position).Snap(Vector2.Zero);
							Core.Operations.SetProperty.Perform(bone, nameof(Bone.Rotation), dir.Atan2Deg);
							Core.Operations.SetProperty.Perform(bone, nameof(Bone.Length), dir.Length);
							yield return null;
						}
					} finally {
						sv.Input.ReleaseMouse();
						Document.Current.History.EndTransaction();
					}
				}
				if (sv.Input.WasMousePressed(1)) {
					break;
				}
				yield return null;
			}
		}
	}
}
