using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class CreatePointObjectProcessor : ITaskProvider
	{
		SceneView sv => SceneView.Instance;

		public IEnumerator<object> Task()
		{
			while (true) {
				Type nodeType = null;
				if (ConsumeCreateNodeRequest(ref nodeType)) {
					yield return CreatePointObjectTask(nodeType);
				}
				yield return null;
			}
		}

		IEnumerator<object> CreatePointObjectTask(Type nodeType)
		{
			while (true) {
				if (sv.InputArea.IsMouseOver()) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
				}
				ConsumeCreateNodeRequest(ref nodeType);
				if (sv.Input.WasMousePressed()) {
					var pointObject = (PointObject)Core.Operations.CreateNode.Perform(nodeType);
					var container = (Widget)Document.Current.Container;
					var t = sv.Scene.CalcTransitionToSpaceOf(container);
					var pos = Vector2.Zero;
					if (container.Width.Abs() > Mathf.ZeroTolerance && container.Height.Abs() > Mathf.ZeroTolerance) {
						pos = sv.MousePosition * t / container.Size;
					}
					Core.Operations.SetProperty.Perform(pointObject, nameof(PointObject.Position), pos);
				}
				if (sv.Input.WasMousePressed(1)) {
					break;
				}
				yield return null;
			}
		}

		bool ConsumeCreateNodeRequest(ref Type nodeType)
		{
			var c = sv.Components.Get<CreateNodeRequestComponent>();
			if (c != null && c.NodeType.IsSubclassOf(typeof(PointObject))) {
				sv.Components.Remove<CreateNodeRequestComponent>();
				nodeType = c.NodeType;
				return true;
			}
			return false;
		}
	}
}