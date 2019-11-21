using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class CreatePointObjectProcessor : ITaskProvider
	{
		SceneView sv => SceneView.Instance;
		private ICommand command;

		public IEnumerator<object> Task()
		{
			while (true) {
				Type nodeType;
				if (CreateNodeRequestComponent.Consume<PointObject>(sv.Components, out nodeType, out command)) {
					yield return CreatePointObjectTask(nodeType);
				}
				yield return null;
			}
		}

		IEnumerator<object> CreatePointObjectTask(Type nodeType)
		{
			command.Checked = true;
			while (true) {
				if (sv.InputArea.IsMouseOver()) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
				}
				CreateNodeRequestComponent.Consume<Node>(sv.Components);
				if (sv.Input.WasMousePressed()) {
					try {
						Document.Current.History.DoTransaction(() => {
							var currentPoint = (PointObject)Core.Operations.CreateNode.Perform(nodeType, aboveSelected: nodeType != typeof(SplinePoint));
							var container = (Widget)Document.Current.Container;
							var t = container.LocalToWorldTransform.CalcInversed();
							var pos = Vector2.Zero;
							if (container.Width.Abs() > Mathf.ZeroTolerance && container.Height.Abs() > Mathf.ZeroTolerance) {
								pos = sv.MousePosition * t / container.Size;
							}
							if (container is ParticleEmitter && currentPoint is EmitterShapePoint) {
								Core.Operations.SetProperty.Perform(
									container,
									nameof(ParticleEmitter.Shape),
									EmitterShape.Custom);
							}
							Core.Operations.SetProperty.Perform(currentPoint, nameof(PointObject.Position), pos);
						});
					} catch (InvalidOperationException e) {
						AlertDialog.Show(e.Message);
						break;
					}
				}
				if (sv.Input.WasMousePressed(1) || sv.Input.WasKeyPressed(Key.Escape)) {
					break;
				}
				yield return null;
			}
			this.command.Checked = false;
		}
	}
}
