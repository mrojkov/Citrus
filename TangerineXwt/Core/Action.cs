using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xwt;

namespace Tangerine
{
	public class Action
	{
		public string Text
		{
			get;
			set;
		}

		public string Shortcut
		{
			get;
			set;
		}

		public bool Enabled
		{
			get;
			set;
		}

		protected virtual void OnTriggered() { }
	}

	public class ContextualAction : Action
	{
		public ContextualAction(Widget context, string text)
			: base()
		{
			Text = text;
			context.KeyPressed += context_KeyPressed;
//			QAction.ShortcutContext = Qt.ShortcutContext.WidgetWithChildrenShortcut;
//			context.AddAction(QAction);
		}

		void context_KeyPressed(object sender, KeyEventArgs e)
		{
			Console.WriteLine("XXX");
		}
	}
}
