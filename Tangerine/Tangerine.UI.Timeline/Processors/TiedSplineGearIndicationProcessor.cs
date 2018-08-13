using Lime;
using System.Collections.Generic;
using Tangerine.Core;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline.Processors
{
	public class TiedSplineGearIndicationProcessor : SymmetricOperationProcessor
	{
		private class TiedSplineGearIndication : TieIndication
		{
			public TiedSplineGearIndication() : base(NodeIconPool.GetTexture(typeof(SplineGear)), clickable: true)
			{
				Tip = "Tied to Widget(s)";
			}
		}

		private class TiedSplineGear3DIndication : TieIndication
		{
			public TiedSplineGear3DIndication() : base(NodeIconPool.GetTexture(typeof(SplineGear3D)), clickable: true)
			{
				Tip = "Tied to Node3D(s)";
			}
		}

		public override void Process(IOperation op)
		{
			var ties = new Dictionary<Widget, HashSet<SplineGear>>();
			var ties3D = new Dictionary<Node3D, HashSet<SplineGear3D>>();
			foreach (var row in Document.Current.Rows) {
				if (row.Components.Get<RowView>()?.RollRow is RollNodeView view) {
					view.TieIndicationContainer.DisableIndication<TiedSplineGearIndication>();
					view.TieIndicationContainer.DisableIndication<TiedSplineGear3DIndication>();
					switch (view.NodeData?.Node) {
						case SplineGear splineGear:
							TieWidget(ties, splineGear, splineGear.Widget, view);
							TieWidget(ties, splineGear, splineGear.Spline, view);
							break;
						case SplineGear3D splineGear3D:
							TieNode3D(ties3D, splineGear3D, splineGear3D.Node, view);
							TieNode3D(ties3D, splineGear3D, splineGear3D.Spline, view);
							break;
					}
				}
			}
			foreach (var row in Document.Current.Rows) {
				if (row.Components.Get<RowView>()?.RollRow is RollNodeView view) {
					switch (view.NodeData?.Node) {
						case Widget widget:
							ProcessWidgetTies(ties, widget, view);
							break;
						case Node3D node3D:
							ProcessNode3DTies(ties3D, node3D, view);
							break;
					}
				}
			}
		}

		private void Tie<TTieIndication, TNode, TGear>(Dictionary<TNode, HashSet<TGear>> ties, TGear gear, TNode node, RollNodeView view)
			where TTieIndication : TieIndication, new()
			where TNode : Node
		{
			if (node == null) {
				return;
			}
			if (!ties.ContainsKey(node)) {
				ties.Add(node, new HashSet<TGear> { gear });
			} else {
				ties[node].Add(gear);
			}
			view.TieIndicationContainer.EnableIndication<TTieIndication>().AddTiedNode(node);
		}

		private void ProcessTies<TTieIndication, TNode, TGear, TSpline>(Dictionary<TNode, HashSet<TGear>> ties, TNode node, RollNodeView view)
			where TTieIndication : TieIndication, new()
			where TNode : Node
			where TSpline : Node
			where TGear : Node
		{
			if (!ties.ContainsKey(node)) {
				view.Label.Color = Theme.Colors.BlackText;
				return;
			}
			var indication = view.TieIndicationContainer.EnableIndication<TTieIndication>();
			var gears = ties[node];
			if (gears.Count > 1) {
				if (!(node is TSpline)) {
					view.Label.Color = indication.Color = Theme.Colors.RedText;
				}
				indication.Tip = $"Tied to {gears.Count} {typeof(TGear).Name}s";
			} else {
				view.Label.Color = Theme.Colors.BlackText;
				indication.Color = Color4.White;
				indication.Tip = $"Tied to {typeof(TGear).Name}";
			}
			foreach (var gear in gears) {
				indication.AddTiedNode(gear);
			}
		}

		private void TieWidget(Dictionary<Widget, HashSet<SplineGear>> ties, SplineGear splineGear, Widget widget, RollNodeView view) =>
			Tie<TiedSplineGearIndication, Widget, SplineGear>(ties, splineGear, widget, view);

		private void TieNode3D(Dictionary<Node3D, HashSet<SplineGear3D>> ties3D, SplineGear3D splineGear3D, Node3D node3D, RollNodeView view) =>
			Tie<TiedSplineGear3DIndication, Node3D, SplineGear3D>(ties3D, splineGear3D, node3D, view);

		private void ProcessWidgetTies(Dictionary<Widget, HashSet<SplineGear>> ties, Widget widget, RollNodeView view) =>
			ProcessTies<TiedSplineGearIndication, Widget, SplineGear, Spline>(ties, widget, view);

		private void ProcessNode3DTies(Dictionary<Node3D, HashSet<SplineGear3D>> ties3D, Node3D node3D, RollNodeView view) =>
			ProcessTies<TiedSplineGear3DIndication, Node3D, SplineGear3D, Spline3D>(ties3D, node3D, view);
	}
}
