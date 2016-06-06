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

		public SetProperty(object obj, string name, object value)
		{
			this.obj = obj;
			this.value = value;
			property = obj.GetType().GetProperty(name);
		}

		public void Do()
		{
			savedValue = property.GetValue(obj);
			property.SetValue(obj, value);
		}

		public void Undo()
		{
			property.SetValue(obj, savedValue);
			savedValue = null;
		}
	}

	public class SetProperty<T> : IOperation
	{
		Func<T> getter;
		Action<T> setter;
		T value;
		T savedValue;

		public SetProperty(Func<T> getter, Action<T> setter, T value)
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
		readonly Node node;
		readonly string property;
		readonly IKeyframe keyframe;
		IKeyframe savedKeyframe;
		bool animatorExists;

		public SetKeyframe(Node node, string property, IKeyframe keyframe)
		{
			this.node = node;
			this.property = property;
			this.keyframe = keyframe;
		}

		public void Do()
		{
			animatorExists = node.Animators.Any(a => a.TargetProperty == property);
			var animator = node.Animators[property];
			savedKeyframe = animator.Keys.FirstOrDefault(k => k.Frame == keyframe.Frame);
			animator.Keys.AddOrdered(keyframe);
		}

		public void Undo()
		{
			var animator = node.Animators.FirstOrDefault(i => i.TargetProperty == property);
			animator.Keys.Remove(keyframe);
			if (savedKeyframe != null) {
				animator.Keys.AddOrdered(savedKeyframe);
			}
			if (!animatorExists) {
				node.Animators.Remove(animator);
			}
			savedKeyframe = null;
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
