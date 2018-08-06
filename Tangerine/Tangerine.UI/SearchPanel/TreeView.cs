using Lime;
using System;
using System.Collections.Generic;
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
			scrollView.Content.AddNode(new TreeNode(rootNode, JointType.Last, new List<Widget>(), last: true));
		}

		private enum JointType {
			HLine,
			VLine,
			Middle,
			Last
		}

		private class Joint : Widget
		{
			private readonly JointType type;

			public Joint(JointType type, float size)
			{
				this.type = type;
				MinMaxWidth = size;
			}

			public override void Render()
			{
				base.Render();
				PrepareRendererState();
				switch (type) {
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
			private readonly ToolbarButton button;
			private readonly Widget nodeContainer;

			public TreeNode(Node rootNode, JointType jointType, List<Widget> offsetWidgets, bool last)
			{
				this.rootNode = rootNode;
				Layout = new VBoxLayout();
				var widget = new Widget { Layout = new HBoxLayout() };
				foreach (var ow in offsetWidgets) {
					widget.AddNode(ow.Clone());
				}
				widget.AddNode(new Joint(jointType, 18));
				widget.AddNode(rootNode.Nodes.Count > 0 ? (Widget)(button = CreateExpandButton()) : new Joint(JointType.HLine, 18));
				widget.AddNode(CreateLabel());
				AddNode(widget);
				nodeContainer = new Widget {
					Layout = new VBoxLayout()
				};
				offsetWidgets.Add(last ? (Widget)new HSpacer(18) : new Joint(JointType.VLine, 18));
				FillNodeContainer(offsetWidgets);
				offsetWidgets.RemoveAt(offsetWidgets.Count - 1);
			}

			private ToolbarButton CreateExpandButton()
			{
				var button = new ToolbarButton {
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
				return new ThemedSimpleText {
					Padding = new Thickness(5),
					Text = Document.Current.RootNode == rootNode ? "root" : rootNode.Id
				};
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
				if (button != null) {
					button.Texture = expanded ? IconPool.GetTexture("Timeline.minus") : IconPool.GetTexture("Timeline.plus");
				}
			}

			private void FillNodeContainer(List<Widget> offsetWidgets)
			{
				if (rootNode.Nodes.Count == 0) {
					return;
				}
				for (int i = 0; i < rootNode.Nodes.Count - 1; ++i) {
					nodeContainer.AddNode(new TreeNode(rootNode.Nodes[i], JointType.Middle, offsetWidgets, last: false));
				}
				nodeContainer.AddNode(new TreeNode(rootNode.Nodes[rootNode.Nodes.Count - 1], JointType.Last, offsetWidgets, last: true));
			}
		}
	}
}
