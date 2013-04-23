using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
	public static class PanedUtils
	{
		public static SexyPanel GetOwnedPanel(Xwt.Widget widget)
		{
			var paned = widget.Parent as SexyPaned;
			if (paned != null) {
				if (paned.Panel1.Content == widget) {
					return paned.Panel1;
				}
				if (paned.Panel2.Content == widget) {
					return paned.Panel2;
				}
			}
			return null;
		}

		public static void ExtractFromPaned(Xwt.Widget widget)
		{
			var panel = GetOwnedPanel(widget);
			if (panel != null) {
				panel.Content = null;
			}
		}
	}
}
