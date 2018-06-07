using System;
using System.Collections.Generic;
using System.Linq;
using Lime;

namespace Tangerine.Core
{
	public class DocumentHistory
	{
		public static readonly ProcessorList Processors = new ProcessorList();
		private readonly List<IOperation> operations = new List<IOperation>();
		private int transactionId;
		private int saveIndex;
		private int currentIndex;
		private int transactionStartIndex = -1;
		private int transactionRollbackPointIndex = -1;
		private bool transactionCommited;
		
		public bool CanUndo() => !IsTransactionActive && currentIndex > 0;
		public bool CanRedo() => !IsTransactionActive && currentIndex < operations.Count;
		public bool IsDocumentModified { get; private set; }
		public bool IsTransactionActive => transactionStartIndex != -1;
		
		public IDisposable BeginTransaction()
		{
			if (IsTransactionActive) {
				throw new InvalidOperationException("Nested transactions aren't allowed");
			}
			transactionId++;
			transactionStartIndex = currentIndex;
			transactionRollbackPointIndex = currentIndex;
			transactionCommited = false;
			return new Disposable { OnDispose = EndTransaction };
		}
		
		public void SetRollbackPoint()
		{
			transactionRollbackPointIndex = currentIndex;
		}
		
		private class Disposable : IDisposable
		{
			public Action OnDispose;
			
			public void Dispose() => OnDispose?.Invoke();
		}
		
		public void EndTransaction()
		{
			if (!IsTransactionActive) {
				throw new InvalidOperationException("Transaction didn't begin");
			}
			if (!transactionCommited) {
				RollbackTransactionToStart();
			}
			transactionStartIndex = -1;
		}
		
		public void DoTransaction(Action block)
		{
			using (BeginTransaction()) {
				block();
				CommitTransaction();
			}
		}
		
		public void DoTransactionPermitNested(Action block)
		{
			if (IsTransactionActive) {
				block();
			} else {
				using (BeginTransaction()) {
					block();
					CommitTransaction();
				}
			}
		}
		
		public void CommitTransaction()
		{
			transactionCommited = true;
		}
		
		public void RollbackTransactionToStart()
		{
			RollbackTransactionToIndex(transactionStartIndex);
		}
		
		public void RollbackTransaction()
		{
			RollbackTransactionToIndex(transactionRollbackPointIndex);
		}

		private void RollbackTransactionToIndex(int index)
		{
			if (transactionCommited) {
				throw new InvalidOperationException("Can't rollback committed transaction");
			}
			if (currentIndex != index) {
				for (; currentIndex > index; currentIndex--) {
					Processors.UndoOrRedo(operations[currentIndex - 1]);
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
				operations.Insert(currentIndex + 1, operation);
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
				var i = operations[currentIndex - 1];
				if (i.IsChangingDocument && tid == 0) {
					tid = i.TransactionId;
				}
				if (tid > 0 && tid != i.TransactionId) {
					break;
				}
				Processors.UndoOrRedo(i);
			}
			OnChange();
		}

		public void Redo()
		{
			if (!CanRedo()) {
				return;
			}
			int tid = 0;
			for (; currentIndex < operations.Count; currentIndex++) {
				var o = operations[currentIndex];
				if (o.IsChangingDocument && tid == 0) {
					tid = o.TransactionId;
				}
				if (tid > 0 && tid != o.TransactionId) {
					break;
				}
				Processors.UndoOrRedo(o);
			}
			OnChange();
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

			public void UndoOrRedo(IOperation operation)
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
