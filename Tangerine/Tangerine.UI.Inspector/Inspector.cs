using System;
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
		private static readonly Icon inspectRootActivatedTexture;
		private static readonly Icon inspectRootDeactivatedTexture;

		private readonly InspectorContent content;
		private readonly ThemedScrollView contentWidget;

		private HashSet<Type> prevTypes = new HashSet<Type>();
		private float prevPosition;

		public static Inspector Instance { get; private set; }

		public readonly Widget PanelWidget;
		public readonly Widget RootWidget;
		public readonly ToolbarView Toolbar;
		public readonly List<object> Objects;

		static Inspector()
		{
			inspectRootActivatedTexture = IconPool.GetIcon("Tools.InspectRootActivated");
			inspectRootDeactivatedTexture = IconPool.GetIcon("Tools.InspectRootDeactivated");
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
			RootWidget = new Widget { Layout = new VBoxLayout() };
			var toolbarArea = new Widget { Layout = new StackLayout(), Padding = new Thickness(4, 0) };
			contentWidget = new ThemedScrollView();
			RootWidget.AddNode(toolbarArea);
			RootWidget.AddNode(contentWidget);
			contentWidget.Content.Layout = new VBoxLayout();
			Toolbar = new ToolbarView(toolbarArea, GetToolbarLayout());
			Objects = new List<object>();
			content = new InspectorContent(contentWidget.Content) {
				Footer = new Widget { MinHeight = 300.0f },
				History = Document.Current.History
			};
			CreateWatchersToRebuild();
		}

		private static ToolbarModel GetToolbarLayout()
		{
			return new ToolbarModel {
				Rows = {
					new ToolbarModel.ToolbarRow {
						Index = 0,
						Panels = {
							new ToolbarModel.ToolbarPanel {
								Index = 0,
								Title = "Inspector Toolbar Panel",
								Draggable = false,
								CommandIds = { "InspectRootNodeCommand" }
							}
						}
					}
				}
			};
		}

		private void CreateWatchersToRebuild()
		{
			RootWidget.AddChangeLateWatcher(CalcSelectedRowsHashcode, _ => Rebuild());
		}

		private static int CalcSelectedRowsHashcode()
		{
			var r = 0;
			if (Document.Current.InspectRootNode) {
				var rootNode = Document.Current.RootNode;
				r ^= rootNode.GetHashCode();
				foreach (var component in rootNode.Components) {
					r ^= component.GetHashCode();
				}
			} else {
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
			}
			return r;
		}

		private void Rebuild()
		{
			content.BuildForObjects(Document.Current.InspectRootNode ? new[] { Document.Current.RootNode } : Document.Current.SelectedNodes().ToArray());
			InspectorCommands.InspectRootNodeCommand.Icon = Document.Current.InspectRootNode ? inspectRootActivatedTexture : inspectRootDeactivatedTexture;
			Toolbar.Rebuild();
			// contentWidget.MaxScrollPosition is not updated on current frame
			contentWidget.LateTasks.Add(UpdateScrollPosition);
		}

		private IEnumerator<object> UpdateScrollPosition()
		{
			var nodes = Document.Current.InspectRootNode
				? new[] { Document.Current.RootNode }
				: Document.Current.SelectedNodes().ToArray();
			
			var types = new HashSet<Type>(InspectorContent.GetTypes(nodes));
			var areEqual = types.SetEquals(prevTypes);
			contentWidget.ScrollPosition = areEqual ?
				Math.Min(contentWidget.MaxScrollPosition, contentWidget.ScrollPosition) : contentWidget.MinScrollPosition;
			prevTypes = types;
			yield break;
		}
	}
}
