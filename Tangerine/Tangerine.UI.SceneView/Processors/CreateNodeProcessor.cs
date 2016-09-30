using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class CreateNodeProcessor : IProcessor
	{
		SceneView sv => SceneView.Instance;

		public IEnumerator<object> Loop()
		{
			while (true) {
				var c = sv.Components.Get<CreateNodeRequestComponent>();
				if (c != null) {
					sv.Components.Remove<CreateNodeRequestComponent>();
					Core.Operations.CreateNode.Perform(Document.Current.Container, 0, c.NodeType);
				}
				yield return null;
			}
		}
	}
}
