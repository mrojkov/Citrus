using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qyoto;

namespace Tangerine.Timeline
{
	public class GridState
	{
		protected Document doc { get { return The.Document; } }

		public virtual void OnMousePress(QMouseEvent e) { }
		public virtual void OnMouseRelease(QMouseEvent e) { }
		public virtual void OnMouseMove(QMouseEvent e) { }
		public virtual void Paint(QPainter ptr) { }
	}
}
