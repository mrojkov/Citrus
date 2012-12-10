using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gtk;
using Gdk;
using Mono.Unix;
using MonoDevelop.Components.Docking;
using Pinta.Core;

namespace Pinta
{
	public static class Colors
	{
		public static Gdk.Color Document = new Gdk.Color(255, 255, 255);
		public static Gdk.Color DocumentOutline = new Gdk.Color(128, 128, 128);
		public static Gdk.Color SelectedItem = new Gdk.Color(128, 128, 255);
	}

	public class TimelinePad : IDockPad
	{
		public int RowHeight = 30;
		public int TopRow;
//		public List<Lime.Node> Nodes = new List<Lime.Node>();

		public static TimelinePad Instance;

		public void Initialize(DockFrame workspace, Menu padMenu)
		{
			Instance = this;
			var timeline = new Gtk.ScrolledWindow();
			//timeline.SetSizeRequest(200, 200);

			DockItem timelineItem = workspace.AddItem("Timeline");
			timelineItem.Content = timeline;
			timelineItem.Label = Catalog.GetString("Timeline");

			Gtk.ToggleAction showTimeline = padMenu.AppendToggleAction("Timeline", Catalog.GetString("Timeline"), null, "Menu.Layers.MergeLayerDown.png");
			showTimeline.Activated += delegate { timelineItem.Visible = showTimeline.Active; };
			timelineItem.VisibleChanged += delegate { showTimeline.Active = timelineItem.Visible; };

			timelineItem.Icon = PintaCore.Resources.GetIcon("Menu.Layers.MergeLayerDown.png");

			var pane1 = new Gtk.VPaned();
			timeline.Add(pane1);
			pane1.Position = 50;

			var pane2 = new Gtk.HPaned();
			pane1.Add1(CreatePanViewPane());
			pane1.Add2(pane2);
			pane2.Position = 200;
			
			var vbox1 = new Gtk.VBox(false, 0);
			pane2.Add1(vbox1);
			vbox1.PackStart(new TimelineToolbar(), false, true, 0);
			vbox1.PackStart(new TimelineTreeView(), true, true, 0);

			var vbox2 = new Gtk.VBox(false, 0);
			pane2.Add2(vbox2);
			var ruler = CreateTimelineRuler();
			vbox2.PackStart(ruler, false, true, 0);
			vbox2.PackStart(CreateNodesPane(), true, true, 0);
	
			pane1.ShowAll();
			//PintaCore.Workspace.ActiveDocumentChanged += delegate { layers.Reset(); };
		}

		class TimelineToolbar : HBox
		{
			ToolButton eyeButton;
			ToolButton lockButton;

			public TimelineToolbar()
			{
				var item1 = new ToolBarToggleButton("Menu.Layers.MergeLayerDown.png", null, "");
				PackStart(item1, false, false, 0);

				eyeButton = new ToolBarButton("Menu.Layers.AddNewLayer.png", null, "");
				PackEnd(eyeButton, false, false, 0);

				lockButton = new ToolBarButton("Menu.Layers.AddNewLayer.png", null, "");
				PackEnd(lockButton, false, false, 0);
				ShowAll();
			}
		}

		class TimelineTreeviewItem : HBox
		{
			public Lime.Node Node { get; private set; }

			public TimelineTreeviewItem(Lime.Node node)
			{
				RedrawOnAllocate = true;

				this.Node = node;
				this.HeightRequest = TimelinePad.Instance.RowHeight;

				var whitespace = new ToolBarLabel("");
				PackStart(whitespace, false, false, 10);

				var item1 = new ToolBarImage("Menu.Layers.MergeLayerDown.png");
				PackStart(item1, false, false, 2);

				var label = new ToolBarLabel(node.Id);
				PackStart(label, false, false, 0);

				var eyeButton = new ToolBarButton("Menu.Layers.AddNewLayer.png", null, "");
				PackEnd(eyeButton, false, false, 0);

				var lockButton = new ToolBarButton("Menu.Layers.AddNewLayer.png", null, "");
				PackEnd(lockButton, false, false, 0);
			}

			protected override bool OnExposeEvent(EventExpose e)
			{
				var gc = new Gdk.GC(GdkWindow);
				var a = Allocation;
				bool selected = PintaCore.Workspace.ActiveDocument.Selection.Contains(Node);
				if (selected) {
					gc.RgbFgColor = Colors.SelectedItem;
					GdkWindow.DrawRectangle(gc, true, a.X, a.Y + 1, a.Width, a.Height);
				}
				gc.RgbFgColor = Colors.DocumentOutline;
				GdkWindow.DrawLine(gc, a.Left + 2, a.Bottom, a.Right - 2, a.Bottom);
				base.OnExposeEvent(e);
				return true;
			}
		}

		class TimelineTreeView : EventBox
		{
			VBox vbox = new VBox();
			Viewport viewport = new Viewport();

			public TimelineTreeView()
			{
				viewport.ShadowType = ShadowType.None;
				viewport.ModifyBg(StateType.Normal, Colors.Document);
				Add(viewport);
				viewport.Add(vbox);
				PintaCore.Workspace.ActiveDocumentChanged += Workspace_ActiveDocumentChanged;
				PintaCore.Workspace.DocumentModified += Workspace_ActiveDocumentChanged;
				ButtonPressEvent += TimelineTreeView_ButtonPress;
				// KeyPressEvent += TimelineTreeView_KeyPressEvent;
			}

			protected override bool OnKeyPressEvent(EventKey evt)
			{
				//viewport.Vadjustment.PageSize = 5;
				//viewport.Vadjustment.Lower = 0;
				//viewport.Vadjustment.Upper = 20;

				if (!PintaCore.Workspace.HasOpenDocuments) {
					return true;
				}
				var container = PintaCore.Workspace.ActiveDocument.Container;
				var selection = PintaCore.Workspace.ActiveDocument.Selection;
				if (evt.Key == Gdk.Key.Up) {
					if (selection.Count == 0 && container.Nodes.Count > 0) {
						SelectNodes(false, container.Nodes[0]);
					} else if (selection.Count > 0) {
						int i = container.Nodes.IndexOf(selection[0]);
						if (i > 0) {
							//viewport.Vadjustment.Value = TimelinePad.Instance.RowHeight * (i - 1);
							SelectNodes(false, container.Nodes[i - 1]);
						}
					}
				} else if (evt.Key == Gdk.Key.Down) {
					if (selection.Count == 0 && container.Nodes.Count > 0) {
						SelectNodes(false, container.Nodes[0]);
					} else if (selection.Count > 0) {
						int i = container.Nodes.IndexOf(selection[selection.Count - 1]);
						if (i < container.Nodes.Count - 1) {
							//viewport.Vadjustment.Value = TimelinePad.Instance.RowHeight * i;
							SelectNodes(false, container.Nodes[i + 1]);
						}
					}
				} else {
					return base.OnKeyPressEvent(evt);
				}
				return true;
			}

			void TimelineTreeView_ButtonPress(object o, ButtonPressEventArgs args)
			{
				if (!PintaCore.Workspace.HasOpenDocuments) {
					return;
				}
				int i = (int)args.Event.Y / TimelinePad.Instance.RowHeight;
				var container = PintaCore.Workspace.ActiveDocument.Container;
				if (i >= 0 && i < container.Nodes.Count) {
					SelectNodes(false, container.Nodes[i]);
				}
			}

			private void SelectNodes(bool append, params Lime.Node[] nodes)
			{
				PintaCore.Workspace.ActiveDocument.History.PushNewItem(
					new SelectNodesHistoryItem(nodes, append));
			}

			bool IsNodeListChanged()
			{
				var container = PintaCore.Workspace.ActiveDocument.Container;
				if (vbox.Children.Length != container.Nodes.Count) {
					return true;
				}
				for (int i = 0; i < vbox.Children.Length; i++) {
					if ((vbox.Children[i] as TimelineTreeviewItem).Node != container.Nodes[i]) {
						return true;
					}
				}
				return false;
			}

			void Workspace_ActiveDocumentChanged(object sender, EventArgs e)
			{
				if (PintaCore.Workspace.HasOpenDocuments) {
					if (!IsNodeListChanged()) {
						this.QueueDraw();
						return;
					}
					foreach (var child in vbox.Children) {
						vbox.Remove(child);
					}
					var container = PintaCore.Workspace.ActiveDocument.Container;
					foreach (var node in container.Nodes) {
						vbox.PackStart(new TimelineTreeviewItem(node), false, false, 0);
					}
					vbox.ShowAll();
				}
			}
		}

		Gtk.Widget CreateTimelineToolbar()
		{
			var tb = new TimelineToolbar();
			//tb.HeightRequest = 100;
			return tb;
		}

		Gtk.Widget CreateTimelineRuler()
		{
			var ruler = new Gtk.Toolbar();
			//ruler.HeightRequest = 32;
			return ruler;
		}

		Gtk.Widget CreateNodesPane()
		{
			var nodesPane = new Gtk.DrawingArea();
			return nodesPane;
		}
		
		public class PanViewWidget : Gtk.DrawingArea
		{
			public PanViewWidget()
			{
				EnterNotifyEvent += (o, e) => {
					if (PintaCore.Workspace.HasOpenDocuments) {
					}
				};

				Events = EventMask.ButtonPressMask | EventMask.ButtonReleaseMask | EventMask.PointerMotionMask | EventMask.EnterNotifyMask;
				// Give mouse press events to the current tool
				ButtonPressEvent += (sender, e) =>
				{
					Console.WriteLine("Panview");
					if (PintaCore.Workspace.HasOpenDocuments) {
					}
						//PintaCore.Tools.CurrentTool.DoMouseDown(this, e, PintaCore.Workspace.WindowPointToCanvas(e.Event.X, e.Event.Y));
				};

				// Give mouse release events to the current tool
				ButtonReleaseEvent += (sender, e) =>
				{
					//if (PintaCore.Workspace.HasOpenDocuments)
						//PintaCore.Tools.CurrentTool.DoMouseUp(this, e, PintaCore.Workspace.WindowPointToCanvas(e.Event.X, e.Event.Y));
				};

				// Give mouse move events to the current tool
				MotionNotifyEvent += (sender, e) =>
				{
					if (!PintaCore.Workspace.HasOpenDocuments)
						return;

					Cairo.PointD point = PintaCore.Workspace.ActiveWorkspace.WindowPointToCanvas(e.Event.X, e.Event.Y);

					//if (PintaCore.Workspace.ActiveWorkspace.PointInCanvas(point))
					//	PintaCore.Chrome.LastCanvasCursorPoint = point.ToGdkPoint();

					//if (PintaCore.Tools.CurrentTool != null)
					//	PintaCore.Tools.CurrentTool.DoMouseMove(sender, e, point);
				};

				// Handle key press/release events
				//KeyPressEvent += new KeyPressEventHandler(PintaCanvas_KeyPressEvent);
				//KeyReleaseEvent += new KeyReleaseEventHandler(PintaCanvas_KeyReleaseEvent);
			}
		}

		Gtk.Widget CreatePanViewPane()
		{
			var pane = new PanViewWidget();
			return pane;
		}
	}
}
