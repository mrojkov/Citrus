using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Yuzu;

namespace Tangerine.UI.Docking
{
	public class DockManager
	{
		private const string AppIconPath = @"Tangerine.Resources.Icons.icon.ico";
		public const string DocumentAreaId = "DocumentArea";

		private readonly List<FilesDropHandler> filesDropHandlers = new List<FilesDropHandler>();

		public static DockManager Instance { get; private set; }

		public readonly Widget ToolbarArea;
		public readonly Widget DocumentArea;
		public readonly WindowWidget MainWindowWidget;
		public DockHierarchy Model => DockHierarchy.Instance;
		public event Action<IEnumerable<string>> FilesDropped;
		public event Action<System.Exception> UnhandledExceptionOccurred;

		private DockManager(Vector2 windowSize)
		{
			var window = new Window(new WindowOptions {
				ClientSize = windowSize,
				FixedSize = false,
				Title = "Tangerine",
				MacWindowTabbingMode = MacWindowTabbingMode.Disallowed,
#if WIN
				Icon = new System.Drawing.Icon(new EmbeddedResource(AppIconPath, "Tangerine").GetResourceStream()),
#endif // WIN
			});
			window.UnhandledExceptionOnUpdate += e => UnhandledExceptionOccurred?.Invoke(e);
			SetDropHandler(window);
			MainWindowWidget = new ThemedInvalidableWindowWidget(window) {
				Id = "MainWindow",
				Layout = new VBoxLayout(),
				Size = windowSize,
				RedrawMarkVisible = true
			};
			MainWindowWidget.WidgetContext.GestureManager = new HelpModeGestureManager(MainWindowWidget.WidgetContext);
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
				WindowWidget = MainWindowWidget
			};
			DockHierarchy.Instance.Initialize(windowPlacement);
			window.Moved += () => Model.WindowPlacements.First().Position = window.ClientPosition;
			window.Resized += (deviceRotated) => Model.WindowPlacements.First().Size = window.ClientSize;
			MainWindowWidget.AddChangeWatcher(
				() => MainWindowWidget.Window.State,
				value => Model.WindowPlacements[0].State = value);
		}

		public static void Initialize(Vector2 windowSize)
		{
			if (Instance != null) {
				throw new InvalidOperationException();
			}
			Instance = new DockManager(windowSize);
		}

		public void DockPlacementTo(Placement placement, Placement target, DockSite site, float stretch)
		{
			var windowPlacement = Model.GetWindowByPlacement(placement);
			placement.Unlink();
			Model.DockPlacementTo(placement, target, site, stretch);
			if (!windowPlacement.Root.GetDescendantPanels().Any()) {
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
			window.FilesDropped += OnFilesDropped;
			window.Updating += FilesDropHandlersUpdate;
		}

		private void OnFilesDropped(IEnumerable<string> files)
		{
			FilesDropped?.Invoke(files);
			foreach (var filesDropHandler in filesDropHandlers) {
				if (filesDropHandler.TryToHandle(files)) {
					break;
				}
			}
		}

		private void FilesDropHandlersUpdate(float delta)
		{
			foreach (var filesDropHandler in filesDropHandlers) {
				filesDropHandler.HandleDropImage();
			}
		}

		public PanelPlacement AddPanel(Panel panel, Placement targetPlacement, DockSite site, float stretch = 0.25f)
		{
			return Model.AddPanel(panel, targetPlacement, site, stretch);
		}

		public PanelPlacement AppendPanelTo(Panel panel, SplitPlacement targetPlacement)
		{
			return Model.AppendPanelToPlacement(panel, targetPlacement);
		}

		public void ShowPanel(string panelId)
		{
			Model.FindPanelPlacement(panelId).Hidden = false;
			Refresh();
		}

		public void TogglePanel(Panel panel)
		{
			var placement = Model.FindPanelPlacement(panel.Id);
			placement.Hidden = !placement.Hidden;
			Refresh();
		}

		public void Refresh()
		{
			CloseWindowsIfNecessary();
			CreateWindowsIfNecessary();
			RefreshWindows();
		}

		public void ResolveAndRefresh()
		{
			ResolveInconsistency();
			Refresh();
		}

		private void ResolveInconsistency()
		{
			var floatingWindowDefaultSize = new Vector2(200, 300);
			var descendantPanelsList = GetDescendantPanelPlacements().ToList();
			// Remove all placements which not refered to existing panels
			foreach (var panel in descendantPanelsList.ToList()) {
				if (!Model.Panels.Any(p => p.Id == panel.Id)) {
					var windowPlacement = Model.GetWindowByPlacement(panel);
					windowPlacement.RemovePanel(panel.Id);
					descendantPanelsList.Remove(panel);
					windowPlacement.Root.RemoveRedundantNodes();
					if (!windowPlacement.GetDescendantPanels().Any()) {
						Model.WindowPlacements.Remove(windowPlacement);
					}
				}
			}
			foreach (var panel in Model.Panels) {
				// If there is no placement for panel, create new one in new window and hide.
				if (!descendantPanelsList.Any(placement => placement.Id == panel.Id)) {
					var windowPlacement = new WindowPlacement {
						Position = Model.WindowPlacements[0].Position + Model.WindowPlacements[0].Size / 2 - floatingWindowDefaultSize / 2,
						Size = floatingWindowDefaultSize
					};
					var splitPlacement = new SplitPlacement();
					splitPlacement.Append(new PanelPlacement {
						Title = panel.Title,
						Id = panel.Id,
						Hidden = true
					});
					windowPlacement.Append(splitPlacement);
					Model.WindowPlacements.Add(windowPlacement);
				}
			}
		}

		public IEnumerable<PanelPlacement> GetDescendantPanelPlacements() {
			foreach (var windowPlacement in Model.WindowPlacements) {
				foreach (var panelPlacement in windowPlacement.GetDescendantPanels()) {
					yield return panelPlacement;
				}
			}
		}

		private void CloseWindowsIfNecessary()
		{
			foreach (var window in Model.WindowPlacements) {
				if (!window.Root.AnyVisiblePanel() && window.WindowWidget != null) {
					CloseWindow(window.WindowWidget.Window);
				}
			}
		}

		private void CreateWindowsIfNecessary()
		{
			for (var i = 0; i < Model.WindowPlacements.Count; i++) {
				var placement = Model.WindowPlacements[i];
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
				panel => CreatePanelWidget(container, panel, stretch, insertAt),
				panelGroup => {
					if (panelGroup.GetDescendantPanels().Count(p => !p.Hidden) == 1) {
						CreatePanelWidget(container, panelGroup.GetDescendantPanels().First(p => !p.Hidden), stretch, insertAt);
					} else {
						CreateTabBarWidget(container, panelGroup, stretch, insertAt);
					}
				},
				group => CreateSplitWidget(container, group, stretch, insertAt)
			);
		}

		private void CreateSplitWidget(Splitter container, SplitPlacement placement, float stretch, int insertAt)
		{
			var splitter = placement.Separator == DockSeparator.Vertical ? (Splitter)new ThemedHSplitter() : new ThemedVSplitter();
			splitter.DragEnded += RefreshDockedSize(placement, splitter);
			container.Stretches.Insert(insertAt, stretch);
			CreateWidgetForPlacement(splitter, placement.FirstChild, placement.Stretch);
			CreateWidgetForPlacement(splitter, placement.SecondChild, 1 - placement.Stretch, false);
			container.Nodes.Insert(insertAt, splitter);
		}

		private Widget AddTitle(Widget widget, out ThemedTabCloseButton closeButton, out ThemedSimpleText title, string id = null)
		{
			Widget titleWidget;
			var result =  new Widget {
				Id = id,
				LayoutCell = new LayoutCell(),
				Layout = new VBoxLayout(),
				Nodes = {
					(titleWidget = new Widget {
						Layout = new HBoxLayout(),
						HitTestTarget = true,
						Nodes = {
							(title = new ThemedSimpleText {
								Padding = new Thickness(4, 0),
								ForceUncutText = false,
								MinMaxHeight = Theme.Metrics.MinTabSize.Y,
								VAlignment = VAlignment.Center
							}),
							(closeButton = new ThemedTabCloseButton {
								LayoutCell = new LayoutCell(Alignment.Center)
							})
						}
					}),
					new Frame {
						Nodes = {
							widget
						}
					}
				}
			};
			title.Padding.Right += 5; // Add space between close button and titleWidget right border.
			titleWidget.CompoundPresenter.Add(new DelegatePresenter<Widget>(w => {
				w.PrepareRendererState();
				Renderer.DrawRect(Vector2.Zero, w.Size, ColorTheme.Current.Docking.PanelTitleBackground);
				Renderer.DrawLine(0, w.Height - 0.5f, w.Width, w.Height - 0.5f, ColorTheme.Current.Docking.PanelTitleSeparator);
			}));
			widget.ExpandToContainerWithAnchors();
			result.FocusScope = new KeyboardFocusScope(result);
			return result;
		}

		private void CreatePanelWidget(Splitter container, PanelPlacement placement, float stretch, int insertAt)
		{
			var panel = Model.Panels.First(pan => pan.Id == placement.Id);
			var widget = panel.ContentWidget;
			widget.Unlink();
			if (Model.FindPanelPlacement(panel.Id).Hidden) {
				return;
			}
			if (panel.IsUndockable) {
				var rootWidget = AddTitle(widget, out ThemedTabCloseButton closeButton, out ThemedSimpleText titleLabel, $"DockPanel<{panel.Id}>");
				titleLabel.AddChangeWatcher(() => panel.Title, text => titleLabel.Text = text);
				closeButton.Clicked += () => {
					Model.FindPanelPlacement(panel.Id).Hidden = true;
					Refresh();
				};
				CreateDragBehaviour(placement, rootWidget.Nodes[0].AsWidget, widget);
				widget = rootWidget;
			}
			container.Nodes.Insert(insertAt, widget);
			container.Stretches.Insert(insertAt, stretch);
		}

		private void CreateTabBarWidget(Splitter container, TabBarPlacement placement, float stretch, int insertAt)
		{
			var tabbedWidget = new TabbedWidget(TabbedWidget.TabBarPlacement.Bottom) {
				LayoutCell = new LayoutCell { StretchY = 0 },
				AllowReordering = true
			};
			tabbedWidget.FocusScope = new KeyboardFocusScope(tabbedWidget);
			var rootWidget = AddTitle(tabbedWidget, out ThemedTabCloseButton closeButton, out ThemedSimpleText titleLabel);
			foreach (var panelPlacement in placement.Placements) {
				var panel = Model.Panels.First(pan => pan.Id == panelPlacement.Id);
				panel.ContentWidget.Unlink();
				if (panelPlacement.Hidden) {
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
				CreateDragBehaviour(panelPlacement, tab, panel.ContentWidget);
			}
			tabbedWidget.ActiveTabIndex = 0;
			tabbedWidget.TabBar.OnReorder += args => {
				var item = placement.Placements[args.OldIndex];
				placement.RemovePlacement(item);
				placement.Placements.Insert(args.NewIndex, item);
				tabbedWidget.ActivateTab(args.NewIndex);
			};
			titleLabel.AddChangeWatcher(
				() => placement.Placements[tabbedWidget.ActiveTabIndex].Title,
				title => titleLabel.Text = title
			);
			closeButton.Clicked += () => {
				foreach (var panelPlacement in placement.Placements) {
					panelPlacement.Hidden = true;
				}
				Refresh();
			};
			CreateDragBehaviour(placement, rootWidget.Nodes[0].AsWidget, rootWidget);
			container.Nodes.Insert(insertAt, rootWidget);
			container.Stretches.Insert(insertAt, stretch);
		}

		private void CreateDragBehaviour(Placement placement, Widget inputWidget, Widget contentWidget)
		{
			var db = new DragBehaviour(inputWidget, contentWidget, placement);
			db.OnUndock += (positionOffset, windowPosition) => {
				var panelWindow = (WindowWidget)contentWidget.GetRoot();
				var windowPlacement = Model.GetWindowByPlacement(placement);
				if (windowPlacement.Root.GetDescendantPanels().Count(p => !p.Hidden) > 1) {
					var wrapper = new WindowPlacement {
						Size = placement.CalcGlobalSize() * panelWindow.Size
					};
					placement.Unlink();
					windowPlacement.Root.RemoveRedundantNodes();
					Model.AddWindow(wrapper);
					wrapper.FirstChild = placement;
#if MAC
					wrapper.Position = windowPosition - new Vector2(0, wrapper.Size.Y);
#else
					wrapper.Position = windowPosition;
#endif
					RefreshWindow(windowPlacement);
					CreateFloatingWindow(wrapper);
					RefreshWindow(wrapper);
				}
				WindowDragBehaviour.CreateFor(placement, positionOffset);
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
				ToolWindow = true
			});
			window.UnhandledExceptionOnUpdate += e => UnhandledExceptionOccurred?.Invoke(e);
			SetDropHandler(window);
			window.Closing += reason => {
				if (reason == CloseReason.MainWindowClosing) {
					return true;
				}

				var windowPlacement = Model.WindowPlacements.FirstOrDefault(p => p.WindowWidget?.Window == window);
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
			windowWidget.WidgetContext.GestureManager = new HelpModeGestureManager(windowWidget.WidgetContext);
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
			foreach (var placement in Model.WindowPlacements) {
				if (!placement.Root.AnyVisiblePanel()) {
					continue;
				}
				RefreshWindow(placement);
			}
		}

		public void AddFilesDropHandler(FilesDropHandler filesDropHandler) => filesDropHandlers.Add(filesDropHandler);
		public void RemoveFilesDropHandler(FilesDropHandler filesDropHandler) => filesDropHandlers.Remove(filesDropHandler);

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
			foreach (var p in Model.WindowPlacements) {
				state.WindowPlacements.Add(p.Clone() as WindowPlacement);
			}
			return state;
		}

		public void ImportState(State state, bool resizeMainWindow = true)
		{
			try {
				if (!state.WindowPlacements.Any()) {
					throw new System.Exception("Unable to import state: the number of windows is zero");
				}
				for (var i = 1; i < Model.WindowPlacements.Count; i++) {
					if (Model.WindowPlacements[i].AnyVisiblePanel()) {
						Model.WindowPlacements[i].WindowWidget.Nodes.Clear();
						CloseWindow(Model.WindowPlacements[i].WindowWidget.Window);
					}
				}
				var savedPanels = Model.Panels.ToList();
				Model.WindowPlacements.Clear();
				Model.Panels.Clear();
				foreach (var windowPlacement in state.WindowPlacements) {
					Model.WindowPlacements.Add(windowPlacement);
					foreach (var panelPlacement in windowPlacement.Root.GetDescendantPanels()) {
						var p = savedPanels.FirstOrDefault(i => i.Id == panelPlacement.Title);
						if (p == null) continue;
						Model.Panels.Add(p);
					}
				}
				if (resizeMainWindow && state.WindowPlacements.First().Size != Vector2.Zero) {
					var mainWindow = Model.MainWindow;
					MainWindowWidget.Window.ClientSize = mainWindow.Size;
					MainWindowWidget.Window.ClientPosition = mainWindow.Position;
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
					WindowPlacements = WindowPlacements.Select(p => p.Clone() as WindowPlacement).ToList()
				};
			}
		}
	}
}
