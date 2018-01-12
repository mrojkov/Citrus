using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Yuzu.Metadata;
using Yuzu.Util;

namespace Yuzu.Json
{
	public abstract class JsonDeserializerGenBase : JsonDeserializer
	{
		public abstract object FromReaderIntPartial(string name);

		public Assembly Assembly;

		protected virtual string GetWrapperNamespace()
		{
			var ns = GetType().Namespace;
			var i = ns.IndexOf('.');
			return i < 0 ? ns : ns.Remove(i);
		}

		protected string GetDeserializerName(Type t)
		{
			return GetWrapperNamespace() + "." + t.Namespace + "." + Utils.GetMangledTypeName(t) + "_JsonDeserializer";
		}

		private static Dictionary<string, JsonDeserializerGenBase> deserializerCache =
			new Dictionary<string, JsonDeserializerGenBase>();

		private JsonDeserializerGenBase MakeDeserializer(string className)
		{
			JsonDeserializerGenBase result;
			if (!deserializerCache.TryGetValue(className, out result)) {
				var t = FindType(className);
				var dt = TypeSerializer.Deserialize(GetDeserializerName(t) + ", " + (Assembly ?? GetType().Assembly).FullName);
				if (dt == null)
					throw Error("Generated deserializer not found for type '{0}'", className);
				result = (JsonDeserializerGenBase)Activator.CreateInstance(dt);
				deserializerCache[className] = result;
			}
			result.Reader = Reader;
			return result;
		}

		private object MaybeReadObject(string className, string name)
		{
			return name == "" ?
				Activator.CreateInstance(FindType(className)) :
				MakeDeserializer(className).FromReaderIntPartial(name);
		}

		private object FromReaderIntGenerated()
		{
			KillBuf();
			Require('{');
			CheckClassTag(GetNextName(first: true));
			var typeName = RequireUnescapedString();
			return MaybeReadObject(typeName, GetNextName(first: false));
		}

		protected object FromReaderIntGenerated(object obj)
		{
			KillBuf();
			Require('{');
			var expectedType = obj.GetType();
			string name = GetNextName(first: true);
			if (name == JsonOptions.ClassTag) {
				CheckExpectedType(RequireUnescapedString(), expectedType);
				name = GetNextName(first: false);
			}
			return name == "" ? obj :
				MakeDeserializer(TypeSerializer.Serialize(obj.GetType())).ReadFields(obj, name);
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
			return (T)MaybeReadObject(typeName, GetNextName(first: false));
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
			return (T)MaybeReadObject(typeName, GetNextName(first: false));
		}
	}

	public class JsonDeserializerGenerator : JsonDeserializerGenBase, IDeserializerGenerator
	{
		public static new JsonDeserializerGenerator Instance = new JsonDeserializerGenerator();

		private CodeWriter cw = new CodeWriter();
		private string wrapperNameSpace = "";
		private string lastNameSpace = "";

		public StreamWriter GenWriter
		{
			get { return cw.Output; }
			set { cw.Output = value; }
		}

		public JsonDeserializerGenerator(string wrapperNameSpace = "YuzuGen")
		{
			this.wrapperNameSpace = wrapperNameSpace;
		}

		static JsonDeserializerGenerator() { InitSimpleValueReader(); }

		public void GenerateHeader()
		{
			if (Options.AllowUnknownFields || JsonOptions.Unordered)
				throw new NotImplementedException();
			cw.Put("using System;\n");
			cw.Put("using System.Collections.Generic;\n");
			cw.Put("\n");
			cw.Put("using Yuzu;\n");
			cw.Put("using Yuzu.Json;\n");
		}

		public void GenerateFooter()
		{
			cw.Put("}\n"); // Close namespace.
		}

		private void PutRequireOrNull(char ch, Type t, string name)
		{
			cw.PutPart("RequireOrNull('{0}') ? null : new {1}();\n", ch, Utils.GetTypeSpec(t));
			cw.Put("if ({0} != null) {{\n", name);
		}

		private void PutRequireOrNullArray(char ch, Type t, string name)
		{
			cw.PutPart("RequireOrNull('{0}') ? null : new {1};\n", ch, Utils.GetTypeSpec(t, arraySize: "0"));
			cw.Put("if ({0} != null) {{\n", name);
		}

		private void GenerateCollection(Type t, Type icoll, string name)
		{
			cw.Put("if (SkipSpacesCarefully() == ']') {\n");
			cw.Put("Require(']');\n");
			cw.Put("}\n");
			cw.Put("else {\n");
			cw.Put("do {\n");
			var tempElementName = cw.GetTempName();
			cw.Put("var {0} = ", tempElementName);
			GenerateValue(icoll.GetGenericArguments()[0], tempElementName);
			cw.PutAddToColllection(t, icoll, name, tempElementName);
			cw.Put("} while (Require(']', ',') == ',');\n");
			cw.Put("}\n");
		}

		private void GenerateMerge(Type t, string name)
		{
			var icoll = Utils.GetICollection(t);
			if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
				cw.Put("Require('{');\n");
				GenerateDictionary(t, name);
			}
			else if (icoll != null) {
				cw.Put("Require('[');\n");
				GenerateCollection(t, icoll, name);
			}
			else if ((t.IsClass || t.IsInterface) && t != typeof(object))
				cw.Put("{0}.Instance.FromReader({1}, Reader);\n", GetDeserializerName(t), name);
			else
				throw Error("Unable to merge field {1} of type {0}", name, t.Name);
		}

		private void GenerateDictionary(Type t, string name)
		{
			cw.Put("if (SkipSpacesCarefully() == '}') {\n");
			cw.Put("Require('}');\n");
			cw.Put("}\n");
			cw.Put("else {\n");
			cw.Put("do {\n");
			var tempKeyStr = cw.GetTempName();
			cw.Put("var {0} = RequireString();\n", tempKeyStr);
			cw.Put("Require(':');\n");
			var tempValue = cw.GetTempName();
			cw.Put("var {0} = ", tempValue);
			GenerateValue(t.GetGenericArguments()[1], tempValue);
			var keyType = t.GetGenericArguments()[0];
			var tempKey =
				keyType == typeof(string) ? tempKeyStr :
				keyType == typeof(int) ? String.Format("int.Parse({0})", tempKeyStr) :
				keyType.IsEnum ?
					String.Format("({0})Enum.Parse(typeof({0}), {1})", Utils.GetTypeSpec(keyType), tempKeyStr) :
				// Slow.
					String.Format("({0})keyParsers[typeof({0})]({1})", Utils.GetTypeSpec(keyType), tempKeyStr);
			cw.Put("{0}.Add({1}, {2});\n", name, tempKey, tempValue);
			cw.Put("} while (Require('}', ',') == ',');\n");
			cw.Put("}\n");
		}

		private static Dictionary<Type, string> simpleValueReader = new Dictionary<Type, string>();

		private static void InitSimpleValueReader()
		{
			simpleValueReader[typeof(sbyte)] = "checked((sbyte)RequireInt())";
			simpleValueReader[typeof(byte)] = "checked((byte)RequireUInt())";
			simpleValueReader[typeof(short)] = "checked((short)RequireInt())";
			simpleValueReader[typeof(ushort)] = "checked((ushort)RequireUInt())";
			simpleValueReader[typeof(int)] = "RequireInt()";
			simpleValueReader[typeof(uint)] = "RequireUInt()";
			simpleValueReader[typeof(long)] = "RequireLong()";
			simpleValueReader[typeof(ulong)] = "RequireULong()";
			simpleValueReader[typeof(bool)] = "RequireBool()";
			simpleValueReader[typeof(char)] = "RequireChar()";
			simpleValueReader[typeof(float)] = "RequireSingle()";
			simpleValueReader[typeof(double)] = "RequireDouble()";
			simpleValueReader[typeof(DateTime)] = "RequireDateTime()";
			simpleValueReader[typeof(TimeSpan)] = "RequireTimeSpan()";
			simpleValueReader[typeof(string)] = "RequireString()";
			simpleValueReader[typeof(object)] = "ReadAnyObject()";
		}

		private void GenerateValue(Type t, string name)
		{
			string sr;
			if (simpleValueReader.TryGetValue(t, out sr)) {
				cw.PutPart(sr + ";\n");
				return;
			}
			if (t == typeof(decimal)) {
				cw.PutPart(JsonOptions.DecimalAsString ? "RequireDecimalAsString();\n" : "RequireDecimal();\n");
				return;
			}
			if (t.IsEnum) {
				cw.PutPart(
					JsonOptions.EnumAsString ?
						"({0})Enum.Parse(typeof({0}), RequireString());\n" :
						"({0})RequireInt();\n",
					Utils.GetTypeSpec(t));
				return;
			}
			if(t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
				PutRequireOrNull('{', t, name);
				GenerateDictionary(t, name);
				cw.Put("}\n");
				return;
			}
			if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)) {
				cw.PutPart("null;\n");
				cw.Put("if (SkipSpacesCarefully() == 'n') {\n");
				cw.Put("Require(\"null\");\n");
				cw.Put("}\n");
				cw.Put("else {\n");
				cw.Put("{0} = ", name);
				GenerateValue(t.GetGenericArguments()[0], name);
				cw.Put("}\n");
				return;
			}
			if (t.IsArray && !JsonOptions.ArrayLengthPrefix) {
				PutRequireOrNullArray('[', t, name);
				cw.Put("if (SkipSpacesCarefully() == ']') {\n");
				cw.Put("Require(']');\n");
				cw.Put("}\n");
				cw.Put("else {\n");
				var tempListName = cw.GetTempName();
				cw.Put("var {0} = new List<{1}>();\n", tempListName, Utils.GetTypeSpec(t.GetElementType()));
				cw.Put("do {\n");
				var tempName = cw.GetTempName();
				cw.Put("var {0} = ", tempName);
				GenerateValue(t.GetElementType(), tempName);
				cw.Put("{0}.Add({1});\n", tempListName, tempName);
				cw.Put("} while (Require(']', ',') == ',');\n");
				cw.Put("{0} = {1}.ToArray();\n", name, tempListName);
				cw.Put("}\n");
				cw.Put("}\n");
				return;
			}
			if (t.IsArray && JsonOptions.ArrayLengthPrefix) {
				PutRequireOrNullArray('[', t, name);
				cw.Put("if (SkipSpacesCarefully() != ']') {\n");
				var tempArrayName = cw.GetTempName();
				cw.Put("var {0} = new {1};\n", tempArrayName, Utils.GetTypeSpec(t, arraySize: "RequireUInt()"));
				var tempIndexName = cw.GetTempName();
				cw.Put("for(int {0} = 0; {0} < {1}.Length; ++{0}) {{\n", tempIndexName, tempArrayName);
				cw.Put("Require(',');\n");
				cw.Put("{0}[{1}] = ", tempArrayName, tempIndexName);
				GenerateValue(t.GetElementType(), String.Format("{0}[{1}]", tempArrayName, tempIndexName));
				cw.Put("}\n");
				cw.Put("{0} = {1};\n", name, tempArrayName);
				cw.Put("}\n");
				cw.Put("Require(']');\n");
				cw.Put("}\n");
				return;
			}
			var icoll = Utils.GetICollection(t);
			if (icoll != null) {
				PutRequireOrNull('[', t, name);
				GenerateCollection(t, icoll, name);
				cw.Put("}\n");
				return;
			}
			if (t.IsClass || t.IsInterface || Utils.IsStruct(t)) {
				var fmt = t.IsInterface || t.IsAbstract ?
					"{0}.Instance.FromReaderInterface<{1}>(Reader);\n" :
					"{0}.Instance.FromReaderTyped<{1}>(Reader);\n";
				cw.PutPart(fmt, GetDeserializerName(t), Utils.GetTypeSpec(t));
				return;
			}
			throw new NotImplementedException(t.Name);
		}

		private void GenAssigns(string name, object obj)
		{
			foreach (var m in obj.GetType().GetMembers()) {
				if (m.MemberType == MemberTypes.Field) {
					var f = (FieldInfo)m;
					var v = Utils.CodeValueFormat(f.GetValue(obj));
					if (v != "") // TODO
						cw.Put("{0}.{1} = {2};\n", name, f.Name, v);
				}
				else if (m.MemberType == MemberTypes.Property) {
					var p = (PropertyInfo)m;
					if (p.CanWrite) {
						var v = Utils.CodeValueFormat(p.GetValue(obj, Utils.ZeroObjects));
						if (v != "") // TODO
							cw.Put("{0}.{1} = {2};\n", name, p.Name, v);
					}
				}
			}
		}

		public void Generate<T>() { Generate(typeof(T)); }

		public void Generate(Type t)
		{
			var meta = Meta.Get(t, Options);

			if (lastNameSpace != t.Namespace) {
				if (lastNameSpace != "")
					cw.Put("}\n");
				cw.Put("\n");
				lastNameSpace = t.Namespace;
				cw.Put("namespace {0}.{1}\n", wrapperNameSpace, lastNameSpace);
				cw.Put("{\n");
			}

			var deserializerName = Utils.GetMangledTypeName(t) + "_JsonDeserializer";
			cw.Put("class {0} : JsonDeserializerGenBase\n", deserializerName);
			cw.Put("{\n");

			cw.Put("public static new {0} Instance = new {0}();\n", deserializerName);
			cw.Put("\n");

			cw.Put("public {0}()\n", deserializerName);
			cw.Put("{\n");
			GenAssigns("Options", Options);
			GenAssigns("JsonOptions", JsonOptions);
			cw.Put("}\n");
			cw.Put("\n");

			var icoll = Utils.GetICollection(t);
			var typeSpec = Utils.GetTypeSpec(t);
			cw.Put("public override object FromReaderInt()\n");
			cw.Put("{\n");
			if (icoll != null)
				cw.Put("return FromReaderInt(new {0}());\n", typeSpec);
			else if (t.IsInterface || t.IsAbstract)
				cw.Put("return FromReaderInterface<{0}>(Reader);\n", typeSpec);
			else
				cw.Put("return FromReaderTyped<{0}>(Reader);\n", typeSpec);
			cw.Put("}\n");
			cw.Put("\n");

			if (icoll != null) {
				cw.Put("public override object FromReaderInt(object obj)\n");
				cw.Put("{\n");
				cw.Put("var result = ({0})obj;\n", typeSpec);
				cw.Put("Require('[');\n");
				GenerateCollection(t, icoll, "result");
				cw.Put("return result;\n");
				cw.Put("}\n");
				cw.Put("\n");
			}

			cw.Put("public override object FromReaderIntPartial(string name)\n");
			cw.Put("{\n");
			if (t.IsInterface || t.IsAbstract)
				cw.Put("return null;\n");
			else
				cw.Put("return ReadFields(new {0}(), name);\n", typeSpec);
			cw.Put("}\n");
			cw.Put("\n");

			cw.Put("protected override object ReadFields(object obj, string name)\n");
			cw.Put("{\n");
			cw.Put("var result = ({0})obj;\n", typeSpec);
			if (icoll == null) {
				cw.ResetTempNames();
				foreach (var yi in meta.Items) {
					if (yi.IsOptional) {
						cw.Put("if (\"{0}\" == name) {{\n", yi.Tag(Options));
						if (yi.SetValue != null)
							cw.Put("result.{0} = ", yi.Name);
					}
					else {
						cw.Put("if (\"{0}\" != name) throw new YuzuException(\"{0}!=\" + name);\n", yi.Tag(Options));
						if (yi.SetValue != null)
							cw.Put("result.{0} = ", yi.Name);
					}
					if (yi.SetValue != null)
						GenerateValue(yi.Type, "result." + yi.Name);
					else
						GenerateMerge(yi.Type, "result." + yi.Name);
					cw.Put("name = GetNextName(false);\n");
					if (yi.IsOptional)
						cw.Put("}\n");
				}
				cw.GenerateActionList(meta.AfterDeserialization);
			}
			cw.Put("return result;\n");
			cw.Put("}\n");

			if (meta.IsCompact) {
				cw.Put("\n");
				cw.Put("protected override object ReadFieldsCompact(object obj)\n");
				cw.Put("{\n");
				cw.Put("var result = ({0})obj;\n", typeSpec);
				bool isFirst = true;
				cw.ResetTempNames();
				foreach (var yi in meta.Items) {
					if (!isFirst)
						cw.Put("Require(',');\n");
					isFirst = false;
					if (yi.SetValue != null) {
						cw.Put("result.{0} = ", yi.Name);
						GenerateValue(yi.Type, "result." + yi.Name);
					}
					else
						GenerateMerge(yi.Type, "result." + yi.Name);
				}
				cw.Put("Require(']');\n");
				cw.GenerateActionList(meta.AfterDeserialization);
				cw.Put("return result;\n");
				cw.Put("}\n");
			}
			cw.Put("}\n");
			cw.Put("\n");
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
