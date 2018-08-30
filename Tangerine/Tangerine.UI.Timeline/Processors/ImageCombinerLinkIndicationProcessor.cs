using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline.Processors
{
	public class ImageCombinerLinkIndicationProcessor : SymmetricOperationProcessor
	{
		private class ImageCombinerLinkIndicatorButton : LinkIndicatorButton
		{
			public ImageCombinerLinkIndicatorButton() : base(NodeIconPool.GetTexture(typeof(ImageCombiner)))
			{
				Tip = "Linked to ImageCombiner";
			}

			private void SetTipAndTexture(string tip, ITexture texture)
			{
				Tip = tip;
				Texture = texture;
			}

			public void ShowNormal(string tip = "Has linked arguments") => SetTipAndTexture(tip, NodeIconPool.GetTexture(typeof(ImageCombiner)));
			public void ShowError(string tip = "No linked arguments") => SetTipAndTexture(tip, IconPool.GetTexture("Timeline.NoEntry"));
		}

		private Node container;

		public override void Process(IOperation op)
		{
			if (!op.IsChangingDocument && container == Document.Current.Container) {
				return;
			}
			container = Document.Current.Container;
			var rows = Document.Current.Rows;
			for (int i = 0; i < rows.Count; ++i) {
				var row = rows[i];
				if (!(row.Components.Get<RowView>()?.RollRow is RollNodeView view)) {
					continue;
				}
				var node = row.Components.Get<NodeRow>()?.Node;
				if (row.Components.Get<NodeRow>()?.Node is ImageCombiner combiner) {
					if (combiner.GetArgs(out IImageCombinerArg arg1, out IImageCombinerArg arg2)) {
						view.RefreshLabelColor();
						view.LinkIndicatorButtonContainer.EnableIndication<ImageCombinerLinkIndicatorButton>().ShowNormal();
						SetImageCombinerIndication(rows[i + 1]);
						SetImageCombinerIndication(rows[i + 2]);
						i += 2;
					}
					else {
						view.Label.Color = Theme.Colors.RedText;
						view.LinkIndicatorButtonContainer.EnableIndication<ImageCombinerLinkIndicatorButton>().ShowError();
					}
					continue;
				}
				if (row.Components.Get<NodeRow>()?.Node is IImageCombinerArg arg) {
					view.LinkIndicatorButtonContainer.DisableIndication<ImageCombinerLinkIndicatorButton>();
				}
			}
		}

		private static void SetImageCombinerIndication(Row row)
		{
			if (row.Components.Get<RowView>()?.RollRow is RollNodeView view) {
				view.LinkIndicatorButtonContainer.EnableIndication<ImageCombinerLinkIndicatorButton>();
			}
		}
	}
}
