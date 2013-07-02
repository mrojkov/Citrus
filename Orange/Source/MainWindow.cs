using System;
using System.IO;
using Lime;
using System.Reflection;
using System.Collections.Generic;

namespace Orange
{
	enum Action
	{
		BuildGameAndRun,
		BuildContentOnly,
		RebuildGame,
		RevealContent,
		ExtractTangerineScenes,
		ExtractTranslatableStrings,
		GenerateSerializationAssembly
	}

	public partial class MainWindow : Gtk.Window
	{
		public MainWindow() :
			base(Gtk.WindowType.Toplevel)
		{
			this.CreateControls ();
			LoadState();
			TextWriter writer = new LogWriter(OutputPane);
			Console.SetOut(writer);
			Console.SetError(writer);
			GoButton.GrabFocus();
		}

		void LoadState()
		{
			var config = AppConfig.Load();
			CitrusProjectChooser.SetFilename(config.CitrusProject);
			TargetPlatformPicker.Active = config.TargetPlatform;
			ActionPicker.Active = config.Action;
		}
		
		void SaveState()
		{
			var config = AppConfig.Load();
			config.CitrusProject = CitrusProjectChooser.Filename;
			config.TargetPlatform = TargetPlatformPicker.Active;
			config.Action = ActionPicker.Active;
			AppConfig.Save(config);
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

		public bool GenerateSerializationAssembly()
		{
			BuildContent(Orange.TargetPlatform.Desktop);
			if (!BuildSolution(Orange.TargetPlatform.Desktop)) {
				return false;
			}
			var citrusProject = new CitrusProject(CitrusProjectChooser.Filename);
			var slnBuilder = new SolutionBuilder(citrusProject, Orange.TargetPlatform.Desktop);
			int exitCode = slnBuilder.Run("--GenerateSerializationAssembly");
			if (exitCode != 0) {
				Console.WriteLine("Application terminated with exit code {0}", exitCode);
				return false;
			}
			string app = slnBuilder.GetApplicationPath();
			string dir = System.IO.Path.GetDirectoryName(app);
			string assembly = System.IO.Path.Combine(dir, "Serializer.dll");
			if (!System.IO.File.Exists(assembly)) {
				Console.WriteLine("{0} doesn't exist", assembly);
				Console.WriteLine(@"Ensure your Application.cs contains following code:
	public static void Main(string[] args)
	{
		if (Array.IndexOf(args, ""--GenerateSerializationAssembly"") >= 0) {
			Lime.Environment.GenerateSerializationAssembly(""Serializer"");
			return;
		}");
				return false;
			}
			var destination = System.IO.Path.Combine(citrusProject.ProjectDirectory, "Serializer.dll");
			if (System.IO.File.Exists(destination)) {
				System.IO.File.Delete(destination);
			}
			System.IO.File.Move(assembly, destination);
			Console.Write("Serialization assembly saved to '{0}'\n", destination);
			return true;
		}

		bool CleanSolution()
		{
			var platform = (TargetPlatform)this.TargetPlatformPicker.Active;
			var citrusProject = new CitrusProject(CitrusProjectChooser.Filename);
			string assetsDirectory = citrusProject.AssetsDirectory;
			string bundlePath = System.IO.Path.ChangeExtension(assetsDirectory, Helpers.GetTargetPlatformString(platform));
			if (File.Exists(bundlePath)) {
				File.Delete(bundlePath);
			}
			var slnBuilder = new SolutionBuilder(citrusProject, platform);
			if (!slnBuilder.Clean()) {
				Console.WriteLine("CLEANUP FAILED");
				return false;
			}
			return true;
		}

		bool BuildSolution(Orange.TargetPlatform platform)
		{
			var citrusProject = new CitrusProject(CitrusProjectChooser.Filename);
			// Build game solution
			var slnBuilder = new SolutionBuilder(citrusProject, platform);
			if (!slnBuilder.Build()) {
				Console.WriteLine("BUILD FAILED");
				return false;
			}
			return true;
		}

		void BuildContent(Orange.TargetPlatform platform)
		{
			var citrusProject = new CitrusProject(CitrusProjectChooser.Filename);
			new AssetCooker(citrusProject, platform).Cook();
			// new KumquatCooker(citrusProject, platform).Cook();
		}

		bool RunSolution(Orange.TargetPlatform platform)
		{
			var citrusProject = new CitrusProject(CitrusProjectChooser.Filename);
			var slnBuilder = new SolutionBuilder(citrusProject, platform);
			int exitCode = slnBuilder.Run("");
			if (exitCode != 0) {
				Console.WriteLine("Application terminated with exit code {0}", exitCode);
				return false;
			}
			return true;
		}

		void MakeLocalizationDictionary()
		{
			var citrusProject = new CitrusProject(CitrusProjectChooser.Filename);
			DictionaryExtractor extractor = new DictionaryExtractor(citrusProject);
			extractor.ExtractDictionary();
		}

		void ClearLog()
		{
			OutputPane.Buffer.Clear();
		}

		void ScrollLogToEnd()
		{
			OutputPane.ScrollToIter(OutputPane.Buffer.EndIter, 0, false, 0, 0);
		}

		void RevealContent()
		{
			var platform = (TargetPlatform)this.TargetPlatformPicker.Active;
			var citrusProject = new CitrusProject(CitrusProjectChooser.Filename);
			AssetsUnpacker.Unpack(citrusProject, platform);
		}

		void ExtractTangerineScenes()
		{
			var platform = (TargetPlatform)this.TargetPlatformPicker.Active;
			var citrusProject = new CitrusProject(CitrusProjectChooser.Filename);
			AssetsUnpacker.UnpackTangerineScenes(citrusProject, platform);
		}

		void ShowTimeStatistics(DateTime startTime)
		{
			DateTime endTime = DateTime.Now;
			TimeSpan delta = endTime - startTime;
			Console.WriteLine("Elapsed time {0}:{1}:{2}", delta.Hours, delta.Minutes, delta.Seconds);
		}

		protected void GoButton_Clicked(object sender, System.EventArgs e)
		{
			if (!CheckTargetAvailability())
				return;
			DateTime startTime = DateTime.Now;
			SaveState();
			this.Sensitive = false;
			var platform = (TargetPlatform)this.TargetPlatformPicker.Active;
			try {
				try {
					ClearLog();
					switch ((Orange.Action)ActionPicker.Active) {
					case Orange.Action.BuildGameAndRun:
						BuildContent(platform);
						if (BuildSolution(platform)) {
							ScrollLogToEnd();
							RunSolution(platform);
						}
						break;
					case Orange.Action.BuildContentOnly:
						BuildContent(platform);
						break;
					case Orange.Action.RebuildGame:
						if (CleanSolution()) {
							BuildContent(platform);
							BuildSolution(platform);
						}
						break;
					case Orange.Action.ExtractTranslatableStrings:
						MakeLocalizationDictionary();
						break;
					case Orange.Action.RevealContent:
						RevealContent();
						break;
					case Orange.Action.ExtractTangerineScenes:
						ExtractTangerineScenes();
						break;
					case Orange.Action.GenerateSerializationAssembly:
						GenerateSerializationAssembly();
						break;
					}
				} catch (System.Exception exc) {
					Console.WriteLine(exc.Message);
				}
				ScrollLogToEnd();
			} finally {
				this.Sensitive = true;
			}
			ShowTimeStatistics(startTime);
		}

		private bool CheckTargetAvailability()
		{
			var platform = (TargetPlatform)this.TargetPlatformPicker.Active;
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

		protected void Window_Hidden(object sender, System.EventArgs e)
		{
			Gtk.Application.Quit();
		}
	}
}

