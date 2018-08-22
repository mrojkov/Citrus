using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class LinkIndicatorButton : ToolbarButton
	{
		private static readonly Vector2 defaultMinMaxSize = new Vector2(21, 16);
		private static readonly Thickness defaultPadding = new Thickness { Left = 5 };

		public List<Node> LinkedNodes { get; set; } = new List<Node>();

		public LinkIndicatorButton(ITexture texture, bool clickable = false)
		{
			Highlightable = false;
			MinMaxSize = defaultMinMaxSize;
			Padding = defaultPadding;
			LayoutCell = new LayoutCell(Alignment.RightCenter);
			Texture = texture;
			if (clickable) {
				Clicked += () => LinkIndicationContextMenu.Create(LinkedNodes);
			}
		}

		public void AddLinkedNode(Node node)
		{
			LinkedNodes.Add(node);
		}

		public void AddLinkedNodes(params Node[] nodes)
		{
			LinkedNodes.AddRange(nodes);
		}

		public void ClearLinkedNodes()
		{
			LinkedNodes.Clear();
		}
	}

	public class LinkIndicatorButtonContainer : Widget
	{
		private readonly Widget container = new Widget { Layout = new HBoxLayout() };

		public LinkIndicatorButtonContainer()
		{
			Layout = new HBoxLayout();
			AddNode(container);
		}

		public TLinkIndicatorButton Get<TLinkIndicatorButton>() where TLinkIndicatorButton : LinkIndicatorButton, new()
		{
			return Components.Get<LinkIndicatorButtonComponent<TLinkIndicatorButton>>()?.LinkIndicatorButton;
		}

		public TLinkIndicatorButton GetOrAdd<TLinkIndicatorButton>() where TLinkIndicatorButton : LinkIndicatorButton, new()
		{
			return Components.GetOrAdd<LinkIndicatorButtonComponent<TLinkIndicatorButton>>().LinkIndicatorButton;
		}

		public TLinkIndicatorButton EnableIndication<TLinkIndicatorButton>() where TLinkIndicatorButton : LinkIndicatorButton, new()
		{
			var indication = GetOrAdd<TLinkIndicatorButton>();
			if (indication.Parent == null) {
				container.AddNode(indication);
			}
			return indication;
		}

		public void DisableIndication<TLinkIndicatorButton>() where TLinkIndicatorButton : LinkIndicatorButton, new()
		{
			var indication = Get<TLinkIndicatorButton>();
			if (indication != null) {
				indication.Unlink();
				indication.ClearLinkedNodes();
			}
		}

		private class LinkIndicatorButtonComponent<TLinkIndicatorButton> : NodeComponent where TLinkIndicatorButton : LinkIndicatorButton, new()
		{
			public TLinkIndicatorButton LinkIndicatorButton { get; } = new TLinkIndicatorButton();
		}
	}

	public static class LinkIndicationContextMenu
	{
		public static void Create(List<Node> nodes)
		{
			var menu = new Menu();
			var isSameParent = true;
			var parent = nodes.FirstOrDefault()?.Parent;
			foreach (var node in nodes) {
				menu.Add(new Command(node.Id, new ShowLinkedNodes(node).Execute));
				isSameParent &= node.Parent == parent;
			}
			if (nodes.Count > 0) {
				if (isSameParent && nodes.Count() > 1) {
					menu.Add(Command.MenuSeparator);
					menu.Add(new Command("Show All", new ShowLinkedNodes(nodes.ToArray()).Execute));
				}
				menu.Popup();
			}
		}

		private class ShowLinkedNodes : CommandHandler
		{
			private readonly Node[] nodes;

			public ShowLinkedNodes(params Node[] nodes)
			{
				this.nodes = nodes;
			}

			public override void Execute()
			{
				Document.Current.History.DoTransaction(() => {
					var parent = nodes.First().Parent;
					if (parent != Document.Current.Container) {
						Core.Operations.EnterNode.Perform(parent, false);
					}
					Core.Operations.ClearRowSelection.Perform();
					foreach (var node in nodes) {
						Core.Operations.SelectNode.Perform(node);
					}
				});
			}
		}
	}
}
