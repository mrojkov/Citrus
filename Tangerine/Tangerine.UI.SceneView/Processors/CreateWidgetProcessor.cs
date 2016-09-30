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
				Type nodeType = null;
				if (ConsumeCreateWidgetRequest(ref nodeType)) {
					yield return CreateWidgetTask(nodeType);
				}
				yield return null;
			}
		}

		IEnumerator<object> CreateWidgetTask(Type nodeType)
		{
			while (true) {
				if (sv.InputArea.IsMouseOver()) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
				}
				ConsumeCreateWidgetRequest(ref nodeType);
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

		bool ConsumeCreateWidgetRequest(ref Type nodeType)
		{
			var c = sv.Components.Get<CreateNodeRequestComponent>();
			if (c != null && c.NodeType.IsSubclassOf(typeof(Widget))) {
				sv.Components.Remove<CreateNodeRequestComponent>();
				nodeType = c.NodeType;
				return true;
			}
			return false;
		}

		static void DrawRectOutline(Vector2 a, Vector2 b, Matrix32 t)
		{
			var c = SceneViewColors.MouseSelection;
			Renderer.DrawLine(a * t, new Vector2(b.X, a.Y) * t, c, 1, LineCap.Square);
			Renderer.DrawLine(new Vector2(b.X, a.Y) * t, b * t, c, 1, LineCap.Square);
			Renderer.DrawLine(b * t, new Vector2(a.X, b.Y) * t, c, 1, LineCap.Square);
			Renderer.DrawLine(new Vector2(a.X, b.Y) * t, a * t, c, 1, LineCap.Square);
		}
	}
}
