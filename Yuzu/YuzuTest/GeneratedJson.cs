using System;
using System.Collections.Generic;

using Yuzu;
using Yuzu.Json;

namespace YuzuGen.YuzuTest
{
	class Sample1_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Sample1_JsonDeserializer Instance = new Sample1_JsonDeserializer();

		public Sample1_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.Sample1>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.Sample1(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.Sample1)obj;
			if ("X" != name) throw new YuzuException("X!=" + name);
			result.X = RequireInt();
			name = GetNextName(false);
			if ("Y" == name) {
				result.Y = RequireString();
				name = GetNextName(false);
			}
			return result;
		}
	}

	class Sample2_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Sample2_JsonDeserializer Instance = new Sample2_JsonDeserializer();

		public Sample2_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.Sample2>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.Sample2(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.Sample2)obj;
			if ("X" != name) throw new YuzuException("X!=" + name);
			result.X = RequireInt();
			name = GetNextName(false);
			if ("Y" == name) {
				result.Y = RequireString();
				name = GetNextName(false);
			}
			return result;
		}
	}

	class Sample3_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Sample3_JsonDeserializer Instance = new Sample3_JsonDeserializer();

		public Sample3_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.Sample3>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.Sample3(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.Sample3)obj;
			if ("S1" != name) throw new YuzuException("S1!=" + name);
			result.S1 = YuzuGen.YuzuTest.Sample1_JsonDeserializer.Instance.FromReaderTyped<global::YuzuTest.Sample1>(Reader);
			name = GetNextName(false);
			if ("F" == name) {
				result.F = RequireInt();
				name = GetNextName(false);
			}
			if ("S2" == name) {
				result.S2 = YuzuGen.YuzuTest.Sample2_JsonDeserializer.Instance.FromReaderTyped<global::YuzuTest.Sample2>(Reader);
				name = GetNextName(false);
			}
			return result;
		}
	}

	class Sample4_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Sample4_JsonDeserializer Instance = new Sample4_JsonDeserializer();

		public Sample4_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.Sample4>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.Sample4(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.Sample4)obj;
			if ("E" == name) {
				result.E = (global::YuzuTest.SampleEnum)Enum.Parse(typeof(global::YuzuTest.SampleEnum), RequireString());
				name = GetNextName(false);
			}
			return result;
		}
	}

	class SampleDecimal_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleDecimal_JsonDeserializer Instance = new SampleDecimal_JsonDeserializer();

		public SampleDecimal_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleDecimal>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleDecimal(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleDecimal)obj;
			if ("N" != name) throw new YuzuException("N!=" + name);
			result.N = RequireDecimal();
			name = GetNextName(false);
			return result;
		}
	}

	class SampleNullable_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleNullable_JsonDeserializer Instance = new SampleNullable_JsonDeserializer();

		public SampleNullable_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleNullable>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleNullable(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleNullable)obj;
			if ("N" != name) throw new YuzuException("N!=" + name);
			result.N = null;
			if (SkipSpacesCarefully() == 'n') {
				Require("null");
			}
			else {
				result.N = RequireInt();
			}
			name = GetNextName(false);
			return result;
		}
	}

	class SampleBool_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleBool_JsonDeserializer Instance = new SampleBool_JsonDeserializer();

		public SampleBool_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleBool>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleBool(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleBool)obj;
			if ("B" != name) throw new YuzuException("B!=" + name);
			result.B = RequireBool();
			name = GetNextName(false);
			return result;
		}
	}

	class SampleList_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleList_JsonDeserializer Instance = new SampleList_JsonDeserializer();

		public SampleList_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleList>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleList(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleList)obj;
			if ("E" != name) throw new YuzuException("E!=" + name);
			result.E = RequireOrNull('[') ? null : new global::System.Collections.Generic.List<global::System.String>();
			if (result.E != null) {
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp1 = RequireString();
						result.E.Add(tmp1);
					} while (Require(']', ',') == ',');
				}
			}
			name = GetNextName(false);
			return result;
		}
	}

	class SampleObj_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleObj_JsonDeserializer Instance = new SampleObj_JsonDeserializer();

		public SampleObj_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleObj>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleObj(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleObj)obj;
			if ("F" != name) throw new YuzuException("F!=" + name);
			result.F = ReadAnyObject();
			name = GetNextName(false);
			return result;
		}
	}

	class SampleDict_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleDict_JsonDeserializer Instance = new SampleDict_JsonDeserializer();

		public SampleDict_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleDict>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleDict(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleDict)obj;
			if ("Value" != name) throw new YuzuException("Value!=" + name);
			result.Value = RequireInt();
			name = GetNextName(false);
			if ("Children" == name) {
				result.Children = RequireOrNull('{') ? null : new global::System.Collections.Generic.Dictionary<global::System.String,global::YuzuTest.SampleDict>();
				if (result.Children != null) {
					if (SkipSpacesCarefully() == '}') {
						Require('}');
					}
					else {
						do {
							var tmp1 = RequireString();
							Require(':');
							var tmp2 = YuzuGen.YuzuTest.SampleDict_JsonDeserializer.Instance.FromReaderTyped<global::YuzuTest.SampleDict>(Reader);
							result.Children.Add(tmp1, tmp2);
						} while (Require('}', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			return result;
		}
	}

	class SampleDictKeys_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleDictKeys_JsonDeserializer Instance = new SampleDictKeys_JsonDeserializer();

		public SampleDictKeys_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleDictKeys>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleDictKeys(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleDictKeys)obj;
			if ("E" != name) throw new YuzuException("E!=" + name);
			result.E = RequireOrNull('{') ? null : new global::System.Collections.Generic.Dictionary<global::YuzuTest.SampleEnum,global::System.Int32>();
			if (result.E != null) {
				if (SkipSpacesCarefully() == '}') {
					Require('}');
				}
				else {
					do {
						var tmp1 = RequireString();
						Require(':');
						var tmp2 = RequireInt();
						result.E.Add((global::YuzuTest.SampleEnum)Enum.Parse(typeof(global::YuzuTest.SampleEnum), tmp1), tmp2);
					} while (Require('}', ',') == ',');
				}
			}
			name = GetNextName(false);
			if ("I" != name) throw new YuzuException("I!=" + name);
			result.I = RequireOrNull('{') ? null : new global::System.Collections.Generic.Dictionary<global::System.Int32,global::System.Int32>();
			if (result.I != null) {
				if (SkipSpacesCarefully() == '}') {
					Require('}');
				}
				else {
					do {
						var tmp3 = RequireString();
						Require(':');
						var tmp4 = RequireInt();
						result.I.Add(int.Parse(tmp3), tmp4);
					} while (Require('}', ',') == ',');
				}
			}
			name = GetNextName(false);
			if ("K" != name) throw new YuzuException("K!=" + name);
			result.K = RequireOrNull('{') ? null : new global::System.Collections.Generic.Dictionary<global::YuzuTest.SampleKey,global::System.Int32>();
			if (result.K != null) {
				if (SkipSpacesCarefully() == '}') {
					Require('}');
				}
				else {
					do {
						var tmp5 = RequireString();
						Require(':');
						var tmp6 = RequireInt();
						result.K.Add((global::YuzuTest.SampleKey)keyParsers[typeof(global::YuzuTest.SampleKey)](tmp5), tmp6);
					} while (Require('}', ',') == ',');
				}
			}
			name = GetNextName(false);
			return result;
		}
	}

	class ISampleMember_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new ISampleMember_JsonDeserializer Instance = new ISampleMember_JsonDeserializer();

		public ISampleMember_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderInterface<global::YuzuTest.ISampleMember>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return null;
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.ISampleMember)obj;
			if ("X" == name) {
				result.X = RequireInt();
				name = GetNextName(false);
			}
			return result;
		}
	}

	class SampleMemberI_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleMemberI_JsonDeserializer Instance = new SampleMemberI_JsonDeserializer();

		public SampleMemberI_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleMemberI>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleMemberI(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleMemberI)obj;
			if ("X" == name) {
				result.X = RequireInt();
				name = GetNextName(false);
			}
			return result;
		}
	}

}

namespace YuzuGen.System.Collections.Generic
{
	class List_ISampleMember_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new List_ISampleMember_JsonDeserializer Instance = new List_ISampleMember_JsonDeserializer();

		public List_ISampleMember_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new global::System.Collections.Generic.List<global::YuzuTest.ISampleMember>());
		}

		public override object FromReaderInt(object obj)
		{
			var result = (global::System.Collections.Generic.List<global::YuzuTest.ISampleMember>)obj;
			Require('[');
			if (SkipSpacesCarefully() == ']') {
				Require(']');
			}
			else {
				do {
					var tmp1 = YuzuGen.YuzuTest.ISampleMember_JsonDeserializer.Instance.FromReaderInterface<global::YuzuTest.ISampleMember>(Reader);
					result.Add(tmp1);
				} while (Require(']', ',') == ',');
			}
			return result;
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::System.Collections.Generic.List<global::YuzuTest.ISampleMember>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::System.Collections.Generic.List<global::YuzuTest.ISampleMember>)obj;
			return result;
		}
	}

}

namespace YuzuGen.YuzuTest
{
	class SampleArray_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleArray_JsonDeserializer Instance = new SampleArray_JsonDeserializer();

		public SampleArray_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleArray>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleArray(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleArray)obj;
			if ("A" != name) throw new YuzuException("A!=" + name);
			result.A = RequireOrNull('[') ? null : new global::System.String[0];
			if (result.A != null) {
				if (SkipSpacesCarefully() != ']') {
					var tmp1 = new global::System.String[RequireUInt()];
					for(int tmp2 = 0; tmp2 < tmp1.Length; ++tmp2) {
						Require(',');
						tmp1[tmp2] = RequireString();
					}
					result.A = tmp1;
				}
				Require(']');
			}
			name = GetNextName(false);
			return result;
		}
	}

	class SampleArray2D_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleArray2D_JsonDeserializer Instance = new SampleArray2D_JsonDeserializer();

		public SampleArray2D_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleArray2D>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleArray2D(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleArray2D)obj;
			if ("A" != name) throw new YuzuException("A!=" + name);
			result.A = RequireOrNull('[') ? null : new global::System.Int32[0][];
			if (result.A != null) {
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					var tmp1 = new List<global::System.Int32[]>();
					do {
						var tmp2 = RequireOrNull('[') ? null : new global::System.Int32[0];
						if (tmp2 != null) {
							if (SkipSpacesCarefully() == ']') {
								Require(']');
							}
							else {
								var tmp3 = new List<global::System.Int32>();
								do {
									var tmp4 = RequireInt();
									tmp3.Add(tmp4);
								} while (Require(']', ',') == ',');
								tmp2 = tmp3.ToArray();
							}
						}
						tmp1.Add(tmp2);
					} while (Require(']', ',') == ',');
					result.A = tmp1.ToArray();
				}
			}
			name = GetNextName(false);
			return result;
		}
	}

	class SampleBase_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleBase_JsonDeserializer Instance = new SampleBase_JsonDeserializer();

		public SampleBase_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleBase>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleBase(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleBase)obj;
			if ("FBase" != name) throw new YuzuException("FBase!=" + name);
			result.FBase = RequireInt();
			name = GetNextName(false);
			return result;
		}
	}

	class SampleDerivedA_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleDerivedA_JsonDeserializer Instance = new SampleDerivedA_JsonDeserializer();

		public SampleDerivedA_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleDerivedA>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleDerivedA(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleDerivedA)obj;
			if ("FBase" != name) throw new YuzuException("FBase!=" + name);
			result.FBase = RequireInt();
			name = GetNextName(false);
			if ("FA" != name) throw new YuzuException("FA!=" + name);
			result.FA = RequireInt();
			name = GetNextName(false);
			return result;
		}
	}

	class SampleDerivedB_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleDerivedB_JsonDeserializer Instance = new SampleDerivedB_JsonDeserializer();

		public SampleDerivedB_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleDerivedB>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleDerivedB(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleDerivedB)obj;
			if ("FBase" != name) throw new YuzuException("FBase!=" + name);
			result.FBase = RequireInt();
			name = GetNextName(false);
			if ("FB" != name) throw new YuzuException("FB!=" + name);
			result.FB = RequireInt();
			name = GetNextName(false);
			return result;
		}
	}

	class SampleMatrix_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleMatrix_JsonDeserializer Instance = new SampleMatrix_JsonDeserializer();

		public SampleMatrix_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleMatrix>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleMatrix(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleMatrix)obj;
			if ("M" != name) throw new YuzuException("M!=" + name);
			result.M = RequireOrNull('[') ? null : new global::System.Collections.Generic.List<global::System.Collections.Generic.List<global::System.Int32>>();
			if (result.M != null) {
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp1 = RequireOrNull('[') ? null : new global::System.Collections.Generic.List<global::System.Int32>();
						if (tmp1 != null) {
							if (SkipSpacesCarefully() == ']') {
								Require(']');
							}
							else {
								do {
									var tmp2 = RequireInt();
									tmp1.Add(tmp2);
								} while (Require(']', ',') == ',');
							}
						}
						result.M.Add(tmp1);
					} while (Require(']', ',') == ',');
				}
			}
			name = GetNextName(false);
			return result;
		}
	}

	class SamplePoint_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SamplePoint_JsonDeserializer Instance = new SamplePoint_JsonDeserializer();

		public SamplePoint_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SamplePoint>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SamplePoint(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SamplePoint)obj;
			if ("X" != name) throw new YuzuException("X!=" + name);
			result.X = RequireInt();
			name = GetNextName(false);
			if ("Y" != name) throw new YuzuException("Y!=" + name);
			result.Y = RequireInt();
			name = GetNextName(false);
			return result;
		}

		protected override object ReadFieldsCompact(object obj)
		{
			var result = (global::YuzuTest.SamplePoint)obj;
			result.X = RequireInt();
			Require(',');
			result.Y = RequireInt();
			Require(']');
			return result;
		}
	}

	class SampleRect_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleRect_JsonDeserializer Instance = new SampleRect_JsonDeserializer();

		public SampleRect_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleRect>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleRect(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleRect)obj;
			if ("A" != name) throw new YuzuException("A!=" + name);
			result.A = YuzuGen.YuzuTest.SamplePoint_JsonDeserializer.Instance.FromReaderTyped<global::YuzuTest.SamplePoint>(Reader);
			name = GetNextName(false);
			if ("B" != name) throw new YuzuException("B!=" + name);
			result.B = YuzuGen.YuzuTest.SamplePoint_JsonDeserializer.Instance.FromReaderTyped<global::YuzuTest.SamplePoint>(Reader);
			name = GetNextName(false);
			return result;
		}
	}

	class SampleDate_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleDate_JsonDeserializer Instance = new SampleDate_JsonDeserializer();

		public SampleDate_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleDate>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleDate(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleDate)obj;
			if ("D" != name) throw new YuzuException("D!=" + name);
			result.D = RequireDateTime();
			name = GetNextName(false);
			if ("T" != name) throw new YuzuException("T!=" + name);
			result.T = RequireTimeSpan();
			name = GetNextName(false);
			return result;
		}
	}

	class Color_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Color_JsonDeserializer Instance = new Color_JsonDeserializer();

		public Color_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.Color>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.Color(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.Color)obj;
			if ("B" != name) throw new YuzuException("B!=" + name);
			result.B = checked((byte)RequireUInt());
			name = GetNextName(false);
			if ("G" != name) throw new YuzuException("G!=" + name);
			result.G = checked((byte)RequireUInt());
			name = GetNextName(false);
			if ("R" != name) throw new YuzuException("R!=" + name);
			result.R = checked((byte)RequireUInt());
			name = GetNextName(false);
			return result;
		}

		protected override object ReadFieldsCompact(object obj)
		{
			var result = (global::YuzuTest.Color)obj;
			result.B = checked((byte)RequireUInt());
			Require(',');
			result.G = checked((byte)RequireUInt());
			Require(',');
			result.R = checked((byte)RequireUInt());
			Require(']');
			return result;
		}
	}

}

namespace YuzuGen.System.Collections.Generic
{
	class List_List_Int32_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new List_List_Int32_JsonDeserializer Instance = new List_List_Int32_JsonDeserializer();

		public List_List_Int32_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new global::System.Collections.Generic.List<global::System.Collections.Generic.List<global::System.Int32>>());
		}

		public override object FromReaderInt(object obj)
		{
			var result = (global::System.Collections.Generic.List<global::System.Collections.Generic.List<global::System.Int32>>)obj;
			Require('[');
			if (SkipSpacesCarefully() == ']') {
				Require(']');
			}
			else {
				do {
					var tmp1 = RequireOrNull('[') ? null : new global::System.Collections.Generic.List<global::System.Int32>();
					if (tmp1 != null) {
						if (SkipSpacesCarefully() == ']') {
							Require(']');
						}
						else {
							do {
								var tmp2 = RequireInt();
								tmp1.Add(tmp2);
							} while (Require(']', ',') == ',');
						}
					}
					result.Add(tmp1);
				} while (Require(']', ',') == ',');
			}
			return result;
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::System.Collections.Generic.List<global::System.Collections.Generic.List<global::System.Int32>>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::System.Collections.Generic.List<global::System.Collections.Generic.List<global::System.Int32>>)obj;
			return result;
		}
	}

}

namespace YuzuGen.YuzuTest
{
	class SampleClassList_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleClassList_JsonDeserializer Instance = new SampleClassList_JsonDeserializer();

		public SampleClassList_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleClassList>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleClassList(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleClassList)obj;
			if ("E" != name) throw new YuzuException("E!=" + name);
			result.E = RequireOrNull('[') ? null : new global::System.Collections.Generic.List<global::YuzuTest.SampleBase>();
			if (result.E != null) {
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp1 = YuzuGen.YuzuTest.SampleBase_JsonDeserializer.Instance.FromReaderTyped<global::YuzuTest.SampleBase>(Reader);
						result.E.Add(tmp1);
					} while (Require(']', ',') == ',');
				}
			}
			name = GetNextName(false);
			return result;
		}
	}

	class SampleSmallTypes_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleSmallTypes_JsonDeserializer Instance = new SampleSmallTypes_JsonDeserializer();

		public SampleSmallTypes_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleSmallTypes>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleSmallTypes(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleSmallTypes)obj;
			if ("B" != name) throw new YuzuException("B!=" + name);
			result.B = checked((byte)RequireUInt());
			name = GetNextName(false);
			if ("Ch" != name) throw new YuzuException("Ch!=" + name);
			result.Ch = RequireChar();
			name = GetNextName(false);
			if ("Sb" != name) throw new YuzuException("Sb!=" + name);
			result.Sb = checked((sbyte)RequireInt());
			name = GetNextName(false);
			if ("Sh" != name) throw new YuzuException("Sh!=" + name);
			result.Sh = checked((short)RequireInt());
			name = GetNextName(false);
			if ("USh" != name) throw new YuzuException("USh!=" + name);
			result.USh = checked((ushort)RequireUInt());
			name = GetNextName(false);
			return result;
		}
	}

	class SampleWithNullFieldCompact_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleWithNullFieldCompact_JsonDeserializer Instance = new SampleWithNullFieldCompact_JsonDeserializer();

		public SampleWithNullFieldCompact_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleWithNullFieldCompact>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleWithNullFieldCompact(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleWithNullFieldCompact)obj;
			if ("N" != name) throw new YuzuException("N!=" + name);
			result.N = YuzuGen.YuzuTest.Sample1_JsonDeserializer.Instance.FromReaderTyped<global::YuzuTest.Sample1>(Reader);
			name = GetNextName(false);
			return result;
		}

		protected override object ReadFieldsCompact(object obj)
		{
			var result = (global::YuzuTest.SampleWithNullFieldCompact)obj;
			result.N = YuzuGen.YuzuTest.Sample1_JsonDeserializer.Instance.FromReaderTyped<global::YuzuTest.Sample1>(Reader);
			Require(']');
			return result;
		}
	}

	class SampleNested__NestedClass_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleNested__NestedClass_JsonDeserializer Instance = new SampleNested__NestedClass_JsonDeserializer();

		public SampleNested__NestedClass_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleNested.NestedClass>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleNested.NestedClass(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleNested.NestedClass)obj;
			if ("Z" == name) {
				result.Z = RequireInt();
				name = GetNextName(false);
			}
			return result;
		}
	}

	class SampleNested_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleNested_JsonDeserializer Instance = new SampleNested_JsonDeserializer();

		public SampleNested_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleNested>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleNested(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleNested)obj;
			if ("C" != name) throw new YuzuException("C!=" + name);
			result.C = YuzuGen.YuzuTest.SampleNested__NestedClass_JsonDeserializer.Instance.FromReaderTyped<global::YuzuTest.SampleNested.NestedClass>(Reader);
			name = GetNextName(false);
			if ("E" != name) throw new YuzuException("E!=" + name);
			result.E = (global::YuzuTest.SampleNested.NestedEnum)Enum.Parse(typeof(global::YuzuTest.SampleNested.NestedEnum), RequireString());
			name = GetNextName(false);
			if ("Z" == name) {
				result.Z = RequireOrNull('[') ? null : new global::YuzuTest.SampleNested.NestedEnum[0];
				if (result.Z != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						var tmp1 = new List<global::YuzuTest.SampleNested.NestedEnum>();
						do {
							var tmp2 = (global::YuzuTest.SampleNested.NestedEnum)Enum.Parse(typeof(global::YuzuTest.SampleNested.NestedEnum), RequireString());
							tmp1.Add(tmp2);
						} while (Require(']', ',') == ',');
						result.Z = tmp1.ToArray();
					}
				}
				name = GetNextName(false);
			}
			return result;
		}
	}

	class SamplePerson_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SamplePerson_JsonDeserializer Instance = new SamplePerson_JsonDeserializer();

		public SamplePerson_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SamplePerson>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SamplePerson(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SamplePerson)obj;
			if ("1" != name) throw new YuzuException("1!=" + name);
			result.Name = RequireString();
			name = GetNextName(false);
			if ("2" != name) throw new YuzuException("2!=" + name);
			result.Birth = RequireDateTime();
			name = GetNextName(false);
			if ("3" != name) throw new YuzuException("3!=" + name);
			result.Children = RequireOrNull('[') ? null : new global::System.Collections.Generic.List<global::YuzuTest.SamplePerson>();
			if (result.Children != null) {
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp1 = YuzuGen.YuzuTest.SamplePerson_JsonDeserializer.Instance.FromReaderTyped<global::YuzuTest.SamplePerson>(Reader);
						result.Children.Add(tmp1);
					} while (Require(']', ',') == ',');
				}
			}
			name = GetNextName(false);
			if ("4" != name) throw new YuzuException("4!=" + name);
			result.EyeColor = YuzuGen.YuzuTest.Color_JsonDeserializer.Instance.FromReaderTyped<global::YuzuTest.Color>(Reader);
			name = GetNextName(false);
			return result;
		}
	}

	class ISample_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new ISample_JsonDeserializer Instance = new ISample_JsonDeserializer();

		public ISample_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderInterface<global::YuzuTest.ISample>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return null;
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.ISample)obj;
			return result;
		}
	}

	class SampleInterfaced_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleInterfaced_JsonDeserializer Instance = new SampleInterfaced_JsonDeserializer();

		public SampleInterfaced_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleInterfaced>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleInterfaced(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleInterfaced)obj;
			if ("X" != name) throw new YuzuException("X!=" + name);
			result.X = RequireInt();
			name = GetNextName(false);
			return result;
		}
	}

	class SampleInterfaceField_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleInterfaceField_JsonDeserializer Instance = new SampleInterfaceField_JsonDeserializer();

		public SampleInterfaceField_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleInterfaceField>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleInterfaceField(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleInterfaceField)obj;
			if ("I" != name) throw new YuzuException("I!=" + name);
			result.I = YuzuGen.YuzuTest.ISample_JsonDeserializer.Instance.FromReaderInterface<global::YuzuTest.ISample>(Reader);
			name = GetNextName(false);
			return result;
		}
	}

	class SampleInterfacedGeneric_String_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleInterfacedGeneric_String_JsonDeserializer Instance = new SampleInterfacedGeneric_String_JsonDeserializer();

		public SampleInterfacedGeneric_String_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleInterfacedGeneric<global::System.String>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleInterfacedGeneric<global::System.String>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleInterfacedGeneric<global::System.String>)obj;
			if ("G" != name) throw new YuzuException("G!=" + name);
			result.G = RequireString();
			name = GetNextName(false);
			if ("X" != name) throw new YuzuException("X!=" + name);
			result.X = RequireInt();
			name = GetNextName(false);
			return result;
		}
	}

	class SampleAbstract_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleAbstract_JsonDeserializer Instance = new SampleAbstract_JsonDeserializer();

		public SampleAbstract_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderInterface<global::YuzuTest.SampleAbstract>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return null;
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleAbstract)obj;
			return result;
		}
	}

	class SampleConcrete_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleConcrete_JsonDeserializer Instance = new SampleConcrete_JsonDeserializer();

		public SampleConcrete_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleConcrete>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleConcrete(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleConcrete)obj;
			if ("XX" != name) throw new YuzuException("XX!=" + name);
			result.XX = RequireInt();
			name = GetNextName(false);
			return result;
		}
	}

	class SampleCollection_Int32_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleCollection_Int32_JsonDeserializer Instance = new SampleCollection_Int32_JsonDeserializer();

		public SampleCollection_Int32_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new global::YuzuTest.SampleCollection<global::System.Int32>());
		}

		public override object FromReaderInt(object obj)
		{
			var result = (global::YuzuTest.SampleCollection<global::System.Int32>)obj;
			Require('[');
			if (SkipSpacesCarefully() == ']') {
				Require(']');
			}
			else {
				do {
					var tmp1 = RequireInt();
					result.Add(tmp1);
				} while (Require(']', ',') == ',');
			}
			return result;
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleCollection<global::System.Int32>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleCollection<global::System.Int32>)obj;
			return result;
		}
	}

	class SampleExplicitCollection_Int32_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleExplicitCollection_Int32_JsonDeserializer Instance = new SampleExplicitCollection_Int32_JsonDeserializer();

		public SampleExplicitCollection_Int32_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new global::YuzuTest.SampleExplicitCollection<global::System.Int32>());
		}

		public override object FromReaderInt(object obj)
		{
			var result = (global::YuzuTest.SampleExplicitCollection<global::System.Int32>)obj;
			Require('[');
			if (SkipSpacesCarefully() == ']') {
				Require(']');
			}
			else {
				do {
					var tmp2 = RequireInt();
					((global::System.Collections.Generic.ICollection<global::System.Int32>)result).Add(tmp2);
				} while (Require(']', ',') == ',');
			}
			return result;
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleExplicitCollection<global::System.Int32>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleExplicitCollection<global::System.Int32>)obj;
			return result;
		}
	}

	class SampleWithCollection_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleWithCollection_JsonDeserializer Instance = new SampleWithCollection_JsonDeserializer();

		public SampleWithCollection_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleWithCollection>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleWithCollection(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleWithCollection)obj;
			if ("A" != name) throw new YuzuException("A!=" + name);
			result.A = RequireOrNull('[') ? null : new global::YuzuTest.SampleCollection<global::YuzuTest.ISample>();
			if (result.A != null) {
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp1 = YuzuGen.YuzuTest.ISample_JsonDeserializer.Instance.FromReaderInterface<global::YuzuTest.ISample>(Reader);
						result.A.Add(tmp1);
					} while (Require(']', ',') == ',');
				}
			}
			name = GetNextName(false);
			if ("B" != name) throw new YuzuException("B!=" + name);
			result.B = RequireOrNull('[') ? null : new global::YuzuTest.SampleCollection<global::System.Int32>();
			if (result.B != null) {
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = RequireInt();
						result.B.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
			}
			name = GetNextName(false);
			return result;
		}
	}

	class SampleConcreteCollection_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleConcreteCollection_JsonDeserializer Instance = new SampleConcreteCollection_JsonDeserializer();

		public SampleConcreteCollection_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new global::YuzuTest.SampleConcreteCollection());
		}

		public override object FromReaderInt(object obj)
		{
			var result = (global::YuzuTest.SampleConcreteCollection)obj;
			Require('[');
			if (SkipSpacesCarefully() == ']') {
				Require(']');
			}
			else {
				do {
					var tmp3 = RequireInt();
					result.Add(tmp3);
				} while (Require(']', ',') == ',');
			}
			return result;
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleConcreteCollection(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleConcreteCollection)obj;
			return result;
		}
	}

	class SampleAfter2_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleAfter2_JsonDeserializer Instance = new SampleAfter2_JsonDeserializer();

		public SampleAfter2_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleAfter2>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleAfter2(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleAfter2)obj;
			if ("X" != name) throw new YuzuException("X!=" + name);
			result.X = RequireString();
			name = GetNextName(false);
			result.After2();
			result.After3();
			result.After();
			return result;
		}
	}

	class SampleMerge_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleMerge_JsonDeserializer Instance = new SampleMerge_JsonDeserializer();

		public SampleMerge_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleMerge>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleMerge(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleMerge)obj;
			if ("DI" != name) throw new YuzuException("DI!=" + name);
			Require('{');
			if (SkipSpacesCarefully() == '}') {
				Require('}');
			}
			else {
				do {
					var tmp1 = RequireString();
					Require(':');
					var tmp2 = RequireInt();
					result.DI.Add(int.Parse(tmp1), tmp2);
				} while (Require('}', ',') == ',');
			}
			name = GetNextName(false);
			if ("LI" != name) throw new YuzuException("LI!=" + name);
			Require('[');
			if (SkipSpacesCarefully() == ']') {
				Require(']');
			}
			else {
				do {
					var tmp3 = RequireInt();
					result.LI.Add(tmp3);
				} while (Require(']', ',') == ',');
			}
			name = GetNextName(false);
			if ("M" == name) {
				YuzuGen.YuzuTest.Sample1_JsonDeserializer.Instance.FromReader(result.M, Reader);
				name = GetNextName(false);
			}
			return result;
		}
	}

	class SampleAssemblyDerivedR_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleAssemblyDerivedR_JsonDeserializer Instance = new SampleAssemblyDerivedR_JsonDeserializer();

		public SampleAssemblyDerivedR_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleAssemblyDerivedR>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleAssemblyDerivedR(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleAssemblyDerivedR)obj;
			if ("P" == name) {
				result.P = checked((short)RequireInt());
				name = GetNextName(false);
			}
			if ("R" == name) {
				result.R = RequireString();
				name = GetNextName(false);
			}
			return result;
		}
	}

	class SampleAliasMany_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleAliasMany_JsonDeserializer Instance = new SampleAliasMany_JsonDeserializer();

		public SampleAliasMany_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest.SampleAliasMany>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest.SampleAliasMany(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest.SampleAliasMany)obj;
			if ("X" != name) throw new YuzuException("X!=" + name);
			result.X = RequireInt();
			name = GetNextName(false);
			return result;
		}
	}

}

namespace YuzuGen.System.Collections.Generic
{
	class List_SampleAssemblyBase_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new List_SampleAssemblyBase_JsonDeserializer Instance = new List_SampleAssemblyBase_JsonDeserializer();

		public List_SampleAssemblyBase_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new global::System.Collections.Generic.List<global::YuzuTestAssembly.SampleAssemblyBase>());
		}

		public override object FromReaderInt(object obj)
		{
			var result = (global::System.Collections.Generic.List<global::YuzuTestAssembly.SampleAssemblyBase>)obj;
			Require('[');
			if (SkipSpacesCarefully() == ']') {
				Require(']');
			}
			else {
				do {
					var tmp1 = YuzuGen.YuzuTestAssembly.SampleAssemblyBase_JsonDeserializer.Instance.FromReaderTyped<global::YuzuTestAssembly.SampleAssemblyBase>(Reader);
					result.Add(tmp1);
				} while (Require(']', ',') == ',');
			}
			return result;
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::System.Collections.Generic.List<global::YuzuTestAssembly.SampleAssemblyBase>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::System.Collections.Generic.List<global::YuzuTestAssembly.SampleAssemblyBase>)obj;
			return result;
		}
	}

}

namespace YuzuGen.YuzuTestAssembly
{
	class SampleAssemblyBase_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleAssemblyBase_JsonDeserializer Instance = new SampleAssemblyBase_JsonDeserializer();

		public SampleAssemblyBase_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTestAssembly.SampleAssemblyBase>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTestAssembly.SampleAssemblyBase(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTestAssembly.SampleAssemblyBase)obj;
			if ("P" == name) {
				result.P = checked((short)RequireInt());
				name = GetNextName(false);
			}
			return result;
		}
	}

	class SampleAssemblyDerivedQ_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleAssemblyDerivedQ_JsonDeserializer Instance = new SampleAssemblyDerivedQ_JsonDeserializer();

		public SampleAssemblyDerivedQ_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTestAssembly.SampleAssemblyDerivedQ>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTestAssembly.SampleAssemblyDerivedQ(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTestAssembly.SampleAssemblyDerivedQ)obj;
			if ("P" == name) {
				result.P = checked((short)RequireInt());
				name = GetNextName(false);
			}
			if ("Q" == name) {
				result.Q = checked((short)RequireInt());
				name = GetNextName(false);
			}
			return result;
		}
	}

}

namespace YuzuGen.YuzuTest2
{
	class SampleNamespace_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleNamespace_JsonDeserializer Instance = new SampleNamespace_JsonDeserializer();

		public SampleNamespace_JsonDeserializer()
		{
			Options.AllowUnknownFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = false;
			JsonOptions.MaxOnelineFields = 0;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.DecimalAsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
			JsonOptions.Unordered = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::YuzuTest2.SampleNamespace>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::YuzuTest2.SampleNamespace(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::YuzuTest2.SampleNamespace)obj;
			if ("B" != name) throw new YuzuException("B!=" + name);
			result.B = YuzuGen.YuzuTest.SampleBase_JsonDeserializer.Instance.FromReaderTyped<global::YuzuTest.SampleBase>(Reader);
			name = GetNextName(false);
			return result;
		}
	}

}
