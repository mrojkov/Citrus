using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Qyoto;

namespace Tangerine
{
	public class Action : QObject
	{
		protected event Lime.BareEventHandler Triggered;
		protected QAction qAction;

		public string Text { get { return qAction.Text; } }
		
		public Action(QWidget parent, string text) {
			qAction = new QAction(text, parent);
			qAction.Triggered += qAction_Triggered;
		}

		[Q_SLOT]
		void qAction_Triggered()
		{
			if (Triggered != null) {
				Triggered();
			}
		}
	}

	public class MainWindow : QMainWindow
	{
		private string dockStateFile = "DockState.bin";

		public static readonly MainWindow Instance = new MainWindow();

		public QMenu FileMenu;

		public MainWindow()
		{
			WindowTitle = "Tangerine";
			InitUI();
			CreateDockWindows();
			Move(300, 300);
			ShowMaximized();
			var doc = new Document(readOnly: false);
			doc.AddSomeNodes();
			doc.OnChanged();
			LoadDockState();
		}

		Action a;

		public void Initialize()
		{
			a = new TimelinePrevNode();
		}

		private void CreateDockWindows()
		{
			this.AddDockWidget(Qt.DockWidgetArea.TopDockWidgetArea, The.Timeline.DockWidget);
			this.AddDockWidget(Qt.DockWidgetArea.LeftDockWidgetArea, The.Inspector.DockWidget);
			this.CentralWidget = The.SceneView.DockWidget;
		}

		private void InitUI()
		{
			QAction saveState = new QAction("&SaveState", this);
			saveState.Triggered += saveState_Triggered;

			QAction restoreState = new QAction("&RestoreState", this);
			restoreState.Triggered += restoreState_Triggered;

			FileMenu = MenuBar.AddMenu("&File");
			FileMenu.AddAction(restoreState);
			FileMenu.AddAction(saveState);

			//Connect(quit, SIGNAL("triggered()"), qApp, SLOT("quit()"));
		}

		[Q_SLOT]
		void restoreState_Triggered()
		{
			LoadDockState();
		}

		[Q_SLOT]
		void saveState_Triggered()
		{
			SaveDockState();
		}

		private void LoadDockState()
		{
			if (File.Exists(dockStateFile)) {
				var bytes = File.ReadAllBytes(dockStateFile);
				this.RestoreState(new QByteArray(bytes), 1);
			}
		}

		private void SaveDockState()
		{
			var state = this.SaveState(1);
			File.WriteAllBytes(dockStateFile, state.ToArray());
		}
	}
}