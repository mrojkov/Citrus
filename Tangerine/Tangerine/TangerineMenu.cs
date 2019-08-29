using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;
using Orange;
using Tangerine.Core;
using Tangerine.Core.Commands;
using Tangerine.MainMenu;
using Tangerine.UI;
using Tangerine.UI.SceneView;

namespace Tangerine
{
	static class TangerineMenu
	{
		public static readonly List<ICommand> CreateNodeCommands = new List<ICommand>();
		public static IMenu PanelsMenu;
		public static Menu overlaysMenu;
		public static Menu rulerMenu;
		private static IMenu resolution;
		private static ICommand customNodes;
		private static IMenu create;
		private static Menu localizationMenu;
		private static Menu layoutMenu;
		private static Menu orangeMenu;
		private static List<ICommand> orangeCommands = new List<ICommand>();
		private static ICommand orangeMenuCommand;

		static TangerineMenu()
		{
			PanelsMenu = new Menu();
		}

		public static void Create()
		{
			resolution = new Menu();
			overlaysMenu = new Menu();
			rulerMenu = new Menu();
			orangeMenu = new Menu();
			orangeMenuCommand = new Command("Orange", orangeMenu);
			RebuildOrangeMenu(null);
			CreateMainMenu();
			CreateResolutionMenu();
		}

		private static void CreateResolutionMenu()
		{
			foreach (var orientation in DisplayResolutions.Items) {
				resolution.Add(new Command(orientation.Name, () => DisplayResolutions.SetResolution(orientation)));
			}
		}

		public static void RebuildCreateImportedTypeMenu()
		{
			var menus = new[] { customNodes.Menu, GenericCommands.NewTanWithCustomRoot.Menu, create };
			foreach (var menu in menus) {
				foreach (var command in menu) {
					CommandHandlerList.Global.Disconnect(command);
				}
			}
			CreateNodeCommands.Clear();
			customNodes.Menu.Clear();
			GenericCommands.NewTanWithCustomRoot.Menu.Clear();
			create.Clear();
			create.Add(customNodes = new Command("Custom Nodes", new Menu()));

			foreach (var type in Project.Current.RegisteredNodeTypes) {
				var cmd = new Command("Create " + type.Name) { Icon = NodeIconPool.GetIcon(type) };
				CommandRegistry.Register(cmd, "CreateCommands", "Create" + type.Name, @override: true);
				CommandHandlerList.Global.Connect(cmd, new CreateNode(type, cmd));
				if (type.Namespace == "Lime") {
					create.Add(cmd);
					CreateNodeCommands.Add(cmd);
				} else {
					customNodes.Menu.Add(cmd);
				}
				if (IsNodeTypeCanBeRoot(type)) {
					var newFileCmd = new Command(type.Name);
					var format = typeof(Node3D).IsAssignableFrom(type) ? DocumentFormat.T3D : DocumentFormat.Tan;
					CommandHandlerList.Global.Connect(newFileCmd, new FileNew(format, type));
					GenericCommands.NewTanWithCustomRoot.Menu.Add(newFileCmd);
				}
			}
			customNodes.Enabled = customNodes.Menu.Count > 0;
			GenericCommands.NewTanWithCustomRoot.Enabled = GenericCommands.NewTanWithCustomRoot.Menu.Count > 0;
			TangerineApp.Instance?.RefreshCreateNodeCommands();
		}

		private static bool IsNodeTypeCanBeRoot(Type type)
		{
			return type.GetCustomAttributes(false).OfType<TangerineRegisterNodeAttribute>().First().CanBeRoot;
		}

		private static void CreateMainMenu()
		{
			Menu viewMenu;
			Application.MainMenu = new Menu {
#if MAC
				new Command("Application", new Menu {
					GenericCommands.PreferencesDialog,
					Command.MenuSeparator,
					GenericCommands.Quit,
				}),
#endif
				new Command("File", new Menu {
					new Command("New", new Menu {
						GenericCommands.NewScene,
						GenericCommands.NewTan,
						GenericCommands.NewTanWithCustomRoot,
					}),
					GenericCommands.NewProject,
					Command.MenuSeparator,
					GenericCommands.Open,
					GenericCommands.OpenProject,
					GenericCommands.CloseProject,
					Command.MenuSeparator,
					GenericCommands.RecentDocuments,
					GenericCommands.RecentProjects,
					Command.MenuSeparator,
					GenericCommands.Save,
					GenericCommands.SaveAs,
					GenericCommands.SaveAll,
					GenericCommands.Revert,
					GenericCommands.UpgradeDocumentFormat,
					Command.MenuSeparator,
#if !MAC
					GenericCommands.PreferencesDialog,
					Command.MenuSeparator,
#endif
					GenericCommands.Close,
					GenericCommands.CloseAll,
					GenericCommands.CloseAllButCurrent,
#if !MAC
					GenericCommands.Quit,
#endif
				}),
				new Command("Edit", new Menu {
					Command.Undo,
					Command.Redo,
					Command.MenuSeparator,
					Command.Cut,
					Command.Copy,
					Command.Paste,
					Command.Delete,
					SceneViewCommands.Duplicate,
					TimelineCommands.DeleteKeyframes,
					TimelineCommands.CreateMarkerPlay,
					TimelineCommands.CreateMarkerStop,
					TimelineCommands.CreateMarkerJump,
					TimelineCommands.DeleteMarker,
					Command.MenuSeparator,
					Command.SelectAll,
					Command.MenuSeparator,
					GenericCommands.Group,
					GenericCommands.Ungroup,
					GenericCommands.InsertTimelineColumn,
					GenericCommands.RemoveTimelineColumn,
					Command.MenuSeparator,
					SceneViewCommands.TieWidgetsWithBones,
					SceneViewCommands.UntieWidgetsFromBones,
					GenericCommands.GroupContentsToMorphableMeshes,
					GenericCommands.ExportScene,
					GenericCommands.UpsampleAnimationTwice,
					GenericCommands.ConvertToButton,
					SceneViewCommands.GeneratePreview
				}),
				new Command("Create", (create = new Menu())),
				new Command("Tools", new Menu {
					ToolsCommands.RenderToPngSequence,
				}),
				new Command("View", (viewMenu = new Menu {
					new Command("Layouts", (layoutMenu = new Menu {
						GenericCommands.SaveLayout,
						GenericCommands.LoadLayout,
						GenericCommands.DefaultLayout,
					})),
					new Command("Panels", PanelsMenu),
					new Command("Resolution", resolution),
					Command.MenuSeparator,
					new Command("Overlays", overlaysMenu),
					new Command("Rulers", rulerMenu),
					SceneViewCommands.SnapWidgetBorderToRuler,
					SceneViewCommands.SnapWidgetPivotToRuler,
					SceneViewCommands.SnapRulerLinesToWidgets,
					SceneViewCommands.ResolutionChanger,
					SceneViewCommands.ResolutionReverceChanger,
					SceneViewCommands.ResolutionOrientation,
					TimelineCommands.CenterTimelineOnCurrentColumn,
					new Command("Locale", localizationMenu = new Menu()),
				})),
				new Command("Window", new Menu {
					GenericCommands.NextDocument,
					GenericCommands.PreviousDocument
				}),
				orangeMenuCommand,
				new Command("Git", new Menu {
					GitCommands.ForceUpdate,
					GitCommands.Update,
				}),
				new Command("Help", new Menu {
					GenericCommands.ViewHelp,
					GenericCommands.HelpMode,
					GenericCommands.ViewChangelog
				}),
				new Command("System", new Menu {
					GenericCommands.ResetGlobalSettings,
					GenericCommands.ClearCache,
					GenericCommands.PurgeBackups,
				}),
			};
			create.Add(customNodes = new Command("Custom Nodes", new Menu()));
			foreach (var t in Project.GetNodesTypesOrdered("Lime")) {
				var cmd = new Command(t.Name) { Icon = NodeIconPool.GetIcon(t) };
				CommandHandlerList.Global.Connect(cmd, new CreateNode(t, cmd));
				create.Add(cmd);
				CreateNodeCommands.Add(cmd);
			}
			Command.Undo.Icon = IconPool.GetIcon("Tools.Undo");
			Command.Redo.Icon = IconPool.GetIcon("Tools.Redo");
			GenericCommands.Revert.Icon = IconPool.GetIcon("Tools.Revert");
		}

		private static void RebuildOrangeMenu(string citprojPath)
		{
			orangeMenu.Clear();
			foreach (var command in orangeCommands) {
				CommandHandlerList.Global.Disconnect(command);
			}
			orangeCommands.Clear();
			OrangeBuildCommand.Instance = null;
			orangeMenuCommand.Enabled = citprojPath != null;
			if (!orangeMenuCommand.Enabled) {
				return;
			}

			void AddOrangeCommand(ICommand command, CommandHandler handler)
			{
				orangeMenu.Add(command);
				CommandHandlerList.Global.Connect(command, handler);
			}
			void OnOrangeCommandExecuting()
			{
				UI.Console.Instance.Show();
			}
			var orangeMenuItems = Orange.MenuController.Instance.GetVisibleAndSortedItems();
			const string BuildAndRunLabel = "Build and Run";
			const string BuildLabel = "Build";
			const string CookGameAssetsLabel = "Cook Game Assets";
			var blacklist = new HashSet<string> { "Run Tangerine", BuildAndRunLabel, BuildLabel, CookGameAssetsLabel };
			var buildAndRun = orangeMenuItems.First(item => item.Label == BuildAndRunLabel);
			var build = orangeMenuItems.First(item => item.Label == BuildLabel);
			var cookGameAssets = orangeMenuItems.First(item => item.Label == CookGameAssetsLabel);
			AddOrangeCommand(OrangeCommands.Run, new OrangeCommand(() => buildAndRun.Action()) { Executing = OnOrangeCommandExecuting });
			AddOrangeCommand(OrangeCommands.Build, OrangeBuildCommand.Instance = new OrangeBuildCommand(build.Action) { Executing = OnOrangeCommandExecuting });
			AddOrangeCommand(OrangeCommands.RunConfig, new OrangePluginOptionsCommand());
			AddOrangeCommand(OrangeCommands.CookGameAssets, new OrangeCommand(() => cookGameAssets.Action()) { Executing = OnOrangeCommandExecuting });
			foreach (var menuItem in orangeMenuItems) {
				if (blacklist.Contains(menuItem.Label)) {
					continue;
				}
				AddOrangeCommand(new Command(menuItem.Label), new OrangeCommand(() => menuItem.Action()) { Executing = OnOrangeCommandExecuting });
			}

			// TODO Duplicates code from Orange.GUI.OrangeInterface.cs. Both should be presented at one file
			var orangeInterfaceInstance = (OrangeInterface) Orange.UserInterface.Instance;
			var updateAction = new Action<AssetCacheMode>(mode => orangeInterfaceInstance.UpdateCacheModeCheckboxes(mode));
			orangeInterfaceInstance.CacheLocalAndRemote = new Command("Local &and remote", () => updateAction(Orange.AssetCacheMode.Local | Orange.AssetCacheMode.Remote));
			orangeInterfaceInstance.CacheRemote = new Command("&Remote", () => updateAction(Orange.AssetCacheMode.Remote));
			orangeInterfaceInstance.CacheLocal = new Command("&Local", () => updateAction(Orange.AssetCacheMode.Local));
			orangeInterfaceInstance.CacheNone = new Command("&None", () => updateAction(Orange.AssetCacheMode.None));

			var uploadCacheToServerCommand = new Command("&Upload cache to server");
			CommandHandlerList.Global.Connect(uploadCacheToServerCommand, new OrangeCommand(UploadCacheToServer.UploadCacheToServerAction) { Executing = OnOrangeCommandExecuting });

			orangeMenu.Add(new Command("Cache", new Menu {
				new Command("&Actions", new Menu {
					uploadCacheToServerCommand
				}),
				new Command("Mode", new Menu{
					orangeInterfaceInstance.CacheLocalAndRemote,
					orangeInterfaceInstance.CacheRemote,
					orangeInterfaceInstance.CacheLocal,
					orangeInterfaceInstance.CacheNone,
				})
			}));
			Orange.The.UI.LoadFromWorkspaceConfig(Orange.WorkspaceConfig.Load());
			Orange.The.Workspace.LoadCacheSettings();
		}

		public static void OnProjectChanged(Project proj)
		{
			foreach (var item in overlaysMenu) {
				CommandHandlerList.Global.Disconnect(item);
			}
			overlaysMenu.Clear();
			RebuildOrangeMenu(proj.CitprojPath);
			LocalizationMenuFactory.Rebuild(localizationMenu);
			if (proj == Project.Null)
				return;
			proj.UserPreferences.Rulers.CollectionChanged += OnRulersCollectionChanged;
			AddRulersCommands(proj.UserPreferences.DefaultRulers);
			AddRulersCommands(proj.UserPreferences.Rulers);
			RebuildRulerMenu();
			AddOverlaysCommands(proj.Overlays);
		}

		private static void RebuildRulerMenu()
		{
			rulerMenu.Clear();
			rulerMenu.Add(SceneViewCommands.ToggleDisplayRuler);
			rulerMenu.Add(SceneViewCommands.ClearActiveRuler);
			rulerMenu.Add(SceneViewCommands.SaveCurrentRuler);
			rulerMenu.Add(SceneViewCommands.ManageRulers);
			rulerMenu.Add(Command.MenuSeparator);
			foreach (var ruler in ProjectUserPreferences.Instance.DefaultRulers) {
				rulerMenu.Add(ruler.Components.Get<CommandComponent>().Command);
			}
			if (ProjectUserPreferences.Instance.Rulers.Count > 0) {
				rulerMenu.Add(Command.MenuSeparator);
			}
			foreach (var ruler in ProjectUserPreferences.Instance.Rulers) {
				rulerMenu.Add(ruler.Components.Get<CommandComponent>().Command);
			}
		}

		public static void OnRulersCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			// Invoke handler at the next update to avoid collection changed exceptions while
			// command handler iterates the commands list.
			UpdateHandler handler = null;
			handler = delta => {
				AddRulersCommands(e.NewItems, true);
				RemoveRulersCommands(e.OldItems);
				RebuildRulerMenu();
				UI.Docking.DockManager.Instance.MainWindowWidget.Updated -= handler;
			};
			UI.Docking.DockManager.Instance.MainWindowWidget.Updated += handler;
		}

		public static void AddRulersCommands(IEnumerable rulers, bool issueCommands = false)
		{
			if (rulers == null)
				return;
			foreach (Ruler ruler in rulers) {
				Command c;
				ruler.Components.Add(new CommandComponent {
					Command = (c = new Command(ruler.Name))
				});
				CommandHandlerList.Global.Connect(c, new RulerToggleCommandHandler(ruler.Name));
				if (issueCommands) {
					c.Issue();
				}
			}
		}

		public static void RemoveRulersCommands(IEnumerable rulers)
		{
			if (rulers == null)
				return;
			foreach (Ruler ruler in rulers) {
				CommandHandlerList.Global.Disconnect(ruler.Components.Get<CommandComponent>().Command);
				ProjectUserPreferences.Instance.DisplayedRulers.Remove(ruler.Name);
				ruler.Components.Clear();
			}
		}

		private static void AddOverlaysCommands(SortedDictionary<string, Widget> overlays)
		{
			var commands = new List<ICommand>(overlays.Count) {
				GenericCommands.Overlay1, GenericCommands.Overlay2, GenericCommands.Overlay3,
				GenericCommands.Overlay4, GenericCommands.Overlay5, GenericCommands.Overlay6,
				GenericCommands.Overlay7, GenericCommands.Overlay8, GenericCommands.Overlay9
			};
			int lastIndex = 0;
			foreach (var overlayPair in overlays) {
				var command = lastIndex < 9 ? commands[lastIndex] : new Command(overlayPair.Key);
				command.Text = overlayPair.Key;
				overlayPair.Value.Components.Add(new NodeCommandComponent { Command = command });
				overlaysMenu.Add(command);
				var commandHandler = new OverlayToggleCommandHandler(overlayPair.Key);
				CommandHandlerList.Global.Connect(command, commandHandler);
				lastIndex++;
			}
		}

		public static void RebuildRecentDocumentsMenu()
		{
			var recentDocuments = ProjectUserPreferences.Instance.RecentDocuments;
			var menu = new Menu();
			int counter = 1;
			foreach (var i in recentDocuments) {
				string name = System.String.Format("{0}. {1}", counter++, i);
				menu.Add(new Command(name, () => Project.Current.OpenDocument(i) ));
			}
			GenericCommands.RecentDocuments.Menu = menu;
			GenericCommands.RecentDocuments.Enabled = recentDocuments.Count > 0;
		}

		public static void RebuildRecentProjectsMenu()
		{
			var recentProjects = AppUserPreferences.Instance.RecentProjects;
			var menu = new Menu();
			int counter = 1;
			foreach (var i in recentProjects) {
				string name = System.String.Format("{0}. {1} ({2})", counter++, System.IO.Path.GetFileName(i),
					System.IO.Path.GetDirectoryName(i));
				menu.Add(new Command(name, () =>  {
					if (Project.Current.Close()) {
						new Project(i).Open();
						FileOpenProject.AddRecentProject(i);
					}
				}));
			}
			GenericCommands.RecentProjects.Menu = menu;
			GenericCommands.RecentProjects.Enabled = recentProjects.Count > 0;
		}

		private class OverlayToggleCommandHandler : DocumentCommandHandler
		{
			private readonly string overlayName;

			public override bool GetChecked() => ProjectUserPreferences.Instance.DisplayedOverlays.Contains(overlayName);

			public OverlayToggleCommandHandler(string overlayName) => this.overlayName = overlayName;

			public override void ExecuteTransaction()
			{
				var prefs = ProjectUserPreferences.Instance;
				if (prefs.DisplayedOverlays.Contains(overlayName)) {
					prefs.DisplayedOverlays.Remove(overlayName);
				} else {
					prefs.DisplayedOverlays.Add(overlayName);
				}
				Application.InvalidateWindows();
			}
		}

		private class RulerToggleCommandHandler : DocumentCommandHandler
		{
			private readonly string rulerName;

			public override bool GetChecked() => ProjectUserPreferences.Instance
				.DisplayedRulers.Contains(rulerName);

			public RulerToggleCommandHandler(string rulerName)
			{
				this.rulerName = rulerName;
			}

			public override void ExecuteTransaction()
			{
				var prefs = ProjectUserPreferences.Instance;
				if (prefs.DisplayedRulers.Contains(rulerName)) {
					prefs.DisplayedRulers.Remove(rulerName);
				} else {
					prefs.DisplayedRulers.Add(rulerName);
				}
				Application.InvalidateWindows();
			}
		}

		private static IEnumerator<object> OrangeTask(Action action)
		{
			Tangerine.UI.Console.Instance.Show();
			Orange.The.Workspace?.AssetFiles?.Rescan();
			yield return Task.ExecuteAsync(() => {
				try {
					action();
				} catch (System.Exception e) {
					System.Console.WriteLine(e);
				}
			});
			System.Console.WriteLine("Done");
		}
	}
}
