using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qyoto;

namespace Tangerine
{
	public class ExitAction : Action
	{
		public static ExitAction Instance = new ExitAction();
	
		ExitAction()
		{
			Text = "&Exit";
			Shortcut = new QKeySequence(QKeySequence.StandardKey.Close);
		}

		protected override void OnTriggered()
		{
			MainWindow.Instance.Close();
			//QCoreApplication.Exit();
		}
	}
}
