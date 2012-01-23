using System;
using System.IO;
using Lime;
using System.Reflection;
using System.Collections.Generic;

namespace Orange
{
	enum Action
	{
		BuildAndRun,
		Build,
		Rebuild,
		UpdateLocalizationTags
	}

	public partial class MainWindow : Gtk.Window
	{
		public MainWindow () :
				base(Gtk.WindowType.Toplevel)
		{
			this.Build ();
			LoadState();
			TextWriter writer = new LogWriter(CompileLog);
			Console.SetOut(writer);
			Console.SetError(writer);
			GoButton.GrabFocus();
		}

		void LoadState()
		{
			var config = AppConfig.Load();
			CitrusProjectChooser.SetFilename(config.CitrusProject);
			TargetPlatform.Active = config.TargetPlatform;
		}
		
		void SaveState()
		{
			var config = AppConfig.Load();
			config.CitrusProject = CitrusProjectChooser.Filename;
			config.TargetPlatform = TargetPlatform.Active;
			AppConfig.Save(config);
		}
		
		class LogWriter : TextWriter
		{
			Gtk.TextView textView;
			int bufferedLines = 0;
			
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
		
		public static void GenerateSerializerDll(ProtoBuf.Meta.RuntimeTypeModel model, string directory)
		{
			string currentDirectory = System.IO.Directory.GetCurrentDirectory();
			try {
				System.IO.Directory.SetCurrentDirectory(directory);
				model.Compile("Serializer", "Serializer.dll");
			} finally {
				System.IO.Directory.SetCurrentDirectory(currentDirectory);
			}
		}
		
		private void RegisterEngineTypes(ProtoBuf.Meta.RuntimeTypeModel model)
		{
			model.Add(typeof(Node), true);
			model.Add(typeof(TextureAtlasPart), true);
			model.Add(typeof(Font), true);
		}

		private bool CleanSolution()
		{
			var platform = (TargetPlatform)this.TargetPlatform.Active;
			var citrusProject = new CitrusProject(CitrusProjectChooser.Filename);
			string assetsDirectory = citrusProject.AssetsDirectory;
			string bundlePath = System.IO.Path.ChangeExtension(assetsDirectory, Helpers.GetTargetPlatformString(platform));
			if (File.Exists(bundlePath)) {
				File.Delete(bundlePath);
			}
			var slnBuilder = new SolutionBuilder(citrusProject, platform);
			if (!slnBuilder.Clean()) {
				Console.WriteLine("Clean failed");
				return false;
			}
			return true;
		}

		private bool BuildSolution()
		{
			DateTime startTime = DateTime.Now;
			var platform = (TargetPlatform)this.TargetPlatform.Active;
			var citrusProject = new CitrusProject(CitrusProjectChooser.Filename);
			// Create serialization model
			var model = ProtoBuf.Meta.TypeModel.Create();
			model.UseImplicitZeroDefaults = false;
			RegisterEngineTypes(model);
			Serialization.Serializer = model;
			model.CompileInPlace();
			// Cook all assets(the main job)
			AssetCooker cooker = new AssetCooker(citrusProject, platform);
			cooker.Cook();
			// Update serialization assembly
			GenerateSerializerDll(model, citrusProject.ProjectDirectory);
			// Build game solution
			var slnBuilder = new SolutionBuilder(citrusProject, platform);
			if (!slnBuilder.Build()) {
				Console.WriteLine("Build failed");
				ShowTimeStatistics(startTime);
				return false;
			}
			ShowTimeStatistics(startTime);
			return true;
		}

		private bool RunSolution()
		{
			var platform = (TargetPlatform)this.TargetPlatform.Active;
			var citrusProject = new CitrusProject(CitrusProjectChooser.Filename);
			var slnBuilder = new SolutionBuilder(citrusProject, platform);
			slnBuilder.Run();
			return true;
		}

		private void UpdateLocalizationDictionary()
		{
			var citrusProject = new CitrusProject(CitrusProjectChooser.Filename);
			DictionaryExtractor extractor = new DictionaryExtractor(citrusProject);
			extractor.ExtractDictionary();
		}

		private void ClearLog()
		{
			CompileLog.Buffer.Clear();
		}

		private void ScrollLogToEnd()
		{
			CompileLog.ScrollToIter(CompileLog.Buffer.EndIter, 0, false, 0, 0);
		}

		void ShowTimeStatistics(DateTime startTime)
		{
			DateTime endTime = DateTime.Now;
			TimeSpan delta = endTime - startTime;
			Console.WriteLine("Done at " + endTime.ToLongTimeString());
			Console.WriteLine("Elapsed time {0}:{1}:{2}", delta.Hours, delta.Minutes, delta.Seconds);
		}

		protected void OnGoButtonClicked(object sender, System.EventArgs e)
		{
			if (!CheckTargetAvailability())
				return;
			SaveState();
			this.Sensitive = false;
			try {
				try {
					ClearLog();
					switch ((Orange.Action)Action.Active) {
					case Orange.Action.BuildAndRun:
						// UpdateLocalizationDictionary();
						if (BuildSolution()) {
							ScrollLogToEnd();
							RunSolution();
						}
						break;
					case Orange.Action.Build:
						BuildSolution();
						break;
					case Orange.Action.Rebuild:
						if (CleanSolution()) {
							BuildSolution();
						}
						break;
					case Orange.Action.UpdateLocalizationTags:
						UpdateLocalizationDictionary();
						break;
					}
				} catch (System.Exception exc) {
					Console.WriteLine("Exception:" + exc.Message);
				}
				ScrollLogToEnd();
			} finally {
				this.Sensitive = true;
			}
		}

		private bool CheckTargetAvailability()
		{
			var platform = (TargetPlatform)this.TargetPlatform.Active;
#if WIN
			if (platform == Orange.TargetPlatform.iOS) {
				var message = new Gtk.MessageDialog(this, 
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

		protected void OnHidden(object sender, System.EventArgs e)
		{
			Gtk.Application.Quit();
		}
	}
}

