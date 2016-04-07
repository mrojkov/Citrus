using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine.Core
{
	public interface ICommand
	{
		void Do();
		void Undo();
	}

	public class ActionCommand : ICommand
	{
		readonly Action @do;
		readonly Action undo;

		public ActionCommand(Action @do, Action undo)
		{
			this.@do = @do;
			this.undo = undo;
		}

		public void Do() => @do?.Invoke();
		public void Undo() => undo?.Invoke();
	}

	public class CompoundCommand : ICommand
	{
		List<ICommand> commands = new List<ICommand>();

		public void Add(ICommand command)
		{
			commands.Add(command);
		}

		public void Insert(int index, ICommand command)
		{
			commands.Insert(index, command);
		}

		public void Do()
		{
			foreach (var command in commands) {
				command.Do();
			}
		}

		public void Undo()
		{
			foreach (var command in Enumerable.Reverse(commands)) {
				command.Undo();
			}
		}
	}

	public abstract class InteractiveCommand : ICommand
	{
		readonly List<ICommand> history = new List<ICommand>();

		public abstract void Do();

		public void Undo()
		{
			foreach (var command in Enumerable.Reverse(history)) {
				command.Undo();
			}
			history.Clear();
		}

		protected void Execute(ICommand command)
		{
			history.Add(command);
			command.Do();
		}

		protected void AddUndoAction(Action undo)
		{
			history.Add(new ActionCommand(null, undo));
		}
	}
}
