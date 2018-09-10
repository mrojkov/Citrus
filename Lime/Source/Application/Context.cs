using System;
using System.Collections.Generic;
using System.Linq;

namespace Lime
{
	public interface IContext
	{
		IContext Activate();
		void Deactivate();
		ContextScope Scoped();
	}

	public struct ContextScope : IDisposable
	{
		private IContext context;
		public ContextScope(IContext context) { this.context = context; }
		public void Dispose() { context.Deactivate(); }
	}

	public class Context : IContext
	{
		struct ActivationRecord
		{
			public Context Context;
			public object OldValue;
		}

		private object value;
		private Property property;

		[ThreadStatic]
		private static Stack<ActivationRecord> stack;

		public Context(Property property, object value)
		{
			EnsureStack();
			this.property = property;
			this.value = value;
		}

		protected Context(string propertyName)
		{
			EnsureStack();
			this.property = new Property(GetType(), propertyName);
			this.value = this;
		}

		protected Context(Property property)
		{
			EnsureStack();
			this.property = property;
			this.value = this;
		}

		private void EnsureStack()
		{
			stack = stack ?? new Stack<ActivationRecord>();
		}

		public IContext Activate()
		{
			var r = new ActivationRecord { Context = this, OldValue = property.Getter() };
			stack.Push(r);
			property.Setter(value);
			return this;
		}

		public void Deactivate()
		{
			var r = stack.Pop();
			if (r.Context != this) {
				throw new InvalidOperationException();
			}
			property.Setter(r.OldValue);
		}

		public ContextScope Scoped()
		{
			return new ContextScope(this);
		}
	}

	public class CombinedContext : IContext
	{
		private IContext[] contexts;

		public CombinedContext(params IContext[] contexts)
		{
			this.contexts = contexts;
		}

		public CombinedContext(IEnumerable<IContext> contexts)
		{
			this.contexts = contexts.ToArray();
		}

		public IContext Activate()
		{
			foreach (var i in contexts) {
				i.Activate();
			}
			return this;
		}

		public void Deactivate()
		{
			for (int i = contexts.Length - 1; i >= 0; i--) {
				contexts[i].Deactivate();
			}
		}

		public ContextScope Scoped()
		{
			return new ContextScope(this);
		}
	}

	public class Property
	{
		public Func<object> Getter { get; private set; }
		public Action<object> Setter { get; private set; }

		public Property(Func<object> getter, Action<object> setter)
		{
			Getter = getter;
			Setter = setter;
		}

		public static Property Create<T>(Func<T> getter, Action<T> setter)
		{
			return new Property(() => getter(), x => setter((T)x));
		}

		public Property(Type singleton, string propertyName = "Instance")
		{
			var pi = singleton.GetProperty(propertyName);
			Getter = () => pi.GetValue(null, null);
			Setter = val => pi.SetValue(null, val, null);
		}

		public Property(object obj, string propertyName)
		{
			var pi = obj.GetType().GetProperty(propertyName);
			Getter = () => pi.GetValue(obj, null);
			Setter = val => pi.SetValue(obj, val, null);
		}

		public object Value
		{
			get { return Getter(); }
			set { Setter(value); }
		}
	}

	public class IndexedProperty
	{
		public Func<object> Getter { get; private set; }
		public Action<object> Setter { get; private set; }

		public IndexedProperty(object obj, string propertyName, int index)
		{
			var pi = obj.GetType().GetProperty(propertyName);
			Getter = () => pi.GetGetMethod().Invoke(obj, new object[] { index });
			Setter = val => pi.GetSetMethod().Invoke(obj, new object[] { index, val });
		}

		public object Value
		{
			get { return Getter(); }
			set { Setter(value); }
		}
	}
}
