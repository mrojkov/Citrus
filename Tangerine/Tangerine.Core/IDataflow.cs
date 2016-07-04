using System;
using System.Linq;
using System.Collections.Generic;
using Lime;

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
	public interface IDataflow<T>
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

	public class Property<T> : IDataflowProvider<T> where T: IEquatable<T>
	{
		public Func<T> Getter { get; private set; }
		public Action<T> Setter { get; private set; }

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
	}


	class PropertyDataflow<T> : IDataflow<T> where T: IEquatable<T>
	{
		readonly Func<T> getter;

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
	}

	public static class DataSourceMixins
	{
		public static IDataflowProvider<R> Select<T, R>(this IDataflowProvider<T> arg, Func<T, R> selector)
		{
			return new DataflowProvider<R>(() => new SelectProvider<T, R>(arg.GetDataflow(), selector));
		}

		public static IDataflowProvider<Tuple<T1, T2>> With<T1, T2>(this IDataflowProvider<T1> arg1, IDataflowProvider<T2> arg2)
		{
			return new DataflowProvider<Tuple<T1, T2>>(() => new WithProvider<T1, T2>(arg1.GetDataflow(), arg2.GetDataflow()));
		}

		public static IDataflowProvider<T> Where<T>(this IDataflowProvider<T> arg, Predicate<T> predicate)
		{
			return new DataflowProvider<T>(() => new WhereProvider<T>(arg.GetDataflow(), predicate));
		}

		public static IDataflowProvider<T> Distinct<T>(this IDataflowProvider<T> arg)
		{
			return new DataflowProvider<T>(() => new DistinctProvider<T>(arg.GetDataflow()));
		}

		class SelectProvider<T, R> : IDataflow<R>
		{
			readonly IDataflow<T> arg;
			readonly Func<T, R> selector;

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
		}

		class WithProvider<T1, T2> : IDataflow<Tuple<T1, T2>>
		{
			readonly IDataflow<T1> arg1;
			readonly IDataflow<T2> arg2;

			public bool GotValue { get; private set; }
			public Tuple<T1, T2> Value { get; private set; }

			public WithProvider(IDataflow<T1> arg1, IDataflow<T2> arg2)
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
		}

		class WhereProvider<T> : IDataflow<T>
		{
			readonly IDataflow<T> arg;
			readonly Predicate<T> predicate;

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
		}

		class DistinctProvider<T> : IDataflow<T>
		{
			readonly IDataflow<T> arg;
			T previous;
			bool hasValue;

			public bool GotValue { get; private set; }
			public T Value { get; private set; }

			public DistinctProvider(IDataflow<T> arg)
			{
				this.arg = arg;
			}

			public void Poll()
			{
				arg.Poll();
				if ((GotValue = arg.GotValue)) {
					var current = arg.Value;
					if ((GotValue = !hasValue || (current != null) != (previous != null) || (current != null && !current.Equals(previous)))) {
						Value = current;
					}
					hasValue = true;
					previous = current;
				}
			}
		}

		class DataflowProvider<T> : IDataflowProvider<T>
		{
			readonly Func<IDataflow<T>> func;

			public DataflowProvider(Func<IDataflow<T>> func)
			{
				this.func = func;
			}

			public IDataflow<T> GetDataflow() => func();
		}
	}
}