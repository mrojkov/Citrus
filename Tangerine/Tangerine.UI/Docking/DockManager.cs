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

	public class DockPanel
	{
		public readonly Widget RootWidget;
		public readonly Widget TitleWidget;
		public readonly Widget ContentWidget;
		public readonly Button CloseButton;
		public readonly string Title;
		public WindowWidget WindowWidget;
		public PanelPlacement Placement;

		public DockPanel(string title)
		{
			Title = title;
			TitleWidget = new Widget {
				Layout = new HBoxLayout(),
				Nodes = {
					new SimpleText { Text = Title, Padding = new Thickness(4, 0), AutoSizeConstraints = false, MinMaxHeight = 20 },
					(CloseButton = new BitmapButton(IconPool.GetTexture("PopupClose"), IconPool.GetTexture("PopupCloseHover")) { LayoutCell = new LayoutCell(Alignment.Center) })
				},
				HitTestTarget = true
			};
			ContentWidget = new Frame { ClipChildren = ClipMethod.ScissorTest };
			RootWidget = new Widget {
				LayoutCell = new LayoutCell(),
				Layout = new VBoxLayout(),
				Nodes = {
					TitleWidget,
					ContentWidget
				}
			};
		}

		internal void RefreshDockedSize()
		{
			var s = Vector2.Zero;
			foreach (var w in RootWidget.Parent.Nodes) {
				s += w.AsWidget.LayoutCell.Stretch;
			}
			Placement.DockedSize = RootWidget.LayoutCell.Stretch / s;
		}

		[ProtoContract]
		public class PanelPlacement
		{
			[ProtoMember(1)]
			public string Title;
			[ProtoMember(2)]
			public bool Docked;
			[ProtoMember(3)]
			public DockSite Site;
			[ProtoMember(4)]
			public Vector2 DockedSize;
			[ProtoMember(5)]
			public bool Hidden;
			[ProtoMember(6)]
			public Vector2 UndockedPosition;
			[ProtoMember(7)]
			public Vector2 UndockedSize;
		}

		public class DragBehaviour
		{
			readonly WindowWidget mainWidget;
			readonly DockPanel panel;

			public event Action<DockSite> OnDock;
			public event Action<Vector2> OnUndock;

			public DragBehaviour(WindowWidget mainWindow, DockPanel panel)
			{
				this.mainWidget = mainWindow;
				this.panel = panel;
				panel.TitleWidget.Tasks.Add(MainTask());
			}

			IEnumerator<object> MainTask()
			{
				var input = panel.TitleWidget.Input;
				while (true) {
					var mousePos = input.MousePosition;
					if (input.WasMousePressed() && (panel.TitleWidget.IsMouseOver() && !panel.CloseButton.IsMouseOver())) {
						while ((mousePos - input.MousePosition).Length < 10 && input.IsMousePressed()) {
							yield return null;
						}
						if (input.IsMousePressed()) {
							yield return DragTask();
						}
					}
					yield return null;
				}
			}

			IEnumerator<object> DragTask()
			{
				var dockSite = DockSite.None;
				var dockSiteRect = new Rectangle();
				mainWidget.PostPresenter = new DelegatePresenter<Widget>(widget => {
					if (dockSite != DockSite.None) {
						widget.PrepareRendererState();
						Renderer.DrawRectOutline(dockSiteRect.A + Vector2.One, dockSiteRect.B - Vector2.One, Colors.DockingRectagleOutline, 2);
					}
				});
				var input = panel.TitleWidget.Input;
				input.CaptureMouse();
				const float dockSiteWidth = 0.25f;
				var dockSiteRects = new Rectangle[4] {
					new Rectangle(Vector2.Zero, new Vector2(dockSiteWidth, 1)),
					new Rectangle(Vector2.Zero, new Vector2(1, dockSiteWidth)),
					new Rectangle(new Vector2(1 - dockSiteWidth, 0), Vector2.One),
					new Rectangle(new Vector2(0, 1 - dockSiteWidth), Vector2.One)
				};
				ThumbnalWindow thumbWindow = null;
				var initialMousePos = input.MousePosition;
				var mainWindow = mainWidget.Window;
				while (input.IsMousePressed()) {
					var extent = mainWindow.ClientSize;
					dockSite = DockSite.None;
					for (int i = 0; i < 4; i++) {
						var r = dockSiteRects[i];
						r.A *= extent;
						r.B *= extent;
						var p = Application.DesktopMousePosition - mainWindow.ClientPosition;
						if (Application.Platform == PlatformId.Mac) {
							p.Y = mainWindow.ClientSize.Y - p.Y;
						}
						if (r.Contains(p)) {
							dockSiteRect = r;
							dockSite = (DockSite)(i + 1);
						}
					}
					if (dockSite == DockSite.None) {
						if (thumbWindow == null) {
							thumbWindow = new ThumbnalWindow(panel.Title);
						}
					} else {
						mainWindow.Invalidate();
						thumbWindow?.Dispose();
						thumbWindow = null;
					}
					yield return null;
				}
				input.ReleaseMouse();
				mainWidget.PostPresenter = null;
				mainWindow.Invalidate();
				thumbWindow?.Dispose();
				thumbWindow = null;
				if (dockSite != DockSite.None) {
					OnDock?.Invoke(dockSite);	
				} else {
					OnUndock?.Invoke(Application.DesktopMousePosition - (initialMousePos - panel.TitleWidget.GlobalPosition));
				}
			}
		}

		class ThumbnalWindow : IDisposable
		{
			Window window;
			WindowWidget rootWidget;

			public ThumbnalWindow(string title)
			{
				window = new Window(new WindowOptions { FixedSize = true, ClientSize = new Vector2(100, 40), Style = WindowStyle.Borderless });
				rootWidget = new DefaultWindowWidget(window, continuousRendering: false) {
					PostPresenter = new WidgetBoundsPresenter(Color4.Black, 1),
					Layout = new StackLayout(),
					Nodes = {
						new SimpleText { Text = title, LayoutCell = new LayoutCell(Alignment.Center) }
					}
				};
				rootWidget.Updated += delta => StickToMouseCursor();
				StickToMouseCursor();
			}

			void StickToMouseCursor()
			{
				window.DecoratedPosition = Application.DesktopMousePosition - window.ClientSize / 2;
			}

			public void Dispose()
			{
				window.Close();
			}
		}
	}

	public class DockManager
	{
		List<DockPanel> panels = new List<DockPanel>();
		WindowWidget mainWidget;
		Widget documentArea;

		public event Action Closed;

		public DockManager(Vector2 windowSize)
		{
			var window = new Window(new WindowOptions { ClientSize = windowSize, FixedSize = false, RefreshRate = 30 });
			window.Closed += () => Closed?.Invoke();
			mainWidget = new DefaultWindowWidget(window, continuousRendering: false) {
				Id = "MainWindow",
				Layout = new HBoxLayout(),
				Padding = new Thickness(4),
				CornerBlinkOnRendering = true
			};
			documentArea = new Widget { PostPresenter = new WidgetFlatFillPresenter(Color4.Gray) };
		}

		public void AddPanel(DockPanel panel, DockSite site, Vector2 size)
		{
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
				p.UndockedPosition = position - new Vector2(0, p.UndockedSize.Y);
				Refresh();
			};
			panel.CloseButton.Clicked += () => {
				panel.Placement.Hidden = true;
				Refresh();
			};
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
			documentArea.Unlink();
			var currentContainer = (Widget)mainWidget;
			int insertAt = 0;
			var stretch = Vector2.Zero;
			foreach (var p in panels.Where(p => !p.Placement.Hidden && p.Placement.Docked)) {
				if (p.WindowWidget != null) {
					p.WindowWidget.Window.Close();
					p.WindowWidget = null;
				}
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
			}
			documentArea.LayoutCell = new LayoutCell { Stretch = Vector2.One - stretch };
			currentContainer.Nodes.Insert(insertAt, documentArea);
		}

		void RefreshUndockedPanels()
		{
			foreach (var p in panels.Where(p => !p.Placement.Hidden && !p.Placement.Docked)) {
				p.RootWidget.Unlink();
				if (p.WindowWidget == null) {
					var window = new Window(new WindowOptions { RefreshRate = 30, Title = p.Title, FixedSize = false });
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
				}
				p.WindowWidget.AddNode(p.RootWidget);
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