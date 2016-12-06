using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class CreateNodeProcessor : ITaskProvider
	{
		SceneView sv => SceneView.Instance;

		public IEnumerator<object> Task()
		{
			while (true) {
				var c = sv.Components.Get<CreateNodeRequestComponent>();
				if (c != null) {
					sv.Components.Remove<CreateNodeRequestComponent>();
					Core.Operations.CreateNode.Perform(c.NodeType);
				}
				yield return null;
			}
		}
	}
}
