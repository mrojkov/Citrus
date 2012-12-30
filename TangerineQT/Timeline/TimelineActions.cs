using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	public class TimelinePrevNode : Action
	{
		public TimelinePrevNode()
			: base(Timeline.Instance.DockWidget, "Set previous node")
		{
			MainWindow.Instance.FileMenu.AddAction(qAction);
			qAction.ShortcutContext = ShortcutContext.WidgetWithChildrenShortcut;
			qAction.Shortcut = new QKeySequence((int)Qt.Key.Key_Up);
			Triggered += TimelinePrevNode_Triggered;
		}

		void TimelinePrevNode_Triggered()
		{
			throw new NotImplementedException();
		}
	}
}
