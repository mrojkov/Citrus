using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;

namespace Tangerine.UI.SceneView.WidgetTransforms
{
	/// <summary>
	/// All this code is serves to increase ACCURACY of mathematical transformations.
	/// That is why it is uses double numbers and a calculation of optimal transition matrices.
	/// </summary>
	public static class WidgetTransformsHelper
	{
		private const double FloatSignificantDelta = 1e-5;

		public delegate Transform2d CalculateTransformationDelegate(Vector2d originalVectorInObbSpace,
			Vector2d deformedVectorInObbSpace);

		public static void ApplyTransformationToWidgetsGroupObb(Widget sceneWidget, IList<Widget> widgetsInParentSpace,
			Vector2? overridePivotInSceneSpace, bool obbInFirstWidgetSpace,
			Vector2 currentMousePosInSceneSpace, Vector2 previousMousePosSceneSpace,
			bool convertScaleToSize, CalculateTransformationDelegate onCalculateTransformation)
		{
			if (widgetsInParentSpace.Count == 0) return;

			Matrix32d fromSceneToParentSpace = widgetsInParentSpace[0]
				.ParentWidget?.CalcLocalToWorldTransformDouble().CalcInversed() ?? Matrix32d.Identity;

			ApplyTransformationToWidgetsGroupObb(
				widgetsInParentSpace,
				overridePivotInSceneSpace == null
					? (Vector2d?) null
					: ((Vector2d) overridePivotInSceneSpace.Value * fromSceneToParentSpace),
				obbInFirstWidgetSpace,
				(Vector2d) currentMousePosInSceneSpace * fromSceneToParentSpace,
				(Vector2d) previousMousePosSceneSpace * fromSceneToParentSpace,
				convertScaleToSize, onCalculateTransformation);
		}

		public static void ApplyTransformationToWidgetsGroupObb(IList<Widget> widgetsInParentSpace,
			Vector2d? overridePivotInParentSpace, bool obbInFirstWidgetSpace,
			Vector2d currentMousePosInParentSpace, Vector2d previousMousePosInParentSpace,
			bool convertScaleToSize, CalculateTransformationDelegate onCalculateTransformation)
		{
			if (widgetsInParentSpace.Count == 0) return;

			// Importent. Try to correct the pivot point to the closest pivot of one of widgets.
			if (overridePivotInParentSpace != null) {
				foreach (Widget widget in widgetsInParentSpace) {
					double length = ((Vector2d) widget.Position - overridePivotInParentSpace.Value).Length;
					if (length <= 1e-3) {
						overridePivotInParentSpace = (Vector2d) widget.Position;
						break;
					}
				}
			}

			Matrix32d originalObbToParentSpace;
			if (!obbInFirstWidgetSpace) {
				originalObbToParentSpace =
					Matrix32d.Translation(overridePivotInParentSpace ?? (Vector2d) widgetsInParentSpace[0].Position);
			} else {
				Widget widgetFirst = widgetsInParentSpace[0];

				WidgetZeroScalePreserver zeroScalePreserver = new WidgetZeroScalePreserver(widgetFirst);
				zeroScalePreserver.Store();

				// Nullify Pivot and Scale, to simplify transformation matrix from M = mT' * mR * mS * mT, to M = mR * mT,
				// to be able to use it with complex transformation by user (U = uT' uR * uS * uT), as
				// dM = U * M, instead of we must make dM = mT' * uR * mR * uS * mS * uT * mT,
				// and now if mT' = 1 and mS = 1 and uT = 1 it is reduced to dM = uT' * uS * uR * mR * mT = U * M.
				Matrix32d firstWidgetToParentSpace;
				Vector2 savedPivot = widgetFirst.Pivot;
				Vector2 savedScale = widgetFirst.Scale;
				widgetFirst.Pivot = Vector2.Zero;
				widgetFirst.Scale = Vector2.One;
				try {
					firstWidgetToParentSpace = widgetFirst.CalcLocalToParentTransformDouble();
					if (overridePivotInParentSpace != null) {
						firstWidgetToParentSpace.T = overridePivotInParentSpace.Value;
					}
				} finally {
					widgetFirst.Pivot = savedPivot;
					widgetFirst.Scale = savedScale;
					zeroScalePreserver.Restore();
				}

				originalObbToParentSpace = firstWidgetToParentSpace;
			}

			ApplyTransformationToWidgetsGroupObb(
				widgetsInParentSpace, originalObbToParentSpace,
				currentMousePosInParentSpace, previousMousePosInParentSpace,
				convertScaleToSize, onCalculateTransformation);
		}

		public static void ApplyTransformationToWidgetsGroupObb(IEnumerable<Widget> widgetsInParentSpace,
			Matrix32d obbInParentSpace, Vector2d currentMousePosInParentSpace, Vector2d previousMousePosInParentSpace,
			bool convertScaleToSize, CalculateTransformationDelegate onCalculateTransformation)
		{
			if (Math.Abs(obbInParentSpace.CalcDeterminant()) < Mathf.ZeroTolerance) return;

			Matrix32d transformationFromParentToObb = obbInParentSpace.CalcInversed();
			Vector2d controlPointInObbSpace = previousMousePosInParentSpace * transformationFromParentToObb;
			Vector2d targetPointInObbSpace = currentMousePosInParentSpace * transformationFromParentToObb;

			Vector2d originalVectorInObbSpace = controlPointInObbSpace;
			Vector2d deformedVectorInObbSpace = targetPointInObbSpace;

			Transform2d obbTransformation = onCalculateTransformation(
				originalVectorInObbSpace, deformedVectorInObbSpace
			);

			ApplyTransformationToWidgetsGroupObb(
				widgetsInParentSpace, obbInParentSpace, obbTransformation, convertScaleToSize
			);
		}

		public static void ApplyTransformationToWidgetsGroupObb(IEnumerable<Widget> widgetsInParentSpace,
			Matrix32d obbInParentSpace, Transform2d obbTransformation, bool convertScaleToSize)
		{
			Matrix32d originalObbToParentSpace = obbInParentSpace;

			if (Math.Abs(originalObbToParentSpace.CalcDeterminant()) < Mathf.ZeroTolerance) return;

			Matrix32d obbTransformationMatrix = obbTransformation.ToMatrix32();

			foreach (Widget widget in widgetsInParentSpace) {
				WidgetZeroScalePreserver zeroScalePreserver = new WidgetZeroScalePreserver(widget);
				zeroScalePreserver.Store();
				try {

					Matrix32d widgetToParentSpace = widget.CalcLocalToParentTransformDouble();
					Matrix32d widgetToOriginalObbSpace = widgetToParentSpace * originalObbToParentSpace.CalcInversed();

					// Calculate the new obb transformation in the parent space.
					Matrix32d deformedObbToParentSpace = obbTransformationMatrix * originalObbToParentSpace;

					Matrix32d deformedWidgetToParentSpace = widgetToOriginalObbSpace * deformedObbToParentSpace;

					Transform2d widgetResultTransform = widget.ExtractTransform2Double(deformedWidgetToParentSpace,
						widget.Rotation + obbTransformation.Rotation);

					// Correct a rotation delta, to prevent wrong values if a new angle 0 and previous is 359,
					// then rotationDelta must be 1.
					double rotationDelta = Mathd.Wrap180(widgetResultTransform.Rotation - widget.Rotation);

					// Reduce an influence of small transformations (Scale, Position, Rotation).
					bool needChangeScaleX = IsSignificantChangeOfValue(widget.Scale.X, widgetResultTransform.Scale.X);
					bool needChangeScaleY = IsSignificantChangeOfValue(widget.Scale.Y, widgetResultTransform.Scale.Y);

					if (needChangeScaleX || needChangeScaleY) {
						Vector2 useScale = new Vector2(
							(float) (!needChangeScaleX ? widget.Scale.X : widgetResultTransform.Scale.X),
							(float) (!needChangeScaleY ? widget.Scale.Y : widgetResultTransform.Scale.Y)
						);
						useScale = zeroScalePreserver.AdjustToScale(useScale);

						zeroScalePreserver.Restore();

						if (!convertScaleToSize) {
							SetAnimableProperty.Perform(widget, nameof(Widget.Scale), useScale,
								CoreUserPreferences.Instance.AutoKeyframes);
						} else {
							Vector2 useSize = new Vector2(
								Math.Abs(widget.Scale.X) < FloatSignificantDelta
									? widget.Size.X
									: widget.Size.X * useScale.X / widget.Scale.X,
								Math.Abs(widget.Scale.Y) < FloatSignificantDelta
									? widget.Size.Y
									: widget.Size.Y * useScale.Y / widget.Scale.Y
							);
							SetAnimableProperty.Perform(widget, nameof(Widget.Size), useSize,
								CoreUserPreferences.Instance.AutoKeyframes);
						}

					}

					bool needChangePositionX = IsSignificantChangeOfValue(widget.Position.X, widgetResultTransform.Translation.X);
					bool needChangePositionY = IsSignificantChangeOfValue(widget.Position.Y, widgetResultTransform.Translation.Y);

					if (needChangePositionX || needChangePositionY) {
						SetAnimableProperty.Perform(widget, nameof(Widget.Position),
							new Vector2(
								(float) (!needChangePositionX ? widget.Position.X : widgetResultTransform.Translation.X),
								(float) (!needChangePositionY ? widget.Position.Y : widgetResultTransform.Translation.Y)
							),
							CoreUserPreferences.Instance.AutoKeyframes);
					}

					if (IsSignificantChangeByDelta(widget.Rotation, rotationDelta)) {
						SetAnimableProperty.Perform(widget, nameof(Widget.Rotation), (float) (widget.Rotation + rotationDelta),
							CoreUserPreferences.Instance.AutoKeyframes);
					}

				} finally {
					zeroScalePreserver.Restore();
				}
			}
		}

		private static bool IsSignificantChangeOfValue(double valuePrevious, double valueCurrent)
		{
			return IsSignificantChangeByDelta(valuePrevious, valuePrevious - valueCurrent);
		}

		private static bool IsSignificantChangeByDelta(double valuePrevious, double valueDelta)
		{
			return Math.Abs(valueDelta) > FloatSignificantDelta &&
				Math.Abs(valueDelta) / Math.Max(FloatSignificantDelta, Math.Abs(valuePrevious)) > FloatSignificantDelta;
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

		private static Matrix32d CalcLocalToWorldTransformDouble(this Widget widget)
		{
			// Copy of double-precision code of Widget.RecalcGlobalTransformDouble.
			Matrix32d localToParentTransform = widget.CalcLocalToParentTransformDouble();
			var parentWidget = widget.Parent?.AsWidget;
			if (parentWidget == null) {
				return localToParentTransform;
			}
			return localToParentTransform * parentWidget.CalcLocalToWorldTransformDouble();
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
			return localToParentTransform.ToTransform2Double(preferedRotationDeg, (Vector2d) widget.Scale);
		}

		private static Transform2d ToTransform2Double(this Matrix32d matrix, double preferedRotationDeg,
			Vector2d originalScale)
		{
			// Calculate the required signs of the scale axes, excluding rotation from the deformation matrix.   
			Matrix32d matrixWithoutRotation = matrix * Matrix32d.Rotation(-preferedRotationDeg * Math.PI / 180.0);
			int directionClock = Math.Sign(Vector2d.CrossProduct(matrixWithoutRotation.U, matrixWithoutRotation.V));

			Vector2d direction = matrixWithoutRotation.U.Normalized + matrixWithoutRotation.V.Normalized;
			Vector2d directionU = direction * Matrix32d.Rotation(-directionClock * Math.PI / 4);
			Vector2d directionV = direction * Matrix32d.Rotation(directionClock * Math.PI / 4);

			bool isRequiredScaleXNegative = directionU.X < 0;
			bool isRequiredScaleYNegative = directionV.Y < 0;

			var vSign = Math.Sign(Vector2d.CrossProduct(matrix.U, matrix.V));

			Vector2d originalScaleAbs = new Vector2d(Math.Abs(originalScale.X), Math.Abs(originalScale.Y));

			Vector2d useMatrixU = isRequiredScaleXNegative ? -matrix.U : matrix.U;
			Vector2d useMatrixV = isRequiredScaleXNegative ? -matrix.V : matrix.V;
			int useMatrixUSign = isRequiredScaleXNegative ? -1 : 1;
			int useMatrixVSign = isRequiredScaleXNegative ? -1 : 1;

			double rotation;
			if (Math.Abs(Vector2d.DotProduct(useMatrixU, useMatrixV)) < FloatSignificantDelta) {
				rotation = useMatrixU.Atan2Deg;
			} else {
				// Calculate the correct rotation, for deformed not-perpendicular UV axes.
				// Naive realisation as atg(U.X/V.X) or atg(V.Y/U.Y). 
				// To keep an original widget rotation.
				if (Math.Abs(useMatrixU.X) + Math.Abs(useMatrixV.X) > Math.Abs(useMatrixU.Y) + Math.Abs(useMatrixV.Y)) {
					if (originalScaleAbs.X > originalScaleAbs.Y) {
						rotation = new Vector2d(useMatrixU.X * originalScaleAbs.Y / originalScaleAbs.X, -useMatrixV.X * vSign).Atan2Deg;
					} else {
						rotation = new Vector2d(useMatrixU.X, -useMatrixV.X * vSign * originalScaleAbs.X / originalScaleAbs.Y).Atan2Deg;
					}
				} else {
					if (originalScaleAbs.Y > originalScaleAbs.X) {
						rotation = new Vector2d(useMatrixV.Y * vSign * originalScaleAbs.X / originalScaleAbs.Y, useMatrixU.Y).Atan2Deg;
					} else {
						rotation = new Vector2d(useMatrixV.Y * vSign, useMatrixU.Y * originalScaleAbs.Y / originalScaleAbs.X).Atan2Deg;
					}
				}
			}

			return new Transform2d {
				Translation = matrix.T,
				Scale = new Vector2d(useMatrixUSign * useMatrixU.Length, useMatrixVSign * useMatrixV.Length * vSign),
				Rotation = rotation
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
