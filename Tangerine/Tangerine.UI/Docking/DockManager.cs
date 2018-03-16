using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Yuzu;

namespace Tangerine.UI
{
	public class DockManager
	{
		public const string DocumentAreaId = "DocumentArea";

		public DockingModel Model => DockingModel.Instance;
		private readonly IMenu padsMenu;
		public readonly Widget ToolbarArea;
		public readonly Widget DocumentArea;
		public readonly WindowWidget MainWindowWidget;
		public static DockManager Instance { get; private set; }

		public event Action<IEnumerable<string>> FilesDropped;
		private const string AppIconPath = @"Tangerine.Resources.Icons.icon.ico";

		private DockManager(Vector2 windowSize, IMenu padsMenu)
		{
			this.padsMenu = padsMenu;
			var window = new Window(new WindowOptions {
				ClientSize = windowSize,
				FixedSize = false,
				Title = "Tangerine",
				MacWindowTabbingMode = MacWindowTabbingMode.Disallowed,
#if WIN
				Icon = new System.Drawing.Icon(new EmbeddedResource(AppIconPath, "Tangerine").GetResourceStream()),
#endif // WIN
			});
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
				Layout = new HBoxLayout { Spacing = 4 },
				LayoutCell = new LayoutCell { StretchY = 0 },
			};
			DocumentArea = new Frame {
				ClipChildren = ClipMethod.ScissorTest,
			};
			DocumentArea.CompoundPresenter.Add(new WidgetFlatFillPresenter(Color4.Gray));
			var windowPlacement = new WindowPlacement {
				Size = windowSize,
				Position = window.ClientPosition,
				Root = new SplitPlacement(),
				WindowWidget = MainWindowWidget
			};
			DockingModel.Instance.Initialize(windowPlacement);
			window.Moved += () => Model.Placements.First().Position = window.ClientPosition;
			window.Resized += (deviceRotated) => Model.Placements.First().Size = window.ClientSize;
			MainWindowWidget.AddChangeWatcher(
				() => MainWindowWidget.Window.State,
				value => Model.Placements[0].State = value);
		}

		public static void Initialize(Vector2 windowSize, IMenu padsMenu)
		{
			if (Instance != null) {
				throw new InvalidOperationException();
			}
			Instance = new DockManager(windowSize, padsMenu);
		}

		public void DockPanelTo(PanelPlacement placement, PanelPlacement target, DockSite site, float stretch)
		{
			var windowPlacement = Model.GetWindowByPlacement(placement);
			placement.Unlink();
			Model.DockPanelTo(placement, target, site, stretch);
			if (!windowPlacement.Root.DescendantPanels().Any()) {
				Model.RemoveWindow(windowPlacement);
				CloseWindow(windowPlacement.WindowWidget.Window);
			} else {
				windowPlacement.Root.RemoveRedundantNodes();
				RefreshWindow(windowPlacement);
			}
			RefreshWindow(Model.GetWindowByPlacement(target));
		}

		private void SetDropHandler(IWindow window)
		{
			window.AllowDropFiles = true;
			window.FilesDropped += files => FilesDropped?.Invoke(files);
		}

		public PanelPlacement AddPanel(DockPanel panel, Placement targetPlacement, DockSite site, float stretch)
		{
			padsMenu.Add(new Command(panel.Title, () => ShowPanel(panel)));
			padsMenu.Refresh();
			return Model.AddPanel(panel, targetPlacement, site, stretch);
		}

		public PanelPlacement AppendPanelTo(DockPanel panel, SplitPlacement targetPlacement)
		{
			padsMenu.Add(new Command(panel.Title, () => ShowPanel(panel)));
			padsMenu.Refresh();
			return Model.AppendPanelToPlacement(panel, targetPlacement);
		}

		public void ShowPanel(DockPanel panel)
		{
			Model.FindPanelPlacement(panel.Id).Hidden = false;
			Refresh();
		}

		public void Refresh()
		{
			CloseWindowsIfNecessary();
			CreateWindowsIfNecessary();
			RefreshWindows();
		}

		private void CloseWindowsIfNecessary()
		{
			foreach (var window in Model.Placements) {
				if (!window.Root.AnyVisiblePanel() && window.WindowWidget != null) {
					CloseWindow(window.WindowWidget.Window);
				}
			}
		}

		private void CreateWindowsIfNecessary()
		{
			for (var i = 0; i < Model.Placements.Count; i++) {
				var placement = Model.Placements[i];
				if (!placement.Root.AnyVisiblePanel()) {
					continue;
				}

				if (placement.WindowWidget != null) continue;
				if (i == 0) {
					placement.WindowWidget = MainWindowWidget;
				} else {
					CreateFloatingWindow(placement);
				}
			}
		}

		private void CreateWidgetForPlacement(Splitter container, Placement placement, float stretch = 1, bool isFirst = true)
		{
			var insertAt = isFirst ? 0 : 1;
			if (container.Nodes.Count == 0) {
				insertAt = 0;
			}
			if (placement == null || !placement.AnyVisiblePanel()) {
				return;
			}
			placement.SwitchType(
				panel => CreateWidgetForPanel(container, stretch, insertAt, panel),
				panelGroup => {
					if (panelGroup.DescendantPanels().Count(p => !p.Hidden) == 1) {
						CreateWidgetForPanel(container, stretch, insertAt, panelGroup.DescendantPanels().First(p => !p.Hidden));
					} else {
						CreateWidgetForPanelGroup(container, panelGroup, stretch, insertAt);
					}
				},
				group => CreateWidgetForGroup(container, group, stretch, insertAt)
			);
		}

		private void CreateWidgetForGroup(Splitter container, SplitPlacement placement, float stretch, int insertAt)
		{
			var splitter = placement.Separator == DockSeparator.Vertical ? (Splitter)new ThemedHSplitter() : new ThemedVSplitter();
			splitter.DragEnded += RefreshDockedSize(placement, splitter);
			container.Stretches.Insert(insertAt, stretch);
			CreateWidgetForPlacement(splitter, placement.FirstChild, placement.Stretch);
			CreateWidgetForPlacement(splitter, placement.SecondChild, 1 - placement.Stretch, false);
			container.Nodes.Insert(insertAt, splitter);
		}

		private void CreateWidgetForPanel(Splitter container, float stretch, int insertAt, PanelPlacement placement)
		{
			var panel = Model.Panels.First(pan => pan.Id == placement.Id);
			var widget = panel.ContentWidget;
			widget.Unlink();
			if (Model.FindPanelPlacement(panel.Id).Hidden) {
				return;
			}
			if (panel.IsUndockable) {
				ThemedSimpleText titleLabel;
				ThemedTabCloseButton closeButton;
				var titleWidget = new Widget {
					Layout = new HBoxLayout(),
					Nodes = {
					(titleLabel = new ThemedSimpleText {
						Padding = new Thickness(4, 0),
						ForceUncutText = false,
						MinMaxHeight = Theme.Metrics.MinTabSize.Y,
						VAlignment = VAlignment.Center,
					}),
					(closeButton = new ThemedTabCloseButton { LayoutCell = new LayoutCell(Alignment.Center) })
				},
					HitTestTarget = true
				};
				titleLabel.AddChangeWatcher(() => panel.Title, text => titleLabel.Text = text);
				titleWidget.CompoundPresenter.Add(new DelegatePresenter<Widget>(w => {
					w.PrepareRendererState();
					Renderer.DrawRect(Vector2.Zero, w.Size, ColorTheme.Current.Docking.PanelTitleBackground);
					Renderer.DrawLine(0, w.Height - 0.5f, w.Width, w.Height - 0.5f, ColorTheme.Current.Docking.PanelTitleSeparator);
				}));
				var rootWidget = new Widget {
					Id = $"DockPanel<{panel.Id}>",
					LayoutCell = new LayoutCell(),
					Layout = new VBoxLayout(),
					Nodes = {
						titleWidget,
						new Frame { Nodes = { widget } },
					}
				};
				widget.ExpandToContainerWithAnchors();
				rootWidget.FocusScope = new KeyboardFocusScope(rootWidget);
				closeButton.Clicked += () => {
					Model.FindPanelPlacement(panel.Id).Hidden = true;
					Refresh();
				};
				widget = rootWidget;
				CreateDragBehaviour(panel, titleWidget);
			}
			container.Nodes.Insert(insertAt, widget);
			container.Stretches.Insert(insertAt, stretch);
		}

		private void CreateWidgetForPanelGroup(Splitter container, TabBarPlacement panelGroup, float stretch, int insertAt)
		{
			var tabbedWidget = new TabbedWidget { LayoutCell = new LayoutCell { StretchY = 0 }, AllowReordering = true };
			foreach (var panel in panelGroup.Placements.Select(p => Model.Panels.First(pan => pan.Id == p.Id))) {
				panel.ContentWidget.Unlink();
				if (Model.FindPanelPlacement(panel.Id).Hidden) {
					continue;
				}
				var tab = new ThemedTab {
					Text = panel.Id,
					Closable = true,
				};
				tab.Closing += () => {
					Model.FindPanelPlacement(panel.Id).Hidden = true;
					Refresh();
				};
				tabbedWidget.AddTab(tab, new Widget {
					Id = $"DockPanel<{panel.Id}>",
					LayoutCell = new LayoutCell(),
					Layout = new VBoxLayout(),
					Padding = new Thickness(0, 1),
					Nodes = { panel.ContentWidget }
				}, true);
				CreateDragBehaviour(panel, tab);
			}
			tabbedWidget.TabBar.OnReorder += args => {
				var item = panelGroup.Placements[args.OldIndex];
				panelGroup.RemovePlacement(item);
				panelGroup.Placements.Insert(args.NewIndex, item);
			};
			container.Nodes.Insert(insertAt, tabbedWidget);
			container.Stretches.Insert(insertAt, stretch);
		}

		private void CreateDragBehaviour(DockPanel panel, Widget inputWidget)
		{
			var db = new PanelDragBehaviour(inputWidget, panel);
			db.OnUndock += (positionOffset, windowPosition) => {
				var panelWindow = (WindowWidget)panel.ContentWidget.GetRoot();
				var placement = Model.FindPanelPlacement(panel.Id);
				var windowPlacement = Model.GetWindowByPlacement(placement);
				if (windowPlacement.Root.DescendantPanels().Count(p => !p.Hidden) > 1) {

					var wrapper = new WindowPlacement {
						Size = placement.CalcGlobalSize() * panelWindow.Size,
						Root = new SplitPlacement()
					};
					placement.Unlink();
					windowPlacement.Root.RemoveRedundantNodes();
					Model.AddWindow(wrapper);
					wrapper.Root.FirstChild = placement;
#if MAC
					wrapper.Position = windowPosition - new Vector2(0, wrapper.Size.Y);
#else
					wrapper.Position = windowPosition;
#endif
					RefreshWindow(windowPlacement);
					CreateFloatingWindow(wrapper);
					RefreshWindow(wrapper);
				}
				WindowDragBehaviour.CreateFor(panel.Id, positionOffset);
			};
		}

		private void CreateFloatingWindow(WindowPlacement placement)
		{
			var window = new Window(new WindowOptions {
				FixedSize = false,
				MacWindowTabbingMode = MacWindowTabbingMode.Disallowed,
#if WIN
				Icon = new System.Drawing.Icon(new EmbeddedResource(AppIconPath, "Tangerine").GetResourceStream()),
#endif
			});
			window.UnhandledExceptionOnUpdate += HandleException;
			SetDropHandler(window);
			window.Closing += reason => {
				if (reason == CloseReason.MainWindowClosing) {
					return true;
				}

				var windowPlacement = Model.Placements.FirstOrDefault(p => p.WindowWidget?.Window == window);
				if (windowPlacement != null) {
					windowPlacement.WindowWidget = null;
					Model.HideWindowPanels(windowPlacement);
				}
				return true;
			};
			window.ClientSize = placement.Size;
			var windowWidget = new ThemedInvalidableWindowWidget(window) {
				Layout = new StackLayout(),
			};
			windowWidget.Components.Add(new RequestedDockingComponent());
			windowWidget.CompoundPostPresenter.Add(new DockingPresenter());
			windowWidget.AddChangeWatcher(() => windowWidget.Window.State, value => placement.State = value);
			placement.WindowWidget = windowWidget;
			window.ClientPosition = placement.Position;
			window.Moved += () => placement.Position = window.ClientPosition;
			window.Resized += (deviceRotated) => placement.Size = window.ClientSize;
		}

		private void RefreshWindow(WindowPlacement root)
		{
			if (root.WindowWidget == null) return;
			var windowWidget = root.WindowWidget;
			windowWidget.Nodes.Clear();
			if (!root.Root.AnyVisiblePanel()) {
				CloseWindow(root.WindowWidget.Window);
				return;
			}
			if (MainWindowWidget == windowWidget) {
				ToolbarArea.Unlink();
				MainWindowWidget.Nodes.Add(ToolbarArea);
			}
			Splitter currentContainer = new ThemedHSplitter { Anchors = Anchors.LeftRightTopBottom };
			windowWidget.Window.ClientPosition = root.Position;
			windowWidget.Window.ClientSize = root.Size;
			windowWidget.Window.State = root.State;
			windowWidget.Nodes.Add(currentContainer);
			CreateWidgetForPlacement(currentContainer, root.Root);
		}

		private Action RefreshDockedSize(SplitPlacement placement, Splitter splitter)
		{
			return () => {
				var overal = Vector2.Zero;
				foreach (var w in splitter.Nodes) {
					overal += w.AsWidget.LayoutCell.Stretch;
				}
				var first = splitter.Nodes.First().AsWidget.LayoutCell.Stretch;
				var value = first / overal;
				placement.Stretch = placement.Separator == DockSeparator.Horizontal ? value.Y : value.X;
			};
		}

		private void RefreshWindows()
		{
			foreach (var placement in Model.Placements) {
				if (!placement.Root.AnyVisiblePanel()) {
					continue;
				}
				RefreshWindow(placement);
			}
		}

		private void HandleException(System.Exception e)
		{
			AlertDialog.Show(e.Message);
		}

		private void CloseWindow(IWindow window)
		{
			UpdateHandler closeWindowOnNextFrame = null;
			closeWindowOnNextFrame = delta => {
				window.Close();
				MainWindowWidget.Updating -= closeWindowOnNextFrame;
			};
			MainWindowWidget.Updating += closeWindowOnNextFrame;
		}

		public State ExportState()
		{
			var state = new State();
			foreach (var p in Model.Placements) {
				state.WindowPlacements.Add(p.Clone());
				p.State = p.WindowWidget.Window.State;
			}
			return state;
		}

		public void ImportState(State state, bool resizeMainWindow = true)
		{
			try {
				if (!state.WindowPlacements.Any()) {
					throw new System.Exception("Unable to import state: the number of windows is zero");
				}
				for (var i = 1; i < Model.Placements.Count; i++) {
					Model.Placements[i].WindowWidget.Nodes.Clear();
					CloseWindow(Model.Placements[i].WindowWidget.Window);
				}
				var savedPanels = Model.Panels.ToList();
				Model.Placements.Clear();
				Model.Panels.Clear();
				foreach (var windowPlacement in state.WindowPlacements) {
					Model.Placements.Add(windowPlacement);
					foreach (var panelPlacement in windowPlacement.Root.DescendantPanels()) {
						var p = savedPanels.FirstOrDefault(i => i.Id == panelPlacement.Title);
						if (p == null) continue;
						Model.Panels.Add(p);
					}
				}
				if (resizeMainWindow && state.WindowPlacements.First().Size != Vector2.Zero) {
					var mainWindow = Model.MainWindow;
					MainWindowWidget.Window.ClientSize = mainWindow.Size;
					MainWindowWidget.Window.State = mainWindow.State;
				}
				Model.Panels.AddRange(savedPanels.Except(Model.Panels).ToList());
			} catch {
				System.Console.WriteLine("Warning: Unable to load dock state from user preferences");
				CommandQueue.Instance.Add((Command)GenericCommands.DefaultLayout);
			}
		}

		public class State
		{
			[YuzuMember]
			public List<WindowPlacement> WindowPlacements = new List<WindowPlacement>();

			public State Clone()
			{
				return new State {
					WindowPlacements = WindowPlacements.Select(p => p.Clone()).ToList()
				};
			}
		}
	}
}
