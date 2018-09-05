using System;
using System.Collections.Generic;

namespace Tangerine.Core
{
	public interface IOperation
	{
		int TransactionId { get; set; }
		bool IsChangingDocument { get; }
		bool Performed { get; set; }
	}

	public abstract class Operation : IOperation
	{
		public int TransactionId { get; set; }
		public abstract bool IsChangingDocument { get; }
		public bool Performed { get; set; }

		private readonly List<object> backup = new List<object>();

		public void Save<T>(T data)
		{
			backup.Add(data);
		}

		public T Restore<T>()
		{
			foreach (var i in backup) {
				if (i is T ti) {
					backup.Remove(ti);
					return ti;
				}
			}
			throw new InvalidOperationException();
		}

		public T Peek<T>()
		{
			if (!Find(out T res)) {
				throw new InvalidOperationException();
			}
			return res;
		}

		public bool Find<T>(out T result)
		{
			foreach (var i in backup) {
				if (i is T ti) {
					result = ti;
					return true;
				}
			}
			result = default;
			return false;
		}
	}

	public interface IOperationProcessor
	{
		void Do(IOperation operation);
		void Undo(IOperation operation);
		void Redo(IOperation operation);
	}

	public abstract class OperationProcessor<TOperation> : IOperationProcessor where TOperation: IOperation
	{
		public void Do(IOperation op) => InternalDo((TOperation)op);
		public void Redo(IOperation op) => InternalRedo((TOperation)op);
		public void Undo(IOperation op) => InternalUndo((TOperation)op);
		protected virtual void InternalDo(TOperation op) => InternalRedo(op);
		protected abstract void InternalRedo(TOperation op);
		protected abstract void InternalUndo(TOperation op);
	}

	public abstract class SymmetricOperationProcessor : IOperationProcessor
	{
		public void Do(IOperation op) => Process(op);
		public void Undo(IOperation op) => Process(op);
		public void Redo(IOperation op) => Process(op);
		public abstract void Process(IOperation op);
	}
}
