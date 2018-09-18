using System;
using System.Collections.Generic;
using System.Linq;
using Lime;

namespace Tangerine.Core
{
	public interface ITransactionalHistory
	{
		IDisposable BeginTransaction();
		void RollbackTransaction();
		void CommitTransaction();
		void EndTransaction();
	}

	public class DocumentHistory : ITransactionalHistory
	{
		private readonly Stack<int> transactionStartIndices = new Stack<int>();
		private readonly List<IOperation> operations = new List<IOperation>();
		private int transactionId;
		private int saveIndex;
		private int currentIndex;

		public static DocumentHistory Current { get; private set; }

		public bool CanUndo() => !IsTransactionActive && currentIndex > 0;
		public bool CanRedo() => !IsTransactionActive && currentIndex < operations.Count;
		public bool IsDocumentModified { get; private set; }
		public bool IsTransactionActive => transactionStartIndices.Count > 0;
		public event Action<IOperation> PerformingOperation;

		public static void AddOperationProcessorTypes(IEnumerable<Type> types)
		{
			Processors.OperationProcessorTypes.AddRange(types);
		}

		public IDisposable BeginTransaction()
		{
			if (transactionStartIndices.Count == 0) {
				transactionId++;
				Current = this;
			}
			transactionStartIndices.Push(currentIndex);
			return new Disposable { OnDispose = EndTransaction };
		}

		private class Disposable : IDisposable
		{
			public Action OnDispose;

			public void Dispose() => OnDispose?.Invoke();
		}

		public void EndTransaction()
		{
			RollbackTransaction();
			transactionStartIndices.Pop();
			if (transactionStartIndices.Count == 0) {
				Current = null;
			}
		}

		public void DoTransaction(Action block)
		{
			using (BeginTransaction()) {
				block();
				CommitTransaction();
			}
		}

		public void CommitTransaction()
		{
			AssertTransaction();
			transactionStartIndices.Pop();
			transactionStartIndices.Push(currentIndex);
		}

		public void RollbackTransaction()
		{
			AssertTransaction();
			var index = transactionStartIndices.Peek();
			if (currentIndex != index) {
				operations.RemoveRange(currentIndex, GetTransactionEndIndex() - currentIndex);
				for (; currentIndex > index; currentIndex--) {
					Processors.Invert(operations[currentIndex - 1]);
				}
				OnChange();
			}
		}

		public void Perform(IOperation operation)
		{
			AssertTransaction();
			PerformingOperation?.Invoke(operation);
			operation.TransactionId = transactionId;
			if (saveIndex > currentIndex) {
				saveIndex = -1;
			}
			if (currentIndex == operations.Count) {
				operations.Add(operation);
				currentIndex++;
			} else if (operation.IsChangingDocument) {
				operations.RemoveRange(currentIndex, operations.Count - currentIndex);
				operations.Add(operation);
				currentIndex = operations.Count;
			} else {
				operations.Insert(currentIndex, operation);
				operations.Insert(currentIndex, operation);
				currentIndex++;
			}
			Processors.Do(operation);
			OnChange();
		}

		private void AssertTransaction()
		{
			if (!IsTransactionActive) {
				throw new InvalidOperationException("Can't perform an operation, commit or rollback outside the transaction");
			}
		}

		public void Undo()
		{
			if (!CanUndo()) {
				return;
			}
			bool documentChanged = false;
			int s = GetTransactionStartIndex();
			while (currentIndex > 0 && !documentChanged) {
				documentChanged |= AnyChangingOperationWithinRange(s, currentIndex);
				for (; currentIndex > s; currentIndex--) {
					Processors.Invert(operations[currentIndex - 1]);
				}
				s = GetTransactionStartIndex();
			}
			OnChange();
		}

		public void Redo()
		{
			if (!CanRedo()) {
				return;
			}
			bool documentChanged = false;
			int e = GetTransactionEndIndex();
			while (currentIndex < e) {
				bool b = AnyChangingOperationWithinRange(currentIndex, e);
				if (b && documentChanged) {
					break;
				}
				documentChanged |= b;
				for (; currentIndex < e; currentIndex++) {
					Processors.Invert(operations[currentIndex]);
				}
				e = GetTransactionEndIndex();
			}
			OnChange();
		}

		private int GetTransactionStartIndex()
		{
			for (int i = currentIndex; i > 0; i--) {
				if (operations[i - 1].TransactionId != operations[currentIndex - 1].TransactionId) {
					return i;
				}
			}
			return 0;
		}

		private int GetTransactionEndIndex()
		{
			for (int i = currentIndex; i < operations.Count; i++) {
				if (operations[i].TransactionId != operations[currentIndex].TransactionId) {
					return i;
				}
			}
			return operations.Count;
		}

		public void AddSavePoint()
		{
			for (saveIndex = currentIndex; saveIndex > 0; saveIndex--) {
				if (operations[saveIndex - 1].IsChangingDocument) {
					break;
				}
			}
			RefreshModifiedStatus();
		}

		public void ExternalModification()
		{
			saveIndex = -1;
			RefreshModifiedStatus();
		}

		private void OnChange()
		{
			RefreshModifiedStatus();
			Application.InvalidateWindows();
		}

		private void RefreshModifiedStatus()
		{
			IsDocumentModified = saveIndex < 0 || (saveIndex <= currentIndex ?
				AnyChangingOperationWithinRange(saveIndex, currentIndex) :
				AnyChangingOperationWithinRange(currentIndex, saveIndex));
		}

		private bool AnyChangingOperationWithinRange(int start, int end)
		{
			for (int i = start; i < end; i++) {
				if (operations[i].IsChangingDocument)
					return true;
			}
			return false;
		}

		private static class Processors
		{
			private static readonly Dictionary<Type, List<IOperationProcessor>> operationTypeToProcessorList = new Dictionary<Type, List<IOperationProcessor>>();
			private static readonly Dictionary<Type, IOperationProcessor> processorInstances = new Dictionary<Type, IOperationProcessor>();
			public static readonly List<Type> OperationProcessorTypes = new List<Type>();

			private static IOperationProcessor GetProcessor(Type t)
			{
				if (processorInstances.TryGetValue(t, out IOperationProcessor value)) {
					return value;
				}
				value = (IOperationProcessor)Activator.CreateInstance(t);
				processorInstances.Add(t, value);
				return value;
			}

			public static void Do(IOperation operation)
			{
				foreach (var p in EnumerateProcessors(operation)) {
					p.Do(operation);
				}
				operation.Performed = true;
			}

			public static void Invert(IOperation operation)
			{
				foreach (var p in EnumerateProcessors(operation)) { 
					if (operation.Performed) {
						p.Undo(operation);
					} else {
						p.Redo(operation);
					}
				}
				operation.Performed = !operation.Performed;
			}

			private static IEnumerable<IOperationProcessor> EnumerateProcessors(IOperation operation)
			{
				var operationType = operation.GetType();
				if (operationTypeToProcessorList.TryGetValue(operationType, out List<IOperationProcessor> cachedProcessorList)
				) {
					foreach (var processor in cachedProcessorList) {
						yield return processor;
					}
					yield break;
				}
				operationTypeToProcessorList.Add(operationType, cachedProcessorList = new List<IOperationProcessor>());
				foreach (var processorType in OperationProcessorTypes) {
					if (!typeof(IOperationProcessor).IsAssignableFrom(processorType)) {
						throw new InvalidOperationException();
					}
					Type genericOperationProcessorType = null;
					var t = processorType;
					while (t != null && t != typeof(object)) {
						if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(OperationProcessor<>)) {
							genericOperationProcessorType = t;
							break;
						}
						t = t.BaseType;
					}
					if (genericOperationProcessorType != null) {
						var operationTypeOfProcessor = genericOperationProcessorType.GetGenericArguments().First();
						if (operationType.IsGenericType) {
							var operationGenericArguments = operationType.GetGenericArguments();
							if (
								operationTypeOfProcessor.IsGenericType &&
								operationTypeOfProcessor.GetGenericTypeDefinition().IsAssignableFrom(operationType.GetGenericTypeDefinition())
							) {
								var specializedGenericProcessor = processorType.MakeGenericType(operationGenericArguments);
								var p = GetProcessor(specializedGenericProcessor);
								cachedProcessorList.Add(p);
								yield return p;
							}
						} else if (operationTypeOfProcessor.IsAssignableFrom(operationType)) {
							var p = GetProcessor(processorType);
							cachedProcessorList.Add(p);
							yield return p;
						}
					} else {
						var p = GetProcessor(processorType);
						cachedProcessorList.Add(p);
						yield return p;
					}
				}
			}
		}
	}
}
