using Lime;
using System;
using System.Linq;
using Tangerine.UI;

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
				usedCommands.ScrollPosition = usedCommands.MinScrollPosition;
				foreach (var item in panelList.Items.Cast<ListBox.ListBoxItem>()) {
					if (item.Widget is PanelRow row) {
						row.StopEdit();
					}
				}
			};
			foreach (var row in toolbarLayout.Rows) {
				panelList.AddItem(new ToolbarRowRow(row));
				foreach (var panel in row.Panels) {
					var panelRow = new PanelRow(panel);
					var item = panelList.AddItem(panelRow);
					item.DoubleClicked += () => panelRow.StartEdit();
				}
			}
			if (panelList.Items.Count > 0) {
				panelList.Items.RemoveAt(0);
			}
			panelList.SelectedIndex = 0;
			AddNode(new Widget {
				Layout = new HBoxLayout { Spacing = 5 },
				LayoutCell = new LayoutCell { StretchY = 0 },
				Nodes = {
					new Widget {
						Layout = new VBoxLayout(),
						Nodes = {
							CreateButton("Preferences.AddPanel", "Add panel", AddPanel, panelList),
							CreateButton("Preferences.RemovePanel", "Remove panel", RemovePanelOrRow, panelList),
							CreateButton("Preferences.MoveUp", "Move panel up", () => MovePanel(-1), panelList),
							CreateButton("Preferences.MoveDown", "Move panel down", () => MovePanel(1), panelList),
							CreateButton("Preferences.AddRow", "Add row", AddRow, panelList),
						}
					},
					panelList
				}
			});
		}

		private ToolbarLayout.ToolbarPanel GetSelectedPanel()
		{
			var item = panelList.SelectedItem;
			if (panelList.SelectedItem == null) {
				return null;
			}
			if (item.Widget is PanelRow row) {
				return row.Panel;
			}
			return null;
		}

		private ToolbarLayout.ToolbarRow GetSelectedRow()
		{
			var item = panelList.SelectedItem;
			if (panelList.SelectedItem == null) {
				return null;
			}
			if (item.Widget is ToolbarRowRow row) {
				return row.Row;
			}
			return null;
		}

		private void RemovePanelOrRow()
		{
			var item = panelList.SelectedItem;
			if (item == null) {
				return;
			}
			if (item.Widget is ToolbarRowRow) {
				RemoveRow();
			} else {
				RemovePanel();
			}
		}

		private void AddPanel()
		{
			var panel = GetSelectedPanel();
			var row = panel?.Parent;
			if (row == null) {
				row = toolbarLayout.Rows[0];
			}
			var newPanel = new ToolbarLayout.ToolbarPanel() {
				Title = "Panel",
				Index = panel?.Index ?? 0
			};
			var panelRow = new PanelRow(newPanel);
			int index = panelList.SelectedIndex;
			index = index < 0 ? 0 : index;
			panelList.SelectedItem = panelList.InsertItem(index, panelRow);
			panelList.SelectedItem.DoubleClicked += () => panelRow.StartEdit();
			toolbarLayout.InsertPanel(row, newPanel, newPanel.Index);
			RefreshUsedCommands();
			TangerineApp.Instance.Toolbars["Toolbar"].Rebuild();
		}

		private void RemovePanel()
		{
			var panel = GetSelectedPanel();
			int index = panelList.SelectedIndex;
			toolbarLayout.RemovePanel(panel);
			panelList.SelectedItem.Unlink();
			index = index == panelList.Items.Count ? index - 1 : index;
			while (index >= 0 && ((ListBox.ListBoxItem)panelList.Items[index]).Widget is ToolbarRowRow) {
				index -= 1;
			}
			if (index < 0) {
				panelList.SelectedItem = null;
			} else {
				panelList.SelectedIndex = index;
			}
			RefreshAvailableCommands();
			RefreshUsedCommands();
			TangerineApp.Instance.Toolbars["Toolbar"].Rebuild();
		}

		private void MovePanel(int dir)
		{
			int index = panelList.SelectedIndex;
			int newIndex = index + dir;
			var item1 = panelList.SelectedItem;
			if (newIndex < 0 || newIndex >= panelList.Items.Count || !(item1.Widget is PanelRow)) {
				return;
			}
			var panelRow1 = (PanelRow)item1.Widget;
			var panel1 = panelRow1.Panel;
			var item2 = (ListBox.ListBoxItem)panelList.Items[newIndex];
			if (item2.Widget is ToolbarRowRow) {
				var row1 = panel1.Parent;
				var row2 = toolbarLayout.Rows[row1.Index + dir];
				toolbarLayout.RemovePanel(panel1);
				item1.Unlink();
				panelList.Items.Insert(newIndex, item1);
				toolbarLayout.InsertPanel(row2, panel1, dir > 0 ? 0 : row2.Panels.Count);
			} else {
				panelList.SelectedItem = (ListBox.ListBoxItem)panelList.Items[newIndex];
				var panelRow2 = (PanelRow)panelList.SelectedItem.Widget;
				var panel2 = panelRow2.Panel;
				toolbarLayout.SwapPanels(panel1, panel2);
				panelRow1.Panel = panel2;
				panelRow2.Panel = panel1;
			}
			TangerineApp.Instance.Toolbars["Toolbar"].Rebuild();
		}

		private void RemoveRow()
		{
			var row = GetSelectedRow();
			if (row == null) {
				return;
			}
			toolbarLayout.RemoveRow(row);
			var index = panelList.SelectedIndex;
			panelList.Items.RemoveAt(index);
			if (index < panelList.Items.Count) {
				panelList.SelectedIndex = index;
			} else {
				panelList.SelectedItem = null;
			}
			TangerineApp.Instance.Toolbars["Toolbar"].Rebuild();
		}

		private void AddRow()
		{
			var row = new ToolbarLayout.ToolbarRow();
			int rowIndex = 0;
			var panel = GetSelectedPanel();
			if (panel != null) {
				var oldRow = panel.Parent;
				int savedIndex = panel.Index;
				for (int i = panel.Index; i < oldRow.Panels.Count; ++i) {
					toolbarLayout.InsertPanel(row, oldRow.Panels[i], i - savedIndex);
				}
				oldRow.Panels.RemoveRange(savedIndex, oldRow.Panels.Count - savedIndex);
				rowIndex = oldRow.Index + 1;
			} else {
				int i = panelList.SelectedIndex;
				ListBox.ListBoxItem item = null;
				while (i >= 0 && (item = (ListBox.ListBoxItem)panelList.Items[i]).Widget is ToolbarRowRow) {
					i -= 1;
				}
				if (i >= 0) {
					rowIndex = ((PanelRow)item.Widget).Panel.Parent.Index + panelList.SelectedIndex - i + 1;
				}
			}
			var index = panelList.SelectedIndex;
			if (index < 0) {
				index = 0;
			}
			panelList.InsertItem(index, new ToolbarRowRow(row));
			toolbarLayout.InsertRow(row, rowIndex);
			TangerineApp.Instance.Toolbars["Toolbar"].Rebuild();
		}

		private void RefreshAvailableCommands()
		{
			availableCommands.Items.Clear();
			foreach (var pair in CommandRegister.RegisteredPairs()) {
				if (toolbarLayout.ContainsId(pair.Key)) {
					continue;
				}
				availableCommands.AddItem(new CommandRow(pair.Value, pair.Key));
			}
		}

		private void RefreshUsedCommands()
		{
			var panel = GetSelectedPanel();
			usedCommands.Items.Clear();
			if (panel == null) {
				return;
			}
			foreach (var id in panel.CommandIds) {
				usedCommands.AddItem(new CommandRow(CommandRegister.GetCommand(id), id));
			}
		}

		public void ResetToDefaults()
		{
			AppUserPreferences.Instance.ToolbarLayout = toolbarLayout = ToolbarLayout.DefaultToolbarLayout();
			Initialize();
			TangerineApp.Instance.Toolbars["Toolbar"].Rebuild(toolbarLayout);
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

		private void AddCommand()
		{
			var panel = ((PanelRow)panelList.SelectedItem.Widget).Panel;
			int leftPanelIndex = availableCommands.SelectedIndex;
			if (leftPanelIndex < 0) {
				if (availableCommands.Items.Count == 0) {
					return;
				}
				leftPanelIndex = 0;
			}
			int rightPanelIndex = usedCommands.SelectedIndex;
			rightPanelIndex = rightPanelIndex < 0 ? 0 : rightPanelIndex;
			var leftItem = availableCommands.SelectedItem;
			var leftPanel = (CommandRow)leftItem.Widget;
			leftPanel.Unlink();
			availableCommands.Items.RemoveAt(leftPanelIndex);
			usedCommands.InsertItem(rightPanelIndex, leftPanel);
			panel.CommandIds.Insert(rightPanelIndex, leftPanel.CommandId);
			TangerineApp.Instance.Toolbars["Toolbar"].Rebuild();
		}

		private void RemoveCommand()
		{
			var panel = ((PanelRow)panelList.SelectedItem.Widget).Panel;
			int index = usedCommands.SelectedIndex;
			if (index < 0) {
				if (usedCommands.Items.Count == 0) {
					return;
				}
				index = 0;
			}
			usedCommands.Items.RemoveAt(index);
			panel.CommandIds.RemoveAt(index);
			RefreshAvailableCommands();
			TangerineApp.Instance.Toolbars["Toolbar"].Rebuild();
		}

		private void MoveCommand(int dir)
		{
			var index = usedCommands.SelectedIndex;
			if (index < 0) {
				return;
			}
			int newIndex = index + dir;
			if (newIndex < 0 || newIndex >= usedCommands.Items.Count) {
				return;
			}
			var panel = ((PanelRow)panelList.SelectedItem.Widget).Panel;
			var tmp = panel.CommandIds[index];
			panel.CommandIds[index] = panel.CommandIds[newIndex];
			panel.CommandIds[newIndex] = tmp;
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
			TangerineApp.Instance.Toolbars["Toolbar"].Rebuild();
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
				AddNode(title);
				AddNode(dummy);
				Layout = new StackLayout();
				Padding = new Thickness(5);
			}

			public void StartEdit()
			{
				if (title.Parent != null) {
					title.Unlink();
					dummy.Unlink();
					editBox.Text = Panel.Title;
					AddNode(editBox);
				}
			}

			public void StopEdit()
			{
				if (editBox.Parent != null) {
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
		}

		private class CommandRow : Widget
		{
			public ICommand Command { get; private set; }
			public string CommandId { get; private set; }

			public CommandRow(ICommand command, string id)
			{
				Command = command;
				CommandId = id;
				Layout = new HBoxLayout { Spacing = 10 };
				Padding = new Thickness(5);
				if (command.Icon != null) {
					AddNode(new Image {
						Texture = command.Icon,
						MinMaxSize = new Vector2(16),
					});
				} else {
					AddNode(new HSpacer(16));
				}
				AddNode(new ThemedSimpleText {
					Text = command.Text ?? id,
					LayoutCell = new LayoutCell(Alignment.Center),
					Padding = new Thickness { Left = 5 }
				});
				AddNode(new Widget());
			}
		}

		private class ToolbarRowRow : Widget
		{
			public ToolbarLayout.ToolbarRow Row { get; private set; }

			public ToolbarRowRow(ToolbarLayout.ToolbarRow row)
			{
				Row = row;
				MinMaxHeight = 10;
			}

			public override void Render()
			{
				base.Render();
				PrepareRendererState();
				Renderer.DrawLine(5, Height / 2, Width - 5, Height / 2, Color4.Gray);
			}
		}

		private class ListBox : ThemedScrollView
		{
			private ListBoxItem selectedItem = null;
			public ListBoxItem SelectedItem
			{
				get {
					if (selectedItem?.parent != this) {
						selectedItem = null;
					}
					return selectedItem;
				}
				set {
					if (value != null && value.parent != this) {
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
