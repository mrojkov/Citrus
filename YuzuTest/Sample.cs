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
			className = "YuzuTest.Sample1";
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
			var obj = new Sample1();
			ReadFields(obj, name);
			return obj;
		}

		protected override void ReadFields(object obj, string name)
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
		}
	}

	class Sample2_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Sample2_JsonDeserializer Instance = new Sample2_JsonDeserializer();

		public Sample2_JsonDeserializer()
		{
			className = "YuzuTest.Sample2";
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
			var obj = new Sample2();
			ReadFields(obj, name);
			return obj;
		}

		protected override void ReadFields(object obj, string name)
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
		}
	}

	class Sample3_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Sample3_JsonDeserializer Instance = new Sample3_JsonDeserializer();

		public Sample3_JsonDeserializer()
		{
			className = "YuzuTest.Sample3";
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
			var obj = new Sample3();
			ReadFields(obj, name);
			return obj;
		}

		protected override void ReadFields(object obj, string name)
		{
			var result = (Sample3)obj;
			if ("S1" != name) throw new YuzuException();
			result.S1 = new Sample1();
			Sample1_JsonDeserializer.Instance.FromReader(result.S1, Reader);
			name = GetNextName(false);
			if ("F" == name) {
				result.F = RequireInt();
				name = GetNextName(false);
			}
			if ("S2" == name) {
				result.S2 = new Sample2();
				Sample2_JsonDeserializer.Instance.FromReader(result.S2, Reader);
				name = GetNextName(false);
			}
			Require('}');
		}
	}

	class Sample4_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Sample4_JsonDeserializer Instance = new Sample4_JsonDeserializer();

		public Sample4_JsonDeserializer()
		{
			className = "YuzuTest.Sample4";
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
			var obj = new Sample4();
			ReadFields(obj, name);
			return obj;
		}

		protected override void ReadFields(object obj, string name)
		{
			var result = (Sample4)obj;
			if ("E" == name) {
				result.E = (SampleEnum)Enum.Parse(typeof(SampleEnum), RequireString());
				name = GetNextName(false);
			}
			Require('}');
		}
	}

	class SampleList_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleList_JsonDeserializer Instance = new SampleList_JsonDeserializer();

		public SampleList_JsonDeserializer()
		{
			className = "YuzuTest.SampleList";
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
			var obj = new SampleList();
			ReadFields(obj, name);
			return obj;
		}

		protected override void ReadFields(object obj, string name)
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
		}
	}

	class SampleBase_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleBase_JsonDeserializer Instance = new SampleBase_JsonDeserializer();

		public SampleBase_JsonDeserializer()
		{
			className = "YuzuTest.SampleBase";
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
			var obj = new SampleBase();
			ReadFields(obj, name);
			return obj;
		}

		protected override void ReadFields(object obj, string name)
		{
			var result = (SampleBase)obj;
			if ("FBase" != name) throw new YuzuException();
			result.FBase = RequireInt();
			name = GetNextName(false);
			Require('}');
		}
	}

	class SampleDerivedA_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleDerivedA_JsonDeserializer Instance = new SampleDerivedA_JsonDeserializer();

		public SampleDerivedA_JsonDeserializer()
		{
			className = "YuzuTest.SampleDerivedA";
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
			var obj = new SampleDerivedA();
			ReadFields(obj, name);
			return obj;
		}

		protected override void ReadFields(object obj, string name)
		{
			var result = (SampleDerivedA)obj;
			if ("FBase" != name) throw new YuzuException();
			result.FBase = RequireInt();
			name = GetNextName(false);
			if ("FA" != name) throw new YuzuException();
			result.FA = RequireInt();
			name = GetNextName(false);
			Require('}');
		}
	}

	class SampleDerivedB_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleDerivedB_JsonDeserializer Instance = new SampleDerivedB_JsonDeserializer();

		public SampleDerivedB_JsonDeserializer()
		{
			className = "YuzuTest.SampleDerivedB";
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
			var obj = new SampleDerivedB();
			ReadFields(obj, name);
			return obj;
		}

		protected override void ReadFields(object obj, string name)
		{
			var result = (SampleDerivedB)obj;
			if ("FBase" != name) throw new YuzuException();
			result.FBase = RequireInt();
			name = GetNextName(false);
			if ("FB" != name) throw new YuzuException();
			result.FB = RequireInt();
			name = GetNextName(false);
			Require('}');
		}
	}

	class SampleMatrix_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleMatrix_JsonDeserializer Instance = new SampleMatrix_JsonDeserializer();

		public SampleMatrix_JsonDeserializer()
		{
			className = "YuzuTest.SampleMatrix";
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
			var obj = new SampleMatrix();
			ReadFields(obj, name);
			return obj;
		}

		protected override void ReadFields(object obj, string name)
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
		}
	}

	class SampleClassList_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleClassList_JsonDeserializer Instance = new SampleClassList_JsonDeserializer();

		public SampleClassList_JsonDeserializer()
		{
			className = "YuzuTest.SampleClassList";
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
			var obj = new SampleClassList();
			ReadFields(obj, name);
			return obj;
		}

		protected override void ReadFields(object obj, string name)
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
		}
	}

}
