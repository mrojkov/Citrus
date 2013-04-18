using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
	public class GridSelection
	{
		public List<Lime.IntRectangle> Rectangles = new List<Lime.IntRectangle>();

		public GridSelection() {}

		public void Select(Lime.IntVector2 cell)
		{
			var rect = new Lime.IntRectangle(cell, cell + Lime.IntVector2.One);
			Rectangles.Add(rect);
		}

		public void Select(Lime.IntRectangle rect)
		{
			Rectangles.Add(rect);
		}

		public bool Contains(Lime.IntVector2 cell)
		{
			foreach (var rect in Rectangles) {
				if (rect.Contains(cell)) {
					return true;
				}
			}
			return false;
		}

		public bool Empty
		{
			get { return Rectangles.Count == 0; }
		}
	}
}
