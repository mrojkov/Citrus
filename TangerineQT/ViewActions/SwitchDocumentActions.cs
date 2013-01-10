using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qyoto;

namespace Tangerine
{
	public class ChooseNextDocumentAction : Action
	{
		public static ChooseNextDocumentAction Instance = new ChooseNextDocumentAction();

		ChooseNextDocumentAction()
		{
			Text = "Next Document";
			Shortcut = "Ctrl+Tab";
		}

		protected override void OnTriggered()
		{
			The.MainWindow.MdiArea.ActivateNextSubWindow();
		}
	}

	public class ChoosePrevDocumentAction : Action
	{
		public static ChoosePrevDocumentAction Instance = new ChoosePrevDocumentAction();

		ChoosePrevDocumentAction()
		{
			Text = "Previous Document";
			Shortcut = "Ctrl+Shift+Tab";
		}

		protected override void OnTriggered()
		{
			The.MainWindow.MdiArea.ActivatePreviousSubWindow();
		}
	}
}
