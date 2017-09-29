using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;
using Tangerine.UI;
using Task = Lime.Task;

namespace Tangerine
{
	static class TangerineMenu
	{
		public static IMenu PadsMenu;
		private static IMenu resolution;
		private static List<ICommand> imported = new List<ICommand>();
		private static IMenu create;

		static TangerineMenu()
		{
			PadsMenu = new Menu();
		}

		public static void Create()
		{
			resolution = new Menu();
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
					SceneViewCommands.BindBones,
					GenericCommands.GroupContentsToMorphableMeshes,
					GenericCommands.ExportScene,
					GenericCommands.UpsampleAnimationTwice,
				}),
				new Command("Create", (create = new Menu())),
				new Command("View", (viewMenu = new Menu {
					GenericCommands.DefaultLayout,
					new Command("Pads", PadsMenu),
					new Command("Resolution", resolution),
					GenericCommands.Overlays,
					SceneViewCommands.DisplayBones,
				})),
				new Command("Window", new Menu {
					GenericCommands.NextDocument,
					GenericCommands.PreviousDocument
				}),
				new Command("Orange", new Menu {
					OrangeCommands.Run
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
				typeof(ImageCombiner),
				typeof(Viewport3D),
				typeof(Camera3D),
				typeof(Model3D),
				typeof(Node3D),
				typeof(WidgetAdapter3D),
				typeof(Spline3D),
				typeof(SplinePoint3D),
				typeof(SplineGear3D),
				typeof(LightSource)
			};
			foreach (var t in nodeTypes) {
				var cmd = new Command(t.Name) { Icon = NodeIconPool.GetTexture(t) };
				CommandHandlerList.Global.Connect(cmd, new CreateNode(t));
				create.Add(cmd);
			}
			viewMenu.DisplayCheckMark = true;
		}
	}
}
