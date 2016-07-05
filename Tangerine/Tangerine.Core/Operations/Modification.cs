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

		public SetProperty(object obj, string propertyName, object value)
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

		public SetGenericProperty(Func<T> getter, Action<T> setter, T value)
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

	public class SetAnimableProperty : CompoundOperation
	{
		public SetAnimableProperty(object @object, string propertyName, object value)
		{
			Add(new SetProperty(@object, propertyName, value));
			IAnimator animator;
			var animable = @object as IAnimable;
			if (animable != null && animable.Animators.TryFind(propertyName, out animator, Document.Current.AnimationId)) {
				var type = animable.GetType().GetProperty(propertyName).PropertyType;
				var key =
					animator.ReadonlyKeys.FirstOrDefault(i => i.Frame == Document.Current.AnimationFrame)?.Clone() ??
					Keyframe.CreateForType(type);
				key.Frame = Document.Current.AnimationFrame;
				key.Value = value;
				Add(new SetKeyframe(animable, propertyName, Document.Current.AnimationId, key));
			}
		}
	}

	public class RemoveKeyframe : IOperation
	{
		readonly int frame;
		readonly IAnimator animator;
		IKeyframe savedKeyframe;

		public RemoveKeyframe(IAnimator animator, int frame)
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

		public SetKeyframe(IAnimable animable, string propertyName, string animationId, IKeyframe keyframe)
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

		public InsertNode(Node container, int index, Node node)
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

	public class UnlinkNode : IOperation
	{
		readonly Node node;
		int savedIndex;
		Node container;

		public UnlinkNode(Node node)
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
}
