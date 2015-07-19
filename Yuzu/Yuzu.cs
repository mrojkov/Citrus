using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuzu
{
	public class CommonOptions
	{
	}

	public class YuzuException: Exception
	{
	}

	public abstract class AbstractSerializer
	{
		public CommonOptions Options = new CommonOptions();
		public BinaryWriter Writer;

		public abstract void Serialize(object obj);

		public string SerializeToStringUTF8(object obj)
		{
			var ms = new MemoryStream();
			Writer = new BinaryWriter(ms);
			Serialize(obj);
			return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
		}

		public void SerializeToStream(object obj, Stream target)
		{
			Writer = new BinaryWriter(target);
			Serialize(obj);
		}
	};

	public abstract class AbstractDeserializer
	{
		public CommonOptions Options = new CommonOptions();
		public BinaryReader Reader;

		public abstract void Deserialize(object obj);

		public void DeserializeFromStringUTF8(object obj, string source)
		{
			Reader = new BinaryReader(new MemoryStream(Encoding.UTF8.GetBytes(source), false));
			Deserialize(obj);
		}

		public void DeserializeFromStream(object obj, Stream source)
		{
			Reader = new BinaryReader(source);
			Deserialize(obj);
		}
	};

}
