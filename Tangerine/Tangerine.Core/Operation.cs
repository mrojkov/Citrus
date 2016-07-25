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
		bool HasModifications { get; }
	}

	public class DelegateOperation : IOperation
	{
		readonly Action @do;
		readonly Action undo;

		public bool HasModifications { get; private set; }

		public DelegateOperation(Action @do, Action undo, bool hasModifications = false)
		{
			this.@do = @do;
			this.undo = undo;
			this.HasModifications = hasModifications;
		}

		public void Do() => @do?.Invoke();
		public void Undo() => undo?.Invoke();
	}

	public class CompoundOperation : IOperation
	{
		readonly List<IOperation> operations = new List<IOperation>();

		public bool HasModifications { get; private set; }

		public void Add(IOperation operation)
		{
			operations.Add(operation);
			HasModifications |= operation.HasModifications;
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

		public bool HasModifications { get; set; }
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
			HasModifications |= operation.HasModifications;
		}

		protected void AddUndoAction(Action undo)
		{
			history.Add(new DelegateOperation(null, undo));
		}
	}
}
