using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using System.Reflection;

namespace Tangerine.Core.Operations
{
	public class SetProperty : IOperation
	{
		readonly object obj;
		readonly object value;
		readonly PropertyInfo property;
		object savedValue;

		public bool IsChangingDocument => true;
		public DateTime Timestamp { get; set; }

		public static void Perform(object obj, string propertyName, object value)
		{
			Document.Current.History.Perform(new SetProperty(obj, propertyName, value));
		}

		private SetProperty(object obj, string propertyName, object value)
		{
			this.obj = obj;
			this.value = value;
			property = obj.GetType().GetProperty(propertyName);
		}

		public void Do()
		{
			savedValue = property.GetValue(obj, null);
			property.SetValue(obj, value, null);
		}

		public void Undo()
		{
			property.SetValue(obj, savedValue, null);
			savedValue = null;
		}
	}

	public class SetGenericProperty<T> : IOperation
	{
		Func<T> getter;
		Action<T> setter;
		T value;
		T savedValue;

		public bool IsChangingDocument => true;
		public DateTime Timestamp { get; set; }

		public static void Perform(Func<T> getter, Action<T> setter, T value)
		{
			Document.Current.History.Perform(new SetGenericProperty<T>(getter, setter, value));
		}

		private SetGenericProperty(Func<T> getter, Action<T> setter, T value)
		{
			this.value = value;
			this.getter = getter;
			this.setter = setter;
		}

		public void Do()
		{
			savedValue = getter();
			setter(value);
		}

		public void Undo()
		{
			setter(savedValue);
		}
	}

	public class SetAnimableProperty
	{
		public static void Perform(IEnumerable<object> objects, string propertyName, object value)
		{
			foreach (var obj in objects) {
				Perform(obj, propertyName, value);
			}
		}

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
				key.Value = value;
				SetKeyframe.Perform(animable, propertyName, Document.Current.AnimationId, key);
			}
		}
	}

	public class RemoveKeyframe : IOperation
	{
		readonly int frame;
		readonly IAnimator animator;
		IKeyframe savedKeyframe;

		public bool IsChangingDocument => true;
		public DateTime Timestamp { get; set; }

		public static void Perform(IAnimator animator, int frame)
		{
			Document.Current.History.Perform(new RemoveKeyframe(animator, frame));
		}

		private RemoveKeyframe(IAnimator animator, int frame)
		{
			this.frame = frame;
			this.animator = animator;
		}

		public void Do()
		{
			savedKeyframe = animator.Keys.FirstOrDefault(k => k.Frame == frame);
			animator.Keys.Remove(savedKeyframe);
		}

		public void Undo()
		{
			animator.Keys.AddOrdered(savedKeyframe);
			savedKeyframe = null;
		}
	}

	public class SetKeyframe : IOperation
	{
		readonly IAnimable animable;
		readonly string propertyName;
		readonly string animationId;
		readonly IKeyframe keyframe;
		IKeyframe savedKeyframe;
		bool animatorExists;
		IAnimator animator;

		public bool IsChangingDocument => true;
		public DateTime Timestamp { get; set; }

		public static void Perform(IAnimable animable, string propertyName, string animationId, IKeyframe keyframe)
		{
			Document.Current.History.Perform(new SetKeyframe(animable, propertyName, animationId, keyframe));
		}

		private SetKeyframe(IAnimable animable, string propertyName, string animationId, IKeyframe keyframe)
		{
			this.animable = animable;
			this.propertyName = propertyName;
			this.keyframe = keyframe;
			this.animationId = animationId;
		}

		public void Do()
		{
			animatorExists = animable.Animators.Any(a => a.TargetProperty == propertyName && a.AnimationId == animationId);
			animator = animable.Animators[propertyName, animationId];
			savedKeyframe = animator.Keys.FirstOrDefault(k => k.Frame == keyframe.Frame);
			animator.Keys.AddOrdered(keyframe);
		}

		public void Undo()
		{
			animator.Keys.Remove(keyframe);
			if (savedKeyframe != null) {
				animator.Keys.AddOrdered(savedKeyframe);
			}
			if (!animatorExists) {
				animable.Animators.Remove(animator);
			}
			savedKeyframe = null;
			animator = null;
		}
	}

	public class InsertNode : IOperation
	{
		readonly Node container;
		readonly int index;
		readonly Node node;

		public bool IsChangingDocument => true;
		public DateTime Timestamp { get; set; }

		public static void Perform(Node container, int index, Node node)
		{
			Document.Current.History.Perform(new InsertNode(container, index, node));
		}

		private InsertNode(Node container, int index, Node node)
		{
			this.container = container;
			this.index = index;
			this.node = node;
		}

		public void Do()
		{
			container.Nodes.Insert(index, node);
		}

		public void Undo()
		{
			node.Unlink();
		}
	}

	public static class CreateNode
	{
		public static Node Perform(Node container, int index, Type nodeType)
		{
			if (!nodeType.IsSubclassOf(typeof(Node))) {
				throw new InvalidOperationException();
			}
			var ctr = nodeType.GetConstructor(System.Type.EmptyTypes);
			var node = (Node)ctr.Invoke(new object[] {});
			node.Id = GenerateNodeId(container, nodeType);
			InsertNode.Perform(container, index, node);
			ClearRowSelection.Perform();
			SelectNode.Perform(node);
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

	public class UnlinkNode : IOperation
	{
		readonly Node node;
		int savedIndex;
		Node container;

		public bool IsChangingDocument => true;
		public DateTime Timestamp { get; set; }

		public static void Perform(Node node)
		{
			Document.Current.History.Perform(new UnlinkNode(node));
		}

		private UnlinkNode(Node node)
		{
			this.node = node;
		}

		public void Do()
		{
			container = node.Parent;
			savedIndex = container.Nodes.IndexOf(node);
			node.Unlink();
		}

		public void Undo()
		{
			container.Nodes.Insert(savedIndex, node);
			container = null;
		}
	}

	public class SetMarker : IOperation
	{
		readonly MarkerCollection collection;
		readonly Marker marker;
		Marker savedMarker;

		public bool IsChangingDocument => true;
		public DateTime Timestamp { get; set; }

		public static void Perform(MarkerCollection collection, Marker marker)
		{
			Document.Current.History.Perform(new SetMarker(collection, marker));
		}

		private SetMarker(MarkerCollection collection, Marker marker)
		{
			this.collection = collection;
			this.marker = marker;
		}

		public void Do()
		{
			savedMarker = collection.FirstOrDefault(i => i.Frame == marker.Frame);
			collection.AddOrdered(marker);
		}

		public void Undo()
		{
			collection.Remove(marker);
			if (savedMarker != null) {
				collection.AddOrdered(savedMarker);
			}
		}
	}
}
