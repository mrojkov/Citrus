using System;
using Lime;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine
{
	public class CreateNodeCommand : Command
	{
		readonly Type type;

		public override string Text => type.Name;
		public override ITexture Icon => NodeIconPool.GetTexture(type);

		public CreateNodeCommand(Type type)
		{
			this.type = type;
		}

		public override void Execute()
		{
			UI.SceneView.SceneView.Instance?.CreateNode(type);
		}
	}
}
