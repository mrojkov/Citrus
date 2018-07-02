using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.UI;
using Tangerine.UI.SceneView;
using Task = Lime.Task;

namespace Tangerine
{
	static class TangerineMenu
	{
		public static IMenu PadsMenu;
		public static Menu overlaysMenu;
		public static Menu rulerMenu;
		private static IMenu resolution;
		private static List<ICommand> imported = new List<ICommand>();
		private static IMenu create;
		private static Menu localizationMenu;
		private static Command localizationCommand;

		static TangerineMenu()
		{
			PadsMenu = new Menu();
		}

		public static void Create()
		{
			resolution = new Menu();
			overlaysMenu = new Menu();
			rulerMenu = new Menu();
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
			foreach (var c in imported) {
				create.Remove(c);
			}
			imported.Clear();
			foreach (var t in Orange.PluginLoader.EnumerateTangerineExportedTypes()) {
				var cmd = new Command(t.Name) { Icon = NodeIconPool.GetTexture(t) };
				CommandHandlerList.Global.Connect(cmd, new CreateNode(t));
				create.Add(cmd);
				imported.Add(cmd);
			}
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
					GenericCommands.New,
					Command.MenuSeparator,
					GenericCommands.Open,
					GenericCommands.OpenProject,
					Command.MenuSeparator,
					GenericCommands.RecentDocuments,
					GenericCommands.RecentProjects,
					Command.MenuSeparator,
					GenericCommands.SaveCurrent,
					GenericCommands.SaveAs,
					GenericCommands.SaveAll,
					GenericCommands.Revert,
					GenericCommands.UpgradeDocumentFormat,
					Command.MenuSeparator,
#if !MAC
					GenericCommands.PreferencesDialog,
					Command.MenuSeparator,
#endif
					GenericCommands.CloseCurrentDocument,
					GenericCommands.CloseAllTabs,
					GenericCommands.CloseAllTabsExceptCurrent,
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
				}),
				new Command("Create", (create = new Menu())),
				new Command("View", (viewMenu = new Menu {
					GenericCommands.DefaultLayout,
					new Command("Pads", PadsMenu),
					new Command("Resolution", resolution),
					SceneViewCommands.DisplayBones,
					SceneViewCommands.DisplayPivotsForAllWidgets,
					SceneViewCommands.DisplayPivotsForInvisibleWidgets,
					Command.MenuSeparator,
					new Command("Overlays", overlaysMenu),
					new Command("Rulers", rulerMenu),
					SceneViewCommands.SnapWidgetBorderToRuler,
					SceneViewCommands.SnapWidgetPivotToRuler,
					SceneViewCommands.SnapRulerLinesToWidgets,
					SceneViewCommands.ResolutionChanger,
					SceneViewCommands.ResolutionReverceChanger,
					SceneViewCommands.ResolutionOrientation,
					(localizationCommand = new Command("Localization", localizationMenu = new Menu()) {
						Enabled = false
					})
				})),
				new Command("Window", new Menu {
					GenericCommands.NextDocument,
					GenericCommands.PreviousDocument
				}),
				new Command("Orange", new Menu {
					OrangeCommands.Run,
					OrangeCommands.OptionsDialog
				}),
			};
			var nodeTypes = new[] {
				typeof(Frame),
				typeof(Button),
				typeof(Image),
				typeof(Audio),
				typeof(Movie),
				typeof(Bone),
				typeof(ParticleEmitter),
				typeof(ParticleModifier),
				typeof(EmitterShapePoint),
				typeof(ParticlesMagnet),
				typeof(SimpleText),
				typeof(RichText),
				typeof(TextStyle),
				typeof(NineGrid),
				typeof(DistortionMesh),
				typeof(Spline),
				typeof(SplinePoint),
				typeof(SplineGear),
				typeof(Slider),
				typeof(ImageCombiner),
				typeof(Viewport3D),
				typeof(Camera3D),
				typeof(Model3D),
				typeof(Node3D),
				typeof(WidgetAdapter3D),
				typeof(Spline3D),
				typeof(SplinePoint3D),
				typeof(SplineGear3D),
				typeof(LightSource),
				typeof(Polyline),
				typeof(PolylinePoint),
			};
			foreach (var t in nodeTypes) {
				var cmd = new Command(t.Name) { Icon = NodeIconPool.GetTexture(t) };
				CommandHandlerList.Global.Connect(cmd, new CreateNode(t));
				create.Add(cmd);
			}
		}

		public static void OnProjectChanged(Project proj)
		{
			foreach (var item in overlaysMenu) {
				CommandHandlerList.Global.Disconnect(item);
			}
			overlaysMenu.Clear();
			if (proj == Project.Null)
				return;
			proj.UserPreferences.Rulers.CollectionChanged += OnRulersCollectionChanged;
			AddRulersCommands(proj.UserPreferences.DefaultRulers);
			AddRulersCommands(proj.UserPreferences.Rulers);
			RebuildRulerMenu();
			AddOverlaysCommands(proj.Overlays);
			RebuildLocalizationMenu();
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

		private static void AddOverlaysCommands(Dictionary<string, Widget> overlays)
		{
			foreach (var overlayPair in overlays) {
				Command command;
				overlayPair.Value.Components.Add(new NodeCommandComponent {
					Command = (command = new Command(overlayPair.Key))
				});
				overlaysMenu.Add(command);
				CommandHandlerList.Global.Connect(command, new OverlayToggleCommandHandler(overlayPair.Key));
			}
		}

		private static void RebuildLocalizationMenu()
		{
			foreach (var item in localizationMenu) {
				CommandHandlerList.Global.Disconnect(item);
			}
			localizationMenu.Clear();
			ProjectLocalization.Drop();
			foreach (var locale in ProjectLocalization.GetLocales()) {
				var command = new Command(locale.Code);
				var commandHandler = new ProjectLocalization(locale);
				CommandHandlerList.Global.Connect(command, commandHandler);
				localizationMenu.Add(command);
				if (ProjectLocalization.Current == null) {
					commandHandler.Execute();
				}
			}
			localizationCommand.Enabled = localizationMenu.Count > 0;
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

			public override bool GetChecked() => ProjectUserPreferences.Instance
				.DisplayedOverlays.Contains(overlayName);

			public OverlayToggleCommandHandler(string overlayName)
			{
				this.overlayName = overlayName;
			}

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
	}
}
