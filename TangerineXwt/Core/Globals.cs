using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine
{
	using TL = Timeline.Timeline;

	public static class The
	{
		//public static Qyoto.QWidget DefaultQtParent { get { return MainWindow.Instance.DefaultQtParent; } }
		public static TL Timeline { get { return TL.Instance; } }
		//public static Inspector Inspector { get { return Inspector.Instance; } }
		public static DocumentView SceneView { get { return Document.Active.View; } }
		public static Document Document { get { return Document.Active; } }
		public static Preferences Preferences { get { return Preferences.Instance; } }
		public static Workspace Workspace { get { return Workspace.Instance; } }
	}
}
