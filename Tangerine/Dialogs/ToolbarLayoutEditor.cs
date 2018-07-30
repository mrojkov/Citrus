using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.UI;
using Tangerine.UI.Docking;

namespace Tangerine.Dialogs
{
	class ToolbarLayoutEditor : Widget
	{
		private readonly ToolbarLayout toolbarLayout = AppUserPreferences.Instance.ToolbarLayout;
		private readonly ThemedScrollView availableCommands = CreateCommandsView();
		private readonly ThemedScrollView usedCommands = CreateCommandsView();
		private readonly ThemedDropDownList rowList = new ThemedDropDownList();
		private readonly ThemedEditBox editBox = new ThemedEditBox();

		public ToolbarLayoutEditor()
		{
			Layout = new VBoxLayout { Spacing = 10 };
			Padding = new Thickness(10);
			CreateRowControls();
			var widget = new Widget {
				Layout = new HBoxLayout()
			};
			AddNode(widget);
			widget.AddNode(AddLabel(availableCommands, "Available commands:"));
			widget.AddNode(AddLabel(usedCommands, "Used commands:"));
			RefreshAvailableCommands();
			RefreshUsedCommands();
			CreateCommandControls();
		}

		private static Widget AddLabel(Widget widget, string label)
		{
			return new Widget {
				Layout = new VBoxLayout { Spacing = 10 },
				Nodes = {
					new ThemedSimpleText(label),
					widget
				}
			};
		}

		private void CreateRowControls()
		{
			rowList.Index = 0;
			rowList.LayoutCell = new LayoutCell {
				Alignment = Alignment.Center,
			};
			rowList.Changed += e => {
				RefreshUsedCommands();
				editBox.Text = rowList.Items[rowList.Index].Text;
			};
			foreach (var row in toolbarLayout.GetAllRows()) {
				rowList.Items.Add(new CommonDropDownList.Item(row.Title, row));
			}
			AddNode(rowList);
			var btnAdd = new ThemedButton("Add row");
			btnAdd.Clicked += AddRow;
			var btnRem = new ThemedButton("Remove row");
			btnRem.Clicked += RemoveRow;
			var btnUp = new ThemedButton("Move row up") {
				MinMaxWidth = 100f
			};
			btnUp.Clicked += () => MoveRow(-1);
			var btnDown = new ThemedButton("Move row down") {
				MinMaxWidth = 100f
			};
			btnDown.Clicked += () => MoveRow(1);
			editBox.Submitted += e => {
				var row = rowList.Items[rowList.Index];
				((ToolbarLayout.ToolbarRow)row.Value).Title = e;
				row.Text = e;
				rowList.TextWidget.Text = e;
			};
			editBox.Text = rowList.Items[rowList.Index].Text;
			AddNode(editBox);
			AddNode(new Widget {
				Layout = new HBoxLayout { Spacing = 10 },
				Nodes = {
					btnAdd,
					btnRem,
					btnUp,
					btnDown
				}
			});
		}

		private void AddRow()
		{
			int index = rowList.Index;
			var row = new ToolbarLayout.ToolbarRow {
				Title = "Row",
			};
			rowList.Items.Insert(index, new CommonDropDownList.Item(row.Title, row));
			if (index > toolbarLayout.CreateRowIndex) {
				index -= 1;
			} else {
				toolbarLayout.CreateToolbarRow.Index += 1;
			}
			for (int i = index; i < toolbarLayout.Rows.Count; ++i) {
				toolbarLayout.Rows[i].Index += 1;
			}
			row.Index = index;
			toolbarLayout.Rows.Insert(index, row);
			RefreshAvailableCommands();
			RefreshUsedCommands();
			editBox.Text = row.Title;
			toolbarLayout.Rebuild(DockManager.Instance.ToolbarArea);
		}

		private void RemoveRow()
		{
			var row = (ToolbarLayout.ToolbarRow)rowList.Items[rowList.Index].Value;
			if (!row.Editable) {
				return;
			}
			int index = rowList.Index;
			if (index > toolbarLayout.CreateRowIndex) {
				index -= 1;
			}
			else {
				toolbarLayout.CreateToolbarRow.Index -= 1;
			}
			for (int i = index; i < toolbarLayout.Rows.Count; ++i) {
				toolbarLayout.Rows[i].Index -= 1;
			}
			toolbarLayout.Rows.RemoveAt(index);
			rowList.Items.RemoveAt(rowList.Index);
			if (rowList.Index == rowList.Items.Count) {
				rowList.Index -= 1;
			}
			RefreshAvailableCommands();
			RefreshUsedCommands();
			editBox.Text = rowList.Items[rowList.Index].Text;
			toolbarLayout.Rebuild(DockManager.Instance.ToolbarArea);
		}

			private static ThemedScrollView CreateCommandsView()
		{
			var view = new ThemedScrollView();
			view.Content.Layout = new VBoxLayout { Spacing = 4 };
			view.Padding = new Thickness(10);
			return view;
		}

		private void RefreshAvailableCommands()
		{
			var row = (ToolbarLayout.ToolbarRow)rowList.Items[rowList.Index].Value;
			availableCommands.Content.Nodes.Clear();
			for (int i = 0; i < ToolbarCommandRegister.RegisteredCommands.Count; ++i) {
				var command = ToolbarCommandRegister.RegisteredCommands[i];
				if (toolbarLayout.ContainsIndex(i)) {
					continue;
				}
				availableCommands.Content.AddNode(new CommandRow(command));
			}
		}

		private void RefreshUsedCommands()
		{
			var row = (ToolbarLayout.ToolbarRow)rowList.Items[rowList.Index].Value;
			usedCommands.Content.Nodes.Clear();
			if (!row.Editable) {
				return;
			}
			foreach (var index in row.CommandIndexes) {
				var command = ToolbarCommandRegister.RegisteredCommands[index];
				usedCommands.Content.AddNode(new CommandRow(command));
			}
		}

		private void CreateCommandControls()
		{
			var addCmd = new ThemedButton("Add command") {
				MinMaxWidth = 100f
			};
			addCmd.Clicked += AddCommand;
			var remCmd = new ThemedButton("Remove command") {
				MinMaxWidth = 100f
			};
			remCmd.Clicked += RemoveCommand;
			var moveCmdUp = new ThemedButton("Move command up") {
				MinMaxWidth = 130f
			};
			moveCmdUp.Clicked += () => MoveCommand(-1);
			var moveCmdDown = new ThemedButton("Move command down") {
				MinMaxWidth = 130f
			};
			moveCmdDown.Clicked += () => MoveCommand(1);
			AddNode(new Widget {
				Layout = new HBoxLayout { Spacing = 10 },
				Nodes = {
					addCmd,
					remCmd,
					moveCmdUp,
					moveCmdDown
				}
			});
		}

		private int FindSelectedRowIndex(ThemedScrollView view)
		{
			int index;
			for (index = view.Content.Nodes.Count - 1; index >= 0; --index) {
				var rowNode = (CommandRow)view.Content.Nodes[index];
				if (rowNode.Selected) {
					break;
				}
			}
			return index;
		}

		private void AddCommand()
		{
			var row = (ToolbarLayout.ToolbarRow)rowList.Items[rowList.Index].Value;
			if (!row.Editable) {
				return;
			}
			int leftRowIndex = FindSelectedRowIndex(availableCommands);
			if (leftRowIndex < 0) {
				if (availableCommands.Content.Nodes.Count == 0) {
					return;
				}
				leftRowIndex = 0;
			}
			int rightRowIndex = FindSelectedRowIndex(usedCommands);
			rightRowIndex = rightRowIndex < 0 ? 0 : rightRowIndex;
			var leftRow = (CommandRow)availableCommands.Content.Nodes[leftRowIndex];
			int commandIndex = ToolbarCommandRegister.RegisteredCommands.IndexOf(leftRow.Command);
			availableCommands.Content.Nodes.RemoveAt(leftRowIndex);
			usedCommands.Content.Nodes.Insert(rightRowIndex, new CommandRow(leftRow.Command));
			row.CommandIndexes.Insert(rightRowIndex, commandIndex);
			toolbarLayout.Rebuild(DockManager.Instance.ToolbarArea);
		}

		private void RemoveCommand()
		{
			var row = (ToolbarLayout.ToolbarRow)rowList.Items[rowList.Index].Value;
			if (!row.Editable) {
				return;
			}
			int index = FindSelectedRowIndex(usedCommands);
			if (index < 0) {
				if (usedCommands.Content.Nodes.Count == 0) {
					return;
				}
				index = 0;
			}
			usedCommands.Content.Nodes.RemoveAt(index);
			row.CommandIndexes.RemoveAt(index);
			RefreshAvailableCommands();
			toolbarLayout.Rebuild(DockManager.Instance.ToolbarArea);
		}

		private void MoveCommand(int dir)
		{
			var index = FindSelectedRowIndex(usedCommands);
			if (index < 0) {
				return;
			}
			int newIndex = index + dir;
			if (newIndex < 0 || newIndex >= usedCommands.Content.Nodes.Count) {
				return;
			}
			var row = (ToolbarLayout.ToolbarRow)rowList.Items[rowList.Index].Value;
			var tmp = row.CommandIndexes[index];
			row.CommandIndexes[index] = row.CommandIndexes[newIndex];
			row.CommandIndexes[newIndex] = tmp;
			var row1 = (CommandRow)usedCommands.Content.Nodes[index];
			var row2 = (CommandRow)usedCommands.Content.Nodes[newIndex];
			row1.Unlink();
			row2.Unlink();
			if (newIndex < index) {
				usedCommands.Content.Nodes.Insert(newIndex, row1);
				usedCommands.Content.Nodes.Insert(index, row2);
			} else {
				usedCommands.Content.Nodes.Insert(index, row2);
				usedCommands.Content.Nodes.Insert(newIndex, row1);
			}
			toolbarLayout.Rebuild(DockManager.Instance.ToolbarArea);
		} 

		private void MoveRow(int dir)
		{
			int index = rowList.Index;
			int newIndex = index + dir;
			if (newIndex < 0 || newIndex >= rowList.Items.Count) {
				return;
			}
			var row1 = rowList.Items[index];
			var row2 = rowList.Items[newIndex];
			((ToolbarLayout.ToolbarRow)row1.Value).Index = newIndex;
			((ToolbarLayout.ToolbarRow)row2.Value).Index = index;
			rowList.Items[index] = row2;
			rowList.Items[newIndex] = row1;
			rowList.Index = newIndex;
			toolbarLayout.SortRows();
			toolbarLayout.Rebuild(DockManager.Instance.ToolbarArea);
		}

		private class CommandRow : Button
		{
			public ICommand Command { get; private set; }
			public bool Selected { get; private set; } = false;

			public CommandRow(ICommand command)
			{
				Command = command;
				Layout = new HBoxLayout { Spacing = 10 };
				AddNode(new Image {
					Texture = command.Icon,
					MinMaxSize = new Vector2(21, 21),
				});
				AddNode(new ThemedSimpleText {
					Text = command.Text,
					LayoutCell = new LayoutCell(Alignment.Center),
					Padding = new Thickness { Left = 5 }
				});
				Awoke += Awake;
			}

			private static void Awake(Node owner)
			{
				var row = (CommandRow)owner;
				row.Clicked += () => {
					foreach (var node in row.Parent.Nodes) {
						if (!(node is CommandRow crow)) {
							return;
						}
						crow.Selected = false;
					}
					row.Selected = true;
				};
			}

			public override void Render()
			{
				PrepareRendererState();
				if (Selected) {
					Renderer.DrawRectOutline(new Vector2(0, 0), new Vector2(Width, Height), ColorTheme.Current.Toolbar.ButtonHighlightBorder);
				}
			}
		}
	}
}
