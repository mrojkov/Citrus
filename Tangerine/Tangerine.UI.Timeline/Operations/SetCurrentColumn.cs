using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Operations
{
	public class SetCurrentColumn : Operation
	{
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
				SetColumn(op.Restore<Backup>().Column, op.Container);
			}

			void SetColumn(int value, Node node)
			{
				Document.SetCurrentFrameToNode(value, node, CoreUserPreferences.Instance.AnimationMode);
				Timeline.Instance.EnsureColumnVisible(value);
			}

		}
	}
}
