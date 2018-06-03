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
		private readonly List<IOperation> operationStash = new List<IOperation>();
		private int transactionId;
		private int saveIndex;
		private int headIndex;
		private int stashHeadIndex;
		private int transactionStartIndex = -1;
		private int transactionRollbackPointIndex = -1;
		private bool transactionCommited;
		
		public bool CanUndo() => !IsTransactionActive && headIndex > 0;
		public bool CanRedo() => !IsTransactionActive && (headIndex < operations.Count || operationStash.Count > 0);
		public bool IsDocumentModified { get; private set; }
		public bool IsTransactionActive => transactionStartIndex != -1;
		
		public IDisposable BeginTransaction()
		{
			if (IsTransactionActive) {
				throw new InvalidOperationException("Nested transactions aren't allowed");
			}
			transactionId++;
			transactionStartIndex = headIndex;
			transactionRollbackPointIndex = headIndex;
			transactionCommited = false;
			return new Disposable { OnDispose = EndTransaction };
		}
		
		public void SetRollbackPoint()
		{
			transactionRollbackPointIndex = headIndex;
		}
		
		private class Disposable : IDisposable
		{
			public Action OnDispose;
			
			public void Dispose() => OnDispose?.Invoke();
		}
		
		public void EndTransaction()
		{
			if (!IsTransactionActive) {
				throw new InvalidOperationException("Transaction wasn't began");
			}
			if (!transactionCommited) {
				RollbackTransactionFromBeginning();
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
		
		public void DoTransactionMaybeNested(Action block)
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
		
		public void RollbackTransactionFromBeginning()
		{
			RollbackTransactionFromIndex(transactionStartIndex);
		}
		
		public void RollbackTransaction()
		{
			RollbackTransactionFromIndex(transactionRollbackPointIndex);
		}

		private void RollbackTransactionFromIndex(int index)
		{
			if (transactionCommited) {
				throw new InvalidOperationException("Can't rollback committed transaction");
			}
			if (headIndex != index) {
				for (; headIndex > index; headIndex--) {
					Processors.Undo(operations[headIndex - 1]);
				}
				operations.RemoveRange(headIndex, operations.Count - headIndex);
				OnChange();
			}
		}
		
		public void Perform(IOperation operation)
		{
			if (!IsTransactionActive) {
				throw new InvalidOperationException("Can't perform an operation outside a transaction block");
			}
			operation.TransactionId = transactionId;
			if (saveIndex > headIndex) {
				saveIndex = -1;
			}
			if (headIndex < operations.Count) {
				// Save current history tail just in case we redo it further.
				operationStash.Clear();
				stashHeadIndex = headIndex;
				var tail = operations.GetRange(headIndex, operations.Count - headIndex);
				if (tail.Any(i => i.IsChangingDocument)) {
					operationStash.AddRange(tail);
				}
				operations.RemoveRange(headIndex, operations.Count - headIndex);
			}
			operations.Add(operation);
			headIndex = operations.Count;
			Processors.Do(operation);
			OnChange();
		}
		
		public void Undo()
		{
			if (!CanUndo()) {
				return;
			}
			int bid = 0;
			for (; headIndex > 0; headIndex--) {
				var o = operations[headIndex - 1];
				if (o.IsChangingDocument && bid == 0) {
					bid = o.TransactionId;
				}
				if (bid > 0 && bid != o.TransactionId) {
					break;
				}
				Processors.Undo(o);
			}
			OnChange();
		}

		public void Redo()
		{
			if (!CanRedo()) {
				return;
			}
			var tail = operations.GetRange(headIndex, operations.Count - headIndex);
			if (tail.Any(i => i.IsChangingDocument)) {
				// Current history tail has modifying operations, so no need in stashed operations.
				operationStash.Clear();
			}
			if (operationStash.Count > 0) {
				for (int i = headIndex; i > stashHeadIndex; i--) {
					Processors.Undo(operations[i - 1]);
				}
				operations.RemoveRange(headIndex, operations.Count - headIndex);
				operations.AddRange(operationStash);
				operationStash.Clear();
			}
			int bid = 0;
			for (; headIndex < operations.Count; headIndex++) {
				var o = operations[headIndex];
				if (o.IsChangingDocument && bid == 0) {
					bid = o.TransactionId;
				}
				if (bid > 0 && bid != o.TransactionId) {
					break;
				}
				Processors.Redo(o);
			}
			OnChange();
		}
		
		public void AddSavePoint()
		{
			for (saveIndex = headIndex; saveIndex > 0; saveIndex--) {
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
			IsDocumentModified = saveIndex < 0 || (saveIndex <= headIndex ? 
				IsChangingOperationWithinRange(saveIndex, headIndex) : 
				IsChangingOperationWithinRange(headIndex, saveIndex));
		}

		private bool IsChangingOperationWithinRange(int start, int end)
		{
			for (int i = start; i < end; i++) {
				if (operations[i].IsChangingDocument)
					return true;
			}
			return false;
		}
		
		public interface ITransaction : IDisposable
		{
			void Commit();
			void CommitAndDispose();
			void Rollback();
		}

		public class ProcessorList : List<IOperationProcessor> 
		{					
			public void Do(IOperation operation)
			{
				foreach (var p in this) {
					p.Do(operation);
				}
			}

			public void Undo(IOperation operation)
			{
				foreach (var p in this) {
					p.Undo(operation);
				}
			}
						
			public void Redo(IOperation operation)
			{
				foreach (var p in this) {
					p.Redo(operation);
				}
			}
		}
	}
}
