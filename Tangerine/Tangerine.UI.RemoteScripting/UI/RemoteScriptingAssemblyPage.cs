using System.IO;
using System.Linq;
using Lime;
using RemoteScripting;
using Tangerine.Core;
using Tangerine.Core.Commands;
using Exception = Lime.Exception;

namespace Tangerine.UI.RemoteScripting
{
	internal class RemoteScriptingAssemblyPage : RemoteScriptingWidgets.TabbedWidgetPage
	{
		private readonly RemoteScriptingStatusBar statusBar;
		private RemoteScriptingWidgets.TextView assemblyBuilderLog;
		private ToolbarButton buildGameAndAssemblyButton;
		private ToolbarButton buildAssemblyButton;

		public RemoteScriptingAssemblyPage(RemoteScriptingStatusBar statusBar)
		{
			this.statusBar = statusBar;
		}

		public override void Initialize()
		{
			Tab = new ThemedTab { Text = "Assembly" };
			RemoteScriptingWidgets.Toolbar toolbar;
			Content = new Widget {
				Layout = new VBoxLayout(),
				Nodes = {
					(toolbar = new RemoteScriptingWidgets.Toolbar()),
					(assemblyBuilderLog = new RemoteScriptingWidgets.TextView())
				}
			};
			toolbar.Content.Nodes.AddRange(
				buildAssemblyButton = new ToolbarButton("Build Assembly") { Clicked = () => BuildAssembly(requiredBuildGame: false) },
				buildGameAndAssemblyButton = new ToolbarButton("Build Game and Assembly") { Clicked = () => BuildAssembly() }
			);

			var preferences = ProjectPreferences.Instance;
			var arePreferencesCorrect =
				!string.IsNullOrEmpty(preferences.ScriptsPath) &&
				!string.IsNullOrEmpty(preferences.ScriptsAssemblyName) &&
				!string.IsNullOrEmpty(preferences.RemoteStoragePath);
			if (!arePreferencesCorrect) {
				buildAssemblyButton.Enabled = false;
				buildGameAndAssemblyButton.Enabled = false;
				SetStatusAndLog("Preferences is invalid.");
			}
		}

		private void BuildAssembly(bool requiredBuildGame = true)
		{
			async void BuildAssemblyAsync()
			{
				buildAssemblyButton.Enabled = false;
				buildGameAndAssemblyButton.Enabled = false;

				try {
					if (requiredBuildGame) {
						SetStatusAndLog("Building game...");
						await OrangeBuildCommand.ExecuteAsync();
						Log("Done.");
					}

					SetStatusAndLog("Building assembly...");
					var preferences = ProjectPreferences.Instance;
					var compiler = new CSharpCodeCompiler {
						ProjectReferences = CSharpCodeCompiler.DefaultProjectReferences.Concat(preferences.ScriptsReferences)
					};
					var csFiles = Directory.EnumerateFiles(preferences.ScriptsPath, "*.cs", SearchOption.AllDirectories);
					SetStatusAndLog($"Compile code in {preferences.ScriptsPath} to assembly {preferences.ScriptsAssemblyName}..");
					var result = await compiler.CompileAssemblyToRawBytesAsync(preferences.ScriptsAssemblyName, csFiles);
					foreach (var diagnostic in result.Diagnostics) {
						Log(diagnostic.ToString());
					}
					var success = false;
					if (result.Success) {
						Log($"Assembly length in bytes: {result.AssemblyRawBytes.Length}");
						try {
							var portableAssembly = new PortableAssembly(result.AssemblyRawBytes, preferences.EntryPointsClass);
							var compiledAssembly = new CompiledAssembly {
								RawBytes = result.AssemblyRawBytes,
								PortableAssembly = portableAssembly
							};
							CompiledAssembly.Instance = compiledAssembly;
							success = true;
						} catch (System.Reflection.ReflectionTypeLoadException exception) {
							Log(exception.ToString());
							Log("Can't load assembly due to type load exceptions:");
							foreach (var loaderException in exception.LoaderExceptions) {
								Log(loaderException.ToString());
							}
						} catch (Exception exception) {
							Log("Can't load assembly due to unknown exception:");
							Log(exception.ToString());
						}
					}
					SetStatusAndLog(success ? "Assembly was build." : "Assembly wasn't build due to errors in the code.");
					Log(string.Empty);
				} catch (System.Exception e) {
					System.Console.WriteLine(e);
				} finally {
					buildAssemblyButton.Enabled = true;
					buildGameAndAssemblyButton.Enabled = true;
				}
			}

			BuildAssemblyAsync();
		}

		public void Log(string text) => assemblyBuilderLog.AppendLine(text);

		private void SetStatusAndLog(string message)
		{
			statusBar.Text = message;
			Log(message);
		}
	}
}
