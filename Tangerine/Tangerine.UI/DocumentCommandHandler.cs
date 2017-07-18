using System;
using Lime;

namespace Tangerine.UI
{
	public abstract class DocumentCommandHandler : CommandHandler
	{
		public override void RefreshCommand(ICommand command)
		{
			command.Enabled = Core.Document.Current != null && GetEnabled();
		}

		public virtual bool GetEnabled() => true;
	}
}
