using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.Panels
{
	public class BackupHistoryPanel : IDocumentView
	{
		private static BackupManager.Backup currentBackup;

		private readonly Widget panelWidget;
		private readonly Frame rootWidget;
		private readonly Widget resultPane;
		private readonly ThemedScrollView scrollView;
		private readonly int rowHeight = Theme.Metrics.TextHeight;
		private List<BackupManager.Backup> history = new List<BackupManager.Backup>();
		private int selectedIndex;

		private class Cmds
		{
			public static readonly ICommand Up = new Command(Key.Up);
			public static readonly ICommand Down = new Command(Key.Down);
			public static readonly ICommand Cancel = new Command(Key.Escape);
			public static readonly ICommand Enter = new Command(Key.Enter);
		}

		public BackupHistoryPanel(Widget panelWidget)
		{
			this.panelWidget = panelWidget;
			scrollView = new ThemedScrollView { TabTravesable = new TabTraversable() };
			this.rootWidget = new Frame {
				Id = "BackupHistoryPanel",
				Padding = new Thickness(4),
				Layout = new VBoxLayout { Spacing = 4 },
				Nodes = { scrollView }
			};
			resultPane = scrollView.Content;
			resultPane.CompoundPresenter.Insert(0, new SyncDelegatePresenter<Widget>(w => {
				w.PrepareRendererState();
				if (selectedIndex == 0) {
					if (history?.Count > 0) {
						Renderer.DrawRect(
							0, rowHeight * selectedIndex,
							w.Width, (selectedIndex + 1) * rowHeight,
							Theme.Colors.SelectedBackground);
					}
				}

				if (history?.Count > 0) {
					Renderer.DrawRect(
						0, rowHeight * selectedIndex,
						w.Width, (selectedIndex + 1) * rowHeight,
						Theme.Colors.SelectedBackground);
				}
			}));
			scrollView.LateTasks.Add(new KeyRepeatHandler(ScrollView_KeyRepeated));
		}

		private void ScrollView_KeyRepeated(WidgetInput input, Key key)
		{
			if (history == null || history.Count < 1) {
				return;
			}

			if (key == Key.Mouse0) {
				scrollView.SetFocus();
				selectedIndex = (resultPane.LocalMousePosition().Y / rowHeight).Floor().Clamp(0, history.Count - 1);
			} else if (Cmds.Down.WasIssued()) {
				selectedIndex++;
				Cmds.Down.Consume();
			} else if (Cmds.Up.WasIssued()) {
				selectedIndex--;
				Cmds.Up.Consume();
			} else if (Cmds.Cancel.WasIssued()) {
				scrollView.RevokeFocus();
				Cmds.Cancel.Consume();
			} else if (Cmds.Enter.WasIssued() || key == Key.Mouse0DoubleClick) {
				Document.Current.History.DoTransaction(() => NavigateToItem(selectedIndex));
				Cmds.Enter.Consume();
			} else {
				return;
			}

			selectedIndex = selectedIndex.Clamp(0, history?.Count - 1 ?? 0);
			EnsureRowVisible(selectedIndex);
			Window.Current.Invalidate();
		}

		private void NavigateToItem(int index)
		{
			currentBackup = history[history.Count - 1 - index];
			BackupManager.Instance.SelectBackup(history[history.Count - 1 - index]);
		}

		private void EnsureRowVisible(int row)
		{
			while ((row + 1) * rowHeight > scrollView.ScrollPosition + scrollView.Height) {
				scrollView.ScrollPosition++;
			}

			while (row * rowHeight < scrollView.ScrollPosition) {
				scrollView.ScrollPosition--;
			}
		}

		private void RefreshHistory()
		{
			selectedIndex = 0;
			resultPane.Nodes.Clear();
			history?.Clear();
			history = BackupManager.Instance.GetHistory(Document.Current);
			if (history == null || history.Count < 1) {
				return;
			}

			resultPane.Layout = new TableLayout {
				ColumnCount = 1,
				ColumnSpacing = 8,
				RowCount = history.Count,
				ColumnDefaults = new List<DefaultLayoutCell> { new DefaultLayoutCell { StretchY = 0 } }
			};

			for (int i = history.Count - 1; i >= 0; i--) {
				resultPane.Nodes.Add(new ThemedSimpleText(history[i].DateTime.ToString() + (history[i].IsActual ? "(Latest)" : "")));
				if (currentBackup != null && currentBackup.DateTime.Equals(history[i].DateTime)) {
					selectedIndex = history.Count - i - 1;
				}
			}

			scrollView.ScrollPosition = scrollView.MinScrollPosition;
		}

		public void Attach()
		{
			BackupManager.Instance.BackupSaved += RefreshHistory;
			panelWidget.PushNode(rootWidget);
			RefreshHistory();
		}

		public void Detach()
		{
			BackupManager.Instance.BackupSaved -= RefreshHistory;
			rootWidget.Unlink();
		}
	}
}
