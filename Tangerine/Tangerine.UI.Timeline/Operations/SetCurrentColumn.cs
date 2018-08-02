using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Operations
{
	public class SetCurrentColumn : Operation
	{
		private static bool isScrollingFrozen;
		// Evgenii Polikutin: needed for RulerbarMouseScrollProcessor to avoid extra operations
		public static bool IsFrozen;

		protected int Column;
		protected Node Container;

		public override bool IsChangingDocument => false;

		public static void Perform(int column, Node container)
		{
			if (Document.Current.PreviewAnimation) {
				Document.Current.TogglePreviewAnimation(CoreUserPreferences.Instance.AnimationMode, false);
			}
			Document.Current.History.Perform(new SetCurrentColumn(column, container));
		}

		public static void Perform(int column)
		{
			Perform(column, Document.Current.Container);
		}

		public static void RollbackHistoryWithoutScrolling()
		{
			isScrollingFrozen = true;
			try {
				Document.Current.History.RollbackTransaction();
			} finally {
				isScrollingFrozen = false;
			}
		}

		private SetCurrentColumn(int column, Node node)
		{
			Column = column;
			Container = node;
		}

		public class Processor : OperationProcessor<SetCurrentColumn>
		{
			class Backup { public int Column; }

			public static bool CacheAnimationsStates
			{
				set { Document.CacheAnimationsStates = value; }
			}

			protected override void InternalRedo(SetCurrentColumn op)
			{
				op.Save(new Backup { Column = Timeline.Instance.CurrentColumn });
				SetColumn(op.Column, op.Container);
			}

			protected override void InternalUndo(SetCurrentColumn op)
			{
				if (IsFrozen) {
					return;
				}
				SetColumn(op.Restore<Backup>().Column, op.Container);
			}

			void SetColumn(int value, Node node)
			{
				Document.SetCurrentFrameToNode(value, node, CoreUserPreferences.Instance.AnimationMode);
				if (!isScrollingFrozen) {
					Timeline.Instance.EnsureColumnVisible(value);
				}
			}

		}
	}
}
