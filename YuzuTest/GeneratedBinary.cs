using System;
using System.Reflection;

using Yuzu;
using Yuzu.Binary;

namespace YuzuGenBin
{
	public class BinaryDeserializerGen: BinaryDeserializer
	{
		private void Read_YuzuTest__Sample1(ClassDef def, object obj)
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

		private object Make_YuzuTest__Sample1(ClassDef def)
		{
			var result = new global::YuzuTest.Sample1();
			Read_YuzuTest__Sample1(def, result);
			return result;
		}

		private void Read_YuzuTest__Sample2(ClassDef def, object obj)
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

		private object Make_YuzuTest__Sample2(ClassDef def)
		{
			var result = new global::YuzuTest.Sample2();
			Read_YuzuTest__Sample2(def, result);
			return result;
		}

		private void Read_YuzuTest__Sample3(ClassDef def, object obj)
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

		private object Make_YuzuTest__Sample3(ClassDef def)
		{
			var result = new global::YuzuTest.Sample3();
			Read_YuzuTest__Sample3(def, result);
			return result;
		}

		private void Read_YuzuTest__Sample4(ClassDef def, object obj)
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

		private object Make_YuzuTest__Sample4(ClassDef def)
		{
			var result = new global::YuzuTest.Sample4();
			Read_YuzuTest__Sample4(def, result);
			return result;
		}

		private void Read_YuzuTest__SampleObj(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleObj)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.F = ReadAny();
			fd = def.Fields[Reader.ReadInt16()];
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_YuzuTest__SampleObj(ClassDef def)
		{
			var result = new global::YuzuTest.SampleObj();
			Read_YuzuTest__SampleObj(def, result);
			return result;
		}

		private void Read_YuzuTest__SampleDict(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleDict)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.Value = Reader.ReadInt32();
			fd = def.Fields[Reader.ReadInt16()];
			if (2 == fd.OurIndex) {
				result.Children = (global::System.Collections.Generic.Dictionary<global::System.String,global::YuzuTest.SampleDict>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.Children = new global::System.Collections.Generic.Dictionary<global::System.String,global::YuzuTest.SampleDict>();
					while (--tmp1 >= 0) {
						var tmp2 = Reader.ReadString();
						if (tmp2 == "" && Reader.ReadBoolean()) tmp2 = null;
						var tmp3 = (global::YuzuTest.SampleDict)ReadObject<global::YuzuTest.SampleDict>();
						result.Children.Add(tmp2, tmp3);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_YuzuTest__SampleDict(ClassDef def)
		{
			var result = new global::YuzuTest.SampleDict();
			Read_YuzuTest__SampleDict(def, result);
			return result;
		}

		private void Read_YuzuTest__SampleDictKeys(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleDictKeys)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.E = (global::System.Collections.Generic.Dictionary<global::YuzuTest.SampleEnum,global::System.Int32>)null;
			var tmp1 = Reader.ReadInt32();
			if (tmp1 >= 0) {
				result.E = new global::System.Collections.Generic.Dictionary<global::YuzuTest.SampleEnum,global::System.Int32>();
				while (--tmp1 >= 0) {
					var tmp2 = (global::YuzuTest.SampleEnum)Reader.ReadInt32();
					var tmp3 = Reader.ReadInt32();
					result.E.Add(tmp2, tmp3);
				}
			}
			fd = def.Fields[Reader.ReadInt16()];
			if (2 != fd.OurIndex) throw Error("2!=" + fd.OurIndex);
			result.I = (global::System.Collections.Generic.Dictionary<global::System.Int32,global::System.Int32>)null;
			var tmp4 = Reader.ReadInt32();
			if (tmp4 >= 0) {
				result.I = new global::System.Collections.Generic.Dictionary<global::System.Int32,global::System.Int32>();
				while (--tmp4 >= 0) {
					var tmp5 = Reader.ReadInt32();
					var tmp6 = Reader.ReadInt32();
					result.I.Add(tmp5, tmp6);
				}
			}
			fd = def.Fields[Reader.ReadInt16()];
			if (3 != fd.OurIndex) throw Error("3!=" + fd.OurIndex);
			result.K = (global::System.Collections.Generic.Dictionary<global::YuzuTest.SampleKey,global::System.Int32>)null;
			var tmp7 = Reader.ReadInt32();
			if (tmp7 >= 0) {
				result.K = new global::System.Collections.Generic.Dictionary<global::YuzuTest.SampleKey,global::System.Int32>();
				while (--tmp7 >= 0) {
					var tmp8 = (global::YuzuTest.SampleKey)ReadObject<global::YuzuTest.SampleKey>();
					var tmp9 = Reader.ReadInt32();
					result.K.Add(tmp8, tmp9);
				}
			}
			fd = def.Fields[Reader.ReadInt16()];
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_YuzuTest__SampleDictKeys(ClassDef def)
		{
			var result = new global::YuzuTest.SampleDictKeys();
			Read_YuzuTest__SampleDictKeys(def, result);
			return result;
		}

		private void Read_YuzuTest__SampleMemberI(ClassDef def, object obj)
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

		private object Make_YuzuTest__SampleMemberI(ClassDef def)
		{
			var result = new global::YuzuTest.SampleMemberI();
			Read_YuzuTest__SampleMemberI(def, result);
			return result;
		}

		private void Read_YuzuTest__SampleArray(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleArray)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.A = (global::System.String[])null;
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

		private object Make_YuzuTest__SampleArray(ClassDef def)
		{
			var result = new global::YuzuTest.SampleArray();
			Read_YuzuTest__SampleArray(def, result);
			return result;
		}

		private void Read_YuzuTest__SampleBase(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleBase)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.FBase = Reader.ReadInt32();
			fd = def.Fields[Reader.ReadInt16()];
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_YuzuTest__SampleBase(ClassDef def)
		{
			var result = new global::YuzuTest.SampleBase();
			Read_YuzuTest__SampleBase(def, result);
			return result;
		}

		private void Read_YuzuTest__SampleDerivedA(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleDerivedA)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.FBase = Reader.ReadInt32();
			fd = def.Fields[Reader.ReadInt16()];
			if (2 != fd.OurIndex) throw Error("2!=" + fd.OurIndex);
			result.FA = Reader.ReadInt32();
			fd = def.Fields[Reader.ReadInt16()];
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_YuzuTest__SampleDerivedA(ClassDef def)
		{
			var result = new global::YuzuTest.SampleDerivedA();
			Read_YuzuTest__SampleDerivedA(def, result);
			return result;
		}

		private void Read_YuzuTest__SampleDerivedB(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleDerivedB)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.FBase = Reader.ReadInt32();
			fd = def.Fields[Reader.ReadInt16()];
			if (2 != fd.OurIndex) throw Error("2!=" + fd.OurIndex);
			result.FB = Reader.ReadInt32();
			fd = def.Fields[Reader.ReadInt16()];
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_YuzuTest__SampleDerivedB(ClassDef def)
		{
			var result = new global::YuzuTest.SampleDerivedB();
			Read_YuzuTest__SampleDerivedB(def, result);
			return result;
		}

		private void Read_YuzuTest__SampleMatrix(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleMatrix)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.M = (global::System.Collections.Generic.List<global::System.Collections.Generic.List<global::System.Int32>>)null;
			var tmp1 = Reader.ReadInt32();
			if (tmp1 >= 0) {
				result.M = new global::System.Collections.Generic.List<global::System.Collections.Generic.List<global::System.Int32>>();
				while (--tmp1 >= 0) {
					var tmp2 = (global::System.Collections.Generic.List<global::System.Int32>)null;
					var tmp3 = Reader.ReadInt32();
					if (tmp3 >= 0) {
						tmp2 = new global::System.Collections.Generic.List<global::System.Int32>();
						while (--tmp3 >= 0) {
							var tmp4 = Reader.ReadInt32();
							tmp2.Add(tmp4);
						}
					}
					result.M.Add(tmp2);
				}
			}
			fd = def.Fields[Reader.ReadInt16()];
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_YuzuTest__SampleMatrix(ClassDef def)
		{
			var result = new global::YuzuTest.SampleMatrix();
			Read_YuzuTest__SampleMatrix(def, result);
			return result;
		}

		private object Make_YuzuTest__SamplePoint(ClassDef def)
		{
			var result = new global::YuzuTest.SamplePoint();
			result.X = Reader.ReadInt32();
			result.Y = Reader.ReadInt32();
			return result;
		}

		private void Read_YuzuTest__SampleRect(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleRect)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.A = (global::YuzuTest.SamplePoint)ReadStruct<global::YuzuTest.SamplePoint>();
			fd = def.Fields[Reader.ReadInt16()];
			if (2 != fd.OurIndex) throw Error("2!=" + fd.OurIndex);
			result.B = (global::YuzuTest.SamplePoint)ReadStruct<global::YuzuTest.SamplePoint>();
			fd = def.Fields[Reader.ReadInt16()];
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_YuzuTest__SampleRect(ClassDef def)
		{
			var result = new global::YuzuTest.SampleRect();
			Read_YuzuTest__SampleRect(def, result);
			return result;
		}

		private void Read_YuzuTest__Color(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.Color)obj;
			result.B = Reader.ReadByte();
			result.G = Reader.ReadByte();
			result.R = Reader.ReadByte();
		}

		private object Make_YuzuTest__Color(ClassDef def)
		{
			var result = new global::YuzuTest.Color();
			Read_YuzuTest__Color(def, result);
			return result;
		}

		private void Read_YuzuTest__SampleClassList(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleClassList)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.E = (global::System.Collections.Generic.List<global::YuzuTest.SampleBase>)null;
			var tmp1 = Reader.ReadInt32();
			if (tmp1 >= 0) {
				result.E = new global::System.Collections.Generic.List<global::YuzuTest.SampleBase>();
				while (--tmp1 >= 0) {
					var tmp2 = (global::YuzuTest.SampleBase)ReadObject<global::YuzuTest.SampleBase>();
					result.E.Add(tmp2);
				}
			}
			fd = def.Fields[Reader.ReadInt16()];
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_YuzuTest__SampleClassList(ClassDef def)
		{
			var result = new global::YuzuTest.SampleClassList();
			Read_YuzuTest__SampleClassList(def, result);
			return result;
		}

		private void Read_YuzuTest__SampleSmallTypes(ClassDef def, object obj)
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

		private object Make_YuzuTest__SampleSmallTypes(ClassDef def)
		{
			var result = new global::YuzuTest.SampleSmallTypes();
			Read_YuzuTest__SampleSmallTypes(def, result);
			return result;
		}

		private void Read_YuzuTest__SampleWithNullFieldCompact(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleWithNullFieldCompact)obj;
			result.N = (global::YuzuTest.Sample1)ReadObject<global::YuzuTest.Sample1>();
		}

		private object Make_YuzuTest__SampleWithNullFieldCompact(ClassDef def)
		{
			var result = new global::YuzuTest.SampleWithNullFieldCompact();
			Read_YuzuTest__SampleWithNullFieldCompact(def, result);
			return result;
		}

		private void Read_YuzuTest__SampleNested__NestedClass(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleNested.NestedClass)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Z = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_YuzuTest__SampleNested__NestedClass(ClassDef def)
		{
			var result = new global::YuzuTest.SampleNested.NestedClass();
			Read_YuzuTest__SampleNested__NestedClass(def, result);
			return result;
		}

		private void Read_YuzuTest__SampleNested(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleNested)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.C = (global::YuzuTest.SampleNested.NestedClass)ReadObject<global::YuzuTest.SampleNested.NestedClass>();
			fd = def.Fields[Reader.ReadInt16()];
			if (2 != fd.OurIndex) throw Error("2!=" + fd.OurIndex);
			result.E = (global::YuzuTest.SampleNested.NestedEnum)Reader.ReadInt32();
			fd = def.Fields[Reader.ReadInt16()];
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_YuzuTest__SampleNested(ClassDef def)
		{
			var result = new global::YuzuTest.SampleNested();
			Read_YuzuTest__SampleNested(def, result);
			return result;
		}

		private void Read_YuzuTest__SamplePerson(ClassDef def, object obj)
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
			result.Children = (global::System.Collections.Generic.List<global::YuzuTest.SamplePerson>)null;
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

		private object Make_YuzuTest__SamplePerson(ClassDef def)
		{
			var result = new global::YuzuTest.SamplePerson();
			Read_YuzuTest__SamplePerson(def, result);
			return result;
		}

		private void Read_YuzuTest__SampleInterfaceField(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleInterfaceField)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.I = (global::YuzuTest.ISample)ReadObject<global::YuzuTest.ISample>();
			fd = def.Fields[Reader.ReadInt16()];
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_YuzuTest__SampleInterfaceField(ClassDef def)
		{
			var result = new global::YuzuTest.SampleInterfaceField();
			Read_YuzuTest__SampleInterfaceField(def, result);
			return result;
		}

		private void Read_YuzuTest__SampleInterfacedGeneric_String(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleInterfacedGeneric<global::System.String>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.G = Reader.ReadString();
			if (result.G == "" && Reader.ReadBoolean()) result.G = null;
			fd = def.Fields[Reader.ReadInt16()];
			if (2 != fd.OurIndex) throw Error("2!=" + fd.OurIndex);
			result.X = Reader.ReadInt32();
			fd = def.Fields[Reader.ReadInt16()];
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_YuzuTest__SampleInterfacedGeneric_String(ClassDef def)
		{
			var result = new global::YuzuTest.SampleInterfacedGeneric<global::System.String>();
			Read_YuzuTest__SampleInterfacedGeneric_String(def, result);
			return result;
		}

		private void Read_YuzuTest__SampleConcrete(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleConcrete)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.XX = Reader.ReadInt32();
			fd = def.Fields[Reader.ReadInt16()];
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_YuzuTest__SampleConcrete(ClassDef def)
		{
			var result = new global::YuzuTest.SampleConcrete();
			Read_YuzuTest__SampleConcrete(def, result);
			return result;
		}

		private void Read_YuzuTest__SampleWithCollection(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleWithCollection)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.A = (global::YuzuTest.SampleCollection<global::YuzuTest.ISample>)null;
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
			result.B = (global::YuzuTest.SampleCollection<global::System.Int32>)null;
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

		private object Make_YuzuTest__SampleWithCollection(ClassDef def)
		{
			var result = new global::YuzuTest.SampleWithCollection();
			Read_YuzuTest__SampleWithCollection(def, result);
			return result;
		}

		private void Read_YuzuTest__SampleAfter2(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleAfter2)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.X = Reader.ReadString();
			if (result.X == "" && Reader.ReadBoolean()) result.X = null;
			fd = def.Fields[Reader.ReadInt16()];
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
			result.After2();
			result.After3();
			result.After();
		}

		private object Make_YuzuTest__SampleAfter2(ClassDef def)
		{
			var result = new global::YuzuTest.SampleAfter2();
			Read_YuzuTest__SampleAfter2(def, result);
			return result;
		}

		private void Read_YuzuTest2__SampleNamespace(ClassDef def, object obj)
		{
			var result = (global::YuzuTest2.SampleNamespace)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.B = (global::YuzuTest.SampleBase)ReadObject<global::YuzuTest.SampleBase>();
			fd = def.Fields[Reader.ReadInt16()];
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_YuzuTest2__SampleNamespace(ClassDef def)
		{
			var result = new global::YuzuTest2.SampleNamespace();
			Read_YuzuTest2__SampleNamespace(def, result);
			return result;
		}

		public BinaryDeserializerGen()
		{
			readFieldsCache[typeof(global::YuzuTest.Sample1)] = Read_YuzuTest__Sample1;
			readFieldsCache[typeof(global::YuzuTest.Sample2)] = Read_YuzuTest__Sample2;
			readFieldsCache[typeof(global::YuzuTest.Sample3)] = Read_YuzuTest__Sample3;
			readFieldsCache[typeof(global::YuzuTest.Sample4)] = Read_YuzuTest__Sample4;
			readFieldsCache[typeof(global::YuzuTest.SampleObj)] = Read_YuzuTest__SampleObj;
			readFieldsCache[typeof(global::YuzuTest.SampleDict)] = Read_YuzuTest__SampleDict;
			readFieldsCache[typeof(global::YuzuTest.SampleDictKeys)] = Read_YuzuTest__SampleDictKeys;
			readFieldsCache[typeof(global::YuzuTest.SampleMemberI)] = Read_YuzuTest__SampleMemberI;
			readFieldsCache[typeof(global::YuzuTest.SampleArray)] = Read_YuzuTest__SampleArray;
			readFieldsCache[typeof(global::YuzuTest.SampleBase)] = Read_YuzuTest__SampleBase;
			readFieldsCache[typeof(global::YuzuTest.SampleDerivedA)] = Read_YuzuTest__SampleDerivedA;
			readFieldsCache[typeof(global::YuzuTest.SampleDerivedB)] = Read_YuzuTest__SampleDerivedB;
			readFieldsCache[typeof(global::YuzuTest.SampleMatrix)] = Read_YuzuTest__SampleMatrix;
			readFieldsCache[typeof(global::YuzuTest.SampleRect)] = Read_YuzuTest__SampleRect;
			readFieldsCache[typeof(global::YuzuTest.Color)] = Read_YuzuTest__Color;
			readFieldsCache[typeof(global::YuzuTest.SampleClassList)] = Read_YuzuTest__SampleClassList;
			readFieldsCache[typeof(global::YuzuTest.SampleSmallTypes)] = Read_YuzuTest__SampleSmallTypes;
			readFieldsCache[typeof(global::YuzuTest.SampleWithNullFieldCompact)] = Read_YuzuTest__SampleWithNullFieldCompact;
			readFieldsCache[typeof(global::YuzuTest.SampleNested.NestedClass)] = Read_YuzuTest__SampleNested__NestedClass;
			readFieldsCache[typeof(global::YuzuTest.SampleNested)] = Read_YuzuTest__SampleNested;
			readFieldsCache[typeof(global::YuzuTest.SamplePerson)] = Read_YuzuTest__SamplePerson;
			readFieldsCache[typeof(global::YuzuTest.SampleInterfaceField)] = Read_YuzuTest__SampleInterfaceField;
			readFieldsCache[typeof(global::YuzuTest.SampleInterfacedGeneric<global::System.String>)] = Read_YuzuTest__SampleInterfacedGeneric_String;
			readFieldsCache[typeof(global::YuzuTest.SampleConcrete)] = Read_YuzuTest__SampleConcrete;
			readFieldsCache[typeof(global::YuzuTest.SampleWithCollection)] = Read_YuzuTest__SampleWithCollection;
			readFieldsCache[typeof(global::YuzuTest.SampleAfter2)] = Read_YuzuTest__SampleAfter2;
			readFieldsCache[typeof(global::YuzuTest2.SampleNamespace)] = Read_YuzuTest2__SampleNamespace;
			makeCache[typeof(global::YuzuTest.Sample1)] = Make_YuzuTest__Sample1;
			makeCache[typeof(global::YuzuTest.Sample2)] = Make_YuzuTest__Sample2;
			makeCache[typeof(global::YuzuTest.Sample3)] = Make_YuzuTest__Sample3;
			makeCache[typeof(global::YuzuTest.Sample4)] = Make_YuzuTest__Sample4;
			makeCache[typeof(global::YuzuTest.SampleObj)] = Make_YuzuTest__SampleObj;
			makeCache[typeof(global::YuzuTest.SampleDict)] = Make_YuzuTest__SampleDict;
			makeCache[typeof(global::YuzuTest.SampleDictKeys)] = Make_YuzuTest__SampleDictKeys;
			makeCache[typeof(global::YuzuTest.SampleMemberI)] = Make_YuzuTest__SampleMemberI;
			makeCache[typeof(global::YuzuTest.SampleArray)] = Make_YuzuTest__SampleArray;
			makeCache[typeof(global::YuzuTest.SampleBase)] = Make_YuzuTest__SampleBase;
			makeCache[typeof(global::YuzuTest.SampleDerivedA)] = Make_YuzuTest__SampleDerivedA;
			makeCache[typeof(global::YuzuTest.SampleDerivedB)] = Make_YuzuTest__SampleDerivedB;
			makeCache[typeof(global::YuzuTest.SampleMatrix)] = Make_YuzuTest__SampleMatrix;
			makeCache[typeof(global::YuzuTest.SamplePoint)] = Make_YuzuTest__SamplePoint;
			makeCache[typeof(global::YuzuTest.SampleRect)] = Make_YuzuTest__SampleRect;
			makeCache[typeof(global::YuzuTest.Color)] = Make_YuzuTest__Color;
			makeCache[typeof(global::YuzuTest.SampleClassList)] = Make_YuzuTest__SampleClassList;
			makeCache[typeof(global::YuzuTest.SampleSmallTypes)] = Make_YuzuTest__SampleSmallTypes;
			makeCache[typeof(global::YuzuTest.SampleWithNullFieldCompact)] = Make_YuzuTest__SampleWithNullFieldCompact;
			makeCache[typeof(global::YuzuTest.SampleNested.NestedClass)] = Make_YuzuTest__SampleNested__NestedClass;
			makeCache[typeof(global::YuzuTest.SampleNested)] = Make_YuzuTest__SampleNested;
			makeCache[typeof(global::YuzuTest.SamplePerson)] = Make_YuzuTest__SamplePerson;
			makeCache[typeof(global::YuzuTest.SampleInterfaceField)] = Make_YuzuTest__SampleInterfaceField;
			makeCache[typeof(global::YuzuTest.SampleInterfacedGeneric<global::System.String>)] = Make_YuzuTest__SampleInterfacedGeneric_String;
			makeCache[typeof(global::YuzuTest.SampleConcrete)] = Make_YuzuTest__SampleConcrete;
			makeCache[typeof(global::YuzuTest.SampleWithCollection)] = Make_YuzuTest__SampleWithCollection;
			makeCache[typeof(global::YuzuTest.SampleAfter2)] = Make_YuzuTest__SampleAfter2;
			makeCache[typeof(global::YuzuTest2.SampleNamespace)] = Make_YuzuTest2__SampleNamespace;
		}
	}
}
