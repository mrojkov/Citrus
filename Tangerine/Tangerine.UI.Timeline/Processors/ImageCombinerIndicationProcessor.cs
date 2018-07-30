using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline.Processors
{
	public class ImageCombinerIndicationProcessor : SymmetricOperationProcessor
	{
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
						view.Label.Color = ColorTheme.Current.Basic.BlackText;
						view.ImageCombinerIndicator.Color = Color4.Transparent;
						SetImageCombinerIndication(rows[i + 1]);
						SetImageCombinerIndication(rows[i + 2]);
						i += 2;
					}
					else {
						view.Label.Color = ColorTheme.Current.Basic.RedText;
						view.ImageCombinerIndicator.Color = Color4.White;
					}
					continue;
				}
				if (row.Components.Get<NodeRow>()?.Node is IImageCombinerArg arg) {
					view.ImageCombinerIndicator.Color = Color4.Transparent;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void SetImageCombinerIndication(Row row)
		{
			if (row.Components.Get<RowView>()?.RollRow is RollNodeView view) {
				view.ImageCombinerIndicator.Color = Color4.White;
			}
		}
	}
}
