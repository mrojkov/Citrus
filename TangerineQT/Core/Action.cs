using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	/// <summary>
	/// The Action class provides an abstract user interface action that can be 
	/// inserted into widgets.
	/// </summary>
	public class Action : QObject
	{
		protected event Lime.BareEventHandler Triggered;
		protected QAction qAction;

		public string Text { get { return qAction.Text; } }
		public string Shortcut {
			get { return qAction.Shortcut.ToString(); }
			set { qAction.Shortcut = QKeySequence.FromString(value); }
		}

		public Action(QObject parent, string text)
		{
			qAction = new QAction(text, parent);
			qAction.Triggered += OnTriggered;
		}

		[Q_SLOT]
		protected virtual void OnTriggered()
		{
			if (Triggered != null) {
				Triggered();
			}
		}
	}

	public class ContextualAction : Action
	{
		public ContextualAction(QWidget context, string text)
			: base(context, text)
		{
			qAction.ShortcutContext = ShortcutContext.WidgetWithChildrenShortcut;
			context.AddAction(qAction);
		}
	}
}
