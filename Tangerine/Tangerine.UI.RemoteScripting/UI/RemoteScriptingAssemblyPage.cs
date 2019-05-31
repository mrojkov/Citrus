using System.IO;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Commands;

namespace Tangerine.UI.RemoteScripting
{
	internal class RemoteScriptingAssemblyPage : RemoteScriptingTabbedWidget.Page
	{
		private readonly RemoteScriptingStatusBar statusBar;
		private ThemedTextView assemblyBuilderLog;
		private ToolbarButton buildGameAndAssemblyButton;
		private ToolbarButton buildAssemblyButton;

		public RemoteScriptingAssemblyPage(RemoteScriptingStatusBar statusBar)
		{
			this.statusBar = statusBar;
		}

		public override void Initialize()
		{
			Tab = new ThemedTab { Text = "Assembly" };
			RemoteScriptingTabbedWidget.Toolbar toolbar;
			Content = new Widget {
				Layout = new VBoxLayout(),
				Nodes = {
					(toolbar = new RemoteScriptingTabbedWidget.Toolbar()),
					(assemblyBuilderLog = new RemoteScriptingTabbedWidget.TextView())
				}
			};
			toolbar.Content.Nodes.AddRange(
				buildAssemblyButton = new ToolbarButton("Build Assembly") { Clicked = () => BuildAssembly(requiredBuildGame: false) },
				buildGameAndAssemblyButton = new ToolbarButton("Build Game and Assembly") { Clicked = () => BuildAssembly() }
			);

			var preferences = ProjectPreferences.Instance;
			var arePreferencesCorrect = !string.IsNullOrEmpty(preferences.ScriptsPath) && !string.IsNullOrEmpty(preferences.ScriptsAssemblyName);
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
					SetStatusAndLog(result.Success ? "Assembly was build." : "Assembly wasn't build due to errors in the code.");
					if (result.Success) {
						Log($"Assembly length in bytes: {result.AssemblyRawBytes.Length}");
					}
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

		private void Log(string message) => assemblyBuilderLog.Append($"{message}\n");

		private void SetStatusAndLog(string message)
		{
			statusBar.Text = message;
			Log(message);
		}
	}
}
