using System;
using System.IO;
using Lime;
using System.Reflection;
using System.Collections.Generic;

namespace Orange
{
	public partial class MainWindow : UserInterface
	{
		internal MainWindow()
		{
			Instance = this;
		}

		public override TargetPlatform GetActivePlatform() 
		{
			return (TargetPlatform)PlatformPicker.Active;
		}
				
		class LogWriter : TextWriter
		{
			Gtk.TextView textView;
#if !WIN
			int bufferedLines;
#endif
			
			public LogWriter(Gtk.TextView textView)
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
				while (Gtk.Application.EventsPending())
					Gtk.Application.RunIteration();
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

			public override System.Text.Encoding Encoding {
				get {
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
			var box = new Gtk.MessageDialog(NativeWindow,
				Gtk.DialogFlags.Modal, Gtk.MessageType.Question,
				Gtk.ButtonsType.YesNo,
				text);
			box.Title = "Orange";
			box.Modal = true;
			int result = box.Run();
			box.Destroy();
			return result == (int)Gtk.ResponseType.Yes;
		}

		public override bool AskChoice(string text, out bool yes)
		{
			var dialog = new Gtk.Dialog("Orange", NativeWindow, Gtk.DialogFlags.Modal,
				Gtk.Stock.Yes, Gtk.ResponseType.Yes,
				Gtk.Stock.No, Gtk.ResponseType.No,
				Gtk.Stock.Cancel, Gtk.ResponseType.Cancel);
			var label = new Gtk.Label(text);
			label.Justify = Gtk.Justification.Center;
			label.SetPadding(50, 30);
			dialog.VBox.Add(label);
			dialog.ShowAll();
			int result = dialog.Run();
			dialog.Destroy();
			yes = (result == (int)Gtk.ResponseType.Yes);
			return (result == (int)Gtk.ResponseType.Yes) || (result == (int)Gtk.ResponseType.No);
		}

		public override void RefreshMenu()
		{
			var picker = ActionPicker;
			var activeText = picker.ActiveText;
			int count = picker.Model.IterNChildren();
			for (int i = 0; i < count; i++) {
				picker.RemoveText(0);
			}
			int active = 0;
			int c = 0;
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


		private void Execute(Action action)
		{
			if (!CheckTargetAvailability())
				return;
			DateTime startTime = DateTime.Now;
			The.Workspace.Save();
			EnableControls(false);
			var platform = (TargetPlatform)this.PlatformPicker.Active;
			try {
				try {
					ClearLog();
					if (DoesNeedSvnUpdate()) {
						var builder = new SolutionBuilder(The.Workspace.ActivePlatform);
						builder.SvnUpdate();
					}
					The.Workspace.AssetFiles.Rescan();
					action();
				} catch (System.Exception exc) {
					if (exc.InnerException != null) {
						Console.WriteLine(exc.InnerException.Message);
					} else {
						Console.WriteLine(exc.Message);
					}
				}
				ScrollLogToEnd();
			} finally {
				EnableControls(true);
			}
			ShowTimeStatistics(startTime);
		}

		private void EnableControls(bool value)
		{
			this.GoButton.Sensitive = value;
			this.ActionPicker.Sensitive = value;
			this.PlatformPicker.Sensitive = value;
			this.CitrusProjectChooser.Sensitive = value;
			this.UpdateBeforeBuildCheckbox.Sensitive = value;
		}

		public override bool DoesNeedSvnUpdate()
		{
			return UpdateBeforeBuildCheckbox.Active;
		}

		void ShowTimeStatistics(DateTime startTime)
		{
			DateTime endTime = DateTime.Now;
			TimeSpan delta = endTime - startTime;
			Console.WriteLine("Elapsed time {0}:{1}:{2}", delta.Hours, delta.Minutes, delta.Seconds);
		}

		bool citrusProjectChooserRecursed = false;

		void CitrusProjectChooser_SelectionChanged(object sender, System.EventArgs e)
		{
			if (!citrusProjectChooserRecursed) {
				citrusProjectChooserRecursed = true;
				try {
					if (CitrusProjectChooser.Filename != null) {
						The.Workspace.Open(CitrusProjectChooser.Filename);
					}
				} finally {
					citrusProjectChooserRecursed = false;
				}
			}
		}

		void GoButton_Clicked(object sender, System.EventArgs e)
		{
			var menuItem = The.MenuController.Items.Find(i => i.Label == ActionPicker.ActiveText);
			if (menuItem != null) {
				Execute(menuItem.Action);
			}
		}

		private bool CheckTargetAvailability()
		{
			var platform = (TargetPlatform)this.PlatformPicker.Active;
#if WIN
			if (platform == Orange.TargetPlatform.iOS) {
				var message = new Gtk.MessageDialog(NativeWindow,
					Gtk.DialogFlags.DestroyWithParent,
					Gtk.MessageType.Error, Gtk.ButtonsType.Close,
					"iOS target is not supported on Windows platform");
				message.Title = "Orange";
				message.Run();
				message.Destroy();
				return false;
			}
#endif
			return true;
		}

		protected void Window_Hidden(object sender, System.EventArgs e)
		{
			The.Workspace.Save();
			Gtk.Application.Quit();
		}
	}
}

