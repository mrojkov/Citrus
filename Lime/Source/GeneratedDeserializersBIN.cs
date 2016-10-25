using System;
using System.Reflection;

using Yuzu;
using Yuzu.Binary;

namespace GeneratedDeserializersBIN
{
	public class BinaryDeserializerGen: BinaryDeserializerGenBase
	{
		private static void Read_Lime__Font(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Font)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.About = d.Reader.ReadString();
				if (result.About == "" && d.Reader.ReadBoolean()) result.About = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.FontChar)dg.ReadObject<global::Lime.FontChar>();
						((global::System.Collections.Generic.ICollection<global::Lime.FontChar>)result.CharCollection).Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.ITexture)dg.ReadObject<global::Lime.ITexture>();
						result.Textures.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Font(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Font();
			Read_Lime__Font(d, def, result);
			return result;
		}

		private static void Read_Lime__SerializableSample(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.SerializableSample)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.SerializationPath = d.Reader.ReadString();
				if (result.SerializationPath == "" && d.Reader.ReadBoolean()) result.SerializationPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__SerializableSample(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.SerializableSample();
			Read_Lime__SerializableSample(d, def, result);
			return result;
		}

		private static object Make_Lime__KerningPair(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.KerningPair();
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Char = d.Reader.ReadChar();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Kerning = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
			return result;
		}

		private static void Read_Lime__FontChar(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.FontChar)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.ACWidths = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Char = d.Reader.ReadChar();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Height = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.KerningPairs = (global::System.Collections.Generic.List<global::Lime.KerningPair>)null;
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.KerningPairs = new global::System.Collections.Generic.List<global::Lime.KerningPair>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.KerningPair)dg.ReadStruct<global::Lime.KerningPair>();
						result.KerningPairs.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.TextureIndex = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.UV0 = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.UV1 = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.VerticalOffset = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Width = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__FontChar(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.FontChar();
			Read_Lime__FontChar(d, def, result);
			return result;
		}

		private static void Read_Lime__SerializableFont(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.SerializableFont)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Name = d.Reader.ReadString();
				if (result.Name == "" && d.Reader.ReadBoolean()) result.Name = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__SerializableFont(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.SerializableFont();
			Read_Lime__SerializableFont(d, def, result);
			return result;
		}

		private static object Make_Lime__BlendIndices(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.BlendIndices();
			result.Index0 = d.Reader.ReadByte();
			result.Index1 = d.Reader.ReadByte();
			result.Index2 = d.Reader.ReadByte();
			result.Index3 = d.Reader.ReadByte();
			return result;
		}

		private static object Make_Lime__BlendWeights(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.BlendWeights();
			result.Weight0 = d.Reader.ReadSingle();
			result.Weight1 = d.Reader.ReadSingle();
			result.Weight2 = d.Reader.ReadSingle();
			result.Weight3 = d.Reader.ReadSingle();
			return result;
		}

		private static void Read_Lime__GeometryBuffer(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.GeometryBuffer)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.BlendIndices = (global::Lime.BlendIndices[])null;
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					var tmp2 = new global::Lime.BlendIndices[tmp1];
					for(tmp1 = 0; tmp1 < tmp2.Length; ++tmp1) {
						tmp2[tmp1] = (global::Lime.BlendIndices)dg.ReadStruct<global::Lime.BlendIndices>();
					}
					result.BlendIndices = tmp2;
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.BlendWeights = (global::Lime.BlendWeights[])null;
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					var tmp4 = new global::Lime.BlendWeights[tmp3];
					for(tmp3 = 0; tmp3 < tmp4.Length; ++tmp3) {
						tmp4[tmp3] = (global::Lime.BlendWeights)dg.ReadStruct<global::Lime.BlendWeights>();
					}
					result.BlendWeights = tmp4;
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Colors = (global::Lime.Color4[])null;
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					var tmp6 = new global::Lime.Color4[tmp5];
					for(tmp5 = 0; tmp5 < tmp6.Length; ++tmp5) {
						tmp6[tmp5] = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
					}
					result.Colors = tmp6;
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Indices = (global::System.UInt16[])null;
				var tmp7 = d.Reader.ReadInt32();
				if (tmp7 >= 0) {
					var tmp8 = new global::System.UInt16[tmp7];
					for(tmp7 = 0; tmp7 < tmp8.Length; ++tmp7) {
						tmp8[tmp7] = d.Reader.ReadUInt16();
					}
					result.Indices = tmp8;
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.UV1 = (global::Lime.Vector2[])null;
				var tmp9 = d.Reader.ReadInt32();
				if (tmp9 >= 0) {
					var tmp10 = new global::Lime.Vector2[tmp9];
					for(tmp9 = 0; tmp9 < tmp10.Length; ++tmp9) {
						tmp10[tmp9] = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
					}
					result.UV1 = tmp10;
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.UV2 = (global::Lime.Vector2[])null;
				var tmp11 = d.Reader.ReadInt32();
				if (tmp11 >= 0) {
					var tmp12 = new global::Lime.Vector2[tmp11];
					for(tmp11 = 0; tmp11 < tmp12.Length; ++tmp11) {
						tmp12[tmp11] = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
					}
					result.UV2 = tmp12;
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.UV3 = (global::Lime.Vector2[])null;
				var tmp13 = d.Reader.ReadInt32();
				if (tmp13 >= 0) {
					var tmp14 = new global::Lime.Vector2[tmp13];
					for(tmp13 = 0; tmp13 < tmp14.Length; ++tmp13) {
						tmp14[tmp13] = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
					}
					result.UV3 = tmp14;
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.UV4 = (global::Lime.Vector2[])null;
				var tmp15 = d.Reader.ReadInt32();
				if (tmp15 >= 0) {
					var tmp16 = new global::Lime.Vector2[tmp15];
					for(tmp15 = 0; tmp15 < tmp16.Length; ++tmp15) {
						tmp16[tmp15] = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
					}
					result.UV4 = tmp16;
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Vertices = (global::Lime.Vector3[])null;
				var tmp17 = d.Reader.ReadInt32();
				if (tmp17 >= 0) {
					var tmp18 = new global::Lime.Vector3[tmp17];
					for(tmp17 = 0; tmp17 < tmp18.Length; ++tmp17) {
						tmp18[tmp17] = (global::Lime.Vector3)dg.ReadStruct<global::Lime.Vector3>();
					}
					result.Vertices = tmp18;
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
			result.AfterDeserialization();
		}

		private static object Make_Lime__GeometryBuffer(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.GeometryBuffer();
			Read_Lime__GeometryBuffer(d, def, result);
			return result;
		}

		private static object Make_Lime__TextureAtlasElement__Params(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.TextureAtlasElement.Params();
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AtlasPath = d.Reader.ReadString();
				if (result.AtlasPath == "" && d.Reader.ReadBoolean()) result.AtlasPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.AtlasRect = (global::Lime.IntRectangle)dg.ReadStruct<global::Lime.IntRectangle>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
			return result;
		}

		private static object Make_Lime__BitSet32(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.BitSet32();
			result.Value = d.Reader.ReadUInt32();
			return result;
		}

		private static object Make_Lime__BoundingSphere(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.BoundingSphere();
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Center = (global::Lime.Vector3)dg.ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Radius = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
			return result;
		}

		private static object Make_Lime__Color4(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Color4();
			result.ABGR = d.Reader.ReadUInt32();
			return result;
		}

		private static object Make_Lime__IntRectangle(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.IntRectangle();
			var dg = (BinaryDeserializerGen)d;
			result.A = (global::Lime.IntVector2)dg.ReadStruct<global::Lime.IntVector2>();
			result.B = (global::Lime.IntVector2)dg.ReadStruct<global::Lime.IntVector2>();
			return result;
		}

		private static object Make_Lime__IntVector2(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.IntVector2();
			result.X = d.Reader.ReadInt32();
			result.Y = d.Reader.ReadInt32();
			return result;
		}

		private static object Make_Lime__Matrix32(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Matrix32();
			var dg = (BinaryDeserializerGen)d;
			result.U = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
			result.V = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
			result.T = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
			return result;
		}

		private static object Make_Lime__Matrix44(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Matrix44();
			result.M11 = d.Reader.ReadSingle();
			result.M12 = d.Reader.ReadSingle();
			result.M13 = d.Reader.ReadSingle();
			result.M14 = d.Reader.ReadSingle();
			result.M21 = d.Reader.ReadSingle();
			result.M22 = d.Reader.ReadSingle();
			result.M23 = d.Reader.ReadSingle();
			result.M24 = d.Reader.ReadSingle();
			result.M31 = d.Reader.ReadSingle();
			result.M32 = d.Reader.ReadSingle();
			result.M33 = d.Reader.ReadSingle();
			result.M34 = d.Reader.ReadSingle();
			result.M41 = d.Reader.ReadSingle();
			result.M42 = d.Reader.ReadSingle();
			result.M43 = d.Reader.ReadSingle();
			result.M44 = d.Reader.ReadSingle();
			return result;
		}

		private static object Make_Lime__NumericRange(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.NumericRange();
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Dispersion = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Median = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
			return result;
		}

		private static object Make_Lime__Plane(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Plane();
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.D = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Normal = (global::Lime.Vector3)dg.ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
			return result;
		}

		private static object Make_Lime__Quaternion(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Quaternion();
			result.X = d.Reader.ReadSingle();
			result.Y = d.Reader.ReadSingle();
			result.Z = d.Reader.ReadSingle();
			result.W = d.Reader.ReadSingle();
			return result;
		}

		private static object Make_Lime__Ray(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Ray();
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Direction = (global::Lime.Vector3)dg.ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Position = (global::Lime.Vector3)dg.ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
			return result;
		}

		private static object Make_Lime__Rectangle(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Rectangle();
			var dg = (BinaryDeserializerGen)d;
			result.A = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
			result.B = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
			return result;
		}

		private static object Make_Lime__Size(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Size();
			result.Width = d.Reader.ReadInt32();
			result.Height = d.Reader.ReadInt32();
			return result;
		}

		private static object Make_Lime__Vector2(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Vector2();
			result.X = d.Reader.ReadSingle();
			result.Y = d.Reader.ReadSingle();
			return result;
		}

		private static object Make_Lime__Vector3(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Vector3();
			result.X = d.Reader.ReadSingle();
			result.Y = d.Reader.ReadSingle();
			result.Z = d.Reader.ReadSingle();
			return result;
		}

		private static object Make_Lime__Vector4(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Vector4();
			result.X = d.Reader.ReadSingle();
			result.Y = d.Reader.ReadSingle();
			result.Z = d.Reader.ReadSingle();
			result.W = d.Reader.ReadSingle();
			return result;
		}

		private static void Read_Lime__Camera3D(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Camera3D)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.AspectRatio = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.FarClipPlane = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.FieldOfView = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.NearClipPlane = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Position = (global::Lime.Vector3)dg.ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Rotation = (global::Lime.Quaternion)dg.ReadStruct<global::Lime.Quaternion>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector3)dg.ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Camera3D(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Camera3D();
			Read_Lime__Camera3D(d, def, result);
			return result;
		}

		private static void Read_Lime__Material(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Material)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.DiffuseColor = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.DiffuseTexture = (global::Lime.ITexture)dg.ReadObject<global::Lime.ITexture>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.EmissiveColor = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.FogColor = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.FogDensity = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.FogEnd = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.FogMode = (global::Lime.FogMode)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.FogStart = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Name = d.Reader.ReadString();
				if (result.Name == "" && d.Reader.ReadBoolean()) result.Name = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Opacity = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.OpacityTexture = (global::Lime.ITexture)dg.ReadObject<global::Lime.ITexture>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.SpecularColor = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.SpecularPower = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Material(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Material();
			Read_Lime__Material(d, def, result);
			return result;
		}

		private static void Read_Lime__Mesh3D(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Mesh3D)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.BoundingSphere = (global::Lime.BoundingSphere)dg.ReadStruct<global::Lime.BoundingSphere>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Position = (global::Lime.Vector3)dg.ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Rotation = (global::Lime.Quaternion)dg.ReadStruct<global::Lime.Quaternion>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector3)dg.ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				var tmp7 = d.Reader.ReadInt32();
				if (tmp7 >= 0) {
					while (--tmp7 >= 0) {
						var tmp8 = (global::Lime.Submesh3D)dg.ReadObject<global::Lime.Submesh3D>();
						result.Submeshes.Add(tmp8);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Mesh3D(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Mesh3D();
			Read_Lime__Mesh3D(d, def, result);
			return result;
		}

		private static void Read_Lime__Submesh3D(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Submesh3D)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Matrix44)dg.ReadStruct<global::Lime.Matrix44>();
						result.BoneBindPoses.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = d.Reader.ReadString();
						if (tmp4 == "" && d.Reader.ReadBoolean()) tmp4 = null;
						result.BoneNames.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				dg.ReadIntoObject<global::Lime.GeometryBuffer>(result.Geometry);
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Material = (global::Lime.Material)dg.ReadObject<global::Lime.Material>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Submesh3D(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Submesh3D();
			Read_Lime__Submesh3D(d, def, result);
			return result;
		}

		private static void Read_Lime__Node3D(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Node3D)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Position = (global::Lime.Vector3)dg.ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Rotation = (global::Lime.Quaternion)dg.ReadStruct<global::Lime.Quaternion>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector3)dg.ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Node3D(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Node3D();
			Read_Lime__Node3D(d, def, result);
			return result;
		}

		private static void Read_Lime__Spline3D(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Spline3D)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Closed = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Points = (global::System.Collections.Generic.List<global::Lime.Spline3D.Point>)null;
				var tmp7 = d.Reader.ReadInt32();
				if (tmp7 >= 0) {
					result.Points = new global::System.Collections.Generic.List<global::Lime.Spline3D.Point>();
					while (--tmp7 >= 0) {
						var tmp8 = (global::Lime.Spline3D.Point)dg.ReadObject<global::Lime.Spline3D.Point>();
						result.Points.Add(tmp8);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Position = (global::Lime.Vector3)dg.ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Rotation = (global::Lime.Quaternion)dg.ReadStruct<global::Lime.Quaternion>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector3)dg.ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Spline3D(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Spline3D();
			Read_Lime__Spline3D(d, def, result);
			return result;
		}

		private static void Read_Lime__Spline3D__Point(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Spline3D.Point)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.InterpolationMode = (global::Lime.Spline3D.InterpolationMode)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Position = (global::Lime.Vector3)dg.ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TangentA = (global::Lime.Vector3)dg.ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.TangentB = (global::Lime.Vector3)dg.ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Spline3D__Point(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Spline3D.Point();
			Read_Lime__Spline3D__Point(d, def, result);
			return result;
		}

		private static void Read_Lime__Viewport3D(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Viewport3D)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)dg.ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Frame = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Viewport3D(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Viewport3D();
			Read_Lime__Viewport3D(d, def, result);
			return result;
		}

		private static void Read_Lime__Animation(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animation)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Marker)dg.ReadObject<global::Lime.Marker>();
						result.Markers.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animation(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animation();
			Read_Lime__Animation(d, def, result);
			return result;
		}

		private static void Read_Lime__Spline(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Spline)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)dg.ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Spline(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Spline();
			Read_Lime__Spline(d, def, result);
			return result;
		}

		private static void Read_Lime__LinearLayout(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.LinearLayout)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Horizontal = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ProcessHidden = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__LinearLayout(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.LinearLayout();
			Read_Lime__LinearLayout(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_String(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::System.String>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::System.String>)dg.ReadObject<global::Lime.Keyframe<global::System.String>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_String(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::System.String>();
			Read_Lime__Animator_String(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_Int32(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::System.Int32>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::System.Int32>)dg.ReadObject<global::Lime.Keyframe<global::System.Int32>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_Int32(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::System.Int32>();
			Read_Lime__Animator_Int32(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_Boolean(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::System.Boolean>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::System.Boolean>)dg.ReadObject<global::Lime.Keyframe<global::System.Boolean>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_Boolean(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::System.Boolean>();
			Read_Lime__Animator_Boolean(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_Blending(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.Blending>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.Blending>)dg.ReadObject<global::Lime.Keyframe<global::Lime.Blending>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_Blending(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.Blending>();
			Read_Lime__Animator_Blending(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_ITexture(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.ITexture>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.ITexture>)dg.ReadObject<global::Lime.Keyframe<global::Lime.ITexture>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_ITexture(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.ITexture>();
			Read_Lime__Animator_ITexture(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_NumericRange(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.NumericRange>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.NumericRange>)dg.ReadObject<global::Lime.Keyframe<global::Lime.NumericRange>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_NumericRange(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.NumericRange>();
			Read_Lime__Animator_NumericRange(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_Vector2(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.Vector2>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.Vector2>)dg.ReadObject<global::Lime.Keyframe<global::Lime.Vector2>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_Vector2(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.Vector2>();
			Read_Lime__Animator_Vector2(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_Color4(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.Color4>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.Color4>)dg.ReadObject<global::Lime.Keyframe<global::Lime.Color4>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_Color4(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.Color4>();
			Read_Lime__Animator_Color4(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_Single(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::System.Single>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::System.Single>)dg.ReadObject<global::Lime.Keyframe<global::System.Single>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_Single(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::System.Single>();
			Read_Lime__Animator_Single(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_EmitterShape(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.EmitterShape>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.EmitterShape>)dg.ReadObject<global::Lime.Keyframe<global::Lime.EmitterShape>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_EmitterShape(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.EmitterShape>();
			Read_Lime__Animator_EmitterShape(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_AudioAction(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.AudioAction>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.AudioAction>)dg.ReadObject<global::Lime.Keyframe<global::Lime.AudioAction>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_AudioAction(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.AudioAction>();
			Read_Lime__Animator_AudioAction(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_SerializableSample(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.SerializableSample>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.SerializableSample>)dg.ReadObject<global::Lime.Keyframe<global::Lime.SerializableSample>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_SerializableSample(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.SerializableSample>();
			Read_Lime__Animator_SerializableSample(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_HAlignment(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.HAlignment>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.HAlignment>)dg.ReadObject<global::Lime.Keyframe<global::Lime.HAlignment>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_HAlignment(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.HAlignment>();
			Read_Lime__Animator_HAlignment(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_VAlignment(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.VAlignment>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.VAlignment>)dg.ReadObject<global::Lime.Keyframe<global::Lime.VAlignment>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_VAlignment(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.VAlignment>();
			Read_Lime__Animator_VAlignment(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_MovieAction(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.MovieAction>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.MovieAction>)dg.ReadObject<global::Lime.Keyframe<global::Lime.MovieAction>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_MovieAction(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.MovieAction>();
			Read_Lime__Animator_MovieAction(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_ShaderId(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.ShaderId>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.ShaderId>)dg.ReadObject<global::Lime.Keyframe<global::Lime.ShaderId>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_ShaderId(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.ShaderId>();
			Read_Lime__Animator_ShaderId(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_Vector3(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.Vector3>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.Vector3>)dg.ReadObject<global::Lime.Keyframe<global::Lime.Vector3>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_Vector3(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.Vector3>();
			Read_Lime__Animator_Vector3(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_Quaternion(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.Quaternion>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.Quaternion>)dg.ReadObject<global::Lime.Keyframe<global::Lime.Quaternion>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_Quaternion(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.Quaternion>();
			Read_Lime__Animator_Quaternion(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_EmissionType(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.EmissionType>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.EmissionType>)dg.ReadObject<global::Lime.Keyframe<global::Lime.EmissionType>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_EmissionType(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.EmissionType>();
			Read_Lime__Animator_EmissionType(d, def, result);
			return result;
		}

		private static void Read_Lime__NumericAnimator(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.NumericAnimator)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::System.Single>)dg.ReadObject<global::Lime.Keyframe<global::System.Single>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__NumericAnimator(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.NumericAnimator();
			Read_Lime__NumericAnimator(d, def, result);
			return result;
		}

		private static void Read_Lime__Vector2Animator(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Vector2Animator)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.Vector2>)dg.ReadObject<global::Lime.Keyframe<global::Lime.Vector2>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Vector2Animator(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Vector2Animator();
			Read_Lime__Vector2Animator(d, def, result);
			return result;
		}

		private static void Read_Lime__Color4Animator(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Color4Animator)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.Color4>)dg.ReadObject<global::Lime.Keyframe<global::Lime.Color4>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Color4Animator(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Color4Animator();
			Read_Lime__Color4Animator(d, def, result);
			return result;
		}

		private static void Read_Lime__QuaternionAnimator(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.QuaternionAnimator)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.Quaternion>)dg.ReadObject<global::Lime.Keyframe<global::Lime.Quaternion>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__QuaternionAnimator(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.QuaternionAnimator();
			Read_Lime__QuaternionAnimator(d, def, result);
			return result;
		}

		private static void Read_Lime__Vector3Animator(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Vector3Animator)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.Vector3>)dg.ReadObject<global::Lime.Keyframe<global::Lime.Vector3>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Vector3Animator(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Vector3Animator();
			Read_Lime__Vector3Animator(d, def, result);
			return result;
		}

		private static void Read_Lime__Matrix44Animator(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Matrix44Animator)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = d.Reader.ReadString();
				if (result.AnimationId == "" && d.Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.Matrix44>)dg.ReadObject<global::Lime.Keyframe<global::Lime.Matrix44>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = d.Reader.ReadString();
				if (result.TargetProperty == "" && d.Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Matrix44Animator(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Matrix44Animator();
			Read_Lime__Matrix44Animator(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_String(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::System.String>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = d.Reader.ReadString();
				if (result.Value == "" && d.Reader.ReadBoolean()) result.Value = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Keyframe_String(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::System.String>();
			Read_Lime__Keyframe_String(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_Int32(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::System.Int32>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Keyframe_Int32(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::System.Int32>();
			Read_Lime__Keyframe_Int32(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_Boolean(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::System.Boolean>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Keyframe_Boolean(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::System.Boolean>();
			Read_Lime__Keyframe_Boolean(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_Blending(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.Blending>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.Blending)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Keyframe_Blending(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.Blending>();
			Read_Lime__Keyframe_Blending(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_ITexture(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.ITexture>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.ITexture)dg.ReadObject<global::Lime.ITexture>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Keyframe_ITexture(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.ITexture>();
			Read_Lime__Keyframe_ITexture(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_NumericRange(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.NumericRange>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Keyframe_NumericRange(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.NumericRange>();
			Read_Lime__Keyframe_NumericRange(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_Vector2(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.Vector2>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Keyframe_Vector2(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.Vector2>();
			Read_Lime__Keyframe_Vector2(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_Color4(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.Color4>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Keyframe_Color4(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.Color4>();
			Read_Lime__Keyframe_Color4(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_Single(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::System.Single>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Keyframe_Single(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::System.Single>();
			Read_Lime__Keyframe_Single(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_EmitterShape(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.EmitterShape>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.EmitterShape)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Keyframe_EmitterShape(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.EmitterShape>();
			Read_Lime__Keyframe_EmitterShape(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_AudioAction(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.AudioAction>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.AudioAction)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Keyframe_AudioAction(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.AudioAction>();
			Read_Lime__Keyframe_AudioAction(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_SerializableSample(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.SerializableSample>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.SerializableSample)dg.ReadObject<global::Lime.SerializableSample>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Keyframe_SerializableSample(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.SerializableSample>();
			Read_Lime__Keyframe_SerializableSample(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_HAlignment(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.HAlignment>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.HAlignment)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Keyframe_HAlignment(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.HAlignment>();
			Read_Lime__Keyframe_HAlignment(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_VAlignment(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.VAlignment>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.VAlignment)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Keyframe_VAlignment(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.VAlignment>();
			Read_Lime__Keyframe_VAlignment(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_MovieAction(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.MovieAction>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.MovieAction)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Keyframe_MovieAction(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.MovieAction>();
			Read_Lime__Keyframe_MovieAction(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_ShaderId(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.ShaderId>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Keyframe_ShaderId(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.ShaderId>();
			Read_Lime__Keyframe_ShaderId(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_Vector3(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.Vector3>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.Vector3)dg.ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Keyframe_Vector3(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.Vector3>();
			Read_Lime__Keyframe_Vector3(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_Quaternion(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.Quaternion>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.Quaternion)dg.ReadStruct<global::Lime.Quaternion>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Keyframe_Quaternion(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.Quaternion>();
			Read_Lime__Keyframe_Quaternion(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_EmissionType(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.EmissionType>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.EmissionType)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Keyframe_EmissionType(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.EmissionType>();
			Read_Lime__Keyframe_EmissionType(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_Matrix44(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.Matrix44>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.Matrix44)dg.ReadStruct<global::Lime.Matrix44>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Keyframe_Matrix44(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.Matrix44>();
			Read_Lime__Keyframe_Matrix44(d, def, result);
			return result;
		}

		private static void Read_Lime__Audio(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Audio)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Bumpable = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.FadeTime = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Group = (global::Lime.AudioChannelGroup)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Looping = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Pan = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Pitch = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Priority = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Sample = (global::Lime.SerializableSample)dg.ReadObject<global::Lime.SerializableSample>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Volume = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Audio(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Audio();
			Read_Lime__Audio(d, def, result);
			return result;
		}

		private static object Make_Lime__BoneWeight(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.BoneWeight();
			result.Index = d.Reader.ReadInt32();
			result.Weight = d.Reader.ReadSingle();
			return result;
		}

		private static void Read_Lime__SkinningWeights(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.SkinningWeights)obj;
			var dg = (BinaryDeserializerGen)d;
			result.Bone0 = (global::Lime.BoneWeight)dg.ReadStruct<global::Lime.BoneWeight>();
			result.Bone1 = (global::Lime.BoneWeight)dg.ReadStruct<global::Lime.BoneWeight>();
			result.Bone2 = (global::Lime.BoneWeight)dg.ReadStruct<global::Lime.BoneWeight>();
			result.Bone3 = (global::Lime.BoneWeight)dg.ReadStruct<global::Lime.BoneWeight>();
		}

		private static object Make_Lime__SkinningWeights(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.SkinningWeights();
			Read_Lime__SkinningWeights(d, def, result);
			return result;
		}

		private static void Read_Lime__Bone(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Bone)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.BaseIndex = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.EffectiveRadius = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.FadeoutZone = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.IKStopper = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Index = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Length = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.RefLength = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.RefPosition = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.RefRotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Bone(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Bone();
			Read_Lime__Bone(d, def, result);
			return result;
		}

		private static object Make_Lime__BoneArray(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.BoneArray();
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.items = (global::Lime.BoneArray.Entry[])null;
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					var tmp2 = new global::Lime.BoneArray.Entry[tmp1];
					for(tmp1 = 0; tmp1 < tmp2.Length; ++tmp1) {
						tmp2[tmp1] = (global::Lime.BoneArray.Entry)dg.ReadStruct<global::Lime.BoneArray.Entry>();
					}
					result.items = tmp2;
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
			return result;
		}

		private static object Make_Lime__BoneArray__Entry(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.BoneArray.Entry();
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Joint = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Length = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.RelativeTransform = (global::Lime.Matrix32)dg.ReadStruct<global::Lime.Matrix32>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.Tip = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
			return result;
		}

		private static void Read_Lime__Button(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Button)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)dg.ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Draggable = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Enabled = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.Text = d.Reader.ReadString();
				if (result.Text == "" && d.Reader.ReadBoolean()) result.Text = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Button(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Button();
			Read_Lime__Button(d, def, result);
			return result;
		}

		private static void Read_Lime__DistortionMesh(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.DistortionMesh)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)dg.ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.NumCols = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.NumRows = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.Texture = (global::Lime.ITexture)dg.ReadObject<global::Lime.ITexture>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__DistortionMesh(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.DistortionMesh();
			Read_Lime__DistortionMesh(d, def, result);
			return result;
		}

		private static void Read_Lime__DistortionMeshPoint(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.DistortionMeshPoint)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Offset = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.UV = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__DistortionMeshPoint(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.DistortionMeshPoint();
			Read_Lime__DistortionMeshPoint(d, def, result);
			return result;
		}

		private static void Read_Lime__Frame(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Frame)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)dg.ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.RenderTarget = (global::Lime.RenderTarget)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Frame(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Frame();
			Read_Lime__Frame(d, def, result);
			return result;
		}

		private static void Read_Lime__Image(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Image)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)dg.ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Texture = (global::Lime.ITexture)dg.ReadObject<global::Lime.ITexture>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.UV0 = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.UV1 = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Image(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Image();
			Read_Lime__Image(d, def, result);
			return result;
		}

		private static void Read_Lime__ImageCombiner(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.ImageCombiner)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.Enabled = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__ImageCombiner(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.ImageCombiner();
			Read_Lime__ImageCombiner(d, def, result);
			return result;
		}

		private static void Read_Lime__Marker(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Marker)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Action = (global::Lime.MarkerAction)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Frame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.JumpTo = d.Reader.ReadString();
				if (result.JumpTo == "" && d.Reader.ReadBoolean()) result.JumpTo = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Marker(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Marker();
			Read_Lime__Marker(d, def, result);
			return result;
		}

		private static void Read_Lime__Movie(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Movie)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)dg.ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Looped = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Path = d.Reader.ReadString();
				if (result.Path == "" && d.Reader.ReadBoolean()) result.Path = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Movie(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Movie();
			Read_Lime__Movie(d, def, result);
			return result;
		}

		private static void Read_Lime__NineGrid(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.NineGrid)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)dg.ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.BottomOffset = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.LeftOffset = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.RightOffset = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Texture = (global::Lime.ITexture)dg.ReadObject<global::Lime.ITexture>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				result.TopOffset = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__NineGrid(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.NineGrid();
			Read_Lime__NineGrid(d, def, result);
			return result;
		}

		private static void Read_Lime__Node(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Node)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Node(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Node();
			Read_Lime__Node(d, def, result);
			return result;
		}

		private static void Read_Lime__ParticleEmitter(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.ParticleEmitter)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AlongPathOrientation = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.AngularVelocity = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.AspectRatio = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)dg.ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Direction = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.EmissionType = (global::Lime.EmissionType)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.GravityAmount = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.GravityDirection = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.ImmortalParticles = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.Lifetime = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.LinkageWidgetName = d.Reader.ReadString();
				if (result.LinkageWidgetName == "" && d.Reader.ReadBoolean()) result.LinkageWidgetName = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.MagnetAmount = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Number = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				result.Orientation = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
				result.ParticlesLinkage = (global::Lime.ParticlesLinkage)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (25 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (26 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (27 == fd.OurIndex) {
				result.RandomMotionAspectRatio = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (28 == fd.OurIndex) {
				result.RandomMotionRadius = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (29 == fd.OurIndex) {
				result.RandomMotionRotation = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (30 == fd.OurIndex) {
				result.RandomMotionSpeed = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (31 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (32 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (33 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (34 == fd.OurIndex) {
				result.Shape = (global::Lime.EmitterShape)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (35 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (36 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (37 == fd.OurIndex) {
				result.Speed = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (38 == fd.OurIndex) {
				result.Spin = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (39 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (40 == fd.OurIndex) {
				result.TimeShift = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (41 == fd.OurIndex) {
				result.Velocity = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (42 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (43 == fd.OurIndex) {
				result.WindAmount = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (44 == fd.OurIndex) {
				result.WindDirection = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (45 == fd.OurIndex) {
				result.Zoom = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__ParticleEmitter(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.ParticleEmitter();
			Read_Lime__ParticleEmitter(d, def, result);
			return result;
		}

		private static void Read_Lime__ParticleModifier(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.ParticleModifier)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AngularVelocity = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.AnimationFps = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.AspectRatio = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.FirstFrame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.GravityAmount = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.LastFrame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.LoopedAnimation = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.MagnetAmount = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Scale = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Spin = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.Texture = (global::Lime.ITexture)dg.ReadObject<global::Lime.ITexture>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Velocity = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.WindAmount = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__ParticleModifier(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.ParticleModifier();
			Read_Lime__ParticleModifier(d, def, result);
			return result;
		}

		private static void Read_Lime__ParticlesMagnet(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.ParticlesMagnet)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Attenuation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)dg.ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Shape = (global::Lime.EmitterShape)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.Strength = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__ParticlesMagnet(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.ParticlesMagnet();
			Read_Lime__ParticlesMagnet(d, def, result);
			return result;
		}

		private static void Read_Lime__PointObject(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.PointObject)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__PointObject(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.PointObject();
			Read_Lime__PointObject(d, def, result);
			return result;
		}

		private static void Read_Lime__Slider(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Slider)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)dg.ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.RangeMax = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.RangeMin = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.Step = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Value = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Slider(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Slider();
			Read_Lime__Slider(d, def, result);
			return result;
		}

		private static void Read_Lime__SplineGear(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.SplineGear)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.SplineId = d.Reader.ReadString();
				if (result.SplineId == "" && d.Reader.ReadBoolean()) result.SplineId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.SplineOffset = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.WidgetId = d.Reader.ReadString();
				if (result.WidgetId == "" && d.Reader.ReadBoolean()) result.WidgetId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__SplineGear(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.SplineGear();
			Read_Lime__SplineGear(d, def, result);
			return result;
		}

		private static void Read_Lime__SplinePoint(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.SplinePoint)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Straight = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.TangentAngle = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.TangentWeight = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__SplinePoint(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.SplinePoint();
			Read_Lime__SplinePoint(d, def, result);
			return result;
		}

		private static void Read_Lime__RichText(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.RichText)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)dg.ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.HAlignment = (global::Lime.HAlignment)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.OverflowMode = (global::Lime.TextOverflowMode)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.Text = d.Reader.ReadString();
				if (result.Text == "" && d.Reader.ReadBoolean()) result.Text = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.VAlignment = (global::Lime.VAlignment)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
				result.WordSplitAllowed = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__RichText(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.RichText();
			Read_Lime__RichText(d, def, result);
			return result;
		}

		private static void Read_Lime__SimpleText(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.SimpleText)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)dg.ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Font = (global::Lime.SerializableFont)dg.ReadObject<global::Lime.SerializableFont>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.FontHeight = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.HAlignment = (global::Lime.HAlignment)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.OverflowMode = (global::Lime.TextOverflowMode)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Spacing = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
				result.Text = d.Reader.ReadString();
				if (result.Text == "" && d.Reader.ReadBoolean()) result.Text = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (25 == fd.OurIndex) {
				result.TextColor = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (26 == fd.OurIndex) {
				result.VAlignment = (global::Lime.VAlignment)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (27 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (28 == fd.OurIndex) {
				result.WordSplitAllowed = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__SimpleText(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.SimpleText();
			Read_Lime__SimpleText(d, def, result);
			return result;
		}

		private static void Read_Lime__TextStyle(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.TextStyle)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Bold = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.CastShadow = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Font = (global::Lime.SerializableFont)dg.ReadObject<global::Lime.SerializableFont>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.ImageSize = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.ImageTexture = (global::Lime.ITexture)dg.ReadObject<global::Lime.ITexture>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.ImageUsage = (global::Lime.TextStyle.ImageUsageEnum)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.ShadowColor = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.ShadowOffset = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Size = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.SpaceAfter = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.TextColor = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__TextStyle(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.TextStyle();
			Read_Lime__TextStyle(d, def, result);
			return result;
		}

		private static void Read_Lime__Widget(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Widget)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)dg.ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)dg.ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)dg.ReadStruct<global::Lime.Color4>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)dg.ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Widget(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Widget();
			Read_Lime__Widget(d, def, result);
			return result;
		}

		private static void Read_Lime__SerializableTexture(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.SerializableTexture)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.SerializationPath = d.Reader.ReadString();
				if (result.SerializationPath == "" && d.Reader.ReadBoolean()) result.SerializationPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__SerializableTexture(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.SerializableTexture();
			Read_Lime__SerializableTexture(d, def, result);
			return result;
		}

		static BinaryDeserializerGen()
		{
			readCache[typeof(global::Lime.Font)] = Read_Lime__Font;
			readCache[typeof(global::Lime.SerializableSample)] = Read_Lime__SerializableSample;
			readCache[typeof(global::Lime.FontChar)] = Read_Lime__FontChar;
			readCache[typeof(global::Lime.SerializableFont)] = Read_Lime__SerializableFont;
			readCache[typeof(global::Lime.GeometryBuffer)] = Read_Lime__GeometryBuffer;
			readCache[typeof(global::Lime.Camera3D)] = Read_Lime__Camera3D;
			readCache[typeof(global::Lime.Material)] = Read_Lime__Material;
			readCache[typeof(global::Lime.Mesh3D)] = Read_Lime__Mesh3D;
			readCache[typeof(global::Lime.Submesh3D)] = Read_Lime__Submesh3D;
			readCache[typeof(global::Lime.Node3D)] = Read_Lime__Node3D;
			readCache[typeof(global::Lime.Spline3D)] = Read_Lime__Spline3D;
			readCache[typeof(global::Lime.Spline3D.Point)] = Read_Lime__Spline3D__Point;
			readCache[typeof(global::Lime.Viewport3D)] = Read_Lime__Viewport3D;
			readCache[typeof(global::Lime.Animation)] = Read_Lime__Animation;
			readCache[typeof(global::Lime.Spline)] = Read_Lime__Spline;
			readCache[typeof(global::Lime.LinearLayout)] = Read_Lime__LinearLayout;
			readCache[typeof(global::Lime.Animator<global::System.String>)] = Read_Lime__Animator_String;
			readCache[typeof(global::Lime.Animator<global::System.Int32>)] = Read_Lime__Animator_Int32;
			readCache[typeof(global::Lime.Animator<global::System.Boolean>)] = Read_Lime__Animator_Boolean;
			readCache[typeof(global::Lime.Animator<global::Lime.Blending>)] = Read_Lime__Animator_Blending;
			readCache[typeof(global::Lime.Animator<global::Lime.ITexture>)] = Read_Lime__Animator_ITexture;
			readCache[typeof(global::Lime.Animator<global::Lime.NumericRange>)] = Read_Lime__Animator_NumericRange;
			readCache[typeof(global::Lime.Animator<global::Lime.Vector2>)] = Read_Lime__Animator_Vector2;
			readCache[typeof(global::Lime.Animator<global::Lime.Color4>)] = Read_Lime__Animator_Color4;
			readCache[typeof(global::Lime.Animator<global::System.Single>)] = Read_Lime__Animator_Single;
			readCache[typeof(global::Lime.Animator<global::Lime.EmitterShape>)] = Read_Lime__Animator_EmitterShape;
			readCache[typeof(global::Lime.Animator<global::Lime.AudioAction>)] = Read_Lime__Animator_AudioAction;
			readCache[typeof(global::Lime.Animator<global::Lime.SerializableSample>)] = Read_Lime__Animator_SerializableSample;
			readCache[typeof(global::Lime.Animator<global::Lime.HAlignment>)] = Read_Lime__Animator_HAlignment;
			readCache[typeof(global::Lime.Animator<global::Lime.VAlignment>)] = Read_Lime__Animator_VAlignment;
			readCache[typeof(global::Lime.Animator<global::Lime.MovieAction>)] = Read_Lime__Animator_MovieAction;
			readCache[typeof(global::Lime.Animator<global::Lime.ShaderId>)] = Read_Lime__Animator_ShaderId;
			readCache[typeof(global::Lime.Animator<global::Lime.Vector3>)] = Read_Lime__Animator_Vector3;
			readCache[typeof(global::Lime.Animator<global::Lime.Quaternion>)] = Read_Lime__Animator_Quaternion;
			readCache[typeof(global::Lime.Animator<global::Lime.EmissionType>)] = Read_Lime__Animator_EmissionType;
			readCache[typeof(global::Lime.NumericAnimator)] = Read_Lime__NumericAnimator;
			readCache[typeof(global::Lime.Vector2Animator)] = Read_Lime__Vector2Animator;
			readCache[typeof(global::Lime.Color4Animator)] = Read_Lime__Color4Animator;
			readCache[typeof(global::Lime.QuaternionAnimator)] = Read_Lime__QuaternionAnimator;
			readCache[typeof(global::Lime.Vector3Animator)] = Read_Lime__Vector3Animator;
			readCache[typeof(global::Lime.Matrix44Animator)] = Read_Lime__Matrix44Animator;
			readCache[typeof(global::Lime.Keyframe<global::System.String>)] = Read_Lime__Keyframe_String;
			readCache[typeof(global::Lime.Keyframe<global::System.Int32>)] = Read_Lime__Keyframe_Int32;
			readCache[typeof(global::Lime.Keyframe<global::System.Boolean>)] = Read_Lime__Keyframe_Boolean;
			readCache[typeof(global::Lime.Keyframe<global::Lime.Blending>)] = Read_Lime__Keyframe_Blending;
			readCache[typeof(global::Lime.Keyframe<global::Lime.ITexture>)] = Read_Lime__Keyframe_ITexture;
			readCache[typeof(global::Lime.Keyframe<global::Lime.NumericRange>)] = Read_Lime__Keyframe_NumericRange;
			readCache[typeof(global::Lime.Keyframe<global::Lime.Vector2>)] = Read_Lime__Keyframe_Vector2;
			readCache[typeof(global::Lime.Keyframe<global::Lime.Color4>)] = Read_Lime__Keyframe_Color4;
			readCache[typeof(global::Lime.Keyframe<global::System.Single>)] = Read_Lime__Keyframe_Single;
			readCache[typeof(global::Lime.Keyframe<global::Lime.EmitterShape>)] = Read_Lime__Keyframe_EmitterShape;
			readCache[typeof(global::Lime.Keyframe<global::Lime.AudioAction>)] = Read_Lime__Keyframe_AudioAction;
			readCache[typeof(global::Lime.Keyframe<global::Lime.SerializableSample>)] = Read_Lime__Keyframe_SerializableSample;
			readCache[typeof(global::Lime.Keyframe<global::Lime.HAlignment>)] = Read_Lime__Keyframe_HAlignment;
			readCache[typeof(global::Lime.Keyframe<global::Lime.VAlignment>)] = Read_Lime__Keyframe_VAlignment;
			readCache[typeof(global::Lime.Keyframe<global::Lime.MovieAction>)] = Read_Lime__Keyframe_MovieAction;
			readCache[typeof(global::Lime.Keyframe<global::Lime.ShaderId>)] = Read_Lime__Keyframe_ShaderId;
			readCache[typeof(global::Lime.Keyframe<global::Lime.Vector3>)] = Read_Lime__Keyframe_Vector3;
			readCache[typeof(global::Lime.Keyframe<global::Lime.Quaternion>)] = Read_Lime__Keyframe_Quaternion;
			readCache[typeof(global::Lime.Keyframe<global::Lime.EmissionType>)] = Read_Lime__Keyframe_EmissionType;
			readCache[typeof(global::Lime.Keyframe<global::Lime.Matrix44>)] = Read_Lime__Keyframe_Matrix44;
			readCache[typeof(global::Lime.Audio)] = Read_Lime__Audio;
			readCache[typeof(global::Lime.SkinningWeights)] = Read_Lime__SkinningWeights;
			readCache[typeof(global::Lime.Bone)] = Read_Lime__Bone;
			readCache[typeof(global::Lime.Button)] = Read_Lime__Button;
			readCache[typeof(global::Lime.DistortionMesh)] = Read_Lime__DistortionMesh;
			readCache[typeof(global::Lime.DistortionMeshPoint)] = Read_Lime__DistortionMeshPoint;
			readCache[typeof(global::Lime.Frame)] = Read_Lime__Frame;
			readCache[typeof(global::Lime.Image)] = Read_Lime__Image;
			readCache[typeof(global::Lime.ImageCombiner)] = Read_Lime__ImageCombiner;
			readCache[typeof(global::Lime.Marker)] = Read_Lime__Marker;
			readCache[typeof(global::Lime.Movie)] = Read_Lime__Movie;
			readCache[typeof(global::Lime.NineGrid)] = Read_Lime__NineGrid;
			readCache[typeof(global::Lime.Node)] = Read_Lime__Node;
			readCache[typeof(global::Lime.ParticleEmitter)] = Read_Lime__ParticleEmitter;
			readCache[typeof(global::Lime.ParticleModifier)] = Read_Lime__ParticleModifier;
			readCache[typeof(global::Lime.ParticlesMagnet)] = Read_Lime__ParticlesMagnet;
			readCache[typeof(global::Lime.PointObject)] = Read_Lime__PointObject;
			readCache[typeof(global::Lime.Slider)] = Read_Lime__Slider;
			readCache[typeof(global::Lime.SplineGear)] = Read_Lime__SplineGear;
			readCache[typeof(global::Lime.SplinePoint)] = Read_Lime__SplinePoint;
			readCache[typeof(global::Lime.RichText)] = Read_Lime__RichText;
			readCache[typeof(global::Lime.SimpleText)] = Read_Lime__SimpleText;
			readCache[typeof(global::Lime.TextStyle)] = Read_Lime__TextStyle;
			readCache[typeof(global::Lime.Widget)] = Read_Lime__Widget;
			readCache[typeof(global::Lime.SerializableTexture)] = Read_Lime__SerializableTexture;
			makeCache[typeof(global::Lime.Font)] = Make_Lime__Font;
			makeCache[typeof(global::Lime.SerializableSample)] = Make_Lime__SerializableSample;
			makeCache[typeof(global::Lime.KerningPair)] = Make_Lime__KerningPair;
			makeCache[typeof(global::Lime.FontChar)] = Make_Lime__FontChar;
			makeCache[typeof(global::Lime.SerializableFont)] = Make_Lime__SerializableFont;
			makeCache[typeof(global::Lime.BlendIndices)] = Make_Lime__BlendIndices;
			makeCache[typeof(global::Lime.BlendWeights)] = Make_Lime__BlendWeights;
			makeCache[typeof(global::Lime.GeometryBuffer)] = Make_Lime__GeometryBuffer;
			makeCache[typeof(global::Lime.TextureAtlasElement.Params)] = Make_Lime__TextureAtlasElement__Params;
			makeCache[typeof(global::Lime.BitSet32)] = Make_Lime__BitSet32;
			makeCache[typeof(global::Lime.BoundingSphere)] = Make_Lime__BoundingSphere;
			makeCache[typeof(global::Lime.Color4)] = Make_Lime__Color4;
			makeCache[typeof(global::Lime.IntRectangle)] = Make_Lime__IntRectangle;
			makeCache[typeof(global::Lime.IntVector2)] = Make_Lime__IntVector2;
			makeCache[typeof(global::Lime.Matrix32)] = Make_Lime__Matrix32;
			makeCache[typeof(global::Lime.Matrix44)] = Make_Lime__Matrix44;
			makeCache[typeof(global::Lime.NumericRange)] = Make_Lime__NumericRange;
			makeCache[typeof(global::Lime.Plane)] = Make_Lime__Plane;
			makeCache[typeof(global::Lime.Quaternion)] = Make_Lime__Quaternion;
			makeCache[typeof(global::Lime.Ray)] = Make_Lime__Ray;
			makeCache[typeof(global::Lime.Rectangle)] = Make_Lime__Rectangle;
			makeCache[typeof(global::Lime.Size)] = Make_Lime__Size;
			makeCache[typeof(global::Lime.Vector2)] = Make_Lime__Vector2;
			makeCache[typeof(global::Lime.Vector3)] = Make_Lime__Vector3;
			makeCache[typeof(global::Lime.Vector4)] = Make_Lime__Vector4;
			makeCache[typeof(global::Lime.Camera3D)] = Make_Lime__Camera3D;
			makeCache[typeof(global::Lime.Material)] = Make_Lime__Material;
			makeCache[typeof(global::Lime.Mesh3D)] = Make_Lime__Mesh3D;
			makeCache[typeof(global::Lime.Submesh3D)] = Make_Lime__Submesh3D;
			makeCache[typeof(global::Lime.Node3D)] = Make_Lime__Node3D;
			makeCache[typeof(global::Lime.Spline3D)] = Make_Lime__Spline3D;
			makeCache[typeof(global::Lime.Spline3D.Point)] = Make_Lime__Spline3D__Point;
			makeCache[typeof(global::Lime.Viewport3D)] = Make_Lime__Viewport3D;
			makeCache[typeof(global::Lime.Animation)] = Make_Lime__Animation;
			makeCache[typeof(global::Lime.Spline)] = Make_Lime__Spline;
			makeCache[typeof(global::Lime.LinearLayout)] = Make_Lime__LinearLayout;
			makeCache[typeof(global::Lime.Animator<global::System.String>)] = Make_Lime__Animator_String;
			makeCache[typeof(global::Lime.Animator<global::System.Int32>)] = Make_Lime__Animator_Int32;
			makeCache[typeof(global::Lime.Animator<global::System.Boolean>)] = Make_Lime__Animator_Boolean;
			makeCache[typeof(global::Lime.Animator<global::Lime.Blending>)] = Make_Lime__Animator_Blending;
			makeCache[typeof(global::Lime.Animator<global::Lime.ITexture>)] = Make_Lime__Animator_ITexture;
			makeCache[typeof(global::Lime.Animator<global::Lime.NumericRange>)] = Make_Lime__Animator_NumericRange;
			makeCache[typeof(global::Lime.Animator<global::Lime.Vector2>)] = Make_Lime__Animator_Vector2;
			makeCache[typeof(global::Lime.Animator<global::Lime.Color4>)] = Make_Lime__Animator_Color4;
			makeCache[typeof(global::Lime.Animator<global::System.Single>)] = Make_Lime__Animator_Single;
			makeCache[typeof(global::Lime.Animator<global::Lime.EmitterShape>)] = Make_Lime__Animator_EmitterShape;
			makeCache[typeof(global::Lime.Animator<global::Lime.AudioAction>)] = Make_Lime__Animator_AudioAction;
			makeCache[typeof(global::Lime.Animator<global::Lime.SerializableSample>)] = Make_Lime__Animator_SerializableSample;
			makeCache[typeof(global::Lime.Animator<global::Lime.HAlignment>)] = Make_Lime__Animator_HAlignment;
			makeCache[typeof(global::Lime.Animator<global::Lime.VAlignment>)] = Make_Lime__Animator_VAlignment;
			makeCache[typeof(global::Lime.Animator<global::Lime.MovieAction>)] = Make_Lime__Animator_MovieAction;
			makeCache[typeof(global::Lime.Animator<global::Lime.ShaderId>)] = Make_Lime__Animator_ShaderId;
			makeCache[typeof(global::Lime.Animator<global::Lime.Vector3>)] = Make_Lime__Animator_Vector3;
			makeCache[typeof(global::Lime.Animator<global::Lime.Quaternion>)] = Make_Lime__Animator_Quaternion;
			makeCache[typeof(global::Lime.Animator<global::Lime.EmissionType>)] = Make_Lime__Animator_EmissionType;
			makeCache[typeof(global::Lime.NumericAnimator)] = Make_Lime__NumericAnimator;
			makeCache[typeof(global::Lime.Vector2Animator)] = Make_Lime__Vector2Animator;
			makeCache[typeof(global::Lime.Color4Animator)] = Make_Lime__Color4Animator;
			makeCache[typeof(global::Lime.QuaternionAnimator)] = Make_Lime__QuaternionAnimator;
			makeCache[typeof(global::Lime.Vector3Animator)] = Make_Lime__Vector3Animator;
			makeCache[typeof(global::Lime.Matrix44Animator)] = Make_Lime__Matrix44Animator;
			makeCache[typeof(global::Lime.Keyframe<global::System.String>)] = Make_Lime__Keyframe_String;
			makeCache[typeof(global::Lime.Keyframe<global::System.Int32>)] = Make_Lime__Keyframe_Int32;
			makeCache[typeof(global::Lime.Keyframe<global::System.Boolean>)] = Make_Lime__Keyframe_Boolean;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.Blending>)] = Make_Lime__Keyframe_Blending;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.ITexture>)] = Make_Lime__Keyframe_ITexture;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.NumericRange>)] = Make_Lime__Keyframe_NumericRange;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.Vector2>)] = Make_Lime__Keyframe_Vector2;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.Color4>)] = Make_Lime__Keyframe_Color4;
			makeCache[typeof(global::Lime.Keyframe<global::System.Single>)] = Make_Lime__Keyframe_Single;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.EmitterShape>)] = Make_Lime__Keyframe_EmitterShape;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.AudioAction>)] = Make_Lime__Keyframe_AudioAction;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.SerializableSample>)] = Make_Lime__Keyframe_SerializableSample;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.HAlignment>)] = Make_Lime__Keyframe_HAlignment;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.VAlignment>)] = Make_Lime__Keyframe_VAlignment;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.MovieAction>)] = Make_Lime__Keyframe_MovieAction;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.ShaderId>)] = Make_Lime__Keyframe_ShaderId;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.Vector3>)] = Make_Lime__Keyframe_Vector3;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.Quaternion>)] = Make_Lime__Keyframe_Quaternion;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.EmissionType>)] = Make_Lime__Keyframe_EmissionType;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.Matrix44>)] = Make_Lime__Keyframe_Matrix44;
			makeCache[typeof(global::Lime.Audio)] = Make_Lime__Audio;
			makeCache[typeof(global::Lime.BoneWeight)] = Make_Lime__BoneWeight;
			makeCache[typeof(global::Lime.SkinningWeights)] = Make_Lime__SkinningWeights;
			makeCache[typeof(global::Lime.Bone)] = Make_Lime__Bone;
			makeCache[typeof(global::Lime.BoneArray)] = Make_Lime__BoneArray;
			makeCache[typeof(global::Lime.BoneArray.Entry)] = Make_Lime__BoneArray__Entry;
			makeCache[typeof(global::Lime.Button)] = Make_Lime__Button;
			makeCache[typeof(global::Lime.DistortionMesh)] = Make_Lime__DistortionMesh;
			makeCache[typeof(global::Lime.DistortionMeshPoint)] = Make_Lime__DistortionMeshPoint;
			makeCache[typeof(global::Lime.Frame)] = Make_Lime__Frame;
			makeCache[typeof(global::Lime.Image)] = Make_Lime__Image;
			makeCache[typeof(global::Lime.ImageCombiner)] = Make_Lime__ImageCombiner;
			makeCache[typeof(global::Lime.Marker)] = Make_Lime__Marker;
			makeCache[typeof(global::Lime.Movie)] = Make_Lime__Movie;
			makeCache[typeof(global::Lime.NineGrid)] = Make_Lime__NineGrid;
			makeCache[typeof(global::Lime.Node)] = Make_Lime__Node;
			makeCache[typeof(global::Lime.ParticleEmitter)] = Make_Lime__ParticleEmitter;
			makeCache[typeof(global::Lime.ParticleModifier)] = Make_Lime__ParticleModifier;
			makeCache[typeof(global::Lime.ParticlesMagnet)] = Make_Lime__ParticlesMagnet;
			makeCache[typeof(global::Lime.PointObject)] = Make_Lime__PointObject;
			makeCache[typeof(global::Lime.Slider)] = Make_Lime__Slider;
			makeCache[typeof(global::Lime.SplineGear)] = Make_Lime__SplineGear;
			makeCache[typeof(global::Lime.SplinePoint)] = Make_Lime__SplinePoint;
			makeCache[typeof(global::Lime.RichText)] = Make_Lime__RichText;
			makeCache[typeof(global::Lime.SimpleText)] = Make_Lime__SimpleText;
			makeCache[typeof(global::Lime.TextStyle)] = Make_Lime__TextStyle;
			makeCache[typeof(global::Lime.Widget)] = Make_Lime__Widget;
			makeCache[typeof(global::Lime.SerializableTexture)] = Make_Lime__SerializableTexture;
		}
	}
}
