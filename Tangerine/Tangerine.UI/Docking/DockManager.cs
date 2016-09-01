using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Lime;
using ProtoBuf;

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
		readonly Menu padsMenu;
		public readonly Widget DocumentArea;
		public readonly WindowWidget MainWindowWidget;
		public event Action<DockPanel> DockPanelAdded;
		public static DockManager Instance { get; private set; }

		public static void Initialize(Vector2 windowSize, Menu padsMenu)
		{
			if (Instance != null) {
				throw new InvalidOperationException();
			}
			Instance = new DockManager(windowSize, padsMenu);
		}

		private DockManager(Vector2 windowSize, Menu padsMenu)
		{
			this.padsMenu = padsMenu;
			var window = new Window(new WindowOptions { ClientSize = windowSize, FixedSize = false, Title = "Tangerine" });
			MainWindowWidget = new InvalidableWindowWidget(window) {
				Id = "MainWindow",
				Layout = new HBoxLayout(),
				Padding = new Thickness(4),
				Size = windowSize,
				RedrawMarkVisible = true
			};
			DocumentArea = new Frame {
				ClipChildren = ClipMethod.ScissorTest,
			};
			DocumentArea.CompoundPresenter.Add(new WidgetFlatFillPresenter(Color4.Gray));
		}

		public void AddPanel(DockPanel panel, DockSite site, Vector2 size)
		{
			padsMenu.Add(new DelegateCommand(panel.Title, () => ShowPanel(panel)));
			padsMenu.Refresh();
			var dockedSize = CalcDockedSize(site, size);
			panel.Placement = new DockPanel.PanelPlacement { Title = panel.Title, Site = site, DockedSize = dockedSize, Docked = true, UndockedSize = size };
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

		void ShowPanel(DockPanel panel)
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
			DocumentArea.Unlink();
			var currentContainer = (Widget)MainWindowWidget;
			int insertAt = 0;
			var stretch = Vector2.Zero;
			foreach (var p in panels.Where(p => p.Placement.Docked)) {
				ClosePanelWindow(p);
				if (!p.Placement.Hidden) {
					p.RootWidget.Unlink();
					p.RootWidget.LayoutCell.Stretch = p.Placement.DockedSize;
					var splitter = (p.Placement.Site == DockSite.Left || p.Placement.Site == DockSite.Right) ?
						(Splitter)new HSplitter() : (Splitter)new VSplitter();
					splitter.DragEnded += p.RefreshDockedSize;
					splitter.AddNode(p.RootWidget);
					splitter.LayoutCell = new LayoutCell { Stretch = Vector2.One - stretch };
					currentContainer.Nodes.Insert(insertAt, splitter);
					currentContainer = splitter;
					insertAt = (p.Placement.Site == DockSite.Left || p.Placement.Site == DockSite.Top) ? 1 : 0;
					stretch = p.Placement.DockedSize;
				} else {
					p.RootWidget.Unlink();
				}
			}
			DocumentArea.LayoutCell = new LayoutCell { Stretch = Vector2.One - stretch };
			currentContainer.Nodes.Insert(insertAt, DocumentArea);
		}

		void RefreshUndockedPanels()
		{
			foreach (var p in panels.Where(p => !p.Placement.Docked)) {
				if (!p.Placement.Hidden) {
					if (p.WindowWidget == null) {
						var window = new Window(new WindowOptions { Title = p.Title, FixedSize = false });
						window.Closing += () => {
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
						p.WindowWidget = new InvalidableWindowWidget(window) {
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
			return state;
		}

		public void ImportState(State state, bool resizeMainWindow = true)
		{
			if (resizeMainWindow && state.MainWindowSize != Vector2.Zero) {
				MainWindowWidget.Window.ClientSize = state.MainWindowSize;
				MainWindowWidget.Window.ClientPosition = state.MainWindowPosition;
			}
			foreach (var p in panels) {
				var pp = state.PanelPlacements.FirstOrDefault(i => i.Title == p.Title);
				if (pp != null) {
					p.Placement = pp;
				}
			}
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
		}
	}
}
