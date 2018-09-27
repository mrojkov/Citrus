using System;
using System.Collections;
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
		public readonly Type Type;
		public override bool IsChangingDocument { get; }

		public static void Perform(object obj, string propertyName, object value, bool isChangingDocument = true)
		{
			DocumentHistory.Current.Perform(new SetProperty(obj, propertyName, value, isChangingDocument));
		}

		public static void Perform(Type type, object obj, string propertyName, object value, bool isChangingDocument = true)
		{
			DocumentHistory.Current.Perform(new SetProperty(type, obj, propertyName, value, isChangingDocument));
		}

		protected SetProperty(object obj, string propertyName, object value, bool isChangingDocument)
		{
			Type = obj.GetType();
			Obj = obj;
			Value = value;
			Property = Type.GetProperty(propertyName);
			IsChangingDocument = isChangingDocument;
		}

		protected SetProperty(Type type, object obj, string propertyName, object value, bool isChangingDocument)
		{
			Type = type;
			Obj = obj;
			Value = value;
			Property = Type.GetProperty(propertyName);
			IsChangingDocument = isChangingDocument;
		}

		public class Processor : OperationProcessor<SetProperty>
		{
			private class Backup { public object Value; }

			protected override void InternalRedo(SetProperty op)
			{
				op.Save(new Backup { Value = op.Property.GetValue(op.Obj, null) });
				op.Property.SetValue(op.Obj, op.Value, null);
				PropertyAttributes<TangerineOnPropertySetAttribute>.Get(op.Obj.GetType(), op.Property.Name)?.Invoke(op.Obj);
			}

			protected override void InternalUndo(SetProperty op)
			{
				var v = op.Restore<Backup>().Value;
				op.Property.SetValue(op.Obj, v, null);
				PropertyAttributes<TangerineOnPropertySetAttribute>.Get(op.Obj.GetType(), op.Property.Name)?.Invoke(op.Obj);
			}
		}
	}

	public class SetIndexedProperty : Operation
	{
		public readonly object Obj;
		public readonly object Value;
		public readonly int Index;
		public readonly PropertyInfo Property;
		public readonly Type Type;
		public override bool IsChangingDocument { get; }

		public static void Perform(object obj, string propertyName, int index, object value, bool isChangingDocument = true)
		{
			DocumentHistory.Current.Perform(new SetIndexedProperty(obj, propertyName, index, value, isChangingDocument));
		}

		public static void Perform(Type type, object obj, string propertyName, int indexProvider, object value, bool isChangingDocument = true)
		{
			DocumentHistory.Current.Perform(new SetIndexedProperty(type, obj, propertyName, indexProvider, value, isChangingDocument));
		}

		protected SetIndexedProperty(object obj, string propertyName, int index, object value, bool isChangingDocument)
		{
			Type = obj.GetType();
			Obj = obj;
			Index = index;
			Value = value;
			Property = Type.GetProperty(propertyName);
			IsChangingDocument = isChangingDocument;
		}

		protected SetIndexedProperty(Type type, object obj, string propertyName, int index, object value, bool isChangingDocument)
		{
			Type = type;
			Obj = obj;
			Index = index;
			Value = value;
			Property = Type.GetProperty(propertyName);
			IsChangingDocument = isChangingDocument;
		}

		public class Processor : OperationProcessor<SetIndexedProperty>
		{
			private class Backup { public object Value; }

			protected override void InternalRedo(SetIndexedProperty op)
			{
				op.Save(new Backup { Value = op.Property.GetGetMethod().Invoke(op.Obj, new object[] { op.Index }) });
				op.Property.GetSetMethod().Invoke(op.Obj, new [] { op.Index, op.Value });
			}

			protected override void InternalUndo(SetIndexedProperty op)
			{
				var v = op.Restore<Backup>().Value;
				op.Property.GetSetMethod().Invoke(op.Obj, new[] { op.Index, v });
			}
		}
	}

	public class SetAnimableProperty
	{

		public static void Perform(object @object, string propertyPath, object value, bool createAnimatorIfNeeded = false, bool createInitialKeyframeForNewAnimator = true, int atFrame = -1)
		{
			IAnimator animator;
			var animationHost = @object as IAnimationHost;
			object owner = @object;
			int index = -1;
			AnimationUtils.PropertyData propertyData = AnimationUtils.PropertyData.Empty;
			if (animationHost != null) {
				(propertyData, owner, index) = AnimationUtils.GetPropertyByPath(animationHost, propertyPath);
			}
			if (index == -1) {
				SetProperty.Perform(owner, propertyData.Info?.Name ?? propertyPath, value);
			} else {
				SetIndexedProperty.Perform(owner, propertyData.Info?.Name ?? propertyPath, index, value);
			}
			if (animationHost != null && (animationHost.Animators.TryFind(propertyPath, out animator, Document.Current.AnimationId) || createAnimatorIfNeeded)) {
				if (animator == null && createInitialKeyframeForNewAnimator) {
					var propertyValue = propertyData.Info.GetValue(owner);
					Perform(animationHost, propertyPath, propertyValue, true, false, 0);
				}

				int savedFrame = -1;
				if (atFrame >= 0 && Document.Current.AnimationFrame != atFrame) {
					savedFrame = Document.Current.AnimationFrame;
					Document.Current.AnimationFrame = atFrame;
				}

				try {
					var type = propertyData.Info.PropertyType;
					var key =
						animator?.ReadonlyKeys.GetByFrame(Document.Current.AnimationFrame)?.Clone() ??
						Keyframe.CreateForType(type);
					key.Frame = Document.Current.AnimationFrame;
					key.Function = animator?.Keys.LastOrDefault(k => k.Frame <= key.Frame)?.Function ?? KeyFunction.Linear;
					key.Value = value;
					SetKeyframe.Perform(animationHost, propertyPath, Document.Current.AnimationId, key);
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

		public static void Perform<T>(object @object, string propertyPath, AnimablePropertyProcessor<T> propertyProcessor)
		{
			var propertyInfo = @object.GetType().GetProperty(propertyPath);
			if (propertyInfo != null) {
				var value = propertyInfo.GetValue(@object);
				if (value is T) {
					T processedValue;
					if (propertyProcessor((T) value, out processedValue)) {
						SetProperty.Perform(@object, propertyPath, processedValue);
					}
				}
			}

			IAnimator animator;
			var animable = @object as IAnimationHost;
			if (animable != null && animable.Animators.TryFind(propertyPath, out animator, Document.Current.AnimationId)) {
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
		public readonly IAnimationHost AnimationHost;

		public override bool IsChangingDocument => true;

		public static void Perform(IAnimator animator, int frame)
		{
			DocumentHistory.Current.Perform(new RemoveKeyframe(animator, frame));
		}

		private RemoveKeyframe(IAnimator animator, int frame)
		{
			Frame = frame;
			Animator = animator;
			AnimationHost = Animator.Owner;
		}

		public class Processor : OperationProcessor<RemoveKeyframe>
		{
			class Backup { public IKeyframe Keyframe; }

			protected override void InternalRedo(RemoveKeyframe op)
			{
				var kf = op.Animator.Keys.GetByFrame(op.Frame);
				op.Save(new Backup { Keyframe = kf });
				op.Animator.Keys.Remove(kf);
				if (op.Animator.Keys.Count == 0) {
					op.AnimationHost.Animators.Remove(op.Animator);
				} else {
					op.Animator.ResetCache();
				}
				if (op.Animator.TargetPropertyPath == nameof(Node.Trigger)) {
					Document.ForceAnimationUpdate();
				}
			}

			protected override void InternalUndo(RemoveKeyframe op)
			{
				if (op.Animator.Owner == null) {
					op.AnimationHost.Animators.Add(op.Animator);
				}
				op.Animator.Keys.AddOrdered(op.Restore<Backup>().Keyframe);
				op.Animator.ResetCache();
				if (op.Animator.TargetPropertyPath == nameof(Node.Trigger)) {
					Document.ForceAnimationUpdate();
				}
			}
		}
	}

	public class SetKeyframe : Operation
	{
		public readonly IAnimationHost AnimationHost;
		public readonly string PropertyPath;
		public readonly string AnimationId;
		public readonly IKeyframe Keyframe;

		public override bool IsChangingDocument => true;

		public static void Perform(IAnimationHost animationHost, string propertyName, string animationId, IKeyframe keyframe)
		{
			DocumentHistory.Current.Perform(new SetKeyframe(animationHost, propertyName, animationId, keyframe));
		}

		public static void Perform(IAnimator animator, IKeyframe keyframe)
		{
			Perform(animator.Owner, animator.TargetPropertyPath, animator.AnimationId, keyframe);
		}

		private SetKeyframe(IAnimationHost animationHost, string propertyPath, string animationId, IKeyframe keyframe)
		{
			AnimationHost = animationHost;
			PropertyPath = propertyPath;
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
						op.AnimationHost.Animators.Any(a => a.TargetPropertyPath == op.PropertyPath && a.AnimationId == op.AnimationId);
					animator = op.AnimationHost.Animators[op.PropertyPath, op.AnimationId];
					op.Save(new Backup {
						AnimatorExists = animatorExists,
						Animator = animator,
						Keyframe = animator.Keys.GetByFrame(op.Keyframe.Frame)
					});
				} else {
					animator = backup.Animator;
					if (!backup.AnimatorExists) {
						op.AnimationHost.Animators.Add(animator);
					}
				}

				animator.Keys.AddOrdered(op.Keyframe);
				animator.ResetCache();
				if (animator.TargetPropertyPath == nameof(Node.Trigger)) {
					Document.ForceAnimationUpdate();
				}
			}

			protected override void InternalUndo(SetKeyframe op)
			{
				var b = op.Peek<Backup>();
				var key = b.Animator.Keys.GetByFrame(op.Keyframe.Frame);
				if (key == null) {
					throw new InvalidOperationException();
				}
				b.Animator.Keys.Remove(key);
				if (b.Keyframe != null) {
					b.Animator.Keys.AddOrdered(b.Keyframe);
				}
				if (!b.AnimatorExists || b.Animator.Keys.Count == 0) {
					op.AnimationHost.Animators.Remove(b.Animator);
				}
				b.Animator.ResetCache();
				if (b.Animator.TargetPropertyPath == nameof(Node.Trigger)) {
					Document.ForceAnimationUpdate();
				}
			}
		}
	}

	public class InsertIntoList : Operation
	{
		private readonly IList collection;
		private readonly int index;
		private readonly object element;

		public override bool IsChangingDocument => true;

		protected InsertIntoList(IList collection, int index, object element)
		{
			this.collection = collection;
			this.index = index;
			this.element = element;
		}

		public static void Perform(IList collection, int index, object element) => DocumentHistory.Current.Perform(new InsertIntoList(collection, index, element));

		public class Processor : OperationProcessor<InsertIntoList>
		{
			protected override void InternalRedo(InsertIntoList op) => op.collection.Insert(op.index, op.element);
			protected override void InternalUndo(InsertIntoList op) => op.collection.RemoveAt(op.index);
		}
	}

	public class RemoveFromList : Operation
	{
		private readonly IList collection;
		private readonly int index;
		private object backup;

		public override bool IsChangingDocument => true;

		protected RemoveFromList(IList collection, int index)
		{
			this.collection = collection;
			this.index = index;
		}

		public static void Perform(IList collection, int index) => DocumentHistory.Current.Perform(new RemoveFromList(collection, index));

		public class Processor : OperationProcessor<RemoveFromList>
		{
			protected override void InternalRedo(RemoveFromList op)
			{
				op.backup = op.collection[op.index];
				op.collection.RemoveAt(op.index);
			}

			protected override void InternalUndo(RemoveFromList op) => op.collection.Insert(op.index, op.backup);
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
			DocumentHistory.Current.Perform(new InsertFolderItem(container, location, item));
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
			DocumentHistory.Current.Perform(new UnlinkFolderItem(container, item));
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
		private readonly Marker marker;
		private readonly bool removeDependencies;

		public override bool IsChangingDocument => true;

		private SetMarker(Marker marker, bool removeDependencies)
		{
			this.marker = marker;
			this.removeDependencies = removeDependencies;
		}

		public static void Perform(Marker marker, bool removeDependencies)
		{
			var previousMarker = Document.Current.Animation.Markers.GetByFrame(marker.Frame);

			DocumentHistory.Current.Perform(new SetMarker(marker, removeDependencies));

			if (removeDependencies) {
				// Detect if a previous marker id is unique then rename it in triggers and markers.
				if (previousMarker != null && previousMarker.Id != marker.Id &&
					Document.Current.Animation.Markers.All(markerEl => markerEl.Id != previousMarker.Id)) {

					foreach (var markerEl in Document.Current.Animation.Markers.ToList()) {
						if (markerEl.Action == MarkerAction.Jump && markerEl.JumpTo == previousMarker.Id) {
							SetProperty.Perform(markerEl, nameof(markerEl.JumpTo), marker.Id);
						}
					}

					ProcessAnimableProperty.Perform(Document.Current.Container, nameof(Node.Trigger),
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
					Marker = Document.Current.Animation.Markers.GetByFrame(op.marker.Frame)
				};

				op.Save(backup);
				Document.Current.Animation.Markers.AddOrdered(op.marker);

				if (op.removeDependencies) {
					backup.SavedJumpTo = op.marker.JumpTo;
					if (op.marker.Action == MarkerAction.Jump &&
						Document.Current.Animation.Markers.All(markerEl => markerEl.Id != op.marker.JumpTo)) {
						op.marker.JumpTo = "";
					}
				}
			}

			protected override void InternalUndo(SetMarker op)
			{
				Document.Current.Animation.Markers.Remove(op.marker);
				var b = op.Restore<Backup>();
				if (b.Marker != null) {
					Document.Current.Animation.Markers.AddOrdered(b.Marker);
				}

				if (op.removeDependencies) {
					op.marker.JumpTo = b.SavedJumpTo;
				}
			}
		}

	}

	public class DeleteMarker : Operation
	{
		private readonly Marker marker;
		private readonly bool removeDependencies;

		public override bool IsChangingDocument => true;

		public static void Perform(Marker marker, bool removeDependencies)
		{
			DocumentHistory.Current.Perform(new DeleteMarker(marker, removeDependencies));

			if (removeDependencies) {
				ProcessAnimableProperty.Perform(Document.Current.Container, nameof(Node.Trigger),
					(string value, out string newValue) => {
						return TriggersValidation.TryRemoveMarkerFromTrigger(marker.Id, value, out newValue);
					}
				);
			}
		}

		private DeleteMarker(Marker marker, bool removeDependencies)
		{
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
				Document.Current.Animation.Markers.Remove(op.marker);

				if (op.removeDependencies) {
					var removedJumpToMarkers = new List<Marker>();
					for (int i = Document.Current.Animation.Markers.Count - 1; i >= 0; i--) {
						var marker = Document.Current.Animation.Markers[i];
						if (marker.Action != MarkerAction.Jump || marker.JumpTo != op.marker.Id) {
							continue;
						}
						removedJumpToMarkers.Insert(0, marker);
						Document.Current.Animation.Markers.RemoveAt(i);
					}

					op.Save(new Backup(removedJumpToMarkers));
				}
			}

			protected override void InternalUndo(DeleteMarker op)
			{
				Document.Current.Animation.Markers.AddOrdered(op.marker);

				Backup backup;
				if (op.Find(out backup)) {
					backup = op.Restore<Backup>();
					foreach (var marker in backup.RemovedJumpToMarkers) {
						Document.Current.Animation.Markers.AddOrdered(marker);
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
			DocumentHistory.Current.Perform(
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
				DocumentHistory.Current.Perform(
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

		public static void Perform(Node node, NodeComponent component) => DocumentHistory.Current.Perform(new SetComponent(node, component));

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

		public static void Perform(Node node, NodeComponent component) => DocumentHistory.Current.Perform(new DeleteComponent(node, component));

		public class Processor : OperationProcessor<DeleteComponent>
		{
			protected override void InternalRedo(DeleteComponent op) => op.node.Components.Remove(op.component);
			protected override void InternalUndo(DeleteComponent op) => op.node.Components.Add(op.component);
		}
	}

	public static class UntieWidgetsFromBones
	{
		public static void Perform(IEnumerable<Bone> bones, IEnumerable<Widget> widgets)
		{
			var sortedBones = BoneUtils.SortBones(bones);
			if (!widgets.Any() || !sortedBones.Any()) {
				return;
			}
			if (!CheckConsistency(bones, widgets)) throw new InvalidOperationException("Not all bones and widgets have the same parent");
			foreach (var widget in widgets) {
				if (widget is DistortionMesh) {
					foreach (PointObject point in widget.Nodes) {
						UntieBonesFromNode(point, nameof(PointObject.SkinningWeights), sortedBones);
					}
				} else {
					UntieBonesFromNode(widget, nameof(Widget.SkinningWeights), sortedBones);
				}
			}
		}

		private static bool CheckConsistency(IEnumerable<Bone> bones, IEnumerable<Widget> widgets)
		{
			var container = bones.First().Parent.AsWidget;
			foreach (var bone in bones) {
				if (bone.Parent == null || bone.Parent != container) return false;
			}

			foreach (var widget in widgets) {
				if (widget.Parent == null || widget.Parent != container) return false;
			}
			return true;
		}

		private static void UntieBonesFromNode(Node node, string skinningPropertyName, IEnumerable<Bone> bones)
		{
			var property = node.GetType().GetProperty(skinningPropertyName);
			var originSkinningWeights = (SkinningWeights)property.GetValue(node);
			var boneIndices = new List<int>();
			for (int i = 0; i < 4; i++) {
				if (bones.Any(b => b.Index == originSkinningWeights[i].Index)) {
					boneIndices.Add(i);
				}
			}
			if (boneIndices.Count != 0) {
				var skinningWeights = ResetSkinningWeights(boneIndices, originSkinningWeights);
				BakeSkinningTransform(skinningWeights, node);
				SetProperty.Perform(node, skinningPropertyName, skinningWeights);
			}
		}

		private static void BakeSkinningTransform(SkinningWeights newSkinningWeights, Node node)
		{
			if (node is PointObject) {
				var point = (PointObject) node;
				var originTranslation = point.TransformedPosition;
				var boneArray = node.Parent.Parent.AsWidget.BoneArray;
				var localToParentTransform = node.Parent.AsWidget.CalcLocalToParentTransform();
				var transformedPosition = originTranslation * localToParentTransform  *
					boneArray.CalcWeightedRelativeTransform(newSkinningWeights).CalcInversed() * localToParentTransform.CalcInversed();
				var translation = (transformedPosition - point.Offset) / point.Parent.AsWidget.Size;
				SetAnimableProperty.Perform(node, nameof(PointObject.Position), translation);
			} else {
				var widget = node.AsWidget;
				var originLocalToParent = node.AsWidget.CalcLocalToParentTransform();
				var transform = (originLocalToParent *
					widget.Parent.AsWidget.BoneArray.CalcWeightedRelativeTransform(newSkinningWeights).CalcInversed()).ToTransform2();
				SetAnimableProperty.Perform(node, nameof(Widget.Rotation), transform.Rotation);
				var localToParentTransform =
					Matrix32.Translation(-(widget.Pivot * widget.Size)) *
					Matrix32.Transformation(
						Vector2.Zero,
						widget.Scale,
						widget.Rotation * Mathf.Pi / 180f,
						Vector2.Zero);
				SetAnimableProperty.Perform(node, nameof(Widget.Position), transform.Translation - localToParentTransform.T);
			}
		}

		private static SkinningWeights ResetSkinningWeights(List<int> bonesIndices, SkinningWeights originSkinningWeights)
		{
			var skinningWeights = new SkinningWeights();
			var overallWeight = 0f;
			var newOverallWeight = 0f;
			for (var i = 0; i < 4; i++) {
				overallWeight += originSkinningWeights[i].Weight;
				if (bonesIndices.Contains(i)) {
					skinningWeights[i] = new BoneWeight();
				} else {
					skinningWeights[i] = originSkinningWeights[i];
					newOverallWeight += skinningWeights[i].Weight;
				}
			}
			if (Mathf.Abs(overallWeight) > Mathf.ZeroTolerance && Mathf.Abs(newOverallWeight) > Mathf.ZeroTolerance) {
				var factor = overallWeight / newOverallWeight;
				for (var i = 0; i < 4; i++) {
					var boneWeight = skinningWeights[i];
					boneWeight.Weight *= factor;
					skinningWeights[i] = boneWeight;
				}
			}
			return skinningWeights;
		}
	}

	public class TieWidgetsWithBonesException : Lime.Exception
	{
		public Node Node { get; set; }

		public TieWidgetsWithBonesException(Node node)
		{
			Node = node;
		}
	}

	public static class TieWidgetsWithBones
	{
		public static void Perform(IEnumerable<Bone> bones, IEnumerable<Widget> widgets)
		{
			var boneList = bones.ToList();
			if (!widgets.Any() || !bones.Any()) {
				return;
			}
			if (!CheckConsistency(bones, widgets)) throw new InvalidOperationException("Not all bones and widgets have the same parent");
			foreach (var widget in widgets) {
				if (widget is DistortionMesh) {
					var mesh = widget as DistortionMesh;
					foreach (PointObject point in mesh.Nodes) {
						if (!CanApplyBone(point.SkinningWeights)) {
							throw new TieWidgetsWithBonesException(point);
						}
						SetProperty.Perform(point, nameof(PointObject.SkinningWeights),
							CalcSkinningWeight(point.SkinningWeights, point.CalcPositionInSpaceOf(widget.ParentWidget), boneList));
					}
				} else {
					if (!CanApplyBone(widget.SkinningWeights)) {
						throw new TieWidgetsWithBonesException(widget);
					}
					SetProperty.Perform(widget, nameof(Widget.SkinningWeights),
						CalcSkinningWeight(widget.SkinningWeights, widget.Position, boneList));
				}
			}
			foreach (var bone in bones) {
				var entry = bone.Parent.AsWidget.BoneArray[bone.Index];
				SetAnimableProperty.Perform(bone, nameof(Bone.RefPosition), entry.Joint, CoreUserPreferences.Instance.AutoKeyframes);
				SetAnimableProperty.Perform(bone, nameof(Bone.RefLength), entry.Length, CoreUserPreferences.Instance.AutoKeyframes);
				SetAnimableProperty.Perform(bone, nameof(Bone.RefRotation), entry.Rotation, CoreUserPreferences.Instance.AutoKeyframes);
			}
		}

		private static bool CanApplyBone(SkinningWeights skinningWeights)
		{
			for (var i = 0; i < 4; i++) {
				if (skinningWeights[i].Index == 0) {
					return true;
				}
			}
			return false;
		}

		private static bool CheckConsistency(IEnumerable<Bone> bones, IEnumerable<Widget> widgets)
		{
			var container = bones.First().Parent.AsWidget;
			foreach (var bone in bones) {
				if (bone.Parent == null || bone.Parent != container) return false;
			}

			foreach (var widget in widgets) {
				if (widget.Parent == null || widget.Parent != container) return false;
			}
			return true;
		}

		private static SkinningWeights CalcSkinningWeight(SkinningWeights oldSkinningWeights, Vector2 position, List<Bone> bones)
		{
			var skinningWeights = new SkinningWeights();
			var i = 0;
			var overallWeight = 0f;
			while (oldSkinningWeights[i].Index != 0) {
				skinningWeights[i] = oldSkinningWeights[i];
				overallWeight += skinningWeights[i].Weight;
				i++;
			}
			var j = 0;
			while (j < bones.Count && i < 4) {
				var weight = bones[j].CalcWeightForPoint(position);
				if (Mathf.Abs(weight) > Mathf.ZeroTolerance) {
					skinningWeights[i] = new BoneWeight {
						Weight = weight,
						Index = bones[j].Index
					};
					overallWeight += skinningWeights[i].Weight;
					i++;
				}
				j++;
			}
			if (overallWeight != 0) {
				for(i = 0; i < 4; i++) {
					var bw = skinningWeights[i];
					bw.Weight /= overallWeight;
					skinningWeights[i] = bw;
				}
			}
			return skinningWeights;
		}
	}

	public static class PropagateMarkers
	{
		public static void Perform(Node node)
		{
			EnterNode.Perform(node);
			foreach (var m in node.DefaultAnimation.Markers) {
				SetMarker.Perform(m.Clone(), true);
				SetKeyframe.Perform(node, nameof(Node.Trigger), null, new Keyframe<string> {
					Frame = m.Frame,
					Value = m.Id,
					Function = KeyFunction.Linear
				});
			}
			LeaveNode.Perform();
		}
	}

	public static class Flip
	{
		public static void Perform(IEnumerable<Node> nodes, Widget container, bool flipX, bool flipY)
		{
			if (!flipX && !flipY) return;
			foreach (var widget in nodes.OfType<Widget>()) {
				var s = widget.Scale;
				if (flipX) {
					s.X = -s.X;
				}
				if (flipY) {
					s.Y = -s.Y;
				}
				SetAnimableProperty.Perform(widget, nameof(Widget.Scale), s);
			}
			FlipBones.Perform(nodes, container, flipX, flipY);
		}
	}

	public static class FlipBones
	{
		public static void Perform(IEnumerable<Node> nodes, Widget container, bool flipX, bool flipY)
		{
			if (!flipX && !flipY) return;
			var roots = new List<Bone>();
			foreach (var bone in nodes.OfType<Bone>()) {
				var root = BoneUtils.FindBoneRoot(bone, container.Nodes);
				if (!roots.Contains(root)) {
					if (flipX && flipY) {
						SetAnimableProperty.Perform(root, nameof(Bone.Rotation), root.Rotation + 180);
					} else {
						SetAnimableProperty.Perform(root, nameof(Bone.Rotation), (flipY ? 180 : 0) - root.Rotation);
						SetAnimableProperty.Perform(root, nameof(Bone.Length), -root.Length);
						var bones = BoneUtils.FindBoneDescendats(root,
							Document.Current.Container.Nodes.OfType<Bone>());
						foreach (var childBone in bones) {
							SetAnimableProperty.Perform(childBone, nameof(Bone.Rotation), -childBone.Rotation);
							SetAnimableProperty.Perform(childBone, nameof(Bone.Length), -childBone.Length);
						}
					}
					roots.Add(root);
				}
			}
		}
	}
}
