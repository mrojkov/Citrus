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
		readonly CustomCheckbox expandButton;
		readonly Widget expandButtonContainer;

		public RollNodeView(Row row, int indentation)
		{
			this.row = row;			nodeData = row.Components.Get<NodeRow>();
			expandButton = CreateExpandButton();
			label = new SimpleText { AutoSizeConstraints = false, LayoutCell = new LayoutCell(Alignment.Center) };			editBox = new EditBox { AutoSizeConstraints = false, LayoutCell = new LayoutCell(Alignment.Center) };			nodeIcon = new Image {				LayoutCell = new LayoutCell { Alignment = Alignment.Center },				Texture = IconPool.GetTexture("Nodes." + nodeData.Node.GetType(), "Nodes.Unknown"),
			};			Timeline.Instance.Focus.Fields.Add(editBox.Editor);			nodeIcon.MinMaxSize = (Vector2)nodeIcon.Texture.ImageSize;
			expandButtonContainer = new Widget { 
				Layout = new StackLayout { IgnoreHidden = false },
				LayoutCell = new LayoutCell(Alignment.Center),
				Nodes = { expandButton }
			};
			widget = new Widget {				Padding = new Thickness { Left = 4, Right = 2 },
				MinHeight = Metrics.DefaultRowHeight,				Layout = new HBoxLayout(),				Nodes = {
					new HSpacer(indentation * Metrics.RollIndentation),
					expandButtonContainer,
					new HSpacer(3),
					nodeIcon,					new HSpacer(3),
					label,
					editBox,
				},
			};			widget.Presenter = new DelegatePresenter<Widget>(RenderBackground);			editBox.Visible = false;			widget.Tasks.Add(MonitorDoubleClickTask());
		}

		CustomCheckbox CreateExpandButton()
		{
			var button = new CustomCheckbox(IconPool.GetTexture("Timeline.Expanded"), IconPool.GetTexture("Timeline.Collapsed")) {
				LayoutCell = new LayoutCell(Alignment.Center),
			};
			button.Updated += delta => {
				button.Checked = nodeData.Expanded;
				button.Visible = nodeData.Node.Animators.Count > 0;
			};
			button.Clicked += () => {
				Document.Current.History.Execute(new Core.Commands.SetProperty<bool>(() => nodeData.Expanded, value => nodeData.Expanded = value, !nodeData.Expanded));
			};
			return button;
		}

		void RenderBackground(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, widget.Size, Timeline.Instance.SelectedRows.Contains(row) ? Colors.SelectedBackground : Colors.WhiteBackground);
		}

		// XXX
		// Timeline.Instance.Focus.Fields.Remove(editBox.Editor);
		Widget IRollWidget.Widget => widget;

		IEnumerator<object> MonitorDoubleClickTask()
		{
			while (true) {
				label.Text = nodeData.Node.Id;
				if (widget.Input.WasKeyPressed(Key.Mouse0DoubleClick) && widget.HitTest(widget.Input.MousePosition)) {
					Document.Current.History.Execute(
						new Commands.ClearRowSelection(), 
						new Commands.SelectRow(row));
					label.Visible = false;
					editBox.Visible = true;
					editBox.Text = label.Text;
					yield return null;
					editBox.Tasks.Add(EditNodeIdTask());
				}
				yield return null;
			}
		}

		IEnumerator<object> EditNodeIdTask()
		{
			editBox.Input.CaptureMouse();
			while (true) {
				var commitChanges = editBox.Input.WasKeyReleased(Key.Mouse0) && !editBox.HitTest(editBox.Input.MousePosition) ||
					editBox.Input.WasKeyPressed(Key.Enter);
				var cancelChanges = editBox.Input.WasKeyPressed(Key.Escape);
				if (commitChanges || cancelChanges) {
					editBox.Visible = false;
					label.Visible = true;
					if (commitChanges) {
						Document.Current.History.Execute(new Core.Commands.SetProperty(nodeData.Node, "Id", editBox.Text));
					}
					break;
				}
				yield return null;
			}
			editBox.Input.ReleaseMouse();
		}
	}
}
