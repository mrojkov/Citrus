using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline.Processors
{
	public class ImageCombinerIndicationProcessor : SymmetricOperationProcessor
	{
		private class ImageCombinerTieIndication : TieIndication
		{
			public ImageCombinerTieIndication() : base(NodeIconPool.GetTexture(typeof(ImageCombiner)))
			{
			}
		}

		public override void Process(IOperation op)
		{
			var rows = Document.Current.Rows;
			for (int i = 0; i < rows.Count; ++i) {
				var row = rows[i];
				if (!(row.Components.Get<RowView>()?.RollRow is RollNodeView view)) {
					continue;
				}
				var node = row.Components.Get<NodeRow>()?.Node;
				if (row.Components.Get<NodeRow>()?.Node is ImageCombiner combiner) {
					if (combiner.GetArgs(out IImageCombinerArg arg1, out IImageCombinerArg arg2)) {
						view.Label.Color = Theme.Colors.BlackText;
						view.TieIndicationContainer.EnableIndication<ImageCombinerTieIndication>().Texture = NodeIconPool.GetTexture(typeof(ImageCombiner));
						SetImageCombinerIndication(rows[i + 1]);
						SetImageCombinerIndication(rows[i + 2]);
						i += 2;
					}
					else {
						view.Label.Color = Theme.Colors.RedText;
						view.TieIndicationContainer.EnableIndication<ImageCombinerTieIndication>().Texture = IconPool.GetTexture("Timeline.NoEntry");
					}
					continue;
				}
				if (row.Components.Get<NodeRow>()?.Node is IImageCombinerArg arg) {
					view.TieIndicationContainer.DisableIndication<ImageCombinerTieIndication>();
				}
			}
		}

		private static void SetImageCombinerIndication(Row row)
		{
			if (row.Components.Get<RowView>()?.RollRow is RollNodeView view) {
				view.TieIndicationContainer.EnableIndication<ImageCombinerTieIndication>();
			}
		}
	}
}
