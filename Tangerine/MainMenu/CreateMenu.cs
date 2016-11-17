using System;
using Lime;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine
{
	public class CreateNode : DocumentCommandHandler
	{
		readonly Type type;

		public CreateNode(Type type)
		{
			this.type = type;
		}

		public override void Execute()
		{
			UI.SceneView.SceneView.Instance?.CreateNode(type);
		}
	}
}
