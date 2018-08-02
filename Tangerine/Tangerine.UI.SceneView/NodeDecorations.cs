using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Core;
using Tangerine.UI.Docking;

namespace Tangerine.UI.SceneView
{
	public class NodeDecorations
	{
		public static NodeDecorations Instance { get; private set; }
		private readonly Panel panel;
		private readonly ThemedScrollView rootWidget;
		private BooleanEditor showAllEditor;
		private readonly List<List<Type>> Groups = new List<List<Type>> {
			new List<Type> { typeof(Frame), typeof(Button), typeof(Slider), typeof(Viewport3D), typeof(Node3D) },
			new List<Type> { typeof(Image), typeof(ImageCombiner) },
			new List<Type> { typeof(Audio), typeof(Movie) },
			new List<Type> { typeof(Bone) },
			new List<Type> { typeof(DistortionMesh) },
			new List<Type> { typeof(ParticleEmitter), typeof(ParticleModifier), typeof(EmitterShapePoint), typeof(ParticlesMagnet) },
			new List<Type> { typeof(SimpleText), typeof(RichText), typeof(TextStyle), typeof(NineGrid) },
			new List<Type> { typeof(Spline), typeof(SplinePoint), typeof(SplineGear), typeof(Spline3D), typeof(SplinePoint3D), typeof(SplineGear3D), typeof(Polyline), typeof(PolylinePoint) },
			new List<Type> { typeof(Camera3D), typeof(Model3D), typeof(WidgetAdapter3D), typeof(LightSource) }
		};
		private readonly string[] GroupNames = { "Groups", "Images", "Media", "Bones", "DistortionMeshes", "Particles", "UI", "Splines", "3D" };

		public NodeDecorations(Panel panel)
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
			showAllEditor = new BooleanEditor("All");
			showAllEditor.CheckBox.Changed += e => {
				if (e.ChangedByUser) {
					foreach(var group in rootWidget.Content.Nodes.OfType<DisplayPivotPropertyEditorGroup>()) {
						group.AllEditor.CheckBox.SetChecked(e.Value, true);
					}
				}
			};
			rootWidget.Content.AddNode(showAllEditor);
			for (int i = 0; i < GroupNames.Length; ++i) {
				var group = new DisplayPivotPropertyEditorGroup(Groups[i], GroupNames[i]);
				rootWidget.Content.AddNode(group);
				showAllEditor.AddChangeWatcher(() => group.AllEditor.CheckBox.Checked, v => TryCheckAll());
			}
			rootWidget.Content.AddNode(new Separator());
			var showInvisibleEditor = new BooleanEditor("Invisible");
			showInvisibleEditor.CheckBox.Changed += e => {
				if (e.ChangedByUser) {
					SceneUserPreferences.Instance.DisplayNodeDecorationsForInvisibleWidgets = e.Value;
				}
			};
			showInvisibleEditor.CheckBox.AddChangeWatcher(
				() => SceneUserPreferences.Instance.DisplayNodeDecorationsForInvisibleWidgets,
				v => showInvisibleEditor.CheckBox.Checked = v);
			showInvisibleEditor.Padding = new Thickness { Left = 29, Top = 5 };
			showInvisibleEditor.CheckBox.Checked = SceneUserPreferences.Instance.DisplayNodeDecorationsForInvisibleWidgets;
			rootWidget.Content.AddNode(showInvisibleEditor);
		}

		private void TryCheckAll()
		{
			foreach (var group in rootWidget.Content.Nodes.OfType<DisplayPivotPropertyEditorGroup>()) {
				if (!group.AllEditor.CheckBox.Checked) {
					showAllEditor.CheckBox.Checked = false;
					return;
				}
			}
			showAllEditor.CheckBox.Checked = true;
		}

		public static void Refresh() => Instance?.RefreshEditors();

		private class ThemedCheckBox : Lime.ThemedCheckBox
		{
			public void SetChecked(bool value, bool changedByUser)
			{
				Checked = value;
				RiseChanged(changedByUser);
			}
		}

		private class BooleanEditor : Widget
		{
			public readonly ThemedCheckBox CheckBox;

			public BooleanEditor(string text)
			{
				Layout = new HBoxLayout() { Spacing = 6 };
				Padding = new Thickness(5);
				CheckBox = new ThemedCheckBox();
				AddNode(CheckBox);
				AddNode(new ThemedSimpleText {
					Text = text,
					Padding = new Thickness { Left = 5 }
				});
			}
		}

		private class DisplayPivotPropertyEditor : Widget
		{
			private readonly Type type;
			public readonly BooleanEditor BooleanEditor;

			public DisplayPivotPropertyEditor(Type type)
			{
				this.type = type;
				Layout = new VBoxLayout();
				BooleanEditor = new BooleanEditor(type.Name);
				BooleanEditor.CheckBox.Checked = SceneUserPreferences.Instance.DisplayNodeDecorationsForTypes.Contains(type.Name);
				BooleanEditor.CheckBox.Changed += UpdateValue;
				AddNode(BooleanEditor);
			}

			private void UpdateValue(CheckBox.ChangedEventArgs e)
			{
				if (e.Value) {
					SceneUserPreferences.Instance.DisplayNodeDecorationsForTypes.Add(type.Name);
				} else {
					SceneUserPreferences.Instance.DisplayNodeDecorationsForTypes.Remove(type.Name);
				}
			}
		}

		private class DisplayPivotPropertyEditorGroup : Widget
		{
			public BooleanEditor AllEditor { get; private set; }
			private ToolbarButton button;
			private readonly List<Type> allTypes;
			private List<Type> types;
			private bool expanded = false;

			public DisplayPivotPropertyEditorGroup(List<Type> types, string name)
			{
				allTypes = types;
				Id = name;
				Layout = new VBoxLayout { Spacing = 6 };
				Rebuild();
			}

			private void CheckAll(CheckBox.ChangedEventArgs e)
			{
				if (!e.ChangedByUser) {
					return;
				}
				if (!expanded) {
					if (e.Value) {
						foreach (var type in types) {
							SceneUserPreferences.Instance.DisplayNodeDecorationsForTypes.Add(type.Name);
						}
					} else {
						foreach (var type in types) {
							SceneUserPreferences.Instance.DisplayNodeDecorationsForTypes.Remove(type.Name);
						}
					}
				} else {
					AllEditor.CheckBox.Checked = e.Value;
					foreach (var editor in Nodes.Skip(1).OfType<DisplayPivotPropertyEditor>()) {
						editor.BooleanEditor.CheckBox.Checked = e.Value;
					}
				}
			}

			private void TryCheckAll()
			{
				foreach (var type in types) {
					if (!SceneUserPreferences.Instance.DisplayNodeDecorationsForTypes.Contains(type.Name)) {
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
					Color = Color4.Red
				};
				button.Clicked += ToggleExpanded;
				return button;
			}

			private Widget CreateAllEditor()
			{
				AllEditor = new BooleanEditor(Id);
				AllEditor.CheckBox.Changed += CheckAll;
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
				types = Project.Current.RegisteredNodeTypes.Where(t => allTypes.Contains(t)).ToList();
				if (expanded) {
					foreach (var type in types) {
						var editor = new DisplayPivotPropertyEditor(type);
						editor.BooleanEditor.CheckBox.Changed += _ => TryCheckAll();
						editor.BooleanEditor.CheckBox.Checked = SceneUserPreferences.Instance.DisplayNodeDecorationsForTypes.Contains(type.Name);
						editor.Padding = new Thickness { Left = 38 };
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
				Renderer.DrawLine(10, Height / 2, Width - 10, Height / 2, Color4.Gray.Lighten(0.3f));
			}
		}
	}
}
