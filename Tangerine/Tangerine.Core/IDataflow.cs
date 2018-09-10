using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tangerine.Core
{
	/// <summary>
	/// Exposes the dataflow.
	/// </summary>
	public interface IDataflowProvider<T>
	{
		IDataflow<T> GetDataflow();
	}

	/// <summary>
	/// Represents the poll-based stream of values.
	/// </summary>
	public interface IDataflow<out T> : IDisposable
	{
		/// <summary>
		/// Polls the dataflow.
		/// </summary>
		void Poll();
		/// <summary>
		/// Indicates whether a new value has arrived.
		/// </summary>
		bool GotValue { get; }
		/// <summary>
		/// Returns the last received value from the dataflow.
		/// </summary>
		T Value { get; }
	}

	public class DataflowProvider<T> : IDataflowProvider<T>
	{
		private readonly Func<IDataflow<T>> func;

		public DataflowProvider(Func<IDataflow<T>> func)
		{
			this.func = func;
		}

		public IDataflow<T> GetDataflow() => func();
	}

	public class Property<T> : IDataflowProvider<T>
	{
		public Func<T> Getter { get; }
		public Action<T> Setter { get; }

		public Property(Func<T> getter)
		{
			Getter = getter;
		}

		public Property(Func<T> getter, Action<T> setter)
		{
			Getter = getter;
			Setter = setter;
		}

		public Property(object obj, string propertyName)
		{
			var pi = obj.GetType().GetProperty(propertyName);
			var getMethod = pi.GetGetMethod();
			Getter = (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), obj, getMethod);
			var setMethod = pi.GetSetMethod();
			if (setMethod != null) {
				Setter = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), obj, setMethod);
			}
		}

		IDataflow<T> IDataflowProvider<T>.GetDataflow() => new PropertyDataflow<T>(Getter);

		public T Value
		{
			get => Getter();
			set => Setter(value);
		}
	}

	public class IndexedProperty<T> : IDataflowProvider<T>
	{
		public Func<T> Getter { get; }
		public Action<T> Setter { get; }

		public IndexedProperty(object obj, string propertyName, int index)
		{
			var pi = obj.GetType().GetProperty(propertyName);
			var getMethod = pi.GetGetMethod();
			Getter = () => (T)getMethod.Invoke(obj, new object[] { index });
			var setMethod = pi.GetSetMethod();
			if (setMethod != null) {
				Setter = (v) => setMethod.Invoke(obj, new object[] { index, v });
			}
		}

		IDataflow<T> IDataflowProvider<T>.GetDataflow() => new PropertyDataflow<T>(Getter);

		public T Value
		{
			get => Getter();
			set => Setter(value);
		}
	}

	public class EventflowProvider<T> : IDataflowProvider<T>
	{
		private readonly object obj;
		private readonly string eventName;

		public EventflowProvider(object obj, string eventName)
		{
			this.obj = obj;
			this.eventName = eventName;
		}

		public IDataflow<T> GetDataflow() => new Eventflow<T>(obj, eventName);
	}

	public class Eventflow<T> : IDataflow<T>
	{
		private readonly List<T> queue = new List<T>();
		private readonly EventInfo eventInfo;
		private readonly Delegate @delegate;
		private readonly object obj;
		private bool disposed;

		public T Value { get; private set; }
		public bool GotValue { get; private set; }

		public Eventflow(object obj, string eventName)
		{
			this.obj = obj;
			eventInfo = obj.GetType().GetEvent(eventName);
			if (eventInfo == null) {
				throw new ArgumentException($"Unknown event {eventName} for type {obj.GetType()}");
			}
			@delegate = CreateDelegate(eventInfo, obj, eventName);
			eventInfo.AddEventHandler(obj, @delegate);
		}

		public void Dispose()
		{
			if (disposed) {
				throw new InvalidOperationException();
			}
			eventInfo.RemoveEventHandler(obj, @delegate);
			disposed = true;
		}

		private Delegate CreateDelegate(EventInfo evt, object obj, string eventName)
		{
			var p = evt.EventHandlerType.GetMethod("Invoke").GetParameters();
			if (p.Length == 1 && p[0].ParameterType == typeof(T)) {
				return Delegate.CreateDelegate(evt.EventHandlerType, this, "EventHandler1");
			}
			if (p.Length == 2 && p[0].ParameterType == typeof(object) && p[1].ParameterType == typeof(T)) {
				return Delegate.CreateDelegate(evt.EventHandlerType, this, "EventHandler2");
			}
			throw new ArgumentException();
		}

		private void EventHandler1(T value)
		{
			lock (queue) {
				queue.Add(value);
			}
		}

		private void EventHandler2(object sender, T value)
		{
			lock (queue) {
				queue.Add(value);
			}
		}

		public void Poll()
		{
			lock (queue) {
				if ((GotValue = queue.Count > 0)) {
					Value = queue[0];
					queue.RemoveAt(0);
				}
			}
		}
	}

	internal class PropertyDataflow<T> : IDataflow<T>
	{
		private readonly Func<T> getter;

		public T Value { get; private set; }
		public bool GotValue { get; private set; }

		public PropertyDataflow(Func<T> getter)
		{
			this.getter = getter;
		}

		public void Poll()
		{
			Value = getter();
			GotValue = true;
		}

		public void Dispose() { }
	}

	public static class DataflowMixins
	{
		public static bool Poll<T>(this IDataflow<T> dataflow, out T value)
		{
			dataflow.Poll();
			value = dataflow.Value;
			return dataflow.GotValue;
		}
	}

	public static class DataflowProviderMixins
	{
		public static IDataflowProvider<R> Select<T, R>(this IDataflowProvider<T> arg, Func<T, R> selector)
		{
			return new DataflowProvider<R>(() => new SelectProvider<T, R>(arg.GetDataflow(), selector));
		}

		public static IDataflowProvider<Tuple<T1, T2>> Coalesce<T1, T2>(this IDataflowProvider<T1> arg1, IDataflowProvider<T2> arg2)
		{
			return new DataflowProvider<Tuple<T1, T2>>(() => new CoalesceProvider<T1, T2>(arg1.GetDataflow(), arg2.GetDataflow()));
		}

		public static IDataflowProvider<T> Where<T>(this IDataflowProvider<T> arg, Predicate<T> predicate)
		{
			return new DataflowProvider<T>(() => new WhereProvider<T>(arg.GetDataflow(), predicate));
		}

		public static IDataflowProvider<T> DistinctUntilChanged<T>(this IDataflowProvider<T> arg)
		{
			return new DataflowProvider<T>(() => new DistinctUntilChangedProvider<T>(arg.GetDataflow()));
		}

		public static Consumer<T> WhenChanged<T>(this IDataflowProvider<T> arg, Action<T> action)
		{
			return DistinctUntilChanged(arg).Consume(action);
		}

		public static IDataflowProvider<T> SameOrDefault<T>(this IDataflowProvider<T> arg1, IDataflowProvider<T> arg2, T defaultValue = default(T))
		{
			return new DataflowProvider<T>(() => new SameOrDefaultProvider<T>(arg1.GetDataflow(), arg2.GetDataflow(), defaultValue));
		}

		public static IDataflowProvider<T> Skip<T>(this IDataflowProvider<T> arg, int count)
		{
			return new DataflowProvider<T>(() => new SkipProvider<T>(arg.GetDataflow(), count));
		}

		public static Consumer<T> Consume<T>(this IDataflowProvider<T> arg, Action<T> action)
		{
			return new Consumer<T>(arg.GetDataflow(), action);
		}

		public static T GetValue<T>(this IDataflowProvider<T> arg)
		{
			var dataflow = arg.GetDataflow();
			dataflow.Poll();
			return dataflow.Value;
		}

		public static bool TryGetValue<T>(this IDataflowProvider<T> arg, out T result)
		{
			var dataflow = arg.GetDataflow();
			dataflow.Poll();
			result = dataflow.GotValue ? dataflow.Value : default(T);
			return dataflow.GotValue;
		}

		private class SelectProvider<T, R> : IDataflow<R>
		{
			private readonly IDataflow<T> arg;
			private readonly Func<T, R> selector;

			public R Value { get; private set; }
			public bool GotValue { get; private set; }

			public SelectProvider(IDataflow<T> arg, Func<T, R> selector)
			{
				this.arg = arg;
				this.selector = selector;
			}

			public void Poll()
			{
				arg.Poll();
				if ((GotValue = arg.GotValue)) {
					Value = selector(arg.Value);
				}
			}

			public void Dispose()
			{
				arg.Dispose();
			}
		}

		private class CoalesceProvider<T1, T2> : IDataflow<Tuple<T1, T2>>
		{
			private readonly IDataflow<T1> arg1;
			private readonly IDataflow<T2> arg2;

			public bool GotValue { get; private set; }
			public Tuple<T1, T2> Value { get; private set; }

			public CoalesceProvider(IDataflow<T1> arg1, IDataflow<T2> arg2)
			{
				this.arg1 = arg1;
				this.arg2 = arg2;
			}

			public void Poll()
			{
				arg1.Poll();
				if ((GotValue = arg1.GotValue)) {
					arg2.Poll();
					Value = new Tuple<T1, T2>(arg1.Value, arg2.Value);
				}
			}

			public void Dispose()
			{
				arg1.Dispose();
				arg2.Dispose();
			}
		}

		private class WhereProvider<T> : IDataflow<T>
		{
			private readonly IDataflow<T> arg;
			private readonly Predicate<T> predicate;

			public bool GotValue { get; private set; }
			public T Value { get; private set; }

			public WhereProvider(IDataflow<T> arg, Predicate<T> predicate)
			{
				this.arg = arg;
				this.predicate = predicate;
			}

			public void Poll()
			{
				arg.Poll();
				if ((GotValue = arg.GotValue && predicate(arg.Value))) {
					Value = arg.Value;
				}
			}

			public void Dispose()
			{
				arg.Dispose();
			}
		}

		private class DistinctUntilChangedProvider<T> : IDataflow<T>
		{
			private readonly IDataflow<T> arg;
			private T previous;
			private bool hasValue;

			public bool GotValue { get; private set; }
			public T Value { get; private set; }

			public DistinctUntilChangedProvider(IDataflow<T> arg)
			{
				this.arg = arg;
			}

			public void Poll()
			{
				arg.Poll();
				if ((GotValue = arg.GotValue)) {
					var current = arg.Value;
					if ((GotValue = !hasValue || !EqualityComparer<T>.Default.Equals(current, previous))) {
						Value = current;
						hasValue = true;
						previous = current;
					}
				}
			}

			public void Dispose()
			{
				arg.Dispose();
			}
		}

		private class SkipProvider<T> : IDataflow<T>
		{
			private readonly IDataflow<T> arg;
			private int countdown;
			private bool done;

			public bool GotValue { get; private set; }
			public T Value { get; private set; }

			public SkipProvider(IDataflow<T> arg, int count)
			{
				this.arg = arg;
				countdown = count;
			}

			public void Poll()
			{
				arg.Poll();
				if ((GotValue = arg.GotValue && (done || countdown-- <= 0))) {
					done = true;
					Value = arg.Value;
				}
			}

			public void Dispose()
			{
				arg.Dispose();
			}
		}

		private class SameOrDefaultProvider<T> : IDataflow<T>
		{
			private readonly IDataflow<T> arg1;
			private readonly IDataflow<T> arg2;
			private readonly T defaultValue;

			public bool GotValue { get; private set; }
			public T Value { get; private set; }

			public SameOrDefaultProvider(IDataflow<T> arg1, IDataflow<T> arg2, T defaultValue)
			{
				this.arg1 = arg1;
				this.arg2 = arg2;
				this.defaultValue = defaultValue;
			}

			public void Poll()
			{
				arg1.Poll();
				arg2.Poll();
				if ((GotValue = arg1.GotValue || arg2.GotValue)) {
					Value = EqualityComparer<T>.Default.Equals(arg1.Value, arg2.Value) ? arg1.Value : defaultValue;
				}
			}

			public void Dispose()
			{
				arg1.Dispose();
				arg2.Dispose();
			}
		}

		public class Consumer<T> : ITaskProvider
		{
			private readonly IDataflow<T> dataflow;
			private readonly Action<T> action;

			public Consumer(IDataflow<T> dataflow, Action<T> action)
			{
				this.dataflow = dataflow;
				this.action = action;
			}

			public IEnumerator<object> Task()
			{
				try {
					while (true) {
						Execute();
						yield return null;
					}
				} finally {
					dataflow.Dispose();
				}
			}

			private void Execute()
			{
				dataflow.Poll();
				if (dataflow.GotValue) {
					action(dataflow.Value);
				}
			}
		}
	}
}
