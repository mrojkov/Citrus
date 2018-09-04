using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.BackupHistoryPanel
{
	public class BackupHistoryPanel : IDocumentView
	{
		public readonly Widget PanelWidget;
		public readonly Frame RootWidget;
		readonly Widget resultPane;
		readonly ThemedScrollView scrollView;

		private int SelectedIndex { get; set; }
		private static BackupManager.Backup currenttBackup;

		private readonly int rowHeight = Theme.Metrics.TextHeight;
		private List<BackupManager.Backup> history = new List<BackupManager.Backup>();

		class Cmds
		{
			public static readonly ICommand Up = new Command(Key.Up);
			public static readonly ICommand Down = new Command(Key.Down);
			public static readonly ICommand Cancel = new Command(Key.Escape);
			public static readonly ICommand Enter = new Command(Key.Enter);
		}

		public BackupHistoryPanel(Widget rootWidget)
		{
			PanelWidget = rootWidget;
			scrollView = new ThemedScrollView();
			RootWidget = new Frame {
				Id = "BackupHistoryPanel",
				Padding = new Thickness(4),
				Layout = new VBoxLayout {Spacing = 4},
				Nodes = {scrollView}
			};
			resultPane = scrollView.Content;
			scrollView.TabTravesable = new TabTraversable();
			resultPane.CompoundPresenter.Insert(0, new SyncDelegatePresenter<Widget>(w => {
				w.PrepareRendererState();
				if (SelectedIndex == 0) {
					if (history?.Count > 0) {
						Renderer.DrawRect(
							0, rowHeight * SelectedIndex,
							w.Width, (SelectedIndex + 1) * rowHeight,
							Theme.Colors.SelectedBackground);
					}
				}

				if (history?.Count > 0) {
					Renderer.DrawRect(
						0, rowHeight * SelectedIndex,
						w.Width, (SelectedIndex + 1) * rowHeight,
						Theme.Colors.SelectedBackground);
				}
			}));
			scrollView.LateTasks.Add(new KeyRepeatHandler(ScrollView_KeyRepeated));
		}

		void ScrollView_KeyRepeated(WidgetInput input, Key key)
		{
			if (history == null || history.Count < 1) {
				return;
			}

			if (key == Key.Mouse0) {
				scrollView.SetFocus();
				SelectedIndex = (resultPane.LocalMousePosition().Y / rowHeight).Floor().Clamp(0, history.Count - 1);
			} else if (Cmds.Down.WasIssued()) {
				SelectedIndex++;
				Cmds.Down.Consume();
			} else if (Cmds.Up.WasIssued()) {
				SelectedIndex--;
				Cmds.Up.Consume();
			} else if (Cmds.Cancel.WasIssued()) {
				scrollView.RevokeFocus();
				Cmds.Cancel.Consume();
			} else if (Cmds.Enter.WasIssued() || key == Key.Mouse0DoubleClick) {
				Document.Current.History.DoTransaction(() => NavigateToItem(SelectedIndex));
				Cmds.Enter.Consume();
			} else {
				return;
			}

			SelectedIndex = SelectedIndex.Clamp(0, history?.Count - 1 ?? 0);
			EnsureRowVisible(SelectedIndex);
			Window.Current.Invalidate();
		}

		void NavigateToItem(int index)
		{
			currenttBackup = history[history.Count - 1 - index];
			BackupManager.Instance.SelectBackup(history[history.Count - 1 - index]);
		}

		void EnsureRowVisible(int row)
		{
			while ((row + 1) * rowHeight > scrollView.ScrollPosition + scrollView.Height) {
				scrollView.ScrollPosition++;
			}

			while (row * rowHeight < scrollView.ScrollPosition) {
				scrollView.ScrollPosition--;
			}
		}

		void RefreshHistory()
		{
			SelectedIndex = 0;
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
				ColumnDefaults = new List<DefaultLayoutCell> {new DefaultLayoutCell { StretchY = 0}}
			};

			for (int i = history.Count - 1; i >= 0; i--) {
				resultPane.Nodes.Add(new ThemedSimpleText(history[i].DateTime.ToString() + (history[i].IsActual ? "(Latest)" : "")));
				if (currenttBackup != null && currenttBackup.DateTime.Equals(history[i].DateTime)) {
					SelectedIndex = history.Count - i - 1;
				}
			}

			scrollView.ScrollPosition = scrollView.MinScrollPosition;
		}

		public void Attach()
		{
			BackupManager.Instance.BackupSaved += RefreshHistory;
			PanelWidget.PushNode(RootWidget);
			RefreshHistory();
		}

		public void Detach()
		{
			BackupManager.Instance.BackupSaved -= RefreshHistory;
			RootWidget.Unlink();
		}
	}
}
