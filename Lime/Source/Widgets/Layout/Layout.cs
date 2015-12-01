using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public interface ILayout
	{
		List<Rectangle> ContentRectangles { get; }

		void OnSizeChanged(Widget widget, Vector2 sizeDelta);
	}
}
