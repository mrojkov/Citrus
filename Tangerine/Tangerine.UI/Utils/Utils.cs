using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;

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

		public static void ApplyTransformationToWidgetsGroupOobb(Widget sceneWidget, IList<Widget> widgetsInParentSpace,
			Vector2 pivotInSceneSpace, bool oobbInFirstWidgetSpace,
			Vector2 curMousePosInSceneSpace, Vector2 prevMousePosSceneSpace,
			Func<Vector2, Vector2, Matrix32> onCalcTransformationFromOriginalVectorInOobbSpaceAndDeformedVectorInOobbSpace)
		{
			if (widgetsInParentSpace.Count == 0) return;

			Matrix32 fromSceneToParentSpace = sceneWidget.CalcTransitionToSpaceOf(widgetsInParentSpace[0].ParentWidget);

			ApplyTransformationToWidgetsGroupOobb(
				widgetsInParentSpace, 
				pivotInSceneSpace * fromSceneToParentSpace,
				oobbInFirstWidgetSpace,
				curMousePosInSceneSpace * fromSceneToParentSpace,
				prevMousePosSceneSpace * fromSceneToParentSpace, 
				onCalcTransformationFromOriginalVectorInOobbSpaceAndDeformedVectorInOobbSpace
			);
		}

		public static void ApplyTransformationToWidgetsGroupOobb(IList<Widget> widgetsInParentSpace,
			Vector2 pivotInParentSpace, bool oobbInFirstWidgetSpace,
			Vector2 curMousePosInParentSpace, Vector2 prevMousePosInParentSpace,
			Func<Vector2, Vector2, Matrix32> onCalcTransformationFromOriginalVectorInOobbSpaceAndDeformedVectorInOobbSpace)
		{
			if (widgetsInParentSpace.Count == 0) return;

			Matrix32 originalOobbToParentSpace = Matrix32.Translation(pivotInParentSpace);
			if (oobbInFirstWidgetSpace) {
				Matrix32 firstWidgetToParentSpace = widgetsInParentSpace[0].CalcLocalToParentTransform();

				originalOobbToParentSpace = firstWidgetToParentSpace *
						Matrix32.Translation(firstWidgetToParentSpace.T).CalcInversed() * Matrix32.Translation(pivotInParentSpace);
			}

			ApplyTransformationToWidgetsGroupOobb(
				widgetsInParentSpace, widgetsInParentSpace[0].ParentWidget, originalOobbToParentSpace, curMousePosInParentSpace, prevMousePosInParentSpace,
				onCalcTransformationFromOriginalVectorInOobbSpaceAndDeformedVectorInOobbSpace
			);
		}

		public static void ApplyTransformationToWidgetsGroupOobb(IEnumerable<Widget> widgetsInParentSpace,
			Widget parentWidget, Matrix32 oobbInParentSpace,
			Vector2 curMousePosInParentSpace, Vector2 prevMousePosInParentSpace,
			Func<Vector2, Vector2, Matrix32> onCalcTransformationFromOriginalVectorInOobbSpaceAndDeformedVectorInOobbSpace)
		{
			Vector2 pivotInParentSpace = oobbInParentSpace.T;

			Vector2 pivotInOobbSpace = pivotInParentSpace;
			Vector2 controlPointInOobbSpace = prevMousePosInParentSpace;
			Vector2 targetPointInOobbSpace = curMousePosInParentSpace;

			Matrix32 transformationFromParentToOobb = oobbInParentSpace.CalcInversed();
			pivotInOobbSpace = pivotInOobbSpace * transformationFromParentToOobb;
			controlPointInOobbSpace = controlPointInOobbSpace * transformationFromParentToOobb;
			targetPointInOobbSpace = targetPointInOobbSpace * transformationFromParentToOobb;

			Vector2 originalVectorInOobbSpace = controlPointInOobbSpace - pivotInOobbSpace;
			Vector2 deformedVectorInOobbSpace = targetPointInOobbSpace - pivotInOobbSpace;

			// calculate transformation from original vector to deformed vector
			Matrix32 deformationInOobbSpace =
					onCalcTransformationFromOriginalVectorInOobbSpaceAndDeformedVectorInOobbSpace(originalVectorInOobbSpace,
						deformedVectorInOobbSpace);

			ApplyTransformationToWidgetsGroupOobb(
				widgetsInParentSpace, parentWidget, oobbInParentSpace, deformationInOobbSpace
			);
		}

		public static void ApplyTransformationToWidgetsGroupOobb(IEnumerable<Widget> widgetsInParentSpace,
			Widget parentWidget, Matrix32 oobbInParentSpace, Matrix32 oobbTransformation)
		{

			Matrix32 originalOobbToWorldSpace = oobbInParentSpace * parentWidget.LocalToWorldTransform;

			foreach (Widget widget in widgetsInParentSpace) {
				Matrix32 widgetToOriginalOobbSpace = widget.LocalToWorldTransform * originalOobbToWorldSpace.CalcInversed();

				// calculate new oobb transformation in world space
				Matrix32 deformedOobbToWorldSpace = oobbTransformation * originalOobbToWorldSpace;

				Matrix32 deformedWidgetToWorldSpace = widgetToOriginalOobbSpace * deformedOobbToWorldSpace;

				Matrix32 deformedWidgetToParentSpace =
						deformedWidgetToWorldSpace * widget.ParentWidget.LocalToWorldTransform.CalcInversed();

				Transform2 widgetResultTransform = widget.CalcApplicableTransfrom2(deformedWidgetToParentSpace);

				SetAnimableProperty.Perform(widget, nameof(Widget.Position), widgetResultTransform.Translation);
				SetAnimableProperty.Perform(widget, nameof(Widget.Rotation), widgetResultTransform.Rotation);
				SetAnimableProperty.Perform(widget, nameof(Widget.Scale), widgetResultTransform.Scale);
			}
		}

		public static bool CalcHullAndPivot(IEnumerable<Widget> widgets, Widget canvas, out Quadrangle hull, out Vector2 pivot)
		{
			Widget first = null;
			var pivotsEqual = true;
			var aabb = Rectangle.Empty;
			pivot = Vector2.Zero;
			hull = new Quadrangle();
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
					pivotsEqual &= widget.CalcPositionInSpaceOf(canvas) == pivot;
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

		public static bool ExtractAssetPathOrShowAlert(string filePath, out string assetPath, out string assetType)
		{
			if (!filePath.StartsWith(Core.Project.Current.AssetsDirectory, StringComparison.CurrentCultureIgnoreCase)) {
				AlertDialog.Show($"Asset '{filePath}' outside the project directory");
				assetPath = null;
				assetType = null;
				return false;
			} else {
				var localPath = filePath.Substring(Core.Project.Current.AssetsDirectory.Length + 1);
				assetPath = System.IO.Path.ChangeExtension(AssetPath.CorrectSlashes(localPath), null);
				assetType = System.IO.Path.GetExtension(localPath).ToLower();
				return true;
			}
		}

	}
}
