using System;

namespace Yuzu
{
	public class CodeConstructSerializeOptions
	{
		public string VarName = "x";
		public string Indent = "\t";
	};

	public class CodeConstructSerializer : AbstractWriterSerializer
	{
		public CodeConstructSerializeOptions CodeConstructOptions = new CodeConstructSerializeOptions();

		public override void ToWriter(object obj)
		{
			WriteStr(String.Format("var {0} = new {1} {{\n", CodeConstructOptions.VarName, obj.GetType().Name));
			var first = true;
			foreach (var f in obj.GetType().GetFields()) {
				if (!first) {
					WriteStr(",\n");
				}
				first = false;
				WriteStr(CodeConstructOptions.Indent);
				WriteStr(f.Name);
				WriteStr(" = ");
				var t = f.FieldType;
				if (t == typeof(int)) {
					WriteStr(f.GetValue(obj).ToString());
				}
				else if (t == typeof(string)) {
					Writer.Write('"');
					WriteStr(f.GetValue(obj).ToString());
					Writer.Write('"');
				}
				else {
					throw new NotImplementedException(t.Name);
				}
			}
			WriteStr("\n};\n");
		}
	}

	public class CodeAssignSerializeOptions
	{
		public string FuncName = "Init";
		public string Indent = "\t";
	};

	public class CodeAssignSerializer : AbstractWriterSerializer
	{
		public CodeAssignSerializeOptions CodeAssignOptions = new CodeAssignSerializeOptions();

		public override void ToWriter(object obj)
		{
			WriteStr(String.Format("void {0}({1} obj) {{\n", CodeAssignOptions.FuncName, obj.GetType().Name));
			foreach (var f in obj.GetType().GetFields()) {
				string valueStr;
				var t = f.FieldType;
				if (t == typeof(int)) {
					valueStr = f.GetValue(obj).ToString();
				}
				else if (t == typeof(string)) {
					valueStr = '"' + f.GetValue(obj).ToString() + '"';
				}
				else {
					throw new NotImplementedException(t.Name);
				}
				WriteStr(String.Format("{0}obj.{1} = {2};\n", CodeAssignOptions.Indent, f.Name, valueStr));
			}
			WriteStr("}\n");
		}
	}
}
