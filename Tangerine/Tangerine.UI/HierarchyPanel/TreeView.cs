using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Core;

namespace Tangerine.UI
{
	class DocumentHierarchyTreeView
	{
		private readonly Node rootNode;
		private readonly ThemedScrollView scrollView = new ThemedScrollView();
		private readonly TreeNode root;
		private readonly Widget parent;
		private readonly ThemedSimpleText nothingToShow = new ThemedSimpleText("Nothing to show");
		private TreeNode selected = null;

		public DocumentHierarchyTreeView(Widget parent, Node rootNode)
		{
			this.rootNode = rootNode;
			this.parent = parent;
			scrollView.Content.Layout = new VBoxLayout();
			scrollView.Content.AddNode(root = new TreeNode(this, rootNode, null, JointType.LShaped, new List<Joint>(), 0, 0, isLast: true));
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
			if (root.Filter(filter)) {
				nothingToShow.Unlink();
				if (root.Parent == null) {
					scrollView.Content.AddNode(root);
				}
			} else {
				root.Unlink();
				if (nothingToShow.Parent == null) {
					scrollView.Content.AddNode(nothingToShow);
				}
			}
			ClearSelection();
		}

		public void ClearSelection()
		{
			selected = null;
			if (parent.IsFocused()) {
				parent.RevokeFocus();
			}
		}

		private void SelectTreeNode(TreeNode node)
		{
			if (!parent.IsFocused()) {
				parent.SetFocus();
			}
			selected = node;
		}

		public void NavigateToSelectedNode()
		{
			selected?.NavigateToNode();
		}

		public void SelectNextTreeNode()
		{
			if (!HasSelection()) {
				return;
			}
			if (selected.Expandable && selected.Expanded) {
				SelectTreeNode((TreeNode)selected.ChildTreeNodes.First());
				return;
			}
			var newSelected = selected;
			while (newSelected != root && newSelected.Index == newSelected.ParentTreeNode.ChildTreeNodes.Count - 1) {
				newSelected = newSelected.ParentTreeNode;
			}
			if (newSelected == root) {
				return;
			}
			SelectTreeNode((TreeNode)newSelected.ParentTreeNode.ChildTreeNodes[newSelected.Index + 1]);
		}

		public void SelectPreviousTreeNode()
		{
			if (!HasSelection() || selected == root) {
				return;
			}
			if (selected.Index == 0) {
				SelectTreeNode(selected.ParentTreeNode);
				return;
			}
			var newSelected = (TreeNode)selected.ParentTreeNode.ChildTreeNodes[selected.Index - 1];
			while (newSelected.Expandable && newSelected.Expanded) {
				newSelected = (TreeNode)newSelected.ChildTreeNodes.Last();
			}
			SelectTreeNode(newSelected);
		}

		public void EnterSelectedTreeNode()
		{
			if (!HasSelection() || !selected.Expandable) {
				return;
			}
			selected.Expanded = true;
			SelectTreeNode((TreeNode)selected.ChildTreeNodes.First());
		}

		public void LeaveSelectedTreeNode()
		{
			if (selected == null || selected == root) {
				return;
			}
			SelectTreeNode(selected.ParentTreeNode);
		}

		public void ToggleSelectedTreeNode()
		{
			selected?.ToggleExpanded();
		}

		public void EnsureSelectionVisible()
		{
			if (!HasSelection()) {
				return;
			}
			var pos = selected.CalcPositionInSpaceOf(scrollView.Content);
			if (pos.Y < scrollView.ScrollPosition) {
				scrollView.ScrollPosition = pos.Y;
			} else if (pos.Y + selected.RowHeight > scrollView.ScrollPosition + scrollView.Height) {
				scrollView.ScrollPosition = pos.Y - scrollView.Height + selected.RowHeight;
			}
		}

		public bool HasSelection()
		{
			return selected != null;
		}

		public void SelectFirstMatch()
		{
			var node = root;
			while (!node.MatchesFilter()) {
				node = (TreeNode)node.ChildTreeNodes.First();
			}
			SelectTreeNode(node);
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
						Renderer.DrawLine(0, Height / 2, Width, Height / 2, ColorTheme.Current.Hierarchy.JointColor);
						break;
					case JointType.VLine:
						Renderer.DrawLine(Width / 2, 0, Width / 2, Height, ColorTheme.Current.Hierarchy.JointColor);
						break;
					case JointType.TShaped:
						Renderer.DrawLine(Width / 2, 0, Width / 2, Height, ColorTheme.Current.Hierarchy.JointColor);
						Renderer.DrawLine(Width / 2, Height / 2, Width, Height / 2, ColorTheme.Current.Hierarchy.JointColor);
						break;
					case JointType.LShaped:
						Renderer.DrawLine(Width / 2, 0, Width / 2, Height / 2, ColorTheme.Current.Hierarchy.JointColor);
						Renderer.DrawLine(Width / 2, Height / 2, Width, Height / 2, ColorTheme.Current.Hierarchy.JointColor);
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
			private ThemedSimpleText label;
			private readonly Joint expandJoint = new Joint(JointType.HLine, defaultJointWidth);
			private readonly Widget treeNodesContainer = new Widget { Layout = new VBoxLayout() };
			private readonly Widget treeNodeWidget;
			private readonly Joint parentJoint;
			private readonly List<Joint> offsetJoints;
			private readonly int level;
			private readonly DocumentHierarchyTreeView view;
			private bool expanded = false;
			private string filter;
			private List<TreeNode> savedNodes = new List<TreeNode>();

			public NodeList ChildTreeNodes => treeNodesContainer.Nodes;
			public int Index { get; private set; }
			public TreeNode ParentTreeNode { get; private set; }
			public bool Expanded {
				get => expanded;
				set {
					if (Expandable && expanded != value) {
						ToggleExpanded();
					}
				}
			}
			public bool Expandable { get; private set; }
			public float RowHeight => treeNodeWidget.Height;

			public TreeNode(DocumentHierarchyTreeView view, Node rootNode, TreeNode parentTreeNode, JointType jointType, List<Joint> offsetJoints, int level, int index, bool isLast)
			{
				this.rootNode = rootNode;
				this.level = level;
				ParentTreeNode = parentTreeNode;
				this.view = view;
				Index = index;
				Expandable = rootNode.Nodes.Count > 0;
				Layout = new VBoxLayout();

				treeNodeWidget = new Widget {
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
					if (view.selected == this) {
						w.PrepareRendererState();
						var color = view.parent.IsFocused()
							? ColorTheme.Current.Hierarchy.SelectionColor
							: ColorTheme.Current.Hierarchy.GrayedSelectionColor;
						Renderer.DrawRect(0, 0, w.Width, w.Height, color);
					}
					HightlightText();
				}));
				treeNodeWidget.Clicked += () => view.SelectTreeNode(this);
				treeNodeWidget.Gestures.Add(new DoubleClickGesture(NavigateToNode));
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
					Renderer.DrawLine(0, w.Height / 2, w.Width / 2 - iconSize, w.Height / 2, ColorTheme.Current.Hierarchy.JointColor);
					if (Expanded) {
						Renderer.DrawLine(w.Width / 2, w.Height / 2 + iconSize, w.Width / 2, w.Height, ColorTheme.Current.Hierarchy.JointColor);
					}
				}));
			}

			private Widget CreateLabel()
			{
				label = new ThemedSimpleText { Padding = new Thickness(defaultPadding) };
				label.AddChangeWatcher(() => rootNode.Id, t => label.Text = String.IsNullOrEmpty(t) ? $"<{rootNode.GetType().Name}>" : t);
				return label;
			}

			private void HightlightText()
			{
				if (String.IsNullOrEmpty(filter) || !MatchesFilter()) {
					return;
				}
				treeNodeWidget.PrepareRendererState();
				int index = 0;
				int previousIndex = 0;
				var filterSize = label.Font.MeasureTextLine(filter, label.FontHeight, label.LetterSpacing);
				var size = Vector2.Zero;
				var text = rootNode.Id.ToLower();
				var pos = label.CalcPositionInSpaceOf(treeNodeWidget);
				pos.X += label.Padding.Left;
				pos.Y += label.Padding.Top;
				while ((index = text.IndexOf(filter, previousIndex)) >= 0) {
					var skippedText = text.Substring(previousIndex, index - previousIndex);
					var skippedSize = label.Font.MeasureTextLine(skippedText, label.FontHeight, label.LetterSpacing);
					size.X += skippedSize.X;
					size.Y = Mathf.Max(size.Y, skippedSize.Y);
					Renderer.DrawRect(pos.X + size.X, pos.Y, pos.X + size.X + filterSize.X, pos.Y + size.Y, ColorTheme.Current.Hierarchy.MatchColor);
					size.X += filterSize.X;
					size.Y = Mathf.Max(size.Y, filterSize.Y);
					previousIndex = index + filter.Length;
				}
			}

			public void ToggleExpanded()
			{
				expanded = !expanded;
				UpdateButtonTexture();
				if (!Expanded) {
					treeNodesContainer.Unlink();
					return;
				}
				AddNode(treeNodesContainer);
			}

			public void UpdateButtonTexture()
			{
				expandButton.Texture = Expanded ? IconPool.GetTexture("Timeline.minus") : IconPool.GetTexture("Timeline.plus");
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
				if (Expandable == expandable) {
					return;
				}
				Expandable = expandable;
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
				for (int i = 0; i < rootNode.Nodes.Count; ++i) {
					var node = rootNode.Nodes[i];
					var index = rootNodes.IndexOf(node);
					if (index >= 0) {
						treeNodesContainer.AddNode(savedNodes[index]);
						savedNodes[index].SetJoints(JointType.VLine, JointType.TShaped);
						savedNodes[index].Index = i;
					} else {
						treeNodesContainer.AddNode(new TreeNode(view, node, this, JointType.TShaped, offsetJoints, level + 1, i, isLast: false));
					}
				}
				UpdateExpandable();
				savedNodes = treeNodesContainer.Nodes.Cast<TreeNode>().ToList();
				if (!String.IsNullOrEmpty(filter)) {
					view.root.Filter(filter);
				}
			}

			internal void NavigateToNode()
			{
				if (this == view.root) {
					return;
				}
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
				this.filter = filter.ToLower();
				treeNodesContainer.Nodes.Clear();
				bool result = MatchesFilter();
				int index = 0;
				foreach (var node in savedNodes) {
					if (node.Filter(filter)) {
						result = true;
						treeNodesContainer.AddNode(node);
						node.SetJoints(JointType.VLine, JointType.TShaped);
						node.Index = index++;
					}
				}
				UpdateExpandable();
				if (result) {
					Expanded = true;
				}
				return result;
			}

			public bool MatchesFilter()
			{
				return String.IsNullOrEmpty(filter) || (rootNode.Id?.ToLower().Contains(filter.ToLower()) ?? false);
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
