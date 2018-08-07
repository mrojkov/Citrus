using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Core;

namespace Tangerine.UI
{
	class TreeView
	{
		private readonly Node rootNode;
		private readonly ThemedScrollView scrollView;
		private readonly TreeNode root;
		private readonly Widget parent;
		private TreeNode selected = null;

		public TreeView(Widget parent, Node rootNode)
		{
			this.rootNode = rootNode;
			this.parent = parent;
			scrollView = new ThemedScrollView();
			scrollView.Content.Layout = new VBoxLayout();
			scrollView.Content.AddNode(root = new TreeNode(this, rootNode, null, JointType.LShaped, new List<Joint>(), 0, isLast: true));
		}

		public void Detach()
		{
			scrollView.Unlink();
		}

		public void Attach()
		{
			parent.AddNode(scrollView);
		}

		public bool IsAttached()
		{
			return scrollView.Parent != null;
		}

		public void Filter(string filter)
		{
			root.Filter(filter);
		}

		public void ClearSelection()
		{
			if (selected != null) {
				selected.Selected = false;
				selected = null;
			}
		}

		private void SelectTreeNode(TreeNode node)
		{
			if (selected != null) {
				selected.Selected = false;
			}
			selected = node;
			node.Selected = true;
		}

		private enum JointType {
			None,
			HLine,
			VLine,
			TShaped,
			LShaped,
		}

		private class Joint : Widget
		{
			public JointType Type { get; set; }

			public Joint(JointType type, float size)
			{
				Type = type;
				MinMaxWidth = size;
			}

			public override void Render()
			{
				base.Render();
				PrepareRendererState();
				switch (Type) {
					case JointType.None:
						break;
					case JointType.HLine:
						Renderer.DrawLine(0, Height / 2, Width, Height / 2, Color4.Gray);
						break;
					case JointType.VLine:
						Renderer.DrawLine(Width / 2, 0, Width / 2, Height, Color4.Gray);
						break;
					case JointType.TShaped:
						Renderer.DrawLine(Width / 2, 0, Width / 2, Height, Color4.Gray);
						Renderer.DrawLine(Width / 2, Height / 2, Width, Height / 2, Color4.Gray);
						break;
					case JointType.LShaped:
						Renderer.DrawLine(Width / 2, 0, Width / 2, Height / 2, Color4.Gray);
						Renderer.DrawLine(Width / 2, Height / 2, Width, Height / 2, Color4.Gray);
						break;
					default:
						throw new ArgumentException();
				}
			}
		}

		private class TreeNode : Widget
		{
			private static readonly float defaultPadding = 5;
			private static readonly float defaultJointWidth = 19;

			private readonly Node rootNode;
			private ToolbarButton expandButton;
			private readonly Joint expandJoint = new Joint(JointType.HLine, defaultJointWidth);
			private readonly Widget treeNodesContainer = new Widget { Layout = new VBoxLayout() };
			private readonly Joint parentJoint;
			private readonly List<Joint> offsetJoints;
			private readonly int level;
			private readonly TreeNode parentTreeNode;
			private readonly TreeView view;
			private bool expanded = false;
			private bool expandable;
			private string filter;
			private List<TreeNode> savedNodes = new List<TreeNode>();

			internal bool Selected { get; set; }

			public TreeNode(TreeView view, Node rootNode, TreeNode parentTreeNode, JointType jointType, List<Joint> offsetJoints, int level, bool isLast)
			{
				this.rootNode = rootNode;
				this.level = level;
				this.parentTreeNode = parentTreeNode;
				this.view = view;
				expandable = rootNode.Nodes.Count > 0;
				Layout = new VBoxLayout();

				var treeNodeWidget = new Widget {
					Layout = new HBoxLayout(),
					HitTestTarget = true
				};

				this.offsetJoints = offsetJoints.Select(w => (Joint)w.Clone()).ToList();
				foreach (var ow in this.offsetJoints) {
					treeNodeWidget.AddNode(ow);
				}
				this.offsetJoints.Add(new Joint(isLast ? JointType.None : JointType.VLine, defaultJointWidth));

				treeNodeWidget.AddNode(parentJoint = new Joint(jointType, defaultJointWidth));
				CreateExpandButton();
				treeNodeWidget.AddNode(rootNode.Nodes.Count > 0 ? expandButton : (Widget)expandJoint);
				treeNodeWidget.AddNode(CreateLabel());
				treeNodeWidget.AddNode(new Widget());
				treeNodeWidget.CompoundPresenter.Add(new DelegatePresenter<Widget>(w => {
					if (Selected) {
						w.PrepareRendererState();
						Renderer.DrawRect(0, 0, w.Width, w.Height, ColorTheme.Current.Toolbar.ButtonHighlightBackground);
					}
				}));
				if (Document.Current.RootNode != rootNode) {
					treeNodeWidget.Clicked += () => view.SelectTreeNode(this);
					treeNodeWidget.Gestures.Add(new DoubleClickGesture(NavigateToWidget));
				}
				AddNode(treeNodeWidget);

				this.AddChangeWatcher(() => rootNode.NextSibling, _ => parentTreeNode?.UpdateChildTreeNodes());
				this.AddChangeWatcher(() => rootNode.Nodes.Count, _ => UpdateChildTreeNodes());

				UpdateChildTreeNodes();
			}

			private void CreateExpandButton()
			{
				expandButton = new ToolbarButton {
					Highlightable = false,
					MinMaxSize = new Vector2(defaultJointWidth, defaultJointWidth + defaultPadding * 2),
					Padding = new Thickness {
						Left = defaultPadding,
						Right = defaultPadding,
						Top = defaultPadding * 2,
						Bottom = defaultPadding * 2
					},
					Texture = IconPool.GetTexture("Timeline.plus"),
				};
				expandButton.Clicked += ToggleExpanded;
				expandButton.CompoundPresenter.Insert(0, new DelegatePresenter<Widget>(w => {
					w.PrepareRendererState();
					var iconSize = (w.Width - 2 * defaultPadding) / 2;
					Renderer.DrawLine(0, w.Height / 2, w.Width / 2 - iconSize, w.Height / 2, Color4.Gray);
					if (expanded) {
						Renderer.DrawLine(w.Width / 2, w.Height / 2 + iconSize, w.Width / 2, w.Height, Color4.Gray);
					}
				}));
			}

			private Widget CreateLabel()
			{
				var label = new ThemedSimpleText { Padding = new Thickness(defaultPadding) };
				label.AddChangeWatcher(() => rootNode.Id, t => label.Text = Document.Current.RootNode == rootNode ? "root" : t);
				return label;
			}

			public void ToggleExpanded()
			{
				expanded = !expanded;
				UpdateButtonTexture();
				if (!expanded) {
					treeNodesContainer.Unlink();
					return;
				}
				AddNode(treeNodesContainer);
			}

			public void UpdateButtonTexture()
			{
				expandButton.Texture = expanded ? IconPool.GetTexture("Timeline.minus") : IconPool.GetTexture("Timeline.plus");
			}

			private static void ReplaceNode(Node node1, Node node2)
			{
				var node = node2.Parent;
				int index = node.Nodes.IndexOf(node2);
				node2.Unlink();
				node.Nodes.Insert(index, node1);
			}

			private void SetExpandable(bool expandable)
			{
				if (this.expandable == expandable) {
					return;
				}
				this.expandable = expandable;
				if (expandable) {
					ReplaceNode(expandButton, expandJoint);
					return;
				}
				ReplaceNode(expandJoint, expandButton);
			}

			private void SetOffsetJoint(int index, JointType jointType)
			{
				offsetJoints[index].Type = jointType;
				foreach (var node in treeNodesContainer.Nodes.Cast<TreeNode>()) {
					node.SetOffsetJoint(index, jointType);
				}
			}

			private void SetJoints(JointType offsetJoint, JointType joint)
			{
				SetOffsetJoint(level, offsetJoint);
				parentJoint.Type = joint;
			}

			private void UpdateChildTreeNodes()
			{
				var rootNodes = savedNodes.Select(t => t.rootNode).ToList();
				treeNodesContainer.Nodes.Clear();
				foreach (var node in rootNode.Nodes) {
					var index = rootNodes.IndexOf(node);
					if (index >= 0) {
						treeNodesContainer.AddNode(savedNodes[index]);
						savedNodes[index].SetJoints(JointType.VLine, JointType.TShaped);
					} else {
						treeNodesContainer.AddNode(new TreeNode(view, node, this, JointType.TShaped, offsetJoints, level + 1, isLast: false));
					}
				}
				UpdateExpandable();
				savedNodes = treeNodesContainer.Nodes.Cast<TreeNode>().ToList();
				if (!String.IsNullOrEmpty(filter)) {
					view.root.Filter(filter);
				}
			}

			private void NavigateToWidget()
			{
				var node = rootNode;
				var path = new Stack<int>();
				path.Push(node.Parent.Nodes.IndexOf(node));
				var externalScene = node.Parent;
				while (externalScene != Document.Current.RootNode && String.IsNullOrEmpty(externalScene.ContentsPath)) {
					path.Push(externalScene.Parent.Nodes.IndexOf(externalScene));
					externalScene = externalScene.Parent;
				}
				var currentScenePath = Document.Current.Path;
				if (path.Count < level) {
					var index = node.Parent.Nodes.IndexOf(node);
					Document externalSceneDocument;
					try {
						externalSceneDocument = Project.Current.OpenDocument(externalScene.ContentsPath);
					} catch (System.Exception e) {
						AlertDialog.Show(e.Message);
						return;
					}
					externalSceneDocument.SceneNavigatedFrom = currentScenePath;
					node = externalSceneDocument.RootNode;
					foreach (var i in path) {
						node = node.Nodes[i];
					}
				}
				Document.Current.History.DoTransaction(() => {
					Core.Operations.EnterNode.Perform(node.Parent, selectFirstNode: false);
					Core.Operations.SelectNode.Perform(node);
				});
			}

			public bool Filter(string filter)
			{
				this.filter = filter;
				treeNodesContainer.Nodes.Clear();
				bool result = String.IsNullOrEmpty(filter) || (rootNode.Id?.ToLower().Contains(filter.ToLower()) ?? false);
				foreach (var node in savedNodes) {
					if (node.Filter(filter)) {
						result = true;
						treeNodesContainer.AddNode(node);
						node.SetJoints(JointType.VLine, JointType.TShaped);
					}
				}
				UpdateExpandable();
				if (result && expandable && !expanded) {
					ToggleExpanded();
				}
				return result;
			}

			private void UpdateExpandable()
			{
				if (treeNodesContainer.Nodes.Count > 0) {
					((TreeNode)treeNodesContainer.Nodes.Last()).SetJoints(JointType.None, JointType.LShaped);
					SetExpandable(true);
				} else {
					SetExpandable(false);
				}
			}
		}
	}
}
