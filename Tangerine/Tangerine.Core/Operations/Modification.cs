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
		private bool isChangingDocument;

		public readonly object Obj;
		public readonly object Value;
		public readonly PropertyInfo Property;

		public override bool IsChangingDocument => isChangingDocument;

		public static void Perform(object obj, string propertyName, object value, bool isChangingDocument = true)
		{
			Document.Current.History.Perform(new SetProperty(obj, propertyName, value, isChangingDocument));
		}

		protected SetProperty(object obj, string propertyName, object value, bool isChangingDocument)
		{
			Obj = obj;
			Value = value;
			Property = obj.GetType().GetProperty(propertyName);
			this.isChangingDocument = isChangingDocument;
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
		public static void Perform(object @object, string propertyName, object value, bool createAnimatorIfNeeded = false, bool createInitialKeyframeForNewAnimator = true, int atFrame = -1)
		{
			SetProperty.Perform(@object, propertyName, value);
			IAnimator animator;
			var animable = @object as IAnimable;
			if (animable != null && (animable.Animators.TryFind(propertyName, out animator, Document.Current.AnimationId) || createAnimatorIfNeeded)) {

				if (animator == null && createInitialKeyframeForNewAnimator) {
					var propertyValue = animable.GetType().GetProperty(propertyName).GetValue(animable);
					Perform(animable, propertyName, propertyValue, true, false, 0);
				}

				int savedFrame = -1;
				if (atFrame >= 0 && Document.Current.AnimationFrame != atFrame) {
					savedFrame = Document.Current.AnimationFrame;
					Document.Current.AnimationFrame = atFrame;
				}

				try {
					var type = animable.GetType().GetProperty(propertyName).PropertyType;
					var key =
						animator?.ReadonlyKeys.FirstOrDefault(i => i.Frame == Document.Current.AnimationFrame)?.Clone() ??
						Keyframe.CreateForType(type);
					key.Frame = Document.Current.AnimationFrame;
					key.Function = animator?.Keys.LastOrDefault(k => k.Frame <= key.Frame)?.Function ?? KeyFunction.Linear;
					key.Value = value;
					SetKeyframe.Perform(animable, propertyName, Document.Current.AnimationId, key);
				} finally {
					if (savedFrame >= 0) {
						Document.Current.AnimationFrame = savedFrame;
					}
				}
			}
		}
	}

	public static class ProcessAnimableProperty
	{

		public delegate bool AnimablePropertyProcessor<T>(T value, out T newValue);

		public static void Perform<T>(object @object, string propertyName, AnimablePropertyProcessor<T> propertyProcessor)
		{
			var propertyInfo = @object.GetType().GetProperty(propertyName);
			if (propertyInfo != null) {
				var value = propertyInfo.GetValue(@object);
				if (value is T) {
					T processedValue;
					if (propertyProcessor((T) value, out processedValue)) {
						SetProperty.Perform(@object, propertyName, processedValue);
					}
				}
			}

			IAnimator animator;
			var animable = @object as IAnimable;
			if (animable != null && animable.Animators.TryFind(propertyName, out animator, Document.Current.AnimationId)) {
				foreach (var keyframe in animator.ReadonlyKeys.ToList()) {
					if (!(keyframe.Value is T)) continue;

					T processedValue;
					if (propertyProcessor((T) keyframe.Value, out processedValue)) {
						var keyframeClone = keyframe.Clone();
						keyframeClone.Value = processedValue;
						SetKeyframe.Perform(animator, keyframeClone);
					}
				}
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
				Backup backup;
				IAnimator animator;

				if (!op.Find(out backup)) {
					bool animatorExists =
						op.Animable.Animators.Any(a => a.TargetProperty == op.PropertyName && a.AnimationId == op.AnimationId);
					animator = op.Animable.Animators[op.PropertyName, op.AnimationId];
					op.Save(new Backup {
						AnimatorExists = animatorExists,
						Animator = animator,
						Keyframe = animator.Keys.FirstOrDefault(k => k.Frame == op.Keyframe.Frame)
					});
				} else {
					animator = backup.Animator;
					if (!backup.AnimatorExists) {
						op.Animable.Animators.Add(animator);
					}
				}

				animator.Keys.AddOrdered(op.Keyframe);
				animator.ResetCache();
			}

			protected override void InternalUndo(SetKeyframe op)
			{
				var b = op.Peek<Backup>();
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
		private readonly Node container;
		private readonly Marker marker;
		private readonly bool removeDependencies;

		public override bool IsChangingDocument => true;

		private SetMarker(Node container, Marker marker, bool removeDependencies)
		{
			this.container = container;
			this.marker = marker;
			this.removeDependencies = removeDependencies;
		}

		public static void Perform(Node container, Marker marker, bool removeDependencies)
		{
			var previousMarker = container.Markers.FirstOrDefault(i => i.Frame == marker.Frame);

			Document.Current.History.Perform(new SetMarker(container, marker, removeDependencies));

			if (removeDependencies) {
				// Detect if a previous marker id is unique then rename it in triggers and markers.
				if (previousMarker != null && previousMarker.Id != marker.Id &&
					container.Markers.All(markerEl => markerEl.Id != previousMarker.Id)) {

					foreach (var markerEl in container.Markers.ToList()) {
						if (markerEl.Action == MarkerAction.Jump && markerEl.JumpTo == previousMarker.Id) {
							SetProperty.Perform(markerEl, nameof(markerEl.JumpTo), marker.Id);
						}
					}

					ProcessAnimableProperty.Perform(container, nameof(Node.Trigger),
						(string value, out string newValue) => {
							return TriggersValidation.TryRenameMarkerInTrigger(
								previousMarker.Id, marker.Id, value, out newValue
							);
						}
					);
				}
			}
		}

		public class Processor : OperationProcessor<SetMarker>
		{
			private class Backup
			{
				internal Marker Marker;
				internal string SavedJumpTo;
			}

			protected override void InternalRedo(SetMarker op)
			{
				var backup = new Backup {
					Marker = op.container.Markers.FirstOrDefault(i => i.Frame == op.marker.Frame)
				};

				op.Save(backup);
				op.container.Markers.AddOrdered(op.marker);

				if (op.removeDependencies) {
					backup.SavedJumpTo = op.marker.JumpTo;
					if (op.marker.Action == MarkerAction.Jump &&
						op.container.Markers.All(markerEl => markerEl.Id != op.marker.JumpTo)) {
						op.marker.JumpTo = "";
					}
				}
			}

			protected override void InternalUndo(SetMarker op)
			{
				op.container.Markers.Remove(op.marker);
				var b = op.Restore<Backup>();
				if (b.Marker != null) {
					op.container.Markers.AddOrdered(b.Marker);
				}

				if (op.removeDependencies) {
					op.marker.JumpTo = b.SavedJumpTo;
				}
			}
		}

	}

	public class DeleteMarker : Operation
	{
		private readonly Node container;
		private readonly Marker marker;
		private readonly bool removeDependencies;

		public override bool IsChangingDocument => true;

		public static void Perform(Node container, Marker marker, bool removeDependencies)
		{
			Document.Current.History.Perform(new DeleteMarker(container, marker, removeDependencies));

			if (removeDependencies) {
				ProcessAnimableProperty.Perform(container, nameof(Node.Trigger),
					(string value, out string newValue) => {
						return TriggersValidation.TryRemoveMarkerFromTrigger(marker.Id, value, out newValue);
					}
				);
			}
		}

		private DeleteMarker(Node container, Marker marker, bool removeDependencies)
		{
			this.container = container;
			this.marker = marker;
			this.removeDependencies = removeDependencies;
		}

		public class Processor : OperationProcessor<DeleteMarker>
		{
			private class Backup
			{
				internal readonly List<Marker> RemovedJumpToMarkers;

				public Backup(List<Marker> removedJumpToMarkers)
				{
					RemovedJumpToMarkers = removedJumpToMarkers;
				}
			}

			protected override void InternalRedo(DeleteMarker op)
			{
				op.container.Markers.Remove(op.marker);

				if (op.removeDependencies) {
					var removedJumpToMarkers = new List<Marker>();
					for (int i = op.container.Markers.Count - 1; i >= 0; i--) {
						var marker = op.container.Markers[i];
						if (marker.Action != MarkerAction.Jump || marker.JumpTo != op.marker.Id) {
							continue;
						}
						removedJumpToMarkers.Insert(0, marker);
						op.container.Markers.RemoveAt(i);
					}

					op.Save(new Backup(removedJumpToMarkers));
				}
			}

			protected override void InternalUndo(DeleteMarker op)
			{
				op.container.Markers.AddOrdered(op.marker);

				Backup backup;
				if (op.Find(out backup)) {
					backup = op.Restore<Backup>();
					foreach (var marker in backup.RemovedJumpToMarkers) {
						op.container.Markers.AddOrdered(marker);
					}
				}
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

	public class SetComponent : Operation
	{
		private readonly Node node;
		private readonly NodeComponent component;

		public override bool IsChangingDocument => true;

		private SetComponent(Node node, NodeComponent component)
		{
			this.node = node;
			this.component = component;
		}

		public static void Perform(Node node, NodeComponent component) => Document.Current.History.Perform(new SetComponent(node, component));

		public class Processor : OperationProcessor<SetComponent>
		{
			protected override void InternalRedo(SetComponent op) => op.node.Components.Add(op.component);
			protected override void InternalUndo(SetComponent op) => op.node.Components.Remove(op.component);
		}

	}

	public class DeleteComponent : Operation
	{
		private readonly Node node;
		private readonly NodeComponent component;

		public override bool IsChangingDocument => true;

		private DeleteComponent(Node node, NodeComponent component)
		{
			this.node = node;
			this.component = component;
		}

		public static void Perform(Node node, NodeComponent component) => Document.Current.History.Perform(new DeleteComponent(node, component));

		public class Processor : OperationProcessor<DeleteComponent>
		{
			protected override void InternalRedo(DeleteComponent op) => op.node.Components.Remove(op.component);
			protected override void InternalUndo(DeleteComponent op) => op.node.Components.Add(op.component);
		}
	}
}
