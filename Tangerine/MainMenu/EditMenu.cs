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

		public static bool IsValidNode(Node node) => (node is Widget) || (node is Bone) || (node is Audio) || (node is ImageCombiner);
	}

	public class UngroupCommand : Command
	{
		public override string Text => "Ungroup";
		public override Shortcut Shortcut => new Shortcut(Modifiers.Command | Modifiers.Shift, Key.G);

		public override void Execute()
		{
			var groups = Core.Document.Current?.SelectedNodes().Editable().Where(n => n is Frame).ToList();
			if (groups.Count == 0) {
				return;
			}
			var container = (Widget)Core.Document.Current.Container;
			var i = container.Nodes.IndexOf(groups[0]);
			Core.Operations.ClearRowSelection.Perform();
			foreach (var group in groups) {
				Core.Operations.UnlinkNode.Perform(group);
			}
			foreach (var group in groups) {
				foreach (var node in group.Nodes.ToList().Where(GroupCommand.IsValidNode)) {
					Core.Operations.UnlinkNode.Perform(node);
					Core.Operations.InsertNode.Perform(container, i++, node);
					Core.Operations.SelectNode.Perform(node);
					var widget = node as Widget;
					if (widget != null) {
						GroupCommand.TransformPropertyAndKeyframes<Vector2>(node, nameof(Widget.Position), v => container.CalcLocalToParentTransform() * v);
						GroupCommand.TransformPropertyAndKeyframes<Vector2>(node, nameof(Widget.Scale), v => container.Scale * v);
						GroupCommand.TransformPropertyAndKeyframes<float>(node, nameof(Widget.Rotation), v => container.Rotation + v);
						GroupCommand.TransformPropertyAndKeyframes<Color4>(node, nameof(Widget.Color), v => container.Color * v);
					}
				}
			}
		}

		public override bool Enabled => Core.Document.Current?.SelectedNodes().Editable().Any(i => i is Frame) ?? false;
	}
}