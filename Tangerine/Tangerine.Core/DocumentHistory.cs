using System;
using System.Collections.Generic;
using System.Linq;

namespace Tangerine.Core
{
	public class DocumentHistory
	{
		public static readonly List<IOperationProcessor> Processors = new List<IOperationProcessor>();

		int transactionCounter;
		long transactionBatchId;
		readonly List<IOperation> operations = new List<IOperation>();
		int headPos;
		int savePos;

		public bool CanUndo => headPos > 0;
		public bool CanRedo => headPos < operations.Count;
		public bool IsDocumentModified { get; private set; }

		public void BeginTransaction()
		{
			transactionCounter++;
			transactionBatchId = Lime.Application.UpdateCounter;
		}

		public void EndTransaction()
		{
			transactionCounter--;
		}

		public void Perform(IOperation operation)
		{
			operation.BatchId = (transactionCounter > 0) ? transactionBatchId : Lime.Application.UpdateCounter;
			if (savePos > headPos) {
				savePos = -1;
			}
			operations.RemoveRange(headPos, operations.Count - headPos);
			operations.Add(operation);
			headPos = operations.Count;
			foreach (var p in Processors) {
				p.Do(operation);
			}
			OnChange();
		}

		public void Undo()
		{
			if (!CanUndo) {
				return;
			}
			long batchId = 0;
			for (; headPos > 0; headPos--) {
				var o = operations[headPos - 1];
				if (o.IsChangingDocument && batchId == 0) {
					batchId = o.BatchId;
				}
				if (batchId > 0 && batchId != o.BatchId) {
					break;
				}
				foreach (var p in Processors) {
					p.Undo(o);
				}
			}
			OnChange();
		}
		
		public void Redo()
		{
			if (!CanRedo) {
				return;
			}
			long batchId = 0;
			for (; headPos < operations.Count; headPos++) {
				var o = operations[headPos];
				if (o.IsChangingDocument && batchId == 0) {
					batchId = o.BatchId;
				}
				if (batchId > 0 && batchId != o.BatchId) {
					break;
				}
				foreach (var p in Processors) {
					p.Redo(o);
				}
			}
			OnChange();
		}

		void RefreshModifiedStatus()
		{
			if (savePos < 0) {
				IsDocumentModified = true;
				return;
			}
			IsDocumentModified = savePos <= headPos ?
				IsChangingOperationWithinRange(savePos, headPos) :
				IsChangingOperationWithinRange(headPos, savePos);
		}

		private bool IsChangingOperationWithinRange(int start, int end)
		{
			for (int i = start; i < end; i++) {
				if (operations[i].IsChangingDocument)
					return true;
			}
			return false;
		}

		public void AddSavePoint()
		{
			for (savePos = headPos; savePos > 0; savePos--) {
				if (operations[savePos - 1].IsChangingDocument) {
					break;
				}
			}
			RefreshModifiedStatus();
		}

		void OnChange()
		{
			RefreshModifiedStatus();
			Lime.Application.InvalidateWindows();
		}
	}
}
