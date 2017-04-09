using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class CreateNodeProcessor : ITaskProvider
	{
		public IEnumerator<object> Task()
		{
			while (true) {
				Type type;
				if (CreateNodeRequestComponent.Consume<Node>(SceneView.Instance.Components, out type)) {
					Core.Operations.CreateNode.Perform(type);
				}
				yield return null;
			}
		}
	}
}
