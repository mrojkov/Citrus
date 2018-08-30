using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Operations
{
	public class SelectCurveKey : Operation
	{
		public readonly Curve Curve;
		public readonly IKeyframe Key;
		public readonly bool Select;

		public override bool IsChangingDocument => false;

		public static void Perform(Curve curve, IKeyframe key, bool select)
		{
			DocumentHistory.Current.Perform(new SelectCurveKey(curve, key, select));
		}

		private SelectCurveKey(Curve curve, IKeyframe key, bool select)
		{
			Curve = curve;
			Key = key;
			Select = select;
		}

		public class Processor : OperationProcessor<SelectCurveKey>
		{
			protected override void InternalRedo(SelectCurveKey op)
			{
				if (op.Select) {
					op.Curve.SelectedKeys.Add(op.Key);
				} else {
					op.Curve.SelectedKeys.Remove(op.Key);
				}
			}

			protected override void InternalUndo(SelectCurveKey op)
			{
				if (op.Select) {
					op.Curve.SelectedKeys.Remove(op.Key);
				} else {
					op.Curve.SelectedKeys.Add(op.Key);
				}
			}
		}
	}
}
