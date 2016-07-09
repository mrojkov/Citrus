using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Yuzu;
using Yuzu.Json;

namespace YuzuTest
{

	class Sample1_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Sample1_JsonDeserializer Instance = new Sample1_JsonDeserializer();

		public Sample1_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<Sample1>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new Sample1(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (Sample1)obj;
			if ("X" != name) throw new YuzuException("X!=" + name);
			result.X = RequireInt();
			name = GetNextName(false);
			if ("Y" == name) {
				result.Y = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Sample2_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Sample2_JsonDeserializer Instance = new Sample2_JsonDeserializer();

		public Sample2_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<Sample2>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new Sample2(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (Sample2)obj;
			if ("X" != name) throw new YuzuException("X!=" + name);
			result.X = RequireInt();
			name = GetNextName(false);
			if ("Y" == name) {
				result.Y = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Sample3_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Sample3_JsonDeserializer Instance = new Sample3_JsonDeserializer();

		public Sample3_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<Sample3>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new Sample3(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (Sample3)obj;
			if ("S1" != name) throw new YuzuException("S1!=" + name);
			result.S1 = Sample1_JsonDeserializer.Instance.FromReaderTyped<Sample1>(Reader);
			name = GetNextName(false);
			if ("F" == name) {
				result.F = RequireInt();
				name = GetNextName(false);
			}
			if ("S2" == name) {
				result.S2 = Sample2_JsonDeserializer.Instance.FromReaderTyped<Sample2>(Reader);
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Sample4_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Sample4_JsonDeserializer Instance = new Sample4_JsonDeserializer();

		public Sample4_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<Sample4>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new Sample4(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (Sample4)obj;
			if ("E" == name) {
				result.E = (SampleEnum)Enum.Parse(typeof(SampleEnum), RequireString());
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class SampleBool_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleBool_JsonDeserializer Instance = new SampleBool_JsonDeserializer();

		public SampleBool_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<SampleBool>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleBool(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleBool)obj;
			if ("B" != name) throw new YuzuException("B!=" + name);
			result.B = RequireBool();
			name = GetNextName(false);
			Require('}');
			return result;
		}
	}

	class SampleList_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleList_JsonDeserializer Instance = new SampleList_JsonDeserializer();

		public SampleList_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<SampleList>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleList(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleList)obj;
			if ("E" != name) throw new YuzuException("E!=" + name);
			result.E = RequireOrNull('[') ? null : new List<String>();
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
			Require('}');
			return result;
		}
	}

	class SampleDict_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleDict_JsonDeserializer Instance = new SampleDict_JsonDeserializer();

		public SampleDict_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<SampleDict>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleDict(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleDict)obj;
			if ("Value" != name) throw new YuzuException("Value!=" + name);
			result.Value = RequireInt();
			name = GetNextName(false);
			if ("Children" == name) {
				result.Children = RequireOrNull('{') ? null : new Dictionary<String,SampleDict>();
				if (result.Children != null) {
					if (SkipSpacesCarefully() == '}') {
						Require('}');
					}
					else {
						do {
							var tmp1 = RequireString();
							Require(':');
							var tmp2 = SampleDict_JsonDeserializer.Instance.FromReaderTyped<SampleDict>(Reader);
							result.Children.Add(tmp1, tmp2);
						} while (Require('}', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class SampleDictKeys_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleDictKeys_JsonDeserializer Instance = new SampleDictKeys_JsonDeserializer();

		public SampleDictKeys_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = false;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<SampleDictKeys>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleDictKeys(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleDictKeys)obj;
			if ("E" != name) throw new YuzuException("E!=" + name);
			result.E = RequireOrNull('{') ? null : new Dictionary<SampleEnum,Int32>();
			if (result.E != null) {
				if (SkipSpacesCarefully() == '}') {
					Require('}');
				}
				else {
					do {
						var tmp1 = RequireString();
						Require(':');
						var tmp2 = RequireInt();
						result.E.Add((SampleEnum)Enum.Parse(typeof(SampleEnum), tmp1), tmp2);
					} while (Require('}', ',') == ',');
				}
			}
			name = GetNextName(false);
			if ("I" != name) throw new YuzuException("I!=" + name);
			result.I = RequireOrNull('{') ? null : new Dictionary<Int32,Int32>();
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
			result.K = RequireOrNull('{') ? null : new Dictionary<SampleKey,Int32>();
			if (result.K != null) {
				if (SkipSpacesCarefully() == '}') {
					Require('}');
				}
				else {
					do {
						var tmp5 = RequireString();
						Require(':');
						var tmp6 = RequireInt();
						result.K.Add((SampleKey)keyParsers[typeof(SampleKey)](tmp5), tmp6);
					} while (Require('}', ',') == ',');
				}
			}
			name = GetNextName(false);
			Require('}');
			return result;
		}
	}

	class SampleArray_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleArray_JsonDeserializer Instance = new SampleArray_JsonDeserializer();

		public SampleArray_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<SampleArray>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleArray(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleArray)obj;
			if ("A" != name) throw new YuzuException("A!=" + name);
			result.A = RequireOrNull('[') ? null : new String[0];
			if (result.A != null) {
				if (SkipSpacesCarefully() != ']') {
					var tmp1 = new String[RequireUInt()];
					for(int tmp2 = 0; tmp2 < tmp1.Length; ++tmp2) {
						Require(',');
						tmp1[tmp2] = RequireString();
					}
					result.A = tmp1;
				}
				Require(']');
			}
			name = GetNextName(false);
			Require('}');
			return result;
		}
	}

	class SampleBase_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleBase_JsonDeserializer Instance = new SampleBase_JsonDeserializer();

		public SampleBase_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<SampleBase>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleBase(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleBase)obj;
			if ("FBase" != name) throw new YuzuException("FBase!=" + name);
			result.FBase = RequireInt();
			name = GetNextName(false);
			Require('}');
			return result;
		}
	}

	class SampleDerivedA_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleDerivedA_JsonDeserializer Instance = new SampleDerivedA_JsonDeserializer();

		public SampleDerivedA_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<SampleDerivedA>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleDerivedA(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleDerivedA)obj;
			if ("FBase" != name) throw new YuzuException("FBase!=" + name);
			result.FBase = RequireInt();
			name = GetNextName(false);
			if ("FA" != name) throw new YuzuException("FA!=" + name);
			result.FA = RequireInt();
			name = GetNextName(false);
			Require('}');
			return result;
		}
	}

	class SampleDerivedB_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleDerivedB_JsonDeserializer Instance = new SampleDerivedB_JsonDeserializer();

		public SampleDerivedB_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<SampleDerivedB>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleDerivedB(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleDerivedB)obj;
			if ("FBase" != name) throw new YuzuException("FBase!=" + name);
			result.FBase = RequireInt();
			name = GetNextName(false);
			if ("FB" != name) throw new YuzuException("FB!=" + name);
			result.FB = RequireInt();
			name = GetNextName(false);
			Require('}');
			return result;
		}
	}

	class SampleMatrix_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleMatrix_JsonDeserializer Instance = new SampleMatrix_JsonDeserializer();

		public SampleMatrix_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<SampleMatrix>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleMatrix(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleMatrix)obj;
			if ("M" != name) throw new YuzuException("M!=" + name);
			result.M = RequireOrNull('[') ? null : new List<List<Int32>>();
			if (result.M != null) {
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp1 = RequireOrNull('[') ? null : new List<Int32>();
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
			Require('}');
			return result;
		}
	}

	class SamplePoint_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SamplePoint_JsonDeserializer Instance = new SamplePoint_JsonDeserializer();

		public SamplePoint_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<SamplePoint>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SamplePoint(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SamplePoint)obj;
			if ("X" != name) throw new YuzuException("X!=" + name);
			result.X = RequireInt();
			name = GetNextName(false);
			if ("Y" != name) throw new YuzuException("Y!=" + name);
			result.Y = RequireInt();
			name = GetNextName(false);
			Require('}');
			return result;
		}

		protected override object ReadFieldsCompact(object obj)
		{
			var result = (SamplePoint)obj;
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
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<SampleRect>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleRect(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleRect)obj;
			if ("A" != name) throw new YuzuException("A!=" + name);
			result.A = SamplePoint_JsonDeserializer.Instance.FromReaderTyped<SamplePoint>(Reader);
			name = GetNextName(false);
			if ("B" != name) throw new YuzuException("B!=" + name);
			result.B = SamplePoint_JsonDeserializer.Instance.FromReaderTyped<SamplePoint>(Reader);
			name = GetNextName(false);
			Require('}');
			return result;
		}
	}

	class SampleDate_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleDate_JsonDeserializer Instance = new SampleDate_JsonDeserializer();

		public SampleDate_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<SampleDate>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleDate(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleDate)obj;
			if ("D" != name) throw new YuzuException("D!=" + name);
			result.D = RequireDateTime();
			name = GetNextName(false);
			if ("T" != name) throw new YuzuException("T!=" + name);
			result.T = RequireTimeSpan();
			name = GetNextName(false);
			Require('}');
			return result;
		}
	}

	class Color_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Color_JsonDeserializer Instance = new Color_JsonDeserializer();

		public Color_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<Color>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new Color(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (Color)obj;
			if ("B" != name) throw new YuzuException("B!=" + name);
			result.B = checked((byte)RequireUInt());
			name = GetNextName(false);
			if ("G" != name) throw new YuzuException("G!=" + name);
			result.G = checked((byte)RequireUInt());
			name = GetNextName(false);
			if ("R" != name) throw new YuzuException("R!=" + name);
			result.R = checked((byte)RequireUInt());
			name = GetNextName(false);
			Require('}');
			return result;
		}

		protected override object ReadFieldsCompact(object obj)
		{
			var result = (Color)obj;
			result.B = checked((byte)RequireUInt());
			Require(',');
			result.G = checked((byte)RequireUInt());
			Require(',');
			result.R = checked((byte)RequireUInt());
			Require(']');
			return result;
		}
	}

	class List_List_Int32_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new List_List_Int32_JsonDeserializer Instance = new List_List_Int32_JsonDeserializer();

		public List_List_Int32_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new List<List<Int32>>());
		}

		public override object FromReaderInt(object obj)
		{
			var result = (List<List<Int32>>)obj;
			Require('[');
			if (SkipSpacesCarefully() == ']') {
				Require(']');
			}
			else {
				do {
					var tmp1 = RequireOrNull('[') ? null : new List<Int32>();
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
			return ReadFields(new List<List<Int32>>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (List<List<Int32>>)obj;
			return result;
		}
	}

	class SampleClassList_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleClassList_JsonDeserializer Instance = new SampleClassList_JsonDeserializer();

		public SampleClassList_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<SampleClassList>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleClassList(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleClassList)obj;
			if ("E" != name) throw new YuzuException("E!=" + name);
			result.E = RequireOrNull('[') ? null : new List<SampleBase>();
			if (result.E != null) {
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp1 = SampleBase_JsonDeserializer.Instance.FromReaderTyped<SampleBase>(Reader);
						result.E.Add(tmp1);
					} while (Require(']', ',') == ',');
				}
			}
			name = GetNextName(false);
			Require('}');
			return result;
		}
	}

	class SampleSmallTypes_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleSmallTypes_JsonDeserializer Instance = new SampleSmallTypes_JsonDeserializer();

		public SampleSmallTypes_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<SampleSmallTypes>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleSmallTypes(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleSmallTypes)obj;
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
			result.USh = checked((short)RequireInt());
			name = GetNextName(false);
			Require('}');
			return result;
		}
	}

	class SamplePerson_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SamplePerson_JsonDeserializer Instance = new SamplePerson_JsonDeserializer();

		public SamplePerson_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<SamplePerson>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SamplePerson(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SamplePerson)obj;
			if ("1" != name) throw new YuzuException("1!=" + name);
			result.Name = RequireString();
			name = GetNextName(false);
			if ("2" != name) throw new YuzuException("2!=" + name);
			result.Birth = RequireDateTime();
			name = GetNextName(false);
			if ("3" != name) throw new YuzuException("3!=" + name);
			result.Children = RequireOrNull('[') ? null : new List<SamplePerson>();
			if (result.Children != null) {
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp1 = SamplePerson_JsonDeserializer.Instance.FromReaderTyped<SamplePerson>(Reader);
						result.Children.Add(tmp1);
					} while (Require(']', ',') == ',');
				}
			}
			name = GetNextName(false);
			if ("4" != name) throw new YuzuException("4!=" + name);
			result.EyeColor = Color_JsonDeserializer.Instance.FromReaderTyped<Color>(Reader);
			name = GetNextName(false);
			Require('}');
			return result;
		}
	}

	class ISample_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new ISample_JsonDeserializer Instance = new ISample_JsonDeserializer();

		public ISample_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderInterface<ISample>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return null;
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (ISample)obj;
			Require('}');
			return result;
		}
	}

	class SampleInterfaced_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleInterfaced_JsonDeserializer Instance = new SampleInterfaced_JsonDeserializer();

		public SampleInterfaced_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<SampleInterfaced>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleInterfaced(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleInterfaced)obj;
			if ("X" != name) throw new YuzuException("X!=" + name);
			result.X = RequireInt();
			name = GetNextName(false);
			Require('}');
			return result;
		}
	}

	class SampleInterfaceField_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleInterfaceField_JsonDeserializer Instance = new SampleInterfaceField_JsonDeserializer();

		public SampleInterfaceField_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<SampleInterfaceField>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleInterfaceField(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleInterfaceField)obj;
			if ("I" != name) throw new YuzuException("I!=" + name);
			result.I = ISample_JsonDeserializer.Instance.FromReaderInterface<ISample>(Reader);
			name = GetNextName(false);
			Require('}');
			return result;
		}
	}

	class SampleCollection_Int32_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleCollection_Int32_JsonDeserializer Instance = new SampleCollection_Int32_JsonDeserializer();

		public SampleCollection_Int32_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new SampleCollection<Int32>());
		}

		public override object FromReaderInt(object obj)
		{
			var result = (SampleCollection<Int32>)obj;
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
			return ReadFields(new SampleCollection<Int32>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleCollection<Int32>)obj;
			return result;
		}
	}

	class SampleExplicitCollection_Int32_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleExplicitCollection_Int32_JsonDeserializer Instance = new SampleExplicitCollection_Int32_JsonDeserializer();

		public SampleExplicitCollection_Int32_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new SampleExplicitCollection<Int32>());
		}

		public override object FromReaderInt(object obj)
		{
			var result = (SampleExplicitCollection<Int32>)obj;
			Require('[');
			if (SkipSpacesCarefully() == ']') {
				Require(']');
			}
			else {
				do {
					var tmp2 = RequireInt();
					((ICollection<Int32>)result).Add(tmp2);
				} while (Require(']', ',') == ',');
			}
			return result;
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleExplicitCollection<Int32>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleExplicitCollection<Int32>)obj;
			return result;
		}
	}

	class SampleWithCollection_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleWithCollection_JsonDeserializer Instance = new SampleWithCollection_JsonDeserializer();

		public SampleWithCollection_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<SampleWithCollection>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleWithCollection(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleWithCollection)obj;
			if ("A" != name) throw new YuzuException("A!=" + name);
			result.A = RequireOrNull('[') ? null : new SampleCollection<ISample>();
			if (result.A != null) {
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp1 = ISample_JsonDeserializer.Instance.FromReaderInterface<ISample>(Reader);
						result.A.Add(tmp1);
					} while (Require(']', ',') == ',');
				}
			}
			name = GetNextName(false);
			if ("B" != name) throw new YuzuException("B!=" + name);
			result.B = RequireOrNull('[') ? null : new SampleCollection<Int32>();
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
			Require('}');
			return result;
		}
	}

	class SampleConcreteCollection_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleConcreteCollection_JsonDeserializer Instance = new SampleConcreteCollection_JsonDeserializer();

		public SampleConcreteCollection_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new SampleConcreteCollection());
		}

		public override object FromReaderInt(object obj)
		{
			var result = (SampleConcreteCollection)obj;
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
			return ReadFields(new SampleConcreteCollection(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleConcreteCollection)obj;
			return result;
		}
	}

	class SampleAfter2_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleAfter2_JsonDeserializer Instance = new SampleAfter2_JsonDeserializer();

		public SampleAfter2_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<SampleAfter2>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleAfter2(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleAfter2)obj;
			if ("X" != name) throw new YuzuException("X!=" + name);
			result.X = RequireString();
			name = GetNextName(false);
			Require('}');
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
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = true;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<SampleMerge>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleMerge(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleMerge)obj;
			if ("LI" != name) throw new YuzuException("LI!=" + name);
			Require('[');
			if (SkipSpacesCarefully() == ']') {
				Require(']');
			}
			else {
				do {
					var tmp1 = RequireInt();
					result.LI.Add(tmp1);
				} while (Require(']', ',') == ',');
			}
			name = GetNextName(false);
			if ("M" == name) {
				Sample1_JsonDeserializer.Instance.FromReader(result.M, Reader);
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

}
