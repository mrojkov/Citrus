using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Yuzu.Metadata;
using Yuzu.Util;

namespace Yuzu.Json
{
	public abstract class JsonDeserializerGenBase : JsonDeserializer
	{
		public abstract object FromReaderIntPartial(string name);

		private string DeclaringTypes(Type t, string separator)
		{
			return t.DeclaringType == null ? "" :
				DeclaringTypes(t.DeclaringType, separator) + t.DeclaringType.Name + separator;
		}

		protected string GetTypeSpec(Type t)
		{
			var p = "global::" + t.Namespace + ".";
			var n = DeclaringTypes(t, ".") + t.Name;
			if (!t.IsGenericType)
				return p + n;
			var args = String.Join(",", t.GetGenericArguments().Select(a => GetTypeSpec(a)));
			return p + String.Format("{0}<{1}>", n.Remove(n.IndexOf('`')), args);
		}

		protected string GetMangledTypeName(Type t)
		{
			var n = DeclaringTypes(t, "__") + t.Name;
			if (!t.IsGenericType)
				return n;
			var args = String.Join("__", t.GetGenericArguments().Select(a => GetMangledTypeName(a)));
			return n.Remove(n.IndexOf('`')) + "_" + args;
		}

		protected virtual string GetWrapperNamespace()
		{
			var ns = GetType().Namespace;
			var i = ns.IndexOf('.');
			return i < 0 ? ns : ns.Remove(i);
		}

		protected string GetDeserializerName(Type t)
		{
			return GetWrapperNamespace() + "." + t.Namespace + "." + GetMangledTypeName(t) + "_JsonDeserializer";
		}

		private static Dictionary<string, JsonDeserializerGenBase> deserializerCache =
			new Dictionary<string, JsonDeserializerGenBase>();

		protected JsonDeserializerGenBase MakeDeserializer(string className)
		{
			JsonDeserializerGenBase result;
			if (!deserializerCache.TryGetValue(className, out result)) {
				var t = Options.Assembly.GetType(className);
				if (t == null)
					throw Error("Unknown type '{0}'", className);
				var dt = Options.Assembly.GetType(GetDeserializerName(t));
				if (dt == null)
					throw Error("Generated deserializer not found for type '{0}'", className);
				result = (JsonDeserializerGenBase)Activator.CreateInstance(dt);
				deserializerCache[className] = result;
			}
			result.Reader = Reader;
			return result;
		}

		protected object FromReaderIntGenerated()
		{
			KillBuf();
			Require('{');
			CheckClassTag(GetNextName(first: true));
			var d = MakeDeserializer(RequireUnescapedString());
			Require(',');
			return d.FromReaderIntPartial(GetNextName(first: false));
		}

		protected object FromReaderIntGenerated(object obj)
		{
			KillBuf();
			Require('{');
			var expectedType = obj.GetType();
			string name = GetNextName(first: true);
			if (name == JsonOptions.ClassTag) {
				var typeName = RequireUnescapedString();
				var actualType = Options.Assembly.GetType(typeName);
				if (actualType == null)
					throw Error("Unknown type '{0}'", typeName);
				if (actualType != expectedType)
					throw Error("Expected type '{0}', but got {1}", expectedType.Name, typeName);
				name = GetNextName(first: false);
			}
			return MakeDeserializer(obj.GetType().FullName).ReadFields(obj, name);
		}

		public override object FromReaderInt()
		{
			return FromReaderIntGenerated();
		}

		public T FromReaderTyped<T>(BinaryReader reader) where T: new()
		{
			Reader = reader;
			KillBuf();
			var ch = RequireBracketOrNull();
			if (ch == 'n') return default(T);
			if (ch == '[') return (T)ReadFieldsCompact(new T());
			var name = GetNextName(true);
			if (name != JsonOptions.ClassTag) return (T)ReadFields(new T(), name);
			var typeName = RequireUnescapedString();
			return (T)MakeDeserializer(typeName).FromReaderIntPartial(GetNextName(false));
		}

		public T FromReaderInterface<T>(BinaryReader reader) where T : class
		{
			Reader = reader;
			KillBuf();
			var ch = Require('{', 'n');
			if (ch == 'n') {
				Require("ull");
				return null;
			}
			CheckClassTag(GetNextName(first: true));
			var typeName = RequireUnescapedString();
			return (T)MakeDeserializer(typeName).FromReaderIntPartial(GetNextName(first: false));
		}
	}

	public class JsonDeserializerGenerator : JsonDeserializerGenBase
	{
		public static new JsonDeserializerGenerator Instance = new JsonDeserializerGenerator();

		private int indent = 0;
		public StreamWriter GenWriter;

		private string wrapperNameSpace = "";
		private string lastNameSpace = "";

		public JsonDeserializerGenerator(string wrapperNameSpace = "YuzuGen")
		{
			Options.Assembly = Assembly.GetCallingAssembly();
			this.wrapperNameSpace = wrapperNameSpace;
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

		public void GenerateHeader()
		{
			Put("using System;\n");
			Put("using System.Collections.Generic;\n");
			Put("using System.IO;\n");
			Put("using System.Reflection;\n");
			Put("\n");
			Put("using Yuzu;\n");
			Put("using Yuzu.Json;\n");
			Put("\n");
		}

		public void GenerateFooter()
		{
			Put("}\n"); // Close namespace.
		}

		private int tempCount = 0;

		private void PutRequireOrNull(char ch, Type t, string name)
		{
			PutPart(String.Format("RequireOrNull('{0}') ? null : new {1}();\n", ch, GetTypeSpec(t)));
			PutF("if ({0} != null) {{\n", name);
		}

		private void PutRequireOrNullArray(char ch, Type t, string name)
		{
			PutPart(String.Format(
				"RequireOrNull('{0}') ? null : new {1}[0];\n", ch, GetTypeSpec(t.GetElementType())));
			PutF("if ({0} != null) {{\n", name);
		}

		private void GenerateCollection(Type t, Type icoll, string name)
		{
			Put("if (SkipSpacesCarefully() == ']') {\n");
			Put("Require(']');\n");
			Put("}\n");
			Put("else {\n");
			Put("do {\n");
			tempCount += 1;
			var tempName = "tmp" + tempCount.ToString();
			PutF("var {0} = ", tempName);
			GenerateValue(icoll.GetGenericArguments()[0], tempName);
			// Check for explicit vs implicit interface implementation.
			var imap = t.GetInterfaceMap(icoll);
			var addIndex = Array.FindIndex(imap.InterfaceMethods, m => m.Name == "Add");
			if (imap.TargetMethods[addIndex].Name == "Add")
				PutF("{0}.Add({1});\n", name, tempName);
			else
				PutF("(({2}){0}).Add({1});\n", name, tempName, GetTypeSpec(icoll));
			Put("} while (Require(']', ',') == ',');\n");
			Put("}\n");
		}

		private void GenerateMerge(Type t, string name)
		{
			var icoll = t.GetInterface(typeof(ICollection<>).Name);
			if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
				Put("Require('{');\n");
				GenerateDictionary(t, name);
			}
			else if (icoll != null) {
				Put("Require('[');\n");
				GenerateCollection(t, icoll, name);
			}
			else if ((t.IsClass || t.IsInterface) && t != typeof(object))
				PutF(String.Format("{0}.Instance.FromReader({1}, Reader);\n", GetDeserializerName(t), name));
			else
				throw Error("Unable to merge field {1} of type {0}", name, t.Name);
		}

		private void GenerateDictionary(Type t, string name)
		{
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
		}

		private void GenerateValue(Type t, string name)
		{
			var icoll = t.GetInterface(typeof(ICollection<>).Name);
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
				PutPart("checked((short)RequireInt());\n");
			}
			else if (t == typeof(ushort)) {
				PutPart("checked((ushort)RequireUInt());\n");
			}
			else if (t == typeof(sbyte)) {
				PutPart("checked((sbyte)RequireInt());\n");
			}
			else if (t == typeof(byte)) {
				PutPart("checked((byte)RequireUInt());\n");
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
					GetTypeSpec(t)));
			}
			else if(t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
				PutRequireOrNull('{', t, name);
				GenerateDictionary(t, name);
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
			else if (icoll != null) {
				PutRequireOrNull('[', t, name);
				GenerateCollection(t, icoll, name);
				Put("}\n");
			}
			else if (t.IsClass && !t.IsAbstract || Utils.IsStruct(t))
				PutPart(String.Format(
					"{0}.Instance.FromReaderTyped<{1}>(Reader);\n", GetDeserializerName(t), GetTypeSpec(t)));
			else if (t.IsInterface || t.IsAbstract)
				PutPart(String.Format(
					"{0}.Instance.FromReaderInterface<{1}>(Reader);\n", GetDeserializerName(t), GetTypeSpec(t)));
			else
				throw new NotImplementedException(t.Name);
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
						var v = Utils.CodeValueFormat(p.GetValue(obj, new object[] { }));
						if (v != "") // TODO
							PutF("{0}.{1} = {2};\n", name, p.Name, v);
					}
				}
			}
		}

		private void GenerateAfterDeserialization(Meta meta)
		{
			foreach (var a in meta.AfterDeserialization)
				PutF("result.{0}();\n", a.Info.Name);
		}

		public void Generate<T>()
		{
			var meta = Meta.Get(typeof(T), Options);

			if (lastNameSpace != typeof(T).Namespace) {
				if (lastNameSpace != "")
					Put("}\n");
				Put("\n");
				lastNameSpace = typeof(T).Namespace;
				PutF("namespace {0}.{1}\n", wrapperNameSpace, lastNameSpace);
				Put("{\n");
			}

			var deserializerName = GetMangledTypeName(typeof(T)) + "_JsonDeserializer";
			PutF("class {0} : JsonDeserializerGenBase\n", deserializerName);
			Put("{\n");

			PutF("public static new {0} Instance = new {0}();\n", deserializerName);
			Put("\n");

			PutF("public {0}()\n", deserializerName);
			Put("{\n");
			PutF("Options.Assembly = Assembly.Load(\"{0}\");\n", typeof(T).Assembly.FullName);
			GenAssigns("Options", Options);
			GenAssigns("JsonOptions", JsonOptions);
			Put("}\n");
			Put("\n");

			var icoll = typeof(T).GetInterface(typeof(ICollection<>).Name);
			var typeSpec = GetTypeSpec(typeof(T));
			Put("public override object FromReaderInt()\n");
			Put("{\n");
			if (icoll != null)
				PutF("return FromReaderInt(new {0}());\n", typeSpec);
			else if (typeof(T).IsInterface || typeof(T).IsAbstract)
				PutF("return FromReaderInterface<{0}>(Reader);\n", typeSpec);
			else
				PutF("return FromReaderTyped<{0}>(Reader);\n", typeSpec);
			Put("}\n");
			Put("\n");

			if (icoll != null) {
				Put("public override object FromReaderInt(object obj)\n");
				Put("{\n");
				PutF("var result = ({0})obj;\n", typeSpec);
				Put("Require('[');\n");
				GenerateCollection(typeof(T), icoll, "result");
				Put("return result;\n");
				Put("}\n");
				Put("\n");
			}

			Put("public override object FromReaderIntPartial(string name)\n");
			Put("{\n");
			if (typeof(T).IsInterface || typeof(T).IsAbstract)
				Put("return null;\n");
			else
				PutF("return ReadFields(new {0}(), name);\n", typeSpec);
			Put("}\n");
			Put("\n");

			Put("protected override object ReadFields(object obj, string name)\n");
			Put("{\n");
			PutF("var result = ({0})obj;\n", typeSpec);
			if (icoll == null) {
				tempCount = 0;
				foreach (var yi in meta.Items) {
					if (yi.IsOptional) {
						PutF("if (\"{0}\" == name) {{\n", yi.Tag(Options));
						if (yi.SetValue != null)
							PutF("result.{0} = ", yi.Name);
					}
					else {
						PutF("if (\"{0}\" != name) throw new YuzuException(\"{0}!=\" + name);\n", yi.Tag(Options));
						if (yi.SetValue != null)
							PutF("result.{0} = ", yi.Name);
					}
					if (yi.SetValue != null)
						GenerateValue(yi.Type, "result." + yi.Name);
					else
						GenerateMerge(yi.Type, "result." + yi.Name);
					Put("name = GetNextName(false);\n");
					if (yi.IsOptional)
						Put("}\n");
				}
				Put("Require('}');\n");
				GenerateAfterDeserialization(meta);
			}
			Put("return result;\n");
			Put("}\n");

			if (meta.IsCompact) {
				Put("\n");
				Put("protected override object ReadFieldsCompact(object obj)\n");
				Put("{\n");
				PutF("var result = ({0})obj;\n", typeSpec);
				bool isFirst = true;
				tempCount = 0;
				foreach (var yi in meta.Items) {
					if (!isFirst)
						Put("Require(',');\n");
					isFirst = false;
					if (yi.SetValue != null) {
						PutF("result.{0} = ", yi.Name);
						GenerateValue(yi.Type, "result." + yi.Name);
					}
					else
						GenerateMerge(yi.Type, "result." + yi.Name);
				}
				Put("Require(']');\n");
				GenerateAfterDeserialization(meta);
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

		public override object FromReaderIntPartial(string name)
		{
			throw new NotSupportedException();
		}

		protected override string GetWrapperNamespace()
		{
			return wrapperNameSpace;
		}

	}
}
