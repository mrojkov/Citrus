using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;

namespace Tangerine.UI.Timeline.Components
{
	public class RollNodeView : IRollRowView
	{
		protected readonly Row row;
		protected readonly NodeRow nodeData;
		protected readonly Widget widget;
		protected readonly SimpleText label;
		protected readonly EditBox editBox;
		protected readonly Image nodeIcon;
		protected readonly ToolbarButton enterButton;
		protected readonly ToolbarButton expandButton;
		protected readonly ToolbarButton eyeButton;
		protected readonly ToolbarButton lockButton;
		protected readonly ToolbarButton lockAnimationButton;
		protected readonly Widget indentSpacer;
		protected readonly Image imageCombinerIndicator;
		protected readonly ToolbarButton tiedIndicator;

		public NodeRow NodeData => nodeData;
		public SimpleText Label => label;
		public Image ImageCombinerIndicator => imageCombinerIndicator;
		public ToolbarButton TiedIndicator => tiedIndicator;

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
				LayoutCell = new LayoutCell(Alignment.LeftCenter, float.MaxValue)
			};
			editBox = new ThemedEditBox {
				LayoutCell = new LayoutCell(Alignment.LeftCenter, float.MaxValue)
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
			expandButtonContainer.CompoundPresenter.Add(new DelegatePresenter<Widget>(widget => {
				widget.PrepareRendererState();
				var a = new Vector2(0, -4);
				var b = widget.Size + new Vector2(0, 3);
				int colorIndex = nodeData.Node.EditorState().ColorIndex;
				Renderer.DrawRect(a, b, ColorMarks[colorIndex]);
				if (colorIndex != 0) {
					Renderer.DrawRectOutline(a, b, ColorTheme.Current.TimelineRoll.Lines);
				}
			}));
			expandButtonContainer.Updating += delta =>
				expandButton.Visible = nodeData.Node.Animators.Count > 0;
			imageCombinerIndicator = new Image {
				Texture = nodeData.Node is ImageCombiner ? IconPool.GetTexture("Timeline.NoEntry") : NodeIconPool.GetTexture(typeof(ImageCombiner)),
				Color = Color4.Transparent,
				MinMaxSize = new Vector2(21, 16),
				Padding = new Thickness { Left = 5 },
			};
			tiedIndicator = new ToolbarButton {
				Texture = IconPool.GetTexture("Timeline.Tied"),
				Color = Color4.Transparent,
				MinMaxSize = new Vector2(21, 16),
				Padding = new Thickness { Left = 5 },
				Highlightable = false
			};
			tiedIndicator.Clicked += () => ShowTiedNodesContextMenu.Create(this);
			enterButton = NodeCompositionValidator.CanHaveChildren(nodeData.Node.GetType()) ? CreateEnterButton() : null;
			eyeButton = CreateEyeButton();
			lockButton = CreateLockButton();
			lockAnimationButton = CreateLockAnimationButton();
			widget = new Widget {
				Padding = new Thickness { Right = 2 },
				MinHeight = TimelineMetrics.DefaultRowHeight,
				Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center) },
				HitTestTarget = true,
				Nodes = {
					expandButtonContainer,
					(indentSpacer = new Widget()),
					nodeIcon,
					new HSpacer(3),
					label,
					editBox,
					tiedIndicator,
					imageCombinerIndicator,
					new Widget(),
					(Widget)enterButton ?? (Widget)new HSpacer(Theme.Metrics.DefaultToolbarButtonSize.X),
					lockAnimationButton,
					eyeButton,
					lockButton,
				}
			};
			widget.Components.Add(new AwakeBehavior());
			label.AddChangeWatcher(() => nodeData.Node.Id, s => RefreshLabel());
			label.AddChangeWatcher(() => IsGrayedLabel(nodeData.Node), s => RefreshLabel());
			label.AddChangeWatcher(() => nodeData.Node.ContentsPath, s => RefreshLabel());
			widget.CompoundPresenter.Push(new DelegatePresenter<Widget>(RenderBackground));
			editBox.Visible = false;
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
		}

		public Widget Widget => widget;
		public AwakeBehavior AwakeBehavior => widget.Components.Get<AwakeBehavior>();
		public float Indentation { set { indentSpacer.MinMaxWidth = value; } }

		bool IsGrayedLabel(Node node) => node.AsWidget?.Visible ?? true;

		void RefreshLabel()
		{
			var node = nodeData.Node;
			if (!string.IsNullOrEmpty(node.ContentsPath)) {
				label.Text = $"{node.Id} [{node.ContentsPath}]";
			} else {
				label.Text = node.Id;
			}
			label.Color = IsGrayedLabel(node) ? Theme.Colors.BlackText : ColorTheme.Current.TimelineRoll.GrayedLabel;
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
				() => Core.Operations.SetProperty.Perform(nodeData, nameof(NodeRow.Visibility), (NodeVisibility)(((int)nodeData.Visibility + 1) % 3))
			);
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

		static AnimationState GetAnimationState(IAnimable animable)
		{
			var animators = animable.Animators;
			if (animators.Count == 0)
				return AnimationState.None;
			int enabled = 0;
			foreach (var a in animators) {
				if (a.Enabled)
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
				row.Selected ? ColorTheme.Current.Basic.SelectedBackground : ColorTheme.Current.Basic.WhiteBackground);
		}

		public void Rename()
		{
			label.Visible = false;
			editBox.Visible = true;
			editBox.Text = nodeData.Node.Id;
			editBox.SetFocus();
			editBox.Tasks.Add(EditNodeIdTask());
		}

		IEnumerator<object> EditNodeIdTask()
		{
			var initialText = editBox.Text;
			while (editBox.IsFocused()) {
				yield return null;
				if (!row.Selected) {
					editBox.RevokeFocus();
				}
			}
			editBox.Visible = false;
			label.Visible = true;
			if (editBox.Text != initialText) {
				Document.Current.History.DoTransaction(() => {
					Core.Operations.SetProperty.Perform(nodeData.Node, "Id", editBox.Text);
				});
			}
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
				}))
			};
			if (nodeData.Node is Model3D && nodeData.Node.ContentsPath != null) {
				menu.Insert(0, new Command("Edit Attachment Properties", DisplayAttachmentDialog));
			}

			if (NodeCompositionValidator.CanHaveChildren(nodeData.Node.GetType())) {
				menu.Insert(6, new Command("Propagate Markers", () => {
					Document.Current.History.DoTransaction(() => {
						Core.Operations.PropagateMarkers.Perform(nodeData.Node);
					});
				}));
			}

			menu.Popup();
		}

		private void DisplayAttachmentDialog()
		{
			new AttachmentDialog(nodeData.Node as Model3D);
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
	}
}
