using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;

namespace Tangerine.UI.Timeline.Components
{
	public class RollFolderView : IRollWidget
	{
		protected readonly Row row;
		protected readonly Folder folder;
		protected readonly SimpleText label;
		protected readonly EditBox editBox;
		protected readonly Image nodeIcon;
		protected readonly Widget widget;
		protected readonly ToolbarButton expandButton;
		protected readonly ToolbarButton eyeButton;
		protected readonly ToolbarButton lockButton;
		readonly Widget spacer;

		public RollFolderView(Row row)
		{
			this.row = row;
			folder = row.Components.Get<FolderRow>().Folder;
			label = new SimpleText();
			editBox = new EditBox { LayoutCell = new LayoutCell(Alignment.Center, stretchX: float.MaxValue) };
			nodeIcon = new Image(IconPool.GetTexture("Tools.NewFolder")) {
				HitTestTarget = true,
				MinMaxSize = new Vector2(16, 16)
			};
			expandButton = CreateExpandButton();
			var expandButtonContainer = new Widget {
				Layout = new StackLayout { IgnoreHidden = false },
				LayoutCell = new LayoutCell(Alignment.Center),
				Visible = true,
				Nodes = { expandButton }
			};
			eyeButton = CreateEyeButton();
			lockButton = CreateLockButton();
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
					eyeButton,
					lockButton,
				},
			};
			label.AddChangeWatcher(() => folder.Id, s => label.Text = folder.Id);
			widget.CompoundPresenter.Push(new DelegatePresenter<Widget>(RenderBackground));
			editBox.Visible = false;
			widget.Tasks.Add(HandleDobleClickTask());
		}

		Widget IRollWidget.Widget => widget;
		float IRollWidget.Indentation { set { spacer.MinMaxWidth = value; } }

		ToolbarButton CreateEyeButton()
		{
			var button = new ToolbarButton { Highlightable = false };
			button.Texture = IconPool.GetTexture("Timeline.Eye");
			button.Clicked += () => {
				var visibility = NodeVisibility.Hidden;
				if (InnerNodes(folder).All(i => i.EditorState().Visibility == NodeVisibility.Hidden)) {
					visibility = NodeVisibility.Shown;
				} else if (InnerNodes(folder).All(i => i.EditorState().Visibility == NodeVisibility.Shown)) {
					visibility = NodeVisibility.Default;
				}
				foreach (var node in InnerNodes(folder)) {
					Core.Operations.SetProperty.Perform(node.EditorState(), nameof(NodeEditorState.Visibility), visibility);
				}
			};
			return button;
		}

		ToolbarButton CreateLockButton()
		{
			var button = new ToolbarButton { Highlightable = false };
			button.Texture = IconPool.GetTexture("Timeline.Lock");
			button.Clicked += () => {
				var locked = InnerNodes(folder).All(i => !i.EditorState().Locked);
				foreach (var node in InnerNodes(folder)) {
					Core.Operations.SetProperty.Perform(node.EditorState(), nameof(NodeEditorState.Locked), locked);
				}
			};
			return button;
		}

		IEnumerable<Node> InnerNodes(Folder folder)
		{
			foreach (var i in folder.Items) {
				if (i is Node) {
					yield return (Node)i;
				} else if (i is Folder) {
					foreach (var j in InnerNodes(i as Folder)) {
						yield return j;
					}
				}
			}
		}

		ToolbarButton CreateExpandButton()
		{
			var button = new ToolbarButton { Highlightable = false };
			button.Texture = IconPool.GetTexture("Timeline.Expanded");
			button.AddChangeWatcher(
				() => folder.Expanded,
				i => button.Texture = IconPool.GetTexture(i ? "Timeline.Expanded" : "Timeline.Collapsed")
			);
			button.Clicked += () => Core.Operations.SetProperty.Perform(folder, nameof(Folder.Expanded), !folder.Expanded);
			return button;
		}

		void RenderBackground(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, widget.Size, row.Selected ? Colors.SelectedBackground : Colors.WhiteBackground);
		}

		IEnumerator<object> HandleDobleClickTask()
		{
			while (true) {
				if (nodeIcon.Input.WasKeyPressed(Key.Mouse0DoubleClick)) {
					Core.Operations.ClearRowSelection.Perform();
					Core.Operations.SelectRow.Perform(row);
					label.Visible = false;
					editBox.Visible = true;
					editBox.Text = folder.Id;
					editBox.SetFocus();
					editBox.Tasks.Add(EditFolderIdTask());
				}
				yield return null;
			}
		}

		IEnumerator<object> EditFolderIdTask()
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
				Core.Operations.SetProperty.Perform(folder, "Id", editBox.Text);
			}
		}
	}
}
