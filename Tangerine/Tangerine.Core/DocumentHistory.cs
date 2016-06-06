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
		List<IOperation> operations = new List<IOperation>();
		IOperation transaction;
		public event Action OnCommit;
		
		public void Add(IOperation operation)
		{
			if (transaction == null) {
				transaction = operation;
			} else if (transaction is CompoundOperation) {
				(transaction as CompoundOperation).Add(operation);
			} else {
				var t = new CompoundOperation();
				t.Add(transaction);
				t.Add(operation);
				transaction = t;
			}
		}

		public void Execute(IOperation operation)
		{
			Add(operation);
			Commit();
		}

		public void Execute(params IOperation[] operations)
		{
			foreach (var c in operations) {
				Add(c);
			}
			Commit();
		}

		public void Commit()
		{
			if (transaction != null) {
				operations.RemoveRange(undoPosition, operations.Count - undoPosition);
				operations.Add(transaction);
				undoPosition = operations.Count;
				transaction.Do();
				transaction = null;
				OnCommit?.Invoke();
			}
		}
		
		public void Undo()
		{
			if (undoPosition > 0) {
				operations[--undoPosition].Undo();
				OnCommit?.Invoke();
			}
		}
		
		public void Redo()
		{
			if (undoPosition < operations.Count) {
				operations[undoPosition++].Do();
				OnCommit?.Invoke();
			}
		}
	}
}
