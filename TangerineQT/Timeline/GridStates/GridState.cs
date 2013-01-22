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

		public virtual void OnMousePress(Lime.IntVector2 cell) { }
		public virtual void OnMouseRelease() { }
		public virtual void OnMouseMove(Lime.IntVector2 cell) { }
		public virtual void Paint(QPainter ptr) { }
	}
}
