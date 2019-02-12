using Lime;
using System.Collections.Generic;
using Tangerine.Core;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline
{
	public class SplineGearLinkIndicationProcessor : SymmetricOperationProcessor
	{
		private class SplineGearLinkIndicatorButton : LinkIndicatorButton
		{
			public SplineGearLinkIndicatorButton() : base(NodeIconPool.GetTexture(typeof(SplineGear)), clickable: true)
			{
				Tooltip = "Linked to Widget(s)";
			}
		}

		private class SplineGear3DLinkIndicatorButton : LinkIndicatorButton
		{
			public SplineGear3DLinkIndicatorButton() : base(NodeIconPool.GetTexture(typeof(SplineGear3D)), clickable: true)
			{
				Tooltip = "Linked to Node3D(s)";
			}
		}

		private Node container;

		public override void Process(IOperation op)
		{
			if (!op.IsChangingDocument && container == Document.Current.Container) {
				return;
			}
			container = Document.Current.Container;
			var links = new Dictionary<Widget, HashSet<SplineGear>>();
			var links3D = new Dictionary<Node3D, HashSet<SplineGear3D>>();
			foreach (var row in Document.Current.Rows) {
				if (!(row.Components.Get<RowView>()?.RollRow is RollNodeView view)) {
					continue;
				}
				view.LinkIndicatorButtonContainer.DisableIndication<SplineGearLinkIndicatorButton>();
				view.LinkIndicatorButtonContainer.DisableIndication<SplineGear3DLinkIndicatorButton>();
				switch (view.NodeData?.Node) {
					case SplineGear splineGear:
						LinkWidget(links, splineGear, splineGear.Widget, view);
						LinkWidget(links, splineGear, splineGear.Spline, view);
						break;
					case SplineGear3D splineGear3D:
						LinkNode3D(links3D, splineGear3D, splineGear3D.Node, view);
						LinkNode3D(links3D, splineGear3D, splineGear3D.Spline, view);
						break;
				}
			}
			foreach (var row in Document.Current.Rows) {
				if (!(row.Components.Get<RowView>()?.RollRow is RollNodeView view)) {
					continue;
				}
				switch (view.NodeData?.Node) {
					case Widget widget:
						ProcessWidgetLinks(links, widget, view);
						break;
					case Node3D node3D:
						ProcessNode3DLinks(links3D, node3D, view);
						break;
				}
			}
		}

		private static void Link<TLinkIndicatorButton, TNode, TGear>(IDictionary<TNode, HashSet<TGear>> links, TGear gear, TNode node, RollNodeView view)
			where TLinkIndicatorButton : LinkIndicatorButton, new()
			where TNode : Node
		{
			if (node == null) {
				return;
			}
			if (!links.ContainsKey(node)) {
				links.Add(node, new HashSet<TGear> { gear });
			} else {
				links[node].Add(gear);
			}
			view.LinkIndicatorButtonContainer.EnableIndication<TLinkIndicatorButton>().AddLinkedNode(node);
		}

		private static void ProcessLinks<TLinkIndicatorButton, TNode, TGear, TSpline>(IReadOnlyDictionary<TNode, HashSet<TGear>> links, TNode node, RollNodeView view)
			where TLinkIndicatorButton : LinkIndicatorButton, new()
			where TNode : Node
			where TSpline : Node
			where TGear : Node
		{
			if (!links.ContainsKey(node)) {
				view.RefreshLabelColor();
				return;
			}
			var indication = view.LinkIndicatorButtonContainer.EnableIndication<TLinkIndicatorButton>();
			var gears = links[node];
			if (gears.Count > 1) {
				if (!(node is TSpline)) {
					view.Label.Color = indication.Color = Theme.Colors.RedText;
				}
				indication.Tooltip = $"Linked to {gears.Count} {typeof(TGear).Name}s";
			} else {
				view.RefreshLabelColor();
				indication.Color = Color4.White;
				indication.Tooltip = $"Linked to {typeof(TGear).Name}";
			}
			foreach (var gear in gears) {
				indication.AddLinkedNode(gear);
			}
		}

		private void LinkWidget(IDictionary<Widget, HashSet<SplineGear>> links, SplineGear splineGear, Widget widget, RollNodeView view) =>
			Link<SplineGearLinkIndicatorButton, Widget, SplineGear>(links, splineGear, widget, view);

		private void LinkNode3D(IDictionary<Node3D, HashSet<SplineGear3D>> links, SplineGear3D splineGear3D, Node3D node3D, RollNodeView view) =>
			Link<SplineGear3DLinkIndicatorButton, Node3D, SplineGear3D>(links, splineGear3D, node3D, view);

		private void ProcessWidgetLinks(IReadOnlyDictionary<Widget, HashSet<SplineGear>> links, Widget widget, RollNodeView view) =>
			ProcessLinks<SplineGearLinkIndicatorButton, Widget, SplineGear, Spline>(links, widget, view);

		private void ProcessNode3DLinks(IReadOnlyDictionary<Node3D, HashSet<SplineGear3D>> links, Node3D node3D, RollNodeView view) =>
			ProcessLinks<SplineGear3DLinkIndicatorButton, Node3D, SplineGear3D, Spline3D>(links, node3D, view);
	}
}
