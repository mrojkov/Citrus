using System;
using System.Windows.Forms;

namespace Launcher
{
	public partial class LoggingForm : Form
	{
		public LoggingForm()
		{
			InitializeComponent();
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			Application.Exit();
		}

		public void SetBuildStatus(string status)
		{
			InvokeIfRequired(arg => Status.Text = arg, status);
		}

		public void Log(string line)
		{
			InvokeIfRequired(arg => TextBox.Text += arg, line);
		}

		private void InvokeIfRequired(Action<string> action, string arg)
		{
			if (InvokeRequired) {
				Invoke(action, arg);
			}
			else {
				action(arg);
			}

		}

		private void CopyButton_Click(object sender, EventArgs e)
		{
			Clipboard.SetText(TextBox.Text);
		}
	}
}
