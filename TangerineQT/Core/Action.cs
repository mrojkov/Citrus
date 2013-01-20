using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	public class Action
	{
		public QAction QAction;
		private SlotWrapper triggerWrapper;

		public string Text
		{
			get { return QAction.Text; }
			set { QAction.Text = value; } 
		}

		public QKeySequence Shortcut
		{ 
			get { return QAction.Shortcut; }
			set { QAction.Shortcut = value; }
		}

		public bool Enabled
		{
			get { return QAction.Enabled; }
			set { QAction.Enabled = value; }
		}

		public Action()
		{
			triggerWrapper = new SlotWrapper(OnTriggered);
			QAction = new QAction(The.DefaultQtParent);
			QAction.Triggered += triggerWrapper.OnSignal;
		}

		protected virtual void OnTriggered() { }
	}

	public class ContextualAction : Action
	{
		public ContextualAction(QWidget context, string text)
			: base()
		{
			Text = text;
			QAction.ShortcutContext = Qt.ShortcutContext.WidgetWithChildrenShortcut;
			context.AddAction(QAction);
		}
	}
}
