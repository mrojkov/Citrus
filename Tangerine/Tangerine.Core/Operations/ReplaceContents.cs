using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.Core.Operations
{
	public class ReplaceContents : Operation
	{
		public override bool IsChangingDocument => true;

		private Node from;
		private Node to;

		public static void Perform(Node from, Node to)
		{
			DocumentHistory.Current.Perform(new ReplaceContents(from, to));
		}

		protected ReplaceContents(Node from, Node to)
		{
			this.from = from;
			this.to = to;
		}

		private static void ReplaceContent(Node from, Node to)
		{
			to.Components.Remove(typeof(Node.AssetBundlePathComponent));
			var assetBundlePathComponent = from.Components.Get<Node.AssetBundlePathComponent>();
			if (assetBundlePathComponent != null) {
				to.Components.Add(Serialization.Clone(assetBundlePathComponent));
			}
			to.Animations.Clear();
			var animations = from.Animations.ToList();
			from.Animations.Clear();
			to.Animations.AddRange(animations);
			var nodes = from.Nodes.ToList();
			from.Nodes.Clear();
			to.Nodes.Clear();
			to.Nodes.AddRange(nodes);
		}

		public class Processor : OperationProcessor<ReplaceContents>
		{
			protected override void InternalDo(ReplaceContents op) {
				ReplaceContent(op.from.Clone(), op.to);
			}

			protected override void InternalRedo(ReplaceContents op) { }
			protected override void InternalUndo(ReplaceContents op) { }
		}
	}
}
