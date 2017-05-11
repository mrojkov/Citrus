#if ORANGE_GUI
using System;
using System.IO;
using Gtk;

namespace Orange
{
	public partial class MainWindow : UserInterface
	{
		internal MainWindow()
		{
			Instance = this;
		}

		public override Target GetActiveTarget()
		{
			return The.Workspace.Targets[platformPicker.Active];
		}

		class LogWriter : TextWriter
		{
			TextView textView;
#if !WIN
			int bufferedLines;
#endif

			public LogWriter(TextView textView)
			{
				this.textView = textView;
			}

			public override void WriteLine(string value)
			{
				Write(value + '\n');
			}

			public override void Write(string value)
			{
#pragma warning disable 618
				textView.Buffer.Insert(textView.Buffer.EndIter, value);
				while (Application.EventsPending())
					Application.RunIteration();
#if !WIN
				if (bufferedLines > 4) {
					bufferedLines = 0;
#endif
				textView.ScrollToIter(textView.Buffer.EndIter, 0, false, 0, 0);
#if !WIN
				}
				bufferedLines++;
#endif
			}

			public override System.Text.Encoding Encoding
			{
				get
				{
					throw new NotImplementedException();
				}
			}
		}

		public override void ClearLog()
		{
			OutputPane.Buffer.Clear();
		}

		public override void ScrollLogToEnd()
		{
			OutputPane.ScrollToIter(OutputPane.Buffer.EndIter, 0, false, 0, 0);
		}

		public override void OnWorkspaceOpened()
		{
			CitrusProjectChooser.SelectFilename(The.Workspace.ProjectFile);
		}

		public override bool AskConfirmation(string text)
		{
			var box = new MessageDialog(NativeWindow,
				DialogFlags.Modal, MessageType.Question,
				ButtonsType.YesNo,
				text);
			box.Title = "Orange";
			box.Modal = true;
			var result = box.Run();
			box.Destroy();
			return result == (int)ResponseType.Yes;
		}

		public override bool AskChoice(string text, out bool yes)
		{
			var dialog = new Dialog("Orange", NativeWindow, DialogFlags.Modal,
				Stock.Yes, ResponseType.Yes,
				Stock.No, ResponseType.No,
				Stock.Cancel, ResponseType.Cancel);
			var label = new Label(text) {
				Justify = Justification.Center
			};
			label.SetPadding(50, 30);
			dialog.VBox.Add(label);
			dialog.ShowAll();
			var result = dialog.Run();
			dialog.Destroy();
			yes = (result == (int)ResponseType.Yes);
			return (result == (int)ResponseType.Yes) || (result == (int)ResponseType.No);
		}

		public override void RefreshMenu()
		{
			var picker = ActionPicker;
			var activeText = picker.ActiveText;
			var count = picker.Model.IterNChildren();
			for (var i = 0; i < count; i++) {
				picker.RemoveText(0);
			}
			var active = 0;
			var c = 0;
			var items = The.MenuController.GetVisibleAndSortedItems();
			foreach (var item in items) {
				picker.AppendText(item.Label);
				if (item.Label == activeText) {
					active = c;
				}
				c++;
			}
			picker.Active = active;
		}


		private void Execute(System.Action action)
		{
			var startTime = DateTime.Now;
			The.Workspace.Save();
			EnableControls(false);
			try {
				try {
					ClearLog();
					if (DoesNeedSvnUpdate()) {
						var builder = new SolutionBuilder(The.Workspace.ActivePlatform, The.Workspace.CustomSolution);
						builder.SvnUpdate();
					}
					The.Workspace?.AssetFiles?.Rescan();
					action();
				}
				catch (Exception exc) {
					var deepestException = exc;
					while (deepestException.InnerException != null) {
						deepestException = deepestException.InnerException;
					}
					Console.WriteLine(deepestException.Message);
					Console.WriteLine(deepestException.StackTrace);
				}
				ScrollLogToEnd();
			}
			finally {
				EnableControls(true);
			}
			ShowTimeStatistics(startTime);
		}

		private void EnableControls(bool value)
		{
			GoButton.Sensitive = value;
			ActionPicker.Sensitive = value;
			platformPicker.Sensitive = value;
			CitrusProjectChooser.Sensitive = value;
			UpdateBeforeBuildCheckbox.Sensitive = value;
		}

		public override bool DoesNeedSvnUpdate()
		{
			return UpdateBeforeBuildCheckbox.Active;
		}

		void ShowTimeStatistics(DateTime startTime)
		{
			var endTime = DateTime.Now;
			var delta = endTime - startTime;
			Console.WriteLine("Elapsed time {0}:{1}:{2}", delta.Hours, delta.Minutes, delta.Seconds);
		}

		bool citrusProjectChooserRecursed = false;

		void CitrusProjectChooser_SelectionChanged(object sender, EventArgs e)
		{
			if (!citrusProjectChooserRecursed) {
				citrusProjectChooserRecursed = true;
				try {
					if (CitrusProjectChooser.Filename != null) {
						The.Workspace.Open(CitrusProjectChooser.Filename);
						UpdatePlatformPicker();
					}
				}
				finally {
					citrusProjectChooserRecursed = false;
				}
			}
		}

		void GoButton_Clicked(object sender, EventArgs e)
		{
			var menuItem = The.MenuController.Items.Find(i => i.Label == ActionPicker.ActiveText);
			if (menuItem != null) {
				Execute(menuItem.Action);
			}
		}

		public override void ShowError(string message)
		{
			var dialog = new MessageDialog(NativeWindow,
				DialogFlags.DestroyWithParent,
				MessageType.Error, ButtonsType.Close,
				message);
			dialog.Title = "Orange";
			dialog.Run();
			dialog.Destroy();
		}

		protected void Window_Hidden(object sender, EventArgs e)
		{
			The.Workspace.Save();
			Application.Quit();
		}
	}
}
#endif // ORANGE_GUI
