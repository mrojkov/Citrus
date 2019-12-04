using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class DragPointObjectsProcessor : ITaskProvider
	{
		static SceneView sv => SceneView.Instance;

		public IEnumerator<object> Task()
		{
			while (true) {
				if (!SceneView.Instance.InputArea.IsMouseOverThisOrDescendant()) {
					yield return null;
					continue;
				}
				var pointObjects = Document.Current.SelectedNodes().Editable().OfType<PointObject>();
				if (Utils.CalcHullAndPivot(pointObjects, out var hull, out _)) {
					if (sv.HitTestControlPoint((hull.V1 + hull.V3) / 2f)) {
						Utils.ChangeCursorIfDefault(MouseCursor.Hand);
						if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
							yield return Drag(pointObjects.ToList());
						}
					}
				}
				yield return null;
			}
		}

		enum DragDirection
		{
			Any, Horizontal, Vertical
		}

		IEnumerator<object> Drag(List<PointObject> pobjects)
		{
			using (Document.Current.History.BeginTransaction()) {
				var iniMousePos = sv.MousePosition;
				var transform = Document.Current.Container.AsWidget.LocalToWorldTransform.CalcInversed();
				var dragDirection = DragDirection.Any;
				while (sv.Input.IsMousePressed()) {
					Document.Current.History.RollbackTransaction();

					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					var curMousePos = sv.MousePosition;
					var shiftPressed = sv.Input.IsKeyPressed(Key.Shift);
					if (shiftPressed && dragDirection != DragDirection.Any) {
						if (dragDirection == DragDirection.Horizontal) {
							curMousePos.Y = iniMousePos.Y;
						} else if (dragDirection == DragDirection.Vertical) {
							curMousePos.X = iniMousePos.X;
						}
					}
					var dragDelta = curMousePos * transform - iniMousePos * transform;
					if (shiftPressed && dragDirection == DragDirection.Any && (curMousePos - iniMousePos).Length > 5 / sv.Scene.Scale.X) {
						var d = curMousePos - iniMousePos;
						dragDirection = d.X.Abs() > d.Y.Abs() ? DragDirection.Horizontal : DragDirection.Vertical;
					}
					dragDelta = dragDelta.Snap(Vector2.Zero);
					if (dragDelta != Vector2.Zero) {
						for (int i = 0; i < pobjects.Count; i++) {
							var parent = pobjects[i].Parent?.AsWidget;
							if (parent != null &&
								parent.Size.X != 0 &&
								parent.Size.Y != 0
							) {
								var boneArray = parent.ParentWidget.BoneArray;
								var localToParent = parent.CalcLocalToParentTransform();
								var skinningRelativeInversedTransform = boneArray.CalcWeightedRelativeTransform(pobjects[i].SkinningWeights).CalcInversed();
								var newPosition = ((pobjects[i].TransformedPosition + dragDelta) * localToParent
												   * skinningRelativeInversedTransform * localToParent.CalcInversed() - pobjects[i].Offset) / parent.Size;
								SetPosition(pobjects[i], newPosition);
								if (sv.Input.IsKeyPressed(Key.Control) && pobjects[i] is DistortionMeshPoint) {
									var p = pobjects[i] as DistortionMeshPoint;
									SetUV(p, p.UV + dragDelta / parent.Size);
								}
							}
						}
					}
					yield return null;
				}
				sv.Input.ConsumeKey(Key.Mouse0);
				Document.Current.History.CommitTransaction();
			}
		}

		private void SetUV(DistortionMeshPoint pointObject, Vector2 value)
		{
			Core.Operations.SetAnimableProperty.Perform(pointObject, nameof(DistortionMeshPoint.UV), value, CoreUserPreferences.Instance.AutoKeyframes);
		}

		public static void SetPosition(PointObject po, Vector2 value)
		{
			Core.Operations.SetAnimableProperty.Perform(po, nameof(PointObject.Position), value, CoreUserPreferences.Instance.AutoKeyframes);
		}
	}
}
