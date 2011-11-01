using System;
using System.IO;
using Lime;
using System.Reflection;
using System.Collections.Generic;

namespace Orange
{
	public partial class MainDialog : Gtk.Dialog
	{
		public MainDialog ()
		{
			Build ();
			LoadState ();
			TextWriter writer = new LogWriter (CompileLog);
			Console.SetOut (writer);
			Console.SetError (writer);
			RunButton.GrabFocus ();
		}
		
		void LoadState ()
		{
			var config = AppConfig.Load ();
			ProjectFolderChooser.SetCurrentFolder (config.ProjectFolder);
			TargetPlatform.Active = config.TargetPlatform;
		}
		
		void SaveState ()
		{
			var config = AppConfig.Load ();
			config.ProjectFolder = ProjectFolderChooser.CurrentFolder;
			config.TargetPlatform = TargetPlatform.Active;
			AppConfig.Save (config);
		}
		
		class LogWriter : TextWriter
		{
			Gtk.TextView textView;
			int bufferedLines = 0;
			
			public LogWriter (Gtk.TextView textView)
			{
				this.textView = textView;
			}
			
			public override void WriteLine (string value)
			{
				Write (value + '\n');
			}
			
			public override void Write (string value)
			{
				#pragma warning disable 618
				textView.Buffer.Insert (textView.Buffer.EndIter, value);
				while (Gtk.Application.EventsPending ())
					Gtk.Application.RunIteration ();
#if !WIN
				if (bufferedLines > 4) {
					bufferedLines = 0;
#endif
					textView.ScrollToIter (textView.Buffer.EndIter, 0, false, 0, 0);
#if !WIN
				}
				bufferedLines++;
#endif
			}

			public override System.Text.Encoding Encoding {
				get {
					throw new NotImplementedException ();
				}
			}
		}
		
		public static void GenerateSerializerDll (ProtoBuf.Meta.RuntimeTypeModel model, string directory)
		{
			string currentDirectory = System.IO.Directory.GetCurrentDirectory ();
			try {
				System.IO.Directory.SetCurrentDirectory (directory);
				model.Compile ("Lime.Serializer", "Lime.Serializer.dll");
			} finally {
				System.IO.Directory.SetCurrentDirectory (currentDirectory);
			}
		}
		
		private void RegisterEngineTypes (ProtoBuf.Meta.RuntimeTypeModel model)
		{
			model.Add (typeof(Node), true);
			model.Add (typeof(TextureAtlasPart), true);
			model.Add (typeof(Font), true);
		}

		private void RunBuild (bool rebuild)
		{
			var platform = (TargetPlatform)this.TargetPlatform.Active;
			var projectFolder = ProjectFolderChooser.CurrentFolder;
			SaveState ();
			try {
				System.DateTime startTime = System.DateTime.Now;
				CompileLog.Buffer.Clear ();
				// Create serialization model
				var model = ProtoBuf.Meta.TypeModel.Create ();
				RegisterEngineTypes (model);
				Serialization.Serializer = model;
				model.CompileInPlace ();
				// Cook all assets (the main job)
				//AssetCooker cooker = new AssetCooker (projectFolder, platform);
				//cooker.Cook (rebuild);
				// Update serialization assembly
				GenerateSerializerDll (model, ProjectFolderChooser.CurrentFolder);
				// Rebuild and run the game solution
				var slnBuilder = new SolutionBuilder (projectFolder, platform);
				slnBuilder.Build ();
				// Show time statistics
				System.DateTime endTime = System.DateTime.Now;
				System.TimeSpan delta = endTime - startTime;
				Console.WriteLine ("Done at " + endTime.ToLongTimeString ());
				Console.WriteLine ("Building time {0}:{1}:{2}", delta.Hours, delta.Minutes, delta.Seconds);
				CompileLog.ScrollToIter (CompileLog.Buffer.EndIter, 0, false, 0, 0);
				slnBuilder.Run ();
				CompileLog.ScrollToIter (CompileLog.Buffer.EndIter, 0, false, 0, 0);
			} catch (System.Exception exc) {
				Console.WriteLine ("Exception: " + exc.Message);
			}
		}
		
		protected void OnRunClicked (object sender, System.EventArgs e)
		{
			this.Sensitive = false;
			try {
				RunBuild (false);
			} finally {
				this.Sensitive = true;
			}
		}

		protected void OnCleanButtonClicked (object sender, System.EventArgs e)
		{
			this.Sensitive = false;
			try {
				RunBuild (true);
			} finally {
				this.Sensitive = true;
			}
		}

		protected void OnRunButtonClicked (object sender, System.EventArgs e)
		{
			throw new System.NotImplementedException ();
		}
	}
}