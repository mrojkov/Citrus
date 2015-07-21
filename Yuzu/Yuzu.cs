using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuzu
{
	public class YuzuDefault : Attribute { }

	public class CommonOptions
	{
		public Type DefaultAttribute = typeof(YuzuDefault);
	}

	public class YuzuException: Exception
	{
	}

	public abstract class AbstractSerializer
	{
		public CommonOptions Options = new CommonOptions();
		public BinaryWriter Writer;

		public abstract void ToWriter(object obj);

		public void ToWriter(object obj, BinaryWriter writer)
		{
			Writer = writer;
			ToWriter(obj);
		}

		protected void WriteStr(string s)
		{
			Writer.Write(Encoding.UTF8.GetBytes(s));
		}

		public string ToStringUTF8(object obj)
		{
			var ms = new MemoryStream();
			ToStream(obj, ms);
			return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
		}

		public byte[] ToBytes(object obj)
		{
			var ms = new MemoryStream();
			ToStream(obj, ms);
			var result = ms.GetBuffer();
			Array.Resize(ref result, (int)ms.Length);
			return result;
		}

		public void ToStream(object obj, Stream target)
		{
			ToWriter(obj, new BinaryWriter(target));
		}
	};

	public abstract class AbstractDeserializer
	{
		public CommonOptions Options = new CommonOptions();
		public BinaryReader Reader;

		public abstract void FromReader(object obj);

		public void FromReader(object obj, BinaryReader reader)
		{
			Reader = reader;
			FromReader(obj);
		}

		public void FromStringUTF8(object obj, string source)
		{
			FromReader(obj, new BinaryReader(new MemoryStream(Encoding.UTF8.GetBytes(source), false)));
		}

		public void FromStream(object obj, Stream source)
		{
			FromReader(obj, new BinaryReader(source));
		}

		public void FromBytes(object obj, byte[] bytes)
		{
			FromStream(obj, new MemoryStream(bytes, false));
		}

	};

}
