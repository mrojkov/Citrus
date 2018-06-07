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
				for (; currentIndex > index; currentIndex--) {
					Processors.Invert(operations[currentIndex - 1]);
				}
				operations.RemoveRange(currentIndex, operations.Count - currentIndex);
				OnChange();
			}
		}
		
		public void Perform(IOperation operation)
		{
			if (!IsTransactionActive) {
				throw new InvalidOperationException("Can't perform an operation outside a transaction");
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
			int tid = 0;
			for (; currentIndex > 0; currentIndex--) {
				var o = operations[currentIndex - 1];
				if (o.IsChangingDocument && tid == 0) {
					tid = o.TransactionId;
				}
				if (tid > 0 && tid != o.TransactionId) {
					break;
				}
				Processors.Invert(o);
			}
			OnChange();
		}

		public void Redo()
		{
			if (!CanRedo()) {
				return;
			}
			var documentChanged = false;
			for (int e = GetTransactionEnd(); currentIndex < e; e = GetTransactionEnd()) {
				var b = IsChangingOperationWithinRange(currentIndex, e);
				if (b && documentChanged) {
					break;
				}
				documentChanged |= b;
				for (; currentIndex < e; currentIndex++) {
					Processors.Invert(operations[currentIndex]);
				}
			}
			OnChange();
		}
		
		private int GetTransactionEnd()
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
		
		void OnChange()
		{
			RefreshModifiedStatus();
			Application.InvalidateWindows();
		}
		
		void RefreshModifiedStatus()
		{
			IsDocumentModified = saveIndex < 0 || (saveIndex <= currentIndex ? 
				IsChangingOperationWithinRange(saveIndex, currentIndex) : 
				IsChangingOperationWithinRange(currentIndex, saveIndex));
		}
		
		private bool IsChangingOperationWithinRange(int start, int end)
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
