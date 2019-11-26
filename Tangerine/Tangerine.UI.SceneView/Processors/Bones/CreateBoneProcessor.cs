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
		private ICommand command;

		public IEnumerator<object> Task()
		{
			while (true) {
				if (CreateNodeRequestComponent.Consume<Bone>(SceneView.Instance.Components, out command)) {
					yield return CreateBoneTask();
				}
				yield return null;
			}
		}

		IEnumerator<object> CreateBoneTask()
		{
			command.Checked = true;
			while (true) {
				Bone bone = null;

				var transform = Document.Current.Container.AsWidget.LocalToWorldTransform;
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
				if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {

					Widget container = (Widget) Document.Current.Container;
					int boneIndex;
					if (!container.BoneArray.Equals(default(BoneArray))) {
						boneIndex = container.BoneArray.items.Length + 1;
					} else {
						boneIndex = 1;
					}
					Matrix32 t = container.LocalToWorldTransform.CalcInversed();
					Vector2 initPosition = sv.MousePosition * t;

					Vector2 pos = Vector2.Zero;
					if (index == 0 && container.Width.Abs() > Mathf.ZeroTolerance && container.Height.Abs() > Mathf.ZeroTolerance) {
						pos = initPosition;
					}
					using (Document.Current.History.BeginTransaction()) {
						try {
							bone = (Bone) Core.Operations.CreateNode.Perform(typeof(Bone));
						} catch (InvalidOperationException e) {
							AlertDialog.Show(e.Message);
							break;
						}
						bone.Index = boneIndex;
						Core.Operations.SetProperty.Perform(bone, nameof(Bone.Position), pos);
						Core.Operations.SetProperty.Perform(bone, nameof(Bone.BaseIndex), index);
						Core.Operations.SelectNode.Perform(bone);
						if (bone.BaseIndex != 0) {
							Core.Operations.SortBonesInChain.Perform(bone);
						}
						using (Document.Current.History.BeginTransaction()) {
							while (sv.Input.IsMousePressed()) {
								Document.Current.History.RollbackTransaction();
	
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
							Document.Current.History.CommitTransaction();
						}
						// do not create zero bone
						if (bone != null && bone.Length == 0) {
							Document.Current.History.RollbackTransaction();
							// must set length to zero to exectue "break;" later 
							bone.Length = 0;
						}
						Document.Current.History.CommitTransaction();
					}
					SceneView.Instance.Components.Remove<CreateBoneHelper>();
				}
				// turn off creation if was only click without drag (zero length bone)
				if (bone != null && bone.Length == 0) {
					break;
				}
				if (sv.Input.WasMousePressed(1) || sv.Input.WasKeyPressed(Key.Escape)) {
					break;
				}

				yield return null;
			}
			SceneView.Instance.Components.Remove<CreateBoneHelper>();
			command.Checked = false;
		}
	}

	internal class CreateBoneHelper : NodeComponent
	{
		public Vector2 HitTip { get; set; }
	}
}
