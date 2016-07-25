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
		readonly WindowWidget mainWidget;
		readonly Menu padsMenu;

		public readonly Widget DocumentArea;

		public event Func<bool> Closing;
		public event Action Closed;

		public DockManager(Vector2 windowSize, Menu padsMenu)
		{
			this.padsMenu = padsMenu;
			var window = new Window(new WindowOptions { ClientSize = windowSize, FixedSize = false, RefreshRate = 30, Title = "Tangerine" });
			window.Closing += () => Closing();
			window.Closed += () => Closed();
			mainWidget = new DefaultWindowWidget(window, continuousRendering: false) {
				Id = "MainWindow",
				Layout = new HBoxLayout(),
				Padding = new Thickness(4),
				CornerBlinkOnRendering = true
			};
			new TabTraverseController(mainWidget);
			DocumentArea = new Frame {
				ClipChildren = ClipMethod.ScissorTest,
				TabTraverseScope = new TabTraverseScope()
			};
			DocumentArea.CompoundPresenter.Add(new WidgetFlatFillPresenter(Color4.Gray));
		}

		public void AddPanel(DockPanel panel, DockSite site, Vector2 size)
		{
			padsMenu.Add(new DelegateCommand(panel.Title, () => ShowPanel(panel)));
			padsMenu.Refresh();
			var dockedSize = mainWidget.Size / size;
			panel.Placement = new DockPanel.PanelPlacement { Title = panel.Title, Site = site, DockedSize = dockedSize, Docked = true, UndockedSize = size };
			panels.Add(panel);
			var db = new DockPanel.DragBehaviour(mainWidget, panel);
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
			mainWidget.Nodes.Clear();
			DocumentArea.Unlink();
			var currentContainer = (Widget)mainWidget;
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
						var window = new Window(new WindowOptions { RefreshRate = 30, Title = p.Title, FixedSize = false });
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
						p.WindowWidget = new DefaultWindowWidget(window, continuousRendering: false) {
							Layout = new StackLayout(),
						};
						p.RootWidget.Unlink();
						p.WindowWidget.AddNode(p.RootWidget);
						new TabTraverseController(p.WindowWidget);
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
					mainWidget.Updating -= closeWindowOnNextFrame;
				};
				mainWidget.Updating += closeWindowOnNextFrame;
			}
		}

		public State ExportState()
		{
			var state = new State();
			foreach (var p in panels) {
				state.PanelPlacements.Add(p.Placement);
			}
			state.MainWindowPosition = mainWidget.Window.ClientPosition;
			state.MainWindowSize = mainWidget.Window.ClientSize;
			return state;
		}

		public void ImportState(State state)
		{
			if (state.MainWindowSize != Vector2.Zero) {
				mainWidget.Window.ClientSize = state.MainWindowSize;
				mainWidget.Window.ClientPosition = state.MainWindowPosition;
			}
			var allPanels = panels.ToList();
			panels.Clear();
			foreach (var s in state.PanelPlacements) {
				var p = allPanels.FirstOrDefault(i => i.Title == s.Title);
				if (p != null) {
					p.Placement = s;
					panels.Add(p);
				}
			}
			Refresh();
		}

		[ProtoContract]
		public class State
		{
			[ProtoMember(2)]
			public List<DockPanel.PanelPlacement> PanelPlacements = new List<DockPanel.PanelPlacement>();
			[ProtoMember(3)]
			public Vector2 MainWindowPosition;
			[ProtoMember(4)]
			public Vector2 MainWindowSize;
		}
	}
}