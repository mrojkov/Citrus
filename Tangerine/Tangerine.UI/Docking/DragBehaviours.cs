using System;
using System.Collections.Generic;
using System.Linq;
using Lime;

namespace Tangerine.UI.Docking
{
	public class WindowDragBehaviour
	{
		private static DockHierarchy AppPlacement => DockHierarchy.Instance;
		private DockSite requestedSite;
		private PanelPlacement requestedPlacement;
		private Vector2 positionOffset;
		private readonly WindowPlacement windowPlacement;
		private readonly Placement placement;
		public static bool IsActive { get; private set; }
		public WindowInput Input { get; }

		private WindowDragBehaviour(Placement placement)
		{
			this.placement = placement;
			windowPlacement = DockManager.Instance.Model.GetWindowByPlacement(placement);
			((WindowWidget)WidgetContext.Current.Root).Tasks.Add(MoveTask());
			Input = CommonWindow.Current.Input;
			Application.Input.Simulator.SetKeyState(Key.Mouse0, true);
		}

		private IEnumerator<object> MoveTask()
		{
			while (true) {
				if (Input.IsMousePressed()) {
					RefreshPlacementAndSite();
					yield return null;
				} else {
					OnMouseRelease();
					yield break;
				}
			}
		}

		public static void CreateFor(Placement placement, Vector2 positionOffset)
		{
			new WindowDragBehaviour(placement) {
				positionOffset = positionOffset
			};
			IsActive = true;
		}

		private void OnMouseRelease()
		{
			ResetDockComponents();
			if (requestedSite != DockSite.None) {
				DockManager.Instance.DockPlacementTo(placement, requestedPlacement, requestedSite, 0.25f);
			}
			IsActive = false;
		}

		private IEnumerable<Panel> GetPanels()
		{
			foreach (var panel in AppPlacement.Panels) {
				var panelPlacement = DockManager.Instance.Model.FindPanelPlacement(panel.Id);
				if (!panelPlacement.Hidden &&
					panel.ContentWidget.GetRoot() is WindowWidget &&
					panelPlacement!= placement &&
					!panelPlacement.IsDescendantOf(placement)
				) {
					yield return panel;
				}
			}
		}

		private void RefreshPlacementAndSite()
		{
			var mousePosition = Application.DesktopMousePosition;
			if (Application.Platform == PlatformId.Mac) {
				mousePosition.Y -= windowPlacement.WindowWidget.Window.ClientSize.Y;
			}
			ResetDockComponents();
			var cachedSite = requestedSite;
			requestedSite = DockSite.None;
			windowPlacement.WindowWidget.Window.ClientPosition = mousePosition - positionOffset;
			foreach (var p in GetPanels()) {
				var placement = AppPlacement.FindPanelPlacement(p.Id);
				var bounds = p.ContentWidget.CalcAABBInWindowSpace();
				var winPlacement = DockManager.Instance.Model.GetWindowByPlacement(placement);
				var requestedDockingComponent = winPlacement.WindowWidget.Components.Get<RequestedDockingComponent>();
				if (requestedDockingComponent == null) continue;
				var clientMousePos = winPlacement.WindowWidget.Window.Input.MousePosition;
				if (!bounds.Contains(clientMousePos)) continue;
				CalcSiteAndRect(clientMousePos, bounds, out DockSite site, out Rectangle? rect);
				if (placement.Id == windowPlacement.Root.GetDescendantPanels().First().Id ||
					placement.Id == DockManager.DocumentAreaId &&
					site == DockSite.Fill
				) {
					site = DockSite.None;
					rect = null;
					requestedPlacement = null;
				}
				if (cachedSite != site || requestedPlacement?.Id != placement.Id) {
					InvalidateWindows();
				}
				requestedSite = site;
				requestedDockingComponent.Bounds = rect;
				requestedPlacement = placement;
				break;
			}
			if (cachedSite != requestedSite) {
				InvalidateWindows();
			}
		}

		private static void InvalidateWindows()
		{
			foreach (var win in AppPlacement.VisibleWindowPlacements) {
				win.WindowWidget.Window.Invalidate();
			}
		}

		private static void ResetDockComponents()
		{
			foreach (var win in AppPlacement.VisibleWindowPlacements) {
				win.WindowWidget.Components.Get<RequestedDockingComponent>().Bounds = null;
			}
		}

		private static void CalcSiteAndRect(Vector2 position, Rectangle originRect, out DockSite site, out Rectangle? rect)
		{
			var pos = (position - originRect.A) / originRect.Size;
			site = DockSite.None;
			const float offset = 0.25f;
			rect = new Rectangle();
			var bottomRect = new Rectangle(new Vector2(0, 1 - offset), Vector2.One);
			var leftRect = new Rectangle(Vector2.Zero, new Vector2(offset, 1));
			var rightRect = new Rectangle(new Vector2(1 - offset, 0), Vector2.One);
			var centerRect = new Rectangle(new Vector2(offset, offset), new Vector2(1 - offset, 1 - offset));
			var topRect = new Rectangle(Vector2.Zero, new Vector2(1, offset));
			if (centerRect.Contains(pos)) {
				rect = new Rectangle(Vector2.Zero, Vector2.One);
				site = DockSite.Fill;
			} else if (topRect.Contains(pos)) {
				rect = topRect;
				site = DockSite.Top;
			} else if (bottomRect.Contains(pos)) {
				rect = bottomRect;
				site = DockSite.Bottom;
			} else if (leftRect.Contains(pos)) {
				rect = leftRect;
				site = DockSite.Left;
			} else if (rightRect.Contains(pos)) {
				rect = rightRect;
				site = DockSite.Right;
			}
			rect = new Rectangle(originRect.A + originRect.Size * rect.Value.A, originRect.A + originRect.Size * rect.Value.B);
		}
	}

	public class DragBehaviour
	{
		private readonly Widget inputWidget;
		private readonly Widget contentWidget;
		private readonly Placement placement;
		private Vector2 LocalMousePosition => inputWidget.Parent.AsWidget.LocalMousePosition();
		private const float Offset = 30;
		private readonly WidgetInput input;

		public event Action<Vector2, Vector2> OnUndock;

		public DragBehaviour(Widget inputWidget, Widget contentWidget, Placement placement)
		{
			this.inputWidget = inputWidget;
			this.contentWidget = contentWidget;
			this.placement = placement;
			input = inputWidget.Input;
			inputWidget.Tasks.Add(MainTask());
		}

		private IEnumerator<object> MainTask()
		{
			while (true) {
				var pressedPosition = inputWidget.LocalMousePosition();
				if (input.WasMousePressed()) {
					if (placement.Root == placement.Parent) {
						WindowDragBehaviour.CreateFor(placement, pressedPosition);
					} else {
						var size = inputWidget.Parent.AsWidget.Size;
						var initialPosition = LocalMousePosition;
						while (input.IsMousePressed() &&
							LocalMousePosition.X > -Offset &&
							LocalMousePosition.X < size.X + Offset &&
							Mathf.Abs(LocalMousePosition.Y - initialPosition.Y) < Offset
						) {
							yield return null;
						}
						if (input.IsMousePressed()) {
							OnUndock?.Invoke(pressedPosition, Application.DesktopMousePosition - (input.MousePosition - contentWidget.GlobalPosition));
						}
					}
				}
				yield return null;
			}
		}
	}
}
