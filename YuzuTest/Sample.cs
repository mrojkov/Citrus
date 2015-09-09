using System;
using System.Collections.Generic;
using System.Reflection;

using Yuzu;

namespace YuzuTest
{

	class Sample1_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Sample1_JsonDeserializer Instance = new Sample1_JsonDeserializer();

		public Sample1_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.ClassNames = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.EnumAsString = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new Sample1());
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new Sample1(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (Sample1)obj;
			if ("X" != name) throw new YuzuException();
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
			Options.ClassNames = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.EnumAsString = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new Sample2());
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new Sample2(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (Sample2)obj;
			if ("X" != name) throw new YuzuException();
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
			Options.ClassNames = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.EnumAsString = false;
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new Sample3());
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new Sample3(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (Sample3)obj;
			if ("S1" != name) throw new YuzuException();
			result.S1 = (Sample1)Sample1_JsonDeserializer.Instance.FromReader(new Sample1(), Reader);
			name = GetNextName(false);
			if ("F" == name) {
				result.F = RequireInt();
				name = GetNextName(false);
			}
			if ("S2" == name) {
				result.S2 = (Sample2)Sample2_JsonDeserializer.Instance.FromReader(new Sample2(), Reader);
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
			Options.ClassNames = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.EnumAsString = true;
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new Sample4());
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

	class SampleList_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleList_JsonDeserializer Instance = new SampleList_JsonDeserializer();

		public SampleList_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.ClassNames = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.EnumAsString = true;
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new SampleList());
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleList(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleList)obj;
			if ("E" != name) throw new YuzuException();
			result.E = new List<String>();
			Require('[');
			if (SkipSpacesCarefully() == ']') {
				Require(']');
			}
			else {
				do {
					var tmp1 = RequireString();
					result.E.Add(tmp1);
				} while (Require(']', ',') == ',');
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
			Options.ClassNames = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.EnumAsString = true;
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new SampleBase());
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleBase(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleBase)obj;
			if ("FBase" != name) throw new YuzuException();
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
			Options.ClassNames = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.EnumAsString = true;
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new SampleDerivedA());
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleDerivedA(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleDerivedA)obj;
			if ("FBase" != name) throw new YuzuException();
			result.FBase = RequireInt();
			name = GetNextName(false);
			if ("FA" != name) throw new YuzuException();
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
			Options.ClassNames = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.EnumAsString = true;
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new SampleDerivedB());
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleDerivedB(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleDerivedB)obj;
			if ("FBase" != name) throw new YuzuException();
			result.FBase = RequireInt();
			name = GetNextName(false);
			if ("FB" != name) throw new YuzuException();
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
			Options.ClassNames = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.EnumAsString = true;
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new SampleMatrix());
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleMatrix(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleMatrix)obj;
			if ("M" != name) throw new YuzuException();
			result.M = new List<List<Int32>>();
			Require('[');
			if (SkipSpacesCarefully() == ']') {
				Require(']');
			}
			else {
				do {
					var tmp1 = new List<Int32>();
					Require('[');
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp2 = RequireInt();
							tmp1.Add(tmp2);
						} while (Require(']', ',') == ',');
					}
					result.M.Add(tmp1);
				} while (Require(']', ',') == ',');
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
			Options.ClassNames = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.EnumAsString = true;
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new SamplePoint());
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SamplePoint(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SamplePoint)obj;
			if ("X" != name) throw new YuzuException();
			result.X = RequireInt();
			name = GetNextName(false);
			if ("Y" != name) throw new YuzuException();
			result.Y = RequireInt();
			name = GetNextName(false);
			Require('}');
			return result;
		}
	}

	class SampleRect_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleRect_JsonDeserializer Instance = new SampleRect_JsonDeserializer();

		public SampleRect_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.ClassNames = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.EnumAsString = true;
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new SampleRect());
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleRect(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleRect)obj;
			if ("A" != name) throw new YuzuException();
			result.A = (SamplePoint)SamplePoint_JsonDeserializer.Instance.FromReader(new SamplePoint(), Reader);
			name = GetNextName(false);
			if ("B" != name) throw new YuzuException();
			result.B = (SamplePoint)SamplePoint_JsonDeserializer.Instance.FromReader(new SamplePoint(), Reader);
			name = GetNextName(false);
			Require('}');
			return result;
		}
	}

	class SampleClassList_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleClassList_JsonDeserializer Instance = new SampleClassList_JsonDeserializer();

		public SampleClassList_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("YuzuTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Options.ClassNames = true;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.EnumAsString = true;
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new SampleClassList());
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new SampleClassList(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (SampleClassList)obj;
			if ("E" != name) throw new YuzuException();
			result.E = new List<SampleBase>();
			Require('[');
			if (SkipSpacesCarefully() == ']') {
				Require(']');
			}
			else {
				do {
					var tmp1 = (SampleBase)base.FromReaderInt();
					result.E.Add(tmp1);
				} while (Require(']', ',') == ',');
			}
			name = GetNextName(false);
			Require('}');
			return result;
		}
	}

}
