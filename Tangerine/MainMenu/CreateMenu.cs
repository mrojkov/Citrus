using System;
using Lime;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine
{
	public class CreateNodeCommand : Command
	{
		readonly Type type;

		public CreateNodeCommand(Type type)
		{
			this.type = type;
			Text = type.Name;
			Icon = NodeIconPool.GetTexture(type);
		}

		public override void Execute()
		{
			UI.SceneView.SceneView.Instance?.CreateNode(type);
		}
	}
}
