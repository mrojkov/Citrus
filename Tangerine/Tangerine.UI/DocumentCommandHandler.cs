using System;
using Lime;

namespace Tangerine.UI
{
	public abstract class DocumentCommandHandler : CommandHandler
	{
		public override void RefreshCommand(ICommand command)
		{
			command.Enabled = Core.Document.Current != null && GetEnabled();
			command.Checked = Core.Document.Current != null && GetChecked();
		}

		public virtual bool GetEnabled() => true;
		public virtual bool GetChecked() => false;
	}
}
