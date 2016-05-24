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
		}

		public class DragBehaviour
		{
			readonly Widget mainWindow;
			readonly DockPanel panel;

			public event Action<DockSite> OnDrag;

			public DragBehaviour(Widget mainWindow, DockPanel panel)
			{
				this.mainWindow = mainWindow;
				this.panel = panel;
				panel.TitleWidget.Tasks.Add(MainTask());
			}

			IEnumerator<object> MainTask()
			{
				var input = panel.TitleWidget.Input;
				while (true) {
					if (input.WasMousePressed() && (panel.TitleWidget.IsMouseOver() && !panel.CloseButton.IsMouseOver())) {
						yield return DragTask();
					}
					yield return null;
				}
			}

			IEnumerator<object> DragTask()
			{
				var dockSite = DockSite.None;
				var dockSiteRect = new Rectangle();
				mainWindow.PostPresenter = new DelegatePresenter<Widget>(widget => {
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
				while (input.IsMousePressed()) {
					var extent = (Vector2)CommonWindow.Current.ClientSize;
					dockSite = DockSite.None;
					for (int i = 0; i < 4; i++) {
						var r = dockSiteRects[i];
						r.A *= extent;
						r.B *= extent;
						if (r.Contains(input.MousePosition)) {
							dockSiteRect = r;
							dockSite = (DockSite)(i + 1);
						}
						CommonWindow.Current.Invalidate();
					}
					yield return null;
				}
				input.ReleaseMouse();
				mainWindow.PostPresenter = null;
				CommonWindow.Current.Invalidate();
				if (dockSite != DockSite.None) {
					OnDrag?.Invoke(dockSite);	
				}
			}
		}
	}

	public class DockManager
	{
		List<DockPanel> panels = new List<DockPanel>();
		Widget rootWidget;
		Widget documentArea;

		public event Action Closed;

		public DockManager()
		{
			var defaultWindowSize = new Size(1400, 800);
			var window = new Window(new WindowOptions { ClientSize = defaultWindowSize, FixedSize = false, RefreshRate = 30 });
			window.Closed += () => Closed?.Invoke();
			rootWidget = new DefaultWindowWidget(window, continuousRendering: false) {
				Layout = new HBoxLayout(),
				Padding = new Thickness(4),
				CornerBlinkOnRendering = true
			};
			documentArea = new Widget { PostPresenter = new WidgetFlatFillPresenter(Color4.Gray) };
		}

		public void AddPanel(DockPanel panel, DockSite site, Vector2 dockedSize)
		{
			panel.Placement = new DockPanel.PanelPlacement { Title = panel.Title, Site = site, DockedSize = dockedSize };
			panels.Add(panel);
			var db = new DockPanel.DragBehaviour(rootWidget, panel);
			db.OnDrag += newDockSite => {
				panels.Remove(panel);
				panels.Insert(0, panel);
				panel.Placement.Site = newDockSite;
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
			rootWidget.Nodes.Clear();
			documentArea.Unlink();
			var currentContainer = rootWidget;
			int insertAt = 0;
			var stretch = Vector2.Zero;
			foreach (var p in panels) {
				if (p.Placement.Hidden) {
					continue;
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

		public State ExportState()
		{
			var state = new State();
			foreach (var p in panels) {
				state.PanelPlacements.Add(p.Placement);
			}
			return state;
		}

		public void ImportState(State state)
		{
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
		}
	}
}