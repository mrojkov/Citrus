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
		public delegate Matrix32 CalculateTransformationDelegate(Vector2 fromOriginalVectorInObbSpace, Vector2 toDeformedVectorInObbSpace);
		
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

		public static void ApplyTransformationToWidgetsGroupObb(Widget sceneWidget, IList<Widget> widgetsInParentSpace,
			Vector2 pivotInSceneSpace, bool obbInFirstWidgetSpace,
			Vector2 currentMousePosInSceneSpace, Vector2 previousMousePosSceneSpace,
			CalculateTransformationDelegate onCalculateTransformation)
		{
			if (widgetsInParentSpace.Count == 0) return;

			Matrix32 fromSceneToParentSpace = sceneWidget.CalcTransitionToSpaceOf(widgetsInParentSpace[0].ParentWidget);

			ApplyTransformationToWidgetsGroupObb(
				widgetsInParentSpace, 
				pivotInSceneSpace * fromSceneToParentSpace,
				obbInFirstWidgetSpace,
				currentMousePosInSceneSpace * fromSceneToParentSpace,
				previousMousePosSceneSpace * fromSceneToParentSpace, 
				onCalculateTransformation
			);
		}

		public static void ApplyTransformationToWidgetsGroupObb(IList<Widget> widgetsInParentSpace,
			Vector2 pivotInParentSpace, bool obbInFirstWidgetSpace,
			Vector2 currentMousePosInParentSpace, Vector2 previousMousePosInParentSpace,
			CalculateTransformationDelegate onCalculateTransformation)
		{
			if (widgetsInParentSpace.Count == 0) return;

				Matrix32 originalObbToParentSpace = Matrix32.Translation(pivotInParentSpace);
				if (obbInFirstWidgetSpace) {
					WidgetZeroScalePreserver zeroScalePreserver = new WidgetZeroScalePreserver(widgetsInParentSpace[0]);
					zeroScalePreserver.Store();
					
					Matrix32 firstWidgetToParentSpace;
					try {
						firstWidgetToParentSpace = widgetsInParentSpace[0].CalcLocalToParentTransform();
					} finally {
						zeroScalePreserver.Restore();
					}
					
					originalObbToParentSpace = firstWidgetToParentSpace *
						Matrix32.Translation(firstWidgetToParentSpace.T).CalcInversed() * Matrix32.Translation(pivotInParentSpace);
				}

				ApplyTransformationToWidgetsGroupObb(
					widgetsInParentSpace, widgetsInParentSpace[0].ParentWidget, originalObbToParentSpace, 
					currentMousePosInParentSpace, previousMousePosInParentSpace, onCalculateTransformation
				);

		}

		public static void ApplyTransformationToWidgetsGroupObb(IEnumerable<Widget> widgetsInParentSpace,
			Widget parentWidget, Matrix32 obbInParentSpace,
			Vector2 currentMousePosInParentSpace, Vector2 previousMousePosInParentSpace,
			CalculateTransformationDelegate onCalculateTransformation)
		{
			Vector2 pivotInParentSpace = obbInParentSpace.T;

			Vector2 pivotInObbSpace = pivotInParentSpace;
			Vector2 controlPointInObbSpace = previousMousePosInParentSpace;
			Vector2 targetPointInObbSpace = currentMousePosInParentSpace;

			if (Math.Abs(obbInParentSpace.CalcDeterminant()) < Mathf.ZeroTolerance) return;
			
			Matrix32 transformationFromParentToObb = obbInParentSpace.CalcInversed();
			pivotInObbSpace = pivotInObbSpace * transformationFromParentToObb;
			controlPointInObbSpace = controlPointInObbSpace * transformationFromParentToObb;
			targetPointInObbSpace = targetPointInObbSpace * transformationFromParentToObb;

			Vector2 originalVectorInObbSpace = controlPointInObbSpace - pivotInObbSpace;
			Vector2 deformedVectorInObbSpace = targetPointInObbSpace - pivotInObbSpace;

			// calculate transformation from original vector to deformed vector
			Matrix32 deformationInObbSpace =
					onCalculateTransformation(originalVectorInObbSpace,
						deformedVectorInObbSpace);

			ApplyTransformationToWidgetsGroupObb(
				widgetsInParentSpace, parentWidget, obbInParentSpace, deformationInObbSpace
			);
		}

		public static void ApplyTransformationToWidgetsGroupObb(IEnumerable<Widget> widgetsInParentSpace,
			Widget parentWidget, Matrix32 obbInParentSpace, Matrix32 obbTransformation)
		{
			Matrix32 originalObbToWorldSpace = obbInParentSpace * parentWidget.LocalToWorldTransform;

			if (Math.Abs(originalObbToWorldSpace.CalcDeterminant()) < Mathf.ZeroTolerance) return;
			
			foreach (Widget widget in widgetsInParentSpace) {
				WidgetZeroScalePreserver zeroScalePreserver = new WidgetZeroScalePreserver(widget);
				zeroScalePreserver.Store();
				try {

					Matrix32 widgetToOriginalObbSpace = widget.LocalToWorldTransform * originalObbToWorldSpace.CalcInversed();

					// Calculate the new obb transformation in the world space.
					Matrix32 deformedObbToWorldSpace = obbTransformation * originalObbToWorldSpace;

					Matrix32 deformedWidgetToWorldSpace = widgetToOriginalObbSpace * deformedObbToWorldSpace;

					if (Math.Abs(widget.ParentWidget.LocalToWorldTransform.CalcDeterminant()) < Mathf.ZeroTolerance) continue;
					
					Matrix32 deformedWidgetToParentSpace =
						deformedWidgetToWorldSpace * widget.ParentWidget.LocalToWorldTransform.CalcInversed();

					Transform2 widgetResultTransform = widget.ExtractTransform2(deformedWidgetToParentSpace);

					// Correct a rotation delta, to prevent wrong values if a new angle 0 and previous is 359,
					// then rotationDelta must be 1.
					float rotationDelta = Mathf.Wrap180(widget.Rotation - widgetResultTransform.Rotation);

					// The position is less prone to fluctuations than other properties.
					if ((widget.Position - widgetResultTransform.Translation).Length > 15e-5f) {
						SetAnimableProperty.Perform(widget, nameof(Widget.Position), widgetResultTransform.Translation, CoreUserPreferences.Instance.AutoKeyframes);
					}
					if (Mathf.Abs(rotationDelta) > Mathf.ZeroTolerance) {
						SetAnimableProperty.Perform(widget, nameof(Widget.Rotation), widget.Rotation + rotationDelta, CoreUserPreferences.Instance.AutoKeyframes);
					}
					if ((widget.Scale - widgetResultTransform.Scale).Length > Mathf.ZeroTolerance) {
						widgetResultTransform.Scale = zeroScalePreserver.AdjustToScale(widgetResultTransform.Scale);

						zeroScalePreserver.Restore();

						SetAnimableProperty.Perform(widget, nameof(Widget.Scale), widgetResultTransform.Scale, CoreUserPreferences.Instance.AutoKeyframes);
					}
					
				} finally {
					zeroScalePreserver.Restore();
				}
			}
		}

		private static Transform2 ExtractTransform2(this Widget widget, Matrix32 localToParentTransform)
		{
			// Take pivot into account.
			localToParentTransform = Matrix32.Translation(-widget.Pivot * widget.Size).CalcInversed() * localToParentTransform;
			
			// Take SkinningWeights into account.
			if (widget.SkinningWeights != null && widget.Parent?.AsWidget != null) {
				localToParentTransform = localToParentTransform * widget.Parent.AsWidget.BoneArray.CalcWeightedRelativeTransform(widget.SkinningWeights).CalcInversed();
			}

			// Extract simple transformations from matrix.
			return localToParentTransform.ToTransform2();
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
		
		private class WidgetZeroScalePreserver
		{

			private readonly Widget widget;
			private float? savedScaleX;
			private float? savedScaleY;

			internal WidgetZeroScalePreserver(Widget widget)
			{
				this.widget = widget;
			}

			internal Vector2 AdjustToScale(Vector2 scale)
			{
				if (savedScaleX == null && savedScaleY == null) return scale;

				return new Vector2(savedScaleX ?? scale.X, savedScaleY ?? scale.Y);
			}
			
			internal void Store()
			{
				Restore();

				if (Math.Abs(widget.Scale.X) < Mathf.ZeroTolerance) {
					savedScaleX = widget.Scale.X;
					widget.Scale = new Vector2(1, widget.Scale.Y);
				}
				if (Math.Abs(widget.Scale.Y) < Mathf.ZeroTolerance) {
					savedScaleY = widget.Scale.Y;
					widget.Scale = new Vector2(widget.Scale.X, 1);
				}
			}

			internal void Restore()
			{
				if (widget != null && savedScaleX != null) {
					widget.Scale = new Vector2(savedScaleX.Value, widget.Scale.Y);
				}
				if (widget != null && savedScaleY != null) {
					widget.Scale = new Vector2(widget.Scale.X, savedScaleY.Value);
				}
				savedScaleX = null;
				savedScaleY = null;
			}

		}
		
	}
}
