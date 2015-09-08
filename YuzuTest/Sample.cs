using System.Collections.Generic;

using Yuzu;

namespace YuzuTest
{

	class Sample1_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Sample1_JsonDeserializer Instance = new Sample1_JsonDeserializer();

		public Sample1_JsonDeserializer()
		{
			className = "YuzuTest.Sample1";
			Options.ClassNames = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
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
			Options.ClassNames = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
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
			Options.ClassNames = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
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

	class SampleList_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SampleList_JsonDeserializer Instance = new SampleList_JsonDeserializer();

		public SampleList_JsonDeserializer()
		{
			className = "YuzuTest.SampleList";
			Options.ClassNames = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
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
			result.E = new List<System.String>();
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

}
