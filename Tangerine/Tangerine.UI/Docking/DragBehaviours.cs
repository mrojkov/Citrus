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
		private readonly PanelPlacement panelPlacement;
		public static bool IsActive { get; private set; }
		public WindowInput Input { get; }

		private WindowDragBehaviour(string panelId)
		{
			panelPlacement = DockManager.Instance.Model.FindPanelPlacement(panelId);
			windowPlacement = DockManager.Instance.Model.GetWindowByPlacement(panelPlacement);
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

		public static void CreateFor(string panelId, Vector2 positionOffset)
		{
			new WindowDragBehaviour(panelId) {
				positionOffset = positionOffset
			};
			IsActive = true;
		}

		private void OnMouseRelease()
		{
			ResetDockComponents();
			if (requestedSite != DockSite.None) {
				DockManager.Instance.DockPanelTo(panelPlacement, requestedPlacement, requestedSite, 0.25f);
			}
			IsActive = false;
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
			var placements = AppPlacement.Panels.Where(pan => !DockManager.Instance.Model.FindPanelPlacement(pan.Id).Hidden && pan.ContentWidget.Parent != null);
			foreach (var p in placements) {
				var placement = AppPlacement.FindPanelPlacement(p.Id);
				var bounds = p.ContentWidget.CalcAABBInWindowSpace();
				var winPlacement = DockManager.Instance.Model.GetWindowByPlacement(placement);
				var requestedDockingComponent = winPlacement.WindowWidget.Components.Get<RequestedDockingComponent>();
				if (requestedDockingComponent == null) continue;
				var clientMousePos = winPlacement.WindowWidget.Window.Input.MousePosition;
				if (!bounds.Contains(clientMousePos)) continue;
				DockSite site;
				Rectangle? rect;
				CalcSiteAndRect(clientMousePos, bounds, out site, out rect);
				if (placement.Id == windowPlacement.Root.DescendantPanels().First().Id ||
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

	public class PanelDragBehaviour
	{
		private readonly Widget inputWidget;
		private Vector2 LocalMousePosition => inputWidget.Parent.AsWidget.LocalMousePosition();
		private const float Offset = 30;
		private readonly Panel panel;
		private readonly WidgetInput input;

		public event Action<Vector2, Vector2> OnUndock;

		public PanelDragBehaviour(Widget inputWidget, Panel panel)
		{
			this.panel = panel;
			this.inputWidget = inputWidget;
			input = inputWidget.Input;
			inputWidget.Tasks.Add(MainTask());
		}

		private IEnumerator<object> MainTask()
		{
			while (true) {
				var pressedPosition = inputWidget.LocalMousePosition();
				if (input.WasMousePressed()) {
					if (DockManager.Instance.Model.IsPanelSingleInWindow(panel.Id)) {
						WindowDragBehaviour.CreateFor(panel.Id, pressedPosition);
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
							OnUndock?.Invoke(pressedPosition, Application.DesktopMousePosition - (input.MousePosition - panel.ContentWidget.GlobalPosition));
						}
					}
				}
				yield return null;
			}
		}
	}
}
