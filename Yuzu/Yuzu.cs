using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Yuzu
{
	public class YuzuField : Attribute
	{
		public readonly string Alias;
		public YuzuField(string alias) { Alias = alias; }
	}

	public class YuzuRequired : YuzuField
	{
		public YuzuRequired(string alias = null) : base(alias) { }
	}

	public class YuzuOptional : YuzuField
	{
		public YuzuOptional(string alias = null) : base(alias) { }
	}

	public class YuzuMember : YuzuField
	{
		public YuzuMember(string alias = null) : base(alias) { }
	}

	public abstract class YuzuSerializeCondition : Attribute
	{
		public abstract bool Check(object obj, object field);
	}

	public class YuzuSerializeIf : YuzuSerializeCondition
	{
		public readonly string Method;
		public YuzuSerializeIf(string method) { Method = method; }

		private Func<object, bool> checker;

		public override bool Check(object obj, object field) {
			if (checker == null) {
				var fn = obj.GetType().GetMethod(Method);
				if (fn == null)
					throw new YuzuException();
				var p = Expression.Parameter(typeof(object));
				var e = Expression.Call(Expression.Convert(p, obj.GetType()), fn);
				checker = Expression.Lambda<Func<object, bool>>(e, p).Compile();
			}
			return checker(obj);
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

	public class YuzuAfterDeserialization : Attribute { }

	public class YuzuMerge : Attribute { }

	public enum TagMode
	{
		Names,
		Aliases,
		Ids,
	}

	public static class IdGenerator
	{
		static char[] lastId = new char[] { 'A', 'A', 'A', 'A' };

		private static void NextId()
		{
			var i = lastId.Length - 1;
			do {
				switch (lastId[i]) {
					case 'Z':
						lastId[i] = 'a';
						return;
					case 'z':
						lastId[i] = 'A';
						break;
					default:
						lastId[i] = (char)((int)lastId[i] + 1);
						return;
				}
				i--;
			} while (lastId[i] != 'A');
			lastId[i] = 'B';
		}

		public static string GetNextId()
		{
			NextId();
			return new string(lastId);
		}

	}

	public class MetaOptions
	{
		public static MetaOptions Default = new MetaOptions();

		public readonly Type RequiredAttribute = typeof(YuzuRequired);
		public readonly Type OptionalAttribute = typeof(YuzuOptional);
		public readonly Type MemberAttribute = typeof(YuzuMember);
		public readonly Type CompactAttribute = typeof(YuzuCompact);
		public readonly Type SerializeIfAttribute = typeof(YuzuSerializeCondition);
		public readonly Type AfterDeserializationAttribute = typeof(YuzuAfterDeserialization);
		public readonly Type MergeAttribute = typeof(YuzuMerge);

		public readonly Func<Attribute, string> GetAlias = attr => (attr as YuzuField).Alias;
		public readonly Func<Attribute, Func<object, object, bool>> GetSerializeCondition =
			attr => (attr as YuzuSerializeCondition).Check;

		public MetaOptions()
		{

		}
	}

	public class CommonOptions
	{
		public MetaOptions Meta = MetaOptions.Default;
		public TagMode TagMode = TagMode.Names;
		public bool IgnoreNewFields = false;
		public bool AllowEmptyTypes = false;
		public bool ReportErrorPosition = true;
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

		public abstract T FromReader<T>(BinaryReader reader);
		public abstract T FromString<T>(string source);
		public abstract T FromStream<T>(Stream source);
		public abstract T FromBytes<T>(byte[] bytes);

	}
}
