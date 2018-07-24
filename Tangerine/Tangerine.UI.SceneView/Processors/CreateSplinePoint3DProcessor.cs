using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class CreateSplinePoint3DProcessor : ITaskProvider
	{
		ICommand command;

		public IEnumerator<object> Task()
		{
			while (true) {
				if (CreateNodeRequestComponent.Consume<SplinePoint3D>(SceneView.Instance.Components, out command)) {
					yield return CreateSplinePoint3DTask();
				}
				yield return null;
			}
		}

		IEnumerator<object> CreateSplinePoint3DTask()
		{
			var input = SceneView.Instance.Input;
			command.Checked = true;
			while (true) {
				if (SceneView.Instance.InputArea.IsMouseOver()) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
				}
				CreateNodeRequestComponent.Consume<Node>(SceneView.Instance.Components);
				if (SceneView.Instance.Input.ConsumeKeyPress(Key.Mouse0)) {
					SplinePoint3D point;
					try {
						point = (SplinePoint3D)Core.Operations.CreateNode.Perform(typeof(SplinePoint3D), aboveSelected: false);
					} catch (InvalidOperationException e) {
						AlertDialog.Show(e.Message);
						yield break;
					}
					var spline = (Spline3D)Document.Current.Container;
					var vp = spline.Viewport;
					var ray = vp.ScreenPointToRay(SceneView.Instance.Input.MousePosition);
					var xyPlane = new Plane(new Vector3(0, 0, 1), 0).Transform(spline.GlobalTransform);
					var d = ray.Intersects(xyPlane);
					if (d.HasValue) {
						var pos = (ray.Position + ray.Direction * d.Value) * spline.GlobalTransform.CalcInverted();
						Core.Operations.SetProperty.Perform(point, nameof(SplinePoint3D.Position), pos);
						using (Document.Current.History.BeginTransaction()) {
							while (input.IsMousePressed()) {
								Document.Current.History.RollbackTransaction();

								ray = vp.ScreenPointToRay(SceneView.Instance.Input.MousePosition);
								d = ray.Intersects(xyPlane);
								if (d.HasValue) {
									var tangent = (ray.Position + ray.Direction * d.Value) * spline.GlobalTransform.CalcInverted() - point.Position;
									Core.Operations.SetProperty.Perform(point, nameof(SplinePoint3D.TangentA), tangent);
									Core.Operations.SetProperty.Perform(point, nameof(SplinePoint3D.TangentB), -tangent);
								}
								yield return null;
							}
							if (point.TangentA.Length < 0.01f) {
								Core.Operations.SetProperty.Perform(point, nameof(SplinePoint3D.TangentA), new Vector3(1, 0, 0));
								Core.Operations.SetProperty.Perform(point, nameof(SplinePoint3D.TangentB), new Vector3(-1, 0, 0));
							}
							Document.Current.History.CommitTransaction();
						}
					}
				}
				if (SceneView.Instance.Input.WasMousePressed(1) || SceneView.Instance.Input.WasKeyPressed(Key.Escape)) {
					break;
				}
				yield return null;
			}
			this.command.Checked = false;
			Utils.ChangeCursorIfDefault(MouseCursor.Default);
		}
	}
}
