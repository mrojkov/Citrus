using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class DragAnimationPathPointProcessor : ITaskProvider
	{
		static SceneView sv => SceneView.Instance;
		private readonly VisualHint animationPathHint =
			VisualHintsRegistry.Instance.Register("/All/Animation Path", hideRule: VisualHintsRegistry.HideRules.VisibleIfProjectOpened);

		public IEnumerator<object> Task()
		{
			while (true) {
				if (
					!animationPathHint.Enabled ||
					!SceneView.Instance.InputArea.IsMouseOverThisOrDescendant() ||
					!sv.Input.IsKeyPressed(Key.Control)
				) {
					yield return null;
					continue;
				}
				var nodes = Document.Current.SelectedNodes().Editable();
				var mousePosition = sv.Frame.LocalMousePosition();
				foreach (var node in nodes) {
					if (!(node is Widget)) {
						continue;
					}
					if (node is IAnimationHost) {
						var animable = node as IAnimationHost;
						foreach (var animator in animable.Animators) {
							if (
								animator is Vector2Animator &&
								animator.TargetPropertyPath == nameof(Widget.Position)
							) {
								var keys = animator.ReadonlyKeys.ToList();
								var transform = node.Parent.AsWidget.LocalToWorldTransform * sv.CalcTransitionFromSceneSpace(sv.Frame);
								foreach (var key in keys) {
									if ((mousePosition - (Vector2)key.Value * transform).Length < 20) {
										Utils.ChangeCursorIfDefault(MouseCursor.Hand);
										if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
											yield return Drag(node as Widget, animator, key);
										}
										goto Next;
									}
								}
							}
						}
					}
				}
				Next:
				yield return null;
			}
		}

		private IEnumerator<object> Drag(Widget widget, IAnimator animator, IKeyframe key)
		{
			var transform = widget.Parent.AsWidget.LocalToWorldTransform.CalcInversed();
			var initMousePos = sv.MousePosition * transform;
			using (Document.Current.History.BeginTransaction()) {
				while (sv.Input.IsMousePressed()) {
					Document.Current.History.RollbackTransaction();
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					var curMousePos = sv.MousePosition * transform;
					var diff = curMousePos - initMousePos;
					animator.ResetCache();
					Core.Operations.SetProperty.Perform(
						typeof(IKeyframe), key, nameof(IKeyframe.Value), (Vector2)key.Value + diff);
					yield return null;
				}
				sv.Input.ConsumeKey(Key.Mouse0);
				Document.Current.History.CommitTransaction();
			}
		}
	}
}
