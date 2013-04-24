using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
#if !SEXY_PANED
	using SexyPaned = Xwt.Paned;
#endif

	public class DockManager
	{
		public Xwt.Widget CentralWidget { get; set; }
		public Xwt.Box DockArea { get; private set; }
		public DockDragManager DragManager { get; private set; }

		private SexyPaned panedTree;
		private List<IDockElement> elements;

		public DockManager(Xwt.Box dockArea)
		{
			DockArea = dockArea;
			DragManager = new DockDragManager(this);
			elements = new List<IDockElement>();
		}

		public IEnumerable<IDockElement> Elements { get { return elements; } }

		public void AddDockElement(IDockElement element, DockZone zone)
		{
			elements.Add(element);
			// This is advanced feature (not implemented yet):
			// var site = FindDockSite(zone);
			//if (site == null) {
			//	site = CreateDockSite(zone);
			//}
			var site = CreateDockSite(zone);
			site.AddElement(element);
		}

		public bool RemoveDockElement(IDockElement element)
		{
			elements.Remove(element);
			foreach (var site in EnumerateDockSites(panedTree)) {
				if (site.RemoveElement(element)) {
					if (site.ElementCount == 0) {
						RemoveDockSite(site);
					}
					return true;
				}
			}
			return false;
		}

		private void RemoveDockSite(DockSite site)
		{
			var paned = site.Parent as SexyPaned;
			Xwt.Widget content;
			if (site == paned.Panel1.Content) {
				content = paned.Panel2.Content;
			} else {
				content = paned.Panel1.Content;
			}
			PanedUtils.ExtractFromPaned(content);
			var panel = PanedUtils.GetOwnedPanel(paned);
			if (panel != null) {
				panel.Content = content;
			} else {
				panedTree = content as SexyPaned;
				DockArea.Clear();
				DockArea.PackStart(content, Xwt.BoxMode.Expand);
			}
		}

		private DockSite FindDockSite(DockZone zone)
		{
			var e = EnumerateDockSites(panedTree);
			return e.FirstOrDefault(ds => ds.Zone == zone);
		}

		private IEnumerable<DockSite> EnumerateDockSites(Xwt.Widget tree)
		{
			if (tree is DockSite) {
				yield return (DockSite)tree;
			} else if (tree is SexyPaned) {
				var paned = (SexyPaned)tree;
				foreach (var ds in EnumerateDockSites(paned.Panel1.Content)) {
					yield return ds;
				}
				foreach (var ds in EnumerateDockSites(paned.Panel2.Content)) {
					yield return ds;
				}
			}
		}

		private DockSite CreateDockSite(DockZone zone)
		{
			Xwt.Widget content = null;
			if (panedTree == null) {
				content = CentralWidget;
			} else {
				content = panedTree;
			}
			DockArea.Remove(content);
			var site = new DockSite(zone);
			SexyPaned paned = null;
			if (zone == DockZone.Left) {
				paned = new SexyHPaned();
				paned.BackgroundColor = Colors.ToolPanelBackground;
				paned.Panel1.Content = site;
				paned.Panel2.Content = content;
			} else if (zone == DockZone.Right) {
				paned = new SexyHPaned();
				paned.BackgroundColor = Colors.ToolPanelBackground;
				paned.Panel1.Content = content;
				paned.Panel2.Content = site;
			} else if (zone == DockZone.Top) {
				paned = new SexyVPaned();
				paned.BackgroundColor = Colors.ToolPanelBackground;
				paned.Panel1.Content = site;
				paned.Panel2.Content = content;
			} else if (zone == DockZone.Bottom) {
				paned = new SexyVPaned();
				paned.BackgroundColor = Colors.ToolPanelBackground;
				paned.Panel1.Content = content;
				paned.Panel2.Content = site;
			} else {
				throw new InvalidOperationException();
			}
			panedTree = paned;
			DockArea.PackStart(panedTree, Xwt.BoxMode.Expand);
			return site;
		}
	}
}
