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

		public TreeView(Widget parent, Node rootNode)
		{
			this.rootNode = rootNode;
			scrollView = new ThemedScrollView();
			scrollView.Content.Layout = new VBoxLayout();
			parent.AddNode(scrollView);
			scrollView.Content.AddNode(new TreeNode(rootNode, null, JointType.LShaped, new List<Joint>(), 0, isLast: true));
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
			private readonly ToolbarButton expandButton;
			private readonly Joint expandJoint = new Joint(JointType.HLine, defaultJointWidth);
			private readonly Widget treeNodesContainer = new Widget { Layout = new VBoxLayout() };
			private readonly Joint parentJoint;
			private readonly List<Joint> offsetJoints;
			private readonly int level;
			private readonly TreeNode parentTreeNode;
			private bool expanded = false;
			private bool expandable;

			public TreeNode(Node rootNode, TreeNode parentTreeNode, JointType jointType, List<Joint> offsetJoints, int level, bool isLast)
			{
				this.rootNode = rootNode;
				this.level = level;
				this.parentTreeNode = parentTreeNode;
				expandable = rootNode.Nodes.Count > 0;
				Layout = new VBoxLayout();

				var treeNodeWidget = new Widget { Layout = new HBoxLayout() };

				this.offsetJoints = offsetJoints.Select(w => (Joint)w.Clone()).ToList();
				foreach (var ow in this.offsetJoints) {
					treeNodeWidget.AddNode(ow);
				}
				this.offsetJoints.Add(new Joint(isLast ? JointType.None : JointType.VLine, defaultJointWidth));

				treeNodeWidget.AddNode(parentJoint = new Joint(jointType, defaultJointWidth));
				treeNodeWidget.AddNode(rootNode.Nodes.Count > 0 ? (expandButton = CreateExpandButton()) : (Widget)expandJoint);
				treeNodeWidget.AddNode(CreateLabel());
				AddNode(treeNodeWidget);

				this.AddChangeWatcher(() => rootNode.NextSibling, _ => parentTreeNode?.UpdateChildTreeNodes());
				this.AddChangeWatcher(() => rootNode.Nodes.Count, _ => UpdateChildTreeNodes());

				UpdateChildTreeNodes();
			}

			private ToolbarButton CreateExpandButton()
			{
				var button = new ToolbarButton {
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
				button.Clicked += ToggleExpanded;
				button.CompoundPresenter.Insert(0, new DelegatePresenter<Widget>(w => {
					w.PrepareRendererState();
					var iconSize = (w.Width - 2 * defaultPadding) / 2;
					Renderer.DrawLine(0, w.Height / 2, w.Width / 2 - iconSize, w.Height / 2, Color4.Gray);
					if (expanded) {
						Renderer.DrawLine(w.Width / 2, w.Height / 2 + iconSize, w.Width / 2, w.Height, Color4.Gray);
					}
				}));
				return button;
			}

			private ThemedSimpleText CreateLabel()
			{
				var label = new ThemedSimpleText {
					Padding = new Thickness(defaultPadding),
					Text = Document.Current.RootNode == rootNode ? "root" : rootNode.Id
				};
				label.AddChangeWatcher(() => rootNode.Id, t => label.Text = t);
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
				this.parentJoint.Type = joint;
			}

			private void UpdateChildTreeNodes()
			{
				var treeNodes = treeNodesContainer.Nodes.Cast<TreeNode>().ToList();
				var rootNodes = treeNodes.Select(t => t.rootNode).ToList();
				treeNodesContainer.Nodes.Clear();
				foreach (var node in rootNode.Nodes) {
					var index = rootNodes.IndexOf(node);
					if (index >= 0) {
						treeNodesContainer.AddNode(treeNodes[index]);
						treeNodes[index].SetJoints(JointType.VLine, JointType.TShaped);
					} else {
						treeNodesContainer.AddNode(new TreeNode(node, this, JointType.TShaped, offsetJoints, level + 1, isLast: false));
					}
				}
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
