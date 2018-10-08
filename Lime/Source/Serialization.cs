using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Yuzu;
using Yuzu.Binary;
using Yuzu.Json;

namespace Lime
{
	public class Yuzu
	{
		public static Yuzu Current => stackOfCurrent.Value.Count > 0 ? stackOfCurrent.Value.Peek() : null;
		private static void PushCurrent(Yuzu yuzu) => stackOfCurrent.Value.Push(yuzu);
		private static Yuzu PopCurrent() => stackOfCurrent.Value.Pop();

		private static ThreadLocal<Stack<Yuzu>> stackOfCurrent = new ThreadLocal<Stack<Yuzu>>(() => new Stack<Yuzu>());

		public static ThreadLocal<Yuzu> Instance { get; } = new ThreadLocal<Yuzu>(() => new Yuzu());

		private Stack<string> pathStack = new Stack<string>();

		public readonly List<Serialization.DeserializerBuilder> DeserializerBuilders = new List<Serialization.DeserializerBuilder>();

		public UInt32 BinarySignature = 0xdeadbabe;

		private Yuzu()
		{
			DeserializerBuilders.Add(
				(path, stream) => new global::Yuzu.Json.JsonDeserializer {
					JsonOptions = YuzuJsonOptions,
					Options = YuzuCommonOptions,
				}
			);
		}

		public Yuzu(CommonOptions yuzuCommonOptions, JsonSerializeOptions yuzuJsonOptions) : this()
		{
			YuzuCommonOptions = yuzuCommonOptions;
			YuzuJsonOptions = yuzuJsonOptions;
		}

		public string ShrinkPath(string path)
		{
			if (pathStack.Count == 0 || string.IsNullOrEmpty(path)) {
				return path;
			}
			var d = GetCurrentSerializationDirectory() + '/';
			return path.StartsWith(d) ? path.Substring(d.Length) : '/' + path;
		}

		public string ExpandPath(string path)
		{
			if (pathStack.Count == 0 || string.IsNullOrEmpty(path)) {
				return path;
			}
			return (path[0] == '/') ? path.Substring(1) : GetCurrentSerializationDirectory() + '/' + path;
		}

		public readonly CommonOptions YuzuCommonOptions = defaultYuzuCommonOptions;
		public readonly JsonSerializeOptions YuzuJsonOptions = defaultYuzuJsonOptions;

		private static readonly CommonOptions defaultYuzuCommonOptions = new CommonOptions {
			TagMode = TagMode.Aliases,
			AllowEmptyTypes = true,
			CheckForEmptyCollections = true,
		};

		private static readonly JsonSerializeOptions defaultYuzuJsonOptions = new JsonSerializeOptions {
			ArrayLengthPrefix = false,
			Indent = "\t",
			FieldSeparator = "\n",
			SaveRootClass = true,
			Unordered = true,
			MaxOnelineFields = 8,
			BOM = true,
		};

		public void WriteObject<T>(string path, Stream stream, T instance, Serialization.Format format)
		{
			pathStack.Push(path);
			PushCurrent(this);
			AbstractWriterSerializer ys = null;
			try {
				if (format == Serialization.Format.Binary) {
					WriteYuzuBinarySignature(stream);
					ys = new global::Yuzu.Binary.BinarySerializer { Options = YuzuCommonOptions };
				} else if (format == Serialization.Format.JSON) {
					ys = new global::Yuzu.Json.JsonSerializer {
						Options = YuzuCommonOptions,
						JsonOptions = YuzuJsonOptions
					};
				}
				ys.ToStream(instance, stream);
			} finally {
				pathStack.Pop();
				PopCurrent();
			}
		}

		public void WriteObject<T>(string path, Stream stream, T instance, AbstractSerializer serializer)
		{
			pathStack.Push(path);
			PushCurrent(this);
			try {
				if (serializer is BinarySerializer) {
					WriteYuzuBinarySignature(stream);
				}
				serializer.ToStream(instance, stream);
			} finally {
				pathStack.Pop();
				PopCurrent();
			}
		}

		public void WriteObjectToFile<T>(string path, T instance, Serialization.Format format)
		{
			using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
				WriteObject(path, stream, instance, format);
		}

		public void WriteObjectToBundle<T>(AssetBundle bundle, string path, T instance, Serialization.Format format, string sourceExtension, AssetAttributes attributes, byte[] cookingRulesSHA1)
		{
			using (MemoryStream stream = new MemoryStream()) {
				WriteObject(path, stream, instance, format);
				stream.Seek(0, SeekOrigin.Begin);
				bundle.ImportFile(path, stream, 0, sourceExtension, attributes, cookingRulesSHA1);
			}
		}

		public T ReadObject<T>(string path, Stream stream, object obj = null)
		{
			var ms = new MemoryStream();
			stream.CopyTo(ms);
			ms.Seek(0, SeekOrigin.Begin);
			stream = ms;
			pathStack.Push(path);
			PushCurrent(this);
			try {
				AbstractDeserializer d = null;
				if (CheckBinarySignature(stream)) {
					d = new GeneratedDeserializersBIN.BinaryDeserializerGen { Options = YuzuCommonOptions };
				} else {
					foreach (var db in DeserializerBuilders) {
						d = db(path, stream);
						if (d != null)
							break;
					}
				}
				var bd = d as BinaryDeserializer;
				if (obj == null) {
					if (bd != null) {
						return bd.FromReader<T>(new BinaryReader(stream));
					} else {
						return d.FromStream<T>(stream);
					}
				} else {
					if (bd != null) {
						return (T)bd.FromReader(obj, new BinaryReader(stream));
					} else {
						return (T)d.FromStream(obj, stream);
					}
				}
			} finally {
				pathStack.Pop();
				PopCurrent();
			}
		}

		public T ReadObject<T>(string path, object obj = null)
		{
			using (Stream stream = AssetBundle.Current.OpenFileLocalized(path))
				return ReadObject<T>(path, stream, obj);
		}

		public T ReadObjectFromFile<T>(string path, object obj = null)
		{
			using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
				return ReadObject<T>(path, stream, obj);
		}

		public int CalcObjectCheckSum<T>(string path, T obj)
		{
			using (var memStream = new MemoryStream()) {
				WriteObject(path, memStream, obj, Serialization.Format.Binary);
				memStream.Flush();
				int checkSum = Toolbox.ComputeHash(memStream.GetBuffer(), (int)memStream.Length);
				return checkSum;
			}
		}

		public string GetCurrentSerializationPath()
		{
			return pathStack.Peek();
		}

		private string GetCurrentSerializationDirectory()
		{
			return Path.GetDirectoryName(pathStack.Peek());
		}

		private void WriteYuzuBinarySignature(Stream s)
		{
			var bw = new BinaryWriter(s);
			bw.Write(BinarySignature);
		}

		private bool CheckBinarySignature(Stream s)
		{
			UInt32 signature;
			try {
				using (var br = new BinaryReader(s, Encoding.UTF8, leaveOpen: true)) {
					signature = br.ReadUInt32();
				}
			} catch {
				s.Seek(0, SeekOrigin.Begin);
				return false;
			}
			bool r = signature == BinarySignature;
			if (!r) {
				s.Seek(0, SeekOrigin.Begin);
			}
			return r;
		}
	}

	public static class Serialization
	{
		public enum Format
		{
			JSON,
			Binary
		}
		public delegate AbstractDeserializer DeserializerBuilder(string path, Stream stream);
		public static List<DeserializerBuilder> DeserializerBuilders => Yuzu.Instance.Value.DeserializerBuilders;
		public static CommonOptions YuzuCommonOptions => Yuzu.Instance.Value.YuzuCommonOptions;

		public static void WriteObject<T>(string path, Stream stream, T instance, Format format) => Yuzu.Instance.Value.WriteObject(path, stream, instance, format);
		public static void WriteObject<T>(string path, Stream stream, T instance, AbstractSerializer serializer) => Yuzu.Instance.Value.WriteObject(path, stream, instance, serializer);
		public static void WriteObjectToFile<T>(string path, T instance, Format format) => Yuzu.Instance.Value.WriteObjectToFile(path, instance, format);
		public static void WriteObjectToBundle<T>(AssetBundle bundle, string path, T instance, Format format, string sourceExtension, AssetAttributes attributes, byte[] cookingRulesSHA1) => Yuzu.Instance.Value.WriteObjectToBundle(bundle, path, instance, format, sourceExtension, attributes, cookingRulesSHA1);
		public static T ReadObject<T>(string path, Stream stream, object obj = null) => Yuzu.Instance.Value.ReadObject<T>(path, stream, obj);
		public static T ReadObject<T>(string path, object obj = null) => Yuzu.Instance.Value.ReadObject<T>(path, obj);
		public static T ReadObjectFromFile<T>(string path, object obj = null) => Yuzu.Instance.Value.ReadObjectFromFile<T>(path, obj);
		public static int CalcObjectCheckSum<T>(string path, T obj) => Yuzu.Instance.Value.CalcObjectCheckSum(path, obj);
		public static string GetCurrentSerializationPath() => Yuzu.Instance.Value.GetCurrentSerializationPath();
	}
}
