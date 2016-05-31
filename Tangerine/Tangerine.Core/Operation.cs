using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine.Core
{
	public interface IOperation
	{
		void Do();
		void Undo();
	}

	public class DelegateOperation : IOperation
	{
		readonly Action @do;
		readonly Action undo;

		public DelegateOperation(Action @do, Action undo)
		{
			this.@do = @do;
			this.undo = undo;
		}

		public void Do() => @do?.Invoke();
		public void Undo() => undo?.Invoke();
	}

	public class CompoundOperation : IOperation
	{
		List<IOperation> operations = new List<IOperation>();

		public void Add(IOperation operation)
		{
			operations.Add(operation);
		}

		public void Insert(int index, IOperation operation)
		{
			operations.Insert(index, operation);
		}

		public void Do()
		{
			foreach (var operation in operations) {
				operation.Do();
			}
		}

		public void Undo()
		{
			foreach (var operation in Enumerable.Reverse(operations)) {
				operation.Undo();
			}
		}
	}

	public abstract class InteractiveOperation : IOperation
	{
		readonly List<IOperation> history = new List<IOperation>();

		public abstract void Do();

		public void Undo()
		{
			foreach (var operation in Enumerable.Reverse(history)) {
				operation.Undo();
			}
			history.Clear();
		}

		protected void Execute(IOperation operation)
		{
			history.Add(operation);
			operation.Do();
		}

		protected void AddUndoAction(Action undo)
		{
			history.Add(new DelegateOperation(null, undo));
		}
	}
}
