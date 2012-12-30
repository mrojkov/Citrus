using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	public class SceneView
	{
		public QDockWidget DockWidget { get; private set; }

		public static readonly SceneView Instance = new SceneView();

		private SceneView()
		{
			DockWidget = new QDockWidget("SceneView", MainWindow.Instance);
			DockWidget.ObjectName = "SceneView";
		}
	}
}