using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
	public enum DockZone
	{
		None,
		Top,
		Left,
		Right,
		Bottom
	}

	/// <summary>
	/// Represents area within main window which can contain one or more dock panels.
	/// There are can be up to 4 different dock sites for each side of the window.
	/// </summary>
	class DockSite : Xwt.HBox
	{
		public Xwt.Box box;
		public DockZone Zone { get; private set; }
		//private DockSiteManager manager;

		public DockSite(DockZone zone)
		{
			//manager = new DockSiteManager(this);

			if (zone == DockZone.Top || zone == DockZone.Bottom) {
				box = new Xwt.HBox();
			} else {
				box = new Xwt.VBox();
			}
			this.PackStart(box, Xwt.BoxMode.Expand);
			Zone = zone;
		}

		public void AddPanel(IDockPanel panel)
		{
			//if (manager.CentralWidget == null) {
			//	manager.CentralWidget = panel.Widget;
			//} else {
			//	manager.AddDockPanel(panel, DockZone.Bottom);
			//}
			box.PackStart(panel.Widget, Xwt.BoxMode.Expand);
		}

		public bool RemovePanel(IDockPanel panel)
		{
			//return manager.RemoveDockPanel(panel);
			return box.Remove(panel.Widget);
		}

		public int PanelCount
		{
			get { return box.Children.Count(); }
		}

		//private void InitPaned(Xwt.Widget widget1, Xwt.Widget widget2, double position, bool animated)
		//{
		//	Paned.Panel1.Content = widget1;
		//	Paned.Panel2.Content = widget2;
		//	if (!animated) {
		//		Paned.Position = position;
		//	} else {
		//		double p = 0;
		//		Paned.Position = 0;
		//		Xwt.Application.TimeoutInvoke(10, () => {
		//			p += 20;
		//			Paned.Position = p;
		//			if (Paned.Position > position) {
		//				Paned.Position = position;
		//				return false;
		//			}
		//			return true;
		//		});
		//	}
		//}
	}

}
