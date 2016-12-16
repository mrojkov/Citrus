using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace Yuzu
{
	public class YuzuField : Attribute
	{
		public readonly string Alias;
		public YuzuField(string alias) { Alias = alias; }
	}

	// YuzuField attributes must have default constructors for YuzuAll to work.

	public class YuzuRequired : YuzuField
	{
		public YuzuRequired() : base(null) { }
		public YuzuRequired(string alias) : base(alias) { }
	}

	public class YuzuOptional : YuzuField
	{
		public YuzuOptional() : base(null) { }
		public YuzuOptional(string alias) : base(alias) { }
	}

	public class YuzuMember : YuzuField
	{
		public YuzuMember() : base(null) { }
		public YuzuMember(string alias) : base(alias) { }
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

	public class YuzuBeforeSerialization : Attribute { }
	public class YuzuAfterDeserialization : Attribute { }

	public class YuzuMerge : Attribute { }

	[Flags]
	public enum YuzuItemKind
	{
		None = 0,
		Field = 1,
		Property = 2,
		Any = 3,
	}

	public enum YuzuItemOptionality
	{
		None = 0,
		Optional = 1,
		Required = 2,
		Member = 3,
	}

	public class YuzuMust : Attribute
	{
		public readonly YuzuItemKind Kind;
		public YuzuMust(YuzuItemKind kind = YuzuItemKind.Any) { Kind = kind; }
	}

	public class YuzuAll : Attribute
	{
		public readonly YuzuItemOptionality Optionality = YuzuItemOptionality.Member;
		public readonly YuzuItemKind Kind;
		public YuzuAll(YuzuItemOptionality optionality, YuzuItemKind kind = YuzuItemKind.Any)
		{
			Optionality = optionality;
			Kind = kind;
		}
		public YuzuAll(YuzuItemKind kind = YuzuItemKind.Any) { Kind = kind; }
	}

	public class YuzuExclude : Attribute { }

	public class YuzuAllowReadingFromAncestor : Attribute { }

	public class YuzuSurrogateIf : Attribute { }
	public class YuzuToSurrogate : Attribute { }
	public class YuzuFromSurrogate : Attribute { }

	public enum TagMode
	{
		Aliases = 0,
		Names,
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

		public Type RequiredAttribute = typeof(YuzuRequired);
		public Type OptionalAttribute = typeof(YuzuOptional);
		public Type MemberAttribute = typeof(YuzuMember);
		public Type CompactAttribute = typeof(YuzuCompact);
		public Type SerializeIfAttribute = typeof(YuzuSerializeCondition);
		public Type BeforeSerializationAttribute = typeof(YuzuBeforeSerialization);
		public Type AfterDeserializationAttribute = typeof(YuzuAfterDeserialization);
		public Type MergeAttribute = typeof(YuzuMerge);
		public Type MustAttribute = typeof(YuzuMust);
		public Type AllAttribute = typeof(YuzuAll);
		public Type ExcludeAttribute = typeof(YuzuExclude);
		public Type AllowReadingFromAncestorAttribute = typeof(YuzuAllowReadingFromAncestor);

		public Type SurrogateIfAttribute = typeof(YuzuSurrogateIf);
		public Type ToSurrogateAttribute = typeof(YuzuToSurrogate);
		public Type FromSurrogateAttribute = typeof(YuzuFromSurrogate);

		public Func<Attribute, string> GetAlias = attr => (attr as YuzuField).Alias;
		public Func<Attribute, Func<object, object, bool>> GetSerializeCondition =
			attr => (attr as YuzuSerializeCondition).Check;
		public Func<Attribute, YuzuItemKind> GetItemKind = attr => (attr as YuzuMust).Kind;
		public Func<Attribute, Tuple<YuzuItemOptionality, YuzuItemKind>> GetItemOptionalityAndKind =
			attr => Tuple.Create((attr as YuzuAll).Optionality, (attr as YuzuAll).Kind);
	}

	public struct CommonOptions
	{
		public MetaOptions Meta;
		public TagMode TagMode;
		public bool AllowUnknownFields;
		public bool AllowEmptyTypes;
		public bool ReportErrorPosition;
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

	public class YuzuUnknown
	{
		public string ClassTag;
		public SortedDictionary<string, object> Fields = new SortedDictionary<string, object>();
	}

	public class YuzuUnknownStorage
	{
		public struct Item
		{
			public string Name;
			public object Value;
			static public int Comparer(Item i1, Item i2) { return String.CompareOrdinal(i1.Name, i2.Name); }
		}
		public List<Item> Fields = new List<Item>();
		public bool IsOrdered { get; private set; }
		internal object Internal;

		public YuzuUnknownStorage() { IsOrdered = true; }

		public void Sort()
		{
			if (IsOrdered)
				return;
			Fields.Sort(Item.Comparer);
			IsOrdered = true;
		}
		public void Clear()
		{
			Fields.Clear();
			IsOrdered = true;
		}
		public virtual void Add(string name, object value)
		{
			Fields.Add(new Item { Name = name, Value = value });
			if (Fields.Count > 1 && IsOrdered)
				IsOrdered = Item.Comparer(Fields[0], Fields[1]) < 0;
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

	public interface IDeserializerGenerator
	{
		StreamWriter GenWriter { get; set; }
		void GenerateHeader();
		void GenerateFooter();
		void Generate<T>();
		void Generate(Type t);
	}
}
