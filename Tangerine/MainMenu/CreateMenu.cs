using System;
using Lime;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine
{
	public class CreateNodeCommand : Command
	{
		readonly Type type;
		readonly string text;
		readonly ITexture icon;

		public override string Text => text;
		public override ITexture Icon => icon;

		public CreateNodeCommand(Type type)
		{
			this.type = type;
			this.text = type.Name;
			this.icon = NodeIconPool.GetTexture(type);
		}

		public override void Execute()
		{
			UI.SceneView.SceneView.Instance?.CreateNode(type);
		}
	}
}
