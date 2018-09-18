using System;
using Lime;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine
{
	public class CreateNode : DocumentCommandHandler
	{
		readonly Type type;
		readonly ICommand command;

		public CreateNode(Type type, ICommand command)
		{
			this.type = type;
			this.command = command;
		}

		public override void ExecuteTransaction()
		{
			UI.SceneView.SceneView.Instance?.CreateNode(type, command);
		}

		public override bool GetChecked()
		{
			return command.Checked;
		}
	}
}
