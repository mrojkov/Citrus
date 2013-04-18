using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	public class ActorList
	{
		public QDockWidget DockWidget { get; private set; }

		public static readonly ActorList Instance = new ActorList();

		private ActorList()
		{
			DockWidget = new QDockWidget("ActorList", The.DefaultQtParent);
			DockWidget.ObjectName = "ActorList";
			DockWidget.Widget = new ActorListWidget();
		}
	}

	public class ActorListWidget : QWidget
	{
		public override QSize SizeHint()
		{
			return new QSize(The.Preferences.InspectorDefaultWidth, 0);
		}
	}
}
