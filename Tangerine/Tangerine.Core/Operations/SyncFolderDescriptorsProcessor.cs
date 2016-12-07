using System;
using Lime;

namespace Tangerine.Core.Operations
{
	public class SyncFolderDescriptorsProcessor : IOperationProcessor
	{
		public void Do(IOperation op)
		{
			if ((op as SetProperty)?.Obj is Folder) {
				Document.Current.Container.SyncFolderDescriptorsAndNodes();
			}
		}

		public void Redo(IOperation op) => Do(op);
		public void Undo(IOperation op) => Do(op);
	}
}
