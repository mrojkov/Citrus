using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class WASDProcessor : IProcessor
	{
		SceneView sv => SceneView.Instance;

		public IEnumerator<object> Loop()
		{
			while (true) {
				if (sv.Input.ConsumeKeyRepeat(KeyBindings.SceneViewKeys.DragUpFast)) {
					DragWidgets(new Vector2(0, -5));
				}
				if (sv.Input.ConsumeKeyRepeat(KeyBindings.SceneViewKeys.DragLeftFast)) {
					DragWidgets(new Vector2(-5, 0));
				}
				if (sv.Input.ConsumeKeyRepeat(KeyBindings.SceneViewKeys.DragDownFast)) {
					DragWidgets(new Vector2(0, 5));
				}
				if (sv.Input.ConsumeKeyRepeat(KeyBindings.SceneViewKeys.DragRightFast)) {
					DragWidgets(new Vector2(5, 0));
				}
				sv.Input.IsAcceptingKey(Key.A);
				if (sv.Input.ConsumeKeyRepeat(KeyBindings.SceneViewKeys.DragUp)) {
					DragWidgets(new Vector2(0, -1));
				}
				if (sv.Input.ConsumeKeyRepeat(KeyBindings.SceneViewKeys.DragLeft)) {
					DragWidgets(new Vector2(-1, 0));
				}
				if (sv.Input.ConsumeKeyRepeat(KeyBindings.SceneViewKeys.DragDown)) {
					DragWidgets(new Vector2(0, 1));
				}
				if (sv.Input.ConsumeKeyRepeat(KeyBindings.SceneViewKeys.DragRight)) {
					DragWidgets(new Vector2(1, 0));
				}
				yield return Task.WaitForInput();
			}
		}

		void DragWidgets(Vector2 delta)
		{
			var transform = Document.Current.Container.AsWidget.CalcTransitionToSpaceOf(sv.Scene).CalcInversed();
			var dragDelta = transform * delta - transform * Vector2.Zero;
			foreach (var widget in Document.Current.SelectedNodes().Editable().OfType<Widget>()) {
				Core.Operations.SetAnimableProperty.Perform(widget, "Position", widget.Position + dragDelta);
			}
		}
	}
}