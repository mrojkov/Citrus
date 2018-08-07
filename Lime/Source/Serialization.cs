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
			(path, stream) => new Yuzu.Json.JsonDeserializer { JsonOptions = defaultYuzuJSONOptions, Options = defaultYuzuCommonOptions }
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

		private static readonly CommonOptions defaultYuzuCommonOptions = new CommonOptions {
			TagMode = TagMode.Aliases,
			AllowEmptyTypes = true
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
					ys = new Yuzu.Binary.BinarySerializer { Options = defaultYuzuCommonOptions };
				} else if (format == Format.JSON) {
					ys = new Yuzu.Json.JsonSerializer {
						Options = defaultYuzuCommonOptions,
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
					d = new GeneratedDeserializersBIN.BinaryDeserializerGen { Options = defaultYuzuCommonOptions };
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
				jd.Generate<Font>();
				jd.Generate<SerializableSample>();
				jd.Generate<KerningPair>();
				jd.Generate<FontChar>();
				jd.Generate<SerializableFont>();
				jd.Generate<Mesh3D.BlendIndices>();
				jd.Generate<Mesh3D.BlendWeights>();
				jd.Generate<Mesh3D.Vertex>();
				jd.Generate<Mesh<Mesh3D.Vertex>>();
				jd.Generate<TextureAtlasElement.Params>();
				jd.Generate<BitSet32>();
				jd.Generate<BoundingSphere>();
				jd.Generate<Color4>();
				jd.Generate<IntRectangle>();
				jd.Generate<IntVector2>();
				jd.Generate<Matrix32>();
				jd.Generate<Matrix44>();
				jd.Generate<NumericRange>();
				jd.Generate<Plane>();
				jd.Generate<Quaternion>();
				jd.Generate<Ray>();
				jd.Generate<Rectangle>();
				jd.Generate<Size>();
				jd.Generate<Vector2>();
				jd.Generate<Vector3>();
				jd.Generate<Vector4>();
				jd.Generate<Camera3D>();
				jd.Generate<CommonMaterial>();
				jd.Generate<Mesh3D>();
				jd.Generate<Submesh3D>();
				jd.Generate<Node3D>();
				jd.Generate<Spline3D>();
				jd.Generate<SplinePoint3D>();
				jd.Generate<Viewport3D>();
				jd.Generate<Animation>();
				jd.Generate<Spline>();
				jd.Generate<LinearLayout>();

				jd.Generate<Animator<string>>();
				jd.Generate<Animator<int>>();
				jd.Generate<Animator<bool>>();
				jd.Generate<Animator<Blending>>();
				jd.Generate<Animator<ITexture>>();
				jd.Generate<Animator<NumericRange>>();
				jd.Generate<Animator<Vector2>>();
				jd.Generate<Animator<Color4>>();
				jd.Generate<Animator<float>>();
				jd.Generate<Animator<EmitterShape>>();
				jd.Generate<Animator<AudioAction>>();
				jd.Generate<Animator<SerializableSample>>();
				jd.Generate<Animator<HAlignment>>();
				jd.Generate<Animator<VAlignment>>();
				jd.Generate<Animator<MovieAction>>();
				jd.Generate<Animator<ShaderId>>();
				jd.Generate<Animator<Vector3>>();
				jd.Generate<Animator<Quaternion>>();
				jd.Generate<Animator<EmissionType>>();
				jd.Generate<NumericAnimator>();
				jd.Generate<Vector2Animator>();
				jd.Generate<Color4Animator>();
				jd.Generate<QuaternionAnimator>();
				jd.Generate<Vector3Animator>();
				jd.Generate<Matrix44Animator>();

				jd.Generate<Keyframe<string>>();
				jd.Generate<Keyframe<int>>();
				jd.Generate<Keyframe<bool>>();
				jd.Generate<Keyframe<Blending>>();
				jd.Generate<Keyframe<ITexture>>();
				jd.Generate<Keyframe<NumericRange>>();
				jd.Generate<Keyframe<Vector2>>();
				jd.Generate<Keyframe<Color4>>();
				jd.Generate<Keyframe<float>>();
				jd.Generate<Keyframe<EmitterShape>>();
				jd.Generate<Keyframe<AudioAction>>();
				jd.Generate<Keyframe<SerializableSample>>();
				jd.Generate<Keyframe<HAlignment>>();
				jd.Generate<Keyframe<VAlignment>>();
				jd.Generate<Keyframe<MovieAction>>();
				jd.Generate<Keyframe<ShaderId>>();
				jd.Generate<Keyframe<Vector3>>();
				jd.Generate<Keyframe<Quaternion>>();
				jd.Generate<Keyframe<EmissionType>>();
				jd.Generate<Keyframe<Matrix44>>();

				jd.Generate<Audio>();
				jd.Generate<BoneWeight>();
				jd.Generate<SkinningWeights>();
				jd.Generate<Bone>();
				jd.Generate<BoneArray>();
				jd.Generate<BoneArray.Entry>();
				jd.Generate<Button>();
				jd.Generate<DistortionMesh>();
				jd.Generate<DistortionMeshPoint>();
				jd.Generate<Frame>();
				jd.Generate<Image>();
				jd.Generate<TiledImage>();
				jd.Generate<ImageCombiner>();
				jd.Generate<Marker>();
				jd.Generate<Movie>();
				jd.Generate<NineGrid>();
				jd.Generate<ParticleEmitter>();
				jd.Generate<ParticleModifier>();
				jd.Generate<ParticlesMagnet>();
				jd.Generate<PointObject>();
				jd.Generate<Slider>();
				jd.Generate<SplineGear>();
				jd.Generate<SplinePoint>();
				jd.Generate<RichText>();
				jd.Generate<SimpleText>();
				jd.Generate<TextStyle>();
				jd.Generate<Widget>();
				jd.Generate<SerializableTexture>();

				jd.Generate<NodeReference<Widget>>();
				jd.Generate<NodeReference<Spline>>();
				jd.Generate<NodeReference<Camera3D>>();

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
