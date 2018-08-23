using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Yuzu;
using Yuzu.Binary;
using Yuzu.Json;

namespace Lime
{
	public static class Serialization
	{
		static class SerializationStackKeeper
		{
			[ThreadStatic]
			public static Stack<string> stack;
		}

		static Stack<string> SerializationPathStack
		{
			get { return SerializationStackKeeper.stack ?? (SerializationStackKeeper.stack = new Stack<string>()); }
		}

		public delegate AbstractDeserializer DeserializerBuilder(string path, Stream stream);

		public static readonly List<DeserializerBuilder> DeserializerBuilders = new List<DeserializerBuilder> {
			(path, stream) => new Yuzu.Json.JsonDeserializer { JsonOptions = defaultYuzuJSONOptions, Options = DefaultYuzuCommonOptions }
		};

		public static string ShrinkPath(string path)
		{
			if (SerializationPathStack.Count == 0 || string.IsNullOrEmpty(path)) {
				return path;
			}
			var d = GetCurrentSerializationDirectory() + '/';
			return path.StartsWith(d) ? path.Substring(d.Length) : '/' + path;
		}

		public static string ExpandPath(string path)
		{
			if (SerializationPathStack.Count == 0 || string.IsNullOrEmpty(path)) {
				return path;
			}
			return (path[0] == '/') ? path.Substring(1) : GetCurrentSerializationDirectory() + '/' + path;
		}

		public static readonly CommonOptions DefaultYuzuCommonOptions = new CommonOptions {
			TagMode = TagMode.Aliases,
			AllowEmptyTypes = true,
			CheckForEmptyCollections = true,
		};

		private static readonly JsonSerializeOptions defaultYuzuJSONOptions = new JsonSerializeOptions {
			ArrayLengthPrefix = false,
			Indent = "\t",
			FieldSeparator = "\n",
			SaveRootClass = true,
			Unordered = true,
			MaxOnelineFields = 8,
		};

		public enum Format
		{
			JSON,
			Binary
		}

		public static void WriteObject<T>(string path, Stream stream, T instance, Format format)
		{
			SerializationPathStack.Push(path);
			AbstractWriterSerializer ys = null;
			try {
				if (format == Format.Binary) {
					WriteYuzuBinarySignature(stream);
					ys = new Yuzu.Binary.BinarySerializer { Options = DefaultYuzuCommonOptions };
				} else if (format == Format.JSON) {
					ys = new Yuzu.Json.JsonSerializer {
						Options = DefaultYuzuCommonOptions,
						JsonOptions = defaultYuzuJSONOptions
					};
				}
				ys.ToStream(instance, stream);
			} finally {
				SerializationPathStack.Pop();
			}
		}

		public static void WriteObject<T>(string path, Stream stream, T instance, AbstractSerializer serializer)
		{
			SerializationPathStack.Push(path);
			try {
				if (serializer is BinarySerializer) {
					WriteYuzuBinarySignature(stream);
				}
				serializer.ToStream(instance, stream);
			} finally {
				SerializationPathStack.Pop();
			}
		}

		public static void WriteObjectToFile<T>(string path, T instance, Format format)
		{
			using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
				WriteObject(path, stream, instance, format);
		}

		public static void WriteObjectToBundle<T>(AssetBundle bundle, string path, T instance, Format format, string sourceExtension, AssetAttributes attributes, byte[] cookingRulesSHA1)
		{
			using (MemoryStream stream = new MemoryStream()) {
				WriteObject(path, stream, instance, format);
				stream.Seek(0, SeekOrigin.Begin);
				bundle.ImportFile(path, stream, 0, sourceExtension, attributes, cookingRulesSHA1);
			}
		}

		public static T ReadObject<T>(string path, Stream stream, object obj = null)
		{
			var ms = new MemoryStream();
			stream.CopyTo(ms);
			ms.Seek(0, SeekOrigin.Begin);
			stream = ms;
			SerializationPathStack.Push(path);
			try {
				AbstractDeserializer d = null;
				if (CheckYuzuBinarySignature(stream)) {
					d = new GeneratedDeserializersBIN.BinaryDeserializerGen { Options = DefaultYuzuCommonOptions };
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
				SerializationPathStack.Pop();
			}
		}

		public static T ReadObject<T>(string path, object obj = null)
		{
			using (Stream stream = AssetBundle.Current.OpenFileLocalized(path))
				return ReadObject<T>(path, stream, obj);
		}

		public static T ReadObjectFromFile<T>(string path, object obj = null)
		{
			using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
				return ReadObject<T>(path, stream, obj);
		}

		public static int CalcObjectCheckSum<T>(string path, T obj)
		{
			using (var memStream = new MemoryStream()) {
				WriteObject(path, memStream, obj, Format.Binary);
				memStream.Flush();
				int checkSum = Toolbox.ComputeHash(memStream.GetBuffer(), (int)memStream.Length);
				return checkSum;
			}
		}

		public static string GetCurrentSerializationPath()
		{
			return SerializationPathStack.Peek();
		}

		private static string GetCurrentSerializationDirectory()
		{
			return Path.GetDirectoryName(SerializationPathStack.Peek());
		}

		private static void WriteYuzuBinarySignature(Stream s)
		{
			var bw = new BinaryWriter(s);
			bw.Write(0xdeadbabe);
		}

		private static bool CheckYuzuBinarySignature(Stream s)
		{
			UInt32 signature;
			try {
				// TODO: switch to 4.5+, use `using` and `leaveOpen = true`
				var br = new BinaryReader(s);
				signature = br.ReadUInt32();
			} catch {
				s.Seek(0, SeekOrigin.Begin);
				return false;
			}
			bool r = signature == 0xdeadbabe;
			if (!r) {
				s.Seek(0, SeekOrigin.Begin);
			}
			return r;
		}

		public static void GenerateDeserializers(string filename, string rootNamespace, List<Type> types)
		{
			var yjdg = new BinaryDeserializerGenerator(rootNamespace);
			using (var ms = new MemoryStream())
			using (var sw = new StreamWriter(ms)) {
				yjdg.GenWriter = sw;
				yjdg.GenerateHeader();
				foreach (var generate in types
					.Select(t => yjdg.GetType()
					.GetMethod("Generate")
					.MakeGenericMethod(t))) {
					generate.Invoke(yjdg, new object[] { });
				}
				yjdg.GenerateFooter();
				sw.Flush();
				ms.WriteTo(new FileStream(filename, FileMode.Create));
			}
		}

		public static void GenerateBinaryDeserializers()
		{
			var jd = new BinaryDeserializerGenerator("GeneratedDeserializersBIN");
			using (var ms = new MemoryStream())
			using (var sw = new StreamWriter(ms)) {
				jd.GenWriter = sw;
				jd.GenerateHeader();

				var types = new List<Type>();
				var assembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("Lime", StringComparison.OrdinalIgnoreCase)).First();
				foreach (var t in assembly.GetTypes()) {
					if (t.GetCustomAttribute<YuzuDontGenerateDeserializerAttribute>(false) != null) {
						continue;
					}
					if (t.IsGenericType) {
						if (t == typeof(Keyframe<>) || t == typeof(Animator<>)) {
							foreach (var specializationType in AnimatorRegistry.Instance.EnumerateRegisteredTypes()) {
								var specializedType = t.MakeGenericType(new[] { specializationType });
								types.Add(specializedType);
							}
						} else {
							foreach (var specializationType in t.GetCustomAttributes<YuzuSpecializeWithAttribute>().Select(a => a.Type)) {
								var specializedType = t.MakeGenericType(new[] { specializationType });
								types.Add(specializedType);
							}
						}
					} else {
						var meta = Yuzu.Metadata.Meta.Get(t, DefaultYuzuCommonOptions);
						if (meta.Items.Count != 0) {
							types.Add(t);
						}
					}
				}
				types.Sort((a, b) => a.FullName.CompareTo(b.FullName));
				foreach (var t in types) {
					jd.Generate(t);
					Console.WriteLine(t.FullName);
				}

				jd.GenerateFooter();
				sw.Flush();
				var executablePath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
				string goUp =

#if WIN
				"/../../../..";
#elif MAC || MONOMAC
				"/../../../../../../..";
#else
				"";
				throw new NotSupportedException();
#endif
				if (Application.IsTangerine) {
					goUp =
#if WIN
					"/../../..";
#elif MAC || MONOMAC
					"/../../../../../..";
#else
					"";
					throw new NotSupportedException();
#endif
				}
				ms.WriteTo(new FileStream(executablePath + goUp + @"/Lime/Source/GeneratedDeserializersBIN.cs", FileMode.Create));
			}
		}
	}
}
