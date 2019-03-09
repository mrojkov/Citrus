using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline
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
		private List<int> rowIndexesToSkip = new List<int>();

		public override void Process(IOperation op)
		{
			if (!op.IsChangingDocument && container == Document.Current.Container) {
				return;
			}
			container = Document.Current.Container;
			var rows = Document.Current.Rows;
			rowIndexesToSkip.Clear();
			for (int i = 0; i < rows.Count; ++i) {
				var row = rows[i];
				if (!(row.Components.Get<RowView>()?.RollRow is RollNodeView view)) {
					continue;
				}
				if (rowIndexesToSkip.Remove(i)) {
					continue;
				}
				var node = row.Components.Get<NodeRow>()?.Node;
				if (row.Components.Get<NodeRow>()?.Node is ImageCombiner combiner) {
					if (combiner.GetArgs(out IImageCombinerArg arg1, out IImageCombinerArg arg2)) {
						view.RefreshLabelColor();
						view.LinkIndicatorButtonContainer.EnableIndication<ImageCombinerLinkIndicatorButton>().ShowNormal();
						var arg2Row = rows.FirstOrDefault(r => r.Components.Get<NodeRow>()?.Node == arg2);
						var arg1Row = rows.FirstOrDefault(r => r.Components.Get<NodeRow>()?.Node == arg1);
						if (arg2Row != null) {
							rowIndexesToSkip.Add(rows.IndexOf(arg2Row));
							SetImageCombinerIndication(arg2Row);
						}
						if (arg1Row != null) {
							rowIndexesToSkip.Add(rows.IndexOf(arg1Row));
							SetImageCombinerIndication(arg1Row);
						}
					} else {
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
