using System;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class SplinePresenter : CustomPresenter<Spline>
	{
		protected override void InternalRender(Spline spline)
		{
			if (Document.Current.Container.IsRunning) {
				return;
			}
			SceneView.Instance.Frame.PrepareRendererState();
			var l = spline.CalcLengthAccurate(100);
			const float step = 5.0f;
			var xform = spline.CalcTransitionToSpaceOf(SceneView.Instance.Frame);
			var p = spline.CalcPoint(0) * xform;
			for (float t = 0; t <= l + step; t += step) {
				var v = spline.CalcPoint(Math.Min(t, l)) * xform;
				DrawLine(p, v);
				p = v;
			}
		}

		Vertex[] v = new Vertex[4];

		void DrawLine(Vector2 a, Vector2 b)
		{
			var d = (b - a).Normalized * 0.5f;
			var n = new Vector2(-d.Y, d.X);
			var color1 = Color4.White;
			var color2 = Color4.Black;
			v[0] = new Vertex { Pos = a - n, Color = color1 };
			v[1] = new Vertex { Pos = b - n, Color = color2 };
			v[2] = new Vertex { Pos = b + n, Color = color2 };
			v[3] = new Vertex { Pos = a + n, Color = color1 };
			Renderer.DrawTriangleFan(null, null, v, v.Length);
		}
	}
}