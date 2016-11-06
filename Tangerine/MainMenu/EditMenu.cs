using System;
using System.Linq;
using Lime;
using Tangerine.UI;

namespace Tangerine
{
	public class GroupCommand : Command
	{
		public override string Text => "Group";
		public override Shortcut Shortcut => new Shortcut(Modifiers.Command, Key.G);

		public override void Execute()
		{
			var nodes = Core.Document.Current?.SelectedNodes().Editable().Where(IsValidNode).ToList();
			Rectangle aabb;
			if (!Utils.CalcAABB(nodes, (Widget)Core.Document.Current.Container, out aabb)) {
				return;
			}
			var firstNodeIndex = Core.Document.Current.Container.Nodes.IndexOf(nodes[0]);
			var group = (Frame)Core.Operations.CreateNode.Perform(Core.Document.Current.Container, firstNodeIndex, typeof(Frame));
			group.Id = nodes[0].Id + "Group";
			group.Pivot = Vector2.Half;
			group.Position = aabb.Center;
			group.Size = aabb.Size;
			foreach (var n in nodes) {
				Core.Operations.UnlinkNode.Perform(n);
			}
			int i = 0;
			foreach (var node in nodes) {
				Core.Operations.InsertNode.Perform(group, i++, node);
				TransformPropertyAndKeyframes<Vector2>(node, nameof(Widget.Position), v => v - aabb.A);
			}
			Core.Operations.ClearRowSelection.Perform();
			Core.Operations.SelectNode.Perform(group);
		}

		public static void TransformPropertyAndKeyframes<T>(Node node, string propertyId, Func<T, T> transformer)
		{
			var v = new Core.Property<T>(node, propertyId).Value;
			Core.Operations.SetProperty.Perform(node, propertyId, transformer(v));
			foreach (var a in node.Animators) {
				if (a.TargetProperty == propertyId) {
					foreach (var k in a.Keys.ToList()) {
						var c = k.Clone();
						c.Value = transformer((T)c.Value);
						Core.Operations.SetKeyframe.Perform(node, a.TargetProperty, a.AnimationId, c);
					}
				}
			}
		}

		public override bool Enabled => Core.Document.Current?.SelectedNodes().Editable().Any(IsValidNode) ?? false;

		bool IsValidNode(Node node) => (node is Widget) || (node is PointObject) || (node is Bone) || (node is Audio) || (node is ImageCombiner);
	}
}