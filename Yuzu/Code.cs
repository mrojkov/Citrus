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
			foreach (var yi in Utils.GetYuzuItems(obj.GetType(), Options)) {
				if (!first) {
					builder.Append(",\n");
				}
				first = false;
				builder.Append(CodeConstructOptions.Indent);
				builder.Append(yi.Name);
				builder.Append(" = ");
				if (yi.Type == typeof(int)) {
					builder.Append(yi.GetValue(obj).ToString());
				}
				else if (yi.Type == typeof(string)) {
					builder.AppendFormat("\"{0}\"", yi.GetValue(obj).ToString());
				}
				else {
					throw new NotImplementedException(yi.Type.Name);
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
			foreach (var yi in Utils.GetYuzuItems(obj.GetType(), Options)) {
				string valueStr;
				if (yi.Type == typeof(int)) {
					valueStr = yi.GetValue(obj).ToString();
				}
				else if (yi.Type == typeof(string)) {
					valueStr = '"' + yi.GetValue(obj).ToString() + '"';
				}
				else {
					throw new NotImplementedException(yi.Type.Name);
				}
				builder.AppendFormat("{0}obj.{1} = {2};\n", CodeAssignOptions.Indent, yi.Name, valueStr);
			}
			builder.Append("}\n");
		}
	}
}
