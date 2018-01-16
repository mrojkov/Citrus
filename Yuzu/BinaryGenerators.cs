using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Yuzu.Metadata;
using Yuzu.Util;

namespace Yuzu.Binary
{

	public class BinaryDeserializerGenBase: BinaryDeserializer
	{
		protected static Dictionary<Type, Action<BinaryDeserializer, ReaderClassDef, object>> readCache =
			new Dictionary<Type, Action<BinaryDeserializer, ReaderClassDef, object>>();
		protected static Dictionary<Type, Func<BinaryDeserializer, ReaderClassDef, object>> makeCache =
			new Dictionary<Type, Func<BinaryDeserializer, ReaderClassDef, object>>();

		protected override void PrepareReaders(ReaderClassDef def)
		{
			base.PrepareReaders(def);
			Action<BinaryDeserializer, ReaderClassDef, object> r;
			if (readCache.TryGetValue(def.Meta.Type, out r))
				def.ReadFields = r;
			Func<BinaryDeserializer, ReaderClassDef, object> m;
			if (makeCache.TryGetValue(def.Meta.Type, out m))
				def.Make = m;
		}

	}

	public class BinaryDeserializerGenerator : IDeserializerGenerator
	{
		private CodeWriter cw = new CodeWriter();
		private string wrapperNameSpace;
		private CommonOptions options;
		private Dictionary<Type, string> generatedReaders = new Dictionary<Type, string>();
		private Dictionary<Type, string> generatedMakers = new Dictionary<Type, string>();
		private string classDefName = typeof(ReaderClassDef).Name;

		public StreamWriter GenWriter
		{
			get { return cw.Output; }
			set { cw.Output = value; }
		}

		// Turn off for 5% speedup in exchange for potentially missing broken data.
		public bool SafetyChecks = true;

		public BinaryDeserializerGenerator(string wrapperNameSpace = "YuzuGenBin", CommonOptions? options = null)
		{
			this.wrapperNameSpace = wrapperNameSpace;
			this.options = options.HasValue ? options.Value : new CommonOptions();
		}

		static BinaryDeserializerGenerator() { InitSimpleValueReader(); }

		public void GenerateHeader()
		{
			cw.Put("using System;\n");
			cw.Put("using System.Reflection;\n");
			cw.Put("\n");
			cw.Put("using Yuzu;\n");
			cw.Put("using Yuzu.Binary;\n");
			cw.Put("\n");
			cw.Put("namespace {0}\n", wrapperNameSpace);
			cw.Put("{\n");
			cw.Put("public class BinaryDeserializerGen: BinaryDeserializerGenBase\n");
			cw.Put("{\n");
		}

		public void GenerateFooter()
		{
			cw.Put("static BinaryDeserializerGen()\n");
			cw.Put("{\n");
			foreach (var r in generatedReaders)
				cw.Put("readCache[typeof({0})] = {1};\n", Utils.GetTypeSpec(r.Key), r.Value);
			foreach (var r in generatedMakers)
				cw.Put("makeCache[typeof({0})] = {1};\n", Utils.GetTypeSpec(r.Key), r.Value);
			cw.Put("}\n");
			cw.Put("}\n"); // Close class.
			cw.Put("}\n"); // Close namespace.
		}

		private static Dictionary<Type, string> simpleValueReader = new Dictionary<Type, string>();

		private static void InitSimpleValueReader()
		{
			simpleValueReader[typeof(sbyte)] = "d.Reader.ReadSByte()";
			simpleValueReader[typeof(byte)] = "d.Reader.ReadByte()";
			simpleValueReader[typeof(short)] = "d.Reader.ReadInt16()";
			simpleValueReader[typeof(ushort)] = "d.Reader.ReadUInt16()";
			simpleValueReader[typeof(int)] = "d.Reader.ReadInt32()";
			simpleValueReader[typeof(uint)] = "d.Reader.ReadUInt32()";
			simpleValueReader[typeof(long)] = "d.Reader.ReadInt64()";
			simpleValueReader[typeof(ulong)] = "d.Reader.ReadUInt64()";
			simpleValueReader[typeof(bool)] = "d.Reader.ReadBoolean()";
			simpleValueReader[typeof(char)] = "d.Reader.ReadChar()";
			simpleValueReader[typeof(float)] = "d.Reader.ReadSingle()";
			simpleValueReader[typeof(double)] = "d.Reader.ReadDouble()";
			simpleValueReader[typeof(decimal)] = "d.Reader.ReadDecimal()";
			simpleValueReader[typeof(DateTime)] = "DateTime.FromBinary(d.Reader.ReadInt64())";
			simpleValueReader[typeof(TimeSpan)] = "new TimeSpan(d.Reader.ReadInt64())";
			simpleValueReader[typeof(object)] = "dg.ReadAny()";
		}

		private string PutCount()
		{
			var tempCountName = cw.GetTempName();
			cw.Put("var {0} = d.Reader.ReadInt32();\n", tempCountName);
			cw.Put("if ({0} >= 0) {{\n", tempCountName);
			return tempCountName;
		}

		private string PutNullOrCount(Type t)
		{
			cw.PutPart("({0})null;\n", Utils.GetTypeSpec(t));
			return PutCount();
		}

		private void GenerateCollection(Type t, Type icoll, string name, string tempIndexName)
		{
			cw.Put("while (--{0} >= 0) {{\n", tempIndexName);
			var tempElementName = cw.GetTempName();
			cw.Put("var {0} = ", tempElementName);
			GenerateValue(icoll.GetGenericArguments()[0], tempElementName);
			cw.PutAddToColllection(t, icoll, name, tempElementName);
			cw.Put("}\n"); // while
		}

		private void GenerateDictionary(Type t, string name, string tempIndexName)
		{
			cw.Put("while (--{0} >= 0) {{\n", tempIndexName);
			var tempKeyName = cw.GetTempName();
			cw.Put("var {0} = ", tempKeyName);
			GenerateValue(t.GetGenericArguments()[0], tempKeyName);
			var tempValueName = cw.GetTempName();
			cw.Put("var {0} = ", tempValueName);
			GenerateValue(t.GetGenericArguments()[1], tempValueName);
			cw.Put("{0}.Add({1}, {2});\n", name, tempKeyName, tempValueName);
			cw.Put("}\n"); // while
		}

		private string MaybeUnchecked() { return SafetyChecks ? "" : "Unchecked"; }

		private void GenerateValue(Type t, string name)
		{
			string sr;
			if (simpleValueReader.TryGetValue(t, out sr)) {
				cw.PutPart(sr + ";\n");
				return;
			}
			if (t == typeof(string)) {
				cw.PutPart("d.Reader.ReadString();\n");
				cw.Put("if ({0} == \"\" && d.Reader.ReadBoolean()) {0} = null;\n", name);
				return;
			}
			if (t.IsEnum) {
				cw.PutPart("({0})d.Reader.ReadInt32();\n", Utils.GetTypeSpec(t));
				return;
			}
			if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
				var tempIndexName = PutNullOrCount(t);
				cw.Put("{0} = new {1}();\n", name, Utils.GetTypeSpec(t));
				GenerateDictionary(t, name, tempIndexName);
				cw.Put("}\n");
				return;
			}
			if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)) {
				var arg = t.GetGenericArguments()[0];
				cw.PutPart("d.Reader.ReadBoolean() ? ({0}?)null : ", Utils.GetTypeSpec(arg));
				GenerateValue(arg, name);
				return;
			}
			if (t.IsArray) {
				var tempIndexName = PutNullOrCount(t);
				var tempArrayName = cw.GetTempName();
				cw.Put("var {0} = new {1};\n", tempArrayName, Utils.GetTypeSpec(t, arraySize: tempIndexName));
				cw.Put("for({0} = 0; {0} < {1}.Length; ++{0}) {{\n", tempIndexName, tempArrayName);
				GenerateSetValue(t.GetElementType(), String.Format("{0}[{1}]", tempArrayName, tempIndexName), null);
				cw.Put("}\n");
				cw.Put("{0} = {1};\n", name, tempArrayName);
				cw.Put("}\n"); // if >= 0
				return;
			}
			var icoll = Utils.GetICollection(t);
			if (icoll != null) {
				var tempIndexName = PutNullOrCount(t);
				cw.Put("{0} = new {1}();\n", name, Utils.GetTypeSpec(t));
				GenerateCollection(t, icoll, name, tempIndexName);
				cw.Put("}\n");
				return;
			}
			if (t.IsClass || t.IsInterface) {
				cw.PutPart("({0})dg.ReadObject{1}<{0}>();\n", Utils.GetTypeSpec(t), MaybeUnchecked());
				return;
			}
			if (Utils.IsStruct(t)) {
				cw.PutPart("({0})dg.ReadStruct{1}<{0}>();\n", Utils.GetTypeSpec(t), MaybeUnchecked());
				return;
			}
			throw new NotImplementedException();
		}

		private void GenerateSetValue(Type t, string name, Meta.Item item)
		{
			var canInline =
				!t.IsGenericType && Utils.IsStruct(t) &&
				item != null && item.PropInfo == null &&
				!simpleValueReader.ContainsKey(t);
			if (canInline) {
				var meta = Meta.Get(t, options);
				if (meta.IsCompact && meta.Surrogate.FuncFrom == null && meta.AfterDeserialization.Actions.Count == 0) {
					cw.Put("dg.EnsureClassDef(typeof({0}));\n", Utils.GetTypeSpec(t));
					foreach (var yi in meta.Items)
						GenerateSetValue(yi.Type, name + "." + yi.Name, yi);
				}
				else
					cw.Put("dg.ReadIntoStruct(ref {0});\n", name);
				return;
			}
			cw.Put("{0} = ", name);
			GenerateValue(t, name);
		}

		private void GenerateMerge(Type t, string name)
		{
			if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
				GenerateDictionary(t, name, PutCount());
				cw.Put("}\n");
				return;
			}
			var icoll = Utils.GetICollection(t);
			if (icoll != null) {
				GenerateCollection(t, icoll, name, PutCount());
				cw.Put("}\n");
				return;
			}
			if ((t.IsClass || t.IsInterface) && t != typeof(object)) {
				cw.Put("dg.ReadIntoObject{0}<{1}>({2});\n", MaybeUnchecked(), Utils.GetTypeSpec(t), name);
				return;
			}
			throw new YuzuException(String.Format("Unable to merge field {1} of type {0}", name, t.Name));
		}

		private bool IsDeserializerGenRequired(Meta meta)
		{
			if (!meta.IsCompact) return true;
			foreach (var yi in meta.Items)
			{
				if (yi.Type == typeof(object)) return true;
				if (simpleValueReader.ContainsKey(yi.Type) || yi.Type.IsEnum || yi.Type == typeof(string))
					continue;
				// TODO: Containers.
				return true;
			}
			return false;
		}

		private void GenerateReaderBody(Meta meta)
		{
			cw.ResetTempNames();
			if (IsDeserializerGenRequired(meta))
				cw.Put("var dg = (BinaryDeserializerGen)d;\n", Utils.GetTypeSpec(meta.Type));
			if (meta.IsCompact) {
				foreach (var yi in meta.Items)
					GenerateSetValue(yi.Type, "result." + yi.Name, yi);
			}
			else {
				cw.Put("{0}.FieldDef fd;\n", classDefName);
				var ourIndex = 0;
				cw.Put("fd = def.Fields[d.Reader.ReadInt16()];\n");
				foreach (var yi in meta.Items) {
					ourIndex += 1;
					if (yi.IsOptional)
						cw.Put("if ({0} == fd.OurIndex) {{\n", ourIndex);
					else if (SafetyChecks)
						cw.Put("if ({0} != fd.OurIndex) throw dg.Error(\"{0}!=\" + fd.OurIndex);\n", ourIndex);
					if (yi.SetValue != null)
						GenerateSetValue(yi.Type, "result." + yi.Name, yi);
					else
						GenerateMerge(yi.Type, "result." + yi.Name);
					cw.Put("fd = def.Fields[d.Reader.ReadInt16()];\n");
					if (yi.IsOptional)
						cw.Put("}\n");
				}
				if (SafetyChecks)
					cw.Put("if (fd.OurIndex != {0}.EOF) throw dg.Error(\"Unfinished object\");\n", classDefName);
			}
			cw.GenerateActionList(meta.AfterDeserialization);
		}

		private string GetMangledTypeNameNS(Type t)
		{
			return t.Namespace.Replace('.', '_') + "__" + Utils.GetMangledTypeName(t);
		}

		public void Generate<T>() { Generate(typeof(T)); }

		public void Generate(Type t)
		{
			if (t.IsInterface)
				throw new YuzuException("Useless BinaryGenerator for interface " + t.Name);
			if (t.IsAbstract)
				throw new YuzuException("Useless BinaryGenerator for abstract class " + t.Name);

			var meta = Meta.Get(t, options);

			var readerName = "Read_" + GetMangledTypeNameNS(t);
			if (!Utils.IsStruct(t)) {
				cw.Put("private static void {0}(BinaryDeserializer d, {1} def, object obj)\n", readerName, classDefName);
				cw.Put("{\n");
				cw.Put("var result = ({0})obj;\n", Utils.GetTypeSpec(t));
				GenerateReaderBody(meta);
				cw.Put("}\n");
				cw.Put("\n");
				generatedReaders[t] = readerName;
			}

			var makerName = "Make_" + GetMangledTypeNameNS(t);
			cw.Put("private static object {0}(BinaryDeserializer d, {1} def)\n", makerName, classDefName);
			cw.Put("{\n");
			cw.Put("var result = new {0}();\n", Utils.GetTypeSpec(t));
			if (Utils.IsStruct(t))
				GenerateReaderBody(meta);
			else
				cw.Put("{0}(d, def, result);\n", readerName);
			cw.Put("return result;\n");
			cw.Put("}\n");
			cw.Put("\n");
			generatedMakers[t] = makerName;
		}

	}
}
