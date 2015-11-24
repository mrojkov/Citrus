using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Yuzu
{
	public class YuzuOrder : Attribute
	{
		public readonly int Order = 0;
		public YuzuOrder(int order) { Order = order; }
	}

	public class YuzuRequired : YuzuOrder
	{
		public YuzuRequired(int order): base(order) { }
	}

	public class YuzuOptional : YuzuOrder
	{
		public YuzuOptional(int order) : base(order) { }
	}

	public abstract class YuzuSerializeCondition : Attribute
	{
		public abstract bool Check(object obj, object field);
	}

	public class YuzuSerializeIf : YuzuSerializeCondition
	{
		public readonly string Method;
		public YuzuSerializeIf(string method) { Method = method; }

		private Func<bool> checker;

		public override bool Check(object obj, object field) {
			if (checker == null) {
				var fn = obj.GetType().GetMethod(Method);
				if (fn == null)
					throw new YuzuException();
				var e = Expression.Call(Expression.Constant(obj), fn);
				checker = Expression.Lambda<Func<bool>>(e).Compile();
			}
			return checker();
		}
	}

	public class YuzuDefault : YuzuSerializeCondition
	{
		public readonly object Value;
		public YuzuDefault(object value)
		{
			Value = value;
		}
		public override bool Check(object obj, object field)
		{
			return !Value.Equals(field);
		}
	}

	public class YuzuCompact : Attribute { }

	public class CommonOptions
	{
		public Type RequiredAttribute = typeof(YuzuRequired);
		public Type OptionalAttribute = typeof(YuzuOptional);
		public Type CompactAttribute = typeof(YuzuCompact);
		public Type SerializeIfAttribute = typeof(YuzuSerializeCondition);
		public Func<Attribute, int> GetOrder = attr => (attr as YuzuOrder).Order;
		public Func<Attribute, Func<object, object, bool>> GetSerializeCondition =
			attr => (attr as YuzuSerializeCondition).Check;
		public bool ClassNames = false;
		public Assembly Assembly = Assembly.GetCallingAssembly();
	}

	public class YuzuPosition
	{
		public readonly long Offset = 0;
		public YuzuPosition(long offset) { Offset = offset; }
		public override string ToString()
		{
			return "byte " + Offset.ToString();
		}
	}

	public class YuzuException: Exception
	{
		public readonly YuzuPosition Position = null;

		public YuzuException() { }

		public YuzuException(string message, YuzuPosition position = null): base(
			position == null ? message : message + " at " + position.ToString())
		{
			Position = position;
		}
	}

	public class YuzuAssert : YuzuException
	{
		public YuzuAssert(string message = "") : base(message) { }
	}

	public abstract class AbstractSerializer
	{
		public CommonOptions Options = new CommonOptions();
		public abstract void ToWriter(object obj, BinaryWriter writer);
		public abstract string ToString(object obj);
		public abstract byte[] ToBytes(object obj);
		public abstract void ToStream(object obj, Stream target);
	}

	public abstract class AbstractWriterSerializer: AbstractSerializer
	{
		protected BinaryWriter writer;

		protected abstract void ToWriter(object obj);

		public override void ToWriter(object obj, BinaryWriter writer)
		{
			this.writer = writer;
			ToWriter(obj);
		}

		protected void WriteStr(string s)
		{
			writer.Write(Encoding.UTF8.GetBytes(s));
		}

		public override string ToString(object obj)
		{
			var ms = new MemoryStream();
			ToStream(obj, ms);
			return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
		}

		public override byte[] ToBytes(object obj)
		{
			var ms = new MemoryStream();
			ToStream(obj, ms);
			var result = ms.GetBuffer();
			Array.Resize(ref result, (int)ms.Length);
			return result;
		}

		public override void ToStream(object obj, Stream target)
		{
			ToWriter(obj, new BinaryWriter(target));
		}
	}

	public abstract class AbstractStringSerializer : AbstractSerializer
	{
		protected StringBuilder builder;

		protected abstract void ToBuilder(object obj);

		public override void ToWriter(object obj, BinaryWriter writer)
		{
			writer.Write(ToBytes(obj));
		}

		public override string ToString(object obj)
		{
			builder = new StringBuilder();
			ToBuilder(obj);
			return builder.ToString();
		}

		public override byte[] ToBytes(object obj)
		{
			return Encoding.UTF8.GetBytes(ToString(obj));
		}

		public override void ToStream(object obj, Stream target)
		{
			var b = ToBytes(obj);
			target.Write(b, 0, b.Length);
		}
	}

	public abstract class AbstractDeserializer
	{
		public CommonOptions Options = new CommonOptions();

		public abstract object FromReader(object obj, BinaryReader reader);
		public abstract object FromString(object obj, string source);
		public abstract object FromStream(object obj, Stream source);
		public abstract object FromBytes(object obj, byte[] bytes);

		public abstract object FromReader(BinaryReader reader);
		public abstract object FromString(string source);
		public abstract object FromStream(Stream source);
		public abstract object FromBytes(byte[] bytes);
	}

	public abstract class AbstractReaderDeserializer: AbstractDeserializer
	{
		public BinaryReader Reader;

		public virtual void Initialize() { }
		public abstract object FromReaderInt();
		public abstract object FromReaderInt(object obj);

		public override object FromReader(object obj, BinaryReader reader)
		{
			Reader = reader;
			Initialize();
			return FromReaderInt(obj);
		}

		public override object FromString(object obj, string source)
		{
			return FromReader(obj, new BinaryReader(new MemoryStream(Encoding.UTF8.GetBytes(source), false)));
		}

		public override object FromStream(object obj, Stream source)
		{
			return FromReader(obj, new BinaryReader(source));
		}

		public override object FromBytes(object obj, byte[] bytes)
		{
			return FromStream(obj, new MemoryStream(bytes, false));
		}

		public override object FromReader(BinaryReader reader)
		{
			Reader = reader;
			return FromReaderInt();
		}

		public override object FromString(string source)
		{
			return FromReader(new BinaryReader(new MemoryStream(Encoding.UTF8.GetBytes(source), false)));
		}

		public override object FromStream(Stream source)
		{
			return FromReader(new BinaryReader(source));
		}

		public override object FromBytes(byte[] bytes)
		{
			return FromStream(new MemoryStream(bytes, false));
		}
	}

}
