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
		public static readonly List<Key> NodeCaptureKeys = new List<Key> { Key.Mouse0, Key.Mouse1, Key.Mouse2 };
		/// <summary>
		/// Gets the current global context. The context is set on every window's callback.
		/// </summary>
		public static WidgetContext Current { get; private set; }

		public Widget Root { get; private set; }

		/// <summary>
		/// Gets the topmost node which the mouse cursor is up on.
		/// </summary>
		public Node NodeUnderMouse { get; internal set; }

		/// <summary>
		/// Gets the topmost node under mouse at the moment any of mouse buttons down.
		/// </summary>
		public Node NodeCapturedByMouse { get; internal set; }

		/// <summary>
		/// Gets or sets the mouse cursor. The WindowWidget resets mouse cursor to the default value in the beginning of every update cycle.
		/// </summary>
		public MouseCursor MouseCursor { get; set; }

		public GestureManager GestureManager { get; set; }

		public WidgetContext(Widget root) : base(Property.Create(() => Current, (v) => Current = v))
		{
			Root = root;
			if (Current == null) {
				Current = this;
			}
		}
	}
}
