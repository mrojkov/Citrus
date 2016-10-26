using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine.Core
{
	public interface IOperation
	{
		DateTime Timestamp { get; set; }
		bool IsChangingDocument { get; }
	}

	public abstract class Operation : IOperation
	{
		public DateTime Timestamp { get; set; }
		public abstract bool IsChangingDocument { get; }

		readonly Dictionary<Type, object> backup = new Dictionary<Type, object>();

		public void Save<T>(T data)
		{
			backup.Add(typeof(T), data);
		}

		public T Restore<T>()
		{
			var r = backup[typeof(T)];
			backup.Remove(typeof(T));
			return (T)r;
		}
	}

	public abstract class OperationProcessor<T> : IOperationProcessor where T: IOperation
	{
		public void Do(IOperation op)
		{
			if (op is T) {
				InternalDo((T)op);
			}
		}

		public void Undo(IOperation op)
		{
			if (op is T) {
				InternalUndo((T)op);
			}
		}

		protected abstract void InternalDo(T op);
		protected abstract void InternalUndo(T op);
	}

	public interface IOperationProcessor
	{
		void Do(IOperation operation);
		void Undo(IOperation operation);
	}

	public abstract class SymmetricOperationProcessor : IOperationProcessor
	{
		public abstract void Do(IOperation op);
		public void Undo(IOperation op) { Do(op); }
	}
}
