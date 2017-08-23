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
		protected readonly SimpleText label;
		protected readonly EditBox editBox;
		protected readonly Image nodeIcon;
		protected readonly Widget widget;
		protected readonly ToolbarButton enterButton;
		protected readonly ToolbarButton expandButton;
		protected readonly ToolbarButton eyeButton;
		protected readonly ToolbarButton lockButton;
		protected readonly ToolbarButton lockAnimationButton;
		readonly Widget spacer;

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
				MinMaxSize = new Vector2(16)
			};
			expandButton = CreateExpandButton();
			var expandButtonContainer = new Widget {
				Layout = new StackLayout { IgnoreHidden = false },
				LayoutCell = new LayoutCell(Alignment.Center),
				Nodes = { expandButton }
			};
			expandButtonContainer.Updating += delta =>
				expandButtonContainer.Visible = nodeData.Node.Animators.Count > 0;
			enterButton = NodeCompositionValidator.CanHaveChildren(nodeData.Node.GetType()) ? CreateEnterButton() : null;
			eyeButton = CreateEyeButton();
			lockButton = CreateLockButton();
			lockAnimationButton = CreateLockAnimationButton();
			widget = new Widget {
				Padding = new Thickness { Left = 4, Right = 2 },
				MinHeight = TimelineMetrics.DefaultRowHeight,
				Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center) },
				HitTestTarget = true,
				Nodes = {
					(spacer = new Widget()),
					expandButtonContainer,
					nodeIcon,
					new HSpacer(3),
					label,
					editBox,
					new Widget(),
					(Widget)enterButton ?? (Widget)new HSpacer(Theme.Metrics.DefaultToolbarButtonSize.X),
					lockAnimationButton,
					eyeButton,
					lockButton,
				}
			};
			label.AddChangeWatcher(() => nodeData.Node.Id, s => RefreshLabel());
			label.AddChangeWatcher(() => IsGrayedLabel(nodeData.Node), s => RefreshLabel());
			label.AddChangeWatcher(() => nodeData.Node.ContentsPath, s => RefreshLabel());
			widget.CompoundPresenter.Push(new DelegatePresenter<Widget>(RenderBackground));
			editBox.Visible = false;
			widget.Tasks.Add(HandleDoubleClickTask());
		}

		public Widget Widget => widget;
		public float Indentation { set { spacer.MinMaxWidth = value; } }

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
			button.Clicked += () => Core.Operations.EnterNode.Perform(nodeData.Node);
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
			button.Clicked += () => {
				Core.Operations.SetProperty.Perform(nodeData, nameof(NodeRow.Visibility), (NodeVisibility)(((int)nodeData.Visibility + 1) % 3));
			};
			return button;
		}

		ToolbarButton CreateLockButton()
		{
			var button = new ToolbarButton { Highlightable = false };
			button.AddChangeWatcher(
				() => nodeData.Locked,
				i => button.Texture = IconPool.GetTexture(i ? "Timeline.Lock" : "Timeline.Dot")
			);
			button.Clicked += () => Core.Operations.SetProperty.Perform(nodeData, nameof(NodeRow.Locked), !nodeData.Locked);
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
			button.Clicked += () => {
				var enabled = GetAnimationState(nodeData.Node) == AnimationState.Enabled;
				foreach (var a in nodeData.Node.Animators) {
					Core.Operations.SetProperty.Perform(a, nameof(IAnimator.Enabled), !enabled);
				}
			};
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
			foreach (var a in animators.AsArray) {
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
			button.Clicked += () => Core.Operations.SetProperty.Perform(nodeData, nameof(NodeRow.Expanded), !nodeData.Expanded);
			return button;
		}

		void RenderBackground(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(
				Vector2.Zero, widget.Size,
				row.Selected ? ColorTheme.Current.Basic.SelectedBackground : ColorTheme.Current.Basic.WhiteBackground);
		}

		IEnumerator<object> HandleDoubleClickTask()
		{
			while (true) {
				if (nodeIcon.Input.WasKeyPressed(Key.Mouse0DoubleClick)) {
					Core.Operations.ClearRowSelection.Perform();
					Core.Operations.SelectRow.Perform(row);
					Rename();
				} else if (widget.Input.WasKeyPressed(Key.Mouse0DoubleClick)) {
					Core.Operations.EnterNode.Perform(nodeData.Node);
				}
				yield return null;
			}
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
				Core.Operations.SetProperty.Perform(nodeData.Node, "Id", editBox.Text);
			}
		}
	}
}
