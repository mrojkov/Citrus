using Lime;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Core;
using Tangerine.UI.Timeline.Processors;

namespace Tangerine.UI.Timeline.Components
{
	public static class ShowTiedNodesContextMenu
	{
		public static void Create(RollNodeView view)
		{
			var rowNode = view.NodeData?.Node;
			if (rowNode == null) {
				return;
			}
			var siblings = rowNode.Parent.Nodes;
			var nodes = new List<Node>();
			var menu = new Menu();
			switch (rowNode) {
				case Bone bone:
					foreach (var node in siblings) {
						var isTiedAsMesh =
							node is DistortionMesh m &&
							TiedIndicationProcessor.GetTiedBoneIndexes(m).Any();
						SkinningWeights skinningWeights;
						var isTiedAsWidget =
							node is Widget widget &&
							(skinningWeights = widget.SkinningWeights) != null &&
							(
								skinningWeights.Bone0.Index == bone.Index ||
								skinningWeights.Bone1.Index == bone.Index ||
								skinningWeights.Bone2.Index == bone.Index ||
								skinningWeights.Bone3.Index == bone.Index
							);
						if (isTiedAsMesh || isTiedAsWidget) {
							menu.Add(new Command(node.Id, new ShowTiedNodes(node).Execute));
							nodes.Add(node);
						}
					}
					AddShowAll(menu, nodes);
					break;
				case Widget widget:
					if (widget is DistortionMesh mesh) {
						var indexes = new HashSet<int>();
						foreach (var index in TiedIndicationProcessor.GetTiedBoneIndexes(mesh)) {
							if (!indexes.Contains(index)) {
								AddBone(menu, index, nodes, siblings, true);
								indexes.Add(index);
							}
						}
					}
					AddBones(new Property<SkinningWeights>(() => widget.SkinningWeights), menu, nodes, siblings, true);
					AddShowAll(menu, nodes);
					break;
				case DistortionMeshPoint point:
					AddBones(new Property<SkinningWeights>(() => point.SkinningWeights), menu, nodes, rowNode.Parent.Parent.Nodes, false);
					break;
			}
			menu.Popup();
		}

		private static void AddShowAll(Menu menu, List<Node> nodes)
		{
			menu.Add(Command.MenuSeparator);
			menu.Add(new Command("Show all", new ShowTiedNodes(nodes.ToArray()).Execute));
		}

		private static void AddBones(Property<SkinningWeights> property, Menu menu, List<Node> nodes, IEnumerable<Node> parentNodes, bool enabled)
		{
			var skinningWeights = property.Getter();
			if (skinningWeights.Bone0.Index != 0) {
				AddBone(menu, skinningWeights.Bone0.Index, nodes, parentNodes, enabled);
			}
			if (skinningWeights.Bone1.Index != 0) {
				AddBone(menu, skinningWeights.Bone1.Index, nodes, parentNodes, enabled);
			}
			if (skinningWeights.Bone2.Index != 0) {
				AddBone(menu, skinningWeights.Bone2.Index, nodes, parentNodes, enabled);
			}
			if (skinningWeights.Bone3.Index != 0) {
				AddBone(menu, skinningWeights.Bone3.Index, nodes, parentNodes, enabled);
			}
		}

		private static void AddBone(Menu menu, int index, List<Node> nodes, IEnumerable<Node> parentNodes, bool enabled)
		{
			var bone = parentNodes.GetBone(index);
			menu.Add(new Command(bone.Id, new ShowTiedNodes(bone).Execute) {
				Enabled = enabled,
			});
			nodes.Add(bone);
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
					Core.Operations.ClearRowSelection.Perform();
					foreach (var node in nodes) {
						Core.Operations.SelectNode.Perform(node);
					}
				});
			}
		}
	}
}
