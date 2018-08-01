using Lime;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangerine.Core;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline.Processors
{
    public class TiedIndicationProcessor : SymmetricOperationProcessor
    {
        public override void Process(IOperation op)
        {
            var ties = new Dictionary<int, HashSet<Node>>();
            foreach (var row in Document.Current.Rows)
            {
				if (row.Components.Get<RowView>()?.RollRow is RollNodeView view) {
					var node = view.NodeData?.Node;
					switch (node) {
						case Widget widget:
							AddTies(ties, new Property<SkinningWeights>(() => widget.SkinningWeights), view, widget);
							if (widget is DistortionMesh mesh) {
								bool tied = false;
								foreach (var index in GetTiedBoneIndexes(mesh)) {
									AddTie(ties, index, mesh);
									tied = true;
								}
								view.TiedIndicator.Color = tied ? Color4.White : Color4.Transparent;
							}
							break;
						case DistortionMeshPoint point:
							AddTies(ties, new Property<SkinningWeights>(() => point.SkinningWeights), view, point);
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

		private static void AddTies(Dictionary<int, HashSet<Node>> ties, Property<SkinningWeights> property, RollNodeView view, Node node)
		{
			var skinningWeights = property.Getter();
			if (skinningWeights?.IsEmpty() ?? true) {
				view.TiedIndicator.Color = Color4.Transparent;
			} else {
				view.TiedIndicator.Color = Color4.White;
				AddTie(ties, skinningWeights.Bone0.Index, node);
				AddTie(ties, skinningWeights.Bone1.Index, node);
				AddTie(ties, skinningWeights.Bone2.Index, node);
				AddTie(ties, skinningWeights.Bone3.Index, node);
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
