using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
	public class DockDragManager
	{
		class DragHintPanel : IDockPanel
		{
			Xwt.Box box = new Xwt.HBox();

			public Xwt.Widget Widget { get { return box; } }

			public DragHintPanel()
			{
				box.MinWidth = 50;
				box.MinHeight = 50;
				box.BackgroundColor = Xwt.Drawing.Colors.LightBlue;
			}
		}

		class DragSnippet : Xwt.Window
		{
			Xwt.Label titleLabel;
			CustomCanvas canvas;

			public DragSnippet(string title)
			{
				CreateContent(title);
				UpdateLocation();
				Xwt.Application.TimeoutInvoke(10, DragHandler);
			}

			private void CreateContent(string title)
			{
				canvas = new CustomCanvas();
				this.Padding = new Xwt.WidgetSpacing();
				this.TransientFor = MainWindow.Instance.Window;
				this.Size = new Xwt.Size(100, 50);
				this.Decorated = false;
				this.Content = canvas;
				canvas.BoundsChanged += canvas_BoundsChanged;
				titleLabel = new Xwt.Label(title);
				titleLabel.TextAlignment = Xwt.Alignment.Center;
				canvas.AddChild(titleLabel);
				canvas.Drawn += canvas_Drawn;
				this.Show();
			}

			void canvas_Drawn(Xwt.Drawing.Context ctx, Xwt.Rectangle dirtyRect)
			{
				ctx.SetColor(Xwt.Drawing.Colors.White);
				ctx.Rectangle(canvas.Bounds);
				ctx.FillPreserve();
				ctx.SetColor(Xwt.Drawing.Colors.Black);
				ctx.Stroke();
			}

			void canvas_BoundsChanged(object sender, EventArgs e)
			{
				canvas.SetChildBounds(titleLabel, canvas.Bounds);
			}

			private bool DragHandler()
			{
				UpdateLocation();
				if (!XwtPlus.Utils.Instance.GetPointerButtonState(Xwt.PointerButton.Left)) {
					Hide();
					return false;
				}
				return true;
			}

			private void UpdateLocation()
			{
				Xwt.Point p = XwtPlus.Utils.Instance.GetPointerPosition();
				this.Location = p - new Xwt.Size(Size.Width / 2, Size.Height / 2);
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
			dockArea.ButtonPressed += DockArea_ButtonPressed;
		}

		void DockArea_ButtonPressed(object sender, Xwt.ButtonEventArgs e)
		{
			if (e.Button != Xwt.PointerButton.Left) {
				return;
			}
			foreach (var panel in dockManager.Panels) {
				var dockPanel = panel as DockPanel;
				if (dockPanel != null) {
					var p = dockArea.ConvertToScreenCoordinates(new Xwt.Point(e.X, e.Y));
					if (dockPanel.TitleLabel.ScreenBounds.Contains(p)) {
						StartDrag(dockPanel);
						e.Handled = true;
						break;
					}
				}
			}
		}

		private void StopDrag()
		{
			if (draggingPanel != null) {
				DropPanel(cancelOperation: false);
				dockArea.Cursor = Xwt.CursorType.Arrow;
			}
		}

		private void DropPanel(bool cancelOperation)
		{
			if (draggingPanel != null) {
				if (dragZone == DockZone.None || cancelOperation) {
					// Restore panel state as it was before dragging
					dragZone = DockZone.Left;
				}
				dockManager.RemoveDockPanel(draggingPanel);
				dockManager.AddDockPanel(draggingPanel, dragZone);
				dockManager.RemoveDockPanel(hintPanel);
				draggingPanel = null;
			}
		}

		public DockZone GetZoneForPointer(Xwt.Point ptr)
		{
			var da = dockArea;
			const double margin = 50;
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
			draggingPanel = panel;
			dockManager.RemoveDockPanel(panel);
		}

		private bool HandleDrag()
		{
			if (draggingPanel != null) {
				if (!XwtPlus.Utils.Instance.GetPointerButtonState(Xwt.PointerButton.Left)) {
					StopDrag();
					return false;
				} else {
					Xwt.Point p = XwtPlus.Utils.Instance.GetPointerPosition();
					p.X -= dockArea.ScreenBounds.Left;
					p.Y -= dockArea.ScreenBounds.Top;
					DockZone zone = GetZoneForPointer(p);
					if (zone != dragZone) {
						dragZone = zone;
						dockManager.RemoveDockPanel(hintPanel);
						if (zone != DockZone.None) {
							dockManager.AddDockPanel(hintPanel, zone);
						}
					}
				}
				return true;			
			}
			return false;
		}
	}
}
