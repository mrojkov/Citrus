using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.Core
{
	public interface ISaveOperation : IOperation { }

	public class DocumentHistory
	{
		private int transactionCounter;
		private DateTime transactionTimestamp;
		private int undoPosition;
		private List<IOperation> operations = new List<IOperation>();

		public static readonly List<Func<IOperationProcessor>> ProcessorBuilders = new List<Func<IOperationProcessor>>();
		readonly List<IOperationProcessor> processors;

		public event Action Changed;

		public bool UndoEnabled => undoPosition > 0;
		public bool RedoEnabled => undoPosition < operations.Count;
		public bool IsDocumentModified { get; private set; }

		public DocumentHistory()
		{
			processors = ProcessorBuilders.Select(i => i()).ToList();
			Changed += RefreshModifiedStatus;
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
			operations.RemoveRange(undoPosition, operations.Count - undoPosition);
			operations.Add(operation);
			undoPosition = operations.Count;
			foreach (var p in processors) {
				p.Do(operation);
			}
			Changed?.Invoke();
		}

		public void Undo()
		{
			if (!UndoEnabled) {
				return;
			}
			DateTime? timestamp = null;
			for (; undoPosition > 0; undoPosition--) {
				var o = operations[undoPosition - 1];
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
			Changed?.Invoke();
		}
		
		public void Redo()
		{
			if (!RedoEnabled) {
				return;
			}
			DateTime? timestamp = null;
			for (; undoPosition < operations.Count; undoPosition++) {
				var o = operations[undoPosition];
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
			Changed?.Invoke();
		}

		private void RefreshModifiedStatus()
		{
			IsDocumentModified = false;
			for (var i = undoPosition; i > 0; i--) {
				var o = operations[i - 1];
				if (o is ISaveOperation) {
					break;
				}
				IsDocumentModified |= o.IsChangingDocument;
			}
		}
	}
}
