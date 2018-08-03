using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Core;
using Tangerine.UI.Docking;

namespace Tangerine.UI.SceneView
{
	public enum NodeDecoration
	{
		Frame,
		Button,
		Slider,
		Viewport3D,
		Node3D,
		Image,
		Movie,
		Bone,
		Bone3D,
		DistortionMesh,
		ParticleEmitter,
		SimpleText,
		RichText,
		NineGrid,
		Spline,
		Spline3D,
		Polyline,
		Camera3D,
		Model3D,
		WidgetAdapter3D,
		Invisible,
		AnimationPath,
	}

	static class Extentions
	{
		private static readonly Dictionary<Type, NodeDecoration> typeToDecoration = GenerateDictionary();

		private static Dictionary<Type, NodeDecoration> GenerateDictionary()
		{
			var result = new Dictionary<Type, NodeDecoration>();
			foreach (var decoration in Enum.GetValues(typeof(NodeDecoration))) {
				var type = Type.GetType($"Lime.{Enum.GetName(typeof(NodeDecoration), decoration)},Lime");
				if (type != null) {
					result.Add(type, (NodeDecoration)decoration);
				}
			}
			return result;
		}

		public static NodeDecoration ToNodeDecoration(this Type type)
		{
			if (typeToDecoration.ContainsKey(type)) {
				return typeToDecoration[type];
			}
			throw new ArgumentException();
		}
	}

	public class NodeDecorationsPanel
	{
		public static NodeDecorationsPanel Instance { get; private set; }
		private readonly Panel panel;
		private readonly ThemedScrollView rootWidget;
		private BooleanEditor displayAllEditor;
		private readonly List<List<NodeDecoration>> Groups = new List<List<NodeDecoration>> {
			new List<NodeDecoration> {
				NodeDecoration.Frame,
				NodeDecoration.Button,
				NodeDecoration.Slider,
				NodeDecoration.Viewport3D,
				NodeDecoration.Node3D
			},
			new List<NodeDecoration> {
				NodeDecoration.Image
			},
			new List<NodeDecoration> {
				NodeDecoration.Movie
			},
			new List<NodeDecoration> {
				NodeDecoration.Bone,
				NodeDecoration.Bone3D
			},
			new List<NodeDecoration> {
				NodeDecoration.DistortionMesh
			},
			new List<NodeDecoration> {
				NodeDecoration.ParticleEmitter
			},
			new List<NodeDecoration> {
				NodeDecoration.SimpleText,
				NodeDecoration.RichText,
				NodeDecoration.NineGrid
			},
			new List<NodeDecoration> {
				NodeDecoration.Spline,
				NodeDecoration.Spline3D,
				NodeDecoration.Polyline
			},
			new List<NodeDecoration> {
				NodeDecoration.Camera3D,
				NodeDecoration.Model3D,
				NodeDecoration.WidgetAdapter3D
			},
			new List<NodeDecoration> {
				NodeDecoration.AnimationPath
			}
		};

		private static readonly string[] GroupNames = {
			"Groups", "Images", "Media", "Bones", "DistortionMeshes", "Particles", "UI", "Splines", "3D", "Other"
		};

		private static readonly Dictionary<NodeDecoration, ICommand> NodeDecorationCommands = new Dictionary<NodeDecoration, ICommand> {
			{ NodeDecoration.Bone3D, SceneViewCommands.DisplayBones },
			{ NodeDecoration.Invisible, SceneViewCommands.DisplayPivotsForInvisibleWidgets },
			{ NodeDecoration.AnimationPath, SceneViewCommands.ShowAnimationPath }
		};

		public NodeDecorationsPanel(Panel panel)
		{
			if (Instance != null) {
				throw new InvalidOperationException();
			}
			Instance = this;
			this.panel = panel;
			rootWidget = new ThemedScrollView();
			rootWidget.Content.Layout = new VBoxLayout { Spacing = 6 };
			panel.ContentWidget.AddNode(rootWidget);
			RefreshEditors();
		}

		private void RefreshEditors()
		{
			rootWidget.Content.Nodes.Clear();
			displayAllEditor = new BooleanEditor(SceneViewCommands.DisplayPivotsForAllWidgets);
			displayAllEditor.CheckBox.Changed += e => {
				if (e.ChangedByUser) {
					foreach(var group in rootWidget.Content.Nodes.OfType<DisplayNodeDecorationEditorGroup>()) {
						group.AllEditor.CheckBox.SetChecked(e.Value, true);
					}
				}
			};
			rootWidget.Content.AddNode(displayAllEditor);
			for (int i = 0; i < GroupNames.Length; ++i) {
				var group = new DisplayNodeDecorationEditorGroup(Groups[i], GroupNames[i]);
				rootWidget.Content.AddNode(group);
				displayAllEditor.AddChangeWatcher(() => group.AllEditor.CheckBox.Checked, v => TryCheckAll());
			}
			rootWidget.Content.AddNode(new Separator());
			var displayInvisibleEditor = new DisplayNodeDecorationEditor(NodeDecoration.Invisible) {
				Padding = new Thickness { Left = 24 }
			};
			rootWidget.Content.AddNode(displayInvisibleEditor);
		}

		private void TryCheckAll()
		{
			foreach (var group in rootWidget.Content.Nodes.OfType<DisplayNodeDecorationEditorGroup>()) {
				if (!group.AllEditor.CheckBox.Checked) {
					displayAllEditor.CheckBox.Checked = false;
					return;
				}
			}
			displayAllEditor.CheckBox.Checked = true;
		}

		public static void Refresh() => Instance?.RefreshEditors();

		public static void ToggleDisplayAll(bool changedByUser) => Instance?.displayAllEditor.CheckBox.Toggle(changedByUser);

		public static void Invalidate()
		{
			if (Instance?.panel.ContentWidget.GetRoot() is ThemedInvalidableWindowWidget widget) {
				widget.Window.Invalidate();
			}
		}

		private class ThemedCheckBox : Lime.ThemedCheckBox
		{
			public void SetChecked(bool value, bool changedByUser)
			{
				Checked = value;
				RiseChanged(changedByUser);
			}

			public void Toggle(bool changedByUser)
			{
				SetChecked(!Checked, changedByUser);
			}
		}

		private class BooleanEditor : Widget
		{
			public readonly ThemedCheckBox CheckBox;

			public BooleanEditor(string text)
			{
				Layout = new HBoxLayout() { Spacing = 6 };
				LayoutCell = new LayoutCell(Alignment.LeftCenter, 1);
				Padding = new Thickness(5);
				CheckBox = new ThemedCheckBox();
				AddNode(CheckBox);
				AddNode(new ThemedSimpleText {
					Text = text,
					Padding = new Thickness { Left = 5 }
				});
				AddNode(new Widget());
			}

			public BooleanEditor(ICommand command) : this(command.Text)
			{
				AddNode(new ThemedSimpleText(command.Shortcut.ToString().Replace("Unknown", "")) {
					Padding = new Thickness { Right = 10 }
				});
			}
		}

		private class DisplayNodeDecorationEditor : Widget
		{
			private readonly NodeDecoration decoration;
			private readonly BooleanEditor BooleanEditor;

			public DisplayNodeDecorationEditor(NodeDecoration decoration)
			{
				this.decoration = decoration;
				Layout = new HBoxLayout();
				if (NodeDecorationCommands.ContainsKey(decoration)) {
					BooleanEditor = new BooleanEditor(NodeDecorationCommands[decoration]);
				} else {
					BooleanEditor = new BooleanEditor(Enum.GetName(typeof(NodeDecoration), decoration));
				}
				BooleanEditor.CheckBox.Checked = SceneUserPreferences.Instance.DisplayedNodeDecorations.Contains(decoration);
				BooleanEditor.CheckBox.Changed += UpdateValue;
				BooleanEditor.CheckBox.AddChangeWatcher(
					() => SceneUserPreferences.Instance.DisplayedNodeDecorations.Contains(decoration),
					v => BooleanEditor.CheckBox.Checked = v);
				AddNode(BooleanEditor);
			}

			private void UpdateValue(CheckBox.ChangedEventArgs e)
			{
				if (e.ChangedByUser) {
					if (e.Value) {
						SceneUserPreferences.Instance.DisplayedNodeDecorations.Add(decoration);
					} else {
						SceneUserPreferences.Instance.DisplayedNodeDecorations.Remove(decoration);
					}
					Application.MainWindow.Invalidate();
				}
			}
		}

		private class DisplayNodeDecorationEditorGroup : Widget
		{
			public BooleanEditor AllEditor { get; private set; }
			private ToolbarButton button;
			private readonly List<NodeDecoration> decorations;
			private bool expanded = false;

			public DisplayNodeDecorationEditorGroup(List<NodeDecoration> decorations, string name)
			{
				this.decorations = decorations;
				Id = name;
				Layout = new VBoxLayout { Spacing = 6 };
				Rebuild();
			}

			private void CheckAll(CheckBox.ChangedEventArgs e)
			{
				if (!e.ChangedByUser) {
					return;
				}
				if (e.Value) {
					foreach (var decoration in decorations) {
						SceneUserPreferences.Instance.DisplayedNodeDecorations.Add(decoration);
					}
				} else {
					foreach (var decoration in decorations) {
						SceneUserPreferences.Instance.DisplayedNodeDecorations.Remove(decoration);
					}
				}
				Application.MainWindow.Invalidate();
			}

			private void TryCheckAll()
			{
				foreach (var decoration in decorations) {
					if (!SceneUserPreferences.Instance.DisplayedNodeDecorations.Contains(decoration)) {
						AllEditor.CheckBox.Checked = false;
						return;
					}
				}
				AllEditor.CheckBox.Checked = true;
			}

			private Widget CreateExpandButton()
			{
				button = new ToolbarButton {
					Highlightable = false,
					MinMaxSize = new Vector2(18, 26),
					Padding = new Thickness { Top = 8, Left = 8, Right = 0, Bottom = 8 },
					Texture = expanded ? IconPool.GetTexture("Timeline.minus") : IconPool.GetTexture("Timeline.plus"),
				};
				button.Clicked += ToggleExpanded;
				return button;
			}

			private Widget CreateAllEditor()
			{
				AllEditor = new BooleanEditor(Id);
				AllEditor.CheckBox.Changed += CheckAll;
				AllEditor.AddChangeWatcher(
					() => SceneUserPreferences.Instance.DisplayedNodeDecorations.Count,
					 _ => TryCheckAll());
				return AllEditor;
			}

			private void Rebuild()
			{
				Nodes.Clear();
				AddNode(new Widget {
					Layout = new HBoxLayout { Spacing = 6 },
					Nodes = {
						CreateExpandButton(),
						CreateAllEditor()
					}
				});
				if (expanded) {
					foreach (var decoration in decorations) {
						var editor = new DisplayNodeDecorationEditor(decoration) {
							Padding = new Thickness { Left = 38 }
						};
						AddNode(editor);
					}
				}
				TryCheckAll();
			}

			private void ToggleExpanded()
			{
				expanded = !expanded;
				Rebuild();
			}
		}

		private class Separator : Widget
		{
			public override void Render()
			{
				base.Render();
				PrepareRendererState();
				Renderer.DrawLine(10, Height / 2, Width - 10, Height / 2, ColorTheme.Current.Toolbar.Separator);
			}
		}
	}
}
