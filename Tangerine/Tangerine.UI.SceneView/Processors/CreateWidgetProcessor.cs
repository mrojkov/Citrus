using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class CreateWidgetProcessor : IProcessor
	{
		SceneView sv => SceneView.Instance;

		public IEnumerator<object> Loop()
		{
			while (true) {
				var c = sv.Components.Get<CreateNodeRequestComponent>();
				if (c != null && c.NodeType.IsSubclassOf(typeof(Widget))) {
					sv.Components.Remove<CreateNodeRequestComponent>();
					yield return CreateWidgetTask(c.NodeType);
				}
				yield return null;
			}
		}

		public IEnumerator<object> CreateWidgetTask(Type nodeType)
		{
			while (true) {
				if (sv.InputArea.IsMouseOver()) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
				}
				if (sv.Input.WasMousePressed()) {
					sv.Input.CaptureMouse();
					sv.Input.ConsumeKey(Key.Mouse0);
					var container = (Widget)Document.Current.Container;
					var t = sv.Scene.CalcTransitionToSpaceOf(container);
					var rect = new Rectangle(sv.MousePosition * t, sv.MousePosition * t);
					var presenter = new DelegatePresenter<Widget>(w => {
						w.PrepareRendererState();
						var t2 = container.CalcTransitionToSpaceOf(sv.Frame);
						DrawRectOutline(rect.A, (rect.A + rect.B) / 2, t2);
						DrawRectOutline(rect.A, rect.B, t2);
					});
					sv.Frame.CompoundPostPresenter.Add(presenter);
					while (sv.Input.IsMousePressed()) {
						rect.B = sv.MousePosition * t;
						CommonWindow.Current.Invalidate();
						yield return null;
					}
					sv.Input.ReleaseMouse();
					sv.Frame.CompoundPostPresenter.Remove(presenter);
					Document.Current.History.BeginTransaction();
					var widget = (Widget)Core.Operations.CreateNode.Perform(container, 0, nodeType);
					Core.Operations.SetProperty.Perform(widget, "Position", rect.A);
					Core.Operations.SetProperty.Perform(widget, "Size", rect.B - rect.A);
					Document.Current.History.EndTransaction();
					break;
				}
				yield return null;
			}
		}

		public static void DrawRectOutline(Vector2 a, Vector2 b, Matrix32 t)
		{
			var c = SceneViewColors.MouseSelection;
			Renderer.DrawLine(a * t, new Vector2(b.X, a.Y) * t, c, 1, LineCap.Square);
			Renderer.DrawLine(new Vector2(b.X, a.Y) * t, b * t, c, 1, LineCap.Square);
			Renderer.DrawLine(b * t, new Vector2(a.X, b.Y) * t, c, 1, LineCap.Square);
			Renderer.DrawLine(new Vector2(a.X, b.Y) * t, a * t, c, 1, LineCap.Square);
		}
	}
}
