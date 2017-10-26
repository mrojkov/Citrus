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
				var transform = Document.Current.Container.AsWidget.CalcTransitionToSpaceOf(sv.Scene);
				if (sv.InputArea.IsMouseOver()) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
				}
				var items = Document.Current.Container.AsWidget.BoneArray.items;
				var index = 0;
				if (items != null) {
					for (var i = 1; i < items.Length; i++) {
						if (sv.HitTestControlPoint(transform * items[i].Tip)) {
							index = i;
							break;
						}
					}
					SceneView.Instance.Components.GetOrAdd<CreateBoneHelper>().HitTip =
						index != 0 ? items[index].Tip : default(Vector2);
				}

				Window.Current.Invalidate();
				CreateNodeRequestComponent.Consume<Node>(sv.Components);
				var bone = default(Bone);
				if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
					try {
						bone = (Bone)Core.Operations.CreateNode.Perform(typeof(Bone));
						var container = (Widget)Document.Current.Container;
						if (!container.BoneArray.Equals(default(BoneArray))) {
							bone.Index = container.BoneArray.items.Length + 1;
						} else {
							bone.Index = 1;
						}
						var t = sv.Scene.CalcTransitionToSpaceOf(container);
						var pos = Vector2.Zero;
						if (index == 0 && container.Width.Abs() > Mathf.ZeroTolerance && container.Height.Abs() > Mathf.ZeroTolerance) {
							pos = sv.MousePosition * t;
						}
						Core.Operations.SetProperty.Perform(bone, nameof(Bone.Position), pos);
						Core.Operations.SetProperty.Perform(bone, nameof(Bone.BaseIndex), index);
						Core.Operations.SelectNode.Perform(bone);
						Document.Current.History.BeginTransaction();
						if (bone.BaseIndex != 0) {
							Core.Operations.SortBonesInChain.Perform(bone);
						}
						var initPosition = sv.MousePosition * t;
						while (sv.Input.IsMousePressed()) {
							var dir = (sv.MousePosition * t - initPosition).Snap(Vector2.Zero);
							var angle = dir.Atan2Deg;
							if (index != 0) {
								var prentDir = items[index].Tip - items[index].Joint;
								angle = Vector2.AngleDeg(prentDir, dir);
							}
							Core.Operations.SetProperty.Perform(bone, nameof(Bone.Rotation), angle);
							Core.Operations.SetProperty.Perform(bone, nameof(Bone.Length), dir.Length);
							yield return null;
						}
					} finally {
						SceneView.Instance.Components.Remove<CreateBoneHelper>();
						Document.Current.History.EndTransaction();
					}
				}
				if (bone != null && bone?.Length == 0) {
					Document.Current.History.RevertLastTransaction();
					break;
				}
				if (sv.Input.WasMousePressed(1)) {
					break;
				}

				yield return null;
			}
			SceneView.Instance.Components.Remove<CreateBoneHelper>();
		}
	}

	internal class CreateBoneHelper : NodeComponent
	{
		public Vector2 HitTip { get; set; }
	}
}
