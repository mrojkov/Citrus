using Lime;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangerine.Core;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline
{
    public class BoneLinkIndicationProcessor : SymmetricOperationProcessor
    {
		private class BoneLinkIndicatorButton : LinkIndicatorButton
		{
			public BoneLinkIndicatorButton() : base(NodeIconPool.GetTexture(typeof(Bone)), clickable: true)
			{
				Tooltip = "Linked to Bone(s)";
			}
		}

	    private Node container;

        public override void Process(IOperation op)
        {
	        if (!op.IsChangingDocument && container == Document.Current.Container) {
		        return;
	        }
	        container = Document.Current.Container;
			var links = new Dictionary<Bone, HashSet<RollNodeView>>();
            foreach (var row in Document.Current.Rows)
            {
	            if (!(row.Components.Get<RowView>()?.RollRow is RollNodeView view)) {
		            continue;
	            }
	            view.LinkIndicatorButtonContainer.DisableIndication<BoneLinkIndicatorButton>();
	            var node = view.NodeData?.Node;
	            switch (node) {
		            case Widget widget:
			            var nodes = widget.Parent.Nodes;
			            AddLinks(links, widget.SkinningWeights, view, nodes);
			            if (widget is DistortionMesh mesh) {
				            foreach (var index in GetLinkedBoneIndexes(mesh)) {
					            AddLink(links, index, view, nodes);
				            }
			            }
			            break;
		            case DistortionMeshPoint point:
			            AddLinks(links, point.SkinningWeights, view, point.Parent.Parent.Nodes);
			            break;
	            }
            }
            foreach (var row in Document.Current.Rows)
            {
				if (
					row.Components.Get<RowView>()?.RollRow is RollNodeView view &&
					view.NodeData?.Node is Bone bone
				) {
					if (links.ContainsKey(bone)) {
						var indication = view.LinkIndicatorButtonContainer.EnableIndication<BoneLinkIndicatorButton>();
						indication.ClearLinkedNodes();
						var sb = new StringBuilder(bone.Id).Append(": ");
						foreach (var nodeView in links[bone]) {
							var node = nodeView.NodeData.Node;
							indication.AddLinkedNode(node);
							indication.Tooltip = "Linked to Node(s)";
							sb.Append(node.Id).Append(", ");
						}
						view.Label.Text = sb.Remove(sb.Length - 2, 2).ToString();
					} else {
						view.Label.Text = bone.Id;
						view.LinkIndicatorButtonContainer.DisableIndication<BoneLinkIndicatorButton>();
					}
				}
            }
        }

		internal static IEnumerable<int> GetLinkedBoneIndexes(DistortionMesh mesh)
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

		private static void AddLinks(IDictionary<Bone, HashSet<RollNodeView>> links, SkinningWeights skinningWeights, RollNodeView view, NodeList nodes)
		{
			if (skinningWeights?.IsEmpty() ?? true) {
				return;
			}
			AddLink(links, skinningWeights.Bone0.Index, view, nodes);
			AddLink(links, skinningWeights.Bone1.Index, view, nodes);
			AddLink(links, skinningWeights.Bone2.Index, view, nodes);
			AddLink(links, skinningWeights.Bone3.Index, view, nodes);
		}

		private static void AddLink(IDictionary<Bone, HashSet<RollNodeView>> links, int index, RollNodeView view, NodeList nodes)
		{
			if (index == 0) {
				return;
			}
			var bone = nodes.GetBone(index);
			if (bone == null) {
				return;
			}
			if (!links.ContainsKey(bone)) {
				links.Add(bone, new HashSet<RollNodeView> { view });
				view.LinkIndicatorButtonContainer.EnableIndication<BoneLinkIndicatorButton>().AddLinkedNode(bone);
			} else if (!links[bone].Contains(view)) {
				links[bone].Add(view);
				view.LinkIndicatorButtonContainer.EnableIndication<BoneLinkIndicatorButton>().AddLinkedNode(bone);
			}
		}
    }
}
