using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.Timeline
{
	/// <summary>
	/// Абстрактный класс для представления одной строки на таймлайне
	/// </summary>
	public class RowView
	{
		public int Index { get { return row.Index; } }
		protected Row row;
		protected bool selected;
		protected Document doc { get { return The.Document; } }
		protected Roll roll { get { return The.Timeline.Roll; } }
		protected double top { get { return GetTop(); } }
		protected double width { get { return The.Timeline.Roll.Width; } }
		protected double height { get { return The.Preferences.TimelineRowHeight; } }

		public RowView(Row row)
		{
			this.row = row;
		}

		public void SetSelected(bool selected)
		{
			this.selected = selected;
			Refresh();
		}

		public virtual void Attach()
		{
			roll.Canvas.Drawn += roll_Drawn;
		}

		public virtual void Detach()
		{
			roll.Canvas.Drawn -= roll_Drawn;
		}

		void roll_Drawn(Xwt.Drawing.Context ctx, Xwt.Rectangle dirtyRect)
		{
			ctx.SetColor(selected ? Colors.SelectedRow : Colors.NotSelectedRow);
			ctx.Rectangle(0, top, width, height + 1);
			ctx.Fill();
		}

		private double GetTop()
		{
			double y = this.row.Index * The.Preferences.TimelineRowHeight;
			return y;
		}

		public virtual void HandleClick(Xwt.ButtonEventArgs e)
		{
			The.Document.History.Add(new Commands.SelectRows(Index));
			The.Document.History.Commit("Select Line");
		}

		public virtual void HandleDoubleClick(Xwt.ButtonEventArgs e) { }

		public virtual void Refresh()
		{
		}

		public virtual void PaintContent(Xwt.Drawing.Context ctx, double y, double width)
		{
		}
	}
}
