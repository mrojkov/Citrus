using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Yuzu.Metadata;
using Yuzu.Util;

namespace Yuzu.Binary
{
	public class BinaryDeserializerGenerator
	{
		public StreamWriter GenWriter;

		private int indent = 0;
		private string indentStr = "\t";
		private string wrapperNameSpace;
		private CommonOptions options;
		private Dictionary<Type, string> generatedReaders = new Dictionary<Type, string>();
		private Dictionary<Type, string> generatedMakers = new Dictionary<Type, string>();

		public BinaryDeserializerGenerator(string wrapperNameSpace = "YuzuGenBin", CommonOptions options = null)
		{
			this.wrapperNameSpace = wrapperNameSpace;
			this.options = options ?? new CommonOptions();
			InitSimpleValueReader();
		}

		private void PutPart(string format, params object []p)
		{
			var s = p.Length > 0 ? String.Format(format, p) : format;
			GenWriter.Write(s.Replace("\n", "\r\n"));
		}

		private void Put(string s)
		{
			if (s.StartsWith("}")) // "}\n" or "} while"
				indent -= 1;
			if (s != "\n")
				for (int i = 0; i < indent; ++i)
					PutPart(indentStr);
			PutPart(s);
			if (s.EndsWith("{\n"))
				indent += 1;
		}

		private void PutF(string format, params object[] p)
		{
			Put(String.Format(format, p));
		}

		public void GenerateHeader()
		{
			Put("using System;\n");
			Put("using System.Reflection;\n");
			Put("\n");
			Put("using Yuzu;\n");
			Put("using Yuzu.Binary;\n");
			Put("\n");
			PutF("namespace {0}\n", wrapperNameSpace);
			Put("{\n");
			Put("public class BinaryDeserializerGen: BinaryDeserializer\n");
			Put("{\n");
		}

		public void GenerateFooter()
		{
			Put("public BinaryDeserializerGen()\n");
			Put("{\n");
			foreach (var r in generatedReaders)
				PutF("readFieldsCache[typeof({0})] = {1};\n", Utils.GetTypeSpec(r.Key), r.Value);
			foreach (var r in generatedMakers)
				PutF("makeCache[typeof({0})] = {1};\n", Utils.GetTypeSpec(r.Key), r.Value);
			Put("}\n");
			Put("}\n"); // Close class.
			Put("}\n"); // Close namespace.
		}

		private void GenerateAfterDeserialization(Meta meta)
		{
			foreach (var a in meta.AfterDeserialization)
				PutF("result.{0}();\n", a.Info.Name);
		}

		private Dictionary<Type, string> simpleValueReader = new Dictionary<Type, string>();

		private void InitSimpleValueReader()
		{
			simpleValueReader[typeof(sbyte)] = "Reader.ReadSByte()";
			simpleValueReader[typeof(byte)] = "Reader.ReadByte()";
			simpleValueReader[typeof(short)] = "Reader.ReadInt16()";
			simpleValueReader[typeof(ushort)] = "Reader.ReadUInt16()";
			simpleValueReader[typeof(int)] = "Reader.ReadInt32()";
			simpleValueReader[typeof(uint)] = "Reader.ReadUInt32()";
			simpleValueReader[typeof(long)] = "Reader.ReadInt64()";
			simpleValueReader[typeof(ulong)] = "Reader.ReadUInt64()";
			simpleValueReader[typeof(bool)] = "Reader.ReadBoolean()";
			simpleValueReader[typeof(char)] = "Reader.ReadChar()";
			simpleValueReader[typeof(float)] = "Reader.ReadSingle()";
			simpleValueReader[typeof(double)] = "Reader.ReadDouble()";
			simpleValueReader[typeof(DateTime)] = "DateTime.FromBinary(Reader.ReadInt64())";
			simpleValueReader[typeof(TimeSpan)] = "new TimeSpan(Reader.ReadInt64())";
			simpleValueReader[typeof(object)] = "ReadAny()";
		}

		private int tempCount = 0;
		private string GetTempName()
		{
			tempCount += 1;
			return "tmp" + tempCount.ToString();
		}

		private string PutNullOrCount(Type t)
		{
			PutPart("({0})null;\n", Utils.GetTypeSpec(t));
			var tempCountName = GetTempName();
			PutF("var {0} = Reader.ReadInt32();\n", tempCountName);
			PutF("if ({0} >= 0) {{\n", tempCountName);
			return tempCountName;
		}

		private void GenerateCollection(Type t, Type icoll, string name, string tempIndexName)
		{
			PutF("while (--{0} >= 0) {{\n", tempIndexName);
			var tempElementName = GetTempName();
			PutF("var {0} = ", tempElementName);
			GenerateValue(icoll.GetGenericArguments()[0], tempElementName);
			// Check for explicit vs implicit interface implementation.
			var imap = t.GetInterfaceMap(icoll);
			var addIndex = Array.FindIndex(imap.InterfaceMethods, m => m.Name == "Add");
			if (imap.TargetMethods[addIndex].Name == "Add")
				PutF("{0}.Add({1});\n", name, tempElementName);
			else
				PutF("(({2}){0}).Add({1});\n", name, tempElementName, Utils.GetTypeSpec(icoll));
			Put("}\n"); // while
		}

		private void GenerateDictionary(Type t, string name, string tempIndexName)
		{
			PutF("while (--{0} >= 0) {{\n", tempIndexName);
			var tempKeyName = GetTempName();
			PutF("var {0} = ", tempKeyName);
			GenerateValue(t.GetGenericArguments()[0], tempKeyName);
			var tempValueName = GetTempName();
			PutF("var {0} = ", tempValueName);
			GenerateValue(t.GetGenericArguments()[1], tempValueName);
			PutF("{0}.Add({1}, {2});\n", name, tempKeyName, tempValueName);
			Put("}\n"); // while
		}

		private void GenerateValue(Type t, string name)
		{
			string sr;
			if (simpleValueReader.TryGetValue(t, out sr)) {
				PutPart(sr + ";\n");
				return;
			}
			if (t == typeof(string)) {
				PutPart("Reader.ReadString();\n");
				PutF("if ({0} == \"\" && Reader.ReadBoolean()) {0} = null;\n", name);
				return;
			}
			if (t.IsEnum) {
				PutPart("({0})Reader.ReadInt32();\n", Utils.GetTypeSpec(t));
				return;
			}
			if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
				var tempIndexName = PutNullOrCount(t);
				PutF("{0} = new {1}();\n", name, Utils.GetTypeSpec(t));
				GenerateDictionary(t, name, tempIndexName);
				Put("}\n");
				return;
			}
			if (t.IsArray) {
				var tempIndexName = PutNullOrCount(t);
				var tempArrayName = GetTempName();
				PutF("var {0} = new {1}[{2}];\n", tempArrayName, Utils.GetTypeSpec(t.GetElementType()), tempIndexName);
				PutF("for({0} = 0; {0} < {1}.Length; ++{0}) {{\n", tempIndexName, tempArrayName);
				PutF("{0}[{1}] = ", tempArrayName, tempIndexName);
				GenerateValue(t.GetElementType(), String.Format("{0}[{1}]", tempArrayName, tempIndexName));
				Put("}\n");
				PutF("{0} = {1};\n", name, tempArrayName);
				Put("}\n"); // if >= 0
				return;
			}
			var icoll = t.GetInterface(typeof(ICollection<>).Name);
			if (icoll != null) {
				var tempIndexName = PutNullOrCount(t);
				PutF("{0} = new {1}();\n", name, Utils.GetTypeSpec(t));
				GenerateCollection(t, icoll, name, tempIndexName);
				Put("}\n");
				return;
			}
			if (t.IsClass || t.IsInterface) {
				PutPart("({0})ReadObject<{0}>();\n", Utils.GetTypeSpec(t));
				return;
			}
			if (Utils.IsStruct(t)) {
				PutPart("({0})ReadStruct<{0}>();\n", Utils.GetTypeSpec(t));
				return;
			}
			throw new NotImplementedException();
		}

		private void GenerateMerge(Type t, string name)
		{

		}

		private void GenerateReaderBody(Meta meta)
		{
			tempCount = 0;
			if (meta.IsCompact) {
				foreach (var yi in meta.Items) {
					PutF("result.{0} = ", yi.Name);
					GenerateValue(yi.Type, "result." + yi.Name);
				}
			}
			else {
				PutF("ClassDef.FieldDef fd;\n");
				var ourIndex = 0;
				PutF("fd = def.Fields[Reader.ReadInt16()];\n");
				foreach (var yi in meta.Items) {
					ourIndex += 1;
					if (yi.IsOptional) {
						PutF("if ({0} == fd.OurIndex) {{\n", ourIndex);
						if (yi.SetValue != null)
							PutF("result.{0} = ", yi.Name);
					}
					else {
						PutF("if ({0} != fd.OurIndex) throw Error(\"{0}!=\" + fd.OurIndex);\n", ourIndex);
						if (yi.SetValue != null)
							PutF("result.{0} = ", yi.Name);
					}
					if (yi.SetValue != null)
						GenerateValue(yi.Type, "result." + yi.Name);
					else
						GenerateMerge(yi.Type, "result." + yi.Name);
					PutF("fd = def.Fields[Reader.ReadInt16()];\n");
					if (yi.IsOptional)
						Put("}\n");
				}
				PutF("if (fd.OurIndex != ClassDef.EOF) throw Error(\"Unfinished object\");\n");
			}
			GenerateAfterDeserialization(meta);
		}

		public void Generate<T>()
		{
			var meta = Meta.Get(typeof(T), options);

			var readerName = "Read_" + Utils.GetMangledTypeName(typeof(T));
			if (!Utils.IsStruct(typeof(T))) {
				PutF("private void {0}(ClassDef def, object obj)\n", readerName);
				Put("{\n");
				PutF("var result = ({0})obj;\n", Utils.GetTypeSpec(typeof(T)));
				GenerateReaderBody(meta);
				Put("}\n");
				Put("\n");
				generatedReaders[typeof(T)] = readerName;
			}

			var makerName = "Make_" + Utils.GetMangledTypeName(typeof(T));
			PutF("private object {0}(ClassDef def)\n", makerName);
			Put("{\n");
			PutF("var result = new {0}();\n", Utils.GetTypeSpec(typeof(T)));
			if (Utils.IsStruct(typeof(T)))
				GenerateReaderBody(meta);
			else
				PutF("{0}(def, result);\n", readerName);
			Put("return result;\n");
			Put("}\n");
			Put("\n");
			generatedMakers[typeof(T)] = makerName;
		}

	}
}
