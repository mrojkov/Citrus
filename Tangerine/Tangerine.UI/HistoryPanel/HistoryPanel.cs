using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.HistoryPanel
{
	public class HistoryPanel : IDocumentView
	{
		public readonly Widget PanelWidget;
		public readonly Frame RootWidget;
		readonly Widget resultPane;
		readonly ThemedScrollView scrollView;
		private int selectedIndex;
		private readonly int rowHeight = Theme.Metrics.TextHeight;
		private List<BackupsManager.Backup> history = new List<BackupsManager.Backup>();

		class Cmds
		{
			public static readonly ICommand Up = new Command(Key.Up);
			public static readonly ICommand Down = new Command(Key.Down);
			public static readonly ICommand Cancel = new Command(Key.Escape);
			public static readonly ICommand Enter = new Command(Key.Enter);
		}

		public HistoryPanel(Widget rootWidget)
		{
			PanelWidget = rootWidget;
			scrollView = new ThemedScrollView();
			RootWidget = new Frame {
				Id = "HistoryPanel",
				Padding = new Thickness(4),
				Layout = new VBoxLayout {Spacing = 4},
				Nodes = {scrollView}
			};
			resultPane = scrollView.Content;
			scrollView.TabTravesable = new TabTraversable();
			resultPane.CompoundPresenter.Insert(0, new DelegatePresenter<Widget>(w => {
				w.PrepareRendererState();
				if (scrollView.IsFocused() && history.Count > 0) {
					Renderer.DrawRect(
						0, rowHeight * selectedIndex,
						w.Width, (selectedIndex + 1) * rowHeight,
						Theme.Colors.SelectedBackground);
				}
			}));
			scrollView.LateTasks.Add(new KeyRepeatHandler(ScrollView_KeyRepeated));
			BackupsManager.Instance.BackupSaved += RefreshHistory;
		}

		void ScrollView_KeyRepeated(WidgetInput input, Key key)
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

		void NavigateToItem(int index)
		{
			BackupsManager.Instance.SelectBackup(history[history.Count - 1 - index]);
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
			resultPane.Nodes.Clear();
			history?.Clear();
			history = BackupsManager.Instance.GetHistory(Document.Current);
			if (history == null || history.Count < 1) {
				return;
			}

			resultPane.Layout = new TableLayout {
				ColCount = 1,
				ColSpacing = 8,
				RowCount = history.Count,
				ColDefaults = new List<LayoutCell> {new LayoutCell {StretchY = 0}}
			};

			for (int i = history.Count - 1; i >= 0; i--) {
				resultPane.Nodes.Add(new ThemedSimpleText(history[i].DateTime.ToString()));
			}
			selectedIndex = 0;
			scrollView.ScrollPosition = scrollView.MinScrollPosition;
		}

		public void Attach()
		{
			PanelWidget.PushNode(RootWidget);
			RefreshHistory();
		}

		public void Detach()
		{
			RootWidget.Unlink();
		}
	}
}
