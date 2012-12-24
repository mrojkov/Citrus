using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;
using System.Reflection;

namespace Tangerine
{
	public static class PropertyEditorRegistry
	{
		public static PropertyEditor CreateEditor(Lime.Node node, PropertyInfo property)
		{
			PropertyEditor e = null;
			if (property.PropertyType == typeof(Lime.Vector2)) {
				e = new PropertyEditorForVector2();
			} else {
				throw new NotImplementedException();
			}
			e.Node = node;
			e.Property = property;
			return e;
		}
	}

	public abstract class PropertyEditor : QObject
	{
		public Lime.Node Node;
		public PropertyInfo Property;

		protected static void SetWhiteBackground(QWidget w)
		{
			var pal = new QPalette();
			pal.SetColor(w.BackgroundRole, Qt.GlobalColor.white);
			pal.SetColor(QPalette.ColorRole.Text, GlobalColor.black);
			w.Palette = pal;
		}

		protected static void SetTransparentBackground(QWidget w)
		{
			var pal = new QPalette();
			pal.SetColor(w.BackgroundRole, Qt.GlobalColor.transparent);
			pal.SetColor(QPalette.ColorRole.Text, GlobalColor.black);
			w.Palette = pal;
		}

		public abstract void StartEditing();

		public abstract void CreateWidgets(QBoxLayout layout);
		public abstract void SetFromProperty();

		public virtual void Destroy() { }
	}

	public delegate void InplaceEditorHandler(string text);

	public class InplaceTextEditor : QObject
	{
		private QWidget oldFocus;
		private QLineEdit edit;
		
		public event InplaceEditorHandler Finished;
		
		public InplaceTextEditor(QLabel label)
		{
			oldFocus = QApplication.FocusWidget();
			var parent = label.ParentWidget();
			edit = new QLineEdit();
			var p = label.MapTo(parent, new QPoint(0, 0));
			edit.SetParent(parent);
			edit.Move(p.X, p.Y + 2);
			edit.Resize(label.Width, label.Height - 4);
			edit.SetFocus();
			edit.Text = label.Text;
			edit.Raise();
			edit.Frame = false;
			edit.LostFocus += edit_LostFocus;
			edit.ReturnPressed += edit_ReturnPressed;
			edit.Show();
		}

		[Q_SLOT]
		void edit_LostFocus()
		{
			StopEditing();
		}

		[Q_SLOT]
		void edit_ReturnPressed()
		{
			StopEditing();
			if (oldFocus != null) {
				oldFocus.SetFocus();
			}
		}

		void StopEditing()
		{
			edit.SetParent(null);
			if (Finished != null) {
				Finished(edit.Text);
			}
			edit.DeleteLater();
		}
	}

	public class PropertyEditorForVector2 : PropertyEditor
	{
		QLabel label;

		public override void CreateWidgets(QBoxLayout layout)
		{
			label = new QLabel(GetValueFromProperty());
			//edit = new QLineEdit();
			//edit.LostFocus += edit_LostFocus;
			label.MouseDoubleClick += label_MouseDoubleClick;
			layout.AddWidget(label);
			//layout.AddWidget(edit);
			//edit.Hide();
			layout.AddSpacing(4);
		}

		public override void StartEditing()
		{
			edit = new InplaceTextEditor(label);
			edit.Finished += (text) => {
				text = text ?? "";
				var v = (Lime.Vector2)Property.GetValue(Node);
				var vals = text.Split(';');
				if (vals.Length == 2) {
					float x, y;
					if (float.TryParse(vals[0], out x)) {
						v.X = x;
					}
					if (float.TryParse(vals[1], out y)) {
						v.Y = y;
					}
					Property.SetValue(Node, v);
				}
				SetFromProperty();
				label.Show();
			};
		}

		InplaceTextEditor edit;

		void label_MouseDoubleClick(object sender, QEventArgs<QMouseEvent> e)
		{
			StartEditing();
		}

		public override void SetFromProperty()
		{
			label.Text = GetValueFromProperty();
		}

		private string GetValueFromProperty()
		{
			var v = (Lime.Vector2)Property.GetValue(Node);
			return string.Format("{0}; {1}", v.X, v.Y);
		}
	}

	/// <summary>
	/// Абстрактный класс для представления одной строки на таймлайне
	/// </summary>
	public class AbstractRow : QObject
	{
		protected int ActiveRow { get { return The.Timeline.ActiveRow; } }
		protected int RowHeight { get { return The.Timeline.RowHeight; } }
		protected int ColWidth { get { return The.Timeline.ColWidth; } }

		public int Row;

		protected QWidget container;
		protected QHBoxLayout layout;

		public AbstractRow(int row)
		{
			this.Row = row;
			The.Timeline.ActiveRowChanged += Timeline_ActiveRowChanged;
		}

		void Timeline_ActiveRowChanged()
		{
			container.AutoFillBackground = (Row == ActiveRow);
		}

		public virtual void Destroy()
		{
			The.Timeline.ActiveRowChanged -= Timeline_ActiveRowChanged;
			container.SetParent(null);
			//container.DeleteLater();
		}

		public virtual void SetupWidgets()
		{
			container = new QWidget(The.Timeline.NodeRoll);
			container.Move(0, Row * RowHeight);
			container.Resize(The.Timeline.NodeRoll.Width, RowHeight);
			container.Palette = new QPalette(Qt.GlobalColor.lightGray);
			container.AutoFillBackground = (Row == ActiveRow);
			layout = new QHBoxLayout(container);
			layout.Spacing = 0;
			layout.Margin = 0;
		}

		protected QToolButton CreateIconButton(string iconPath)
		{
			var bt = new QToolButton();
			bt.Icon = IconCache.Get(iconPath);
			bt.AutoRaise = true;
			bt.FocusPolicy = Qt.FocusPolicy.NoFocus;
			return bt;
		}

		protected int Top {
			get { return Row * RowHeight; }
		}

		public virtual void HandleMousePress(int x)
		{
		}

		public virtual void PaintContent(QPainter ptr, int width)
		{
		}

		public virtual void HandleKey(Qt.Key key)
		{
		}
	}

	/// <summary>
	/// Строка представляющая один нод на таймлайне
	/// </summary>
	public class NodeRow : AbstractRow
	{
		private Lime.Node node;
		private NodeSettings settings;

		public bool IsFolded
		{
			get { return settings.IsFolded; }
		}

		public NodeRow(int row, Lime.Node node)
			: base(row)
		{
			settings = The.Document.Settings.GetObjectSettings<NodeSettings>(node.Guid.ToString());
			this.node = node;
		}

		public override void SetupWidgets()
		{
			base.SetupWidgets();
			var nodeIcon = CreateIconButton("Nodes/Scene");
			nodeIcon.Clicked += nodeIcon_Clicked;
			layout.AddWidget(nodeIcon, 0);

			var label = new QLabel(node.Id);
			layout.AddWidget(label, 10);

			var bt = CreateIconButton("Timeline/Dot");
			layout.AddWidget(bt, 0);

			bt = CreateIconButton("Timeline/Dot");
			layout.AddWidget(bt, 0);
		}

		public override void HandleKey(Qt.Key key)
		{
			if (key == Key.Key_Return) {
				settings.IsFolded = !settings.IsFolded;
				The.Timeline.Controller.RebuildRows();
			}
		}

		[Q_SLOT]
		void nodeIcon_Clicked()
		{
			settings.IsFolded = !settings.IsFolded;
			The.Timeline.Controller.RebuildRows();
		}

		public virtual void PaintContent(QPainter ptr, int width, int fontHeight)
		{
			int numCols = width / ColWidth + 1;
			var transients = KeyTransientCollector.GetTransients(node);
			DrawTransients(transients, 0, numCols, ptr);
		}

		private void DrawTransients(List<KeyTransient> transients, int startFrame, int endFrame, QPainter ptr)
		{
			for (int i = 0; i < transients.Count; i++) {
				var m = transients[i];
				if (m.Frame >= startFrame && m.Frame < endFrame) {
					int x = ColWidth * (m.Frame - startFrame) + 4;
					int y = Top + m.Line * 6 + 4;
					DrawKey(ptr, m, x, y);
				}
			}
		}

		private void DrawKey(QPainter ptr, KeyTransient m, int x, int y)
		{
			ptr.FillRect(x - 3, y - 3, 6, 6, m.QColor);
			if (m.Length > 0) {
				int x1 = x + m.Length * ColWidth;
				ptr.FillRect(x, y - 1, x1 - x, 2, m.QColor);
			}
		}

	}

	/// <summary>
	/// Строка представляющая одно свойство нода на таймлайне
	/// </summary>
	public class PropertyRow : AbstractRow
	{
		Lime.Node node;
		PropertyInfo prop;
		PropertyEditor editor;

		public PropertyRow(int row, Lime.Node node, PropertyInfo prop)
			: base(row)
		{
			this.node = node;
			this.prop = prop;
			editor = PropertyEditorRegistry.CreateEditor(node, prop);
		}

		public override void Destroy()
		{
			editor.Destroy();
			base.Destroy();
		}

		public override void HandleKey(Qt.Key key)
		{
			if (key == Key.Key_Return) {
				editor.StartEditing();
			}
		}

		public override void SetupWidgets()
		{
			base.SetupWidgets();
			//var bt1 = CreateIconButton("Nodes/Scene");
			//layout.AddWidget(bt1, 0);
			layout.AddSpacing(30);
			var label = new QLabel(prop.Name);
			label.SetFixedWidth(50);
			layout.AddWidget(label);

			var bt1 = CreateIconButton("Timeline/Interpolation/Spline");
			layout.AddWidget(bt1, 0);

			editor.CreateWidgets(layout);
			editor.SetFromProperty();
		}
	}

	public static class NodeExtensions
	{
		public static List<PropertyInfo> GetProperties(this Lime.Node node)
		{
			var result = new List<PropertyInfo>();
			var props = node.GetType().GetProperties();
			foreach (var p in props) {
				var tangAttr = p.GetCustomAttribute(typeof(Lime.TangerinePropertyAttribute));
				if (tangAttr != null) {
					result.Add(p);
				}
			}
			return result;
		}
	}

	public class TimelineController
	{
		public List<AbstractRow> Rows = new List<AbstractRow>();
		public int ActiveRow { get { return The.Timeline.ActiveRow; } set { The.Timeline.ActiveRow = value; } }

		public TimelineController()
		{
			Document.Changed += Document_Changed;
			The.Timeline.KeyPressed += Timeline_KeyPressed;
		}

		void Timeline_KeyPressed(Qt.Key key)
		{
			switch (key) {
				case Qt.Key.Key_Down:
					if (ActiveRow < Rows.Count) {
						ActiveRow++;
						The.Timeline.OnActiveRowChanged();
					}
					break;
				case Qt.Key.Key_Up:
					if (ActiveRow > 0) {
						ActiveRow--;
						The.Timeline.OnActiveRowChanged();
					}
					break;
				case Qt.Key.Key_F1:
					The.Document.OnChanged();
					break;
				default:
					if (Rows.Count > 0) {
						Rows[ActiveRow].HandleKey(key);
					}
					break;
			}
		}

		void Document_Changed(Document document)
		{
			RebuildRows();
		}

		public void RebuildRows()
		{
			The.Timeline.NodeRoll.Hide();
			var document = The.Document;
			foreach (var row in Rows) {
				row.Destroy();
			}
			System.GC.Collect(10, GCCollectionMode.Forced, true);

			Rows.Clear();
			int i = 0;
			var nodes = document.RootNode.Nodes;
			foreach (var node in nodes) {
				var nodeRow = new NodeRow(i++, node);
				Rows.Add(nodeRow);
				if (!nodeRow.IsFolded) {
					foreach (var prop in node.GetProperties()) {
						var propRow = new PropertyRow(i++, node, prop);
						Rows.Add(propRow);
					}
				}
			}
			long t = System.Environment.TickCount;
			foreach (var row in Rows) {
				row.SetupWidgets();
			}
			t = System.Environment.TickCount - t;
			Console.WriteLine("T1: " + t);
			The.Timeline.NodeRoll.Show();
		}
	}
}
