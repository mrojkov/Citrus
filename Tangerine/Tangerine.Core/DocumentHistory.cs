using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.Core
{
	public class DocumentHistory
	{
		public static readonly ProcessorList Processors = new ProcessorList();
		private readonly Stack<int> transactionStartIndices = new Stack<int>();
		private readonly List<IOperation> operations = new List<IOperation>();
		private int transactionId;
		private int saveIndex;
		private int currentIndex;
		
		public bool CanUndo() => !IsTransactionActive && currentIndex > 0;
		public bool CanRedo() => !IsTransactionActive && currentIndex < operations.Count;
		public bool IsDocumentModified { get; private set; }
		public bool IsTransactionActive => transactionStartIndices.Count > 0;
		
		public IDisposable BeginTransaction()
		{
			if (transactionStartIndices.Count == 0) {
				transactionId++;
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
			transactionStartIndices.Pop();
			transactionStartIndices.Push(currentIndex);
		}
		
		public void RollbackTransaction()
		{
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
			if (!IsTransactionActive) {
				throw new InvalidOperationException("Can't perform an operation outside the transaction");
			}
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
		
		public class ProcessorList : List<IOperationProcessor> 
		{					
			public void Do(IOperation operation)
			{
				foreach (var p in this) {
					p.Do(operation);
				}
				operation.Performed = true;
			}

			public void Invert(IOperation operation)
			{
				foreach (var p in this) {
					if (operation.Performed) {
						p.Undo(operation);
					} else {
						p.Redo(operation);
					}
				}
				operation.Performed = !operation.Performed;
			}
		}
	}
}
