using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Yuzu;
using Yuzu.Binary;
using Yuzu.Clone;
using Yuzu.Json;

namespace Lime
{
	public class Persistence
	{
		public enum Format
		{
			Json,
			Binary
		}

		public delegate AbstractDeserializer DeserializerBuilder(string path, Stream stream);

		private static Action<List<DeserializerBuilder>> ExtendDeserializers;

		private readonly List<DeserializerBuilder> deserializerBuilders = new List<DeserializerBuilder>();

		public static Func<Persistence, AbstractCloner> ClonerFactory = persistence => new YuzuGenerated.LimeCloner {
			Options = persistence.YuzuCommonOptions,
		};

		public uint BinarySignature = 0xdeadbabe;

		public Persistence()
		{
			deserializerBuilders.Add(
				(path, stream) => CheckBinarySignature(stream)
					? new YuzuGenerated.LimeDeserializer { Options = YuzuCommonOptions }
					: null
			);
			deserializerBuilders.Add(
				(path, stream) => new global::Yuzu.Json.JsonDeserializer {
					JsonOptions = YuzuJsonOptions,
					Options = YuzuCommonOptions,
				}
			);
			ExtendDeserializers?.Invoke(this.deserializerBuilders);
		}

		public Persistence(CommonOptions yuzuCommonOptions, JsonSerializeOptions yuzuJsonOptions) : this()
		{
			YuzuCommonOptions = yuzuCommonOptions;
			YuzuJsonOptions = yuzuJsonOptions;
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

		public virtual void WriteObject<T>(string path, Stream stream, T instance, Format format)
		{
			AbstractWriterSerializer ys = null;
			if (format == Format.Binary) {
				WriteYuzuBinarySignature(stream);
				ys = new global::Yuzu.Binary.BinarySerializer { Options = YuzuCommonOptions };
			} else if (format == Format.Json) {
				ys = new global::Yuzu.Json.JsonSerializer {
					Options = YuzuCommonOptions,
					JsonOptions = YuzuJsonOptions
				};
			}
			ys.ToStream(instance, stream);
		}

		public virtual void WriteObject<T>(string path, Stream stream, T instance, AbstractSerializer serializer)
		{
			if (serializer is BinarySerializer) {
				WriteYuzuBinarySignature(stream);
			}
			serializer.ToStream(instance, stream);
		}

		public void WriteObjectToFile<T>(string path, T instance, Format format)
		{
			using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
				WriteObject(path, stream, instance, format);
		}

		public void WriteObjectToBundle<T>(AssetBundle bundle, string path, T instance, Format format, string sourceExtension, DateTime time, AssetAttributes attributes, byte[] cookingRulesSHA1)
		{
			using (MemoryStream stream = new MemoryStream()) {
				WriteObject(path, stream, instance, format);
				stream.Seek(0, SeekOrigin.Begin);
				bundle.ImportFile(path, stream, 0, sourceExtension, time, attributes, cookingRulesSHA1);
			}
		}

		public virtual T ReadObject<T>(string path, Stream stream, object obj = null)
		{
			if (!(stream is MemoryStream)) {
				var ms = new MemoryStream();
				stream.CopyTo(ms);
				ms.Seek(0, SeekOrigin.Begin);
				stream = ms;
			}
			AbstractDeserializer d = null;
			foreach (var db in deserializerBuilders) {
				d = db(path, stream);
				if (d != null) {
					break;
				}
			}
			return obj == null
				? d.FromStream<T>(stream)
				: (T)d.FromStream(obj, stream);
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
				WriteObject(path, memStream, obj, Format.Binary);
				memStream.Flush();
				int checkSum = Toolbox.ComputeHash(memStream.GetBuffer(), (int)memStream.Length);
				return checkSum;
			}
		}

		private void WriteYuzuBinarySignature(Stream s)
		{
			var bw = new BinaryWriter(s);
			bw.Write(BinarySignature);
		}

		public bool CheckBinarySignature(Stream s)
		{
			UInt32 signature;
			try {
				s.Seek(0, SeekOrigin.Begin);
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

		private AbstractCloner cloner;

		/// <summary>
		/// Clone object using serialization scheme.
		/// </summary>
		/// <param name="obj">A source object to clone.</param>
		/// <returns></returns>
		public object Clone(object obj)
		{
			if (cloner == null) {
				cloner = ClonerFactory(this);
			}
			return cloner.DeepObject(obj);
		}

		/// <summary>
		/// Clone object using serialization scheme.
		/// </summary>
		/// <typeparam name="T">A type of object that need to be returned.</typeparam>
		/// <param name="obj">A source object to clone.</param>
		/// <returns></returns>
		public T Clone<T>(T obj) => (T)Clone((object)obj);
	}
}
