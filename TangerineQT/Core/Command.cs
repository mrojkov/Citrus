using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
	public class Command
	{
		public string Text { get; set; }
		public virtual bool IsDirty() { return true; }
		public virtual void Do() { }
		public virtual void Undo() { }
	}

	public class CompoundCommand : Command
	{
		List<Command> commands = new List<Command>();

		public void Add(Command command)
		{
			commands.Add(command);
		}

		public override bool IsDirty()
		{
			foreach (var command in commands) {
				if (command.IsDirty()) {
					return true;
				}
			}
			return false;
		}

		public override void Do()
		{
			foreach (var command in commands) {
				command.Do();
			}
		}

		public override void Undo()
		{
			foreach (var command in commands.ToArray().Reverse()) {
				command.Undo();
			}
		}
	}
}
