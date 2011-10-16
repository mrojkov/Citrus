using System;
using System.IO;
using Lime;
using Lemon;
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
			BuildButton.GrabFocus ();
		}
		
		void LoadState ()
		{
			var config = AppConfig.Load ();
			AssetsFolderChooser.SetCurrentFolder (config.AssetsFolder);
			TargetPlatform.Active = config.TargetPlatform;
			GameAssemblyChooser.SetFilename (config.GameAssembly);
		}
		
		void SaveState ()
		{
			var config = AppConfig.Load ();
			config.AssetsFolder = AssetsFolderChooser.CurrentFolder;
			config.TargetPlatform = TargetPlatform.Active;
			config.GameAssembly = GameAssemblyChooser.Filename;
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
			
			public override void Write (string buffer)
			{
				textView.Buffer.Insert (textView.Buffer.EndIter, buffer + "\n");
				while (Gtk.Application.EventsPending ())
        				Gtk.Application.RunIteration ();
				if (bufferedLines > 4) {
					bufferedLines = 0;
					textView.ScrollToIter (textView.Buffer.EndIter, 0, false, 0, 0);
				}
				bufferedLines++;
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
				model.Compile ("Serializer", "Serializer.dll");
			} finally {
				System.IO.Directory.SetCurrentDirectory (currentDirectory);
			}
		}		
		
		private void UpdateTextureAtlases ()
		{
			var directories = Helpers.GetAllDirectories (".", "*.*");
			foreach (string dir in directories) {
 				var name = System.IO.Path.GetFileName (dir);
				if (name == "Atlases") {
					AtlasGenerator.Process (dir);
				}					
			}			
		}
		
		private void UpdateTextureAtlases (string directory)
		{
			string currentDirectory = System.IO.Directory.GetCurrentDirectory ();
			try {
				System.IO.Directory.SetCurrentDirectory (directory);
				UpdateTextureAtlases ();				
			} finally {
				System.IO.Directory.SetCurrentDirectory (currentDirectory);
			}
		}
		
		private void RegisterEngineTypes (ProtoBuf.Meta.RuntimeTypeModel model)
		{
			model.Add (typeof(Node), true);
			model.Add (typeof(TextureParams), true);
			model.Add (typeof(Font), true);
		}
		
		private void PrepareTypeModel (ProtoBuf.Meta.RuntimeTypeModel model)
		{
			RegisterEngineTypes (model);
			if (File.Exists (GameAssemblyChooser.Filename)) {
				Assembly gameAssembly = Assembly.LoadFile (GameAssemblyChooser.Filename);
				Type gameWidgets = gameAssembly.GetType ("SerializationSupport");
				if (gameWidgets == null) {
					throw new Lime.Exception ("Class 'SerializationSupport' not found in assembly {0}", gameAssembly);
				}					
				MethodInfo mi = gameWidgets.GetMethod ("SetupTypes");
				mi.Invoke (null, new object [] {model});
			}
			model.CompileInPlace ();
		}
		
		private void RunBuild (bool rebuild)
		{
			SaveState ();
			try	{
				System.DateTime startTime = System.DateTime.Now;
				CompileLog.Buffer.Clear ();
				// Create serialization model
				var model = ProtoBuf.Meta.TypeModel.Create ();
				Serialization.Serializer = model;
				// Populate model with ingame and engine types
				PrepareTypeModel (model);
				// Update texture atlases
				UpdateTextureAtlases (AssetsFolderChooser.CurrentFolder);				
				// Main assets processing
				AssetsImporter.ProcessDirectory (AssetsFolderChooser.CurrentFolder, rebuild);
				System.DateTime endTime = System.DateTime.Now;
				System.TimeSpan delta = endTime - startTime;
				// Update serialization assembly	
				GenerateSerializerDll (model, System.IO.Path.Combine (AssetsFolderChooser.CurrentFolder, ".."));
				// Show time statistics
				Console.WriteLine ("Done at " + endTime.ToLongTimeString());
				Console.WriteLine ("Building time {0}:{1}:{2}", delta.Hours, delta.Minutes, delta.Seconds);
				CompileLog.ScrollToIter (CompileLog.Buffer.EndIter, 0, false, 0, 0);				
			}
			catch (System.Exception exc) {
				Console.WriteLine("Exception: " + exc.Message);
			}				
		}
		
		protected void OnBuildClicked (object sender, System.EventArgs e)
		{
			RebuildButton.Sensitive = false;
			BuildButton.Sensitive = false;
			try {
				RunBuild (false);
			} finally {
				RebuildButton.Sensitive = true;
				BuildButton.Sensitive = true;
			}
		}

		protected void OnRebuildButtonClicked (object sender, System.EventArgs e)
		{
			RebuildButton.Sensitive = false;
			BuildButton.Sensitive = false;
			try {	
				RunBuild (true);
			} finally {
				RebuildButton.Sensitive = true;
				BuildButton.Sensitive = true;
			}
		}
	}
}

