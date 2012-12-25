using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

public delegate void InplaceEditorHandler(string text);

namespace Tangerine
{
	public class InplaceTextEditor : QObject
	{
		private QWidget oldFocus;
		private QLineEdit edit;

		public event InplaceEditorHandler Finished;

		public InplaceTextEditor(QLabel label)
		{
			oldFocus = QApplication.FocusWidget();
			var parent = label.ParentWidget();
			edit = new QLineEdit();
			var p = label.MapTo(parent, new QPoint(0, 0));
			edit.SetParent(parent);
			edit.Move(p.X, p.Y + 2);
			edit.Resize(label.Width, label.Height - 4);
			edit.SetFocus();
			edit.Text = label.Text;
			edit.Raise();
			edit.Frame = false;
			edit.LostFocus += edit_LostFocus;
			edit.ReturnPressed += edit_ReturnPressed;
			edit.Show();
		}

		[Q_SLOT]
		void edit_LostFocus()
		{
			StopEditing();
		}

		[Q_SLOT]
		void edit_ReturnPressed()
		{
			StopEditing();
			if (oldFocus != null) {
				oldFocus.SetFocus();
			}
		}

		void StopEditing()
		{
			edit.SetParent(null);
			if (Finished != null) {
				Finished(edit.Text);
			}
			edit.DeleteLater();
		}
	}
}
