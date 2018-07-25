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

		public IEnumerator<object> Task()
		{
			while (true) {
				if (!CoreUserPreferences.Instance.ShowAnimationPath) {
					yield return null;
					continue;
				}
				var nodes = Document.Current.SelectedNodes().Editable();
				var mousePosition = sv.MousePosition;
				foreach (var node in nodes) {
					if (node is IAnimable) {
						var animable = node as IAnimable;
						foreach (var animator in animable.Animators) {
							if (
								animator is Vector2Animator &&
								animator.TargetProperty == nameof(Widget.Position)
							) {
								var keys = animator.ReadonlyKeys.ToList();
								var transform = node.Parent.AsWidget.CalcTransitionToSpaceOf(sv.Scene);
								foreach (var key in keys) {
									if ((mousePosition - (Vector2)key.Value * transform).Length < 20) {
										Utils.ChangeCursorIfDefault(MouseCursor.Hand);
										if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
											yield return Drag(node, key);
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

		private IEnumerator<object> Drag(Node node, IKeyframe key)
		{
			var transform = sv.Scene.CalcTransitionToSpaceOf(node.Parent.AsWidget);
			var initMousePos = sv.MousePosition * transform;
			using (Document.Current.History.BeginTransaction()) {
				while (sv.Input.IsMousePressed()) {
					Document.Current.History.RollbackTransaction();
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					var curMousePos = sv.MousePosition * transform;
					var diff = curMousePos - initMousePos;
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
