using System;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class ClampScrollPosOnContainerChange
	{
		public ITaskProvider GetProcessor()
		{
			return new Property<Node>(() => Document.Current.Container).DistinctUntilChanged().Consume(_ => {
				var rows = Document.Current.SelectedRows;
				if (rows.Count > 0) {
					Timeline.Instance.EnsureRowVisible(rows[0]);
				}
			});
		}
	}
}
