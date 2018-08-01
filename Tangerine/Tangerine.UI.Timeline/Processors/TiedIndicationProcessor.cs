using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline.Processors
{
    public class TiedIndicationProcessor : SymmetricOperationProcessor
    {
        public override void Process(IOperation op)
        {
            var rows = Document.Current.Rows;
            var ties = new Dictionary<int, HashSet<Node>>();
            for (int i = 0; i < rows.Count; ++i)
            {
				if (rows[i].Components.Get<RowView>()?.RollRow is RollNodeView view) {
					var node = view.NodeData?.Node;

					Property<SkinningWeights> property = null;
					if (node is Widget widget) {
						property = new Property<SkinningWeights>(() => widget.SkinningWeights);
					}
					if (node is DistortionMeshPoint point) {
						property = new Property<SkinningWeights>(() => point.SkinningWeights);	
					}
					if (property != null) {
						var sw = property.Getter();
						if (sw?.IsEmpty() ?? true) {
							view.TiedIndicator.Color = Color4.Transparent;
						}
						else {
							view.TiedIndicator.Color = Color4.White;
							AddTie(ties, sw.Bone0.Index, node);
							AddTie(ties, sw.Bone1.Index, node);
							AddTie(ties, sw.Bone2.Index, node);
							AddTie(ties, sw.Bone3.Index, node);
						}
					}

					if (node is DistortionMesh mesh) {
						bool tied = false;
						foreach (var index in GetTiedBoneIndexes(mesh)) {
							AddTie(ties, index, mesh);
							tied = true;
						}
						view.TiedIndicator.Color = tied ? Color4.White : Color4.Transparent;
					}
				}
			}
            for (int i = 0; i < rows.Count; ++i)
            {
				if (
					rows[i].Components.Get<RowView>()?.RollRow is RollNodeView view &&
					view.NodeData?.Node is Bone bone
				) {
					if (ties.ContainsKey(bone.Index)) {
						view.TiedIndicator.Color = Color4.White;
						var sb = new StringBuilder(bone.Id).Append(": ");
						foreach (var node in ties[bone.Index]) {
							sb.Append(node.Id).Append(", ");
						}
						view.Label.Text = sb.Remove(sb.Length - 2, 2).ToString();
					} else {
						view.Label.Text = bone.Id;
						view.TiedIndicator.Color = Color4.Transparent;
					}
				}
            }
        }

		internal static IEnumerable<int> GetTiedBoneIndexes(DistortionMesh mesh)
		{
			foreach (var point in mesh.Nodes.OfType<DistortionMeshPoint>()) {
				var sw = point.SkinningWeights;
				if (!sw?.IsEmpty() ?? false) {
					if (sw.Bone0.Index != 0) {
						yield return sw.Bone0.Index;
					}
					if (sw.Bone1.Index != 0) {
						yield return sw.Bone1.Index;
					}
					if (sw.Bone2.Index != 0) {
						yield return sw.Bone2.Index;
					}
					if (sw.Bone3.Index != 0) {
						yield return sw.Bone3.Index;
					}
				}
			}
		}

		private static void AddTie(Dictionary<int, HashSet<Node>> ties, int index, Node node)
		{
			if (index == 0) {
				return;
			}
			if (ties.ContainsKey(index)) {
				ties[index].Add(node);
			} else {
				ties.Add(index, new HashSet<Node> { node });
			}
		}
    }
}
