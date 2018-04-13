using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.UI;
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
					GenericCommands.Save,
					GenericCommands.SaveAs,
					GenericCommands.Revert,
					GenericCommands.UpgradeDocumentFormat,
					Command.MenuSeparator,
#if !MAC
					GenericCommands.PreferencesDialog,
					Command.MenuSeparator,
#endif
					GenericCommands.CloseDocument,
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
					Command.MenuSeparator,
					new Command("Overlays", overlaysMenu),
					new Command("Rulers", rulerMenu),
					SceneViewCommands.SnapWidgetBorderToRuler,
					SceneViewCommands.SnapWidgetPivotToRuler,
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
			Command command;
			foreach (var item in overlaysMenu) {
				CommandHandlerList.Global.Disconnect(item);
			}
			overlaysMenu.Clear();
			foreach (var item in rulerMenu) {
				CommandHandlerList.Global.Disconnect(item);
			}
			rulerMenu.Clear();
			if (proj == Project.Null)
				return;
			foreach (var overlayPair in proj.Overlays) {
				overlayPair.Value.Components.Add(new UI.SceneView.NodeCommandComponent {
					Command = (command = new Command(overlayPair.Key))
				});
				overlaysMenu.Add(command);
				CommandHandlerList.Global.Connect(command, new OverlayToggleCommandHandler());
			}
			AddRulersCommands(proj.Rulers);
			AddRulersCommands(proj.DefaultRulers);
			RebuildRulerMenu();
			proj.Rulers.CollectionChanged += Rulers_CollectionChanged;
			RebuildLocalizationMenu();
		}

		private static void RebuildRulerMenu()
		{
			rulerMenu.Clear();
			rulerMenu.Add(SceneViewCommands.ToggleDisplayRuler);
			rulerMenu.Add(SceneViewCommands.SaveCurrentRuler);
			rulerMenu.Add(SceneViewCommands.DeleteRulers);
			rulerMenu.Add(Command.MenuSeparator);
			foreach (var ruler in Project.Current.DefaultRulers) {
				rulerMenu.Add(ruler.GetComponents().Get<UI.SceneView.CommandComponent>().Command);
			}
			if (Project.Current.Rulers.Count > 0) {
				rulerMenu.Add(Command.MenuSeparator);
			}
			foreach (var ruler in Project.Current.Rulers) {
				rulerMenu.Add(ruler.GetComponents().Get<UI.SceneView.CommandComponent>().Command);
			}
		}

		private static void Rulers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			WidgetContext.Current.Root.LateTasks.Add(RulerColectionChangedTask(e.NewItems, e.OldItems));
		}

		private static IEnumerator<object> RulerColectionChangedTask(IList newItems, IList oldItems)
		{
			AddRulersCommands(newItems);
			RemoveRulersCommands(oldItems);
			if (newItems != null) {
				foreach (RulerData ruler in newItems) {
					ruler.GetComponents().Get<UI.SceneView.CommandComponent>().Command.Issue();
				}
			}
			RebuildRulerMenu();
			yield return null;
		}

		private static void RemoveRulersCommands(IList rulers)
		{
			if (rulers == null)
				return;
			foreach (RulerData ruler in rulers) {
				CommandHandlerList.Global.Disconnect(ruler.GetComponents().Get<UI.SceneView.CommandComponent>().Command);
			}
		}

		private static void AddRulersCommands(IList rulers)
		{
			if (rulers == null)
				return;
			foreach (RulerData ruler in rulers) {
				Command c;
				ruler.GetComponents().Add(new UI.SceneView.CommandComponent {
					Command = (c = new Command(ruler.Name))
				});
				CommandHandlerList.Global.Connect(c, new OverlayToggleCommandHandler());
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

		private class OverlayToggleCommandHandler : ToggleDisplayCommandHandler
		{
			public override void RefreshCommand(ICommand command)
			{
				var checkedState = command.Checked;
				base.RefreshCommand(command);
				if (command.Checked != checkedState) {
					CommonWindow.Current.Invalidate();
				}
			}
		}
	}
}
