using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
			var parentNodes = rowNode.Parent.Nodes;
			var nodes = new List<Node>();
			var menu = new Menu();

			if (rowNode is Bone bone) {
				foreach (var node in parentNodes) {
					if (
						node is Widget wid &&
						(
							wid.SkinningWeights?.Bone0.Index == bone.Index ||
							wid.SkinningWeights?.Bone1.Index == bone.Index ||
							wid.SkinningWeights?.Bone2.Index == bone.Index ||
							wid.SkinningWeights?.Bone3.Index == bone.Index
						)
					) {
						menu.Add(new Command(wid.Id, new ShowTiedNodes(wid).Execute));
						nodes.Add(wid);
					}
					if (
						node is DistortionMesh m &&
						TiedIndicationProcessor.GetTiedBoneIndexes(m).Any()
					) {
						menu.Add(new Command(m.Id, new ShowTiedNodes(m).Execute));
					}
				}
			}

			if (view.NodeData?.Node is DistortionMesh mesh) {
				var indexes = new HashSet<int>();
				foreach (var index in TiedIndicationProcessor.GetTiedBoneIndexes(mesh)) {
					if (!indexes.Contains(index)) {
						AddBone(menu, index, nodes, parentNodes, true);
						indexes.Add(index);
					}
				}
			}

			Property<SkinningWeights> property = null;
			bool enabled = true;
			if (rowNode is Widget widget) {
				property = new Property<SkinningWeights>(() => widget.SkinningWeights);
			}
			if (rowNode is DistortionMeshPoint point) {
				property = new Property<SkinningWeights>(() => point.SkinningWeights);
				enabled = false;
				parentNodes = rowNode.Parent.Parent.Nodes;
			}
			if (property != null) {
				var sw = property.Getter();
				if (sw.Bone0.Index != 0) {
					AddBone(menu, sw.Bone0.Index, nodes, parentNodes, enabled);
				}
				if (sw.Bone1.Index != 0) {
					AddBone(menu, sw.Bone1.Index, nodes, parentNodes, enabled);
				}
				if (sw.Bone2.Index != 0) {
					AddBone(menu, sw.Bone2.Index, nodes, parentNodes, enabled);
				}
				if (sw.Bone3.Index != 0) {
					AddBone(menu, sw.Bone3.Index, nodes, parentNodes, enabled);
				}
			}

			if (enabled) {
				menu.Add(Command.MenuSeparator);
				menu.Add(new Command("Show all", new ShowTiedNodes(nodes.ToArray()).Execute));
			}
			menu.Popup();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
