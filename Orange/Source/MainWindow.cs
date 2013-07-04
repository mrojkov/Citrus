using System;
using System.IO;
using Lime;
using System.Reflection;
using System.Collections.Generic;

namespace Orange
{
	public partial class MainWindow
	{
		public static MainWindow Instance;

		internal MainWindow()
		{
			Create();
			TextWriter writer = new LogWriter(OutputPane);
			Console.SetOut(writer);
			Console.SetError(writer);
			GoButton.GrabFocus();
			NativeWindow.Show();
			Instance = this;
		}

		public TargetPlatform ActivePlatform {
			get { return (TargetPlatform)PlatformPicker.Active; }
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

		public void ClearLog()
		{
			OutputPane.Buffer.Clear();
		}

		public void ScrollLogToEnd()
		{
			OutputPane.ScrollToIter(OutputPane.Buffer.EndIter, 0, false, 0, 0);
		}

		public void Execute(Action action)
		{
			if (!CheckTargetAvailability())
				return;
			DateTime startTime = DateTime.Now;
			The.Workspace.Save();
			NativeWindow.Sensitive = false;
			var platform = (TargetPlatform)this.PlatformPicker.Active;
			try {
				try {
					ClearLog();
					action();
				} catch (System.Exception exc) {
					Console.WriteLine(exc.Message);
				}
				ScrollLogToEnd();
			} finally {
				NativeWindow.Sensitive = true;
			}
			ShowTimeStatistics(startTime);
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
				menuItem.Action();
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

