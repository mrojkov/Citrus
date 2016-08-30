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

		static Stack<string> SerializationPathStack {
			get { return SerializationStackKeeper.stack ?? (SerializationStackKeeper.stack = new Stack<string>()); }
		}

		public static string ShrinkPath(string path)
		{
			if (SerializationPathStack.Count == 0) {
				return path;
			}
			return string.IsNullOrEmpty(path) ? path : '/' + path;
		}

		public static string ExpandPath(string path)
		{
			if (SerializationPathStack.Count == 0) {
				return path;
			}
			string result;
			if (string.IsNullOrEmpty(path))
				return path;
			else if (path[0] == '/')
				result = path.Substring(1);
			else {
				string p = SerializationPathStack.Peek();
				result = Path.Combine(Path.GetDirectoryName(p), path).Replace('\\', '/');
			}
			return result;
		}

		private static JsonSerializeOptions defaultYuzuJSONOptions = new JsonSerializeOptions {
			ArrayLengthPrefix = true,
			Indent = "\t",
			FieldSeparator = "\n",
			SaveRootClass = true
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
					ys = new Yuzu.Binary.BinarySerializer();
				} else if (format == Format.JSON) {
					ys = new Yuzu.Json.JsonSerializer {
						JsonOptions = defaultYuzuJSONOptions,
					};
				}
				ys.ToStream(instance, stream);
			} finally {
				SerializationPathStack.Pop();
			}
		}

		public static void WriteObjectToFile<T>(string path, T instance, Format format)
		{
			using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
				WriteObject(path, stream, instance, format);
		}

		public static void WriteObjectToBundle<T>(AssetsBundle bundle, string path, T instance, Format format, AssetAttributes attributes = AssetAttributes.None)
		{
			using (MemoryStream stream = new MemoryStream()) {
				WriteObject(path, stream, instance, format);
				stream.Seek(0, SeekOrigin.Begin);
				bundle.ImportFile(path, stream, 0, attributes);
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
				Yuzu.Deserializer.AbstractReaderDeserializer yd = null;
				if (CheckYuzuBinarySignature(stream)) {
					yd = new GeneratedDeserializersBIN.BinaryDeserializerGen();
				} else {
					yd = typeof(T) == typeof(Frame)
						? new GeneratedDeserializersJSON.Lime.Frame_JsonDeserializer()
						: typeof(T) == typeof(Node)
							? new GeneratedDeserializersJSON.Lime.Node_JsonDeserializer()
							: new Yuzu.Json.JsonDeserializer();
				}
				var bd = yd as BinaryDeserializer;
				if (obj == null) {
					if (bd != null) {
						return bd.FromReader<T>(new BinaryReader(stream));
					} else {
						return (T)yd.FromStream(stream);
					}
				} else {
					if (bd != null) {
						return (T)bd.FromReader(obj, new BinaryReader(stream));
					} else {
						return (T)yd.FromStream(obj, stream);
					}
				}
			} finally {
				SerializationPathStack.Pop();
			}
		}

		public static T ReadObject<T>(string path, object obj = null) where T : new()
		{
			using (Stream stream = AssetsBundle.Instance.OpenFileLocalized(path))
				return ReadObject<T>(path, stream, obj);
		}

		public static T ReadObjectFromFile<T>(string path, object obj = null) where T : new()
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

		public static void GenerateDeserializers(string filename, List<Type> types, Format format)
		{
			switch (format) {
				case Format.Binary: {
					var yjdg = new BinaryDeserializerGenerator("GeneratedDeserializersBIN");
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
					} break;
				case Format.JSON: {
					var ybdg = new JsonDeserializerGenerator("GeneratedDeserializersJSON");
					ybdg.JsonOptions = defaultYuzuJSONOptions;
						using (var ms = new MemoryStream())
						using (var sw = new StreamWriter(ms)) {
							ybdg.GenWriter = sw;
							ybdg.GenerateHeader();
							foreach (var generate in types
								.Select(t => ybdg.GetType()
								.GetMethod("Generate")
								.MakeGenericMethod(t))) {
								generate.Invoke(ybdg, new object[] { });
							}
							ybdg.GenerateFooter();
							sw.Flush();
							ms.WriteTo(new FileStream(filename, FileMode.Create));
						}
				} break;
				default:
					throw new NotImplementedException();
			}
		}

		public static void GenerateDeserializers()
		{
			var jd = new JsonDeserializerGenerator("GeneratedDeserializersJSON");
			jd.JsonOptions = defaultYuzuJSONOptions;
			using (var ms = new MemoryStream())
			using (var sw = new StreamWriter(ms)) {
				jd.GenWriter = sw;
				jd.GenerateHeader();
				jd.Generate<Font>();
				jd.Generate<SerializableSample>();
				jd.Generate<KerningPair>();
				jd.Generate<FontChar>();
				jd.Generate<ITexture>();
				jd.Generate<SerializableFont>();
				jd.Generate<BlendIndices>();
				jd.Generate<BlendWeights>();
				jd.Generate<GeometryBuffer>();
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
				jd.Generate<Material>();
				jd.Generate<Mesh3D>();
				jd.Generate<Submesh3D>();
				jd.Generate<Node3D>();
				jd.Generate<Spline3D>();
				jd.Generate<Spline3D.Point>();
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
				jd.Generate<ImageCombiner>();
				jd.Generate<Marker>();
				jd.Generate<Movie>();
				jd.Generate<NineGrid>();
				jd.Generate<Node>();
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
				jd.Generate<IAnimator>();
				jd.Generate<SerializableTexture>();
				jd.GenerateFooter();
				sw.Flush();
				ms.WriteTo(new FileStream(@"..\..\..\..\Lime\Source\GeneratedDeserializersJSON.cs", FileMode.Create));
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
				jd.Generate<BlendIndices>();
				jd.Generate<BlendWeights>();
				jd.Generate<GeometryBuffer>();
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
				jd.Generate<Material>();
				jd.Generate<Mesh3D>();
				jd.Generate<Submesh3D>();
				jd.Generate<Node3D>();
				jd.Generate<Spline3D>();
				jd.Generate<Spline3D.Point>();
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
				jd.Generate<ImageCombiner>();
				jd.Generate<Marker>();
				jd.Generate<Movie>();
				jd.Generate<NineGrid>();
				jd.Generate<Node>();
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
				jd.GenerateFooter();
				sw.Flush();
				ms.WriteTo(new FileStream(@"..\..\..\..\Lime\Source\GeneratedDeserializersBIN.cs", FileMode.Create));
			}
		}
	}
}
