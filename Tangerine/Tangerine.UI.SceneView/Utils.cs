using System;
using System.Linq;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.SceneView
{
	public static class Utils
	{
		public static List<Widget> UnlockedWidgets()
		{
			return Core.Document.Current.SelectedNodes().OfType<Widget>().Where(w => !w.GetTangerineFlag(TangerineFlags.Locked)).ToList();
		}

		public static void CalcHullAndPivot(List<Widget> widgets, Widget canvas, out Quadrangle hull, out Vector2 pivot)
		{
			if (widgets.Count == 0) {
				throw new ArgumentException();
			}
			if (widgets.Count == 1) {
				hull = widgets[0].CalcHullInSpaceOf(canvas);
				pivot = widgets[0].CalcPositionInSpaceOf(canvas);
			} else {
				var rect = new Rectangle(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
				foreach (var widget in widgets) {
					var aabb = widget.CalcAABBInSpaceOf(canvas);
					rect = rect
						.IncludingPoint(aabb.A)
						.IncludingPoint(new Vector2(aabb.Right, aabb.Top))
						.IncludingPoint(aabb.B)
						.IncludingPoint(new Vector2(aabb.Left, aabb.Bottom));
				}
				hull = rect.ToQuadrangle();
				pivot = rect.Center;
				bool pivotsEqual = true;
				var p = widgets[0].CalcPositionInSpaceOf(canvas);
				foreach (var widget in widgets) {
					var t = widget.CalcPositionInSpaceOf(canvas);
					if (t != p) {
						pivotsEqual = false;
						break;
					}
				}
				if (pivotsEqual) {
					pivot = p;
				}
			}
		}
	}
}
