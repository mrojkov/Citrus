using Lime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Tangerine.Core;

namespace Tangerine.UI
{
	public static class Utils
	{

		public static IEnumerable<T> Editable<T>(this IEnumerable<T> nodes) where T : Node
		{
			return nodes.Where(n => !n.GetTangerineFlag(TangerineFlags.Locked) && !n.GetTangerineFlag(TangerineFlags.Hidden));
		}

		public static void ChangeCursorIfDefault(MouseCursor cursor)
		{
			if (WidgetContext.Current.MouseCursor == MouseCursor.Default) {
				WidgetContext.Current.MouseCursor = cursor;
			}
		}

		public static Vector2 Snap(this Vector2 value, Vector2 origin, float distanceTolerance = 0.001f)
		{
			return (value - origin).Length > distanceTolerance ? value : origin;
		}

		public static float Snap(this float value, float origin, float distanceTolerance = 0.001f)
		{
			return (value - origin).Abs() > distanceTolerance ? value : origin;
		}

		public static float RoundTo(float value, float step)
		{
			return (value / step).Round() * step;
		}

		public static bool CalcHullAndPivot(IEnumerable<Widget> widgets, Widget canvas, out Quadrangle hull, out Vector2 pivot)
		{
			Widget first = null;
			var pivotsEqual = true;
			var aabb = Rectangle.Empty;
			pivot = Vector2.Zero;
			hull = new Quadrangle();
			float pivotTolerance = 1e-1f;
			foreach (var widget in widgets) {
				if (first == null) {
					hull = widget.CalcHullInSpaceOf(canvas);
					pivot = widget.CalcPositionInSpaceOf(canvas);
					aabb = widget.CalcAABBInSpaceOf(canvas);
					first = widget;
				} else {
					var t = widget.CalcAABBInSpaceOf(canvas);
					aabb = aabb
						.IncludingPoint(t.A)
						.IncludingPoint(new Vector2(t.Right, t.Top))
						.IncludingPoint(t.B)
						.IncludingPoint(new Vector2(t.Left, t.Bottom));
					hull = aabb.ToQuadrangle();
					pivotsEqual &= Vector2.Distance(widget.CalcPositionInSpaceOf(canvas), pivot) <= pivotTolerance;
				}
			}
			if (first == null) {
				return false;
			}
			if (!pivotsEqual) {
				pivot = aabb.Center;
			}
			return true;
		}

		public static bool CalcAABB(IEnumerable<Node> nodes, Widget canvas, out Rectangle aabb)
		{
			var result = false;
			aabb = Rectangle.Empty;
			foreach (var node in nodes) {
				var widget = node as Widget;
				if (widget != null) {
					var t = widget.CalcAABBInSpaceOf(canvas);
					aabb = !result ? t : aabb.
						IncludingPoint(t.A).
						IncludingPoint(new Vector2(t.Right, t.Top)).
						IncludingPoint(t.B).
						IncludingPoint(new Vector2(t.Left, t.Bottom));
					result = true;
				}
				var po = node as PointObject;
				if (po != null) {
					var p = po.CalcPositionInSpaceOf(canvas);
					aabb = result ? aabb.IncludingPoint(p) : new Rectangle(p, p);
					result = true;
				}
			}
			return result;
		}

		public static Quadrangle CalcAABB(IEnumerable<PointObject> points, bool IncludeOffset = false)
		{
			var aabb = new Rectangle(new Vector2(float.MaxValue), new Vector2(float.MinValue));
			foreach (var point in points) {
				aabb = aabb.IncludingPoint(point.Position + (IncludeOffset ? point.Offset / point.Parent.AsWidget.Size : Vector2.Zero));
			}
			return aabb.ToQuadrangle();
		}

		public static bool AssertCurrentDocument(string assetPath, string assetType)
		{
			if (assetPath.Equals(Document.Current.Path)) {
				AlertDialog.Show($"Сycle dependency is not allowed: {assetPath}{assetType}");
				return false;
			}
			return true;
		}

		public static bool ExtractAssetPathOrShowAlert(string filePath, out string assetPath, out string assetType)
		{
			string path = AssetPath.CorrectSlashes(filePath);
			string assetsPath = AssetPath.CorrectSlashes(Core.Project.Current.AssetsDirectory);
			if (Path.IsPathRooted(path)) {
				if (!path.StartsWith(assetsPath, StringComparison.CurrentCultureIgnoreCase)) {
					AlertDialog.Show($"Asset '{filePath}' outside the project directory");
					assetPath = null;
					assetType = null;
					return false;
				} else {
					path = path.Substring(assetsPath.Length + 1);
				}
			}
			assetPath = Path.ChangeExtension(path, null);
			assetType = Path.GetExtension(path).ToLower();
			return true;
		}

		public static IEnumerable<string> GetAssetPaths(IEnumerable<string> filePaths)
		{
			foreach (var filePath in filePaths) {
				if (
					!ExtractAssetPathOrShowAlert(filePath, out var assetPath, out var assetType) ||
					!AssertCurrentDocument(assetPath, assetType)
				) {
					continue;
				}
				yield return assetPath;
			}
		}
	}
}
