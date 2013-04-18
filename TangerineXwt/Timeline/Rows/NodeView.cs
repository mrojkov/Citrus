using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Tangerine.Timeline
{
	public class NodeView : RowView
	{
		private Xwt.Label name;
		private Xwt.TextEntry nameEditor;
		private Xwt.ImageView nodeIcon;
		private Xwt.Button eye;

		public new NodeRow row { get { return base.row as NodeRow; } }

		public NodeView(Row row)
			: base(row)
		{
			nodeIcon = new Xwt.ImageView(IconCache.Get("Nodes/Scene"));
			eye = new Xwt.Button(IconCache.Get("Timeline/Dot"));
			eye.Style = Xwt.ButtonStyle.Flat;
			eye.CanGetFocus = false;
			name = new Xwt.Label(this.row.Node.Id);
			nameEditor = new Xwt.TextEntry();
			nameEditor.Hide();
			nameEditor.KeyPressed += nameEditor_KeyPressed;
			nameEditor.LostFocus += nameEditor_LostFocus;
		}

		public override void Attach()
		{
			var canvas = roll.Canvas;
			canvas.AddChild(eye);
			canvas.AddChild(name);
			canvas.AddChild(nodeIcon);
			canvas.AddChild(nameEditor);
			base.Attach();
		}

		public override void Detach()
		{
			var canvas = roll.Canvas;
			canvas.RemoveChild(eye);
			canvas.RemoveChild(name);
			canvas.RemoveChild(nodeIcon);
			canvas.RemoveChild(nameEditor);
			base.Detach();
		}

		void nameEditor_KeyPressed(object sender, Xwt.KeyEventArgs e)
		{
			if (e.Key == Xwt.Key.Return) {
				e.Handled = true;
				StopEditing(true);
			} else if (e.Key == Xwt.Key.Escape) {
				StopEditing(false);
				e.Handled = true;
			}
		}

		private void StopEditing(bool commit)
		{
			if (commit) {
				The.Document.History.Add(new Commands.ChangeNodeProperty(row.Node, "Id", nameEditor.Text));
				The.Document.History.Commit("Rename node");
			} else {
				nameEditor.Text = name.Text;
			}
			nameEditor.Hide();
			name.Show();
		}

		void nameEditor_LostFocus(object sender, EventArgs e)
		{
			StopEditing(true);
		}

		public override void HandleClick(Xwt.ButtonEventArgs e)
		{
			base.HandleClick(e);
		}

		public override void HandleDoubleClick(Xwt.ButtonEventArgs e)
		{
			nameEditor.Text = this.row.Node.Id;
			nameEditor.Show();
			name.Hide();
			Refresh();
			nameEditor.SetFocus();
		}

		//void nodeName_MouseDoubleClick(object sender, QEventArgs<QMouseEvent> e)
		//{
		//	var label = (QLabel)sender;
		//	var metrics = new QFontMetrics(label.Font);
		//	int textWidth = metrics.Width(label.Text);
		//	bool hitLabel = e.Event.X() < textWidth;
		//	if (hitLabel) {
		//		RenameNode();
		//	} else {
		//		EnterIntoNode();
		//	}
		//}

		public override void Refresh()
		{
			name.Text = row.Node.Id;
			var layout = new CanvasRowLayout(roll.Canvas, top, height);
			layout.AddSpace(3);
			layout.Add(nodeIcon);
			layout.AddSpace(3);
			if (nameEditor.Visible) {
				layout.Add(nameEditor, CanvasLayoutFlags.Expand);
			} else {
				layout.Add(name, CanvasLayoutFlags.Expand);
			}
			layout.AddSpace(3);
			layout.Add(eye);
			layout.AddSpace(3);
			layout.Realize();
		}

		public override void PaintContent(Xwt.Drawing.Context ctx, double y, double width)
		{
			int numCols = (int)(width / doc.ColumnWidth + 1);
			var c = new KeyTransientCollector();
			var tp = new KeyTransientsPainter(doc.ColumnWidth, top);
			var transients = c.GetTransients(row.Node);
			tp.DrawTransients(transients, ctx);
		}

		private static void EnterIntoNode()
		{
			//var lines = The.Document.SelectedRows;
			//if (lines.Count > 0) {
			//	var container = The.Document.Container.Nodes[lines[0]];
			//	The.Document.History.Add(new Commands.ChangeContainer(container));
			//	The.Document.History.Commit("Change container");
			//}
		}

		private void RenameNode()
		{
			//The.Timeline.EnableActions(false);
			//inplaceEditor = new InplaceTextEditor(nodeName);
			//inplaceEditor.Finished += (text) => {
			//	The.Timeline.EnableActions(true);
			//	//The.Timeline.DockWidget.SetDisabled(false);
			//	The.Document.History.Add(new Commands.ChangeNodeProperty(row.Node, "Id", text));
			//	The.Document.History.Commit("Rename node");
			//};	
		}

		//void expanderIcon_MousePress(object sender, QEventArgs<QMouseEvent> e)
		//{
		//	settings.IsFolded = !settings.IsFolded;
		//	The.Timeline.RefreshLines();
		//}

		//public override void HandleKey(Qt.Key key)
		//{
		//	if (key == Key.Key_Return) {
		//		settings.IsFolded = !settings.IsFolded;
		//		The.Timeline.RefreshLines();
		//	}
		//}

		//[Q_SLOT]
		//void nodeIcon_Clicked()
		//{
		//	settings.IsFolded = !settings.IsFolded;
		//	The.Timeline.Controller.Rebuild();
		//}

		//private void DrawKey(QPainter ptr, KeyTransient m, int x, int y)
		//{
		//	ptr.FillRect(x, y, 2, 2, m.QColor);
		//}
	}
}
