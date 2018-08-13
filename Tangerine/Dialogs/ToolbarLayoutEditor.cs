using Lime;
using Tangerine.UI;
using Tangerine.UI.Docking;

namespace Tangerine.Dialogs
{
	class ToolbarLayoutEditor : Widget
	{
		private ToolbarLayout toolbarLayout = AppUserPreferences.Instance.ToolbarLayout;
		private ThemedScrollView availableCommands;
		private ThemedScrollView usedCommands;
		private ThemedDropDownList panelList;
		private ThemedEditBox editBox;

		public ToolbarLayoutEditor()
		{
			Layout = new VBoxLayout { Spacing = 10 };
			Padding = new Thickness(10);
			Initialize();
		}

		private void Initialize()
		{
			Nodes.Clear();
			availableCommands = CreateCommandsView();
			usedCommands = CreateCommandsView();
			panelList = new ThemedDropDownList();
			editBox = new ThemedEditBox();
			CreatePanelControls();
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

		private void CreatePanelControls()
		{
			panelList.Index = 0;
			panelList.LayoutCell = new LayoutCell {
				Alignment = Alignment.Center,
			};
			panelList.Changed += e => {
				RefreshUsedCommands();
				editBox.Text = panelList.Items[panelList.Index].Text;
			};
			foreach (var panel in toolbarLayout.GetAllPanels()) {
				panelList.Items.Add(new CommonDropDownList.Item(panel.Title, panel));
			}
			AddNode(panelList);
			var btnAdd = new ThemedButton("Add panel");
			btnAdd.Clicked += () => AddPanel(isSeparator: false);
			var btnRem = new ThemedButton("Remove panel");
			btnRem.Clicked += RemovePanel;
			var btnUp = new ThemedButton("Move panel up") {
				MinMaxWidth = 100f
			};
			btnUp.Clicked += () => MovePanel(-1);
			var btnDown = new ThemedButton("Move panel down") {
				MinMaxWidth = 100f
			};
			btnDown.Clicked += () => MovePanel(1);
			var btnAddSep = new ThemedButton("Add separator") {
				MinMaxWidth = 100f
			};
			btnAddSep.Clicked += () => AddPanel(isSeparator: true);
			editBox.Submitted += e => {
				var panel = panelList.Items[panelList.Index];
				((ToolbarLayout.ToolbarPanel)panel.Value).Title = e;
				panel.Text = e;
				panelList.TextWidget.Text = e;
			};
			editBox.Text = panelList.Items[panelList.Index].Text;
			AddNode(editBox);
			AddNode(new Widget {
				Layout = new HBoxLayout { Spacing = 10 },
				Nodes = {
					btnAdd,
					btnRem,
					btnUp,
					btnDown,
					btnAddSep
				}
			});
		}

		private void AddPanel(bool isSeparator)
		{
			int index = panelList.Index;
			var panel = new ToolbarLayout.ToolbarPanel(!isSeparator) {
				Title = isSeparator ? "Separator" : "Panel",
				IsSeparator = isSeparator
			};
			panelList.Items.Insert(index, new CommonDropDownList.Item(panel.Title, panel));
			if (index > toolbarLayout.CreatePanelIndex) {
				index -= 1;
			} else {
				toolbarLayout.CreateToolbarPanel.Index += 1;
			}
			for (int i = index; i < toolbarLayout.Panels.Count; ++i) {
				toolbarLayout.Panels[i].Index += 1;
			}
			panel.Index = index;
			toolbarLayout.Panels.Insert(index, panel);
			RefreshAvailableCommands();
			RefreshUsedCommands();
			editBox.Text = panel.Title;
			toolbarLayout.Rebuild(DockManager.Instance.ToolbarArea);
		}

		private void RemovePanel()
		{
			var panel = (ToolbarLayout.ToolbarPanel)panelList.Items[panelList.Index].Value;
			if (panel == toolbarLayout.CreateToolbarPanel) {
				return;
			}
			int index = panelList.Index;
			if (index > toolbarLayout.CreatePanelIndex) {
				index -= 1;
			}
			else {
				toolbarLayout.CreateToolbarPanel.Index -= 1;
			}
			for (int i = index; i < toolbarLayout.Panels.Count; ++i) {
				toolbarLayout.Panels[i].Index -= 1;
			}
			toolbarLayout.Panels.RemoveAt(index);
			panelList.Items.RemoveAt(panelList.Index);
			if (panelList.Index == panelList.Items.Count) {
				panelList.Index -= 1;
			}
			RefreshAvailableCommands();
			RefreshUsedCommands();
			editBox.Text = panelList.Items[panelList.Index].Text;
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
			var panel = (ToolbarLayout.ToolbarPanel)panelList.Items[panelList.Index].Value;
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
			var panel = (ToolbarLayout.ToolbarPanel)panelList.Items[panelList.Index].Value;
			usedCommands.Content.Nodes.Clear();
			if (!panel.Editable) {
				return;
			}
			foreach (var index in panel.CommandIndexes) {
				var command = ToolbarCommandRegister.RegisteredCommands[index];
				usedCommands.Content.AddNode(new CommandRow(command));
			}
		}

		public void ResetToDefaults()
		{
			AppUserPreferences.Instance.ToolbarLayout = toolbarLayout = ToolbarLayout.DefaultToolbarLayout();
			Initialize();
			toolbarLayout.Rebuild(DockManager.Instance.ToolbarArea);
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

		private int FindSelectedPanelIndex(ThemedScrollView view)
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
			var panel = (ToolbarLayout.ToolbarPanel)panelList.Items[panelList.Index].Value;
			if (!panel.Editable) {
				return;
			}
			int leftPanelIndex = FindSelectedPanelIndex(availableCommands);
			if (leftPanelIndex < 0) {
				if (availableCommands.Content.Nodes.Count == 0) {
					return;
				}
				leftPanelIndex = 0;
			}
			int rightPanelIndex = FindSelectedPanelIndex(usedCommands);
			rightPanelIndex = rightPanelIndex < 0 ? 0 : rightPanelIndex;
			var leftPanel = (CommandRow)availableCommands.Content.Nodes[leftPanelIndex];
			int commandIndex = ToolbarCommandRegister.RegisteredCommands.IndexOf(leftPanel.Command);
			availableCommands.Content.Nodes.RemoveAt(leftPanelIndex);
			usedCommands.Content.Nodes.Insert(rightPanelIndex, new CommandRow(leftPanel.Command));
			panel.CommandIndexes.Insert(rightPanelIndex, commandIndex);
			toolbarLayout.Rebuild(DockManager.Instance.ToolbarArea);
		}

		private void RemoveCommand()
		{
			var panel = (ToolbarLayout.ToolbarPanel)panelList.Items[panelList.Index].Value;
			if (!panel.Editable) {
				return;
			}
			int index = FindSelectedPanelIndex(usedCommands);
			if (index < 0) {
				if (usedCommands.Content.Nodes.Count == 0) {
					return;
				}
				index = 0;
			}
			usedCommands.Content.Nodes.RemoveAt(index);
			panel.CommandIndexes.RemoveAt(index);
			RefreshAvailableCommands();
			toolbarLayout.Rebuild(DockManager.Instance.ToolbarArea);
		}

		private void MoveCommand(int dir)
		{
			var index = FindSelectedPanelIndex(usedCommands);
			if (index < 0) {
				return;
			}
			int newIndex = index + dir;
			if (newIndex < 0 || newIndex >= usedCommands.Content.Nodes.Count) {
				return;
			}
			var panel = (ToolbarLayout.ToolbarPanel)panelList.Items[panelList.Index].Value;
			var tmp = panel.CommandIndexes[index];
			panel.CommandIndexes[index] = panel.CommandIndexes[newIndex];
			panel.CommandIndexes[newIndex] = tmp;
			var panel1 = (CommandRow)usedCommands.Content.Nodes[index];
			var panel2 = (CommandRow)usedCommands.Content.Nodes[newIndex];
			panel1.Unlink();
			panel2.Unlink();
			if (newIndex < index) {
				usedCommands.Content.Nodes.Insert(newIndex, panel1);
				usedCommands.Content.Nodes.Insert(index, panel2);
			} else {
				usedCommands.Content.Nodes.Insert(index, panel2);
				usedCommands.Content.Nodes.Insert(newIndex, panel1);
			}
			toolbarLayout.Rebuild(DockManager.Instance.ToolbarArea);
		}

		private void MovePanel(int dir)
		{
			int index = panelList.Index;
			int newIndex = index + dir;
			if (newIndex < 0 || newIndex >= panelList.Items.Count) {
				return;
			}
			var panel1 = panelList.Items[index];
			var panel2 = panelList.Items[newIndex];
			((ToolbarLayout.ToolbarPanel)panel1.Value).Index = newIndex;
			((ToolbarLayout.ToolbarPanel)panel2.Value).Index = index;
			panelList.Items[index] = panel2;
			panelList.Items[newIndex] = panel1;
			panelList.Index = newIndex;
			toolbarLayout.SortPanels();
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
