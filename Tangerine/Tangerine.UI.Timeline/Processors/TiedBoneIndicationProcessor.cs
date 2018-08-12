using Lime;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangerine.Core;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline.Processors
{
    public class TiedBoneIndicationProcessor : SymmetricOperationProcessor
    {
		private class TiedBoneIndication : TieIndication
		{
			public TiedBoneIndication() : base(NodeIconPool.GetTexture(typeof(Bone)), clickable: true)
			{
			}
		}

        public override void Process(IOperation op)
        {
            var ties = new Dictionary<Bone, HashSet<RollNodeView>>();
            foreach (var row in Document.Current.Rows)
            {
				if (row.Components.Get<RowView>()?.RollRow is RollNodeView view) {
					view.TieIndicationContainer.DisableIndication<TiedBoneIndication>();
					var node = view.NodeData?.Node;
					switch (node) {
						case Widget widget:
							var nodes = widget.Parent.Nodes;
							AddTies(ties, new Property<SkinningWeights>(() => widget.SkinningWeights), view, nodes);
							if (widget is DistortionMesh mesh) {
								foreach (var index in GetTiedBoneIndexes(mesh)) {
									AddTie(ties, index, view, nodes);
								}
							}
							break;
						case DistortionMeshPoint point:
							AddTies(ties, new Property<SkinningWeights>(() => point.SkinningWeights), view, point.Parent.Parent.Nodes);
							break;
					}
				}
			}
            foreach (var row in Document.Current.Rows)
            {
				if (
					row.Components.Get<RowView>()?.RollRow is RollNodeView view &&
					view.NodeData?.Node is Bone bone
				) {
					if (ties.ContainsKey(bone)) {
						var indication = view.TieIndicationContainer.EnableIndication<TiedBoneIndication>();
						indication.ClearTiedNodes();
						var sb = new StringBuilder(bone.Id).Append(": ");
						foreach (var nodeView in ties[bone]) {
							var node = nodeView.NodeData.Node;
							indication.AddTiedNode(node);
							sb.Append(node.Id).Append(", ");
						}
						view.Label.Text = sb.Remove(sb.Length - 2, 2).ToString();
					} else {
						view.Label.Text = bone.Id;
						view.TieIndicationContainer.DisableIndication<TiedBoneIndication>();
					}
				}
            }
        }

		internal static IEnumerable<int> GetTiedBoneIndexes(DistortionMesh mesh)
		{
			foreach (var point in mesh.Nodes.OfType<DistortionMeshPoint>()) {
				if (point.SkinningWeights?.IsEmpty() ?? true) {
					continue;
				}
				if (point.SkinningWeights.Bone0.Index != 0) {
					yield return point.SkinningWeights.Bone0.Index;
				}
				if (point.SkinningWeights.Bone1.Index != 0) {
					yield return point.SkinningWeights.Bone1.Index;
				}
				if (point.SkinningWeights.Bone2.Index != 0) {
					yield return point.SkinningWeights.Bone2.Index;
				}
				if (point.SkinningWeights.Bone3.Index != 0) {
					yield return point.SkinningWeights.Bone3.Index;
				}
			}
		}

		private static void AddTies(Dictionary<Bone, HashSet<RollNodeView>> ties, Property<SkinningWeights> property, RollNodeView view, NodeList nodes)
		{
			var skinningWeights = property.Getter();
			if (skinningWeights?.IsEmpty() ?? true) {
				return;
			}
			AddTie(ties, skinningWeights.Bone0.Index, view, nodes);
			AddTie(ties, skinningWeights.Bone1.Index, view, nodes);
			AddTie(ties, skinningWeights.Bone2.Index, view, nodes);
			AddTie(ties, skinningWeights.Bone3.Index, view, nodes);
		}

		private static void AddTie(Dictionary<Bone, HashSet<RollNodeView>> ties, int index, RollNodeView view, NodeList nodes)
		{
			if (index == 0) {
				return;
			}
			var bone = nodes.GetBone(index);
			if (!ties.ContainsKey(bone)) {
				ties.Add(bone, new HashSet<RollNodeView> { view });
				view.TieIndicationContainer.EnableIndication<TiedBoneIndication>().AddTiedNode(bone);
			} else if (!ties[bone].Contains(view)) {
				ties[bone].Add(view);
				view.TieIndicationContainer.EnableIndication<TiedBoneIndication>().AddTiedNode(bone);
			}
		}
    }
}
