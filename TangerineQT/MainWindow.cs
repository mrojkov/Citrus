using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Qyoto;

namespace Tangerine
{
	public class MainWindow : QMainWindow
	{
		private string dockStateFile = Lime.Environment.GetPathInsideDataDirectory("Tangerine", "DockState.bin");

		public static readonly MainWindow Instance = new MainWindow();

		public QMdiArea MdiArea;
		public event Lime.BareEventHandler Closing;

		public QWidget DefaultQtParent = new QWidget();

		public MainWindow()
		{
			InitUI();
		}

		private void InitUI()
		{
			WindowTitle = "Tangerine";
			MdiArea = new QMdiArea(this);
			CentralWidget = MdiArea;
			MdiArea.ActivationOrder = QMdiArea.WindowOrder.CreationOrder;
			MdiArea.viewMode = QMdiArea.ViewMode.TabbedView;
			MdiArea.TabShape = QTabWidget.TabShape.Rounded;
			MdiArea.TabsClosable = true;
			MdiArea.TabsMovable = true;
		}

		public void Initialize()
		{
			CreateDockWindows();
			Move(300, 300);
			ShowMaximized();
			var doc = new Document(readOnly: false);
			doc.UpdateViews();
			LoadDockState();
			CloseEvent += OnClose;
		}

		~MainWindow()
		{
			System.Diagnostics.Debug.WriteLine("Destroying " + GetType().Name);
		}

		[Q_SLOT]
		void OnClose(object sender, QEventArgs<QCloseEvent> e)
		{
			if (!Workspace.Close()) {
				e.Event.Ignore();
				e.Handled = true;
			} else {
				SaveDockState();
				DefaultQtParent.Dispose();
				if (Closing != null) {
					Closing();
				}
			}
		}

		public void AddMenu(Menu menu)
		{
			MenuBar.AddMenu(menu.QMenu);
		}

		private void CreateDockWindows()
		{
			this.AddDockWidget(Qt.DockWidgetArea.TopDockWidgetArea, The.Timeline.DockWidget);
			this.AddDockWidget(Qt.DockWidgetArea.LeftDockWidgetArea, The.Inspector.DockWidget);
			this.AddDockWidget(Qt.DockWidgetArea.LeftDockWidgetArea, The.ActorList.DockWidget);
		}

		private void LoadDockState()
		{
			//if (File.Exists(dockStateFile)) {
			//	var bytes = File.ReadAllBytes(dockStateFile);
			//	this.RestoreState(new QByteArray(bytes), 1);
			//}
		}

		private void SaveDockState()
		{
			var state = this.SaveState(1);
			File.WriteAllBytes(dockStateFile, state.ToArray());
		}
	}
}