using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine.Panels
{
	internal class DocumentHierarchyTreeView
	{
		private readonly ThemedScrollView scrollView = new ThemedScrollView();
		private readonly TreeNode root;
		private readonly Widget parent;
		private readonly ThemedSimpleText nothingToShow = new ThemedSimpleText("Nothing to show");
		private TreeNode selected;
		private string filter;
		private uint filterVersion;

		public DocumentHierarchyTreeView(Widget parent, Node rootNode)
		{
			this.parent = parent;
			scrollView.Content.Layout = new VBoxLayout();
			scrollView.Content.AddNode(root = new TreeNode(this, rootNode, null, JointType.LShaped, new List<Joint>(), 0, 0, isLast: true, string.Empty));
		}

		public void Detach() => scrollView.Unlink();
		public void Attach() => parent.AddNode(scrollView);
		public bool IsAttached() => scrollView.Parent != null;

		public void Filter(string filter)
		{
			if (this.filter == filter) {
				return;
			}
			this.filter = filter;
			filterVersion++;
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

			protected override Lime.RenderObject GetRenderObject()
			{
				if (Type == JointType.None) {
					return null;
				}
				var ro = RenderObjectPool<RenderObject>.Acquire();
				ro.CaptureRenderState(this);
				ro.Type = Type;
				ro.Size = Size;
				ro.Color = ColorTheme.Current.Hierarchy.JointColor;
				return ro;
			}

			private class RenderObject : WidgetRenderObject
			{
				public Vector2 Size;
				public Color4 Color;
				public JointType Type;

				public override void Render()
				{
					PrepareRenderState();
					switch (Type) {
						case JointType.HLine:
							Renderer.DrawLine(0, Size.Y / 2, Size.X, Size.Y / 2, Color);
							break;
						case JointType.VLine:
							Renderer.DrawLine(Size.X / 2, 0, Size.X / 2, Size.Y, Color);
							break;
						case JointType.TShaped:
							Renderer.DrawLine(Size.X / 2, 0, Size.X / 2, Size.Y, Color);
							Renderer.DrawLine(Size.X / 2, Size.Y / 2, Size.X, Size.Y / 2, Color);
							break;
						case JointType.LShaped:
							Renderer.DrawLine(Size.X / 2, 0, Size.X / 2, Size.Y / 2, Color);
							Renderer.DrawLine(Size.X / 2, Size.Y / 2, Size.X, Size.Y / 2, Color);
							break;
						default:
							throw new ArgumentException();
					}
				}
			}
		}

		private class TreeNode : Widget
		{
			private const float DefaultPadding = 2;
			private const float DefaultJointWidth = 19;

			private readonly Node rootNode;
			private ToolbarButton expandButton;
			private ThemedSimpleText label;
			private readonly Joint expandJoint = new Joint(JointType.HLine, DefaultJointWidth);
			private readonly Widget treeNodesContainer = new Widget { Layout = new VBoxLayout() };
			private readonly Widget treeNodeWidget;
			private readonly Joint parentJoint;
			private readonly List<Joint> offsetJoints;
			private readonly int level;
			private readonly DocumentHierarchyTreeView view;
			private bool expanded;
			private string filter;
			private List<TreeNode> savedNodes = new List<TreeNode>();
			private string lowercaseId;

			public NodeList ChildTreeNodes => treeNodesContainer.Nodes;
			public int Index { get; private set; }
			public TreeNode ParentTreeNode { get; }
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

			private readonly bool[] skippedChangeWatcherUpdate = { false, false };

			public TreeNode(DocumentHierarchyTreeView view, Node rootNode, TreeNode parentTreeNode, JointType jointType, IEnumerable<Joint> offsetJoints, int level, int index, bool isLast, string filter)
			{
				this.filter = filter;
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
				this.offsetJoints.Add(new Joint(isLast ? JointType.None : JointType.VLine, DefaultJointWidth));

				treeNodeWidget.AddNode(parentJoint = new Joint(jointType, DefaultJointWidth));
				CreateExpandButton();
				treeNodeWidget.AddNode(rootNode.Nodes.Count > 0 ? expandButton : (Widget)expandJoint);
				treeNodeWidget.AddNode(CreateLabel());
				treeNodeWidget.AddNode(new Widget());
				treeNodeWidget.CompoundPresenter.Add(new SyncDelegatePresenter<Widget>(w => {
					if (view.selected == this) {
						w.PrepareRendererState();
						var color = view.parent.IsFocused()
							? ColorTheme.Current.Hierarchy.SelectionColor
							: ColorTheme.Current.Hierarchy.GrayedSelectionColor;
						Renderer.DrawRect(0, 0, w.Width, w.Height, color);
					}
					HighlightText();
				}));
				var clickGesture = new ClickGesture();
				clickGesture.Began += () => view.SelectTreeNode(this);
				treeNodeWidget.Gestures.Add(clickGesture);
				treeNodeWidget.Gestures.Add(new DoubleClickGesture(NavigateToNode));
				AddNode(treeNodeWidget);

				if (parentTreeNode != null) {
					this.AddChangeWatcher(() => rootNode.NextSibling, _ => {
						if (!firstTimeSiblingChanged) {
							firstTimeSiblingChanged = true;
						} else {
							parentTreeNode.UpdateChildren();
						}
					});
				}
				this.AddChangeWatcher(() => rootNode.Nodes.Version, _ => {
					if (!firstTimeVersionChanged) {
						firstTimeVersionChanged = true;
					} else {
						UpdateChildren();
					}
				});
				UpdateChildren();
			}

			private bool firstTimeVersionChanged;
			private bool firstTimeSiblingChanged;

			private void CreateExpandButton()
			{
				expandButton = new ToolbarButton {
					Highlightable = false,
					MinMaxSize = new Vector2(DefaultJointWidth, DefaultJointWidth + DefaultPadding * 2),
					Padding = new Thickness {
						Left = DefaultPadding,
						Right = DefaultPadding,
						Top = DefaultPadding * 2,
						Bottom = DefaultPadding * 2
					},
					Texture = IconPool.GetTexture("Timeline.plus"),
				};
				expandButton.Clicked += ToggleExpanded;
				expandButton.CompoundPresenter.Insert(0, new SyncDelegatePresenter<Widget>(w => {
					w.PrepareRendererState();
					float iconSize = (w.Width - 2 * DefaultPadding) / 2;
					Renderer.DrawLine(0, w.Height / 2, w.Width / 2 - iconSize, w.Height / 2, ColorTheme.Current.Hierarchy.JointColor);
					if (Expanded) {
						Renderer.DrawLine(w.Width / 2, w.Height / 2 + iconSize, w.Width / 2, w.Height, ColorTheme.Current.Hierarchy.JointColor);
					}
				}));
			}

			private void SetLabelText(string id)
			{
				label.Text = string.IsNullOrEmpty(id) ? $"<{rootNode.GetType().Name}>" : id;
				lowercaseId = label.Text.ToLower();
			}

			private Widget CreateLabel()
			{
				label = new ThemedSimpleText { Padding = new Thickness(DefaultPadding) };
				label.AddChangeWatcher(() => rootNode.Id, SetLabelText);
				SetLabelText(rootNode.Id);
				return label;
			}

			private void HighlightText()
			{
				if (string.IsNullOrEmpty(filter) || !MatchesFilter()) {
					return;
				}
				treeNodeWidget.PrepareRendererState();
				int index;
				int previousIndex = 0;
				var filterSize = label.Font.MeasureTextLine(filter, label.FontHeight, label.LetterSpacing);
				var size = Vector2.Zero;
				var pos = label.CalcPositionInSpaceOf(treeNodeWidget);
				pos.X += label.Padding.Left;
				pos.Y += label.Padding.Top;
				while ((index = lowercaseId.IndexOf(filter, previousIndex, StringComparison.Ordinal)) >= 0) {
					string skippedText = lowercaseId.Substring(previousIndex, index - previousIndex);
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

			private void UpdateButtonTexture()
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

			private void UpdateChildren()
			{
				var rootNodes = savedNodes.Select(t => t.rootNode).ToList();
				treeNodesContainer.Nodes.Clear();
				for (int i = 0; i < rootNode.Nodes.Count; ++i) {
					var node = rootNode.Nodes[i];
					int index = rootNodes.IndexOf(node);
					if (index >= 0) {
						treeNodesContainer.AddNode(savedNodes[index]);
						savedNodes[index].SetJoints(JointType.VLine, JointType.TShaped);
						savedNodes[index].Index = i;
					} else {
						treeNodesContainer.AddNode(new TreeNode(view, node, this, JointType.TShaped, offsetJoints, level + 1, i, isLast: false, filter));
					}
				}
				UpdateExpandable();
				savedNodes = treeNodesContainer.Nodes.Cast<TreeNode>().ToList();
				if (!string.IsNullOrEmpty(filter)) {
					filterVersion = uint.MaxValue;
					Filter(filter);
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
				while (externalScene != Document.Current.RootNode && string.IsNullOrEmpty(externalScene.ContentsPath)) {
					path.Push(externalScene.Parent.Nodes.IndexOf(externalScene));
					externalScene = externalScene.Parent;
				}
				string currentScenePath = Document.Current.Path;
				if (path.Count < level) {
					Document externalSceneDocument;
					try {
						externalSceneDocument = Project.Current.OpenDocument(externalScene.ContentsPath);
					} catch (System.Exception e) {
						AlertDialog.Show(e.Message);
						return;
					}
					externalSceneDocument.SceneNavigatedFrom = currentScenePath;
					node = externalSceneDocument.RootNode;
					foreach (int i in path) {
						node = node.Nodes[i];
					}
				} else {
					// to ensure TangerineFlags.DisplayContent set by EnterNode is cleared
					Document.Current.History.DoTransaction(() => {
						Core.Operations.LeaveNode.Perform();
					});
				}
				Document.Current.History.DoTransaction(() => {
					if (Core.Operations.EnterNode.Perform(node.Parent, selectFirstNode: false)) {
						Core.Operations.SelectNode.Perform(node);
					}
				});
			}

			private uint filterVersion;
			private bool savedFilterResult;

			public bool Filter(string newFilter)
			{
				if (filterVersion == view.filterVersion) {
					return savedFilterResult;
				}
				filterVersion = view.filterVersion;
				filter = newFilter;
				treeNodesContainer.Nodes.Clear();
				bool result = MatchesFilter();
				int index = 0;
				foreach (var node in savedNodes) {
					if (!node.Filter(newFilter)) {
						continue;
					}
					result = true;
					treeNodesContainer.AddNode(node);
					node.SetJoints(JointType.VLine, JointType.TShaped);
					node.Index = index++;
				}
				UpdateExpandable();
				if (result) {
					Expanded = true;
				}
				savedFilterResult = result;
				return result;
			}

			public bool MatchesFilter()
			{
				return string.IsNullOrEmpty(filter) || (lowercaseId?.Contains(filter) ?? false);
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
