using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.FilesystemView
{
	// TODO: refactor tree manipulation code all over the file
	// whole pane with all filesystem views
	public class FilesystemPane
	{
		public static FilesystemPane Instance;
		private Widget dockPanelWidget;
		private Widget rootWidget;
		private DockPanel dockPanel;
		private List<FilesystemView> views = new List<FilesystemView>();

		public FilesystemPane(DockPanel dockPanel)
		{
			Instance = this;
			this.dockPanel = dockPanel;
			dockPanelWidget = dockPanel.ContentWidget;
			dockPanelWidget.AddChangeWatcher(() => Core.Project.Current.CitprojPath, (path) => {
				Initialize();
			});
			CommandHandlerList.Global.Connect(FilesystemCommands.NavigateTo, HandleHavigateTo);
			CommandHandlerList.Global.Connect(FilesystemCommands.OpenInSystemFileManager, HandleOpenInSystemFileManager);
		}

		private void Initialize()
		{
			// TODO: clear references from user preferences tree to widgets from tree disposed
			rootWidget?.UnlinkAndDispose();
			var up = Core.UserPreferences.Instance.Get<UserPreferences>();
			var q = new Queue<ViewNode>();
			q.Enqueue(up.ViewRoot);
			while (q.Count != 0) {
				var n = q.Dequeue();
				foreach (var child in n.Children) {
					child.Parent = n;
					q.Enqueue(child);
				}
				Widget w;
				if (n is FSViewNode) {
					var fsView = new FilesystemView();
					views.Add(fsView);
					w = fsView.RootWidget;
					w.Components.Add(new ViewNodeComponent { ViewNode = n });
					fsView.Initialize();
				} else if (n is SplitterNode) {
					var type = (n as SplitterNode).Type;
					Splitter s = type.MakeSplitter();
					s.Stretches = Splitter.GetStretchesList((n as SplitterNode).Stretches, 1, 1);
					w = s;
					w.Components.Add(new ViewNodeComponent { ViewNode = n }); // copy pasted line
				} else {
					throw new InvalidDataException();
				}
				n.Widget = w;
				if (n.Parent != null) {
					n.Parent.Widget.AddNode(w);
				} else {
					rootWidget = w;
				}
			}
			dockPanelWidget.PushNode(rootWidget);
			rootWidget.SetFocus();
		}

		public void Split(FilesystemView fsView, SplitterType type)
		{
			var RootWidget = fsView.RootWidget;
			FSViewNode vn = RootWidget.Components.Get<ViewNodeComponent>().ViewNode as FSViewNode;

			var newFsView = new FilesystemView();
			views.Add(newFsView);
			var newVn = new FSViewNode {
				Widget = newFsView.RootWidget,
				Path = vn.Path,
				ShowSelectionPreview = vn.ShowSelectionPreview,
				ShowCookingRulesEditor = vn.ShowCookingRulesEditor,
			};
			// TODO: setup internal stretches of fsView here
			newFsView.RootWidget.Components.Add(new ViewNodeComponent { ViewNode = newVn });
			newFsView.Initialize();

			if (vn.Parent == null) {
				// Root node, need to replace on in UserPreferences
				Splitter s = type.MakeSplitter();
				var up = Core.UserPreferences.Instance.Get<UserPreferences>();
				var sn = new SplitterNode {
					Widget = s,
					Type = type
				};
				up.ViewRoot = sn;
				newVn.Parent = sn;
				s.Stretches = Splitter.GetStretchesList(sn.Stretches, 1, 1);
				sn.Children.Add(vn);
				sn.Children.Add(newVn);
				var thisParent = RootWidget.ParentWidget;
				RootWidget.Unlink();
				s.AddNode(RootWidget);
				s.AddNode(newFsView.RootWidget);
				s.Components.Add(new ViewNodeComponent { ViewNode = sn });
				vn.Parent = sn;
				thisParent.Nodes.Add(s);

				// TODO setup stretches ^
			} else if (vn.Parent is SplitterNode) {
				if ((vn.Parent as SplitterNode).Type == type) {
					var sn = vn.Parent as SplitterNode;
					var s = sn.Widget;
					s.Nodes.Insert(s.Nodes.IndexOf(RootWidget), newFsView.RootWidget);
					newVn.Parent = sn;
					sn.Children.Insert(sn.Children.IndexOf(vn), newVn);
				} else {
					Splitter s = type.MakeSplitter();
					var sn = new SplitterNode {
						Widget = s,
						Type = type
					};
					s.Components.Add(new ViewNodeComponent { ViewNode = sn });
					s.Stretches = Splitter.GetStretchesList(sn.Stretches);

					var psn = vn.Parent as SplitterNode;

					// or vn.Parent.Widget
					int thisIndex = RootWidget.ParentWidget.Nodes.IndexOf(RootWidget);
					var thisParent = RootWidget.ParentWidget;
					RootWidget.Unlink();
					s.AddNode(RootWidget);
					s.AddNode(newFsView.RootWidget);
					sn.Children.Add(vn);
					sn.Children.Add(newVn);
					vn.Parent = sn;
					newVn.Parent = sn;
					thisParent.Nodes.Insert(thisIndex, s);


					var ps = psn.Widget;
					sn.Parent = psn;
					psn.Children.RemoveAt(thisIndex);
					psn.Children.Insert(thisIndex, sn);
				}
			} else if (vn.Parent is FSViewNode) {
				// wat
			}

		}

		public void Close(FilesystemView fsView)
		{
			views.Remove(fsView);
			var RootWidget = fsView.RootWidget;
			ViewNode vn = RootWidget.Components.Get<ViewNodeComponent>().ViewNode;

			if (vn.Parent == null) {
				// oh noes can't close root!
				return;
			}

			if (vn.Parent is SplitterNode) {
				var sn = vn.Parent as SplitterNode;
				vn.Widget = null; // just in case?
				sn.Children.Remove(vn);
				RootWidget.UnlinkAndDispose();
				if (sn.Children.Count == 1) {
					var ovn = sn.Children.First();
					if (sn.Parent == null) {
						// Root => update up
						var up = Core.UserPreferences.Instance.Get<UserPreferences>();
						up.ViewRoot = ovn;
						ovn.Parent = null;
						ovn.Widget.Unlink();
						var pw = sn.Widget.ParentWidget;
						sn.Widget.UnlinkAndDispose();
						pw.AddNode(ovn.Widget);
					} else {
						// remap
						ovn.Parent = sn.Parent;
						ovn.Widget.Unlink();
						var pwIndex = sn.Widget.ParentWidget.Nodes.IndexOf(sn.Widget);
						var pw = sn.Widget.ParentWidget;
						sn.Widget.UnlinkAndDispose();
						pw.Nodes.Insert(pwIndex, ovn.Widget);
						ovn.Parent.Children.Remove(sn);
						ovn.Parent.Children.Insert(pwIndex, ovn);
					}
				} else {
					// wat
				}
			} else {
				// wat
			}
		}

		private void HandleHavigateTo()
		{
			if (views.Count == 0) {
				// In case we start Tangerine with FilesystemPane hidden we want to invoke above added task
				// which calls initialize for the first time regardless of this pane being hidden or not.
				dockPanelWidget.Update(0);
			}
			var view = views.First();
			DockManager.Instance.ShowPanel(dockPanel);
			var path = FilesystemCommands.NavigateTo.UserData as string;
			var dir = Path.GetDirectoryName(path);
			view.GoTo(dir);
			// kind of force reaction to GoTo with Update(0)
			view.RootWidget.Update(0);
			view.SelectAsset(path);
			FilesystemCommands.NavigateTo.UserData = null;
		}

		private void OpenInSystemFileManager(string path)
		{
#if WIN
			System.Diagnostics.Process.Start("explorer.exe", "/select, \"" + path + "\"");
#elif MAC
			throw new NotImplementedException();
#else
			throw new NotImplementedException();
#endif
		}

		private void HandleOpenInSystemFileManager()
		{
			var path = FilesystemCommands.OpenInSystemFileManager.UserData as string;
			path = path.Replace('/', '\\');
			var extension = Path.GetExtension(path);
			if (string.IsNullOrEmpty(extension)) {
				foreach (string f in Directory.GetFiles(Path.GetDirectoryName(path))) {
					if (Path.ChangeExtension(f, null) == path && !f.EndsWith(".txt")) {
						OpenInSystemFileManager(f);
						break;
					}
				}
			} else {
				OpenInSystemFileManager(path);
			}
		}
	}
}