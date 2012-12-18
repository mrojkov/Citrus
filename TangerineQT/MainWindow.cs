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
		private string dockStateFile = "DockState.bin";

		public static MainWindow Instance;

		public MainWindow()
		{
			Instance = this;
			WindowTitle = "Tangerine";
			new Document();
			InitUI();
			CreateDockWindows();
			//Resize(800, 600);
			Move(300, 300);
			ShowMaximized();
			The.Timeline.GrabKeyboard();

			LoadDockState();
		}

		private void CreateDockWindows()
		{
			var tl = new Timeline();
			this.AddDockWidget(Qt.DockWidgetArea.TopDockWidgetArea, tl.Dock);
			var inspector = new QDockWidget("Inspector", this);
			inspector.ObjectName = "Inspector";
			inspector.SetAllowedAreas(Qt.DockWidgetArea.LeftDockWidgetArea | Qt.DockWidgetArea.RightDockWidgetArea);
			this.AddDockWidget(Qt.DockWidgetArea.LeftDockWidgetArea, inspector);
		}

		void timeline_Paint(object sender, QEventArgs<QPaintEvent> e)
		{
			var tl = (QGLWidget)sender;
			//tl.QglClearColor(new QColor(128, 128, 128));
			//tl.RenderText(50, 50, "Hello QT!");
			//tl.SwapBuffers();
			//var ptr = new QPainter(tl);
			//ptr.Pen = new QPen(new QColor(255, 0, 0), 2);
			//ptr.SetBrush(Qt.BrushStyle.NoBrush);

			//int c = 0;
			//for (int i = 0; i < tl.Width / 10; i++) {
			//	for (int j = 0; j < tl.Height / 10; j++) {
			//		c++;
			//		//ptr.DrawRect(i * 10, j * 10, (i + 1) * 10, (j + 1) * 10);
			//		ptr.DrawRect(i * 10, j * 10, 10, 10);
			//		//ptr.DrawPoint(i, i);
			//		//ptr.DrawRect(0, 0, 10, 10);
			//	}
			//}
			//Console.WriteLine(c);
			//ptr.End();
		}

		private void InitUI()
		{
			QAction saveState = new QAction("&SaveState", this);
			saveState.Triggered += saveState_Triggered;

			QAction restoreState = new QAction("&RestoreState", this);
			restoreState.Triggered += restoreState_Triggered;

			QMenu file = MenuBar.AddMenu("&File");
			file.AddAction(restoreState);
			file.AddAction(saveState);

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

		[STAThread]
		public static int Main(String[] args)
		{
			var app = new QApplication(args);
			new MainWindow();
			return QApplication.Exec();
		}
	}
}