using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	/// <summary>
	/// Абстрактный класс для представления одной строки на таймлайне
	/// </summary>
	public class InspectorItem : QObject
	{
		public int Row { get; set; }

		protected QWidget slot;
		protected QHBoxLayout layout;

		public InspectorItem()
		{
		}

		void Timeline_ActiveRowChanged()
		{
			slot.AutoFillBackground = (Row == The.Inspector.ActiveRow);
		}

		public virtual void Attach(int row)
		{
			Row = row;
			slot.SetParent(The.Timeline.NodeRoll);
			slot.Move(0, row * Inspector.RowHeight);
			slot.Resize(The.Timeline.NodeRoll.Width, Inspector.RowHeight);
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

		protected int Top
		{
			get { return Row * Inspector.RowHeight; }
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

	public class Inspector
	{
		public static int RowHeight { get { return The.Preferences.InspectorRowHeight; } }

		public int ActiveRow;

		public QDockWidget DockWidget { get; private set; }

		public static readonly Inspector Instance = new Inspector();

		private Inspector()
		{
			DockWidget = new QDockWidget("Inspector", MainWindow.Instance);
			DockWidget.ObjectName = "Inspector";
			DockWidget.Widget = new InspectorWidget();
		}
	}

	public class InspectorWidget : QWidget
	{
		public override QSize SizeHint()
		{
			return new QSize(The.Preferences.InspectorDefaultWidth, 0);
		}
	}
}
