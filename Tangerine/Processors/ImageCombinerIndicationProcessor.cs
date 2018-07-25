using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.UI;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.Processors
{
	class ImageCombinerIndicationProcessor : SymmetricOperationProcessor
	{
		public override void Process(IOperation op)
		{
			var rows = Document.Current.Rows;
			for (int i = 0; i < rows.Count; ++i) {
				var row = rows[i];
				var view = row.Components.Get<RowView>()?.RollRow as RollNodeView;
				if (view == null) {
					continue;
				}
				var combiner = row.Components.Get<NodeRow>()?.Node as ImageCombiner;
				if (combiner != null) {
					IImageCombinerArg arg1;
					IImageCombinerArg arg2;
					if (combiner.GetArgs(out arg1, out arg2)) {
						view.Label.Color = ColorTheme.Current.Basic.BlackText;
						view.ImageCombinerIndicator.Color = Color4.Transparent;
						SetImageCombinerIndication(rows[i + 1]);
						SetImageCombinerIndication(rows[i + 2]);
						i += 2;
					} else {
						view.Label.Color = ColorTheme.Current.Basic.RedText;
						view.ImageCombinerIndicator.Color = Color4.White;
					}
					continue;
				}
				var arg = row.Components.Get<NodeRow>()?.Node as IImageCombinerArg;
				if (arg != null) {
					view.ImageCombinerIndicator.Color = Color4.Transparent;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void SetImageCombinerIndication(Row row)
		{
			var view = row.Components.Get<RowView>().RollRow as RollNodeView;
			view.ImageCombinerIndicator.Color = Color4.White;
		}
	}
}
