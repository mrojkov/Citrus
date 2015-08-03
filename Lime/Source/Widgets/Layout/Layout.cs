using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public abstract class Layout
	{
		public abstract void OnSizeChanged(Widget widget, Vector2 sizeDelta);
	}
}
