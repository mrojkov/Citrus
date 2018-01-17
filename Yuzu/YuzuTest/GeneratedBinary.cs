using System;
using System.Reflection;

using Yuzu;
using Yuzu.Binary;

namespace YuzuGenBin
{
	public class BinaryDeserializerGen: BinaryDeserializerGenBase
	{
		private static void Read_YuzuTest__Sample1(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.Sample1)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.X = d.Reader.ReadInt32();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (2 == fd.OurIndex) {
				result.Y = d.Reader.ReadString();
				if (result.Y == "" && d.Reader.ReadBoolean()) result.Y = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__Sample1(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.Sample1();
			Read_YuzuTest__Sample1(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__Sample2(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.Sample2)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.X = d.Reader.ReadInt32();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (2 == fd.OurIndex) {
				result.Y = d.Reader.ReadString();
				if (result.Y == "" && d.Reader.ReadBoolean()) result.Y = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__Sample2(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.Sample2();
			Read_YuzuTest__Sample2(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__Sample3(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.Sample3)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.S1 = (global::YuzuTest.Sample1)dg.ReadObject<global::YuzuTest.Sample1>();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (2 == fd.OurIndex) {
				result.F = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.S2 = (global::YuzuTest.Sample2)dg.ReadObject<global::YuzuTest.Sample2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__Sample3(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.Sample3();
			Read_YuzuTest__Sample3(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__Sample4(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.Sample4)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.E = (global::YuzuTest.SampleEnum)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__Sample4(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.Sample4();
			Read_YuzuTest__Sample4(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleDecimal(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleDecimal)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.N = d.Reader.ReadDecimal();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleDecimal(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleDecimal();
			Read_YuzuTest__SampleDecimal(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleNullable(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleNullable)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.N = d.Reader.ReadBoolean() ? (global::System.Int32?)null : d.Reader.ReadInt32();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleNullable(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleNullable();
			Read_YuzuTest__SampleNullable(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleObj(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleObj)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.F = dg.ReadAny();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleObj(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleObj();
			Read_YuzuTest__SampleObj(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleDict(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleDict)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.Value = d.Reader.ReadInt32();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (2 == fd.OurIndex) {
				result.Children = (global::System.Collections.Generic.Dictionary<global::System.String,global::YuzuTest.SampleDict>)null;
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.Children = new global::System.Collections.Generic.Dictionary<global::System.String,global::YuzuTest.SampleDict>();
					while (--tmp1 >= 0) {
						var tmp2 = d.Reader.ReadString();
						if (tmp2 == "" && d.Reader.ReadBoolean()) tmp2 = null;
						var tmp3 = (global::YuzuTest.SampleDict)dg.ReadObject<global::YuzuTest.SampleDict>();
						result.Children.Add(tmp2, tmp3);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleDict(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleDict();
			Read_YuzuTest__SampleDict(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleDictKeys(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleDictKeys)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.E = (global::System.Collections.Generic.Dictionary<global::YuzuTest.SampleEnum,global::System.Int32>)null;
			var tmp1 = d.Reader.ReadInt32();
			if (tmp1 >= 0) {
				result.E = new global::System.Collections.Generic.Dictionary<global::YuzuTest.SampleEnum,global::System.Int32>();
				while (--tmp1 >= 0) {
					var tmp2 = (global::YuzuTest.SampleEnum)d.Reader.ReadInt32();
					var tmp3 = d.Reader.ReadInt32();
					result.E.Add(tmp2, tmp3);
				}
			}
			fd = def.Fields[d.Reader.ReadInt16()];
			if (2 != fd.OurIndex) throw dg.Error("2!=" + fd.OurIndex);
			result.I = (global::System.Collections.Generic.Dictionary<global::System.Int32,global::System.Int32>)null;
			var tmp4 = d.Reader.ReadInt32();
			if (tmp4 >= 0) {
				result.I = new global::System.Collections.Generic.Dictionary<global::System.Int32,global::System.Int32>();
				while (--tmp4 >= 0) {
					var tmp5 = d.Reader.ReadInt32();
					var tmp6 = d.Reader.ReadInt32();
					result.I.Add(tmp5, tmp6);
				}
			}
			fd = def.Fields[d.Reader.ReadInt16()];
			if (3 != fd.OurIndex) throw dg.Error("3!=" + fd.OurIndex);
			result.K = (global::System.Collections.Generic.Dictionary<global::YuzuTest.SampleKey,global::System.Int32>)null;
			var tmp7 = d.Reader.ReadInt32();
			if (tmp7 >= 0) {
				result.K = new global::System.Collections.Generic.Dictionary<global::YuzuTest.SampleKey,global::System.Int32>();
				while (--tmp7 >= 0) {
					var tmp8 = (global::YuzuTest.SampleKey)dg.ReadObject<global::YuzuTest.SampleKey>();
					var tmp9 = d.Reader.ReadInt32();
					result.K.Add(tmp8, tmp9);
				}
			}
			fd = def.Fields[d.Reader.ReadInt16()];
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleDictKeys(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleDictKeys();
			Read_YuzuTest__SampleDictKeys(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleMemberI(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleMemberI)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.X = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleMemberI(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleMemberI();
			Read_YuzuTest__SampleMemberI(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleArray(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleArray)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.A = (global::System.String[])null;
			var tmp1 = d.Reader.ReadInt32();
			if (tmp1 >= 0) {
				var tmp2 = new global::System.String[tmp1];
				for(tmp1 = 0; tmp1 < tmp2.Length; ++tmp1) {
					tmp2[tmp1] = d.Reader.ReadString();
					if (tmp2[tmp1] == "" && d.Reader.ReadBoolean()) tmp2[tmp1] = null;
				}
				result.A = tmp2;
			}
			fd = def.Fields[d.Reader.ReadInt16()];
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleArray(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleArray();
			Read_YuzuTest__SampleArray(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleArray2D(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleArray2D)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.A = (global::System.Int32[][])null;
			var tmp1 = d.Reader.ReadInt32();
			if (tmp1 >= 0) {
				var tmp2 = new global::System.Int32[tmp1][];
				for(tmp1 = 0; tmp1 < tmp2.Length; ++tmp1) {
					tmp2[tmp1] = (global::System.Int32[])null;
					var tmp3 = d.Reader.ReadInt32();
					if (tmp3 >= 0) {
						var tmp4 = new global::System.Int32[tmp3];
						for(tmp3 = 0; tmp3 < tmp4.Length; ++tmp3) {
							tmp4[tmp3] = d.Reader.ReadInt32();
						}
						tmp2[tmp1] = tmp4;
					}
				}
				result.A = tmp2;
			}
			fd = def.Fields[d.Reader.ReadInt16()];
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleArray2D(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleArray2D();
			Read_YuzuTest__SampleArray2D(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleBase(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleBase)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.FBase = d.Reader.ReadInt32();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleBase(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleBase();
			Read_YuzuTest__SampleBase(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleDerivedA(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleDerivedA)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.FBase = d.Reader.ReadInt32();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (2 != fd.OurIndex) throw dg.Error("2!=" + fd.OurIndex);
			result.FA = d.Reader.ReadInt32();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleDerivedA(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleDerivedA();
			Read_YuzuTest__SampleDerivedA(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleDerivedB(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleDerivedB)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.FBase = d.Reader.ReadInt32();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (2 != fd.OurIndex) throw dg.Error("2!=" + fd.OurIndex);
			result.FB = d.Reader.ReadInt32();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleDerivedB(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleDerivedB();
			Read_YuzuTest__SampleDerivedB(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleMatrix(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleMatrix)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.M = (global::System.Collections.Generic.List<global::System.Collections.Generic.List<global::System.Int32>>)null;
			var tmp1 = d.Reader.ReadInt32();
			if (tmp1 >= 0) {
				result.M = new global::System.Collections.Generic.List<global::System.Collections.Generic.List<global::System.Int32>>();
				while (--tmp1 >= 0) {
					var tmp2 = (global::System.Collections.Generic.List<global::System.Int32>)null;
					var tmp3 = d.Reader.ReadInt32();
					if (tmp3 >= 0) {
						tmp2 = new global::System.Collections.Generic.List<global::System.Int32>();
						while (--tmp3 >= 0) {
							var tmp4 = d.Reader.ReadInt32();
							tmp2.Add(tmp4);
						}
					}
					result.M.Add(tmp2);
				}
			}
			fd = def.Fields[d.Reader.ReadInt16()];
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleMatrix(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleMatrix();
			Read_YuzuTest__SampleMatrix(d, def, result);
			return result;
		}

		private static object Make_YuzuTest__SamplePoint(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SamplePoint();
			result.X = d.Reader.ReadInt32();
			result.Y = d.Reader.ReadInt32();
			return result;
		}

		private static void Read_YuzuTest__SampleRect(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleRect)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			dg.EnsureClassDef(typeof(global::YuzuTest.SamplePoint));
			result.A.X = d.Reader.ReadInt32();
			result.A.Y = d.Reader.ReadInt32();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (2 != fd.OurIndex) throw dg.Error("2!=" + fd.OurIndex);
			dg.EnsureClassDef(typeof(global::YuzuTest.SamplePoint));
			result.B.X = d.Reader.ReadInt32();
			result.B.Y = d.Reader.ReadInt32();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleRect(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleRect();
			Read_YuzuTest__SampleRect(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleDefault(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleDefault)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.A = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.B = d.Reader.ReadString();
				if (result.B == "" && d.Reader.ReadBoolean()) result.B = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::YuzuTest.SamplePoint));
				result.P.X = d.Reader.ReadInt32();
				result.P.Y = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleDefault(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleDefault();
			Read_YuzuTest__SampleDefault(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__Color(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.Color)obj;
			result.B = d.Reader.ReadByte();
			result.G = d.Reader.ReadByte();
			result.R = d.Reader.ReadByte();
		}

		private static object Make_YuzuTest__Color(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.Color();
			Read_YuzuTest__Color(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleClassList(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleClassList)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.E = (global::System.Collections.Generic.List<global::YuzuTest.SampleBase>)null;
			var tmp1 = d.Reader.ReadInt32();
			if (tmp1 >= 0) {
				result.E = new global::System.Collections.Generic.List<global::YuzuTest.SampleBase>();
				while (--tmp1 >= 0) {
					var tmp2 = (global::YuzuTest.SampleBase)dg.ReadObject<global::YuzuTest.SampleBase>();
					result.E.Add(tmp2);
				}
			}
			fd = def.Fields[d.Reader.ReadInt16()];
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleClassList(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleClassList();
			Read_YuzuTest__SampleClassList(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleSmallTypes(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleSmallTypes)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.B = d.Reader.ReadByte();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (2 != fd.OurIndex) throw dg.Error("2!=" + fd.OurIndex);
			result.Ch = d.Reader.ReadChar();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (3 != fd.OurIndex) throw dg.Error("3!=" + fd.OurIndex);
			result.Sb = d.Reader.ReadSByte();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (4 != fd.OurIndex) throw dg.Error("4!=" + fd.OurIndex);
			result.Sh = d.Reader.ReadInt16();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (5 != fd.OurIndex) throw dg.Error("5!=" + fd.OurIndex);
			result.USh = d.Reader.ReadUInt16();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleSmallTypes(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleSmallTypes();
			Read_YuzuTest__SampleSmallTypes(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleWithNullFieldCompact(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleWithNullFieldCompact)obj;
			var dg = (BinaryDeserializerGen)d;
			result.N = (global::YuzuTest.Sample1)dg.ReadObject<global::YuzuTest.Sample1>();
		}

		private static object Make_YuzuTest__SampleWithNullFieldCompact(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleWithNullFieldCompact();
			Read_YuzuTest__SampleWithNullFieldCompact(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleNested__NestedClass(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleNested.NestedClass)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Z = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleNested__NestedClass(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleNested.NestedClass();
			Read_YuzuTest__SampleNested__NestedClass(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleNested(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleNested)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.C = (global::YuzuTest.SampleNested.NestedClass)dg.ReadObject<global::YuzuTest.SampleNested.NestedClass>();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (2 != fd.OurIndex) throw dg.Error("2!=" + fd.OurIndex);
			result.E = (global::YuzuTest.SampleNested.NestedEnum)d.Reader.ReadInt32();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (3 == fd.OurIndex) {
				result.Z = (global::YuzuTest.SampleNested.NestedEnum[])null;
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					var tmp2 = new global::YuzuTest.SampleNested.NestedEnum[tmp1];
					for(tmp1 = 0; tmp1 < tmp2.Length; ++tmp1) {
						tmp2[tmp1] = (global::YuzuTest.SampleNested.NestedEnum)d.Reader.ReadInt32();
					}
					result.Z = tmp2;
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleNested(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleNested();
			Read_YuzuTest__SampleNested(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SamplePerson(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SamplePerson)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.Name = d.Reader.ReadString();
			if (result.Name == "" && d.Reader.ReadBoolean()) result.Name = null;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (2 != fd.OurIndex) throw dg.Error("2!=" + fd.OurIndex);
			result.Birth = DateTime.FromBinary(d.Reader.ReadInt64());
			fd = def.Fields[d.Reader.ReadInt16()];
			if (3 != fd.OurIndex) throw dg.Error("3!=" + fd.OurIndex);
			result.Children = (global::System.Collections.Generic.List<global::YuzuTest.SamplePerson>)null;
			var tmp1 = d.Reader.ReadInt32();
			if (tmp1 >= 0) {
				result.Children = new global::System.Collections.Generic.List<global::YuzuTest.SamplePerson>();
				while (--tmp1 >= 0) {
					var tmp2 = (global::YuzuTest.SamplePerson)dg.ReadObject<global::YuzuTest.SamplePerson>();
					result.Children.Add(tmp2);
				}
			}
			fd = def.Fields[d.Reader.ReadInt16()];
			if (4 != fd.OurIndex) throw dg.Error("4!=" + fd.OurIndex);
			result.EyeColor = (global::YuzuTest.Color)dg.ReadObject<global::YuzuTest.Color>();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SamplePerson(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SamplePerson();
			Read_YuzuTest__SamplePerson(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleInterfaceField(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleInterfaceField)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.I = (global::YuzuTest.ISample)dg.ReadObject<global::YuzuTest.ISample>();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleInterfaceField(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleInterfaceField();
			Read_YuzuTest__SampleInterfaceField(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleInterfacedGeneric_String(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleInterfacedGeneric<global::System.String>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.G = d.Reader.ReadString();
			if (result.G == "" && d.Reader.ReadBoolean()) result.G = null;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (2 != fd.OurIndex) throw dg.Error("2!=" + fd.OurIndex);
			result.X = d.Reader.ReadInt32();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleInterfacedGeneric_String(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleInterfacedGeneric<global::System.String>();
			Read_YuzuTest__SampleInterfacedGeneric_String(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleConcrete(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleConcrete)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.XX = d.Reader.ReadInt32();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleConcrete(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleConcrete();
			Read_YuzuTest__SampleConcrete(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleWithCollection(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleWithCollection)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.A = (global::YuzuTest.SampleCollection<global::YuzuTest.ISample>)null;
			var tmp1 = d.Reader.ReadInt32();
			if (tmp1 >= 0) {
				result.A = new global::YuzuTest.SampleCollection<global::YuzuTest.ISample>();
				while (--tmp1 >= 0) {
					var tmp2 = (global::YuzuTest.ISample)dg.ReadObject<global::YuzuTest.ISample>();
					result.A.Add(tmp2);
				}
			}
			fd = def.Fields[d.Reader.ReadInt16()];
			if (2 != fd.OurIndex) throw dg.Error("2!=" + fd.OurIndex);
			result.B = (global::YuzuTest.SampleCollection<global::System.Int32>)null;
			var tmp3 = d.Reader.ReadInt32();
			if (tmp3 >= 0) {
				result.B = new global::YuzuTest.SampleCollection<global::System.Int32>();
				while (--tmp3 >= 0) {
					var tmp4 = d.Reader.ReadInt32();
					result.B.Add(tmp4);
				}
			}
			fd = def.Fields[d.Reader.ReadInt16()];
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleWithCollection(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleWithCollection();
			Read_YuzuTest__SampleWithCollection(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleAfter2(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleAfter2)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.X = d.Reader.ReadString();
			if (result.X == "" && d.Reader.ReadBoolean()) result.X = null;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
			result.After2();
			result.After3();
			result.After();
		}

		private static object Make_YuzuTest__SampleAfter2(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleAfter2();
			Read_YuzuTest__SampleAfter2(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleMerge(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleMerge)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			var tmp1 = d.Reader.ReadInt32();
			if (tmp1 >= 0) {
				while (--tmp1 >= 0) {
					var tmp2 = d.Reader.ReadInt32();
					var tmp3 = d.Reader.ReadInt32();
					result.DI.Add(tmp2, tmp3);
				}
			}
			fd = def.Fields[d.Reader.ReadInt16()];
			if (2 != fd.OurIndex) throw dg.Error("2!=" + fd.OurIndex);
			var tmp4 = d.Reader.ReadInt32();
			if (tmp4 >= 0) {
				while (--tmp4 >= 0) {
					var tmp5 = d.Reader.ReadInt32();
					result.LI.Add(tmp5);
				}
			}
			fd = def.Fields[d.Reader.ReadInt16()];
			if (3 == fd.OurIndex) {
				dg.ReadIntoObject<global::YuzuTest.Sample1>(result.M);
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleMerge(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleMerge();
			Read_YuzuTest__SampleMerge(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleAssemblyDerivedR(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleAssemblyDerivedR)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.P = d.Reader.ReadInt16();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.R = d.Reader.ReadString();
				if (result.R == "" && d.Reader.ReadBoolean()) result.R = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleAssemblyDerivedR(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleAssemblyDerivedR();
			Read_YuzuTest__SampleAssemblyDerivedR(d, def, result);
			return result;
		}

		private static object Make_YuzuTest__SampleAoS__Color(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleAoS.Color();
			result.B = d.Reader.ReadByte();
			result.G = d.Reader.ReadByte();
			result.R = d.Reader.ReadByte();
			return result;
		}

		private static object Make_YuzuTest__SampleAoS__Vertex(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleAoS.Vertex();
			result.X = d.Reader.ReadSingle();
			result.Y = d.Reader.ReadSingle();
			result.Z = d.Reader.ReadSingle();
			return result;
		}

		private static void Read_YuzuTest__SampleAoS__S(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleAoS.S)obj;
			var dg = (BinaryDeserializerGen)d;
			dg.EnsureClassDef(typeof(global::YuzuTest.SampleAoS.Color));
			result.C.B = d.Reader.ReadByte();
			result.C.G = d.Reader.ReadByte();
			result.C.R = d.Reader.ReadByte();
			dg.EnsureClassDef(typeof(global::YuzuTest.SampleAoS.Vertex));
			result.V.X = d.Reader.ReadSingle();
			result.V.Y = d.Reader.ReadSingle();
			result.V.Z = d.Reader.ReadSingle();
		}

		private static object Make_YuzuTest__SampleAoS__S(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleAoS.S();
			Read_YuzuTest__SampleAoS__S(d, def, result);
			return result;
		}

		private static void Read_YuzuTest__SampleAoS(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleAoS)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.A = (global::System.Collections.Generic.List<global::YuzuTest.SampleAoS.S>)null;
			var tmp1 = d.Reader.ReadInt32();
			if (tmp1 >= 0) {
				result.A = new global::System.Collections.Generic.List<global::YuzuTest.SampleAoS.S>();
				while (--tmp1 >= 0) {
					var tmp2 = (global::YuzuTest.SampleAoS.S)dg.ReadObject<global::YuzuTest.SampleAoS.S>();
					result.A.Add(tmp2);
				}
			}
			fd = def.Fields[d.Reader.ReadInt16()];
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleAoS(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleAoS();
			Read_YuzuTest__SampleAoS(d, def, result);
			return result;
		}

		private static object Make_YuzuTest__SampleStructWithProps(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleStructWithProps();
			var dg = (BinaryDeserializerGen)d;
			result.A = d.Reader.ReadInt32();
			result.P = (global::YuzuTest.SamplePoint)dg.ReadStruct<global::YuzuTest.SamplePoint>();
			return result;
		}

		private static void Read_YuzuTest__SampleAliasMany(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest.SampleAliasMany)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.X = d.Reader.ReadInt32();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest__SampleAliasMany(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest.SampleAliasMany();
			Read_YuzuTest__SampleAliasMany(d, def, result);
			return result;
		}

		private static void Read_YuzuTestAssembly__SampleAssemblyBase(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTestAssembly.SampleAssemblyBase)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.P = d.Reader.ReadInt16();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTestAssembly__SampleAssemblyBase(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTestAssembly.SampleAssemblyBase();
			Read_YuzuTestAssembly__SampleAssemblyBase(d, def, result);
			return result;
		}

		private static void Read_YuzuTestAssembly__SampleAssemblyDerivedQ(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTestAssembly.SampleAssemblyDerivedQ)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.P = d.Reader.ReadInt16();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Q = d.Reader.ReadInt16();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTestAssembly__SampleAssemblyDerivedQ(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTestAssembly.SampleAssemblyDerivedQ();
			Read_YuzuTestAssembly__SampleAssemblyDerivedQ(d, def, result);
			return result;
		}

		private static void Read_YuzuTest2__SampleNamespace(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::YuzuTest2.SampleNamespace)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 != fd.OurIndex) throw dg.Error("1!=" + fd.OurIndex);
			result.B = (global::YuzuTest.SampleBase)dg.ReadObject<global::YuzuTest.SampleBase>();
			fd = def.Fields[d.Reader.ReadInt16()];
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_YuzuTest2__SampleNamespace(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::YuzuTest2.SampleNamespace();
			Read_YuzuTest2__SampleNamespace(d, def, result);
			return result;
		}

		static BinaryDeserializerGen()
		{
			readCache[typeof(global::YuzuTest.Sample1)] = Read_YuzuTest__Sample1;
			readCache[typeof(global::YuzuTest.Sample2)] = Read_YuzuTest__Sample2;
			readCache[typeof(global::YuzuTest.Sample3)] = Read_YuzuTest__Sample3;
			readCache[typeof(global::YuzuTest.Sample4)] = Read_YuzuTest__Sample4;
			readCache[typeof(global::YuzuTest.SampleDecimal)] = Read_YuzuTest__SampleDecimal;
			readCache[typeof(global::YuzuTest.SampleNullable)] = Read_YuzuTest__SampleNullable;
			readCache[typeof(global::YuzuTest.SampleObj)] = Read_YuzuTest__SampleObj;
			readCache[typeof(global::YuzuTest.SampleDict)] = Read_YuzuTest__SampleDict;
			readCache[typeof(global::YuzuTest.SampleDictKeys)] = Read_YuzuTest__SampleDictKeys;
			readCache[typeof(global::YuzuTest.SampleMemberI)] = Read_YuzuTest__SampleMemberI;
			readCache[typeof(global::YuzuTest.SampleArray)] = Read_YuzuTest__SampleArray;
			readCache[typeof(global::YuzuTest.SampleArray2D)] = Read_YuzuTest__SampleArray2D;
			readCache[typeof(global::YuzuTest.SampleBase)] = Read_YuzuTest__SampleBase;
			readCache[typeof(global::YuzuTest.SampleDerivedA)] = Read_YuzuTest__SampleDerivedA;
			readCache[typeof(global::YuzuTest.SampleDerivedB)] = Read_YuzuTest__SampleDerivedB;
			readCache[typeof(global::YuzuTest.SampleMatrix)] = Read_YuzuTest__SampleMatrix;
			readCache[typeof(global::YuzuTest.SampleRect)] = Read_YuzuTest__SampleRect;
			readCache[typeof(global::YuzuTest.SampleDefault)] = Read_YuzuTest__SampleDefault;
			readCache[typeof(global::YuzuTest.Color)] = Read_YuzuTest__Color;
			readCache[typeof(global::YuzuTest.SampleClassList)] = Read_YuzuTest__SampleClassList;
			readCache[typeof(global::YuzuTest.SampleSmallTypes)] = Read_YuzuTest__SampleSmallTypes;
			readCache[typeof(global::YuzuTest.SampleWithNullFieldCompact)] = Read_YuzuTest__SampleWithNullFieldCompact;
			readCache[typeof(global::YuzuTest.SampleNested.NestedClass)] = Read_YuzuTest__SampleNested__NestedClass;
			readCache[typeof(global::YuzuTest.SampleNested)] = Read_YuzuTest__SampleNested;
			readCache[typeof(global::YuzuTest.SamplePerson)] = Read_YuzuTest__SamplePerson;
			readCache[typeof(global::YuzuTest.SampleInterfaceField)] = Read_YuzuTest__SampleInterfaceField;
			readCache[typeof(global::YuzuTest.SampleInterfacedGeneric<global::System.String>)] = Read_YuzuTest__SampleInterfacedGeneric_String;
			readCache[typeof(global::YuzuTest.SampleConcrete)] = Read_YuzuTest__SampleConcrete;
			readCache[typeof(global::YuzuTest.SampleWithCollection)] = Read_YuzuTest__SampleWithCollection;
			readCache[typeof(global::YuzuTest.SampleAfter2)] = Read_YuzuTest__SampleAfter2;
			readCache[typeof(global::YuzuTest.SampleMerge)] = Read_YuzuTest__SampleMerge;
			readCache[typeof(global::YuzuTest.SampleAssemblyDerivedR)] = Read_YuzuTest__SampleAssemblyDerivedR;
			readCache[typeof(global::YuzuTest.SampleAoS.S)] = Read_YuzuTest__SampleAoS__S;
			readCache[typeof(global::YuzuTest.SampleAoS)] = Read_YuzuTest__SampleAoS;
			readCache[typeof(global::YuzuTest.SampleAliasMany)] = Read_YuzuTest__SampleAliasMany;
			readCache[typeof(global::YuzuTestAssembly.SampleAssemblyBase)] = Read_YuzuTestAssembly__SampleAssemblyBase;
			readCache[typeof(global::YuzuTestAssembly.SampleAssemblyDerivedQ)] = Read_YuzuTestAssembly__SampleAssemblyDerivedQ;
			readCache[typeof(global::YuzuTest2.SampleNamespace)] = Read_YuzuTest2__SampleNamespace;
			makeCache[typeof(global::YuzuTest.Sample1)] = Make_YuzuTest__Sample1;
			makeCache[typeof(global::YuzuTest.Sample2)] = Make_YuzuTest__Sample2;
			makeCache[typeof(global::YuzuTest.Sample3)] = Make_YuzuTest__Sample3;
			makeCache[typeof(global::YuzuTest.Sample4)] = Make_YuzuTest__Sample4;
			makeCache[typeof(global::YuzuTest.SampleDecimal)] = Make_YuzuTest__SampleDecimal;
			makeCache[typeof(global::YuzuTest.SampleNullable)] = Make_YuzuTest__SampleNullable;
			makeCache[typeof(global::YuzuTest.SampleObj)] = Make_YuzuTest__SampleObj;
			makeCache[typeof(global::YuzuTest.SampleDict)] = Make_YuzuTest__SampleDict;
			makeCache[typeof(global::YuzuTest.SampleDictKeys)] = Make_YuzuTest__SampleDictKeys;
			makeCache[typeof(global::YuzuTest.SampleMemberI)] = Make_YuzuTest__SampleMemberI;
			makeCache[typeof(global::YuzuTest.SampleArray)] = Make_YuzuTest__SampleArray;
			makeCache[typeof(global::YuzuTest.SampleArray2D)] = Make_YuzuTest__SampleArray2D;
			makeCache[typeof(global::YuzuTest.SampleBase)] = Make_YuzuTest__SampleBase;
			makeCache[typeof(global::YuzuTest.SampleDerivedA)] = Make_YuzuTest__SampleDerivedA;
			makeCache[typeof(global::YuzuTest.SampleDerivedB)] = Make_YuzuTest__SampleDerivedB;
			makeCache[typeof(global::YuzuTest.SampleMatrix)] = Make_YuzuTest__SampleMatrix;
			makeCache[typeof(global::YuzuTest.SamplePoint)] = Make_YuzuTest__SamplePoint;
			makeCache[typeof(global::YuzuTest.SampleRect)] = Make_YuzuTest__SampleRect;
			makeCache[typeof(global::YuzuTest.SampleDefault)] = Make_YuzuTest__SampleDefault;
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
			makeCache[typeof(global::YuzuTest.SampleMerge)] = Make_YuzuTest__SampleMerge;
			makeCache[typeof(global::YuzuTest.SampleAssemblyDerivedR)] = Make_YuzuTest__SampleAssemblyDerivedR;
			makeCache[typeof(global::YuzuTest.SampleAoS.Color)] = Make_YuzuTest__SampleAoS__Color;
			makeCache[typeof(global::YuzuTest.SampleAoS.Vertex)] = Make_YuzuTest__SampleAoS__Vertex;
			makeCache[typeof(global::YuzuTest.SampleAoS.S)] = Make_YuzuTest__SampleAoS__S;
			makeCache[typeof(global::YuzuTest.SampleAoS)] = Make_YuzuTest__SampleAoS;
			makeCache[typeof(global::YuzuTest.SampleStructWithProps)] = Make_YuzuTest__SampleStructWithProps;
			makeCache[typeof(global::YuzuTest.SampleAliasMany)] = Make_YuzuTest__SampleAliasMany;
			makeCache[typeof(global::YuzuTestAssembly.SampleAssemblyBase)] = Make_YuzuTestAssembly__SampleAssemblyBase;
			makeCache[typeof(global::YuzuTestAssembly.SampleAssemblyDerivedQ)] = Make_YuzuTestAssembly__SampleAssemblyDerivedQ;
			makeCache[typeof(global::YuzuTest2.SampleNamespace)] = Make_YuzuTest2__SampleNamespace;
		}
	}
}
