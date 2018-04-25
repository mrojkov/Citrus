using System;
using System.Linq;
using Lime;
using Tangerine.UI;
using Tangerine.Core;

namespace Tangerine
{
	public class GroupNodes : DocumentCommandHandler
	{
		public override void Execute()
		{
			var nodes = Document.Current?.SelectedNodes().Where(IsValidNode).ToList();
			Rectangle aabb;
			if (!UI.Utils.CalcAABB(nodes, (Widget)Document.Current.Container, out aabb)) {
				return;
			}
			var container = Document.Current.Container;
			var loc = container.RootFolder().Find(nodes[0]);
			var group = (Frame)Core.Operations.CreateNode.Perform(container, loc, typeof(Frame));
			group.Id = nodes[0].Id + "Group";
			group.Pivot = Vector2.Half;
			group.Position = aabb.Center;
			group.Size = aabb.Size;
			foreach (var n in nodes) {
				Core.Operations.UnlinkFolderItem.Perform(container, n);
			}
			int i = 0;
			foreach (var node in nodes) {
				Core.Operations.InsertFolderItem.Perform(group, new FolderItemLocation(group.RootFolder(), i++), node);
				if (node is Widget) {
					TransformPropertyAndKeyframes<Vector2>(node, nameof(Widget.Position), v => v - aabb.A);
				}
			}
			Core.Operations.ClearRowSelection.Perform();
			Core.Operations.SelectNode.Perform(group);
		}

		public static void TransformPropertyAndKeyframes<T>(Node node, string propertyId, Func<T, T> transformer)
		{
			var v = new Property<T>(node, propertyId).Value;
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

		public override bool GetEnabled() => Document.Current.SelectedNodes().Any(IsValidNode);

		public static bool IsValidNode(Node node) => (node is Widget) || (node is Bone) || (node is Audio) || (node is ImageCombiner);
	}

	public class UngroupNodes : DocumentCommandHandler
	{
		public override void Execute()
		{
			var groups = Document.Current?.SelectedNodes().OfType<Frame>().ToList();
			if (groups.Count == 0) {
				return;
			}
			var container = (Widget)Document.Current.Container;
			var p = container.RootFolder().Find(groups[0]);
			Core.Operations.ClearRowSelection.Perform();
			foreach (var group in groups) {
				Core.Operations.UnlinkFolderItem.Perform(container, group);
			}
			foreach (var group in groups) {
				foreach (var node in group.Nodes.ToList().Where(GroupNodes.IsValidNode)) {
					Core.Operations.UnlinkFolderItem.Perform(group, node);
					Core.Operations.InsertFolderItem.Perform(container, p, node);
					Core.Operations.SelectNode.Perform(node);
					p.Index++;
					var widget = node as Widget;
					if (widget != null) {
						GroupNodes.TransformPropertyAndKeyframes<Vector2>(node, nameof(Widget.Position), v => group.CalcLocalToParentTransform() * v);
						GroupNodes.TransformPropertyAndKeyframes<Vector2>(node, nameof(Widget.Scale), v => group.Scale * v);
						GroupNodes.TransformPropertyAndKeyframes<float>(node, nameof(Widget.Rotation), v => group.Rotation + v);
						GroupNodes.TransformPropertyAndKeyframes<Color4>(node, nameof(Widget.Color), v => group.Color * v);
					}
				}
			}
		}

		public override bool GetEnabled() => Core.Document.Current.SelectedNodes().Any(i => i is Frame);
	}

	public class InsertTimelineColumn : DocumentCommandHandler
	{
		public override void Execute()
		{
			Core.Operations.TimelineHorizontalShift.Perform(UI.Timeline.Timeline.Instance.CurrentColumn, 1);
		}
	}

	public class RemoveTimelineColumn : DocumentCommandHandler
	{
		public override void Execute()
		{
			Core.Operations.TimelineHorizontalShift.Perform(UI.Timeline.Timeline.Instance.CurrentColumn, -1);
		}
	}

	public class GroupContentsToMorphableMeshes : DocumentCommandHandler
	{
		public override void Execute()
		{
			var nodes = Document.Current?.SelectedNodes().Editable().ToList();
			var container = Document.Current.Container;
			Core.Operations.ClearRowSelection.Perform();
			foreach (var node in nodes) {
				var clone = node.Clone();
				var loc = container.RootFolder().Find(node);
				Core.Operations.UnlinkFolderItem.Perform(container, node);
				Core.Operations.InsertFolderItem.Perform(container, loc, clone);
				new MorphableMeshBuilder().BuildNodeContents(clone, MorphableMeshBuilder.Options.None);
				Core.Operations.SelectNode.Perform(clone);
			}
		}
	}

	public class ExportScene : DocumentCommandHandler
	{
		public override void Execute()
		{
			var nodes = Document.Current?.SelectedNodes().Editable().ToList();
			var container = Document.Current.Container;
			if (nodes.Count != 1) {
				AlertDialog.Show("Please, select a single node");
				return;
			}
			Export(nodes[0]);
		}

		public static void Export(Node node)
		{
			var dlg = new FileDialog {
				AllowedFileTypes = new string[] { Document.Current.GetFileExtension() },
				Mode = FileDialogMode.Save,
				InitialDirectory = Project.Current.GetSystemDirectory(Document.Current.Path),
			};
			if (dlg.RunModal()) {
				string assetPath;
				if (!Project.Current.TryGetAssetPath(dlg.FileName, out assetPath)) {
					AlertDialog.Show("Can't save the document outside the project directory");
				} else {
					try {
						Document.WriteNodeToFile(assetPath, DocumentFormat.Tan, node);
					} catch (System.Exception e) {
						AlertDialog.Show(e.Message);
					}
				}
			}
		}
	}

	public class UpsampleAnimationTwice : DocumentCommandHandler
	{
		public override void Execute()
		{
			UpsampleNodeAnimation(Document.Current.RootNode);
		}

		private void UpsampleNodeAnimation(Node node)
		{
			foreach (var a in node.Animations) {
				foreach (var m in a.Markers) {
					Core.Operations.SetProperty.Perform(m, "Frame", m.Frame * 2);
				}
			}
			foreach (var a in node.Animators) {
				foreach (var k in a.Keys) {
					Core.Operations.SetProperty.Perform(k, "Frame", k.Frame * 2);
				}
			}
			foreach (var n in node.Nodes) {
				UpsampleNodeAnimation(n);
			}
		}
	}
}
