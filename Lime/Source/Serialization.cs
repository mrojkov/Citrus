using System;
using System.Collections.Generic;
using System.IO;
using Yuzu.Json;

namespace Lime
{
	public static class Serialization
	{
		const float iPadDeserializationSpeed = 1024 * 1024;

		public enum OperationType
		{
			Clone,
			Serialization
		}

		public struct Operation
		{
			public OperationType Type;
			public string SerializationPath;
		}

		public interface IDeserializer
		{
			object Deserialize(Stream stream, object value, Type type);
		}

		public class ProtoBufDeserializer : IDeserializer
		{
			readonly ProtoBuf.Meta.TypeModel typeModel;

			public ProtoBufDeserializer(ProtoBuf.Meta.TypeModel typeModel)
			{
				this.typeModel = typeModel;
			}

			public object Deserialize(Stream stream, object value, Type type)
			{
				return typeModel.Deserialize(stream, value, type);
			}
		}

#if iOS || ANDROID
		public static ProtoBuf.Meta.TypeModel Serializer = null;
#else
		public static ProtoBuf.Meta.TypeModel ProtoBufTypeModel = CreateProtoBufTypeModel();
#endif
#if !iOS && !ANDROID
		public static IDeserializer Deserializer = new ProtoBufDeserializer(ProtoBufTypeModel);
		public static ProtoBuf.Meta.RuntimeTypeModel CreateProtoBufTypeModel()
		{
			var model = ProtoBuf.Meta.RuntimeTypeModel.Create();
			model.UseImplicitZeroDefaults = false;
			// Add ITexture type here due a bug in ProtoBuf-Net
			model.Add(typeof(ITexture), true);
			model.Add(typeof(SurrogateBitmap), false).Add("SerializationData");
			model.Add(typeof(Bitmap), false).SetSurrogate(typeof(SurrogateBitmap));
			model.CompileInPlace();
			return model;
		}
#endif
		static class OperationStackKeeper
		{
			[ThreadStatic]
			public static Stack<Operation> stack;
		}

		static Stack<Operation> OperationStack {
			get { return OperationStackKeeper.stack ?? (OperationStackKeeper.stack = new Stack<Operation>()); }
		}

		public static string ShrinkPath(string path)
		{
			if (OperationStack.Count == 0) {
				return path;
			}
			if (OperationStack.Peek().Type == OperationType.Clone)
				return path;
			return string.IsNullOrEmpty(path) ? path : '/' + path;
		}

		public static string ExpandPath(string path)
		{
			if (OperationStack.Count == 0) {
				return path;
			}
			if (OperationStack.Peek().Type == OperationType.Clone)
				return path;
			string result;
			if (string.IsNullOrEmpty(path))
				return path;
			else if (path[0] == '/')
				result = path.Substring(1);
			else {
				string p = OperationStack.Peek().SerializationPath;
				result = Path.GetDirectoryName(p) + '/' + path;
			}
			return result;
		}

		public static void WriteObject<T>(string path, Stream stream, T instance)
		{
			OperationStack.Push(new Operation { SerializationPath = path, Type = OperationType.Serialization });
			try {
#if iOS || ANDROID
				Serializer.Serialize(stream, instance);
#else
				ProtoBufTypeModel.Serialize(stream, instance);
#endif
			} finally {
				OperationStack.Pop();
			}
		}

		public static void WriteObjectToFile<T>(string path, T instance)
		{
			using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
				WriteObject(path, stream, instance);
		}

		public static void WriteObjectToBundle<T>(AssetsBundle bundle, string path, T instance, AssetAttributes attributes = AssetAttributes.None)
		{
			using (MemoryStream stream = new MemoryStream()) {
				WriteObject(path, stream, instance);
				stream.Seek(0, SeekOrigin.Begin);
				bundle.ImportFile(path, stream, 0, attributes);
			}
		}

		public static T DeepClone<T>(T obj)
		{
			OperationStack.Push(new Operation { Type = OperationType.Clone });
			try {
#if iOS || ANDROID
				return (T)Serializer.DeepClone(obj);
#else
				return (T)ProtoBufTypeModel.DeepClone(obj);
#endif
			} finally {
				OperationStack.Pop();
			}
		}

		public static T ReadObject<T>(string path, Stream stream, object obj = null)
		{
			OperationStack.Push(new Operation { SerializationPath = path, Type = OperationType.Serialization });
			try {
#if iOS || ANDROID
				return (T)Serializer.Deserialize(stream, obj, typeof(T));
#else
				return (T)Deserializer.Deserialize(stream, obj, typeof(T));
#endif
			} finally {
				OperationStack.Pop();
			}
		}

		public static T ReadObject<T>(string path, object obj = null)
		{
			using (Stream stream = AssetsBundle.Instance.OpenFileLocalized(path))
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
				WriteObject(path, memStream, obj);
				memStream.Flush();
				int checkSum = Toolbox.ComputeHash(memStream.GetBuffer(), (int)memStream.Length);
				return checkSum;
			}
		}

		public static Operation GetCurrentOperation()
		{
			return OperationStack.Peek();
		}

		public static Yuzu.Json.JsonSerializeOptions JSONOptions()
		{
			return new JsonSerializeOptions {
				ArrayLengthPrefix = true,
				Indent = "\t",
				FieldSeparator = "\n",
			};
		}

		public static void GenerateDeserializers()
		{
			var jd = new JsonDeserializerGenerator("GeneratedDeserializersJSON");
			jd.JsonOptions = JSONOptions();
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
	}
}
