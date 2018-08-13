using Lime;
using System;
using System.Linq;
using Tangerine.UI;
using Tangerine.UI.Docking;

namespace Tangerine.Dialogs
{
	public class ToolbarLayoutEditor : Widget
	{
		private ToolbarLayout toolbarLayout = AppUserPreferences.Instance.ToolbarLayout;
		private ListBox availableCommands;
		private ListBox usedCommands;
		private ListBox panelList;

		public ToolbarLayoutEditor()
		{
			Layout = new VBoxLayout { Spacing = 10 };
			Padding = new Thickness(10);
			Initialize();
		}

		private void Initialize()
		{
			Nodes.Clear();
			availableCommands = CreateListBox();
			usedCommands = CreateListBox();
			panelList = CreateListBox();
			CreatePanelControls();
			var widget = new Widget {
				Layout = new HBoxLayout()
			};
			AddNode(widget);
			widget.AddNode(AddLabel(availableCommands, "Available commands:"));
			widget.AddNode(CreateCommandControls());
			widget.AddNode(AddLabel(usedCommands, "Used commands:"));
			RefreshAvailableCommands();
			RefreshUsedCommands();
		}

		private ListBox CreateListBox()
		{
			var result = new ListBox();
			result.Content.Padding = new Thickness(6);
			result.CompoundPostPresenter.Add(new DelegatePresenter<Widget>(w => {
				w.PrepareRendererState();
				Renderer.DrawRectOutline(0, 0, w.Width, w.Height, Theme.Colors.ControlBorder);
			}));
			return result;
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

		private ToolbarButton CreateButton(string textureid, string tip, Action clicked, params ListBox[] listBoxes)
		{
			var result = new ToolbarButton {
				Texture = IconPool.GetTexture(textureid),
				Tip = tip
			};
			result.Clicked += () => {
				clicked();
				foreach (var listBox in listBoxes) {
					Application.InvokeOnNextUpdate(listBox.AdjustScrollPosition);
				}
			};
			return result;
		}

		private void CreatePanelControls()
		{
			panelList.LayoutCell = new LayoutCell {
				Alignment = Alignment.Center,
			};
			panelList.Changed += () => {
				RefreshUsedCommands();
				availableCommands.ScrollPosition = availableCommands.MinScrollPosition;
				usedCommands.ScrollPosition = usedCommands.MinScrollPosition;
				foreach (var item in panelList.Items.Cast<ListBox.ListBoxItem>()) {
					((PanelRow)item.Widget).StopEdit();
				}
			};
			foreach (var panel in toolbarLayout.GetAllPanels()) {
				var panelRow = new PanelRow(panel);
				var item = panelList.AddItem(panelRow);
				item.DoubleClicked += () => panelRow.StartEdit();
			}
			panelList.SelectedIndex = 0;
			AddNode(new Widget {
				Layout = new HBoxLayout { Spacing = 5 },
				LayoutCell = new LayoutCell { StretchY = 0 },
				Nodes = {
					new Widget {
						Layout = new VBoxLayout(),
						Nodes = {
							CreateButton("Preferences.AddPanel", "Add panel", () => AddPanel(isSeparator: false), panelList),
							CreateButton("Preferences.RemovePanel", "Remove panel", RemovePanel, panelList),
							CreateButton("Preferences.MoveUp", "Move panel up", () => MovePanel(-1), panelList),
							CreateButton("Preferences.MoveDown", "Move panel down", () => MovePanel(1), panelList),
							CreateButton("Preferences.AddRow", "Add row", () => AddPanel(isSeparator: true), panelList),
						}
					},
					panelList
				}
			});
		}

		private void AddPanel(bool isSeparator)
		{
			int index = panelList.SelectedIndex;
			var panel = new ToolbarLayout.ToolbarPanel(!isSeparator) {
				Title = isSeparator ? "Separator" : "Panel",
				IsSeparator = isSeparator
			};
			var panelRow = new PanelRow(panel);
			panelList.SelectedItem = panelList.InsertItem(index, panelRow);
			panelList.SelectedItem.DoubleClicked += () => panelRow.StartEdit();
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
			toolbarLayout.Rebuild(DockManager.Instance.ToolbarArea);
		}

		private void RemovePanel()
		{
			var panel = ((PanelRow)panelList.SelectedItem.Widget).Panel;
			if (panel == toolbarLayout.CreateToolbarPanel) {
				return;
			}
			int index = panelList.SelectedIndex;
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
			panelList.SelectedItem.Unlink();
			if (index > toolbarLayout.CreatePanelIndex) {
				index += 1;
			}
			panelList.SelectedIndex = index == panelList.Items.Count ? index - 1 : index;
			RefreshAvailableCommands();
			RefreshUsedCommands();
			toolbarLayout.Rebuild(DockManager.Instance.ToolbarArea);
		}

		private void RefreshAvailableCommands()
		{
			var panel = ((PanelRow)panelList.SelectedItem.Widget).Panel;
			availableCommands.Items.Clear();
			for (int i = 0; i < ToolbarCommandRegister.RegisteredCommands.Count; ++i) {
				var command = ToolbarCommandRegister.RegisteredCommands[i];
				if (toolbarLayout.ContainsIndex(i)) {
					continue;
				}
				availableCommands.AddItem(new CommandRow(command));
			}
		}

		private void RefreshUsedCommands()
		{
			var panel = ((PanelRow)panelList.SelectedItem.Widget).Panel;
			usedCommands.Items.Clear();
			if (!panel.Editable) {
				return;
			}
			foreach (var index in panel.CommandIndexes) {
				var command = ToolbarCommandRegister.RegisteredCommands[index];
				usedCommands.AddItem(new CommandRow(command));
			}
		}

		public void ResetToDefaults()
		{
			AppUserPreferences.Instance.ToolbarLayout = toolbarLayout = ToolbarLayout.DefaultToolbarLayout();
			Initialize();
			toolbarLayout.Rebuild(DockManager.Instance.ToolbarArea);
		}

		private Widget CreateCommandControls()
		{
			return new Widget {
				Layout = new VBoxLayout { Spacing = 5 },
				LayoutCell = new LayoutCell(Alignment.Center),
				Padding = new Thickness(5),
				Nodes = {
					CreateButton("Preferences.MoveUp", "Move command up", () => MoveCommand(-1), usedCommands),
					CreateButton("Preferences.MoveRight", "Add command", AddCommand, usedCommands, availableCommands),
					CreateButton("Preferences.MoveLeft", "Remove command", RemoveCommand, usedCommands, availableCommands),
					CreateButton("Preferences.MoveDown", "Move command down", () => MoveCommand(1), usedCommands)
				}
			};
		}

		private int GetSelectedCommandIndex(ListBox listBox)
		{
			var item = listBox.SelectedItem;
			if (item != null && item.Parent != null) {
				return item.Parent.Nodes.IndexOf(item);
			}
			return -1;
		}

		private void AddCommand()
		{
			var panel = ((PanelRow)panelList.SelectedItem.Widget).Panel;
			if (!panel.Editable) {
				return;
			}
			int leftPanelIndex = GetSelectedCommandIndex(availableCommands);
			if (leftPanelIndex < 0) {
				if (availableCommands.Items.Count == 0) {
					return;
				}
				leftPanelIndex = 0;
			}
			int rightPanelIndex = GetSelectedCommandIndex(usedCommands);
			rightPanelIndex = rightPanelIndex < 0 ? 0 : rightPanelIndex;
			var leftItem = (ListBox.ListBoxItem)availableCommands.Items[leftPanelIndex];
			var leftPanel = (CommandRow)leftItem.Widget;
			int commandIndex = ToolbarCommandRegister.RegisteredCommands.IndexOf(leftPanel.Command);
			availableCommands.Items.RemoveAt(leftPanelIndex);
			usedCommands.InsertItem(rightPanelIndex, new CommandRow(leftPanel.Command));
			panel.CommandIndexes.Insert(rightPanelIndex, commandIndex);
			toolbarLayout.Rebuild(DockManager.Instance.ToolbarArea);
		}

		private void RemoveCommand()
		{
			var panel = ((PanelRow)panelList.SelectedItem.Widget).Panel;
			if (!panel.Editable) {
				return;
			}
			int index = GetSelectedCommandIndex(usedCommands);
			if (index < 0) {
				if (usedCommands.Items.Count == 0) {
					return;
				}
				index = 0;
			}
			usedCommands.Items.RemoveAt(index);
			panel.CommandIndexes.RemoveAt(index);
			RefreshAvailableCommands();
			toolbarLayout.Rebuild(DockManager.Instance.ToolbarArea);
		}

		private void MoveCommand(int dir)
		{
			var index = GetSelectedCommandIndex(usedCommands);
			if (index < 0) {
				return;
			}
			int newIndex = index + dir;
			if (newIndex < 0 || newIndex >= usedCommands.Items.Count) {
				return;
			}
			var panel = ((PanelRow)panelList.SelectedItem.Widget).Panel;
			var tmp = panel.CommandIndexes[index];
			panel.CommandIndexes[index] = panel.CommandIndexes[newIndex];
			panel.CommandIndexes[newIndex] = tmp;
			var item1 = (ListBox.ListBoxItem)usedCommands.Items[index];
			var item2 = (ListBox.ListBoxItem)usedCommands.Items[newIndex];
			item1.Unlink();
			item2.Unlink();
			if (newIndex < index) {
				usedCommands.Items.Insert(newIndex, item1);
				usedCommands.Items.Insert(index, item2);
			} else {
				usedCommands.Items.Insert(index, item2);
				usedCommands.Items.Insert(newIndex, item1);
			}
			toolbarLayout.Rebuild(DockManager.Instance.ToolbarArea);
		}

		private void MovePanel(int dir)
		{
			int index = panelList.Items.IndexOf(panelList.SelectedItem);
			int newIndex = index + dir;
			if (newIndex < 0 || newIndex >= panelList.Items.Count) {
				return;
			}
			var panelRow1 = (PanelRow)((ListBox.ListBoxItem)panelList.Items[index]).Widget;
			var panelRow2 = (PanelRow)((ListBox.ListBoxItem)panelList.Items[newIndex]).Widget;
			var panel1 = panelRow1.Panel;
			var panel2 = panelRow2.Panel;
			panel1.Index = newIndex;
			panel2.Index = index;
			panelRow1.Panel = panel2;
			panelRow2.Panel = panel1;
			panelList.SelectedItem = (ListBox.ListBoxItem)panelList.Items[newIndex];
			toolbarLayout.SortPanels();
			toolbarLayout.Rebuild(DockManager.Instance.ToolbarArea);
		}

		private class PanelRow : Widget
		{
			private ToolbarLayout.ToolbarPanel panel;
			public ToolbarLayout.ToolbarPanel Panel
			{
				get => panel;
				set {
					panel = value;
					StopEdit();
					if (value.IsSeparator) {
						title.Unlink();
						dummy.Unlink();
					}
					RefreshTitle();
				}
			}
			private readonly ThemedSimpleText title = new ThemedSimpleText();
			private readonly ThemedEditBox editBox = new ThemedEditBox();
			private readonly Widget dummy = new Widget();

			public PanelRow(ToolbarLayout.ToolbarPanel panel)
			{
				title.Text = editBox.Text = panel.Title;
				editBox.Submitted += s => StopEdit();
				MinHeight = 26;
				Panel = panel;
				Layout = new StackLayout();
				Padding = new Thickness(5);
			}

			public void StartEdit()
			{
				if (title.Parent != null && !Panel.IsSeparator) {
					title.Unlink();
					dummy.Unlink();
					editBox.Text = Panel.Title;
					AddNode(editBox);
				}
			}

			public void StopEdit()
			{
				if (title.Parent == null && !Panel.IsSeparator) {
					Panel.Title = editBox.Text;
					RefreshTitle();
					editBox.Unlink();
					AddNode(title);
					AddNode(dummy);
				}
			}

			public void RefreshTitle()
			{
				title.Text = Panel.Title;
			}

			public override void Render()
			{
				base.Render();
				if (Panel.IsSeparator) {
					PrepareRendererState();
					Renderer.DrawLine(5, Height / 2, Width - 5, Height / 2, Color4.Gray);
				}
			}
		}

		private class CommandRow : Widget
		{
			public ICommand Command { get; private set; }

			public CommandRow(ICommand command)
			{
				Command = command;
				Layout = new HBoxLayout { Spacing = 10 };
				Padding = new Thickness(5);
				AddNode(new Image {
					Texture = command.Icon,
					MinMaxSize = new Vector2(16),
				});
				AddNode(new ThemedSimpleText {
					Text = command.Text,
					LayoutCell = new LayoutCell(Alignment.Center),
					Padding = new Thickness { Left = 5 }
				});
				AddNode(new Widget());
			}
		}

		private class ListBox : ThemedScrollView
		{
			private ListBoxItem selectedItem = null;
			public ListBoxItem SelectedItem
			{
				get => selectedItem;
				set {
					if (value.parent != this) {
						throw new ArgumentException();
					}
					selectedItem = value;
				}
			}

			public void AdjustScrollPosition()
			{
				if (Items.Count == 0) {
					ScrollPosition = MinScrollPosition;
					return;
				}
				Widget widget = (Widget)Items.Last();
				if (SelectedItem != null && SelectedItem.Parent != null) {
					widget = SelectedItem;
				}
				var pos = widget.CalcPositionInSpaceOf(Content);
				if (pos.Y < ScrollPosition) {
					ScrollPosition = pos.Y;
				} else if (pos.Y + widget.Height > ScrollPosition + Height) {
					ScrollPosition = pos.Y - Height + widget.Height;
				}
			}

			public int SelectedIndex
			{
				get => Items.IndexOf(selectedItem);
				set => selectedItem = (ListBoxItem)Items[value];
			}

			public NodeList Items => Content.Nodes;

			public event Action Changed;

			public ListBox()
			{
				Content.Layout = new VBoxLayout { Spacing = 4 };
			}

			public ListBoxItem AddItem(Widget widget)
			{
				var item = new ListBoxItem(widget, this);
				Items.Add(item);
				return item;
			}

			public ListBoxItem InsertItem(int index, Widget widget)
			{
				var item = new ListBoxItem(widget, this);
				Items.Insert(index, item);
				return item;
			}

			public class ListBoxItem : Widget
			{
				internal readonly ListBox parent;
				public Widget Widget { get; private set; }

				public event Action DoubleClicked;

				public ListBoxItem(Widget widget, ListBox parent)
				{
					this.parent = parent;
					Widget = widget;
					Layout = new HBoxLayout();
					HitTestTarget = true;
					AddNode(widget);
					Clicked += () => {
						parent.SelectedItem = this;
						parent.Changed?.Invoke();
					};
					Gestures.Add(new DoubleClickGesture(() => DoubleClicked?.Invoke()));
				}

				public override void Render()
				{
					base.Render();
					if (parent.SelectedItem == this) {
						PrepareRendererState();
						Renderer.DrawRect(0, 0, Width, Height, ColorTheme.Current.Toolbar.ButtonCheckedBackground);
					}
				}
			}
		}
	}
}
