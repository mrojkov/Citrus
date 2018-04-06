using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;

namespace Tangerine.UI.SceneView.ComplexTransforms
{
	/// <summary>
	/// All this code is serves to increase ACCURACY of mathematical transformations.
	/// That is why it is uses double numbers and a calculation of optimal transition matrices.
	/// </summary>
	public static class ComplexTransformationsHelper
	{

		public delegate Matrix32d CalculateTransformationDelegate(Vector2d fromOriginalVectorInObbSpace,
			Vector2d toDeformedVectorInObbSpace, out double obbTransformationRotationDeg);

		public static void ApplyTransformationToWidgetsGroupObb(Widget sceneWidget, IList<Widget> widgetsInParentSpace,
			Vector2 pivotInSceneSpace, bool obbInFirstWidgetSpace,
			Vector2 currentMousePosInSceneSpace, Vector2 previousMousePosSceneSpace,
			CalculateTransformationDelegate onCalculateTransformation)
		{
			if (widgetsInParentSpace.Count == 0) return;

			Matrix32d fromSceneToParentSpace = widgetsInParentSpace[0].ParentWidget
				.CalcEffectiveTransformToTopWidgetDouble(sceneWidget).CalcInversed();

			ApplyTransformationToWidgetsGroupObb(
				widgetsInParentSpace,
				(Vector2d) pivotInSceneSpace * fromSceneToParentSpace,
				obbInFirstWidgetSpace,
				(Vector2d) currentMousePosInSceneSpace * fromSceneToParentSpace,
				(Vector2d) previousMousePosSceneSpace * fromSceneToParentSpace,
				onCalculateTransformation
			);
		}

		public static void ApplyTransformationToWidgetsGroupObb(IList<Widget> widgetsInParentSpace,
			Vector2d pivotInParentSpace, bool obbInFirstWidgetSpace,
			Vector2d currentMousePosInParentSpace, Vector2d previousMousePosInParentSpace,
			CalculateTransformationDelegate onCalculateTransformation)
		{
			if (widgetsInParentSpace.Count == 0) return;

			// Importent. Try to correct the pivot point to the closest pivot of one of widgets.
			if (!obbInFirstWidgetSpace) {
				foreach (Widget widget in widgetsInParentSpace) {
					double length = ((Vector2d) widget.Position - pivotInParentSpace).Length;
					if (length <= 1e-3) {
						pivotInParentSpace = (Vector2d) widget.Position;
						break;
					}
				}
			}

			Matrix32d originalObbToParentSpace = Matrix32d.Translation(pivotInParentSpace);
			if (obbInFirstWidgetSpace) {
				Widget widgetFirst = widgetsInParentSpace[0];

				WidgetZeroScalePreserver zeroScalePreserver = new WidgetZeroScalePreserver(widgetFirst);
				zeroScalePreserver.Store();

				Matrix32d firstWidgetToParentSpace;
				Vector2 savedPivot = widgetFirst.Pivot;
				Vector2 savedScale = widgetFirst.Scale;
				widgetFirst.Pivot = Vector2.Zero;
				widgetFirst.Scale = Vector2.One;
				try {
					firstWidgetToParentSpace = widgetFirst.CalcLocalToParentTransformDouble();
				} finally {
					widgetFirst.Pivot = savedPivot;
					widgetFirst.Scale = savedScale;
					zeroScalePreserver.Restore();
				}

				originalObbToParentSpace = firstWidgetToParentSpace;
			}

			ApplyTransformationToWidgetsGroupObb(
				widgetsInParentSpace, originalObbToParentSpace,
				currentMousePosInParentSpace, previousMousePosInParentSpace, onCalculateTransformation
			);
		}

		public static void ApplyTransformationToWidgetsGroupObb(IEnumerable<Widget> widgetsInParentSpace,
			Matrix32d obbInParentSpace, Vector2d currentMousePosInParentSpace, Vector2d previousMousePosInParentSpace,
			CalculateTransformationDelegate onCalculateTransformation)
		{
			if (Math.Abs(obbInParentSpace.CalcDeterminant()) < Mathf.ZeroTolerance) return;

			Matrix32d transformationFromParentToObb = obbInParentSpace.CalcInversed();
			Vector2d controlPointInObbSpace = previousMousePosInParentSpace * transformationFromParentToObb;
			Vector2d targetPointInObbSpace = currentMousePosInParentSpace * transformationFromParentToObb;

			Vector2d originalVectorInObbSpace = controlPointInObbSpace;
			Vector2d deformedVectorInObbSpace = targetPointInObbSpace;

			double obbTransformationRotationDeg;
			Matrix32d deformationInObbSpace = onCalculateTransformation(
				originalVectorInObbSpace,
				deformedVectorInObbSpace,
				out obbTransformationRotationDeg
			);

			ApplyTransformationToWidgetsGroupObb(
				widgetsInParentSpace, obbInParentSpace, deformationInObbSpace, obbTransformationRotationDeg
			);
		}

		public static void ApplyTransformationToWidgetsGroupObb(IEnumerable<Widget> widgetsInParentSpace,
			Matrix32d obbInParentSpace, Matrix32d obbTransformation, double obbTransformationRotationDeg)
		{
			Matrix32d originalObbToParentSpace = obbInParentSpace;

			if (Math.Abs(originalObbToParentSpace.CalcDeterminant()) < Mathf.ZeroTolerance) return;

			foreach (Widget widget in widgetsInParentSpace) {
				WidgetZeroScalePreserver zeroScalePreserver = new WidgetZeroScalePreserver(widget);
				zeroScalePreserver.Store();
				try {

					Matrix32d widgetToParentSpace = widget.CalcLocalToParentTransformDouble();
					Matrix32d widgetToOriginalObbSpace = widgetToParentSpace * originalObbToParentSpace.CalcInversed();

					// Calculate the new obb transformation in the parent space.
					Matrix32d deformedObbToParentSpace = obbTransformation * originalObbToParentSpace;

					Matrix32d deformedWidgetToParentSpace = widgetToOriginalObbSpace * deformedObbToParentSpace;

					Transform2d widgetResultTransform = widget.ExtractTransform2Double(deformedWidgetToParentSpace,
						widget.Rotation + obbTransformationRotationDeg);

					// Correct a rotation delta, to prevent wrong values if a new angle 0 and previous is 359,
					// then rotationDelta must be 1.
					double rotationDelta = Mathd.Wrap180(widgetResultTransform.Rotation - widget.Rotation);

					// Reduce an influence of small transformations (Scale, Position, Rotation).
					bool needChangeScaleX = Math.Abs(widget.Scale.X - widgetResultTransform.Scale.X) > 1e-5 &&
						Math.Abs(widget.Scale.X - widgetResultTransform.Scale.X) /
						Math.Max(1e-5, Math.Abs((double) widget.Scale.X)) > 1e-5;
					bool needChangeScaleY = Math.Abs(widget.Scale.Y - widgetResultTransform.Scale.Y) > 1e-5 &&
						Math.Abs(widget.Scale.Y - widgetResultTransform.Scale.Y) /
						Math.Max(1e-5, Math.Abs((double) widget.Scale.Y)) > 1e-5;

					if (needChangeScaleX || needChangeScaleY) {
						Vector2 useScale = new Vector2(
							(float) (!needChangeScaleX ? widget.Scale.X : widgetResultTransform.Scale.X),
							(float) (!needChangeScaleY ? widget.Scale.Y : widgetResultTransform.Scale.Y)
						);
						useScale = zeroScalePreserver.AdjustToScale(useScale);

						zeroScalePreserver.Restore();

						SetAnimableProperty.Perform(widget, nameof(Widget.Scale), useScale,
							CoreUserPreferences.Instance.AutoKeyframes);
					}

					bool needChangePositionX = Math.Abs(widget.Position.X - widgetResultTransform.Translation.X) > 1e-5 &&
						Math.Abs(widget.Position.X - widgetResultTransform.Translation.X) /
						Math.Max(1e-5, Math.Abs((double) widget.Position.X)) > 1e-5;
					bool needChangePositionY = Math.Abs(widget.Position.Y - widgetResultTransform.Translation.Y) > 1e-5 &&
						Math.Abs(widget.Position.Y - widgetResultTransform.Translation.Y) /
						Math.Max(1e-5, Math.Abs((double) widget.Position.Y)) > 1e-5;

					if (needChangePositionX || needChangePositionY) {
						SetAnimableProperty.Perform(widget, nameof(Widget.Position),
							new Vector2(
								(float) (!needChangePositionX ? widget.Position.X : widgetResultTransform.Translation.X),
								(float) (!needChangePositionY ? widget.Position.Y : widgetResultTransform.Translation.Y)
							),
							CoreUserPreferences.Instance.AutoKeyframes);
					}

					if (
						Math.Abs(rotationDelta) > 1e-5 &&
						Math.Abs(rotationDelta) / Math.Max(1e-5, Math.Abs((double) widget.Rotation)) > 1e-5
					) {
						SetAnimableProperty.Perform(widget, nameof(Widget.Rotation), (float) (widget.Rotation + rotationDelta),
							CoreUserPreferences.Instance.AutoKeyframes);
					}

				} finally {
					zeroScalePreserver.Restore();
				}
			}
		}

		private static Matrix32d CalcLocalToParentTransformDouble(this Widget widget)
		{
			// Copy of double-precision code of Widget.RecalcLocalToParentTransform.
			Matrix32d localToParentTransform =
				Matrix32d.Translation(-(Vector2d) (widget.Pivot * widget.Size)) *
				Matrix32d.Transformation(
					Vector2d.Zero,
					(Vector2d) widget.Scale,
					widget.Rotation * Math.PI / 180.0,
					(Vector2d) widget.Position
				);
			if (widget.SkinningWeights != null && widget.Parent?.AsWidget != null) {
				localToParentTransform = localToParentTransform *
					(Matrix32d) widget.Parent.AsWidget.BoneArray.CalcWeightedRelativeTransform(widget.SkinningWeights);
			}
			return localToParentTransform;
		}

		private static Matrix32d CalcEffectiveTransformToTopWidgetDouble(this Widget widget, Widget untilWidget)
		{
			// Copy of double-precision code of Widget.RecalcGlobalTransformDouble.
			Matrix32d localToParentTransform = widget.CalcLocalToParentTransformDouble();
			var parentWidget = widget.Parent?.AsWidget;
			if (parentWidget == null || parentWidget == untilWidget) {
				return localToParentTransform;
			}
			return localToParentTransform * parentWidget.CalcEffectiveTransformToTopWidgetDouble(untilWidget);
		}

		private static Transform2d ExtractTransform2Double(this Widget widget, Matrix32d localToParentTransform,
			double preferedRotationDeg)
		{
			// Take pivot into account.
			localToParentTransform = Matrix32d.Translation(-(Vector2d) (widget.Pivot * widget.Size)).CalcInversed() *
				localToParentTransform;

			// Take SkinningWeights into account.
			if (widget.SkinningWeights != null && widget.Parent?.AsWidget != null) {
				localToParentTransform = localToParentTransform *
					((Matrix32d) widget.Parent.AsWidget.BoneArray.CalcWeightedRelativeTransform(widget.SkinningWeights))
					.CalcInversed();
			}

			// Extract simple transformations from matrix.
			return localToParentTransform.ToTransform2Double(preferedRotationDeg);
		}

		private static Transform2d ToTransform2Double(this Matrix32d matrix, double preferedRotationDeg)
		{
			Matrix32d matrixWithoutRotation = matrix * Matrix32d.Rotation(-preferedRotationDeg * Math.PI / 180.0);
			int directionClock = Math.Sign(Vector2d.CrossProduct(matrixWithoutRotation.U, matrixWithoutRotation.V));

			Vector2d direction = matrixWithoutRotation.U.Normalized + matrixWithoutRotation.V.Normalized;
			Vector2d directionU = direction * Matrix32d.Rotation(-directionClock * Math.PI / 4);
			Vector2d directionV = direction * Matrix32d.Rotation(directionClock * Math.PI / 4);

			bool isRequiredScaleXNegative = directionU.X < 0;
			bool isRequiredScaleYNegative = directionV.Y < 0;

			var vSign = Math.Sign(Vector2d.CrossProduct(matrix.U, matrix.V));

			if (!isRequiredScaleXNegative) {
				return new Transform2d {
					Translation = matrix.T,
					Scale = new Vector2d(matrix.U.Length, matrix.V.Length * vSign),
					Rotation = matrix.U.Atan2Deg
				};
			}

			if ((isRequiredScaleYNegative && vSign < 0) || (!isRequiredScaleYNegative && vSign > 0)) {
				return new Transform2d {
					Translation = matrix.T,
					Scale = new Vector2d(-matrix.U.Length, matrix.V.Length * vSign),
					Rotation = matrix.U.Atan2Deg
				};
			}

			return new Transform2d {
				Translation = matrix.T,
				Scale = new Vector2d(-matrix.U.Length, -matrix.V.Length * vSign),
				Rotation = (-matrix.U).Atan2Deg
			};
		}

		public static Vector2d Snap(this Vector2d value, Vector2d origin, double distanceTolerance = 0.001f)
		{
			return (value - origin).Length > distanceTolerance ? value : origin;
		}

		public static double RoundTo(double value, double step)
		{
			return (value / step).Round() * step;
		}
	}
}
