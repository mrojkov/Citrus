using System;
using System.Collections.Generic;
using System.Linq;

namespace Tangerine.Core
{
	public class DocumentHistory
	{
		public static readonly List<Func<IOperationProcessor>> ProcessorBuilders = new List<Func<IOperationProcessor>>();
		readonly List<IOperationProcessor> processors;

		int transactionCounter;
		DateTime transactionTimestamp;
		readonly List<IOperation> operations = new List<IOperation>();
		int headPos;
		int savePos;

		public bool UndoEnabled => headPos > 0;
		public bool RedoEnabled => headPos < operations.Count;
		public bool IsDocumentModified { get; private set; }

		public DocumentHistory()
		{
			processors = ProcessorBuilders.Select(i => i()).ToList();
		}

		public void BeginTransaction()
		{
			transactionCounter++;
			transactionTimestamp = DateTime.UtcNow;
		}

		public void EndTransaction()
		{
			transactionCounter--;
		}

		public void Perform(IOperation operation)
		{
			operation.Timestamp = (transactionCounter > 0) ? transactionTimestamp : DateTime.UtcNow;
			if (savePos > headPos) {
				savePos = -1;
			}
			operations.RemoveRange(headPos, operations.Count - headPos);
			operations.Add(operation);
			headPos = operations.Count;
			foreach (var p in processors) {
				p.Do(operation);
			}
			OnChange();
		}

		public void Undo()
		{
			if (!UndoEnabled) {
				return;
			}
			DateTime? timestamp = null;
			for (; headPos > 0; headPos--) {
				var o = operations[headPos - 1];
				if (o.IsChangingDocument && !timestamp.HasValue) {
					timestamp = o.Timestamp;
				}
				if (timestamp.HasValue && (timestamp.Value - o.Timestamp) > TimeSpan.FromSeconds(0.1f)) {
					break;
				}
				foreach (var p in processors) {
					p.Undo(o);
				}
			}
			OnChange();
		}
		
		public void Redo()
		{
			if (!RedoEnabled) {
				return;
			}
			DateTime? timestamp = null;
			for (; headPos < operations.Count; headPos++) {
				var o = operations[headPos];
				if (o.IsChangingDocument && !timestamp.HasValue) {
					timestamp = o.Timestamp;
				}
				if (timestamp.HasValue && (o.Timestamp - timestamp.Value) > TimeSpan.FromSeconds(0.1f)) {
					break;
				}
				foreach (var p in processors) {
					p.Do(o);
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
			var range = savePos <= headPos ?
				operations.GetRange(savePos, headPos - savePos) :
				operations.GetRange(headPos, savePos - headPos);
			IsDocumentModified = range.Any(i => i.IsChangingDocument);
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
