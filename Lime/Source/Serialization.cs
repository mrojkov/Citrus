using System;
using System.Collections.Generic;
using System.IO;

namespace Lime
{
	public static class Serialization
	{
		const float iPadDeserializationSpeed = 1024 * 1024;

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
		public static ProtoBuf.Meta.TypeModel Serializer = null;
#else
		public static ProtoBuf.Meta.TypeModel Serializer = CreateSerializer();
#endif
#if !iOS
		public static ProtoBuf.Meta.RuntimeTypeModel CreateSerializer()
		{
			var model = ProtoBuf.Meta.RuntimeTypeModel.Create();
			model.UseImplicitZeroDefaults = false;
			// Add ITexture type here due a bug in ProtoBuf-Net
			model.Add(typeof(ITexture), true);
			model.CompileInPlace();
			return model;
		}
#endif
		static class OperationStackCapsule
		{
			[ThreadStatic]
			public static Stack<Operation> OpStack;
		}

		static Stack<Operation> opStack {
			get
			{
				if (OperationStackCapsule.OpStack == null) {
					OperationStackCapsule.OpStack = new Stack<Operation>();
				}
				return OperationStackCapsule.OpStack;
			}
		}
		
		public static string ShrinkPath(string path)
		{
			if (opStack.Count == 0) {
				return path;
			}
			if (opStack.Peek().Type == OperationType.Clone)
				return path;
			return '/' + path;
		}

		public static string ExpandPath(string path)
		{
			if (opStack.Count == 0) {
				return path;
			}
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
		
		public static void WriteObjectToBundle<T>(AssetsBundle bundle, string path, T instance, bool compress = false)
		{
			using (MemoryStream stream = new MemoryStream()) {
				WriteObject(path, stream, instance);
				stream.Seek(0, SeekOrigin.Begin);
				bundle.ImportFile(path, stream, 0, compress: compress);
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

		public static T ReadObject<T>(string path, Stream stream, object obj = null)
		{
			opStack.Push(new Operation { SerializationPath = path, Type = OperationType.Serialization });
			try {
				return (T)Serializer.Deserialize(stream, obj, typeof(T));
			} finally {
				opStack.Pop();
			}
		}

		public static T ReadObject<T>(string path, object obj = null)
		{
			using (Stream stream = PackedAssetsBundle.Instance.OpenFileLocalized(path))
				return ReadObject<T>(path, stream, obj);
		}
		
		public static T ReadObjectFromFile<T>(string path, object obj = null)
		{
			using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
				return ReadObject<T>(path, stream, obj);
		}

	}
}