using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class RollWidgetsUpdater : SymmetricOperationProcessor
	{
		private Timeline timeline => Timeline.Instance;

		public override void Process(IOperation op)
		{
			if (!AreWidgetsValid()) {
				ResetWidgets();
			}
			UpdateIndentation();
		}

		private static void UpdateIndentation()
		{
			foreach (var row in Document.Current.Rows) {
				row.Components.Get<Components.RowView>().RollRow.Indentation = CalcIndentation(row) * TimelineMetrics.RollIndentation;
			}
		}

		private static int CalcIndentation(Row row)
		{
			var i = 0;
			for (var r = row.Parent; r != null; r = r.Parent) {
				i++;
			}
			return i - 1;
		}

		private void ResetWidgets()
		{
			var content = timeline.Roll.ContentWidget;
			content.Nodes.Clear();
			foreach (var row in Document.Current.Rows) {
				var rollRow = row.Components.Get<Components.RowView>().RollRow;
				var widget = rollRow.Widget;
				if (!rollRow.AwakeBehavior.IsAwoken) {
					rollRow.AwakeBehavior.Update(0);
				}
				content.AddNode(widget);
			}
		}

		private bool AreWidgetsValid()
		{
			var content = timeline.Roll.ContentWidget;
			if (Document.Current.Rows.Count != content.Nodes.Count) {
				return false;
			}
			foreach (var row in Document.Current.Rows) {
				if (row.Components.Get<Components.RowView>().RollRow.Widget != content.Nodes[row.Index]) {
					return false;
				}
			}
			return true;
		}
	}
}
