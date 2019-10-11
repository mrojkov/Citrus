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
		private const float panelMinWidth = 50f;
		private readonly List<FilesDropManager> filesDropManagers = new List<FilesDropManager>();

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
				Layout = new VBoxLayout { Spacing = 4 },
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
			MainWindowWidget.Components.Add(new RequestedDockingComponent());
			MainWindowWidget.CompoundPostPresenter.Add(new DockingPresenter());
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
			if (placement == windowPlacement) {
				if (windowPlacement.Placements.Count == 1) {
					placement = windowPlacement.Placements[0];
				} else {
					var linearPlacement = new LinearPlacement(windowPlacement.Direction);
					for (var i = 0; i < windowPlacement.Placements.Count; i++) {
						linearPlacement.Placements.Add(windowPlacement.Placements[i].Clone());
						linearPlacement.Stretches.Add(windowPlacement.Stretches[i]);
					}
					placement = linearPlacement;
				}
				windowPlacement.Placements.Clear();
			}
			placement.Unlink();
			Model.DockPlacementTo(placement, target, site, stretch);
			if (windowPlacement != null) {
				windowPlacement.Root.RemoveRedundantNodes();
				if (!windowPlacement.Root.GetPanelPlacements().Any()) {
					Model.RemoveWindow(windowPlacement);
					CloseWindow(windowPlacement.WindowWidget.Window);
				} else {
					RefreshWindow(windowPlacement);
				}
				RefreshWindow(Model.GetWindowByPlacement(target));
			}
		}

		private void SetDropHandler(IWindow window)
		{
			window.AllowDropFiles = true;
			window.FilesDropped += OnFilesDropped;
		}

		private void OnFilesDropped(IEnumerable<string> files)
		{
			FilesDropped?.Invoke(files);
			foreach (var filesDropManager in filesDropManagers) {
				if (filesDropManager.TryToHandle(files)) {
					break;
				}
			}
		}

		public PanelPlacement AddPanel(Panel panel, Placement targetPlacement, DockSite site, float stretch = 0.25f)
		{
			return Model.AddPanel(panel, targetPlacement, site, stretch);
		}

		public PanelPlacement AppendPanelTo(Panel panel, LinearPlacement targetPlacement, float stretch = 0.25f)
		{
			return Model.AppendPanelToPlacement(panel, targetPlacement, stretch);
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

		private void ResolveInconsistency()
		{
			var floatingWindowDefaultSize = new Vector2(200, 300);
			var descendantPanelsList = GetDescendantPanelPlacements().ToList();
			// Remove all placements which don't refer to existing panels
			foreach (var panel in descendantPanelsList.ToList()) {
				if (Model.Panels.Any(p => p.Id == panel.Id)) {
					continue;
				}
				var windowPlacement = Model.GetWindowByPlacement(panel);
				windowPlacement.FindPanelPlacement(panel.Id).Unlink();
				descendantPanelsList.Remove(panel);
				windowPlacement.Root.RemoveRedundantNodes();
				if (!windowPlacement.GetPanelPlacements().Any()) {
					Model.WindowPlacements.Remove(windowPlacement);
				}
			}
			foreach (var panel in Model.Panels) {
				// If there is no placement for panel, create new one in new window and hide.
				if (descendantPanelsList.Any(placement => placement.Id == panel.Id)) {
					continue;
				}
				var windowPlacement = new WindowPlacement {
					Position = Model.WindowPlacements[0].Position + Model.WindowPlacements[0].Size / 2 - floatingWindowDefaultSize / 2,
					Size = floatingWindowDefaultSize
				};
				windowPlacement.Placements.Add(
					new PanelPlacement {
						Title = panel.Title,
						Id = panel.Id,
						Hidden = true
					}
				);
				windowPlacement.Stretches.Add(1);
				Model.WindowPlacements.Add(windowPlacement);
			}
		}

		public IEnumerable<PanelPlacement> GetDescendantPanelPlacements() {
			foreach (var windowPlacement in Model.WindowPlacements) {
				foreach (var panelPlacement in windowPlacement.GetPanelPlacements()) {
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
			foreach (var placement in Model.WindowPlacements) {
				if (!placement.Root.AnyVisiblePanel()) {
					continue;
				}
				if (placement.WindowWidget != null) continue;
				if (placement == Model.WindowPlacements[0]) {
					placement.WindowWidget = MainWindowWidget;
				} else {
					CreateFloatingWindow(placement);
				}
			}
		}

		private void CreateWidgetForPlacement(Splitter container, Placement placement, float stretch = 1, bool isMainWindow = false)
		{
			if (placement == null || !placement.AnyVisiblePanel()) {
				return;
			}
			bool requestTitle = placement is WindowPlacement windowPlacement &&
				windowPlacement.GetVisiblePanelPlacements().Count() > 1 &&
				!isMainWindow;
			placement.SwitchType(
				panel => CreatePanelWidget(container, panel, stretch),
				panelGroup => {
					if (panelGroup.GetPanelPlacements().Count(p => !p.Hidden) == 1) {
						CreatePanelWidget(container, panelGroup.GetPanelPlacements().First(p => !p.Hidden), stretch);
					} else {
						CreateTabBarWidget(container, panelGroup, stretch, !requestTitle);
					}
				},
				group => CreateLinearWidget(container, group, stretch, requestTitle)
			);
		}

		private void CreateLinearWidget(Splitter container, LinearPlacement placement, float stretch, bool requestTitle)
		{
			var splitter = placement.Direction == LinearPlacementDirection.Vertical
				? (Splitter)new VSplitter()
				: new HSplitter();
			splitter.SeparatorColor = Theme.Colors.SeparatorColor;
			splitter.SeparatorHighlightColor = Theme.Colors.SeparatorHighlightColor;
			splitter.SeparatorDragColor = Theme.Colors.SeparatorDragColor;
			splitter.SeparatorWidth = 5f;
			splitter.SeparatorActiveAreaWidth = 6f;
			Widget rootWidget = splitter;
			bool hasTabBarPlacement = false;
			// Due to the mechanism of docking size refreshing, Placements[i] does not
			// correspond to Stretches[i]. In fact, in Stretches elements ordered by visibility and
			// location in their splitter. Visible widgets at first, hidden at last.
			for (int i = 0, j = 0; i < placement.Placements.Count; ++i) {
				if (!placement.Placements[i].AnyVisiblePanel()) {
					continue;
				}
				hasTabBarPlacement |= placement.Placements[i] is TabBarPlacement;
				CreateWidgetForPlacement(splitter, placement.Placements[i], placement.Stretches[j]);
				j++;
			}
			splitter.DragEnded += () => RefreshDockedSize(placement, splitter);
			if (requestTitle && !(hasTabBarPlacement && placement.Placements.Count == 1)) {
				rootWidget = WrapWithTitle(rootWidget, out var closeButton, out var title);
				closeButton.Clicked += () => {
					placement.HideThisOrDescendantPanels();
					Refresh();
				};
				title.Text = "Tangerine";
				CreateDragBehaviour(placement, rootWidget.Nodes[0].AsWidget, rootWidget);
			}
			container.Stretches.Add(stretch);
			container.Nodes.Add(rootWidget);
		}

		private static Widget WrapWithTitle(Widget widget, out ThemedTabCloseButton closeButton, out ThemedSimpleText title, string id = null)
		{
			Widget titleWidget;
			var result =  new Widget {
				Id = id,
				LayoutCell = new LayoutCell(),
				Layout = new VBoxLayout(),
				MinWidth = panelMinWidth,
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
					widget
				}
			};
			// Add space between close button and titleWidget right border.
			title.Padding = title.Padding + new Thickness(right: 5);
			titleWidget.CompoundPresenter.Add(new SyncDelegatePresenter<Widget>(w => {
				w.PrepareRendererState();
				Renderer.DrawRect(Vector2.Zero, w.Size, ColorTheme.Current.Docking.PanelTitleBackground);
				Renderer.DrawLine(0, w.Height - 0.5f, w.Width, w.Height - 0.5f, ColorTheme.Current.Docking.PanelTitleSeparator);
			}));
			widget.ExpandToContainerWithAnchors();
			result.FocusScope = new KeyboardFocusScope(result);
			return result;
		}

		private void CreatePanelWidget(Splitter container, PanelPlacement placement, float stretch)
		{
			var panel = Model.Panels.First(pan => pan.Id == placement.Id);
			var widget = panel.ContentWidget;
			widget.Unlink();
			if (Model.FindPanelPlacement(panel.Id).Hidden) {
				return;
			}
			if (panel.IsUndockable) {
				// Wrap widget to clip children.
				var frame = new Widget {
					Nodes = { widget }
				};
				widget.ExpandToContainerWithAnchors();
				var rootWidget = WrapWithTitle(frame, out var closeButton, out var titleLabel, $"DockPanel<{panel.Id}>");
				titleLabel.AddChangeWatcher(() => panel.Title, text => titleLabel.Text = text);
				closeButton.Clicked += () => {
					Model.FindPanelPlacement(panel.Id).Hidden = true;
					Refresh();
				};
				CreateDragBehaviour(placement, rootWidget.Nodes[0].AsWidget, widget);
				widget = rootWidget;
			}
			panel.PanelWidget = widget;
			container.Nodes.Add(widget);
			container.Stretches.Add(stretch);
		}

		private void CreateTabBarWidget(Splitter container, TabBarPlacement placement, float stretch, bool requestTitle)
		{
			var tabbedWidget = new TabbedWidget {
				TabBar = new PanelTabBar(),
				AllowReordering = true,
				BarPlacement = TabbedWidget.TabBarPlacement.Bottom,
				PostPresenter = new SyncDelegatePresenter<TabbedWidget>(w => {
					w.PrepareRendererState();
					var activeTab = (Tab)w.TabBar.Nodes[w.ActiveTabIndex];
					var color = ColorTheme.Current.Docking.TabOutline;
					var a = activeTab.CalcPositionInSpaceOf(w);
					var start = a * Vector2.Down;
					var b = a + Vector2.Down * activeTab.ContentHeight;
					var c = b + activeTab.ContentWidth * Vector2.Right;
					var d = c - Vector2.Down * activeTab.ContentHeight;
					var end = start + w.Size * Vector2.Right;
					Renderer.DrawLine(start, a + Vector2.Right * 0.5f, color);
					Renderer.DrawLine(a, b, color);
					Renderer.DrawLine(b + new Vector2(-0.5f, -0.5f), c + new Vector2(0.5f, -0.5f), color);
					Renderer.DrawLine(c, d, color);
					Renderer.DrawLine(d + Vector2.Left * 0.5f, end, color);
				})
			};
			Widget rootWidget = tabbedWidget;
			tabbedWidget.FocusScope = new KeyboardFocusScope(tabbedWidget);
			tabbedWidget.TabBar.OnReorder += args => placement.Placements.Move(args.IndexFrom, args.IndexTo);
			if (requestTitle) {
				rootWidget = WrapWithTitle(tabbedWidget, out var closeButton, out var titleLabel);
				titleLabel.AddChangeWatcher(
					() => ((Tab)tabbedWidget.TabBar.Nodes[tabbedWidget.ActiveTabIndex]).Text,
					title => titleLabel.Text = title
				);
				closeButton.Clicked += () => {
					placement.Placements.Where(p => !p.Hidden).ElementAt(tabbedWidget.ActiveTabIndex).Hidden = true;
					Refresh();
				};
				CreateDragBehaviour(placement, rootWidget.Nodes[0].AsWidget, rootWidget);
			}
			foreach (var panelPlacement in placement.Placements) {
				var panel = Model.Panels.First(pan => pan.Id == panelPlacement.Id);
				panel.ContentWidget.Unlink();
				if (panelPlacement.Hidden) {
					continue;
				}
				var tab = new PanelTab {
					Text = panel.Id,
					Closable = false,
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
				panel.PanelWidget = rootWidget;
			}
			tabbedWidget.ActiveTabIndex = 0;
			container.Nodes.Add(rootWidget);
			container.Stretches.Add(stretch);
		}

		private void CreateDragBehaviour(Placement placement, Widget inputWidget, Widget contentWidget)
		{
			var db = new DragBehaviour(inputWidget, contentWidget, placement);
			db.OnUndock += (positionOffset, windowPosition) => {
				var panelWindow = (WindowWidget)contentWidget.GetRoot();
				var windowPlacement = Model.GetWindowByPlacement(placement);
				if (windowPlacement.Root.GetPanelPlacements().Count(p => !p.Hidden) > 1) {
					var globalSize = placement.CalcGlobalStretch() * panelWindow.Size;
					var wrapper = new WindowPlacement {
						Size = new Vector2(Mathf.Max(panelMinWidth, globalSize.X), globalSize.Y)
					};
					placement.Unlink();
					windowPlacement.Root.RemoveRedundantNodes();
					Model.AddWindow(wrapper);
					wrapper.Placements.Add(placement);
					wrapper.Stretches.Add(1);
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
					DockHierarchy.HideWindowPanels(windowPlacement);
				}
				return true;
			};
			window.ClientSize = placement.Size;
			var windowWidget = new ThemedInvalidableWindowWidget(window) {
				Layout = new StackLayout(),
			};
			windowWidget.CompoundPostPresenter.Add(new WidgetBoundsPresenter(ColorTheme.Current.Docking.PanelTitleSeparator, 1));
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
			Splitter currentContainer = new ThemedVSplitter { Anchors = Anchors.LeftRightTopBottom };
			// Cache window state before resizing. Resizing may trigger Resize Update which in turn
			// lead to placement state rewriting on first update, because window state change watcher don't has a value.
			var savedState = root.State;
			windowWidget.Window.ClientPosition = root.Position;
			windowWidget.Window.ClientSize = root.Size;
			windowWidget.Window.State = savedState;
			windowWidget.Nodes.Add(currentContainer);
			CreateWidgetForPlacement(currentContainer, root.Root, isMainWindow: MainWindowWidget == windowWidget);
		}

		private static void RefreshDockedSize(LinearPlacement placement, Splitter splitter)
		{
			for (int i = 0; i < splitter.Nodes.Count; ++i) {
				var stretch = splitter.Nodes[i].AsWidget.LayoutCell.Stretch;
				placement.Stretches[i] = placement.Direction == LinearPlacementDirection.Horizontal
					? stretch.X
					: stretch.Y;
			}
			float total = placement.Stretches.Aggregate((s1, s2) => s1 + s2);
			for (int i = 0; i < placement.Stretches.Count; ++i) {
				placement.Stretches[i] /= total;
			}
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

		public void AddFilesDropManager(FilesDropManager filesDropManager) => filesDropManagers.Add(filesDropManager);
		public void RemoveFilesDropManager(FilesDropManager filesDropManager) => filesDropManagers.Remove(filesDropManager);

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
					throw new DockingException("Unable to import state: the number of windows is zero");
				}
				foreach (var wp in Model.WindowPlacements.Skip(1)) {
					if (wp.AnyVisiblePanel()) {
						wp.WindowWidget.Nodes.Clear();
						CloseWindow(wp.WindowWidget.Window);
					}
				}
				var savedPanels = Model.Panels.ToList();
				Model.WindowPlacements.Clear();
				Model.Panels.Clear();
				foreach (var windowPlacement in state.WindowPlacements) {
					Model.WindowPlacements.Add(windowPlacement);
					foreach (var panelPlacement in windowPlacement.Root.GetPanelPlacements()) {
						var p = savedPanels.FirstOrDefault(i => i.Id == panelPlacement.Title);
						if (p == null) continue;
						Model.Panels.Add(p);
					}
				}
				Model.Panels.AddRange(savedPanels.Except(Model.Panels));
				ResolveInconsistency();
			} catch (DockingException e) {
				LoadDefaultLayoutWithWarning(e.Message);
			} catch {
				LoadDefaultLayoutWithWarning("Unable to load dock state from user preferences");
			}
			Refresh();
		}

		private static void LoadDefaultLayoutWithWarning(string message)
		{
			System.Console.WriteLine($"Warning: { message }");
			CommandQueue.Instance.Add((Command)GenericCommands.DefaultLayout);
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

		[YuzuDontGenerateDeserializer]
		private class PanelTabBar : TabBar
		{
			public PanelTabBar()
			{
				Layout = new HBoxLayout();
				Padding = new Thickness { Bottom = 10f };
			}
		}

		[YuzuDontGenerateDeserializer]
		private class PanelTab : Tab
		{
			private const float TabHeight = 20f;
			private const float TabMaxWidth = 100f;
			private const float TabMinWidth = 50f;
			public override bool IsNotDecorated() => false;

			public PanelTab()
			{
				MinSize = new Vector2(TabMinWidth, TabHeight);
				MaxSize = new Vector2(TabMaxWidth, TabHeight);
				LayoutCell = new LayoutCell(Alignment.LeftTop);
				Size = MinSize;
				Layout = new HBoxLayout();
				var caption = new SimpleText {
					Id = "TextPresenter",
					Padding = Theme.Metrics.ControlsPadding,
					ForceUncutText = false,
					FontHeight = Theme.Metrics.TextHeight,
					HAlignment = HAlignment.Center,
					VAlignment = VAlignment.Center,
					OverflowMode = TextOverflowMode.Ellipsis,
					LayoutCell = new LayoutCell(Alignment.Center),
				};
				var presenter = new TabPresenter(caption);
				CompoundPresenter.Add(presenter);
				DefaultAnimation.AnimationEngine = new AnimationEngineDelegate {
					OnRunAnimation = (animation, markerId, animationTimeCorrection) => {
						presenter.SetState(markerId);
						return true;
					}
				};
				AddNode(caption);
				HitTestTarget = true;
				LateTasks.Add(Theme.MouseHoverInvalidationTask(this));
				this.AddChangeWatcher(() => Active,
					isActive => Padding = isActive ? new Thickness { Top = -1f, Bottom = 1f} : new Thickness());
			}

			private class TabPresenter : SyncCustomPresenter<Widget>
			{
				private readonly SimpleText label;
				private bool active;

				public TabPresenter(SimpleText label) { this.label = label; }

				public void SetState(string state)
				{
					CommonWindow.Current.Invalidate();
					active = state == "Active";
					label.Color = active ? Theme.Colors.BlackText : Theme.Colors.GrayText;
				}

				protected override bool InternalPartialHitTest(Widget node, ref HitTestArgs args)
				{
					return node.BoundingRectHitTest(args.Point);
				}

				protected override void InternalRender(Widget widget)
				{
					widget.PrepareRendererState();
					var theme = ColorTheme.Current.Docking;
					var color = active ? theme.TabBarBackground :
						widget.IsMouseOverThisOrDescendant() ? theme.TabHighlighted : theme.TabBarBackground;
					Renderer.DrawRect(widget.ContentPosition, widget.ContentPosition + widget.ContentSize, color);
				}
			}
		}

		private class DockingPresenter : SyncDelegatePresenter<Widget>
		{
			public DockingPresenter() : base(Render) { }

			private static void Render(Widget widget)
			{
				var comp = widget.Components.Get<RequestedDockingComponent>();
				if (!comp.Bounds.HasValue) return;
				widget.PrepareRendererState();
				Renderer.DrawRectOutline(comp.Bounds.Value.A + Vector2.One, comp.Bounds.Value.B - Vector2.One, ColorTheme.Current.Docking.DragRectagleOutline, 2);
				Renderer.DrawRect(comp.Bounds.Value.A + Vector2.One, comp.Bounds.Value.B - Vector2.One, ColorTheme.Current.Docking.DragRectagleOutline.Transparentify(0.8f));
			}
		}

		public class DockingException : Lime.Exception
		{
			public DockingException(string message) : base(message) { }
		}
	}
}
