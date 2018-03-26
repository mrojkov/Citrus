using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using System.Reflection;
using Tangerine.Core.Components;

namespace Tangerine.Core.Operations
{
	public class SetProperty : Operation
	{
		public readonly object Obj;
		public readonly object Value;
		public readonly PropertyInfo Property;

		public override bool IsChangingDocument => true;

		public static void Perform(object obj, string propertyName, object value)
		{
			Document.Current.History.Perform(new SetProperty(obj, propertyName, value));
		}

		protected SetProperty(object obj, string propertyName, object value)
		{
			Obj = obj;
			Value = value;
			Property = obj.GetType().GetProperty(propertyName);
		}

		public class Processor : OperationProcessor<SetProperty>
		{
			class Backup { public object Value; }

			protected override void InternalRedo(SetProperty op)
			{
				op.Save(new Backup { Value = op.Property.GetValue(op.Obj, null) });
				op.Property.SetValue(op.Obj, op.Value, null);
			}

			protected override void InternalUndo(SetProperty op)
			{
				var v = op.Restore<Backup>().Value;
				op.Property.SetValue(op.Obj, v, null);
			}
		}
	}

	public class SetAnimableProperty
	{
		public static void Perform(object @object, string propertyName, object value)
		{
			SetProperty.Perform(@object, propertyName, value);
			IAnimator animator;
			var animable = @object as IAnimable;
			if (animable != null && animable.Animators.TryFind(propertyName, out animator, Document.Current.AnimationId)) {
				var type = animable.GetType().GetProperty(propertyName).PropertyType;
				var key =
					animator.ReadonlyKeys.FirstOrDefault(i => i.Frame == Document.Current.AnimationFrame)?.Clone() ??
					Keyframe.CreateForType(type);
				key.Frame = Document.Current.AnimationFrame;
				key.Function = animator.Keys.LastOrDefault(k => k.Frame < key.Frame)?.Function ?? KeyFunction.Linear;
				key.Value = value;
				SetKeyframe.Perform(animable, propertyName, Document.Current.AnimationId, key);
			}
		}
	}

	public class SetAnimablePropertyWhenNeeded
	{
		public static void Perform(object obj, string propertyName, object value)
		{
			var animable = obj as IAnimable;
			IAnimator animator;
			if (animable != null && animable.Animators.TryFind(propertyName, out animator, Document.Current.AnimationId) &&
				animator.ReadonlyKeys.Count > 0) {
				SetAnimableProperty.Perform(obj, propertyName, value);

			} else {
				SetProperty.Perform(obj, propertyName, value);
			}
		}
	}

	public class RemoveKeyframe : Operation
	{
		public readonly int Frame;
		public readonly IAnimator Animator;
		public readonly IAnimable Owner;

		public override bool IsChangingDocument => true;

		public static void Perform(IAnimator animator, int frame)
		{
			Document.Current.History.Perform(new RemoveKeyframe(animator, frame));
		}

		private RemoveKeyframe(IAnimator animator, int frame)
		{
			Frame = frame;
			Animator = animator;
			Owner = Animator.Owner;
		}

		public class Processor : OperationProcessor<RemoveKeyframe>
		{
			class Backup { public IKeyframe Keyframe; }

			protected override void InternalRedo(RemoveKeyframe op)
			{
				var kf = op.Animator.Keys.FirstOrDefault(k => k.Frame == op.Frame);
				op.Save(new Backup { Keyframe = kf });
				op.Animator.Keys.Remove(kf);
				if (op.Animator.Keys.Count == 0) {
					op.Owner.Animators.Remove(op.Animator);
				} else {
					op.Animator.ResetCache();
				}
			}

			protected override void InternalUndo(RemoveKeyframe op)
			{
				if (op.Animator.Owner == null) {
					op.Owner.Animators.Add(op.Animator);
				}
				op.Animator.Keys.AddOrdered(op.Restore<Backup>().Keyframe);
				op.Animator.ResetCache();
			}
		}
	}

	public class SetKeyframe : Operation
	{
		public readonly IAnimable Animable;
		public readonly string PropertyName;
		public readonly string AnimationId;
		public readonly IKeyframe Keyframe;

		public override bool IsChangingDocument => true;

		public static void Perform(IAnimable animable, string propertyName, string animationId, IKeyframe keyframe)
		{
			Document.Current.History.Perform(new SetKeyframe(animable, propertyName, animationId, keyframe));
		}

		public static void Perform(IAnimator animator, IKeyframe keyframe)
		{
			Perform(animator.Owner, animator.TargetProperty, animator.AnimationId, keyframe);
		}

		private SetKeyframe(IAnimable animable, string propertyName, string animationId, IKeyframe keyframe)
		{
			Animable = animable;
			PropertyName = propertyName;
			Keyframe = keyframe;
			AnimationId = animationId;
		}

		public class Processor : OperationProcessor<SetKeyframe>
		{
			class Backup
			{
				public IKeyframe Keyframe;
				public bool AnimatorExists;
				public IAnimator Animator;
			}

			protected override void InternalRedo(SetKeyframe op)
			{
				bool animatorExists = op.Animable.Animators.Any(a => a.TargetProperty == op.PropertyName && a.AnimationId == op.AnimationId);
				var animator = op.Animable.Animators[op.PropertyName, op.AnimationId];
				op.Save(new Backup {
					AnimatorExists = animatorExists,
					Animator = animator,
					Keyframe = animator.Keys.FirstOrDefault(k => k.Frame == op.Keyframe.Frame)
				});
				animator.Keys.AddOrdered(op.Keyframe);
				animator.ResetCache();
			}

			protected override void InternalUndo(SetKeyframe op)
			{
				var b = op.Restore<Backup>();
				if (!b.Animator.Keys.Remove(op.Keyframe)) {
					throw new InvalidOperationException();
				}
				if (b.Keyframe != null) {
					b.Animator.Keys.AddOrdered(b.Keyframe);
				}
				if (!b.AnimatorExists || b.Animator.Keys.Count == 0) {
					op.Animable.Animators.Remove(b.Animator);
				}
				b.Animator.ResetCache();
			}
		}
	}

	public class SetAnimator : Operation
	{
		public readonly IAnimator Animator;
		public readonly IAnimable Animable;

		public override bool IsChangingDocument => true;

		public static void Perform(IAnimable animable, IAnimator animator)
		{
			Document.Current.History.Perform(new SetAnimator(animable, animator));
		}

		private SetAnimator(IAnimable animable, IAnimator animator)
		{
			Animable = animable;
			Animator = animator;
		}

		public class Processor : OperationProcessor<SetAnimator>
		{

			protected override void InternalRedo(SetAnimator op)
			{
				op.Animable.Animators.Add(op.Animator);
			}

			protected override void InternalUndo(SetAnimator op)
			{
				op.Animable.Animators.Remove(op.Animator);
			}
		}
	}

	public class InsertFolderItem : Operation
	{
		public readonly Node Container;
		public readonly FolderItemLocation Location;
		public readonly IFolderItem Item;

		public override bool IsChangingDocument => true;

		public static void Perform(IFolderItem item, bool aboveSelected = true)
		{
			Perform(Document.Current.Container, GetNewFolderItemLocation(aboveSelected), item);
		}

		public static void Perform(Node container, FolderItemLocation location, IFolderItem item)
		{
			if (item is Node && !NodeCompositionValidator.Validate(container.GetType(), item.GetType())) {
				throw new InvalidOperationException($"Can't put {item.GetType()} into {container.GetType()}");
			}
			Document.Current.History.Perform(new InsertFolderItem(container, location, item));
		}

		internal static FolderItemLocation GetNewFolderItemLocation(bool aboveSelected)
		{
			var doc = Document.Current;
			var rootFolder = doc.Container.RootFolder();
			if (aboveSelected) {
				var fi = doc.SelectedFolderItems().FirstOrDefault();
				return fi != null ? rootFolder.Find(fi) : new FolderItemLocation(rootFolder, 0);
			} else {
				var fi = doc.SelectedFolderItems().LastOrDefault();
				return fi != null ? rootFolder.Find(fi) + 1 : new FolderItemLocation(rootFolder, rootFolder.Items.Count);
			}
		}

		private InsertFolderItem(Node container, FolderItemLocation location, IFolderItem item)
		{
			Container = container;
			Location = location;
			Item = item;
		}

		public class Processor : OperationProcessor<InsertFolderItem>
		{
			protected override void InternalRedo(InsertFolderItem op)
			{
				op.Location.Folder.Items.Insert(op.Location.Index, op.Item);
				op.Container.SyncFolderDescriptorsAndNodes();
			}

			protected override void InternalUndo(InsertFolderItem op)
			{
				op.Location.Folder.Items.Remove(op.Item);
				op.Container.SyncFolderDescriptorsAndNodes();
			}
		}
	}

	public static class CreateNode
	{
		public static Node Perform(Type nodeType, bool aboveSelected = true)
		{
			return Perform(Document.Current.Container, InsertFolderItem.GetNewFolderItemLocation(aboveSelected), nodeType);
		}

		public static Node Perform(Type nodeType, FolderItemLocation location)
		{
			return Perform(Document.Current.Container, location, nodeType);
		}

		public static Node Perform(Node container, FolderItemLocation location, Type nodeType)
		{
			if (!nodeType.IsSubclassOf(typeof(Node))) {
				throw new InvalidOperationException();
			}
			var ctr = nodeType.GetConstructor(Type.EmptyTypes);
			var node = (Node)ctr.Invoke(new object[] { });
			var attrs = ClassAttributes<TangerineNodeBuilderAttribute>.Get(nodeType);
			if (attrs?.MethodName != null) {
				var builder = nodeType.GetMethod(attrs.MethodName, BindingFlags.NonPublic | BindingFlags.Instance);
				builder.Invoke(node, new object[] { });
			}
			node.Id = GenerateNodeId(container, nodeType);
			InsertFolderItem.Perform(container, location, node);
			ClearRowSelection.Perform();
			SelectNode.Perform(node);
			Document.Current.Decorate(node);
			return node;
		}

		static string GenerateNodeId(Node container, Type nodeType)
		{
			int c = 1;
			var id = nodeType.Name;
			while (container.Nodes.Any(i => i.Id == id)) {
				id = nodeType.Name + c;
				c++;
			}
			return id;
		}
	}

	public class UnlinkFolderItem : Operation
	{
		public readonly Node Container;
		public readonly IFolderItem Item;

		public override bool IsChangingDocument => true;

		public static void Perform(Node container, IFolderItem item)
		{
			Document.Current.History.Perform(new UnlinkFolderItem(container, item));
		}

		private UnlinkFolderItem(Node container, IFolderItem item)
		{
			Container = container;
			Item = item;
		}

		public class Processor : OperationProcessor<UnlinkFolderItem>
		{
			class Backup
			{
				public Node Container;
				public FolderItemLocation Location;
			}

			protected override void InternalRedo(UnlinkFolderItem op)
			{
				var loc = op.Container.RootFolder().Find(op.Item);
				op.Save(new Backup { Container = op.Container, Location = loc });
				loc.Folder.Items.Remove(op.Item);
				op.Container.SyncFolderDescriptorsAndNodes();
			}

			protected override void InternalUndo(UnlinkFolderItem op)
			{
				var b = op.Restore<Backup>();
				b.Location.Folder.Items.Insert(b.Location.Index, op.Item);
				b.Container.SyncFolderDescriptorsAndNodes();
			}
		}
	}

	public class SetMarker : Operation
	{
		public readonly MarkerList List;
		public readonly Marker Marker;

		public override bool IsChangingDocument => true;

		public static void Perform(MarkerList list, Marker marker)
		{
			Document.Current.History.Perform(new SetMarker(list, marker));
		}

		private SetMarker(MarkerList list, Marker marker)
		{
			List = list;
			Marker = marker;
		}

		public class Processor : OperationProcessor<SetMarker>
		{
			class Backup { public Marker Marker; }

			protected override void InternalRedo(SetMarker op)
			{
				op.Save(new Backup { Marker = op.List.FirstOrDefault(i => i.Frame == op.Marker.Frame) });
				op.List.AddOrdered(op.Marker);
			}

			protected override void InternalUndo(SetMarker op)
			{
				op.List.Remove(op.Marker);
				var b = op.Restore<Backup>();
				if (b.Marker != null) {
					op.List.AddOrdered(b.Marker);
				}
			}
		}
	}

	public class DeleteMarker : Operation
	{
		public readonly MarkerList List;
		public readonly Marker Marker;

		public override bool IsChangingDocument => true;

		public static void Perform(MarkerList list, Marker marker)
		{
			Document.Current.History.Perform(new DeleteMarker(list, marker));
		}

		private DeleteMarker(MarkerList list, Marker marker)
		{
			List = list;
			Marker = marker;
		}

		public class Processor : OperationProcessor<DeleteMarker>
		{
			protected override void InternalRedo(DeleteMarker op)
			{
				op.List.Remove(op.Marker);
			}

			protected override void InternalUndo(DeleteMarker op)
			{
				op.List.AddOrdered(op.Marker);
			}
		}
	}

	public class MoveNodes : Operation
	{
		public FolderItemLocation Location { get; }
		public FolderItemLocation PrevLocation { get; }
		public Node Container { get; }
		public IFolderItem Item { get; }
		public override bool IsChangingDocument => true;
		private static FolderItemLocation GetParentFolder(IFolderItem item) => Document.Current.Container.RootFolder().Find(item);

		private MoveNodes(Node container, FolderItemLocation location, FolderItemLocation prevLocation, IFolderItem item)
		{
			Container = container;
			Location = location;
			Item = item;
			PrevLocation = prevLocation;
		}

		public static void Perform(IEnumerable<IFolderItem> items, FolderItemLocation targetFolder)
		{
			foreach (var item in items) {
				Perform(item, targetFolder);
			}
		}

		public static void Perform(IFolderItem item, FolderItemLocation targetFolder)
		{
			Document.Current.History.Perform(
				new MoveNodes(
					Document.Current.Container,
					targetFolder,
					GetParentFolder(item),
					item));
		}

		public static void Perform(RowLocation newLocation)
		{
			var i = newLocation.Index;
			var nodes = Document.Current.SelectedNodes().ToList();
			var targetFolder = newLocation.ParentRow.Components.Get<FolderRow>()?.Folder;
			if (targetFolder == null) {
				throw new Lime.Exception("Cant put nodes in a non folder row");
			}
			foreach (var node in nodes) {
				Document.Current.History.Perform(
					new MoveNodes(
						Document.Current.Container,
						new FolderItemLocation(targetFolder, i++),
						GetParentFolder(node),
						node));
			}
		}

		public class Processor : OperationProcessor<MoveNodes>
		{
			protected override void InternalRedo(MoveNodes op)
			{

				if (op.PrevLocation.Folder == op.Location.Folder) {
					var oldIndex = op.PrevLocation.Folder.Items.IndexOf(op.Item);
					var index = op.Location.Index;
					op.PrevLocation.Folder.Items.Remove(op.Item);
					if (op.Location.Index > oldIndex) index--;
					var idx = op.PrevLocation.Folder.Items.IndexOf(op.Item);
					op.Location.Folder.Items.Insert(index, op.Item);
				} else {
					op.PrevLocation.Folder.Items.Remove(op.Item);
					op.Location.Folder.Items.Insert(op.Location.Index, op.Item);
				}
				op.Container.SyncFolderDescriptorsAndNodes();
			}

			protected override void InternalUndo(MoveNodes op)
			{
				op.Location.Folder.Items.Remove(op.Item);
				op.PrevLocation.Folder.Items.Insert(op.PrevLocation.Index, op.Item);
				op.Container.SyncFolderDescriptorsAndNodes();
			}
		}
	}

	public static class SortBonesInChain
	{
		public static void Perform(Bone boneInChain)
		{
			var bones = Document.Current.Container.Nodes.OfType<Bone>();
			var rootParent = bones.GetBone(boneInChain.BaseIndex);
			while (rootParent != null && rootParent.BaseIndex != 0) {
				rootParent = bones.GetBone(rootParent.BaseIndex);
			}
			var tree = BoneUtils.SortBones(BoneUtils.FindBoneDescendats(rootParent ?? boneInChain, bones));
			var loc = Document.Current.Container.RootFolder().Find(rootParent ?? boneInChain);
			foreach (var child in tree) {
				MoveNodes.Perform(child, new FolderItemLocation(loc.Folder, ++loc.Index));
			}
		}
	}
}
