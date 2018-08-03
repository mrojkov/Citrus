using System;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class SplinePresenter : CustomPresenter<Spline>
	{
		protected override void InternalRender(Spline spline)
		{
			if (
				Document.Current.PreviewAnimation ||
				(
					!SceneUserPreferences.Instance.DisplayNodeDecorationsForTypes.Contains(NodeDecoration.Spline) &&
					!(Document.Current.Container == spline) &&
					!Document.Current.SelectedNodes().Contains(spline)
				)
			) {
				return;
			}
			SceneView.Instance.Frame.PrepareRendererState();
			DrawSpline(spline);
		}

		void DrawSpline(Spline spline)
		{
			float step = 7.0f / SceneView.Instance.Scene.Scale.X;
			var xform = spline.CalcTransitionToSpaceOf(SceneView.Instance.Frame);
			Vector2? p = null;
			foreach (var v in spline.CalcPoints(step)) {
				if (p.HasValue) {
					DrawColoredLine(p.Value * xform, v * xform, Color4.White, Color4.Black);
				}
				p = v;
			}
		}

		static Vertex[] v = new Vertex[4];

		public static void DrawColoredLine(Vector2 a, Vector2 b, Color4 color1, Color4 color2)
		{
			var d = (b - a).Normalized * 0.5f;
			var n = new Vector2(-d.Y, d.X);
			v[0] = new Vertex { Pos = a - n, Color = color1 };
			v[1] = new Vertex { Pos = b - n, Color = color2 };
			v[2] = new Vertex { Pos = b + n, Color = color2 };
			v[3] = new Vertex { Pos = a + n, Color = color1 };
			Renderer.DrawTriangleFan(null, null, v, v.Length);
		}
	}
}