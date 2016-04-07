using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.Core
{
	public class DocumentHistory
	{
		int undoPosition;
		List<ICommand> commands = new List<ICommand>();
		ICommand transaction;
		public event Action OnCommit;
		
		public void Add(ICommand command)
		{
			if (transaction == null) {
				transaction = command;
			} else if (transaction is CompoundCommand) {
				(transaction as CompoundCommand).Add(command);
			} else {
				var t = new CompoundCommand();
				t.Add(transaction);
				t.Add(command);
				transaction = t;
			}
		}

		public void Execute(ICommand command)
		{
			Add(command);
			Commit();
		}

		public void Execute(params ICommand[] commands)
		{
			foreach (var c in commands) {
				Add(c);
			}
			Commit();
		}

		public void Commit()
		{
			if (transaction != null) {
				commands.RemoveRange(undoPosition, commands.Count - undoPosition);
				commands.Add(transaction);
				undoPosition = commands.Count;
				transaction.Do();
				transaction = null;
				OnCommit?.Invoke();
			}
		}
		
		public void Undo()
		{
			if (undoPosition > 0) {
				commands[--undoPosition].Undo();
				OnCommit?.Invoke();
			}
		}
		
		public void Redo()
		{
			if (undoPosition < commands.Count) {
				commands[undoPosition++].Do();
				OnCommit?.Invoke();
			}
		}
	}
}
