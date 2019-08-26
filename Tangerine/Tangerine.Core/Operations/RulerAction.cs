using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.Core.Operations
{
	public class DeleteRuler : Operation
	{
		private readonly Ruler ruler;
		private readonly RulerLine rulerLine;

		public override bool IsChangingDocument => true;

		public static void Perform(Ruler ruler, RulerLine rulerLine)
		{
			DocumentHistory.Current.Perform(new DeleteRuler(ruler, rulerLine));
		}

		protected DeleteRuler(Ruler ruler, RulerLine rulerLine)
		{
			this.ruler = ruler;
			this.rulerLine = rulerLine;
		}

		public class Processor : OperationProcessor<DeleteRuler>
		{

			protected override void InternalRedo(DeleteRuler op)
			{
				op.ruler.DeleteLine(op.rulerLine);
			}

			protected override void InternalUndo(DeleteRuler op)
			{
				op.ruler.Lines.Add(op.rulerLine);
			}
		}

	}

	public class CreateRuler : Operation
	{
		private readonly Ruler ruler;
		private readonly RulerLine rulerLine;

		public override bool IsChangingDocument => true;

		public static void Perform(Ruler ruler, RulerLine rulerLine)
		{
			DocumentHistory.Current.Perform(new CreateRuler(ruler, rulerLine));
		}

		protected CreateRuler(Ruler ruler, RulerLine rulerLine)
		{
			this.ruler = ruler;
			this.rulerLine = rulerLine;
		}

		public class Processor : OperationProcessor<CreateRuler>
		{

			protected override void InternalRedo(CreateRuler op)
			{
				op.ruler.Lines.Add(op.rulerLine);
			}

			protected override void InternalUndo(CreateRuler op)
			{
				op.ruler.DeleteLine(op.rulerLine);
			}
		}

	}
}
