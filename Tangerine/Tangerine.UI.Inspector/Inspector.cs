using Lime;
using Tangerine.Core;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Core.Components;

namespace Tangerine.UI.Inspector
{
	public delegate IPropertyEditor PropertyEditorBuilder(PropertyEditorParams context);

	public class Inspector : IDocumentView
	{
		private static readonly ITexture inspectRootActivatedTexture;
		private static readonly ITexture inspectRootDeactivatedTexture;

		private readonly InspectorContent content;
		private readonly Widget contentWidget;

		public static Inspector Instance { get; private set; }

		public readonly Widget PanelWidget;
		public readonly ThemedScrollView RootWidget;
		public readonly Toolbar Toolbar;
		public readonly List<object> Objects;

		static Inspector()
		{
			inspectRootActivatedTexture = IconPool.GetTexture("Tools.InspectRootActivated");
			inspectRootDeactivatedTexture = IconPool.GetTexture("Tools.InspectRootDeactivated");
		}

		public static void RegisterGlobalCommands()
		{
			CommandHandlerList.Global.Connect(InspectorCommands.InspectRootNodeCommand, () => Document.Current.InspectRootNode = !Document.Current.InspectRootNode);
		}

		public void Attach()
		{
			Instance = this;
			PanelWidget.PushNode(RootWidget);
			Docking.DockManager.Instance.FilesDropped += OnFilesDropped;
		}

		public void Detach()
		{
			Instance = null;
			Docking.DockManager.Instance.FilesDropped -= OnFilesDropped;
			RootWidget.Unlink();
		}

		private void OnFilesDropped(IEnumerable<string> files) => content.DropFiles(files);

		public Inspector(Widget panelWidget)
		{
			PanelWidget = panelWidget;
			RootWidget = new ThemedScrollView();
			var toolbarArea = new Widget { Layout = new StackLayout(), Padding = new Thickness(4, 0) };
			contentWidget = new Widget();
			RootWidget.Content.AddNode(toolbarArea);
			RootWidget.Content.AddNode(contentWidget);
			RootWidget.Content.Layout = new VBoxLayout();
			Toolbar = new Toolbar(toolbarArea);
			contentWidget.Layout = new VBoxLayout { Tag = "InspectorContent" };
			Objects = new List<object>();
			content = new InspectorContent(contentWidget);
			CreateWatchersToRebuild();
			SetupToolbar();
		}

		private void SetupToolbar()
		{
			Toolbar.Add(InspectorCommands.InspectRootNodeCommand);
		}

		private void CreateWatchersToRebuild()
		{
			RootWidget.AddChangeWatcher(() => CalcSelectedRowsHashcode(), _ => Rebuild());
			RootWidget.AddChangeWatcher(() => Document.Current.InspectRootNode, _ => Rebuild());
		}

		private static int CalcSelectedRowsHashcode()
		{
			var r = 0;
			foreach (var row in Document.Current.Rows) {
				if (row.Selected) {
					r ^= row.GetHashCode();
					var node = row.Components.Get<NodeRow>()?.Node;
					if (node != null) {
						foreach (var component in node.Components) {
							r ^= component.GetHashCode();
						}
					}
				}
			}
			return r;
		}

		private void Rebuild()
		{
			content.BuildForObjects(Document.Current.InspectRootNode ? new[] { Document.Current.RootNode } : Document.Current.SelectedNodes().ToArray());
			InspectorCommands.InspectRootNodeCommand.Icon = Document.Current.InspectRootNode ? inspectRootActivatedTexture : inspectRootDeactivatedTexture;
			Toolbar.Rebuild();
			RootWidget.ScrollPosition = RootWidget.MinScrollPosition;
		}
	}
}
