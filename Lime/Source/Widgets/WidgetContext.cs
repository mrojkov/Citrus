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

		/// <summary>
		/// Widget which holds text input focus. Before processing Input.TextInput string 
		/// you should test whether ActiveTextWidget == this. For revoking text input focus from widget 
		/// you should nullify ActiveTextWidget.
		/// </summary>
		public IKeyboardInputProcessor ActiveTextWidget { get; set; }
		/// <summary>
		/// On each update cycle active text widget must set this flag true.
		/// </summary>
		public bool IsActiveTextWidgetUpdated { get; set; }
		public float DistanceToNodeUnderCursor { get; set; }
		public Node NodeUnderCursor { get; set; }
		public Widget Root { get; private set; }

		public WidgetContext(Widget root) : base("Current")
		{
			Root = root;
			Current = this;
		}
	}
}
