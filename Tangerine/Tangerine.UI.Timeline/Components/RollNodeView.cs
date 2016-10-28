using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;

namespace Tangerine.UI.Timeline.Components
{
	public class RollNodeView : IRollWidget
	{
		readonly Row row;
		readonly NodeRow nodeData;
		readonly SimpleText label;
		readonly EditBox editBox;
		readonly Image nodeIcon;
		readonly Widget widget;

		public RollNodeView(Row row, int indentation)
		{
			this.row = row;
			nodeData = row.Components.Get<NodeRow>();
			label = new SimpleText();
			editBox = new EditBox { LayoutCell = new LayoutCell(Alignment.Center, stretchX: float.MaxValue) };
			nodeIcon = new Image(NodeIconPool.GetTexture(nodeData.Node.GetType())) { HitTestTarget = true };
			nodeIcon.MinMaxSize = (Vector2)nodeIcon.Texture.ImageSize;
			var expandButtonContainer = new Widget {
				Layout = new StackLayout { IgnoreHidden = false },
				LayoutCell = new LayoutCell(Alignment.Center),
				Nodes = { CreateExpandButton() }
			};
			widget = new Widget {
				Padding = new Thickness { Left = 4, Right = 2 },
				MinHeight = TimelineMetrics.DefaultRowHeight,
				Layout = new HBoxLayout { CellDefaults = new LayoutCell { Alignment = Alignment.Center } },
				HitTestTarget = true,
				Nodes = {
					new HSpacer(indentation * TimelineMetrics.RollIndentation),
					expandButtonContainer,
					new HSpacer(3),
					nodeIcon,
					new HSpacer(3),
					label,
					editBox,
					new Widget(),
					CreateEyeButton(),
					CreateLockButton(),
				},
			};
			label.AddChangeWatcher(() => nodeData.Node.Id, s => RefreshLabel());
			label.AddChangeWatcher(() => nodeData.Node.ContentsPath, s => RefreshLabel());
			widget.CompoundPresenter.Push(new DelegatePresenter<Widget>(RenderBackground));
			editBox.Visible = false;
			widget.Tasks.Add(HandleDobleClickTask());
		}

		void RefreshLabel()
		{
			var node = nodeData.Node;
			if (!string.IsNullOrEmpty(node.ContentsPath)) {
				label.Text = $"{node.Id} [{node.ContentsPath}]";
			} else {
				label.Text = node.Id;
			}
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
				Core.Operations.SetGenericProperty<NodeVisibility>.Perform(
					() => nodeData.Visibility, value => nodeData.Visibility = value,
					(NodeVisibility)(((int)nodeData.Visibility + 1) % 3)
				);
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

		ToolbarButton CreateExpandButton()
		{
			var button = new ToolbarButton { Highlightable = false };
			button.AddChangeWatcher(
				() => nodeData.Expanded,
				i => button.Texture = IconPool.GetTexture(i ? "Timeline.Expanded" : "Timeline.Collapsed")
			);
			button.Clicked += () => Core.Operations.SetProperty.Perform(nodeData, nameof(NodeRow.Expanded), !nodeData.Expanded);
			button.Updated += delta => button.Visible = nodeData.Node.Animators.Count > 0;
			return button;
		}

		void RenderBackground(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, widget.Size, Document.Current.SelectedRows.Contains(row) ? Colors.SelectedBackground : Colors.WhiteBackground);
		}

		Widget IRollWidget.Widget => widget;

		IEnumerator<object> HandleDobleClickTask()
		{
			while (true) {
				if (nodeIcon.Input.WasKeyPressed(Key.Mouse0DoubleClick)) {
					Core.Operations.ClearRowSelection.Perform();
					Core.Operations.SelectRow.Perform(row);
					label.Visible = false;
					editBox.Visible = true;
					editBox.Text = nodeData.Node.Id;
					editBox.SetFocus();
					editBox.Tasks.Add(EditNodeIdTask());
				} else if (widget.Input.WasKeyPressed(Key.Mouse0DoubleClick)) {
					Core.Operations.EnterNode.Perform(row.Components.Get<NodeRow>().Node);
				}
				yield return null;
			}
		}

		IEnumerator<object> EditNodeIdTask()
		{
			var initialText = editBox.Text;
			while (editBox.IsFocused()) {
				yield return null;
			}
			editBox.Visible = false;
			label.Visible = true;
			if (editBox.Text != initialText) {
				Core.Operations.SetProperty.Perform(nodeData.Node, "Id", editBox.Text);
			}
		}
	}
}
