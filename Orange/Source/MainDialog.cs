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
			CitrusProjectChooser.SetFilename (config.CitrusProject);
			TargetPlatform.Active = config.TargetPlatform;
		}
		
		void SaveState ()
		{
			var config = AppConfig.Load ();
			config.CitrusProject = CitrusProjectChooser.Filename;
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

		private void Clean ()
		{
			SaveState ();
			try {
				var platform = (TargetPlatform)this.TargetPlatform.Active;
				var citrusProject = new CitrusProject (CitrusProjectChooser.Filename);
				System.DateTime startTime = System.DateTime.Now;
				CompileLog.Buffer.Clear ();
				string assetsDirectory = citrusProject.AssetsDirectory;
				string bundlePath = System.IO.Path.ChangeExtension (assetsDirectory, Helpers.GetTargetPlatformString (platform));
				if (File.Exists (bundlePath)) {
					File.Delete (bundlePath);
				}
				var slnBuilder = new SolutionBuilder (citrusProject, platform);
				if (!slnBuilder.Clean ()) {
					Console.WriteLine ("Clean failed");
					return;
				}
			} catch (System.Exception exc) {
				Console.WriteLine ("Exception: " + exc.Message);
			}
		}

		private bool CheckTargetAvailability ()
		{
			var platform = (TargetPlatform)this.TargetPlatform.Active;
#if WIN
			if (platform == Orange.TargetPlatform.iOS) {
				var message = new Gtk.MessageDialog (this, 
					Gtk.DialogFlags.DestroyWithParent, 
					Gtk.MessageType.Error, Gtk.ButtonsType.Close,
					"iOS target is not supported on Windows platform");
				message.Title = "Orange";
				message.Run ();
				message.Destroy ();
				return false;
			}
#endif
			return true;
		}

		private void BuildAll ()
		{
			SaveState ();
			try {
				var platform = (TargetPlatform)this.TargetPlatform.Active;
				var citrusProject = new CitrusProject (CitrusProjectChooser.Filename);
				System.DateTime startTime = System.DateTime.Now;
				CompileLog.Buffer.Clear ();
				// Create serialization model
				var model = ProtoBuf.Meta.TypeModel.Create ();
				model.UseImplicitZeroDefaults = false;
				RegisterEngineTypes (model);
				Serialization.Serializer = model;
				model.CompileInPlace ();
				// Cook all assets (the main job)
				AssetCooker cooker = new AssetCooker (citrusProject, platform);
				cooker.Cook ();
				// Update serialization assembly
				GenerateSerializerDll (model, citrusProject.ProjectDirectory);
				// Rebuild and run the game solution
				var slnBuilder = new SolutionBuilder (citrusProject, platform);
				if (!slnBuilder.Build ()) {
					Console.WriteLine ("Build failed");
					return;
				}
				// Show time statistics
				System.DateTime endTime = System.DateTime.Now;
				System.TimeSpan delta = endTime - startTime;
				Console.WriteLine ("Done at " + endTime.ToLongTimeString ());
				Console.WriteLine ("Building time {0}:{1}:{2}", delta.Hours, delta.Minutes, delta.Seconds);
				CompileLog.ScrollToIter (CompileLog.Buffer.EndIter, 0, false, 0, 0);
				slnBuilder.Run ();
				CompileLog.ScrollToIter (CompileLog.Buffer.EndIter, 0, false, 0, 0);
			} catch (System.Exception exc) {
				Console.WriteLine ("Exception:" + exc.Message);
			}
		}

		protected void OnRunClicked (object sender, System.EventArgs e)
		{
			this.Sensitive = false;
			try {
				if (CheckTargetAvailability ())
					BuildAll ();
			} finally {
				this.Sensitive = true;
			}
		}

		protected void OnCleanButtonClicked (object sender, System.EventArgs e)
		{
			this.Sensitive = false;
			try {
				if (CheckTargetAvailability ())
					Clean ();
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