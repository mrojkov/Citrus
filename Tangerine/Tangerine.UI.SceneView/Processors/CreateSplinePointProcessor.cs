using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	class CreateSplinePointProcessor : ITaskProvider
	{
		SceneView sv => SceneView.Instance;

		public IEnumerator<object> Task()
		{
			while (true) {
				if (CreateNodeRequestComponent.Consume<SplinePoint>(SceneView.Instance.Components)) {
					yield return CreateSplinePointTask();
				}
				yield return null;
			}
		}

		IEnumerator<object> CreateSplinePointTask()
		{
			while (true) {
				if (sv.InputArea.IsMouseOver()) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
				}
				CreateNodeRequestComponent.Consume<Node>(sv.Components);
				if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
					try {
						var currentPoint = (PointObject)Core.Operations.CreateNode.Perform(typeof(SplinePoint), aboveSelected: false);
						var container = (Widget)Document.Current.Container;
						var t = sv.Scene.CalcTransitionToSpaceOf(container);
						var pos = Vector2.Zero;
						if (container.Width.Abs() > Mathf.ZeroTolerance && container.Height.Abs() > Mathf.ZeroTolerance) {
							pos = sv.MousePosition * t / container.Size;
						}
						Core.Operations.SetProperty.Perform(currentPoint, nameof(PointObject.Position), pos);
						Document.Current.History.BeginTransaction();
						while (sv.Input.IsMousePressed()) {
							var dir = (sv.MousePosition * t - currentPoint.TransformedPosition) / SplinePointPresenter.TangentWeightRatio;
							Core.Operations.SetProperty.Perform(currentPoint, nameof(SplinePoint.TangentAngle), dir.Atan2Deg);
							Core.Operations.SetProperty.Perform(currentPoint, nameof(SplinePoint.TangentWeight), dir.Length);
							yield return null;
						}
					} finally {
						Document.Current.History.EndTransaction();
					}
				}
				if (sv.Input.WasMousePressed(1)) {
					break;
				}
				yield return null;
			}
		}
	}
}
