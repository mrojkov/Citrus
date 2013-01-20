using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine.Timeline
{
	/// <summary>
	/// Абстрактный класс для представления одной строки на таймлайне
	/// </summary>
	public class RowView : QObject
	{
		protected Row row;
		protected Document doc { get { return The.Document; } }

		public int Index { get { return row.Index; } }

		protected QWidget slot;
		protected QHBoxLayout layout;

		public RowView(Row row)
		{
			this.row = row;
			slot = new QWidget();
			slot.Palette = new QPalette(Qt.GlobalColor.lightGray);
			layout = new QHBoxLayout(slot);
			layout.Spacing = 0;
			layout.Margin = 0;
		}

		public void SetSelected(bool selected)
		{
			slot.AutoFillBackground = selected;
		}

		public virtual void Attach()
		{
			slot.SetParent(The.Timeline.Roll);
			slot.Resize(The.Timeline.Roll.Width, doc.RowHeight);
		}

		public void SetPosition(int top)
		{
			slot.Move(0, top);
		}

		public virtual void Detach()
		{
			slot.SetParent(null);
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

		public virtual void HandleMousePress(int x)
		{
		}

		public virtual void Refresh()
		{
		}

		public virtual void PaintContent(QPainter ptr, int y, int width)
		{
		}

		public virtual void HandleKey(Qt.Key key)
		{
		}
	}
}
