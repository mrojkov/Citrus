using Lime;
using System;
using System.Linq;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine.Dialogs
{
	public class ToolbarModelEditor : Widget
	{
		private ToolbarModel toolbarModel = AppUserPreferences.Instance.ToolbarModel;
		private ListBox availableCommands;
		private ListBox usedCommands;
		private ListBox panelList;
		private ThemedDropDownList categoryList;
		private EditBox filterEditBox;

		public ToolbarModelEditor()
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
			AddNode(new Widget {
				Layout = new HBoxLayout { Spacing = 10 },
				Nodes = {
					CreateCategoryList(),
					CreateSearchBox()
				}
			});
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

		private Widget CreateSearchBox()
		{
			filterEditBox = new ThemedEditBox();
			filterEditBox.AddChangeWatcher(() => filterEditBox.Text, _ => RefreshAvailableCommands());
			return new Widget {
				Layout = new HBoxLayout(),
				LayoutCell = new LayoutCell { StretchX = 2 },
				Nodes = {
					new ThemedSimpleText("Search: ") {
						LayoutCell = new LayoutCell(Alignment.LeftCenter)
					},
					filterEditBox
				}
			};
		}

		private static ListBox CreateListBox()
		{
			var result = new ListBox();
			result.Content.Padding = new Thickness(6);
			result.CompoundPostPresenter.Add(new SyncDelegatePresenter<Widget>(w => {
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

		private static ToolbarButton CreateButton(string textureId, string tooltip, Action clicked, params ListBox[] listBoxes)
		{
			var result = new ToolbarButton {
				Texture = IconPool.GetTexture(textureId),
				Tooltip = tooltip
			};
			result.Clicked += () => {
				clicked();
				foreach (var listBox in listBoxes) {
					Application.InvokeOnNextUpdate(listBox.AdjustScrollPosition);
				}
			};
			return result;
		}

		private ThemedDropDownList CreateCategoryList()
		{
			categoryList = new ThemedDropDownList();
			categoryList.Items.Add(new CommonDropDownList.Item("All", CommandRegistry.AllCommands));
			foreach (var categoryInfo in CommandRegistry.RegisteredCategories()) {
				categoryList.Items.Add(new CommonDropDownList.Item(categoryInfo.Title, categoryInfo));
			}
			categoryList.Index = 0;
			categoryList.Changed += e => RefreshAvailableCommands();
			return categoryList;
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
					if (item.Widget is PanelItem row) {
						row.StopEdit();
					}
				}
			};
			foreach (var row in toolbarModel.Rows) {
				panelList.AddItem(new RowItem(row));
				foreach (var panel in row.Panels) {
					var panelRow = new PanelItem(panel);
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

		private ToolbarModel.ToolbarPanel GetSelectedPanel()
		{
			var item = panelList.SelectedItem;
			if (panelList.SelectedItem == null) {
				return null;
			}
			if (item.Widget is PanelItem row) {
				return row.Panel;
			}
			return null;
		}

		private ToolbarModel.ToolbarRow GetSelectedRow()
		{
			var item = panelList.SelectedItem;
			if (panelList.SelectedItem == null) {
				return null;
			}
			if (item.Widget is RowItem row) {
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
			if (item.Widget is RowItem) {
				RemoveRow();
			} else {
				RemovePanel();
			}
		}

		private void AddPanel()
		{
			var panel = GetSelectedPanel();
			var row = panel?.Parent ?? toolbarModel.Rows[0];
			var newPanel = new ToolbarModel.ToolbarPanel() {
				Title = "Panel",
				Index = panel?.Index ?? 0
			};
			var panelRow = new PanelItem(newPanel);
			var index = panelList.SelectedIndex;
			index = index < 0 ? 0 : index;
			panelList.SelectedItem = panelList.InsertItem(index, panelRow);
			panelList.SelectedItem.DoubleClicked += () => panelRow.StartEdit();
			toolbarModel.InsertPanel(row, newPanel, newPanel.Index);
			RefreshUsedCommands();
			TangerineApp.Instance.Toolbar.Rebuild();
		}

		private void RemovePanel()
		{
			var panel = GetSelectedPanel();
			var index = panelList.SelectedIndex;
			toolbarModel.RemovePanel(panel);
			panelList.SelectedItem.Unlink();
			index = index == panelList.Items.Count ? index - 1 : index;
			while (index >= 0 && ((ListBox.ListBoxItem)panelList.Items[index]).Widget is RowItem) {
				index -= 1;
			}
			if (index < 0) {
				panelList.SelectedItem = null;
			} else {
				panelList.SelectedIndex = index;
			}
			RefreshAvailableCommands();
			RefreshUsedCommands();
			TangerineApp.Instance.Toolbar.Rebuild();
		}

		private void MovePanel(int dir)
		{
			var index = panelList.SelectedIndex;
			var newIndex = index + dir;
			var item1 = panelList.SelectedItem;
			if (newIndex < 0 || newIndex >= panelList.Items.Count || !(item1.Widget is PanelItem)) {
				return;
			}
			var panelRow1 = (PanelItem)item1.Widget;
			var panel1 = panelRow1.Panel;
			var item2 = (ListBox.ListBoxItem)panelList.Items[newIndex];
			if (item2.Widget is RowItem) {
				var row1 = panel1.Parent;
				var row2 = toolbarModel.Rows[row1.Index + dir];
				toolbarModel.RemovePanel(panel1);
				item1.Unlink();
				panelList.Items.Insert(newIndex, item1);
				toolbarModel.InsertPanel(row2, panel1, dir > 0 ? 0 : row2.Panels.Count);
			} else {
				panelList.SelectedItem = (ListBox.ListBoxItem)panelList.Items[newIndex];
				var panelRow2 = (PanelItem)panelList.SelectedItem.Widget;
				var panel2 = panelRow2.Panel;
				toolbarModel.SwapPanels(panel1, panel2);
				panelRow1.Panel = panel2;
				panelRow2.Panel = panel1;
			}
			TangerineApp.Instance.Toolbar.Rebuild();
		}

		private void RemoveRow()
		{
			var row = GetSelectedRow();
			if (row == null) {
				return;
			}
			toolbarModel.RemoveRow(row);
			var index = panelList.SelectedIndex;
			panelList.Items.RemoveAt(index);
			if (index < panelList.Items.Count) {
				panelList.SelectedIndex = index;
			} else {
				panelList.SelectedItem = null;
			}
			TangerineApp.Instance.Toolbar.Rebuild();
		}

		private void AddRow()
		{
			var row = new ToolbarModel.ToolbarRow();
			var rowIndex = 0;
			var panel = GetSelectedPanel();
			if (panel != null) {
				var oldRow = panel.Parent;
				var savedIndex = panel.Index;
				for (var i = panel.Index; i < oldRow.Panels.Count; ++i) {
					toolbarModel.InsertPanel(row, oldRow.Panels[i], i - savedIndex);
				}
				oldRow.Panels.RemoveRange(savedIndex, oldRow.Panels.Count - savedIndex);
				rowIndex = oldRow.Index + 1;
			} else {
				var i = panelList.SelectedIndex;
				var item = (ListBox.ListBoxItem)panelList.Items[i];
				while (i >= 0 && item.Widget is RowItem) {
					i -= 1;
					item = (ListBox.ListBoxItem)panelList.Items[i];
				}
				if (i >= 0) {
					rowIndex = ((PanelItem)item.Widget).Panel.Parent.Index + panelList.SelectedIndex - i + 1;
				}
			}
			var index = panelList.SelectedIndex;
			if (index < 0) {
				index = 0;
			}
			panelList.InsertItem(index, new RowItem(row));
			toolbarModel.InsertRow(row, rowIndex);
			TangerineApp.Instance.Toolbar.Rebuild();
		}

		private void RefreshAvailableCommands()
		{
			availableCommands.Items.Clear();
			var filter = filterEditBox.Text.ToLower();
			var categoryInfo = (CommandCategoryInfo)categoryList.Items[categoryList.Index].Value;
			foreach (var commandInfo in CommandRegistry.RegisteredCommandInfo(categoryInfo)) {
				if (toolbarModel.ContainsId(commandInfo.Id) || !commandInfo.Title.ToLower().Contains(filter)) {
					continue;
				}
				availableCommands.AddItem(new CommandItem(commandInfo));
			}
			availableCommands.ScrollPosition = availableCommands.MinScrollPosition;
			availableCommands.Items.Sort((node1, node2) => {
				var string1 = ((CommandItem)((ListBox.ListBoxItem)node1).Widget).CommandTitle;
				var string2 = ((CommandItem)((ListBox.ListBoxItem)node2).Widget).CommandTitle;
				return string.Compare(string1, string2);
			});
		}

		private void RefreshUsedCommands()
		{
			var panel = GetSelectedPanel();
			usedCommands.Items.Clear();
			if (panel == null) {
				return;
			}
			foreach (var id in panel.CommandIds) {
				if (CommandRegistry.TryGetCommandInfo(id, out CommandInfo commandInfo)) {
					usedCommands.AddItem(new CommandItem(commandInfo));
				}
			}
		}

		public void ResetToDefaults()
		{
			AppUserPreferences.Instance.ToolbarModel = toolbarModel = AppUserPreferences.DefaultToolbarModel();
			Initialize();
			TangerineApp.Instance.Toolbar.Rebuild(toolbarModel);
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
			var panel = ((PanelItem)panelList.SelectedItem.Widget).Panel;
			var leftPanelIndex = availableCommands.SelectedIndex;
			if (leftPanelIndex < 0) {
				if (availableCommands.Items.Count == 0) {
					return;
				}
				leftPanelIndex = 0;
			}
			var rightPanelIndex = usedCommands.SelectedIndex;
			rightPanelIndex = rightPanelIndex < 0 ? 0 : rightPanelIndex;
			var leftItem = (ListBox.ListBoxItem)availableCommands.Items[leftPanelIndex];
			var leftPanel = (CommandItem)leftItem.Widget;
			leftPanel.Unlink();
			availableCommands.Items.RemoveAt(leftPanelIndex);
			usedCommands.InsertItem(rightPanelIndex, leftPanel);
			panel.CommandIds.Insert(rightPanelIndex, leftPanel.CommandId);
			TangerineApp.Instance.Toolbar.Rebuild();
		}

		private void RemoveCommand()
		{
			var panel = ((PanelItem)panelList.SelectedItem.Widget).Panel;
			var index = usedCommands.SelectedIndex;
			if (index < 0) {
				if (usedCommands.Items.Count == 0) {
					return;
				}
				index = 0;
			}
			usedCommands.Items.RemoveAt(index);
			panel.CommandIds.RemoveAt(index);
			RefreshAvailableCommands();
			TangerineApp.Instance.Toolbar.Rebuild();
		}

		private void MoveCommand(int dir)
		{
			var index = usedCommands.SelectedIndex;
			if (index < 0) {
				return;
			}
			var newIndex = index + dir;
			if (newIndex < 0 || newIndex >= usedCommands.Items.Count) {
				return;
			}
			var panel = ((PanelItem)panelList.SelectedItem.Widget).Panel;
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
			TangerineApp.Instance.Toolbar.Rebuild();
		}

		private class PanelItem : Widget
		{
			private ToolbarModel.ToolbarPanel panel;
			public ToolbarModel.ToolbarPanel Panel
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

			public PanelItem(ToolbarModel.ToolbarPanel panel)
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
				if (title.Parent == null) {
					return;
				}
				title.Unlink();
				dummy.Unlink();
				editBox.Text = Panel.Title;
				AddNode(editBox);
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

		private class CommandItem : Widget
		{
			public CommandInfo CommandInfo { get; private set; }
			public ICommand Command => CommandInfo.Command;
			public string CommandId => CommandInfo.Id;
			public string CommandTitle => CommandInfo.Title;

			public CommandItem(CommandInfo commandInfo)
			{
				CommandInfo = commandInfo;
				Layout = new HBoxLayout { Spacing = 10 };
				Padding = new Thickness(5);
				if (Command.Icon != null) {
					AddNode(new Image {
						Texture = Command.Icon.AsTexture,
						MinMaxSize = new Vector2(16),
					});
				} else {
					AddNode(Spacer.HSpacer(16));
				}
				AddNode(new ThemedSimpleText {
					Text = commandInfo.Title,
					LayoutCell = new LayoutCell(Alignment.Center),
					Padding = new Thickness { Left = 5 }
				});
				AddNode(new Widget());
			}
		}

		private class RowItem : Widget
		{
			public ToolbarModel.ToolbarRow Row { get; private set; }

			public RowItem(ToolbarModel.ToolbarRow row)
			{
				Row = row;
				MinMaxHeight = 10;
			}

			protected override Lime.RenderObject GetRenderObject()
			{
				var ro = RenderObjectPool<RenderObject>.Acquire();
				ro.CaptureRenderState(this);
				ro.Size = Size;
				return ro;
			}

			private class RenderObject : WidgetRenderObject
			{
				public Vector2 Size;

				public override void Render()
				{
					PrepareRenderState();
					Renderer.DrawLine(5, Size.Y / 2, Size.X - 5, Size.Y / 2, Color4.Gray);
				}
			}
		}

		private class ListBox : ThemedScrollView
		{
			private ListBoxItem selectedItem = null;
			public ListBoxItem SelectedItem
			{
				get {
					if (selectedItem?.Parent == null) {
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
				var widget = SelectedItem?.AsWidget;
				if (widget == null) {
					widget = Items.Last().AsWidget;
				}
				var pos = widget.CalcPositionInSpaceOf(Content);
				if (pos.Y < ScrollPosition) {
					ScrollPosition = pos.Y;
				} else if (pos.Y + widget.Height > ScrollPosition + Height && SelectedItem != null) {
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

				protected override Lime.RenderObject GetRenderObject()
				{
					if (parent.SelectedItem != this) {
						return null;
					}
					var ro = RenderObjectPool<RenderObject>.Acquire();
					ro.CaptureRenderState(this);
					ro.Size = Size;
					ro.Color = ColorTheme.Current.Toolbar.ButtonCheckedBackground;
					return ro;
				}

				private class RenderObject : WidgetRenderObject
				{
					public Vector2 Size;
					public Color4 Color;

					public override void Render()
					{
						PrepareRenderState();
						Renderer.DrawRect(0, 0, Size.X, Size.Y, Color);
					}
				}
			}
		}
	}
}
