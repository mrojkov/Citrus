using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.Core.Operations;

namespace Tangerine.UI.Timeline.Components
{
	public class RollNodeView : IRollRowView
	{
		protected readonly Row row;
		protected readonly NodeRow nodeData;
		protected readonly Widget widget;
		protected readonly SimpleText label;
		protected readonly Widget editBoxContainer;
		protected readonly ObjectIdInplaceEditor nodeIdEditor;
		protected readonly Image nodeIcon;
		protected readonly ToolbarButton enterButton;
		protected readonly ToolbarButton expandButton;
		protected readonly ToolbarButton eyeButton;
		protected readonly ToolbarButton lockButton;
		protected readonly ToolbarButton lockAnimationButton;
		protected readonly Widget indentSpacer;
		protected readonly LinkIndicatorButtonContainer linkIndicatorButtonContainer = new LinkIndicatorButtonContainer();

		public NodeRow NodeData => nodeData;
		public SimpleText Label => label;
		public LinkIndicatorButtonContainer LinkIndicatorButtonContainer => linkIndicatorButtonContainer;

		private static readonly Color4[] ColorMarks = {
			Color4.Transparent,
			ColorTheme.Current.TimelineRoll.RedMark,
			ColorTheme.Current.TimelineRoll.OrangeMark,
			ColorTheme.Current.TimelineRoll.YellowMark,
			ColorTheme.Current.TimelineRoll.BlueMark,
			ColorTheme.Current.TimelineRoll.GreenMark,
			ColorTheme.Current.TimelineRoll.VioletMark,
			ColorTheme.Current.TimelineRoll.GrayMark,
		};

		public RollNodeView(Row row)
		{
			this.row = row;
			nodeData = row.Components.Get<NodeRow>();
			label = new ThemedSimpleText {
				ForceUncutText = false,
				VAlignment = VAlignment.Center,
				OverflowMode = TextOverflowMode.Ellipsis,
				LayoutCell = new LayoutCell(Alignment.LeftCenter, float.MaxValue)
			};
			editBoxContainer = new Widget {
				Visible = false,
				Layout = new HBoxLayout(),
				LayoutCell = new LayoutCell(Alignment.LeftCenter, float.MaxValue),
			};
			nodeIcon = new Image(NodeIconPool.GetTexture(nodeData.Node.GetType())) {
				HitTestTarget = true,
				MinMaxSize = new Vector2(21, 16),
				Padding = new Thickness { Left = 5 }
			};
			expandButton = CreateExpandButton();
			var expandButtonContainer = new Widget {
				Layout = new StackLayout { IgnoreHidden = false },
				LayoutCell = new LayoutCell(Alignment.Center, stretchX: 0),
				Nodes = { expandButton }
			};
			expandButtonContainer.CompoundPresenter.Add(new SyncDelegatePresenter<Widget>(widget => {
				widget.PrepareRendererState();
				var a = new Vector2(0, -4);
				var b = widget.Size + new Vector2(0, 3);
				int colorIndex = nodeData.Node.EditorState().ColorIndex;
				Renderer.DrawRect(a, b, ColorMarks[colorIndex]);
				if (colorIndex != 0) {
					Renderer.DrawRectOutline(a, b, ColorTheme.Current.TimelineRoll.Lines);
				}
			}));
			expandButtonContainer.Updating += delta => {
				bool visible = false;
				foreach (var a in nodeData.Node.Animators) {
					if (!a.IsZombie && a.AnimationId == Document.Current.AnimationId) {
						visible = true;
						break;
					}
				}
				expandButton.Visible = visible;
			};
			enterButton = NodeCompositionValidator.CanHaveChildren(nodeData.Node.GetType()) ? CreateEnterButton() : null;
			eyeButton = CreateEyeButton();
			lockButton = CreateLockButton();
			lockAnimationButton = CreateLockAnimationButton();
			widget = new Widget {
				Padding = new Thickness { Right = 2 },
				MinHeight = TimelineMetrics.DefaultRowHeight,
				Layout = new HBoxLayout { DefaultCell = new DefaultLayoutCell(Alignment.Center) },
				Nodes = {
					expandButtonContainer,
					(indentSpacer = new Widget()),
					nodeIcon,
					Spacer.HSpacer(3),
					label,
					editBoxContainer,
					linkIndicatorButtonContainer,
					(Widget)enterButton ?? (Widget)Spacer.HSpacer(Theme.Metrics.DefaultToolbarButtonSize.X),
					lockAnimationButton,
					eyeButton,
					lockButton,
				},
			};
			widget.Components.Add(new AwakeBehavior());
			label.AddChangeWatcher(() => nodeData.Node.Id, s => RefreshLabel());
			label.AddChangeWatcher(() => IsGrayedLabel(nodeData.Node), s => RefreshLabel());
			label.AddChangeWatcher(() => nodeData.Node.ContentsPath, s => RefreshLabel());
			widget.CompoundPresenter.Push(new SyncDelegatePresenter<Widget>(RenderBackground));
			widget.Gestures.Add(new ClickGesture(1, ShowPropertyContextMenu));
			widget.Gestures.Add(new DoubleClickGesture(() => {
				Document.Current.History.DoTransaction(() => {
					var labelExtent = label.MeasureUncutText();
					if (label.LocalMousePosition().X < labelExtent.X) {
						Rename();
					} else {
						Core.Operations.EnterNode.Perform(nodeData.Node);
					}
				});
			}));
			nodeIcon.Gestures.Add(new DoubleClickGesture(() => {
				Document.Current.History.DoTransaction(() => {
					Core.Operations.ClearRowSelection.Perform();
					Core.Operations.SelectRow.Perform(row);
					Rename();
				});
			}));
			nodeIdEditor = new ObjectIdInplaceEditor(row, nodeData.Node, label, editBoxContainer);
		}

		public Widget Widget => widget;
		public AwakeBehavior AwakeBehavior => widget.Components.Get<AwakeBehavior>();
		public float Indentation { set { indentSpacer.MinMaxWidth = value; } }

		bool IsGrayedLabel(Node node) =>
			node is Widget w && (!w.Visible || w.Color.A == 0) ||
			node is Frame frame && frame.ClipChildren == ClipMethod.NoRender;

		void RefreshLabel()
		{
			var node = nodeData.Node;
			if (!string.IsNullOrEmpty(node.ContentsPath)) {
				label.Text = $"{node.Id} [{node.ContentsPath}]";
			} else {
				label.Text = node.Id;
			}
			RefreshLabelColor();
		}

		public void RefreshLabelColor()
		{
			label.Color = IsGrayedLabel(nodeData.Node) ? ColorTheme.Current.TimelineRoll.GrayedLabel : Theme.Colors.BlackText;
		}

		ToolbarButton CreateEnterButton()
		{
			var button = new ToolbarButton {
				Texture = IconPool.GetTexture("Timeline.EnterContainer"),
				Highlightable = false
			};
			button.AddTransactionClickHandler(() => Core.Operations.EnterNode.Perform(nodeData.Node));
			return button;
		}

		ToolbarButton CreateEyeButton()
		{
			var button = new ToolbarButton { Highlightable = false };
			button.AddChangeWatcher(() => nodeData.Visibility, i => {
				var texture = "Timeline.Dot";
				if (i == NodeVisibility.Shown) {
					texture = "Timeline.Eye";
				} else if (i == NodeVisibility.Hidden) {
					texture = "Timeline.Cross";
				}
				button.Texture = IconPool.GetTexture(texture);
			});
			button.AddTransactionClickHandler(
				() => {
					var selectedRows = Document.Current.SelectedRows().Select(r => r.Components.Get<NodeRow>())
						.Where(nr => nr != null).ToList();
					if (selectedRows.Contains(nodeData)) {
						selectedRows.ForEach(nr => {
							if (nr != nodeData) {
								SetProperty.Perform(nr, nameof(NodeRow.Visibility), NextVisibility(nodeData.Visibility));
							}
						});
					}
					SetProperty.Perform(nodeData, nameof(NodeRow.Visibility), NextVisibility(nodeData.Visibility));
					Application.Input.ConsumeKey(Key.Mouse0);
				}
			);

			NodeVisibility NextVisibility(NodeVisibility v) => (NodeVisibility)(((int)v + 1) % 3);

			return button;
		}

		ToolbarButton CreateLockButton()
		{
			var button = new ToolbarButton { Highlightable = false };
			button.AddChangeWatcher(
				() => nodeData.Locked,
				i => button.Texture = IconPool.GetTexture(i ? "Timeline.Lock" : "Timeline.Dot")
			);
			button.AddTransactionClickHandler(() => Core.Operations.SetProperty.Perform(nodeData, nameof(NodeRow.Locked), !nodeData.Locked));
			return button;
		}

		ToolbarButton CreateLockAnimationButton()
		{
			var button = new ToolbarButton { Highlightable = false };
			button.AddChangeWatcher(
				() => GetAnimationState(nodeData.Node),
				i => {
					var texture = "Timeline.Empty";
					if (i == AnimationState.Enabled)
						texture = "Timeline.AnimationEnabled";
					else if (i == AnimationState.PartiallyEnabled)
						texture = "Timeline.AnimationPartiallyEnabled";
					else if (i == AnimationState.Disabled)
						texture = "Timeline.AnimationDisabled";
					button.Texture = IconPool.GetTexture(texture);
				}
			);
			button.AddTransactionClickHandler(() => {
				var enabled = GetAnimationState(nodeData.Node) == AnimationState.Enabled;
				foreach (var a in nodeData.Node.Animators) {
					Core.Operations.SetProperty.Perform(a, nameof(IAnimator.Enabled), !enabled);
				}
			});
			return button;
		}

		enum AnimationState
		{
			None,
			Enabled,
			PartiallyEnabled,
			Disabled
		}

		static AnimationState GetAnimationState(IAnimationHost animationHost)
		{
			var animators = animationHost.Animators;
			if (animators.Count == 0)
				return AnimationState.None;
			int enabled = 0;
			foreach (var a in animators) {
				if (a.Enabled && !a.IsZombie)
					enabled++;
			}
			if (enabled == 0)
				return AnimationState.Disabled;
			else if (enabled == animators.Count)
				return AnimationState.Enabled;
			else
				return AnimationState.PartiallyEnabled;
		}

		ToolbarButton CreateExpandButton()
		{
			var button = new ToolbarButton { Highlightable = false };
			button.AddChangeWatcher(
				() => nodeData.Expanded,
				i => button.Texture = IconPool.GetTexture(i ? "Timeline.Expanded" : "Timeline.Collapsed")
			);
			button.AddTransactionClickHandler(() => {
				Core.Operations.SetProperty.Perform(nodeData, nameof(NodeRow.Expanded), !nodeData.Expanded, isChangingDocument: false);
				if (nodeData.Expanded) {
					Timeline.Instance.EnsureRowChildsVisible(row);
				}
			});
			return button;
		}

		void RenderBackground(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(
				Vector2.Zero, widget.Size,
				row.Selected ? Theme.Colors.SelectedBackground : Theme.Colors.WhiteBackground);
		}

		public void Rename()
		{
			if (NodeData.Node.EditorState().Locked) {
				return;
			}
			nodeIdEditor.Rename();
		}

		void ShowPropertyContextMenu()
		{
			Document.Current.History.DoTransaction(() => {
				if (!row.Selected) {
					Core.Operations.ClearRowSelection.Perform();
					Core.Operations.SelectRow.Perform(row);
				}
			});
			Menu submenu;
			var menu = new Menu {
				GenericCommands.ExportScene,
				Command.MenuSeparator,
				Command.Cut,
				Command.Copy,
				Command.Paste,
				Command.Delete,
				Command.MenuSeparator,
				new Command("Rename", () => {
					Document.Current.History.DoTransaction(() => {
						Core.Operations.ClearRowSelection.Perform();
						Core.Operations.SelectRow.Perform(row);
						Rename();
					});
				}),
				new Command("Color mark",
					(submenu = new Menu {
						CreateSetColorMarkCommand("No Color", 0),
						CreateSetColorMarkCommand("Red", 1),
						CreateSetColorMarkCommand("Orange", 2),
						CreateSetColorMarkCommand("Yellow", 3),
						CreateSetColorMarkCommand("Blue", 4),
						CreateSetColorMarkCommand("Green", 5),
						CreateSetColorMarkCommand("Violet", 6),
						CreateSetColorMarkCommand("Gray", 7),
				})),
				Command.MenuSeparator,
				GenericCommands.ConvertToButton
			};

			if (NodeCompositionValidator.CanHaveChildren(nodeData.Node.GetType())) {
				menu.Insert(8, new Command("Propagate Markers", () => {
					Document.Current.History.DoTransaction(() => {
						Core.Operations.PropagateMarkers.Perform(nodeData.Node);
					});
				}));
				menu.Insert(0, GenericCommands.InlineExternalScene);
			}

			menu.Popup();
		}

		private ICommand CreateSetColorMarkCommand(string title, int index)
		{
			return new Command(title,
				() => {
					Document.Current.History.DoTransaction(() => {
						foreach (var n in Document.Current.SelectedNodes()) {
							Core.Operations.SetProperty.Perform(n.EditorState(), nameof(NodeEditorState.ColorIndex), index);
						}
					});
				}) { Checked = nodeData.Node.EditorState().ColorIndex == index };
		}

		public class ObjectIdInplaceEditor
		{
			readonly Row row;
			readonly object obj;
			readonly Widget label;
			readonly Widget editBoxContainer;
			readonly EditBox editBox;
			readonly NodeIdPropertyEditor propEditor;

			public ObjectIdInplaceEditor(Row row, object obj, Widget label, Widget editBoxContainer)
			{
				this.row = row;
				this.label = label;
				this.obj = obj;
				this.editBoxContainer = editBoxContainer;
				propEditor = new NodeIdPropertyEditor(new PropertyEditorParams(
					editBoxContainer,
					new[] { obj },
					new[] { obj },
					obj.GetType(),
					"Id",
					"Id") {
					ShowLabel = false,
					History = Document.Current.History,
					PropertySetter = (o, propertyName, value) => Core.Operations.SetProperty.Perform(o, propertyName, value),
				});
				editBox = propEditor.EditorContainer.Nodes[0] as EditBox;
			}

			public void Rename()
			{
				label.Visible = false;
				editBoxContainer.Visible = true;
				// refresh edit box text, since dataflows for coalesced property value wont refresh it when editBox is invisible
				editBox.Text = (string)propEditor.EditorParams.PropertyInfo.GetValue(obj);
				editBox.SetFocus();
				((WindowWidget)editBoxContainer.GetRoot()).Window.Activate();
				editBoxContainer.Tasks.Add(EditObjectIdTask());
			}

			IEnumerator<object> EditObjectIdTask()
			{
				while (editBox.IsFocused()) {
					yield return null;
					if (!row.Selected) {
						editBox.RevokeFocus();
					}
				}
				// Update editBox once to allow Editor catch focus change and call Submit
				// It wont work by itself, since we're hiding editBox and invisible editBox will not update
				editBox.Update(0.0f);
				editBoxContainer.Visible = false;
				label.Visible = true;
			}
		}
	}
}
