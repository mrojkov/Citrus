using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class MouseSelectionProcessor : ITaskProvider
	{
		public static readonly List<ISelectionTester> SelectionTesters = new List<ISelectionTester>();

		static MouseSelectionProcessor()
		{
			SelectionTesters.Add(new WidgetSelectionTester());
			SelectionTesters.Add(new PointObjectSelectionTester());
			SelectionTesters.Add(new SplinePoint3DSelectionTester());
		}

		public IEnumerator<object> Task()
		{
			var sceneView = SceneView.Instance;
			var input = sceneView.Input;
			while (true) {
				if (input.WasMousePressed() && !input.IsKeyPressed(Key.Shift)) {
					var rect = new Rectangle(sceneView.MousePosition, sceneView.MousePosition);
					var presenter = new DelegatePresenter<Widget>(w => {
						w.PrepareRendererState();
						var t = sceneView.Scene.CalcTransitionToSpaceOf(sceneView.Frame);
						Renderer.DrawRectOutline(rect.A * t, rect.B * t, ColorTheme.Current.SceneView.MouseSelection);
					});
					sceneView.Frame.CompoundPostPresenter.Add(presenter);
					input.CaptureMouse();
					Document.Current.History.BeginTransaction();
					try {
						var clicked = true;
						var selectedNodes = Document.Current.SelectedNodes().Editable().ToList();
						while (input.IsMousePressed()) {
							rect.B = sceneView.MousePosition;
							clicked &= (rect.B - rect.A).Length <= 5;
							if (!clicked)
								RefreshSelectedNodes(rect, selectedNodes);
							CommonWindow.Current.Invalidate();
							yield return null;
						}
						if (clicked) {
							var controlPressed = SceneView.Instance.Input.IsKeyPressed(Key.Control);
							if (!controlPressed)
								Core.Operations.ClearRowSelection.Perform();
							foreach (var node in Document.Current.Container.Nodes.Editable()) {
								if (Probe(node, rect.A)) {
									Core.Operations.SelectNode.Perform(node, !controlPressed || !Document.Current.SelectedNodes().Contains(node));
									break;
								}
							}
						}
						sceneView.Frame.CompoundPostPresenter.Remove(presenter);
						CommonWindow.Current.Invalidate();
					} finally {
						input.ReleaseMouse();
						Document.Current.History.EndTransaction();
					}
				}
				yield return null;
			}
		}

		void RefreshSelectedNodes(Rectangle rect, IEnumerable<Node> originalSelection)
		{
			var ctrlPressed = SceneView.Instance.Input.IsKeyPressed(Key.Control);
			var currentSelection = Document.Current.SelectedNodes();
			var newSelection = Document.Current.Container.Nodes.Editable().Where(n =>
				ctrlPressed ? Probe(n, rect) ^ originalSelection.Contains(n) : Probe(n, rect));
			if (!newSelection.SequenceEqual(currentSelection)) {
				Core.Operations.ClearRowSelection.Perform();
				foreach (var node in newSelection) {
					Core.Operations.SelectNode.Perform(node);
				}
			}
		}

		bool Probe(Node node, Vector2 point) => SelectionTesters.Any(i => i.Probe(node, point));
		bool Probe(Node node, Rectangle rectangle) => SelectionTesters.Any(i => i.Probe(node, rectangle));

		public interface ISelectionTester
		{
			bool Probe(Node node, Vector2 point);
			bool Probe(Node node, Rectangle rectangle);
		}

		public abstract class SelectionTester<T> : ISelectionTester where T: Node
		{
			public bool Probe(Node node, Vector2 point) => (node is T) && ProbeInternal((T)node, point);
			public bool Probe(Node node, Rectangle rectangle) => (node is T) && ProbeInternal((T)node, rectangle);

			protected abstract bool ProbeInternal(T node, Vector2 point);
			protected abstract bool ProbeInternal(T node, Rectangle rectangle);
		}

		public class WidgetSelectionTester : SelectionTester<Widget>
		{
			protected override bool ProbeInternal(Widget widget, Vector2 point)
			{
				var hull = widget.CalcHullInSpaceOf(SceneView.Instance.Scene);
				return hull.Contains(point);
			}

			protected override bool ProbeInternal(Widget widget, Rectangle rectangle)
			{
				var canvas = SceneView.Instance.Scene;
				var hull = widget.CalcHullInSpaceOf(canvas);
				for (int i = 0; i < 4; i++) {
					if (rectangle.Contains(hull[i])) {
						return true;
					}
				}
				var pivot = widget.CalcPositionInSpaceOf(canvas);
				return rectangle.Contains(pivot);
			}
		}

		public class PointObjectSelectionTester : SelectionTester<PointObject>
		{
			protected override bool ProbeInternal(PointObject pobject, Vector2 point)
			{
				var pos = pobject.CalcPositionInSpaceOf(SceneView.Instance.Scene);
				return SceneView.Instance.HitTestControlPoint(pos, 5);
			}

			protected override bool ProbeInternal(PointObject pobject, Rectangle rectangle)
			{
				var p = pobject.TransformedPosition;
				var t = ((Widget)pobject.Parent).CalcTransitionToSpaceOf(SceneView.Instance.Scene);
				return rectangle.Contains(t * p);
			}
		}

		public class SplinePoint3DSelectionTester : SelectionTester<SplinePoint3D>
		{
			protected override bool ProbeInternal(SplinePoint3D splinePoint, Vector2 point)
			{
				return SceneView.Instance.HitTestControlPoint(CalcPositionInSceneViewSpace(splinePoint));
			}

			protected override bool ProbeInternal(SplinePoint3D splinePoint, Rectangle rectangle)
			{
				return rectangle.Contains(CalcPositionInSceneViewSpace(splinePoint));
			}

			Vector2 CalcPositionInSceneViewSpace(SplinePoint3D splinePoint)
			{
				var spline = (Spline3D)splinePoint.Parent;
				var viewport = spline.GetViewport();
				var viewportToScene = viewport.CalcTransitionToSpaceOf(SceneView.Instance.Scene);
				return (Vector2)viewport.WorldToViewportPoint(splinePoint.Position * spline.GlobalTransform) * viewportToScene;
			}
		}
	}
}