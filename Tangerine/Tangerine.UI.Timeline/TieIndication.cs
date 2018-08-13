using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class TieIndication : ToolbarButton
	{
		public static readonly Vector2 defaultMinMaxSize = new Vector2(21, 16);
		public static readonly Thickness defaultPadding = new Thickness { Left = 5 };

		public List<Node> TiedNodes { get; set; } = new List<Node>();

		public TieIndication(ITexture texture, bool clickable = false)
		{
			Highlightable = false;
			MinMaxSize = defaultMinMaxSize;
			Padding = defaultPadding;
			LayoutCell = new LayoutCell(Alignment.RightCenter);
			Texture = texture;
			if (clickable) {
				Clicked += () => TieIndicationContextMenu.Create(TiedNodes);
			}
		}

		public void AddTiedNode(Node node)
		{
			TiedNodes.Add(node);
		}

		public void AddTiedNodes(params Node[] nodes)
		{
			TiedNodes.AddRange(nodes);
		}

		public void ClearTiedNodes()
		{
			TiedNodes.Clear();
		}
	}

	public class TieIndicationContainer : Widget
	{
		private readonly Widget container = new Widget { Layout = new HBoxLayout() };

		public TieIndicationContainer()
		{
			Layout = new HBoxLayout();
			AddNode(container);
		}

		public TTieIndication Get<TTieIndication>() where TTieIndication : TieIndication, new()
		{
			return Components.Get<TieIndicationComponent<TTieIndication>>()?.TieIndication;
		}

		public TTieIndication GetOrAdd<TTieIndication>() where TTieIndication : TieIndication, new()
		{
			return Components.GetOrAdd<TieIndicationComponent<TTieIndication>>().TieIndication;
		}

		public TTieIndication EnableIndication<TTieIndication>() where TTieIndication : TieIndication, new()
		{
			var indication = GetOrAdd<TTieIndication>();
			if (indication.Parent == null) {
				container.AddNode(indication);
			}
			return indication;
		}

		public void DisableIndication<TTieIndication>() where TTieIndication : TieIndication, new()
		{
			var indication = Get<TTieIndication>();
			if (indication != null) {
				indication.Unlink();
				indication.ClearTiedNodes();
			}
		}

		private class TieIndicationComponent<TTieIndication> : NodeComponent where TTieIndication : TieIndication, new()
		{
			public TTieIndication TieIndication { get; private set; } = new TTieIndication();
		}
	}

	public static class TieIndicationContextMenu
	{
		public static void Create(List<Node> nodes)
		{
			var menu = new Menu();
			bool isSameParent = true;
			var parent = nodes.FirstOrDefault()?.Parent;
			foreach (var node in nodes) {
				menu.Add(new Command(node.Id, new ShowTiedNodes(node).Execute));
				isSameParent &= node.Parent == parent;
			}
			if (nodes.Count() > 0) {
				if (isSameParent && nodes.Count() > 1) {
					menu.Add(Command.MenuSeparator);
					menu.Add(new Command("Show All", new ShowTiedNodes(nodes.ToArray()).Execute));
				}
				menu.Popup();
			}
		}

		private class ShowTiedNodes : CommandHandler
		{
			private readonly Node[] nodes;

			public ShowTiedNodes(params Node[] nodes)
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
