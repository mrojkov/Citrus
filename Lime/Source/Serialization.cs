using System;
using System.Collections.Generic;
using System.IO;

namespace Lime
{
	public static class Serialization
	{
		enum OperationType
		{
			Clone,
			Serialization
		}
		
		struct Operation
		{
			public OperationType Type;
			public string SerializationPath;
		}
		
#if iOS
		public static ProtoBuf.Meta.TypeModel Serializer = ProtoBuf.Meta.RuntimeTypeModel.Default;
#else
		public static ProtoBuf.Meta.TypeModel Serializer = ProtoBuf.Meta.RuntimeTypeModel.Default;
#endif
		
		static readonly Stack<Operation> opStack = new Stack<Operation>();
		
		public static string ShrinkPath(string path)
		{
			if (opStack.Peek().Type == OperationType.Clone)
				return path;
			return '/' + path;
		}

		public static string ExpandPath(string path)
		{
			if (opStack.Peek().Type == OperationType.Clone)
				return path;
			string result;
			if (string.IsNullOrEmpty(path))
				return path;
			else if (path[0] == '/')
				result = path.Substring(1);
			else {
				string p = opStack.Peek().SerializationPath;
				result = Path.Combine(Path.GetDirectoryName(p), path);
			}
			return result;
		}

		public static void WriteObject<T>(string path, Stream stream, T instance)
		{
			opStack.Push(new Operation { SerializationPath = path, Type = OperationType.Serialization });
			try {
				Serializer.Serialize(stream, instance);
			} finally {
				opStack.Pop();
			}
		}

		public static void WriteObjectToFile<T>(string path, T instance)
		{
			using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
				WriteObject(path, stream, instance);
		}
		
		public static void WriteObjectToBundle<T>(AssetsBundle bundle, string path, T instance)
		{
			using (MemoryStream stream = new MemoryStream()) {
				WriteObject(path, stream, instance);
				stream.Seek(0, SeekOrigin.Begin);
				bundle.ImportFile(path, stream, 0);
			}
		}
		
		static Dictionary<object, MemoryStream> cloneCache = new Dictionary<object, MemoryStream>();
		
		// TODO: use weak references for keys
		public static T DeepCloneCached<T>(T obj)
		{
			opStack.Push(new Operation {Type = OperationType.Clone});
			try {
				MemoryStream stream;
				if (!cloneCache.TryGetValue(obj, out stream)) {
					stream = new MemoryStream();
					Serializer.Serialize(stream, obj);
					cloneCache[obj] = stream;
				}
				stream.Seek(0, SeekOrigin.Begin);
				return (T)Serializer.Deserialize(stream, null, typeof(T));
			} finally {
				opStack.Pop();
			}
		}

		public static T DeepClone<T>(T obj)
		{
			opStack.Push(new Operation { Type = OperationType.Clone });
			try {
				return (T)Serializer.DeepClone(obj);
			} finally {
				opStack.Pop();
			}
		}

		public static T ReadObject<T>(string path, Stream stream)
		{
			opStack.Push(new Operation { SerializationPath = path, Type = OperationType.Serialization });
			try {
				return (T)Serializer.Deserialize(stream, null, typeof(T));
			} finally {
				opStack.Pop();
			}
		}

		static Dictionary<string, MemoryStream> readCache = new Dictionary<string, MemoryStream>();
		
		public static T ReadObjectCached<T>(string path)
		{
			MemoryStream stream;
			if (!readCache.TryGetValue(path, out stream)) {
				stream = new MemoryStream();
				using (Stream input = AssetsBundle.Instance.OpenFileLocalized(path)) {
					input.CopyTo(stream);
				}
				readCache[path] = stream;
			}
			stream.Seek(0, SeekOrigin.Begin);
			return ReadObject<T>(path, stream);
		}

		public static T ReadObject<T>(string path)
		{
			using (Stream stream = AssetsBundle.Instance.OpenFileLocalized(path))
				return ReadObject<T>(path, stream);
		}
		
		public static T ReadObjectFromFile<T>(string path)
		{
			using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
				return ReadObject<T>(path, stream);
		}
	}
}