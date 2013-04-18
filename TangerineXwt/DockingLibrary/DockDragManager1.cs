//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Tangerine
//{
//	public class DockDragManager
//	{
//		[Serializable]
//		class DockPanelDnD
//		{
//		}

//		class DragHintPanel : IDockPanel
//		{
//			Xwt.Box box = new Xwt.HBox();

//			public Xwt.Widget Widget { get { return box; } }

//			public DragHintPanel()
//			{
//				box.MinWidth = 50;
//				box.MinHeight = 50;
//				box.BackgroundColor = Xwt.Drawing.Colors.LightBlue;
//			}
//		}

//		private DockZone dragZone;
//		private DockZone lastDragZone;
//		private DragHintPanel hintPanel;
//		private DockManager dockManager;
//		private DockPanel draggingPanel;

//		public DockDragManager(DockManager dockManager)
//		{
//			hintPanel = new DragHintPanel();
//			this.dockManager = dockManager;
//			var da = dockManager.DockArea;
//			da.SetDragDropTarget(typeof(DockPanelDnD));
//			da.DragOver += dockArea_DragOver;
//			da.DragDrop += dockArea_DragDrop;
//			da.DragLeave += dockArea_DragLeave;
//		}

//		private void dockArea_DragLeave(object sender, EventArgs e)
//		{
//			Console.WriteLine("Leave");
//			lastDragZone = dragZone;
//			dragZone = DockZone.None;
//			//dockManager.RemoveDockPanel(hintPanel);
//		}

//		void dockArea_DragDrop(object sender, Xwt.DragEventArgs e)
//		{
//			if (lastDragZone != DockZone.None) {
//				dockManager.AddDockPanel(draggingPanel, lastDragZone);
//			}
//			e.Success = true;
//		}

//		void dockArea_DragOver(object sender, Xwt.DragOverEventArgs e)
//		{
//			DockZone zone = GetZoneForPointer(e.Position);
//			if (zone != dragZone) {
//				dragZone = zone;
//				dockManager.RemoveDockPanel(hintPanel);
//				if (zone != DockZone.None) {
//					dockManager.AddDockPanel(hintPanel, zone);
//				}
//			}
//			e.AllowedAction = Xwt.DragDropAction.Move;
//		}

//		public DockZone GetZoneForPointer(Xwt.Point ptr)
//		{
//			var da = dockManager.DockArea;
//			const double margin = 50;
//			if (ptr.X < margin) {
//				return DockZone.Left;
//			} else if (ptr.X > da.Size.Width - margin) {
//				return DockZone.Right;
//			} else if (ptr.Y < margin) {
//				return DockZone.Top;
//			} else if (ptr.Y > da.Size.Height - margin) {
//				return DockZone.Bottom;
//			}
//			return DockZone.None;
//		}

//		private static Xwt.Drawing.Image CreateDraggingSnippet(string title)
//		{
//			using (var tl = new Xwt.Drawing.TextLayout()) {
//				tl.Width = 100;
//				tl.Text = title;
//				var textSize = tl.GetSize();
//				using (var ib = new Xwt.Drawing.ImageBuilder(textSize.Width + 20, textSize.Height + 20)) {
//					tl.Width = ib.Width;
//					tl.Height = ib.Height;
//					var ctx = ib.Context;
//					ctx.SetColor(Xwt.Drawing.Colors.White);
//					ctx.Rectangle(0, 0, ib.Width, ib.Height);
//					ctx.FillPreserve();
//					ctx.SetColor(Xwt.Drawing.Colors.Gray);
//					ctx.Stroke();
//					ctx.SetColor(Xwt.Drawing.Colors.Black);
//					var textPos = new Xwt.Point {
//						X = (ib.Width - textSize.Width) / 2,
//						Y = (ib.Height - textSize.Height) * 0.4
//					};
//					ctx.DrawTextLayout(tl, textPos);
//					return ib.ToBitmap();
//				}
//			}
//		}

//		public void StartDrag(DockPanel panel)
//		{
//			//hintPanel.Widget.MinWidth = 100;// panel.Widget.Size.Width;
//			//hintPanel.Widget.MinHeight = 100;// panel.Widget.Size.Height;
//			draggingPanel = panel;
//			dragZone = DockZone.None;
//			var d = dockManager.DockArea.CreateDragOperation();
//			d.Data.AddValue(new DockPanelDnD());
//			dockManager.RemoveDockPanel(panel);
//			var image = CreateDraggingSnippet(panel.Title);
//			d.SetDragImage(image, (int)image.Size.Width / 2, (int)image.Size.Height * 0.75);
//			d.AllowedActions = Xwt.DragDropAction.Move;
//			d.Finished += operation_Finished;
//			d.Start();
//		}

//		void operation_Finished(object sender, Xwt.DragFinishedEventArgs e)
//		{
//			if (!draggingPanel.Docked) {
//				// Restore panel state as it was before dragging
//				dockManager.AddDockPanel(draggingPanel, DockZone.Left);
//			}
//			dockManager.RemoveDockPanel(hintPanel);
//			draggingPanel = null;
//		}
//	}
//}
