using System;
using System.Reflection;

using Yuzu;
using Yuzu.Binary;

namespace YuzuGenBin
{
	public class BinaryDeserializerGen: BinaryDeserializer
	{
		private void Read_Sample1(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.Sample1)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.X = Reader.ReadInt32();
			fd = def.Fields[Reader.ReadInt16()];
			if (2 == fd.OurIndex) {
				result.Y = Reader.ReadString();
				if (result.Y == "" && Reader.ReadBoolean()) result.Y = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private void Read_Sample2(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.Sample2)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.X = Reader.ReadInt32();
			fd = def.Fields[Reader.ReadInt16()];
			if (2 == fd.OurIndex) {
				result.Y = Reader.ReadString();
				if (result.Y == "" && Reader.ReadBoolean()) result.Y = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private void Read_Sample3(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.Sample3)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.S1 = (global::YuzuTest.Sample1)ReadObject<global::YuzuTest.Sample1>();
			fd = def.Fields[Reader.ReadInt16()];
			if (2 == fd.OurIndex) {
				result.F = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.S2 = (global::YuzuTest.Sample2)ReadObject<global::YuzuTest.Sample2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private void Read_Sample4(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.Sample4)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.E = (global::YuzuTest.SampleEnum)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private void Read_SampleMemberI(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleMemberI)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.X = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private void Read_SampleArray(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleArray)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.A = null;
			var tmp1 = Reader.ReadInt32();
			if (tmp1 >= 0) {
				var tmp2 = new global::System.String[tmp1];
				for(tmp1 = 0; tmp1 < tmp2.Length; ++tmp1) {
					tmp2[tmp1] = Reader.ReadString();
					if (tmp2[tmp1] == "" && Reader.ReadBoolean()) tmp2[tmp1] = null;
				}
				result.A = tmp2;
			}
			fd = def.Fields[Reader.ReadInt16()];
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private void Read_Color(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.Color)obj;
			result.B = Reader.ReadByte();
			result.G = Reader.ReadByte();
			result.R = Reader.ReadByte();
		}

		private void Read_SampleSmallTypes(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleSmallTypes)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.B = Reader.ReadByte();
			fd = def.Fields[Reader.ReadInt16()];
			if (2 != fd.OurIndex) throw Error("2!=" + fd.OurIndex);
			result.Ch = Reader.ReadChar();
			fd = def.Fields[Reader.ReadInt16()];
			if (3 != fd.OurIndex) throw Error("3!=" + fd.OurIndex);
			result.Sb = Reader.ReadSByte();
			fd = def.Fields[Reader.ReadInt16()];
			if (4 != fd.OurIndex) throw Error("4!=" + fd.OurIndex);
			result.Sh = Reader.ReadInt16();
			fd = def.Fields[Reader.ReadInt16()];
			if (5 != fd.OurIndex) throw Error("5!=" + fd.OurIndex);
			result.USh = Reader.ReadUInt16();
			fd = def.Fields[Reader.ReadInt16()];
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private void Read_SamplePerson(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SamplePerson)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.Name = Reader.ReadString();
			if (result.Name == "" && Reader.ReadBoolean()) result.Name = null;
			fd = def.Fields[Reader.ReadInt16()];
			if (2 != fd.OurIndex) throw Error("2!=" + fd.OurIndex);
			result.Birth = DateTime.FromBinary(Reader.ReadInt64());
			fd = def.Fields[Reader.ReadInt16()];
			if (3 != fd.OurIndex) throw Error("3!=" + fd.OurIndex);
			result.Children = null;
			var tmp1 = Reader.ReadInt32();
			if (tmp1 >= 0) {
				result.Children = new global::System.Collections.Generic.List<global::YuzuTest.SamplePerson>();
				while (--tmp1 >= 0) {
					var tmp2 = (global::YuzuTest.SamplePerson)ReadObject<global::YuzuTest.SamplePerson>();
					result.Children.Add(tmp2);
				}
			}
			fd = def.Fields[Reader.ReadInt16()];
			if (4 != fd.OurIndex) throw Error("4!=" + fd.OurIndex);
			result.EyeColor = (global::YuzuTest.Color)ReadObject<global::YuzuTest.Color>();
			fd = def.Fields[Reader.ReadInt16()];
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private void Read_SampleWithCollection(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleWithCollection)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.A = null;
			var tmp1 = Reader.ReadInt32();
			if (tmp1 >= 0) {
				result.A = new global::YuzuTest.SampleCollection<global::YuzuTest.ISample>();
				while (--tmp1 >= 0) {
					var tmp2 = (global::YuzuTest.ISample)ReadObject<global::YuzuTest.ISample>();
					result.A.Add(tmp2);
				}
			}
			fd = def.Fields[Reader.ReadInt16()];
			if (2 != fd.OurIndex) throw Error("2!=" + fd.OurIndex);
			result.B = null;
			var tmp3 = Reader.ReadInt32();
			if (tmp3 >= 0) {
				result.B = new global::YuzuTest.SampleCollection<global::System.Int32>();
				while (--tmp3 >= 0) {
					var tmp4 = Reader.ReadInt32();
					result.B.Add(tmp4);
				}
			}
			fd = def.Fields[Reader.ReadInt16()];
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		public BinaryDeserializerGen()
		{
			readFieldsCache[typeof(global::YuzuTest.Sample1)] = Read_Sample1;
			readFieldsCache[typeof(global::YuzuTest.Sample2)] = Read_Sample2;
			readFieldsCache[typeof(global::YuzuTest.Sample3)] = Read_Sample3;
			readFieldsCache[typeof(global::YuzuTest.Sample4)] = Read_Sample4;
			readFieldsCache[typeof(global::YuzuTest.SampleMemberI)] = Read_SampleMemberI;
			readFieldsCache[typeof(global::YuzuTest.SampleArray)] = Read_SampleArray;
			readFieldsCache[typeof(global::YuzuTest.Color)] = Read_Color;
			readFieldsCache[typeof(global::YuzuTest.SampleSmallTypes)] = Read_SampleSmallTypes;
			readFieldsCache[typeof(global::YuzuTest.SamplePerson)] = Read_SamplePerson;
			readFieldsCache[typeof(global::YuzuTest.SampleWithCollection)] = Read_SampleWithCollection;
		}
	}
}
