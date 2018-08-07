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
			scrollView.Content.AddNode(new TreeNode(rootNode, null, JointType.Last, new List<Joint>(), 0, last: true));
		}

		private enum JointType {
			None,
			HLine,
			VLine,
			Middle,
			Last
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
					case JointType.Middle:
						Renderer.DrawLine(Width / 2, 0, Width / 2, Height, Color4.Gray);
						Renderer.DrawLine(Width / 2, Height / 2, Width, Height / 2, Color4.Gray);
						break;
					case JointType.Last:
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
			private readonly Node rootNode;
			private bool expanded = false;
			private ToolbarButton button;
			private readonly Joint hJoint = new Joint(JointType.HLine, 18);
			private readonly Widget nodeContainer;
			private readonly Joint joint;
			private readonly List<Joint> offsetWidgets;
			private readonly int level;
			private readonly TreeNode parentTreeNode;
			private bool expandable;

			public TreeNode(Node rootNode, TreeNode parentTreeNode, JointType jointType, List<Joint> offsetWidgets, int level, bool last)
			{
				this.rootNode = rootNode;
				this.level = level;
				this.parentTreeNode = parentTreeNode;
				expandable = rootNode.Nodes.Count > 0;
				Layout = new VBoxLayout();
				var widget = new Widget { Layout = new HBoxLayout() };
				this.offsetWidgets = offsetWidgets.Select(w => (Joint)w.Clone()).ToList();
				foreach (var ow in this.offsetWidgets) {
					widget.AddNode(ow);
				}
				widget.AddNode(joint = new Joint(jointType, 18));
				widget.AddNode(rootNode.Nodes.Count > 0 ? (Widget)CreateExpandButton() : hJoint);
				widget.AddNode(CreateLabel());
				AddNode(widget);
				nodeContainer = new Widget { Layout = new VBoxLayout() };
				this.offsetWidgets.Add(new Joint(last ? JointType.None : JointType.VLine, 18));
				UpdateChildTreeNodes();
				this.AddChangeWatcher(() => rootNode.NextSibling, _ => parentTreeNode?.UpdateChildTreeNodes());
				this.AddChangeWatcher(() => rootNode.Nodes.Count, _ => UpdateChildTreeNodes());
			}

			private ToolbarButton CreateExpandButton()
			{
				button = new ToolbarButton {
					Highlightable = false,
					MinMaxSize = new Vector2(18, 26),
					Padding = new Thickness { Top = 8, Left = 4, Right = 4, Bottom = 8 },
					Texture = IconPool.GetTexture("Timeline.plus")
				};
				button.Clicked += ToggleExpanded;
				button.CompoundPresenter.Insert(0, new DelegatePresenter<Widget>(w => {
					w.PrepareRendererState();
					Renderer.DrawLine(0, w.Height / 2, w.Width / 2 - 5, w.Height / 2, Color4.Gray);
					if (expanded) {
						Renderer.DrawLine(w.Width / 2, w.Height / 2 + 5, w.Width / 2, w.Height, Color4.Gray);
					}
				}));
				return button;
			}

			private ThemedSimpleText CreateLabel()
			{
				var label = new ThemedSimpleText {
					Padding = new Thickness(5),
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
					nodeContainer.Unlink();
					return;
				}
				AddNode(nodeContainer);
			}

			public void UpdateButtonTexture()
			{
				button.Texture = expanded ? IconPool.GetTexture("Timeline.minus") : IconPool.GetTexture("Timeline.plus");
			}

			public override void Render()
			{
				//base.Render();
				//PrepareRendererState();
				//Renderer.DrawRectOutline(0, 0, Width, Height, Color4.Red);
			}

			private static void ReplaceWith(Node with, Node what)
			{
				var node = what.Parent;
				int index = node.Nodes.IndexOf(what);
				what.Unlink();
				node.Nodes.Insert(index, with);
			}

			private void SetExpandable(bool expandable)
			{
				if (this.expandable == expandable) {
					return;
				}
				this.expandable = expandable;
				if (expandable) {
					ReplaceWith(button, hJoint);
					return;
				}
				ReplaceWith(hJoint, button);
			}

			private void SetOffsetJoint(int index, JointType jointType)
			{
				offsetWidgets[index].Type = jointType;
				foreach (var node in nodeContainer.Nodes.Cast<TreeNode>()) {
					node.SetOffsetJoint(index, jointType);
				}
			}

			private void UpdateChildTreeNodes()
			{
				var treeNodes = nodeContainer.Nodes.Cast<TreeNode>().ToList();
				var rootNodes = treeNodes.Select(t => t.rootNode).ToList();
				nodeContainer.Nodes.Clear();
				foreach (var node in rootNode.Nodes) {
					var index = rootNodes.IndexOf(node);
					if (index >= 0) {
						nodeContainer.AddNode(treeNodes[index]);
						treeNodes[index].SetOffsetJoint(level + 1, JointType.VLine);
						treeNodes[index].joint.Type = JointType.Middle;
					} else {
						nodeContainer.AddNode(new TreeNode(node, this, JointType.Middle, offsetWidgets, level + 1, last: false));
					}
				}
				if (nodeContainer.Nodes.Count > 0) {
					var lastNode = (TreeNode)nodeContainer.Nodes.Last();
					lastNode.SetOffsetJoint(level + 1, JointType.None);
					lastNode.joint.Type = JointType.Last;
					SetExpandable(true);
				} else {
					SetExpandable(false);
				}
			}
		}
	}
}
