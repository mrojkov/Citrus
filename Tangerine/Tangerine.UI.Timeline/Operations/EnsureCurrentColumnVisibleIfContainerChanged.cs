using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class EnsureCurrentColumnVisibleIfContainerChanged : OperationProcessor<Core.Operations.ISetContainer>
	{
		protected override void InternalDo(Core.Operations.ISetContainer op)
		{
			Operations.SetCurrentColumn.Perform(Document.Current.AnimationFrame);
		}

		protected override void InternalRedo(Core.Operations.ISetContainer op) { }
		protected override void InternalUndo(Core.Operations.ISetContainer op) { }
	}
}