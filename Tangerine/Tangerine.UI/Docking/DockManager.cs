using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Yuzu;

namespace Tangerine.UI
{
	public enum DockSite
	{
		None,
		Left,
		Top,
		Right,
		Bottom
	}

	public class DockManager
	{
		readonly List<DockPanel> panels = new List<DockPanel>();
		readonly IMenu padsMenu;
		public readonly Widget ToolbarArea;
		public readonly Widget DocumentArea;
		public readonly WindowWidget MainWindowWidget;
		public event Action<DockPanel> DockPanelAdded;
		public static DockManager Instance { get; private set; }

		public event Action<IEnumerable<string>> FilesDropped;

		public static void Initialize(Vector2 windowSize, IMenu padsMenu)
		{
			if (Instance != null) {
				throw new InvalidOperationException();
			}
			Instance = new DockManager(windowSize, padsMenu);
		}

		private DockManager(Vector2 windowSize, IMenu padsMenu)
		{
			this.padsMenu = padsMenu;
			var window = new Window(new WindowOptions { ClientSize = windowSize, FixedSize = false, Title = "Tangerine" });
			window.UnhandledExceptionOnUpdate += HandleException;
			SetDropHandler(window);
			MainWindowWidget = new ThemedInvalidableWindowWidget(window) {
				Id = "MainWindow",
				Layout = new VBoxLayout(),
				Size = windowSize,
				RedrawMarkVisible = true
			};
			ToolbarArea = new Frame {
				Id = "Toolbar",
				ClipChildren = ClipMethod.ScissorTest,
				Layout = new FlowLayout { Spacing = 4 },
				LayoutCell = new LayoutCell { StretchY = 0 },
			};
			DocumentArea = new Frame {
				ClipChildren = ClipMethod.ScissorTest,
			};
			DocumentArea.CompoundPresenter.Add(new WidgetFlatFillPresenter(Color4.Gray));
		}

		private void SetDropHandler(IWindow window)
		{
			window.AllowDropFiles = true;
			window.FilesDropped += files => FilesDropped?.Invoke(files);
		}

		public void AddPanel(DockPanel panel, DockSite site, Vector2 size)
		{
			padsMenu.Add(new Command(panel.Title, () => ShowPanel(panel)));
			padsMenu.Refresh();
			var dockedSize = CalcDockedSize(site, size);
			panel.Placement = new DockPanel.PanelPlacement { Title = panel.Id, Site = site, DockedSize = dockedSize, Docked = true, UndockedSize = size };
			panels.Add(panel);
			var db = new DockPanel.DragBehaviour(MainWindowWidget, panel);
			db.OnDock += newDockSite => {
				panels.Remove(panel);
				panels.Insert(0, panel);
				panel.Placement.Docked = true;
				panel.Placement.Site = newDockSite;
				Refresh();
			};
			db.OnUndock += position => {
				var p = panel.Placement;
				p.Docked = false;
				p.UndockedSize = panel.ContentWidget.Size;
#if MAC
				p.UndockedPosition = position - new Vector2(0, p.UndockedSize.Y);
#else
				p.UndockedPosition = position;
#endif
				Refresh();
			};
			panel.CloseButton.Clicked += () => {
				panel.Placement.Hidden = true;
				Refresh();
			};
			Refresh();
			DockPanelAdded?.Invoke(panel);
		}

		private Vector2 CalcDockedSize(DockSite site, Vector2 size)
		{
			return size / MainWindowWidget.Size;
		}

		public void ShowPanel(DockPanel panel)
		{
			panel.Placement.Hidden = false;
			Refresh();
		}

		void Refresh()
		{
			RefreshDockedPanels();
			RefreshUndockedPanels();
		}

		void RefreshDockedPanels()
		{
			MainWindowWidget.Nodes.Clear();
			ToolbarArea.Unlink();
			DocumentArea.Unlink();
			MainWindowWidget.Nodes.Add(ToolbarArea);
			Splitter currentContainer = new ThemedHSplitter();
			MainWindowWidget.Nodes.Add(currentContainer);
			int insertAt = 0;
			float stretch = 0;
			foreach (var p in panels.Where(p => p.Placement.Docked)) {
				ClosePanelWindow(p);
				if (!p.Placement.Hidden) {
					p.RootWidget.Unlink();
					Splitter splitter;
					if (p.Placement.Site == DockSite.Left || p.Placement.Site == DockSite.Right) {
						splitter = new ThemedHSplitter();
						splitter.Stretches.Add(p.Placement.DockedSize.X);
					} else {
						splitter = new ThemedVSplitter();
						splitter.Stretches.Add(p.Placement.DockedSize.Y);
					}
					splitter.DragEnded += p.RefreshDockedSize;
					splitter.AddNode(p.RootWidget);
					currentContainer.Stretches.Insert(insertAt, 1 - stretch);
					currentContainer.Nodes.Insert(insertAt, splitter);
					currentContainer = splitter;
					insertAt = (p.Placement.Site == DockSite.Left || p.Placement.Site == DockSite.Top) ? 1 : 0;
					stretch = splitter.Stretches[0];
				} else {
					p.RootWidget.Unlink();
				}
			}
			currentContainer.Stretches.Insert(insertAt, 1 - stretch);
			currentContainer.Nodes.Insert(insertAt, DocumentArea);
		}

		void RefreshUndockedPanels()
		{
			foreach (var p in panels.Where(p => !p.Placement.Docked)) {
				if (!p.Placement.Hidden) {
					if (p.WindowWidget == null) {
						var window = new Window(new WindowOptions { Title = p.Title, FixedSize = false });
						window.UnhandledExceptionOnUpdate += HandleException;
						SetDropHandler(window);
						window.Closing += reason => {
							if (reason == CloseReason.MainWindowClosing) {
								return true;
							}
							if (!p.Placement.Docked) {
								p.Placement.Hidden = true;
							}
							Refresh();
							return true;
						};
						window.ClientSize = p.Placement.UndockedSize;
						window.ClientPosition = p.Placement.UndockedPosition;
						window.Moved += () => {
							p.Placement.UndockedPosition = window.ClientPosition;
						};
						window.Resized += (deviceRotated) => {
							p.Placement.UndockedSize = window.ClientSize;
						};
						p.WindowWidget = new ThemedInvalidableWindowWidget(window) {
							Layout = new StackLayout(),
						};
						p.RootWidget.Unlink();
						p.WindowWidget.AddNode(p.RootWidget);
					}
				} else {
					ClosePanelWindow(p);
				}
			}
		}

		void HandleException(System.Exception e)
		{
			AlertDialog.Show(e.Message);
		}

		void ClosePanelWindow(DockPanel panel)
		{
			if (panel.WindowWidget != null) {
				panel.RootWidget.Unlink();
				var window = panel.WindowWidget.Window;
				panel.WindowWidget = null;
				Lime.UpdateHandler closeWindowOnNextFrame = null;
				closeWindowOnNextFrame = delta => {
					window.Close();
					MainWindowWidget.Updating -= closeWindowOnNextFrame;
				};
				MainWindowWidget.Updating += closeWindowOnNextFrame;
			}
		}

		public State ExportState()
		{
			var state = new State();
			foreach (var p in panels) {
				state.PanelPlacements.Add(p.Placement);
			}
			state.MainWindowPosition = MainWindowWidget.Window.ClientPosition;
			state.MainWindowSize = MainWindowWidget.Window.ClientSize;
			state.MainWindowState = MainWindowWidget.Window.State;
			return state;
		}

		public void ImportState(State state, bool resizeMainWindow = true)
		{
			if (resizeMainWindow && state.MainWindowSize != Vector2.Zero) {
				MainWindowWidget.Window.ClientSize = state.MainWindowSize;
				MainWindowWidget.Window.ClientPosition = state.MainWindowPosition;
				MainWindowWidget.Window.State = state.MainWindowState;
			}
			var savedPanels = panels.ToList();
			panels.Clear();
			foreach (var pp in state.PanelPlacements) {
				var p = savedPanels.FirstOrDefault(i => i.Id == pp.Title);
				if (p != null) {
					p.Placement = pp;
					panels.Add(p);
				}
			}
			panels.AddRange(savedPanels.Except(panels).ToList());
			Refresh();
		}

		public class State
		{
			[YuzuMember]
			public List<DockPanel.PanelPlacement> PanelPlacements = new List<DockPanel.PanelPlacement>();
			[YuzuMember]
			public Vector2 MainWindowPosition;
			[YuzuMember]
			public Vector2 MainWindowSize;
			[YuzuMember]
			public WindowState MainWindowState;
		}
	}
}
