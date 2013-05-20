using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
#if SEXY_PANED
	public class SexyPanel
	{
		Xwt.IContainerEventSink<SexyPanel> parent;
		Xwt.Widget content;

		public SexyPanel(Xwt.IContainerEventSink<SexyPanel> parent)
		{
			this.parent = parent;
		}

		public Xwt.Widget Content
		{
			get { return content; }
			set {
				Xwt.Widget oldContent = content;
				content = value;
				parent.ChildReplaced(this, oldContent, value);
			}
		}
	}

	public abstract class SexyPaned : Xwt.Canvas, Xwt.IContainerEventSink<SexyPanel>
	{
		protected const float dragBandWidth = 5;
		protected bool dragging;
		protected double position;
		protected SexyPanel[] panels = new SexyPanel[2];

		public double Position
		{
			get { return position; }
			set { SetPosition(value); }
		}

		public SexyPanel Panel1
		{
			get { return panels[0]; }
		}

		public SexyPanel Panel2
		{
			get { return panels[1]; }
		}

		protected SexyPaned()
		{
			panels[0] = new SexyPanel(this);
			panels[1] = new SexyPanel(this);
		}

		public void ChildChanged(SexyPanel child, string hint)
		{
		}

		public void ChildReplaced(SexyPanel child, Xwt.Widget oldWidget, Xwt.Widget newWidget)
		{
			if (oldWidget != null) {
				this.RemoveChild(oldWidget);
			}
			if (newWidget != null) {
				this.AddChild(newWidget);
			}
			RefreshMinSize();
		}

		protected void SetWindowCursor(Xwt.CursorType cursor)
		{
			(this.ParentWindow as Xwt.Window).Content.Cursor = cursor;
		}

		protected abstract void RefreshMinSize();
		protected abstract void SetPosition(double value);
	}
#endif
}
