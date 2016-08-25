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

		private object Make_Sample1(ClassDef def)
		{
			var result = new global::YuzuTest.Sample1();
			Read_Sample1(def, result);
			return result;
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

		private object Make_Sample2(ClassDef def)
		{
			var result = new global::YuzuTest.Sample2();
			Read_Sample2(def, result);
			return result;
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

		private object Make_Sample3(ClassDef def)
		{
			var result = new global::YuzuTest.Sample3();
			Read_Sample3(def, result);
			return result;
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

		private object Make_Sample4(ClassDef def)
		{
			var result = new global::YuzuTest.Sample4();
			Read_Sample4(def, result);
			return result;
		}

		private void Read_SampleObj(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleObj)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.F = ReadAny();
			fd = def.Fields[Reader.ReadInt16()];
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_SampleObj(ClassDef def)
		{
			var result = new global::YuzuTest.SampleObj();
			Read_SampleObj(def, result);
			return result;
		}

		private void Read_SampleDict(ClassDef def, object obj)
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

		private object Make_SampleDict(ClassDef def)
		{
			var result = new global::YuzuTest.SampleDict();
			Read_SampleDict(def, result);
			return result;
		}

		private void Read_SampleDictKeys(ClassDef def, object obj)
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

		private object Make_SampleDictKeys(ClassDef def)
		{
			var result = new global::YuzuTest.SampleDictKeys();
			Read_SampleDictKeys(def, result);
			return result;
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

		private object Make_SampleMemberI(ClassDef def)
		{
			var result = new global::YuzuTest.SampleMemberI();
			Read_SampleMemberI(def, result);
			return result;
		}

		private void Read_SampleArray(ClassDef def, object obj)
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

		private object Make_SampleArray(ClassDef def)
		{
			var result = new global::YuzuTest.SampleArray();
			Read_SampleArray(def, result);
			return result;
		}

		private void Read_SampleBase(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleBase)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.FBase = Reader.ReadInt32();
			fd = def.Fields[Reader.ReadInt16()];
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_SampleBase(ClassDef def)
		{
			var result = new global::YuzuTest.SampleBase();
			Read_SampleBase(def, result);
			return result;
		}

		private void Read_SampleDerivedA(ClassDef def, object obj)
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

		private object Make_SampleDerivedA(ClassDef def)
		{
			var result = new global::YuzuTest.SampleDerivedA();
			Read_SampleDerivedA(def, result);
			return result;
		}

		private void Read_SampleDerivedB(ClassDef def, object obj)
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

		private object Make_SampleDerivedB(ClassDef def)
		{
			var result = new global::YuzuTest.SampleDerivedB();
			Read_SampleDerivedB(def, result);
			return result;
		}

		private void Read_SampleMatrix(ClassDef def, object obj)
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

		private object Make_SampleMatrix(ClassDef def)
		{
			var result = new global::YuzuTest.SampleMatrix();
			Read_SampleMatrix(def, result);
			return result;
		}

		private object Make_SamplePoint(ClassDef def)
		{
			var result = new global::YuzuTest.SamplePoint();
			result.X = Reader.ReadInt32();
			result.Y = Reader.ReadInt32();
			return result;
		}

		private void Read_SampleRect(ClassDef def, object obj)
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

		private object Make_SampleRect(ClassDef def)
		{
			var result = new global::YuzuTest.SampleRect();
			Read_SampleRect(def, result);
			return result;
		}

		private void Read_Color(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.Color)obj;
			result.B = Reader.ReadByte();
			result.G = Reader.ReadByte();
			result.R = Reader.ReadByte();
		}

		private object Make_Color(ClassDef def)
		{
			var result = new global::YuzuTest.Color();
			Read_Color(def, result);
			return result;
		}

		private void Read_SampleClassList(ClassDef def, object obj)
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

		private object Make_SampleClassList(ClassDef def)
		{
			var result = new global::YuzuTest.SampleClassList();
			Read_SampleClassList(def, result);
			return result;
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

		private object Make_SampleSmallTypes(ClassDef def)
		{
			var result = new global::YuzuTest.SampleSmallTypes();
			Read_SampleSmallTypes(def, result);
			return result;
		}

		private void Read_SampleWithNullFieldCompact(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleWithNullFieldCompact)obj;
			result.N = (global::YuzuTest.Sample1)ReadObject<global::YuzuTest.Sample1>();
		}

		private object Make_SampleWithNullFieldCompact(ClassDef def)
		{
			var result = new global::YuzuTest.SampleWithNullFieldCompact();
			Read_SampleWithNullFieldCompact(def, result);
			return result;
		}

		private void Read_SampleNested__NestedClass(ClassDef def, object obj)
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

		private object Make_SampleNested__NestedClass(ClassDef def)
		{
			var result = new global::YuzuTest.SampleNested.NestedClass();
			Read_SampleNested__NestedClass(def, result);
			return result;
		}

		private void Read_SampleNested(ClassDef def, object obj)
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

		private object Make_SampleNested(ClassDef def)
		{
			var result = new global::YuzuTest.SampleNested();
			Read_SampleNested(def, result);
			return result;
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

		private object Make_SamplePerson(ClassDef def)
		{
			var result = new global::YuzuTest.SamplePerson();
			Read_SamplePerson(def, result);
			return result;
		}

		private void Read_SampleInterfaceField(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleInterfaceField)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.I = (global::YuzuTest.ISample)ReadObject<global::YuzuTest.ISample>();
			fd = def.Fields[Reader.ReadInt16()];
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_SampleInterfaceField(ClassDef def)
		{
			var result = new global::YuzuTest.SampleInterfaceField();
			Read_SampleInterfaceField(def, result);
			return result;
		}

		private void Read_SampleInterfacedGeneric_String(ClassDef def, object obj)
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

		private object Make_SampleInterfacedGeneric_String(ClassDef def)
		{
			var result = new global::YuzuTest.SampleInterfacedGeneric<global::System.String>();
			Read_SampleInterfacedGeneric_String(def, result);
			return result;
		}

		private void Read_SampleConcrete(ClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleConcrete)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.XX = Reader.ReadInt32();
			fd = def.Fields[Reader.ReadInt16()];
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_SampleConcrete(ClassDef def)
		{
			var result = new global::YuzuTest.SampleConcrete();
			Read_SampleConcrete(def, result);
			return result;
		}

		private void Read_SampleWithCollection(ClassDef def, object obj)
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

		private object Make_SampleWithCollection(ClassDef def)
		{
			var result = new global::YuzuTest.SampleWithCollection();
			Read_SampleWithCollection(def, result);
			return result;
		}

		private void Read_SampleAfter2(ClassDef def, object obj)
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

		private object Make_SampleAfter2(ClassDef def)
		{
			var result = new global::YuzuTest.SampleAfter2();
			Read_SampleAfter2(def, result);
			return result;
		}

		private void Read_SampleNamespace(ClassDef def, object obj)
		{
			var result = (global::YuzuTest2.SampleNamespace)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw Error("1!=" + fd.OurIndex);
			result.B = (global::YuzuTest.SampleBase)ReadObject<global::YuzuTest.SampleBase>();
			fd = def.Fields[Reader.ReadInt16()];
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_SampleNamespace(ClassDef def)
		{
			var result = new global::YuzuTest2.SampleNamespace();
			Read_SampleNamespace(def, result);
			return result;
		}

		public BinaryDeserializerGen()
		{
			readFieldsCache[typeof(global::YuzuTest.Sample1)] = Read_Sample1;
			readFieldsCache[typeof(global::YuzuTest.Sample2)] = Read_Sample2;
			readFieldsCache[typeof(global::YuzuTest.Sample3)] = Read_Sample3;
			readFieldsCache[typeof(global::YuzuTest.Sample4)] = Read_Sample4;
			readFieldsCache[typeof(global::YuzuTest.SampleObj)] = Read_SampleObj;
			readFieldsCache[typeof(global::YuzuTest.SampleDict)] = Read_SampleDict;
			readFieldsCache[typeof(global::YuzuTest.SampleDictKeys)] = Read_SampleDictKeys;
			readFieldsCache[typeof(global::YuzuTest.SampleMemberI)] = Read_SampleMemberI;
			readFieldsCache[typeof(global::YuzuTest.SampleArray)] = Read_SampleArray;
			readFieldsCache[typeof(global::YuzuTest.SampleBase)] = Read_SampleBase;
			readFieldsCache[typeof(global::YuzuTest.SampleDerivedA)] = Read_SampleDerivedA;
			readFieldsCache[typeof(global::YuzuTest.SampleDerivedB)] = Read_SampleDerivedB;
			readFieldsCache[typeof(global::YuzuTest.SampleMatrix)] = Read_SampleMatrix;
			readFieldsCache[typeof(global::YuzuTest.SampleRect)] = Read_SampleRect;
			readFieldsCache[typeof(global::YuzuTest.Color)] = Read_Color;
			readFieldsCache[typeof(global::YuzuTest.SampleClassList)] = Read_SampleClassList;
			readFieldsCache[typeof(global::YuzuTest.SampleSmallTypes)] = Read_SampleSmallTypes;
			readFieldsCache[typeof(global::YuzuTest.SampleWithNullFieldCompact)] = Read_SampleWithNullFieldCompact;
			readFieldsCache[typeof(global::YuzuTest.SampleNested.NestedClass)] = Read_SampleNested__NestedClass;
			readFieldsCache[typeof(global::YuzuTest.SampleNested)] = Read_SampleNested;
			readFieldsCache[typeof(global::YuzuTest.SamplePerson)] = Read_SamplePerson;
			readFieldsCache[typeof(global::YuzuTest.SampleInterfaceField)] = Read_SampleInterfaceField;
			readFieldsCache[typeof(global::YuzuTest.SampleInterfacedGeneric<global::System.String>)] = Read_SampleInterfacedGeneric_String;
			readFieldsCache[typeof(global::YuzuTest.SampleConcrete)] = Read_SampleConcrete;
			readFieldsCache[typeof(global::YuzuTest.SampleWithCollection)] = Read_SampleWithCollection;
			readFieldsCache[typeof(global::YuzuTest.SampleAfter2)] = Read_SampleAfter2;
			readFieldsCache[typeof(global::YuzuTest2.SampleNamespace)] = Read_SampleNamespace;
			makeCache[typeof(global::YuzuTest.Sample1)] = Make_Sample1;
			makeCache[typeof(global::YuzuTest.Sample2)] = Make_Sample2;
			makeCache[typeof(global::YuzuTest.Sample3)] = Make_Sample3;
			makeCache[typeof(global::YuzuTest.Sample4)] = Make_Sample4;
			makeCache[typeof(global::YuzuTest.SampleObj)] = Make_SampleObj;
			makeCache[typeof(global::YuzuTest.SampleDict)] = Make_SampleDict;
			makeCache[typeof(global::YuzuTest.SampleDictKeys)] = Make_SampleDictKeys;
			makeCache[typeof(global::YuzuTest.SampleMemberI)] = Make_SampleMemberI;
			makeCache[typeof(global::YuzuTest.SampleArray)] = Make_SampleArray;
			makeCache[typeof(global::YuzuTest.SampleBase)] = Make_SampleBase;
			makeCache[typeof(global::YuzuTest.SampleDerivedA)] = Make_SampleDerivedA;
			makeCache[typeof(global::YuzuTest.SampleDerivedB)] = Make_SampleDerivedB;
			makeCache[typeof(global::YuzuTest.SampleMatrix)] = Make_SampleMatrix;
			makeCache[typeof(global::YuzuTest.SamplePoint)] = Make_SamplePoint;
			makeCache[typeof(global::YuzuTest.SampleRect)] = Make_SampleRect;
			makeCache[typeof(global::YuzuTest.Color)] = Make_Color;
			makeCache[typeof(global::YuzuTest.SampleClassList)] = Make_SampleClassList;
			makeCache[typeof(global::YuzuTest.SampleSmallTypes)] = Make_SampleSmallTypes;
			makeCache[typeof(global::YuzuTest.SampleWithNullFieldCompact)] = Make_SampleWithNullFieldCompact;
			makeCache[typeof(global::YuzuTest.SampleNested.NestedClass)] = Make_SampleNested__NestedClass;
			makeCache[typeof(global::YuzuTest.SampleNested)] = Make_SampleNested;
			makeCache[typeof(global::YuzuTest.SamplePerson)] = Make_SamplePerson;
			makeCache[typeof(global::YuzuTest.SampleInterfaceField)] = Make_SampleInterfaceField;
			makeCache[typeof(global::YuzuTest.SampleInterfacedGeneric<global::System.String>)] = Make_SampleInterfacedGeneric_String;
			makeCache[typeof(global::YuzuTest.SampleConcrete)] = Make_SampleConcrete;
			makeCache[typeof(global::YuzuTest.SampleWithCollection)] = Make_SampleWithCollection;
			makeCache[typeof(global::YuzuTest.SampleAfter2)] = Make_SampleAfter2;
			makeCache[typeof(global::YuzuTest2.SampleNamespace)] = Make_SampleNamespace;
		}
	}
}
