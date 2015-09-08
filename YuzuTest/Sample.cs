using Yuzu;

namespace YuzuTest
{

	class Sample1_JsonDeserializer : JsonDeserializerGenBase
	{
		public Sample1_JsonDeserializer()
		{
			Options.ClassNames = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new Sample1());
		}

		public override object FromReaderInt(object obj)
		{
			buf = null;
			Require('{');
			var name = GetNextName(true);
			ReadFields(obj, name);
			return obj;
		}

		public override object FromReaderIntPartial(string name)
		{
			var obj = new Sample1();
			ReadFields(obj, name);
			return obj;
		}

		private new void ReadFields(object obj, string name)
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
		public Sample2_JsonDeserializer()
		{
			Options.ClassNames = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new Sample2());
		}

		public override object FromReaderInt(object obj)
		{
			buf = null;
			Require('{');
			var name = GetNextName(true);
			ReadFields(obj, name);
			return obj;
		}

		public override object FromReaderIntPartial(string name)
		{
			var obj = new Sample2();
			ReadFields(obj, name);
			return obj;
		}

		private new void ReadFields(object obj, string name)
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
		public Sample3_JsonDeserializer()
		{
			Options.ClassNames = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
		}

		public override object FromReaderInt()
		{
			return FromReaderInt(new Sample3());
		}

		public override object FromReaderInt(object obj)
		{
			buf = null;
			Require('{');
			var name = GetNextName(true);
			ReadFields(obj, name);
			return obj;
		}

		public override object FromReaderIntPartial(string name)
		{
			var obj = new Sample3();
			ReadFields(obj, name);
			return obj;
		}

		private new void ReadFields(object obj, string name)
		{
			var result = (Sample3)obj;
			if ("S1" != name) throw new YuzuException();
			result.S1 = new Sample1();
			(new Sample1_JsonDeserializer()).FromReader(result.S1, Reader);
			name = GetNextName(false);
			if ("F" == name) {
				result.F = RequireInt();
				name = GetNextName(false);
			}
			if ("S2" == name) {
				result.S2 = new Sample2();
				(new Sample2_JsonDeserializer()).FromReader(result.S2, Reader);
				name = GetNextName(false);
			}
			Require('}');
		}
	}

}
