using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

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
			label = new SimpleText { HitTestTarget = true, LayoutCell = new LayoutCell(Alignment.Center) };
			editBox = new EditBox { AutoSizeConstraints = false, LayoutCell = new LayoutCell(Alignment.Center, stretchX: 1000) };
			nodeIcon = new Image {
				LayoutCell = new LayoutCell { Alignment = Alignment.Center },
				Texture = IconPool.GetTexture("Nodes." + nodeData.Node.GetType(), "Nodes.Unknown"),
			};
			nodeIcon.MinMaxSize = (Vector2)nodeIcon.Texture.ImageSize;
			var expandButtonContainer = new Widget {
				Layout = new StackLayout { IgnoreHidden = false },
				LayoutCell = new LayoutCell(Alignment.Center),
				Nodes = { CreateExpandButton() }
			};
			widget = new Widget {
				Padding = new Thickness { Left = 4, Right = 2 },
				MinHeight = Metrics.TimelineDefaultRowHeight,
				Layout = new HBoxLayout(),
				HitTestTarget = true,
				Nodes = {
					new HSpacer(indentation * Metrics.TimelineRollIndentation),
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
			label.Tasks.Add(new Property<string>(() => nodeData.Node.Id).DistinctUntilChanged().Consume(s => RefreshLabel()));
			label.Tasks.Add(new Property<string>(() => nodeData.Node.ContentsPath).DistinctUntilChanged().Consume(s => RefreshLabel()));
			widget.CompoundPresenter.Push(new DelegatePresenter<Widget>(RenderBackground));
			editBox.Visible = false;
			widget.Tasks.Add(RenameOnDoubleClickTask());
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
			var button = new ToolbarButton { LayoutCell = new LayoutCell(Alignment.Center) };
			button.Tasks.Add(new Property<NodeVisibility>(() => nodeData.Visibility).DistinctUntilChanged().Consume(i => {
				var texture = "Timeline.Dot";
				if (i == NodeVisibility.Shown) {
					texture = "Timeline.Eye";
				} else if (i == NodeVisibility.Hidden) {
					texture = "Timeline.Cross";
				}
				button.Texture = IconPool.GetTexture(texture);
			}));
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
			var button = new ToolbarButton { LayoutCell = new LayoutCell(Alignment.Center) };
			button.Tasks.Add(new Property<bool>(() => nodeData.Locked).DistinctUntilChanged().Consume(i => {
				button.Texture = IconPool.GetTexture(i ? "Timeline.Lock" : "Timeline.Dot");
			}));
			button.Clicked += () => {
				Core.Operations.SetGenericProperty<bool>.Perform(() => nodeData.Locked, value => nodeData.Locked = value, !nodeData.Locked);
			};
			return button;
		}

		ToolbarButton CreateExpandButton()
		{
			var button = new ToolbarButton { LayoutCell = new LayoutCell(Alignment.Center) };
			button.Tasks.Add(new Property<bool>(() => nodeData.Expanded).DistinctUntilChanged().Consume(i => {
				button.Texture = IconPool.GetTexture(i ? "Timeline.Expanded" : "Timeline.Collapsed");
			}));
			button.Clicked += () => {
				Core.Operations.SetGenericProperty<bool>.Perform(() => nodeData.Expanded, value => nodeData.Expanded = value, !nodeData.Expanded);
			};
			button.Updated += delta => button.Visible = nodeData.Node.Animators.Count > 0;
			return button;
		}

		void RenderBackground(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, widget.Size, Timeline.Instance.SelectedRows.Contains(row) ? Colors.SelectedBackground : Colors.WhiteBackground);
		}

		Widget IRollWidget.Widget => widget;

		IEnumerator<object> RenameOnDoubleClickTask()
		{
			while (true) {
				if (widget.Input.WasKeyPressed(Key.Mouse0DoubleClick)) {
					if (label.IsMouseOver()) {
						Operations.ClearRowSelection.Perform();
						Operations.SelectRow.Perform(row);
						label.Visible = false;
						editBox.Visible = true;
						editBox.Text = nodeData.Node.Id;
						yield return null;
						editBox.SetFocus();
						editBox.Tasks.Add(EditNodeIdTask());
					} else if (widget.IsMouseOver()) {
						Operations.EnterNode.Perform(row.Components.Get<NodeRow>().Node);
					}
				}
				yield return null;
			}
		}

		IEnumerator<object> EditNodeIdTask()
		{
			editBox.Input.CaptureMouse();
			var initialText = editBox.Text;
			while (editBox.IsFocused()) {
				yield return null;
			}
			editBox.Input.ReleaseMouse();
			editBox.Visible = false;
			label.Visible = true;
			if (editBox.Text != initialText) {
				Core.Operations.SetProperty.Perform(nodeData.Node, "Id", editBox.Text);
			}
		}
	}
}
