using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Yuzu;

namespace Tangerine.UI.SceneView
{
	public class CreateWidgetProcessor : ITaskProvider
	{
		private SceneView sv => SceneView.Instance;

		public IEnumerator<object> Task()
		{
			ICommand command = new Command();
			Type nodeTypeActive = null;
			while (true) {
				if (sv.Input.WasKeyPressed(Key.Escape) || sv.Input.WasMousePressed(1)) {
					nodeTypeActive = null;
				}
				Type nodeTypeIncome;
				ICommand newCommand;
				if (CreateNodeRequestComponent.Consume<Widget>(sv.Components, out nodeTypeIncome, out newCommand)) {
					nodeTypeActive = nodeTypeIncome;
					command.Checked = false;
					command = newCommand;
					command.Checked = true;
				}

				if (nodeTypeActive == null) {
					command.Checked = false;
					yield return null;
					continue;
				}

				if (sv.InputArea.IsMouseOver()) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
				}
				var container = Document.Current.Container as Widget;
				CreateNodeRequestComponent.Consume<Node>(sv.Components);
				if (sv.Input.WasMousePressed() && container != null) {
					sv.Input.ConsumeKey(Key.Mouse0);
					var t = container.LocalToWorldTransform.CalcInversed();
					var rect = new Rectangle(sv.MousePosition * t, sv.MousePosition * t);
					var presenter = new SyncDelegatePresenter<Widget>(w => {
						w.PrepareRendererState();
						var t2 = container.LocalToWorldTransform * sv.CalcTransitionFromSceneSpace(sv.Frame);
						DrawCreateWidgetGizmo(rect.A, rect.B, t2);
					});
					sv.Frame.CompoundPostPresenter.Add(presenter);
					using (Document.Current.History.BeginTransaction()) {
						while (sv.Input.IsMousePressed()) {
							rect.B = sv.MousePosition * t;
							CommonWindow.Current.Invalidate();
							yield return null;
						}
						sv.Frame.CompoundPostPresenter.Remove(presenter);
						try {
							rect.Normalize();
							var widget = (Widget)Core.Operations.CreateNode.Perform(nodeTypeActive);
							Core.Operations.SetProperty.Perform(widget, nameof(Widget.Size), rect.B - rect.A);
							Core.Operations.SetProperty.Perform(widget, nameof(Widget.Position), rect.A + widget.Size * 0.5f);
							Core.Operations.SetProperty.Perform(widget, nameof(Widget.Pivot), Vector2.Half);
						} catch (InvalidOperationException e) {
							AlertDialog.Show(e.Message);
						}
						Document.Current.History.CommitTransaction();
					}

					nodeTypeActive = null;
					Utils.ChangeCursorIfDefault(MouseCursor.Default);
				}

				yield return null;
			}
		}

		private static void DrawCreateWidgetGizmo(Vector2 a, Vector2 b, Matrix32 t)
		{
			var c = ColorTheme.Current.SceneView.MouseSelection;
			Renderer.DrawLine(a * t, new Vector2(b.X, a.Y) * t, c, 1, LineCap.Square);
			Renderer.DrawLine(new Vector2(b.X, a.Y) * t, b * t, c, 1, LineCap.Square);
			Renderer.DrawLine(b * t, new Vector2(a.X, b.Y) * t, c, 1, LineCap.Square);
			Renderer.DrawLine(new Vector2(a.X, b.Y) * t, a * t, c, 1, LineCap.Square);
			var midX = (a.X + b.X) * 0.5f;
			var midY = (a.Y + b.Y) * 0.5f;
			Renderer.DrawLine(new Vector2(midX, a.Y) * t, new Vector2(midX, b.Y) * t, c, 1, LineCap.Square);
			Renderer.DrawLine(new Vector2(a.X, midY) * t, new Vector2(b.X, midY) * t, c, 1, LineCap.Square);
		}
	}
}
