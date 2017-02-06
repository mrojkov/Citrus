using System;
using System.Windows.Forms;

namespace Launcher
{
	internal partial class MainForm : Form
	{
		private readonly LoggingForm loggingForm = new LoggingForm();
		public readonly LogWriter LogWriter;

		public MainForm()
		{
			LogWriter = new LogWriter(loggingForm.Log);
			InitializeComponent();
		}

		public void SetBuildStatus(string status)
		{
			loggingForm.SetBuildStatus(status);
		}

		public void Log(string line)
		{
			loggingForm.Log(line);
		}

		public void ShowLog()
		{
			if (InvokeRequired) {
				Invoke(new Action(ShowLog));
			}
			else {
				loggingForm.Show();
				Close();
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.F1) {
				ShowLog();
				return true;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}
	}
}
