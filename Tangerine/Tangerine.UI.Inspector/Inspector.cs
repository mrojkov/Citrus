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
			CommandHandlerList.Global.Connect(InspectorCommands.InspectEasing, new InspectEasingCommandHandler());
		}

		class InspectEasingCommandHandler : CommandHandler
		{
			public override void RefreshCommand(ICommand command)
			{
				command.Checked = CoreUserPreferences.Instance.InspectEasing;
			}

			public override void Execute()
			{
				CoreUserPreferences.Instance.InspectEasing = !CoreUserPreferences.Instance.InspectEasing;
			}
		}

		public void Attach()
		{
			Instance = this;
			PanelWidget.PushNode(RootWidget);
			Docking.DockManager.Instance.FilesDropped += OnFilesDropped;
			content.LoadExpandedStates();
			Rebuild();
		}

		public void Detach()
		{
			Instance = null;
			content.SaveExpandedStates();
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
								CommandIds = { "InspectRootNodeCommand", "InspectEasing" }
							}
						}
					}
				}
			};
		}

		private void CreateWatchersToRebuild()
		{
			RootWidget.AddChangeWatcher(CalcSelectedRowsHashcode, _ => Rebuild());
			RootWidget.Tasks.Add(DisableInspectorTask());
		}

		private IEnumerator<object> DisableInspectorTask()
		{
			while (true) {
				if (!Document.Current.InspectRootNode) {
					var enabled = true;
					foreach (var row in Document.Current.Rows) {
						if (row.Selected) {
							var node = row.Components.Get<NodeRow>()?.Node;
							enabled &= !node?.GetTangerineFlag(TangerineFlags.Locked) ?? true;
						}
					}
					contentWidget.Enabled = enabled;
				}
				yield return null;
			}
		}

		private static int CalcSelectedRowsHashcode()
		{
			var r = 0;
			if (CoreUserPreferences.Instance.InspectEasing) {
				r ^= FindMarkerBehind()?.GetHashCode() ?? 0;
			}
			if (Document.Current.Animation.IsCompound) {
				foreach (var track in GetSelectedAnimationTracksAndClips()) {
					r ^= track.GetHashCode();
				}
			} else if (Document.Current.InspectRootNode) {
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
								if (ClassAttributes<NodeComponentDontSerializeAttribute>.Get(component.GetType()) != null) {
									continue;
								}
								r ^= component.GetHashCode();
							}
						}
					}
				}
			}
			return r;
		}

		public static Marker FindMarkerBehind()
		{
			Marker marker = null;
			foreach (var m in Document.Current.Animation.Markers) {
				if (m.Frame > Document.Current.Animation.Frame) {
					break;
				}
				marker = m;
			}
			return marker;
		}

		private void Rebuild()
		{
			if (Document.Current.Animation.IsCompound) {
				content.BuildForObjects(GetSelectedAnimationTracksAndClips().ToList());
			} else if (Document.Current.InspectRootNode) {
				content.BuildForObjects(new[] { Document.Current.RootNode });
			} else {
				content.BuildForObjects(Document.Current.SelectedNodes().ToList());
			}
			InspectorCommands.InspectRootNodeCommand.Icon = Document.Current.InspectRootNode ? inspectRootActivatedTexture : inspectRootDeactivatedTexture;
			Toolbar.Rebuild();
			// Delay UpdateScrollPosition, since contentWidget.MaxScrollPosition is not updated yet.
			contentWidget.LateTasks.Add(UpdateScrollPositionOnNextUpdate);
		}

		private static IEnumerable<object> GetSelectedAnimationTracksAndClips()
		{
			foreach (var row in Document.Current.SelectedRows()) {
				yield return row.Components.Get<AnimationTrackRow>().Track;
			}
			foreach (var row in Document.Current.Rows) {
				var track = row.Components.Get<AnimationTrackRow>().Track;
				foreach (var clip in track.Clips) {
					if (clip.IsSelected) {
						yield return clip;
					}
				}
			}
		}

		private IEnumerator<object> UpdateScrollPositionOnNextUpdate()
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
