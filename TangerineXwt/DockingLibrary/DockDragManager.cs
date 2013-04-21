using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
	public class DockDragManager
	{
		class DragHintPanel : IDockElement
		{
			Xwt.Box box = new Xwt.HBox();

			public Xwt.Widget MainWidget { get { return box; } }

			public DragHintPanel()
			{
				box.MinWidth = 50;
				box.MinHeight = 50;
				box.BackgroundColor = Xwt.Drawing.Colors.LightBlue;
			}
		}

		private DockZone dragZone;
		private DragHintPanel hintPanel;
		private DockManager dockManager;
		private DockPanel draggingPanel;

		private Xwt.HBox dockArea { get { return MainWindow.Instance.DockingArea; } }

		public DockDragManager(DockManager dockManager)
		{
			this.hintPanel = new DragHintPanel();
			this.dockManager = dockManager;
		}

		private void StopDrag()
		{
			if (draggingPanel != null) {
				DropPanel();
				dockArea.Cursor = Xwt.CursorType.Arrow;
			}
		}

		private void DropPanel()
		{
			if (draggingPanel != null) {
				if (dragZone == DockZone.None) {
					draggingPanel.Floating(atMouse: true);
				} else {
					draggingPanel.Dock(dragZone);
				}
				dockManager.RemoveDockElement(hintPanel);
				draggingPanel = null;
			}
		}

		public DockZone GetZoneForPointer(Xwt.Point ptr)
		{
			var da = dockArea;
			const double margin = 50;
			if (!dockArea.ScreenBounds.Contains(ptr)) {
				return DockZone.None;
			}
			ptr.X -= dockArea.ScreenBounds.Left;
			ptr.Y -= dockArea.ScreenBounds.Top;
			if (ptr.X < margin) {
				return DockZone.Left;
			} else if (ptr.X > da.Size.Width - margin) {
				return DockZone.Right;
			} else if (ptr.Y < margin) {
				return DockZone.Top;
			} else if (ptr.Y > da.Size.Height - margin) {
				return DockZone.Bottom;
			}
			return DockZone.None;
		}

		public void StartDrag(DockPanel panel)
		{
			var snippet = new DragSnippet(panel.Title);
			Xwt.Application.TimeoutInvoke(10, HandleDrag);
			panel.Hide();
			draggingPanel = panel;
		}

		private bool HandleDrag()
		{
			if (draggingPanel != null) {
				if (!XwtPlus.Utils.Instance.GetPointerButtonState(Xwt.PointerButton.Left)) {
					StopDrag();
					return false;
				} else {
					Xwt.Point p = XwtPlus.Utils.Instance.GetPointerPosition();
					DockZone zone = GetZoneForPointer(p);
					if (zone != dragZone) {
						dragZone = zone;
						dockManager.RemoveDockElement(hintPanel);
						if (zone != DockZone.None) {
							dockManager.AddDockElement(hintPanel, zone);
						}
					}
				}
				return true;			
			}
			return false;
		}
	}
}
