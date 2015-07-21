using System;

namespace Yuzu
{
	public class CodeConstructSerializeOptions
	{
		public string VarName = "x";
		public string Indent = "\t";
	};

	public class CodeConstructSerializer : AbstractStringSerializer
	{
		public CodeConstructSerializeOptions CodeConstructOptions = new CodeConstructSerializeOptions();

		protected override void ToBuilder(object obj)
		{
			builder.AppendFormat("var {0} = new {1} {{\n", CodeConstructOptions.VarName, obj.GetType().Name);
			var first = true;
			foreach (var f in obj.GetType().GetFields()) {
				if (!first) {
					builder.Append(",\n");
				}
				first = false;
				builder.Append(CodeConstructOptions.Indent);
				builder.Append(f.Name);
				builder.Append(" = ");
				var t = f.FieldType;
				if (t == typeof(int)) {
					builder.Append(f.GetValue(obj).ToString());
				}
				else if (t == typeof(string)) {
					builder.AppendFormat("\"{0}\"", f.GetValue(obj).ToString());
				}
				else {
					throw new NotImplementedException(t.Name);
				}
			}
			builder.Append("\n};\n");
		}
	}

	public class CodeAssignSerializeOptions
	{
		public string FuncName = "Init";
		public string Indent = "\t";
	};

	public class CodeAssignSerializer : AbstractStringSerializer
	{
		public CodeAssignSerializeOptions CodeAssignOptions = new CodeAssignSerializeOptions();

		protected override void ToBuilder(object obj)
		{
			builder.AppendFormat("void {0}({1} obj) {{\n", CodeAssignOptions.FuncName, obj.GetType().Name);
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
				builder.AppendFormat("{0}obj.{1} = {2};\n", CodeAssignOptions.Indent, f.Name, valueStr);
			}
			builder.Append("}\n");
		}
	}
}
