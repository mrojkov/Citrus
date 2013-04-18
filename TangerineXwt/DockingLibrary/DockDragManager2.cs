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

		private DockZone dragZone;
		private DragHintPanel hintPanel;
		private DockManager dockManager;
		private DockPanel draggingPanel;

		private Xwt.Canvas dockCanvas { get { return MainWindow.Instance.DockingCanvas; } }

		public DockDragManager(DockManager dockManager)
		{
			this.hintPanel = new DragHintPanel();
			this.dockManager = dockManager;
			dockCanvas.ButtonPressed += DockArea_ButtonPressed;
			dockCanvas.MouseMoved += MainCanvas_MouseMoved;
			dockCanvas.ButtonReleased += MainCanvas_ButtonReleased;
		}

		void DockArea_ButtonPressed(object sender, Xwt.ButtonEventArgs e)
		{
			foreach (var panel in dockManager.Panels) {
				var dockPanel = panel as DockPanel;
				if (dockPanel != null) {
					var p = dockCanvas.ConvertToScreenCoordinates(new Xwt.Point(e.X, e.Y));
					if (dockPanel.TitleLabel.ScreenBounds.Contains(p)) {
						StartDrag(dockPanel);
						break;
					}
				}
			}
		}

		void Parent_MouseExited(object sender, EventArgs e)
		{
			if (draggingPanel != null) {
				DropPanel(cancelOperation: false);
			}
		}

		void MainCanvas_MouseMoved(object sender, Xwt.MouseMovedEventArgs e)
		{
			Console.WriteLine(e.X);
			if (draggingPanel != null) {
				DockZone zone = GetZoneForPointer(new Xwt.Point(e.X, e.Y));
				if (zone != dragZone) {
					dragZone = zone;
					dockManager.RemoveDockPanel(hintPanel);
					if (zone != DockZone.None) {
						dockManager.AddDockPanel(hintPanel, zone);
					}
				}
			}
			//if (draggingSnippet != null) {
			//	var canvas = this.dockManager.MainCanvas;
			//	var b = canvas.GetChildBounds(draggingSnippet);
			//	b.Location = new Xwt.Point(e.X - draggingSnippet.Size.Width / 2, e.Y - draggingSnippet.Size.Height * 0.75);
			//	canvas.SetChildBounds(draggingSnippet, b);
			//}
		}


		void MainCanvas_ButtonReleased(object sender, Xwt.ButtonEventArgs e)
		{
			//XwtPlus.Utils.Instance.ReleaseMouse();
			if (draggingPanel != null) {
				DropPanel(cancelOperation: false);
				dockCanvas.Cursor = Xwt.CursorType.Arrow;
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

		//private void dockArea_DragLeave(object sender, EventArgs e)
		//{
		//	Console.WriteLine("Leave");
		//	lastDragZone = dragZone;
		//	dragZone = DockZone.None;
		//	dockManager.RemoveDockPanel(hintPanel);
		//}

		//void dockArea_DragDrop(object sender, Xwt.DragEventArgs e)
		//{
		//	if (lastDragZone != DockZone.None) {
		//		dockManager.AddDockPanel(draggingPanel, lastDragZone);
		//	}
		//	e.Success = true;
		//}

		//void dockArea_DragOver(object sender, Xwt.DragOverEventArgs e)
		//{
		//	DockZone zone = GetZoneForPointer(e.Position);
		//	if (zone != dragZone) {
		//		dragZone = zone;
		//		dockManager.RemoveDockPanel(hintPanel);
		//		if (zone != DockZone.None) {
		//			dockManager.AddDockPanel(hintPanel, zone);
		//		}
		//	}
		//	e.AllowedAction = Xwt.DragDropAction.Move;
		//}

		public DockZone GetZoneForPointer(Xwt.Point ptr)
		{
			var da = dockCanvas;
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

		//private static Xwt.Drawing.Image CreateDraggingSnippet(string title)
		//{
		//	using (var tl = new Xwt.Drawing.TextLayout()) {
		//		tl.Width = 100;
		//		tl.Text = title;
		//		var textSize = tl.GetSize();
		//		using (var ib = new Xwt.Drawing.ImageBuilder(textSize.Width + 20, textSize.Height + 20)) {
		//			tl.Width = ib.Width;
		//			tl.Height = ib.Height;
		//			var ctx = ib.Context;
		//			ctx.SetColor(Xwt.Drawing.Colors.White);
		//			ctx.Rectangle(0, 0, ib.Width, ib.Height);
		//			ctx.FillPreserve();
		//			ctx.SetColor(Xwt.Drawing.Colors.Gray);
		//			ctx.Stroke();
		//			ctx.SetColor(Xwt.Drawing.Colors.Black);
		//			var textPos = new Xwt.Point {
		//				X = (ib.Width - textSize.Width) / 2,
		//				Y = (ib.Height - textSize.Height) * 0.4
		//			};
		//			ctx.DrawTextLayout(tl, textPos);
		//			return ib.ToBitmap();
		//		}
		//	}
		//}

		public void StartDrag(DockPanel panel)
		{
			//hintPanel.Widget.MinWidth = 100;// panel.Widget.Size.Width;
			//hintPanel.Widget.MinHeight = 100;// panel.Widget.Size.Height;
			draggingPanel = panel;
			dockManager.RemoveDockPanel(panel);

			//this.dockManager.DockArea.MouseMoved -= MainCanvas_MouseMoved;
			//this.dockManager.DockArea.ButtonReleased -= MainCanvas_ButtonReleased;

			XwtPlus.Utils.Instance.CaptureMouse(dockManager.DockArea);
		
			//var image = CreateDraggingSnippet(panel.Title);

			//var box = new Xwt.HBox();
			//var img = new Xwt.ImageView(image);
			//box.PackStart(img, Xwt.BoxMode.Expand);
			//draggingSnippet = box;

			//var canvas = dockManager.MainCanvas;
			//var children = canvas.Children.ToArray();
			//foreach (var child in children) {
			//	canvas.RemoveChild(child);
			//}
			//dockManager.DockArea.Cursor = Xwt.CursorType.Crosshair;
			//draggingSnippet.ConvertToScreenCoordinates
			//foreach (var child in children) {
			//	canvas.AddChild(child);
			//}
			//canvas.AddChild(draggingSnippet);

			//d.AllowedActions = Xwt.DragDropAction.Move;
			//d.Finished += operation_Finished;
			//d.Start();
		}
	}
}
