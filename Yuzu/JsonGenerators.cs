using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Yuzu
{
	public abstract class JsonDeserializerGenBase : JsonDeserializer
	{
		public abstract object FromReaderIntPartial(string name);

		public override object FromReaderInt()
		{
			return FromReaderIntGenerated();
		}
	}

	public class JsonDeserializerGenerator : JsonDeserializer
	{
		public static new JsonDeserializerGenerator Instance = new JsonDeserializerGenerator();

		private int indent = 0;
		public StreamWriter GenWriter;

		public JsonDeserializerGenerator()
		{
			Options.Assembly = Assembly.GetCallingAssembly();
		}

		private void PutPart(string s)
		{
			GenWriter.Write(s.Replace("\n", "\r\n"));
		}

		private void Put(string s)
		{
			if (s.StartsWith("}")) // "}\n" or "} while"
				indent -= 1;
			if (s != "\n")
				for (int i = 0; i < indent; ++i)
					PutPart(JsonOptions.Indent);
			PutPart(s);
			if (s.EndsWith("{\n"))
				indent += 1;
		}

		private void PutF(string format, params object[] p)
		{
			Put(String.Format(format, p));
		}

		public void GenerateHeader(string namespaceName)
		{
			Put("using System;\n");
			Put("using System.Collections.Generic;\n");
			Put("using System.Reflection;\n");
			Put("\n");
			Put("using Yuzu;\n");
			Put("\n");
			PutF("namespace {0}\n", namespaceName);
			Put("{\n");
			Put("\n");
		}

		public void GenerateFooter()
		{
			Put("}\n");
		}

		private int tempCount = 0;

		private string GetTypeSpec(Type t)
		{
			return t.IsGenericType ?
				String.Format("{0}<{1}>",
					t.Name.Remove(t.Name.IndexOf('`')),
					String.Join(",", t.GetGenericArguments().Select(GetTypeSpec))) :
				t.Name;
		}

		private void PutRequireOrNull(char ch, Type t, string name)
		{
			PutPart(String.Format("RequireOrNull('{0}') ? null : new {1}();\n", ch, GetTypeSpec(t)));
			PutF("if ({0} != null) {{\n", name);
		}

		private void PutRequireOrNullArray(char ch, Type t, string name)
		{
			PutPart(String.Format("RequireOrNull('{0}') ? null : new {1}[0];\n", ch, GetTypeSpec(t.GetElementType())));
			PutF("if ({0} != null) {{\n", name);
		}

		private void GenerateValue(Type t, string name)
		{
			if (t == typeof(int)) {
				PutPart("RequireInt();\n");
			}
			else if (t == typeof(uint)) {
				PutPart("RequireUInt();\n");
			}
			else if (t == typeof(long)) {
				PutPart("RequireLong();\n");
			}
			else if (t == typeof(ulong)) {
				PutPart("RequireULong();\n");
			}
			else if (t == typeof(short)) {
				PutPart("(short)RequireInt();\n");
			}
			else if (t == typeof(ushort)) {
				PutPart("(ushort)RequireUInt();\n");
			}
			else if (t == typeof(sbyte)) {
				PutPart("(sbyte)RequireInt();\n");
			}
			else if (t == typeof(byte)) {
				PutPart("(byte)RequireUInt();\n");
			}
			else if (t == typeof(char)) {
				PutPart("RequireChar();\n");
			}
			else if (t == typeof(string)) {
				PutPart("RequireString();\n");
			}
			else if (t == typeof(bool)) {
				PutPart("RequireBool();\n");
			}
			else if (t == typeof(float)) {
				PutPart("RequireSingle();\n");
			}
			else if (t == typeof(double)) {
				PutPart("RequireDouble();\n");
			}
			else if (t == typeof(DateTime)) {
				PutPart("RequireDateTime();\n");
			}
			else if (t == typeof(TimeSpan)) {
				PutPart("RequireTimeSpan();\n");
			}
			else if (t.IsEnum) {
				PutPart(String.Format(
					JsonOptions.EnumAsString ?
						"({0})Enum.Parse(typeof({0}), RequireString());\n" :
						"({0})RequireInt();\n",
					t.Name));
			}
			else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)) {
				PutRequireOrNull('[', t, name);
				Put("if (SkipSpacesCarefully() == ']') {\n");
				Put("Require(']');\n");
				Put("}\n");
				Put("else {\n");
				Put("do {\n");
				tempCount += 1;
				var tempName = "tmp" + tempCount.ToString();
				PutF("var {0} = ", tempName);
				GenerateValue(t.GetGenericArguments()[0], tempName);
				PutF("{0}.Add({1});\n", name, tempName);
				Put("} while (Require(']', ',') == ',');\n");
				Put("}\n");
				Put("}\n");
			}
			else if (
				t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>)
			) {
				PutRequireOrNull('{', t, name);
				Put("if (SkipSpacesCarefully() == '}') {\n");
				Put("Require('}');\n");
				Put("}\n");
				Put("else {\n");
				Put("do {\n");
				tempCount += 1;
				var tempKeyStr = "tmp" + tempCount.ToString();
				PutF("var {0} = RequireString();\n", tempKeyStr);
				Put("Require(':');\n");
				tempCount += 1;
				var tempValue = "tmp" + tempCount.ToString();
				PutF("var {0} = ", tempValue);
				GenerateValue(t.GetGenericArguments()[1], tempValue);
				var keyType = t.GetGenericArguments()[0];
				var tempKey =
					keyType == typeof(string) ? tempKeyStr :
					keyType == typeof(int) ? String.Format("int.Parse({0})", tempKeyStr) :
					keyType.IsEnum ?
						String.Format("({0})Enum.Parse(typeof({0}), {1})", GetTypeSpec(keyType), tempKeyStr) :
						// Slow.
						String.Format("({0})keyParsers[typeof({0})]({1})", GetTypeSpec(keyType), tempKeyStr);
				PutF("{0}.Add({1}, {2});\n", name, tempKey, tempValue);
				Put("} while (Require('}', ',') == ',');\n");
				Put("}\n");
				Put("}\n");
			}
			else if (t.IsArray && !JsonOptions.ArrayLengthPrefix) {
				PutRequireOrNullArray('[', t, name);
				Put("if (SkipSpacesCarefully() == ']') {\n");
				Put("Require(']');\n");
				Put("}\n");
				Put("else {\n");
				tempCount += 1;
				var tempListName = "tmp" + tempCount.ToString();
				PutF("var {0} = new List<{1}>();\n", tempListName, GetTypeSpec(t.GetElementType()));
				Put("do {\n");
				tempCount += 1;
				var tempName = "tmp" + tempCount.ToString();
				PutF("var {0} = ", tempName);
				GenerateValue(t.GetElementType(), tempName);
				PutF("{0}.Add({1});\n", tempListName, tempName);
				Put("} while (Require(']', ',') == ',');\n");
				PutF("{0} = {1}.ToArray();\n", name, tempListName);
				Put("}\n");
				Put("}\n");
			}
			else if (t.IsArray && JsonOptions.ArrayLengthPrefix) {
				PutRequireOrNullArray('[', t, name);
				Put("if (SkipSpacesCarefully() != ']') {\n");
				tempCount += 1;
				var tempArrayName = "tmp" + tempCount.ToString();
				PutF("var {0} = new {1}[RequireUInt()];\n", tempArrayName, GetTypeSpec(t.GetElementType()));
				tempCount += 1;
				var tempIndexName = "tmp" + tempCount.ToString();
				PutF("for(int {0} = 0; {0} < {1}.Length; ++{0}) {{\n", tempIndexName, tempArrayName);
				Put("Require(',');\n");
				PutF("{0}[{1}] = ", tempArrayName, tempIndexName);
				GenerateValue(t.GetElementType(), String.Format("{0}[{1}]", tempArrayName, tempIndexName));
				Put("}\n");
				PutF("{0} = {1};\n", name, tempArrayName);
				Put("}\n");
				Put("Require(']');\n");
				Put("}\n");
			}
			else if (t.IsClass && Options.ClassNames) {
				PutPart(String.Format("({0})base.FromReaderInt();\n", t.Name));
			}
			else if (t.IsClass && !Options.ClassNames || Utils.IsStruct(t)) {
				PutPart(String.Format("({0}){0}_JsonDeserializer.Instance.FromReader(new {0}(), Reader);\n", t.Name));
			}
			else {
				throw new NotImplementedException(t.Name);
			}
		}

		private void GenAssigns(string name, object obj)
		{
			foreach (var m in obj.GetType().GetMembers()) {
				if (m.MemberType == MemberTypes.Field) {
					var f = (FieldInfo)m;
					var v = Utils.CodeValueFormat(f.GetValue(obj));
					if (v != "") // TODO
						PutF("{0}.{1} = {2};\n", name, f.Name, v);
				}
				else if (m.MemberType == MemberTypes.Property) {
					var p = (PropertyInfo)m;
					if (p.CanWrite) {
						var v = Utils.CodeValueFormat(p.GetValue(obj));
						if (v != "") // TODO
							PutF("{0}.{1} = {2};\n", name, p.Name, v);
					}
				}
			}
		}

		public void Generate<T>()
		{
			PutF("class {0}_JsonDeserializer : JsonDeserializerGenBase\n", typeof(T).Name);
			Put("{\n");

			PutF("public static new {0}_JsonDeserializer Instance = new {0}_JsonDeserializer();\n", typeof(T).Name);
			Put("\n");

			PutF("public {0}_JsonDeserializer()\n", typeof(T).Name);
			Put("{\n");
			PutF("Options.Assembly = Assembly.Load(\"{0}\");\n", typeof(T).Assembly.FullName);
			GenAssigns("Options", Options);
			GenAssigns("JsonOptions", JsonOptions);
			Put("}\n");
			Put("\n");

			Put("public override object FromReaderInt()\n");
			Put("{\n");
			// Since deserializer is dynamically constructed anyway, it is too late to determine object type here.
			PutF("return FromReaderInt(new {0}());\n", typeof(T).Name);
			Put("}\n");
			Put("\n");

			Put("public override object FromReaderIntPartial(string name)\n");
			Put("{\n");
			PutF("return ReadFields(new {0}(), name);\n", typeof(T).Name);
			Put("}\n");
			Put("\n");

			Put("protected override object ReadFields(object obj, string name)\n");
			Put("{\n");
			PutF("var result = ({0})obj;\n", typeof(T).Name);
			tempCount = 0;
			foreach (var yi in Meta.Get(typeof(T), Options).Items) {
				if (yi.IsOptional) {
					PutF("if (\"{0}\" == name) {{\n", yi.Tag(Options));
					PutF("result.{0} = ", yi.Name);
				}
				else {
					PutF("if (\"{0}\" != name) throw new YuzuException(\"{0}!=\" + name);\n", yi.Tag(Options));
					PutF("result.{0} = ", yi.Name);
				}
				GenerateValue(yi.Type, "result." + yi.Name);
				Put("name = GetNextName(false);\n");
				if (yi.IsOptional)
					Put("}\n");
			}
			Put("Require('}');\n");
			Put("return result;\n");
			Put("}\n");

			if (Utils.IsCompact(typeof(T), Options)) {
				Put("\n");
				Put("protected override object ReadFieldsCompact(object obj)\n");
				Put("{\n");
				PutF("var result = ({0})obj;\n", typeof(T).Name);
				bool isFirst = true;
				tempCount = 0;
				foreach (var yi in Meta.Get(typeof(T), Options).Items) {
					if (!isFirst)
						Put("Require(',');\n");
					isFirst = false;
					PutF("result.{0} = ", yi.Name);
					GenerateValue(yi.Type, "result." + yi.Name);
				}
				Put("Require(']');\n");
				Put("return result;\n");
				Put("}\n");
			}
			Put("}\n");
			Put("\n");
		}

		public override object FromReaderInt()
		{
			return FromReaderIntGenerated();
		}

		public override object FromReaderInt(object obj)
		{
			return FromReaderIntGenerated(obj);
		}
	}
}
