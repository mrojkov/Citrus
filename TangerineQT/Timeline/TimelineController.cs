using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;
using System.Reflection;

namespace Tangerine
{
	/// <summary>
	/// Абстрактный класс для представления одной строки на таймлайне
	/// </summary>
	public class AbstractItem : QObject
	{
		protected int ActiveRow { get { return The.Timeline.ActiveRow; } }
		protected int RowHeight { get { return The.Timeline.RowHeight; } }
		protected int ColWidth { get { return The.Timeline.ColWidth; } }

		public int Row { get; set; }

		protected QWidget slot;
		protected QHBoxLayout layout;

		public AbstractItem()
		{
		}

		void Timeline_ActiveRowChanged()
		{
			slot.AutoFillBackground = (Row == ActiveRow);
		}

		public virtual void Attach(int row)
		{
			Row = row;
			slot.SetParent(The.Timeline.NodeRoll);
			slot.Move(0, row * RowHeight);
			slot.Resize(The.Timeline.NodeRoll.Width, RowHeight);
			The.Timeline.ActiveRowChanged += Timeline_ActiveRowChanged;
		}

		public virtual void Detach()
		{
			The.Timeline.ActiveRowChanged -= Timeline_ActiveRowChanged;
			slot.SetParent(null);
		}

		public virtual void CreateWidgets()
		{
			slot = new QWidget();
			slot.Palette = new QPalette(Qt.GlobalColor.lightGray);
			layout = new QHBoxLayout(slot);
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

		protected QLabel CreateImageWidget(string iconPath)
		{
			var label = new QLabel();
			var icon = IconCache.Get(iconPath);
			label.Pixmap = icon.Pixmap(16, 16);
			label.SetContentsMargins(2, 0, 2, 0);
			return label;
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
	public class NodeItem : AbstractItem
	{
		private Lime.Node node;
		private NodeSettings settings;

		public bool IsFolded
		{
			get { return settings.IsFolded; }
		}

		public NodeItem(Lime.Node node)
		{
			settings = The.Document.Settings.GetObjectSettings<NodeSettings>(node.Guid.ToString());
			this.node = node;
		}

		public override void CreateWidgets()
		{
			base.CreateWidgets();
			var expanderIcon = CreateImageWidget("Timeline/Collapsed");
			expanderIcon.MousePress += expanderIcon_MousePress;
			layout.AddWidget(expanderIcon);

			var nodeIcon = CreateImageWidget("Nodes/Scene");
			//nodeIcon.Clicked += nodeIcon_Clicked;
			layout.AddWidget(nodeIcon);

			var label = new QLabel(node.Id);
			layout.AddWidget(label, 10);

			var bt = CreateImageWidget("Timeline/Dot");
			layout.AddWidget(bt);

			bt = CreateImageWidget("Timeline/Dot");
			layout.AddWidget(bt, 0);
		}

		void expanderIcon_MousePress(object sender, QEventArgs<QMouseEvent> e)
		{
			settings.IsFolded = !settings.IsFolded;
			The.Timeline.Controller.Rebuild();
		}

		public override void HandleKey(Qt.Key key)
		{
			if (key == Key.Key_Return) {
				settings.IsFolded = !settings.IsFolded;
				The.Timeline.Controller.Rebuild();
			}
		}

		//[Q_SLOT]
		//void nodeIcon_Clicked()
		//{
		//	settings.IsFolded = !settings.IsFolded;
		//	The.Timeline.Controller.Rebuild();
		//}

		public override void PaintContent(QPainter ptr, int width)
		{
			int numCols = width / ColWidth + 1;
			var c = new KeyTransientCollector();
			var tp = new TransientsPainter(ColWidth, Top);
			var transients = c.GetTransients(node);
			tp.DrawTransients(transients, 0, numCols, ptr);
		}
	}

	/// <summary>
	/// Строка представляющая одно свойство нода на таймлайне
	/// </summary>
	public class PropertyItem : AbstractItem
	{
		Lime.Node node;
		PropertyInfo property;
		PropertyEditor editor;

		public PropertyItem(Lime.Node node, PropertyInfo property)
		{
			this.node = node;
			this.property = property;
			editor = PropertyEditorRegistry.CreateEditor(node, property);
		}

		public override void HandleKey(Qt.Key key)
		{
			if (key == Key.Key_Return) {
				editor.StartEditing();
			}
		}

		public override void CreateWidgets()
		{
			base.CreateWidgets();

			layout.AddSpacing(30);
			var label = new QLabel(property.Name);
			label.SetFixedWidth(50);
			layout.AddWidget(label);

			var iconButton = CreateIconButton("Timeline/Interpolation/Spline");
			layout.AddWidget(iconButton, 0);

			editor.CreateWidgets(layout);
		}

		public override void PaintContent(QPainter ptr, int width)
		{
			int numCols = width / ColWidth + 1;
			var c = new KeyTransientCollector(property);
			var tp = new TransientsPainter(ColWidth, Top);
			var transients = c.GetTransients(node);
			tp.DrawTransients(transients, 0, numCols, ptr);
		}
	}

	public class TransientsPainter
	{
		int colWidth;
		int top;

		public TransientsPainter(int colWidth, int top)
		{
			this.colWidth = colWidth;
			this.top = top;
		}

		public void DrawTransients(List<KeyTransient> transients, int startFrame, int endFrame, QPainter ptr)
		{
			for (int i = 0; i < transients.Count; i++) {
				var m = transients[i];
				if (m.Frame >= startFrame && m.Frame < endFrame) {
					int x = colWidth * (m.Frame - startFrame) + 4;
					int y = top + m.Line * 6 + 4;
					DrawKey(ptr, m, x, y);
				}
			}
		}

		private void DrawKey(QPainter ptr, KeyTransient m, int x, int y)
		{
			ptr.FillRect(x - 3, y - 3, 6, 6, m.QColor);
			if (m.Length > 0) {
				int x1 = x + m.Length * colWidth;
				ptr.FillRect(x, y - 1, x1 - x, 2, m.QColor);
			}
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
		public List<AbstractItem> Items = new List<AbstractItem>();
		public int ActiveItem { get { return The.Timeline.ActiveRow; } set { The.Timeline.ActiveRow = value; } }

		public TimelineController()
		{
			Document.Changed += Document_Changed;
			The.Timeline.KeyPressed += Timeline_KeyPressed;
		}

		void Timeline_KeyPressed(Qt.Key key)
		{
			switch (key) {
				case Qt.Key.Key_Down:
					if (ActiveItem < Items.Count) {
						ActiveItem++;
						The.Timeline.OnActiveRowChanged();
					}
					break;
				case Qt.Key.Key_Up:
					if (ActiveItem > 0) {
						ActiveItem--;
						The.Timeline.OnActiveRowChanged();
					}
					break;
				case Qt.Key.Key_F1:
					The.Document.OnChanged();
					break;
				default:
					if (Items.Count > 0) {
						Items[ActiveItem].HandleKey(key);
					}
					break;
			}
		}

		void Document_Changed(Document document)
		{
			Rebuild();
		}

		Dictionary<string, AbstractItem> rowCache = new Dictionary<string, AbstractItem>();

		NodeItem GetNodeItem(int index, Lime.Node node)
		{
			string key = node.Guid.ToString();
			AbstractItem item;
			if (!rowCache.TryGetValue(key, out item)) {
				item = new NodeItem(node);
				item.CreateWidgets();
				rowCache[key] = item;	
			}
			item.Attach(index);
			return item as NodeItem;
		}

		PropertyItem GetPropertyItem(int index, Lime.Node node, PropertyInfo property)
		{
			string key = node.Guid.ToString() + '#' + property.Name;
			AbstractItem item;
			if (!rowCache.TryGetValue(key, out item)) {
				item = new PropertyItem(node, property);
				item.CreateWidgets();
				rowCache[key] = item;
			}
			item.Attach(index);
			return item as PropertyItem;
		}

		public void Rebuild()
		{
			The.Timeline.NodeRoll.Hide();
			var document = The.Document;
			foreach (var row in Items) {
				row.Detach();
			}
			Items.Clear();
			int i = 0;
			var nodes = document.RootNode.Nodes;
			foreach (var node in nodes) {
				var nodeItem = GetNodeItem(i++, node);
				Items.Add(nodeItem);
				if (!nodeItem.IsFolded) {
					foreach (var prop in node.GetProperties()) {
						var propItem = GetPropertyItem(i++, node, prop);
						Items.Add(propItem);
					}
				}
			}
			The.Timeline.NodeRoll.Show();
			The.Timeline.KeyGrid.Update();
			The.Timeline.OnActiveRowChanged();
		}
	}
}
