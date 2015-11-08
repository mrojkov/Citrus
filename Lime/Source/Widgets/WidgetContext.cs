using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	/// <summary>
	/// Represents the global context for every widget within the current window.
	/// </summary>
	public interface IWidgetContext : IContext
	{
		/// <summary>
		/// Widget which holds text input focus. Before processing Input.TextInput string 
		/// you should test whether ActiveTextWidget == this. For revoking text input focus from widget 
		/// you should nullify ActiveTextWidget.
		/// </summary>
		IKeyboardInputProcessor ActiveTextWidget { get; set; }
		/// <summary>
		/// On each update cycle active text widget must set this flag true.
		/// </summary>
		bool IsActiveTextWidgetUpdated { get; set; }
		IWindow Window { get; }
		Widget Root { get; }
		float DistanceToNodeUnderCursor { get; set; }
		Node NodeUnderCursor { get; set; }
	}

	/// <summary>
	/// Represents the global context for every widget within the current window.
	/// </summary>
	public class WidgetContext : IWidgetContext
	{
		/// <summary>
		/// Gets the default "Null" global context.
		/// </summary>
		public static IWidgetContext Null { get; private set; }
		/// <summary>
		/// Gets the current global context. The context is set on every window's callback.
		/// </summary>
		public static IWidgetContext Current { get; private set; }

		public IKeyboardInputProcessor ActiveTextWidget { get; set; }
		public bool IsActiveTextWidgetUpdated { get; set; }
		public float DistanceToNodeUnderCursor { get; set; }
		public Node NodeUnderCursor { get; set; }
		public IWindow Window { get; private set; }
		public Widget Root { get; private set; }

		static WidgetContext()
		{
			Current = Null = new WidgetContext();
		}

		private WidgetContext() { }

		public WidgetContext(IWindow window, Widget root)
		{
			Window = window;
			Root = root;
		}

		public ContextScope MakeCurrent()
		{
			var r = new ContextScope { SavedContext = Current };
			Current = this;
			return r;
		}
	}
}
