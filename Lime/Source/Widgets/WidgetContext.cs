using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	/// <summary>
	/// Represents the global context for every widget within the current window.
	/// </summary>
	public class WidgetContext : Context
	{
		/// <summary>
		/// Gets the current global context. The context is set on every window's callback.
		/// </summary>
		public static WidgetContext Current { get; private set; }

		public Widget Root { get; private set; }

		public Node NodeUnderMouse { get; internal set; }

		/// <summary>
		/// Gets or sets the mouse cursor. The WindowWidget resets mouse cursor to the default value in the beginning of every update cycle.
		/// </summary>
		public MouseCursor MouseCursor { get; set; }
		public Camera3D CurrentCamera { get; set; }

		public WidgetContext(Widget root) : base("Current")
		{
			Root = root;
			if (Current == null) {
				Current = this;
			}
		}
	}
}
