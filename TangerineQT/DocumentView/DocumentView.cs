using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;
using System.IO;

namespace Tangerine
{
	public class DocumentView : QObject
	{
		public QMdiSubWindow MdiSubWindow { get; private set; }
		public QWidget ContentWidget { get; private set; }
		public Document Document { get; private set; }
		public string Title
		{
			get { return Path.GetFileName(Document.Path); }
		}

		bool isBeingClosed;

		public DocumentView(Document document)
		{
			this.Document = document;
			MdiSubWindow = new QMdiSubWindow();
			MdiSubWindow.WindowTitle = Title;
			The.MainWindow.MdiArea.AddSubWindow(MdiSubWindow);
			ContentWidget = new DocumentCanvas(MdiSubWindow);
			MdiSubWindow.Widget = ContentWidget;
			MdiSubWindow.CloseEvent += MidSubWindow_CloseEvent;
			MdiSubWindow.SetAttribute(WidgetAttribute.WA_DeleteOnClose);
			MdiSubWindow.WindowModified = true;
			MdiSubWindow.AboutToActivate += MdiSubWindow_AboutToActivate;
			MdiSubWindow.ShowMaximized();
		}

		[Q_SLOT]
		void MdiSubWindow_AboutToActivate()
		{
			Document.Active = Document;
			The.Document.RebuildRows();
			The.Document.OnChanged();
			MdiSubWindow.ShowMaximized();
		}

		public void Close()
		{
			isBeingClosed = true;
			MdiSubWindow.Close();
			The.MainWindow.MdiArea.RemoveSubWindow(MdiSubWindow);
		}

		void MidSubWindow_CloseEvent(object sender, QEventArgs<QCloseEvent> e)
		{
			if (!isBeingClosed && !Document.Close()) {
				e.Event.Ignore();
				e.Handled = true;
			}
		}
	}
}