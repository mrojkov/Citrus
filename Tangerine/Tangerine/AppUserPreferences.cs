using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.UI;
using Yuzu;

namespace Tangerine
{
	public class AppUserPreferences : Component
	{
		[YuzuOptional]
		public UI.Docking.DockManager.State DockState;

		[YuzuOptional]
		public readonly List<string> RecentProjects;

		public static int RecentProjectsCount { get; private set; } = 5;

		[YuzuOptional]
		public ColorTheme.ColorThemeKind ColorThemeKind { get; set; } = ColorTheme.ColorThemeKind.Light;

		[YuzuOptional]
		public UI.ColorTheme ColorTheme { get; set; }

		[YuzuOptional]
		public Theme.ColorTheme LimeColorTheme { get; set; }

		[YuzuOptional]
		public Vector2 DefaultSceneDimensions { get; set; }

		[YuzuOptional]
		public string CurrentHotkeyProfile { get; set; }

		/// <summary>
		/// Autosave delay in seconds
		/// </summary>
		[YuzuOptional]
		public int AutosaveDelay { get; set; }

		[YuzuOptional]
		public ToolbarModel ToolbarModel { get; set; } = DefaultToolbarModel();

		public AppUserPreferences()
		{
			DockState = new UI.Docking.DockManager.State();
			RecentProjects = new List<string>();
			ResetToDefaults();
		}

		internal void ResetToDefaults()
		{
			ColorThemeKind = ColorTheme.ColorThemeKind.Light;
			ColorTheme = UI.ColorTheme.CreateLightTheme();
			LimeColorTheme = Theme.ColorTheme.CreateLightTheme();
			DefaultSceneDimensions = new Vector2(1024, 768);
			AutosaveDelay = 600;
		}

		public static AppUserPreferences Instance => Core.UserPreferences.Instance.Get<AppUserPreferences>();

		public static ToolbarModel DefaultToolbarModel()
		{
			IEnumerable<string> GetCommandIds(Type type)
			{
				foreach (var field in type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)) {
					var fieldType = field.FieldType;
					if (fieldType == typeof(ICommand) || fieldType.IsSubclassOf(typeof(ICommand))) {
						yield return field.Name;
					}
				}
			}

			var createToolbarPanel = new ToolbarModel.ToolbarPanel { Title = "Create" };
			createToolbarPanel.CommandIds.AddRange(new[] {
				"Frame",
				"Button",
				"Image",
				"Audio",
				"Movie",
				"Bone",
				"ParticleEmitter",
				"ParticleModifier",
				"EmitterShapePoint",
				"ParticlesMagnet",
				"SimpleText",
				"RichText",
				"TextStyle",
				"NineGrid",
				"DistortionMesh",
				"Spline",
				"SplinePoint",
				"SplineGear",
				"Slider",
				"ImageCombiner",
				"Viewport3D",
				"Camera3D",
				"Model3D",
				"Node3D",
				"WidgetAdapter3D",
				"Spline3D",
				"SplinePoint3D",
				"SplineGear3D",
				"LightSource",
				"Polyline",
				"PolylinePoint",
				"TiledImage"
			}.Select(i => "Create" + i));

			var toolsToolbarPanel = new ToolbarModel.ToolbarPanel { Title = "Tools" };
			toolsToolbarPanel.CommandIds.AddRange(GetCommandIds(typeof(Tools)));

			return new ToolbarModel {
				Rows = {
					new ToolbarModel.ToolbarRow {
						Panels = {
							new ToolbarModel.ToolbarPanel {
								Title = "History",
								CommandIds = {
									"Undo",
									"Redo",
									nameof(GenericCommands.Revert),
								}
							},
							createToolbarPanel,
							toolsToolbarPanel,
						}
					},
				}
			};
		}
	}
}
