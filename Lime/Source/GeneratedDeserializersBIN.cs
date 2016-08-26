using System;
using System.Reflection;

using Yuzu;
using Yuzu.Binary;

namespace GeneratedDeserializersBIN
{
	public class BinaryDeserializerGen: BinaryDeserializer
	{
		private void Read_Lime__Font(ClassDef def, object obj)
		{
			var result = (global::Lime.Font)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.About = Reader.ReadString();
				if (result.About == "" && Reader.ReadBoolean()) result.About = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.FontChar)ReadObject<global::Lime.FontChar>();
						((global::System.Collections.Generic.ICollection<global::Lime.FontChar>)result.CharCollection).Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.ITexture)ReadObject<global::Lime.ITexture>();
						result.Textures.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Font(ClassDef def)
		{
			var result = new global::Lime.Font();
			Read_Lime__Font(def, result);
			return result;
		}

		private void Read_Lime__SerializableSample(ClassDef def, object obj)
		{
			var result = (global::Lime.SerializableSample)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.SerializationPath = Reader.ReadString();
				if (result.SerializationPath == "" && Reader.ReadBoolean()) result.SerializationPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__SerializableSample(ClassDef def)
		{
			var result = new global::Lime.SerializableSample();
			Read_Lime__SerializableSample(def, result);
			return result;
		}

		private object Make_Lime__KerningPair(ClassDef def)
		{
			var result = new global::Lime.KerningPair();
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Char = Reader.ReadChar();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Kerning = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
			return result;
		}

		private void Read_Lime__FontChar(ClassDef def, object obj)
		{
			var result = (global::Lime.FontChar)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.ACWidths = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Char = Reader.ReadChar();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Height = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.KerningPairs = (global::System.Collections.Generic.List<global::Lime.KerningPair>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.KerningPairs = new global::System.Collections.Generic.List<global::Lime.KerningPair>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.KerningPair)ReadStruct<global::Lime.KerningPair>();
						result.KerningPairs.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.TextureIndex = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.UV0 = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.UV1 = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.VerticalOffset = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Width = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__FontChar(ClassDef def)
		{
			var result = new global::Lime.FontChar();
			Read_Lime__FontChar(def, result);
			return result;
		}

		private void Read_Lime__SerializableFont(ClassDef def, object obj)
		{
			var result = (global::Lime.SerializableFont)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Name = Reader.ReadString();
				if (result.Name == "" && Reader.ReadBoolean()) result.Name = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__SerializableFont(ClassDef def)
		{
			var result = new global::Lime.SerializableFont();
			Read_Lime__SerializableFont(def, result);
			return result;
		}

		private object Make_Lime__BlendIndices(ClassDef def)
		{
			var result = new global::Lime.BlendIndices();
			result.Index0 = Reader.ReadByte();
			result.Index1 = Reader.ReadByte();
			result.Index2 = Reader.ReadByte();
			result.Index3 = Reader.ReadByte();
			return result;
		}

		private object Make_Lime__BlendWeights(ClassDef def)
		{
			var result = new global::Lime.BlendWeights();
			result.Weight0 = Reader.ReadSingle();
			result.Weight1 = Reader.ReadSingle();
			result.Weight2 = Reader.ReadSingle();
			result.Weight3 = Reader.ReadSingle();
			return result;
		}

		private void Read_Lime__GeometryBuffer(ClassDef def, object obj)
		{
			var result = (global::Lime.GeometryBuffer)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.BlendIndices = (global::Lime.BlendIndices[])null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					var tmp2 = new global::Lime.BlendIndices[tmp1];
					for(tmp1 = 0; tmp1 < tmp2.Length; ++tmp1) {
						tmp2[tmp1] = (global::Lime.BlendIndices)ReadStruct<global::Lime.BlendIndices>();
					}
					result.BlendIndices = tmp2;
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.BlendWeights = (global::Lime.BlendWeights[])null;
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					var tmp4 = new global::Lime.BlendWeights[tmp3];
					for(tmp3 = 0; tmp3 < tmp4.Length; ++tmp3) {
						tmp4[tmp3] = (global::Lime.BlendWeights)ReadStruct<global::Lime.BlendWeights>();
					}
					result.BlendWeights = tmp4;
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Colors = (global::Lime.Color4[])null;
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					var tmp6 = new global::Lime.Color4[tmp5];
					for(tmp5 = 0; tmp5 < tmp6.Length; ++tmp5) {
						tmp6[tmp5] = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
					}
					result.Colors = tmp6;
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Indices = (global::System.UInt16[])null;
				var tmp7 = Reader.ReadInt32();
				if (tmp7 >= 0) {
					var tmp8 = new global::System.UInt16[tmp7];
					for(tmp7 = 0; tmp7 < tmp8.Length; ++tmp7) {
						tmp8[tmp7] = Reader.ReadUInt16();
					}
					result.Indices = tmp8;
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.UV1 = (global::Lime.Vector2[])null;
				var tmp9 = Reader.ReadInt32();
				if (tmp9 >= 0) {
					var tmp10 = new global::Lime.Vector2[tmp9];
					for(tmp9 = 0; tmp9 < tmp10.Length; ++tmp9) {
						tmp10[tmp9] = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
					}
					result.UV1 = tmp10;
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.UV2 = (global::Lime.Vector2[])null;
				var tmp11 = Reader.ReadInt32();
				if (tmp11 >= 0) {
					var tmp12 = new global::Lime.Vector2[tmp11];
					for(tmp11 = 0; tmp11 < tmp12.Length; ++tmp11) {
						tmp12[tmp11] = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
					}
					result.UV2 = tmp12;
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.UV3 = (global::Lime.Vector2[])null;
				var tmp13 = Reader.ReadInt32();
				if (tmp13 >= 0) {
					var tmp14 = new global::Lime.Vector2[tmp13];
					for(tmp13 = 0; tmp13 < tmp14.Length; ++tmp13) {
						tmp14[tmp13] = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
					}
					result.UV3 = tmp14;
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.UV4 = (global::Lime.Vector2[])null;
				var tmp15 = Reader.ReadInt32();
				if (tmp15 >= 0) {
					var tmp16 = new global::Lime.Vector2[tmp15];
					for(tmp15 = 0; tmp15 < tmp16.Length; ++tmp15) {
						tmp16[tmp15] = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
					}
					result.UV4 = tmp16;
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Vertices = (global::Lime.Vector3[])null;
				var tmp17 = Reader.ReadInt32();
				if (tmp17 >= 0) {
					var tmp18 = new global::Lime.Vector3[tmp17];
					for(tmp17 = 0; tmp17 < tmp18.Length; ++tmp17) {
						tmp18[tmp17] = (global::Lime.Vector3)ReadStruct<global::Lime.Vector3>();
					}
					result.Vertices = tmp18;
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
			result.AfterDeserialization();
		}

		private object Make_Lime__GeometryBuffer(ClassDef def)
		{
			var result = new global::Lime.GeometryBuffer();
			Read_Lime__GeometryBuffer(def, result);
			return result;
		}

		private object Make_Lime__TextureAtlasElement__Params(ClassDef def)
		{
			var result = new global::Lime.TextureAtlasElement.Params();
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AtlasPath = Reader.ReadString();
				if (result.AtlasPath == "" && Reader.ReadBoolean()) result.AtlasPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.AtlasRect = (global::Lime.IntRectangle)ReadStruct<global::Lime.IntRectangle>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
			return result;
		}

		private object Make_Lime__BitSet32(ClassDef def)
		{
			var result = new global::Lime.BitSet32();
			result.Value = Reader.ReadUInt32();
			return result;
		}

		private object Make_Lime__BoundingSphere(ClassDef def)
		{
			var result = new global::Lime.BoundingSphere();
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Center = (global::Lime.Vector3)ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Radius = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
			return result;
		}

		private object Make_Lime__Color4(ClassDef def)
		{
			var result = new global::Lime.Color4();
			result.ABGR = Reader.ReadUInt32();
			return result;
		}

		private object Make_Lime__IntRectangle(ClassDef def)
		{
			var result = new global::Lime.IntRectangle();
			result.A = (global::Lime.IntVector2)ReadStruct<global::Lime.IntVector2>();
			result.B = (global::Lime.IntVector2)ReadStruct<global::Lime.IntVector2>();
			return result;
		}

		private object Make_Lime__IntVector2(ClassDef def)
		{
			var result = new global::Lime.IntVector2();
			result.X = Reader.ReadInt32();
			result.Y = Reader.ReadInt32();
			return result;
		}

		private object Make_Lime__Matrix32(ClassDef def)
		{
			var result = new global::Lime.Matrix32();
			result.T = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
			result.U = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
			result.V = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
			return result;
		}

		private object Make_Lime__Matrix44(ClassDef def)
		{
			var result = new global::Lime.Matrix44();
			result.M11 = Reader.ReadSingle();
			result.M12 = Reader.ReadSingle();
			result.M13 = Reader.ReadSingle();
			result.M14 = Reader.ReadSingle();
			result.M21 = Reader.ReadSingle();
			result.M22 = Reader.ReadSingle();
			result.M23 = Reader.ReadSingle();
			result.M24 = Reader.ReadSingle();
			result.M31 = Reader.ReadSingle();
			result.M32 = Reader.ReadSingle();
			result.M33 = Reader.ReadSingle();
			result.M34 = Reader.ReadSingle();
			result.M41 = Reader.ReadSingle();
			result.M42 = Reader.ReadSingle();
			result.M43 = Reader.ReadSingle();
			result.M44 = Reader.ReadSingle();
			return result;
		}

		private object Make_Lime__NumericRange(ClassDef def)
		{
			var result = new global::Lime.NumericRange();
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Dispersion = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Median = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
			return result;
		}

		private object Make_Lime__Plane(ClassDef def)
		{
			var result = new global::Lime.Plane();
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.D = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Normal = (global::Lime.Vector3)ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
			return result;
		}

		private object Make_Lime__Quaternion(ClassDef def)
		{
			var result = new global::Lime.Quaternion();
			result.W = Reader.ReadSingle();
			result.X = Reader.ReadSingle();
			result.Y = Reader.ReadSingle();
			result.Z = Reader.ReadSingle();
			return result;
		}

		private object Make_Lime__Ray(ClassDef def)
		{
			var result = new global::Lime.Ray();
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Direction = (global::Lime.Vector3)ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Position = (global::Lime.Vector3)ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
			return result;
		}

		private object Make_Lime__Rectangle(ClassDef def)
		{
			var result = new global::Lime.Rectangle();
			result.A = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
			result.B = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
			return result;
		}

		private object Make_Lime__Size(ClassDef def)
		{
			var result = new global::Lime.Size();
			result.Height = Reader.ReadInt32();
			result.Width = Reader.ReadInt32();
			return result;
		}

		private object Make_Lime__Vector2(ClassDef def)
		{
			var result = new global::Lime.Vector2();
			result.X = Reader.ReadSingle();
			result.Y = Reader.ReadSingle();
			return result;
		}

		private object Make_Lime__Vector3(ClassDef def)
		{
			var result = new global::Lime.Vector3();
			result.X = Reader.ReadSingle();
			result.Y = Reader.ReadSingle();
			result.Z = Reader.ReadSingle();
			return result;
		}

		private object Make_Lime__Vector4(ClassDef def)
		{
			var result = new global::Lime.Vector4();
			result.W = Reader.ReadSingle();
			result.X = Reader.ReadSingle();
			result.Y = Reader.ReadSingle();
			result.Z = Reader.ReadSingle();
			return result;
		}

		private void Read_Lime__Camera3D(ClassDef def, object obj)
		{
			var result = (global::Lime.Camera3D)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.AspectRatio = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.FarClipPlane = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.FieldOfView = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.NearClipPlane = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Position = (global::Lime.Vector3)ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Rotation = (global::Lime.Quaternion)ReadStruct<global::Lime.Quaternion>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector3)ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Visible = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Camera3D(ClassDef def)
		{
			var result = new global::Lime.Camera3D();
			Read_Lime__Camera3D(def, result);
			return result;
		}

		private void Read_Lime__Material(ClassDef def, object obj)
		{
			var result = (global::Lime.Material)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.DiffuseColor = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.DiffuseTexture = (global::Lime.ITexture)ReadObject<global::Lime.ITexture>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.EmissiveColor = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.FogColor = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.FogDensity = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.FogEnd = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.FogMode = (global::Lime.FogMode)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.FogStart = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Name = Reader.ReadString();
				if (result.Name == "" && Reader.ReadBoolean()) result.Name = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Opacity = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.OpacityTexture = (global::Lime.ITexture)ReadObject<global::Lime.ITexture>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.SpecularColor = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.SpecularPower = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Material(ClassDef def)
		{
			var result = new global::Lime.Material();
			Read_Lime__Material(def, result);
			return result;
		}

		private void Read_Lime__Mesh3D(ClassDef def, object obj)
		{
			var result = (global::Lime.Mesh3D)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Matrix44)ReadStruct<global::Lime.Matrix44>();
						result.BoneBindPoseInverses.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				var tmp7 = Reader.ReadInt32();
				if (tmp7 >= 0) {
					while (--tmp7 >= 0) {
						var tmp8 = (global::Lime.Node3D)ReadObject<global::Lime.Node3D>();
						result.Bones.Add(tmp8);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoundingSphere = (global::Lime.BoundingSphere)ReadStruct<global::Lime.BoundingSphere>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				var tmp9 = Reader.ReadInt32();
				if (tmp9 >= 0) {
					while (--tmp9 >= 0) {
						var tmp10 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp10);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Position = (global::Lime.Vector3)ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Rotation = (global::Lime.Quaternion)ReadStruct<global::Lime.Quaternion>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector3)ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				var tmp11 = Reader.ReadInt32();
				if (tmp11 >= 0) {
					while (--tmp11 >= 0) {
						var tmp12 = (global::Lime.Submesh3D)ReadObject<global::Lime.Submesh3D>();
						result.Submeshes.Add(tmp12);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Visible = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
			result.AfterDeserialization();
		}

		private object Make_Lime__Mesh3D(ClassDef def)
		{
			var result = new global::Lime.Mesh3D();
			Read_Lime__Mesh3D(def, result);
			return result;
		}

		private void Read_Lime__Submesh3D(ClassDef def, object obj)
		{
			var result = (global::Lime.Submesh3D)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = Reader.ReadInt32();
						result.BoneIndices.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				ReadIntoObject<global::Lime.GeometryBuffer>(result.Geometry);
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Material = (global::Lime.Material)ReadObject<global::Lime.Material>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Submesh3D(ClassDef def)
		{
			var result = new global::Lime.Submesh3D();
			Read_Lime__Submesh3D(def, result);
			return result;
		}

		private void Read_Lime__Node3D(ClassDef def, object obj)
		{
			var result = (global::Lime.Node3D)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Position = (global::Lime.Vector3)ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Rotation = (global::Lime.Quaternion)ReadStruct<global::Lime.Quaternion>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector3)ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Visible = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Node3D(ClassDef def)
		{
			var result = new global::Lime.Node3D();
			Read_Lime__Node3D(def, result);
			return result;
		}

		private void Read_Lime__Spline3D(ClassDef def, object obj)
		{
			var result = (global::Lime.Spline3D)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Closed = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Points = (global::System.Collections.Generic.List<global::Lime.Spline3D.Point>)null;
				var tmp7 = Reader.ReadInt32();
				if (tmp7 >= 0) {
					result.Points = new global::System.Collections.Generic.List<global::Lime.Spline3D.Point>();
					while (--tmp7 >= 0) {
						var tmp8 = (global::Lime.Spline3D.Point)ReadObject<global::Lime.Spline3D.Point>();
						result.Points.Add(tmp8);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Position = (global::Lime.Vector3)ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Rotation = (global::Lime.Quaternion)ReadStruct<global::Lime.Quaternion>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector3)ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Visible = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Spline3D(ClassDef def)
		{
			var result = new global::Lime.Spline3D();
			Read_Lime__Spline3D(def, result);
			return result;
		}

		private void Read_Lime__Spline3D__Point(ClassDef def, object obj)
		{
			var result = (global::Lime.Spline3D.Point)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.InterpolationMode = (global::Lime.Spline3D.InterpolationMode)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Position = (global::Lime.Vector3)ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TangentA = (global::Lime.Vector3)ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.TangentB = (global::Lime.Vector3)ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Spline3D__Point(ClassDef def)
		{
			var result = new global::Lime.Spline3D.Point();
			Read_Lime__Spline3D__Point(def, result);
			return result;
		}

		private void Read_Lime__Viewport3D(ClassDef def, object obj)
		{
			var result = (global::Lime.Viewport3D)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Frame = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Rotation = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.Visible = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Viewport3D(ClassDef def)
		{
			var result = new global::Lime.Viewport3D();
			Read_Lime__Viewport3D(def, result);
			return result;
		}

		private void Read_Lime__Animation(ClassDef def, object obj)
		{
			var result = (global::Lime.Animation)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Marker)ReadObject<global::Lime.Marker>();
						result.Markers.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Animation(ClassDef def)
		{
			var result = new global::Lime.Animation();
			Read_Lime__Animation(def, result);
			return result;
		}

		private void Read_Lime__Spline(ClassDef def, object obj)
		{
			var result = (global::Lime.Spline)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Rotation = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Visible = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Spline(ClassDef def)
		{
			var result = new global::Lime.Spline();
			Read_Lime__Spline(def, result);
			return result;
		}

		private void Read_Lime__LinearLayout(ClassDef def, object obj)
		{
			var result = (global::Lime.LinearLayout)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Horizontal = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ProcessHidden = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__LinearLayout(ClassDef def)
		{
			var result = new global::Lime.LinearLayout();
			Read_Lime__LinearLayout(def, result);
			return result;
		}

		private void Read_Lime__Animator_String(ClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::System.String>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::System.String>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::System.String>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::System.String>)ReadObject<global::Lime.Keyframe<global::System.String>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Animator_String(ClassDef def)
		{
			var result = new global::Lime.Animator<global::System.String>();
			Read_Lime__Animator_String(def, result);
			return result;
		}

		private void Read_Lime__Animator_Int32(ClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::System.Int32>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::System.Int32>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::System.Int32>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::System.Int32>)ReadObject<global::Lime.Keyframe<global::System.Int32>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Animator_Int32(ClassDef def)
		{
			var result = new global::Lime.Animator<global::System.Int32>();
			Read_Lime__Animator_Int32(def, result);
			return result;
		}

		private void Read_Lime__Animator_Boolean(ClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::System.Boolean>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::System.Boolean>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::System.Boolean>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::System.Boolean>)ReadObject<global::Lime.Keyframe<global::System.Boolean>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Animator_Boolean(ClassDef def)
		{
			var result = new global::Lime.Animator<global::System.Boolean>();
			Read_Lime__Animator_Boolean(def, result);
			return result;
		}

		private void Read_Lime__Animator_Blending(ClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.Blending>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::Lime.Blending>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::Lime.Blending>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.Blending>)ReadObject<global::Lime.Keyframe<global::Lime.Blending>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Animator_Blending(ClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.Blending>();
			Read_Lime__Animator_Blending(def, result);
			return result;
		}

		private void Read_Lime__Animator_ITexture(ClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.ITexture>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::Lime.ITexture>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::Lime.ITexture>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.ITexture>)ReadObject<global::Lime.Keyframe<global::Lime.ITexture>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Animator_ITexture(ClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.ITexture>();
			Read_Lime__Animator_ITexture(def, result);
			return result;
		}

		private void Read_Lime__Animator_NumericRange(ClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.NumericRange>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::Lime.NumericRange>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::Lime.NumericRange>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.NumericRange>)ReadObject<global::Lime.Keyframe<global::Lime.NumericRange>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Animator_NumericRange(ClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.NumericRange>();
			Read_Lime__Animator_NumericRange(def, result);
			return result;
		}

		private void Read_Lime__Animator_Vector2(ClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.Vector2>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::Lime.Vector2>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::Lime.Vector2>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.Vector2>)ReadObject<global::Lime.Keyframe<global::Lime.Vector2>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Animator_Vector2(ClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.Vector2>();
			Read_Lime__Animator_Vector2(def, result);
			return result;
		}

		private void Read_Lime__Animator_Color4(ClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.Color4>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::Lime.Color4>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::Lime.Color4>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.Color4>)ReadObject<global::Lime.Keyframe<global::Lime.Color4>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Animator_Color4(ClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.Color4>();
			Read_Lime__Animator_Color4(def, result);
			return result;
		}

		private void Read_Lime__Animator_Single(ClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::System.Single>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::System.Single>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::System.Single>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::System.Single>)ReadObject<global::Lime.Keyframe<global::System.Single>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Animator_Single(ClassDef def)
		{
			var result = new global::Lime.Animator<global::System.Single>();
			Read_Lime__Animator_Single(def, result);
			return result;
		}

		private void Read_Lime__Animator_EmitterShape(ClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.EmitterShape>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::Lime.EmitterShape>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::Lime.EmitterShape>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.EmitterShape>)ReadObject<global::Lime.Keyframe<global::Lime.EmitterShape>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Animator_EmitterShape(ClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.EmitterShape>();
			Read_Lime__Animator_EmitterShape(def, result);
			return result;
		}

		private void Read_Lime__Animator_AudioAction(ClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.AudioAction>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::Lime.AudioAction>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::Lime.AudioAction>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.AudioAction>)ReadObject<global::Lime.Keyframe<global::Lime.AudioAction>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Animator_AudioAction(ClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.AudioAction>();
			Read_Lime__Animator_AudioAction(def, result);
			return result;
		}

		private void Read_Lime__Animator_SerializableSample(ClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.SerializableSample>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::Lime.SerializableSample>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::Lime.SerializableSample>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.SerializableSample>)ReadObject<global::Lime.Keyframe<global::Lime.SerializableSample>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Animator_SerializableSample(ClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.SerializableSample>();
			Read_Lime__Animator_SerializableSample(def, result);
			return result;
		}

		private void Read_Lime__Animator_HAlignment(ClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.HAlignment>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::Lime.HAlignment>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::Lime.HAlignment>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.HAlignment>)ReadObject<global::Lime.Keyframe<global::Lime.HAlignment>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Animator_HAlignment(ClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.HAlignment>();
			Read_Lime__Animator_HAlignment(def, result);
			return result;
		}

		private void Read_Lime__Animator_VAlignment(ClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.VAlignment>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::Lime.VAlignment>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::Lime.VAlignment>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.VAlignment>)ReadObject<global::Lime.Keyframe<global::Lime.VAlignment>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Animator_VAlignment(ClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.VAlignment>();
			Read_Lime__Animator_VAlignment(def, result);
			return result;
		}

		private void Read_Lime__Animator_MovieAction(ClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.MovieAction>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::Lime.MovieAction>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::Lime.MovieAction>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.MovieAction>)ReadObject<global::Lime.Keyframe<global::Lime.MovieAction>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Animator_MovieAction(ClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.MovieAction>();
			Read_Lime__Animator_MovieAction(def, result);
			return result;
		}

		private void Read_Lime__Animator_ShaderId(ClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.ShaderId>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::Lime.ShaderId>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::Lime.ShaderId>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.ShaderId>)ReadObject<global::Lime.Keyframe<global::Lime.ShaderId>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Animator_ShaderId(ClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.ShaderId>();
			Read_Lime__Animator_ShaderId(def, result);
			return result;
		}

		private void Read_Lime__Animator_Vector3(ClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.Vector3>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::Lime.Vector3>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::Lime.Vector3>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.Vector3>)ReadObject<global::Lime.Keyframe<global::Lime.Vector3>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Animator_Vector3(ClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.Vector3>();
			Read_Lime__Animator_Vector3(def, result);
			return result;
		}

		private void Read_Lime__Animator_Quaternion(ClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.Quaternion>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::Lime.Quaternion>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::Lime.Quaternion>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.Quaternion>)ReadObject<global::Lime.Keyframe<global::Lime.Quaternion>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Animator_Quaternion(ClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.Quaternion>();
			Read_Lime__Animator_Quaternion(def, result);
			return result;
		}

		private void Read_Lime__Animator_EmissionType(ClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.EmissionType>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::Lime.EmissionType>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::Lime.EmissionType>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.EmissionType>)ReadObject<global::Lime.Keyframe<global::Lime.EmissionType>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Animator_EmissionType(ClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.EmissionType>();
			Read_Lime__Animator_EmissionType(def, result);
			return result;
		}

		private void Read_Lime__NumericAnimator(ClassDef def, object obj)
		{
			var result = (global::Lime.NumericAnimator)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::System.Single>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::System.Single>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::System.Single>)ReadObject<global::Lime.Keyframe<global::System.Single>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__NumericAnimator(ClassDef def)
		{
			var result = new global::Lime.NumericAnimator();
			Read_Lime__NumericAnimator(def, result);
			return result;
		}

		private void Read_Lime__Vector2Animator(ClassDef def, object obj)
		{
			var result = (global::Lime.Vector2Animator)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::Lime.Vector2>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::Lime.Vector2>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.Vector2>)ReadObject<global::Lime.Keyframe<global::Lime.Vector2>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Vector2Animator(ClassDef def)
		{
			var result = new global::Lime.Vector2Animator();
			Read_Lime__Vector2Animator(def, result);
			return result;
		}

		private void Read_Lime__Color4Animator(ClassDef def, object obj)
		{
			var result = (global::Lime.Color4Animator)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::Lime.Color4>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::Lime.Color4>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.Color4>)ReadObject<global::Lime.Keyframe<global::Lime.Color4>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Color4Animator(ClassDef def)
		{
			var result = new global::Lime.Color4Animator();
			Read_Lime__Color4Animator(def, result);
			return result;
		}

		private void Read_Lime__QuaternionAnimator(ClassDef def, object obj)
		{
			var result = (global::Lime.QuaternionAnimator)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::Lime.Quaternion>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::Lime.Quaternion>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.Quaternion>)ReadObject<global::Lime.Keyframe<global::Lime.Quaternion>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__QuaternionAnimator(ClassDef def)
		{
			var result = new global::Lime.QuaternionAnimator();
			Read_Lime__QuaternionAnimator(def, result);
			return result;
		}

		private void Read_Lime__Vector3Animator(ClassDef def, object obj)
		{
			var result = (global::Lime.Vector3Animator)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::Lime.Vector3>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::Lime.Vector3>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.Vector3>)ReadObject<global::Lime.Keyframe<global::Lime.Vector3>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Vector3Animator(ClassDef def)
		{
			var result = new global::Lime.Vector3Animator();
			Read_Lime__Vector3Animator(def, result);
			return result;
		}

		private void Read_Lime__Matrix44Animator(ClassDef def, object obj)
		{
			var result = (global::Lime.Matrix44Animator)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimationId = Reader.ReadString();
				if (result.AnimationId == "" && Reader.ReadBoolean()) result.AnimationId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ReadonlyKeys = (global::Lime.KeyframeCollection<global::Lime.Matrix44>)null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ReadonlyKeys = new global::Lime.KeyframeCollection<global::Lime.Matrix44>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Keyframe<global::Lime.Matrix44>)ReadObject<global::Lime.Keyframe<global::Lime.Matrix44>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetProperty = Reader.ReadString();
				if (result.TargetProperty == "" && Reader.ReadBoolean()) result.TargetProperty = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Matrix44Animator(ClassDef def)
		{
			var result = new global::Lime.Matrix44Animator();
			Read_Lime__Matrix44Animator(def, result);
			return result;
		}

		private void Read_Lime__Keyframe_String(ClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::System.String>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = Reader.ReadString();
				if (result.Value == "" && Reader.ReadBoolean()) result.Value = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Keyframe_String(ClassDef def)
		{
			var result = new global::Lime.Keyframe<global::System.String>();
			Read_Lime__Keyframe_String(def, result);
			return result;
		}

		private void Read_Lime__Keyframe_Int32(ClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::System.Int32>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Keyframe_Int32(ClassDef def)
		{
			var result = new global::Lime.Keyframe<global::System.Int32>();
			Read_Lime__Keyframe_Int32(def, result);
			return result;
		}

		private void Read_Lime__Keyframe_Boolean(ClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::System.Boolean>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Keyframe_Boolean(ClassDef def)
		{
			var result = new global::Lime.Keyframe<global::System.Boolean>();
			Read_Lime__Keyframe_Boolean(def, result);
			return result;
		}

		private void Read_Lime__Keyframe_Blending(ClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.Blending>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.Blending)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Keyframe_Blending(ClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.Blending>();
			Read_Lime__Keyframe_Blending(def, result);
			return result;
		}

		private void Read_Lime__Keyframe_ITexture(ClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.ITexture>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.ITexture)ReadObject<global::Lime.ITexture>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Keyframe_ITexture(ClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.ITexture>();
			Read_Lime__Keyframe_ITexture(def, result);
			return result;
		}

		private void Read_Lime__Keyframe_NumericRange(ClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.NumericRange>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.NumericRange)ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Keyframe_NumericRange(ClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.NumericRange>();
			Read_Lime__Keyframe_NumericRange(def, result);
			return result;
		}

		private void Read_Lime__Keyframe_Vector2(ClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.Vector2>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Keyframe_Vector2(ClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.Vector2>();
			Read_Lime__Keyframe_Vector2(def, result);
			return result;
		}

		private void Read_Lime__Keyframe_Color4(ClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.Color4>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Keyframe_Color4(ClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.Color4>();
			Read_Lime__Keyframe_Color4(def, result);
			return result;
		}

		private void Read_Lime__Keyframe_Single(ClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::System.Single>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Keyframe_Single(ClassDef def)
		{
			var result = new global::Lime.Keyframe<global::System.Single>();
			Read_Lime__Keyframe_Single(def, result);
			return result;
		}

		private void Read_Lime__Keyframe_EmitterShape(ClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.EmitterShape>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.EmitterShape)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Keyframe_EmitterShape(ClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.EmitterShape>();
			Read_Lime__Keyframe_EmitterShape(def, result);
			return result;
		}

		private void Read_Lime__Keyframe_AudioAction(ClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.AudioAction>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.AudioAction)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Keyframe_AudioAction(ClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.AudioAction>();
			Read_Lime__Keyframe_AudioAction(def, result);
			return result;
		}

		private void Read_Lime__Keyframe_SerializableSample(ClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.SerializableSample>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.SerializableSample)ReadObject<global::Lime.SerializableSample>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Keyframe_SerializableSample(ClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.SerializableSample>();
			Read_Lime__Keyframe_SerializableSample(def, result);
			return result;
		}

		private void Read_Lime__Keyframe_HAlignment(ClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.HAlignment>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.HAlignment)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Keyframe_HAlignment(ClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.HAlignment>();
			Read_Lime__Keyframe_HAlignment(def, result);
			return result;
		}

		private void Read_Lime__Keyframe_VAlignment(ClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.VAlignment>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.VAlignment)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Keyframe_VAlignment(ClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.VAlignment>();
			Read_Lime__Keyframe_VAlignment(def, result);
			return result;
		}

		private void Read_Lime__Keyframe_MovieAction(ClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.MovieAction>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.MovieAction)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Keyframe_MovieAction(ClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.MovieAction>();
			Read_Lime__Keyframe_MovieAction(def, result);
			return result;
		}

		private void Read_Lime__Keyframe_ShaderId(ClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.ShaderId>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.ShaderId)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Keyframe_ShaderId(ClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.ShaderId>();
			Read_Lime__Keyframe_ShaderId(def, result);
			return result;
		}

		private void Read_Lime__Keyframe_Vector3(ClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.Vector3>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.Vector3)ReadStruct<global::Lime.Vector3>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Keyframe_Vector3(ClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.Vector3>();
			Read_Lime__Keyframe_Vector3(def, result);
			return result;
		}

		private void Read_Lime__Keyframe_Quaternion(ClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.Quaternion>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.Quaternion)ReadStruct<global::Lime.Quaternion>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Keyframe_Quaternion(ClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.Quaternion>();
			Read_Lime__Keyframe_Quaternion(def, result);
			return result;
		}

		private void Read_Lime__Keyframe_EmissionType(ClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.EmissionType>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.EmissionType)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Keyframe_EmissionType(ClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.EmissionType>();
			Read_Lime__Keyframe_EmissionType(def, result);
			return result;
		}

		private void Read_Lime__Keyframe_Matrix44(ClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.Matrix44>)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Frame = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Function = (global::Lime.KeyFunction)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Value = (global::Lime.Matrix44)ReadStruct<global::Lime.Matrix44>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Keyframe_Matrix44(ClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.Matrix44>();
			Read_Lime__Keyframe_Matrix44(def, result);
			return result;
		}

		private void Read_Lime__Audio(ClassDef def, object obj)
		{
			var result = (global::Lime.Audio)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Bumpable = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.FadeTime = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Group = (global::Lime.AudioChannelGroup)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Looping = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Pan = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Pitch = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Priority = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Sample = (global::Lime.SerializableSample)ReadObject<global::Lime.SerializableSample>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Volume = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Audio(ClassDef def)
		{
			var result = new global::Lime.Audio();
			Read_Lime__Audio(def, result);
			return result;
		}

		private object Make_Lime__BoneWeight(ClassDef def)
		{
			var result = new global::Lime.BoneWeight();
			result.Index = Reader.ReadInt32();
			result.Weight = Reader.ReadSingle();
			return result;
		}

		private void Read_Lime__SkinningWeights(ClassDef def, object obj)
		{
			var result = (global::Lime.SkinningWeights)obj;
			result.Bone0 = (global::Lime.BoneWeight)ReadStruct<global::Lime.BoneWeight>();
			result.Bone1 = (global::Lime.BoneWeight)ReadStruct<global::Lime.BoneWeight>();
			result.Bone2 = (global::Lime.BoneWeight)ReadStruct<global::Lime.BoneWeight>();
			result.Bone3 = (global::Lime.BoneWeight)ReadStruct<global::Lime.BoneWeight>();
		}

		private object Make_Lime__SkinningWeights(ClassDef def)
		{
			var result = new global::Lime.SkinningWeights();
			Read_Lime__SkinningWeights(def, result);
			return result;
		}

		private void Read_Lime__Bone(ClassDef def, object obj)
		{
			var result = (global::Lime.Bone)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.BaseIndex = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.EffectiveRadius = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.FadeoutZone = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.IKStopper = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Index = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Length = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.RefLength = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.RefPosition = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.RefRotation = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Rotation = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Bone(ClassDef def)
		{
			var result = new global::Lime.Bone();
			Read_Lime__Bone(def, result);
			return result;
		}

		private object Make_Lime__BoneArray(ClassDef def)
		{
			var result = new global::Lime.BoneArray();
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.items = (global::Lime.BoneArray.Entry[])null;
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					var tmp2 = new global::Lime.BoneArray.Entry[tmp1];
					for(tmp1 = 0; tmp1 < tmp2.Length; ++tmp1) {
						tmp2[tmp1] = (global::Lime.BoneArray.Entry)ReadStruct<global::Lime.BoneArray.Entry>();
					}
					result.items = tmp2;
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
			return result;
		}

		private object Make_Lime__BoneArray__Entry(ClassDef def)
		{
			var result = new global::Lime.BoneArray.Entry();
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Joint = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Length = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.RelativeTransform = (global::Lime.Matrix32)ReadStruct<global::Lime.Matrix32>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Rotation = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.Tip = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
			return result;
		}

		private void Read_Lime__Button(ClassDef def, object obj)
		{
			var result = (global::Lime.Button)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Draggable = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Enabled = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Rotation = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.Text = Reader.ReadString();
				if (result.Text == "" && Reader.ReadBoolean()) result.Text = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Visible = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Button(ClassDef def)
		{
			var result = new global::Lime.Button();
			Read_Lime__Button(def, result);
			return result;
		}

		private void Read_Lime__DistortionMesh(ClassDef def, object obj)
		{
			var result = (global::Lime.DistortionMesh)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.NumCols = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.NumRows = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Rotation = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.Texture = (global::Lime.ITexture)ReadObject<global::Lime.ITexture>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Visible = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__DistortionMesh(ClassDef def)
		{
			var result = new global::Lime.DistortionMesh();
			Read_Lime__DistortionMesh(def, result);
			return result;
		}

		private void Read_Lime__DistortionMeshPoint(ClassDef def, object obj)
		{
			var result = (global::Lime.DistortionMeshPoint)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Offset = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.UV = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__DistortionMeshPoint(ClassDef def)
		{
			var result = new global::Lime.DistortionMeshPoint();
			Read_Lime__DistortionMeshPoint(def, result);
			return result;
		}

		private void Read_Lime__Frame(ClassDef def, object obj)
		{
			var result = (global::Lime.Frame)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.RenderTarget = (global::Lime.RenderTarget)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Rotation = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.Visible = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Frame(ClassDef def)
		{
			var result = new global::Lime.Frame();
			Read_Lime__Frame(def, result);
			return result;
		}

		private void Read_Lime__Image(ClassDef def, object obj)
		{
			var result = (global::Lime.Image)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Rotation = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Texture = (global::Lime.ITexture)ReadObject<global::Lime.ITexture>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.UV0 = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.UV1 = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Visible = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Image(ClassDef def)
		{
			var result = new global::Lime.Image();
			Read_Lime__Image(def, result);
			return result;
		}

		private void Read_Lime__ImageCombiner(ClassDef def, object obj)
		{
			var result = (global::Lime.ImageCombiner)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.Enabled = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__ImageCombiner(ClassDef def)
		{
			var result = new global::Lime.ImageCombiner();
			Read_Lime__ImageCombiner(def, result);
			return result;
		}

		private void Read_Lime__Marker(ClassDef def, object obj)
		{
			var result = (global::Lime.Marker)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Action = (global::Lime.MarkerAction)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Frame = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.JumpTo = Reader.ReadString();
				if (result.JumpTo == "" && Reader.ReadBoolean()) result.JumpTo = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Marker(ClassDef def)
		{
			var result = new global::Lime.Marker();
			Read_Lime__Marker(def, result);
			return result;
		}

		private void Read_Lime__Movie(ClassDef def, object obj)
		{
			var result = (global::Lime.Movie)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Looped = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Path = Reader.ReadString();
				if (result.Path == "" && Reader.ReadBoolean()) result.Path = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Rotation = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.Visible = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Movie(ClassDef def)
		{
			var result = new global::Lime.Movie();
			Read_Lime__Movie(def, result);
			return result;
		}

		private void Read_Lime__NineGrid(ClassDef def, object obj)
		{
			var result = (global::Lime.NineGrid)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.BottomOffset = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.LeftOffset = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.RightOffset = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Rotation = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Texture = (global::Lime.ITexture)ReadObject<global::Lime.ITexture>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				result.TopOffset = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
				result.Visible = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__NineGrid(ClassDef def)
		{
			var result = new global::Lime.NineGrid();
			Read_Lime__NineGrid(def, result);
			return result;
		}

		private void Read_Lime__Node(ClassDef def, object obj)
		{
			var result = (global::Lime.Node)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Node(ClassDef def)
		{
			var result = new global::Lime.Node();
			Read_Lime__Node(def, result);
			return result;
		}

		private void Read_Lime__ParticleEmitter(ClassDef def, object obj)
		{
			var result = (global::Lime.ParticleEmitter)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AlongPathOrientation = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.AngularVelocity = (global::Lime.NumericRange)ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.AspectRatio = (global::Lime.NumericRange)ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Direction = (global::Lime.NumericRange)ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.EmissionType = (global::Lime.EmissionType)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.GravityAmount = (global::Lime.NumericRange)ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.GravityDirection = (global::Lime.NumericRange)ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.ImmortalParticles = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.Lifetime = (global::Lime.NumericRange)ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.LinkageWidgetName = Reader.ReadString();
				if (result.LinkageWidgetName == "" && Reader.ReadBoolean()) result.LinkageWidgetName = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.MagnetAmount = (global::Lime.NumericRange)ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Number = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				result.Orientation = (global::Lime.NumericRange)ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
				result.ParticlesLinkage = (global::Lime.ParticlesLinkage)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (25 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (26 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (27 == fd.OurIndex) {
				result.RandomMotionAspectRatio = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (28 == fd.OurIndex) {
				result.RandomMotionRadius = (global::Lime.NumericRange)ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (29 == fd.OurIndex) {
				result.RandomMotionRotation = (global::Lime.NumericRange)ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (30 == fd.OurIndex) {
				result.RandomMotionSpeed = (global::Lime.NumericRange)ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (31 == fd.OurIndex) {
				result.Rotation = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (32 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (33 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (34 == fd.OurIndex) {
				result.Shape = (global::Lime.EmitterShape)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (35 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (36 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (37 == fd.OurIndex) {
				result.Speed = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (38 == fd.OurIndex) {
				result.Spin = (global::Lime.NumericRange)ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (39 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (40 == fd.OurIndex) {
				result.TimeShift = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (41 == fd.OurIndex) {
				result.Velocity = (global::Lime.NumericRange)ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (42 == fd.OurIndex) {
				result.Visible = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (43 == fd.OurIndex) {
				result.WindAmount = (global::Lime.NumericRange)ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (44 == fd.OurIndex) {
				result.WindDirection = (global::Lime.NumericRange)ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (45 == fd.OurIndex) {
				result.Zoom = (global::Lime.NumericRange)ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__ParticleEmitter(ClassDef def)
		{
			var result = new global::Lime.ParticleEmitter();
			Read_Lime__ParticleEmitter(def, result);
			return result;
		}

		private void Read_Lime__ParticleModifier(ClassDef def, object obj)
		{
			var result = (global::Lime.ParticleModifier)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AngularVelocity = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.AnimationFps = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.AspectRatio = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.FirstFrame = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.GravityAmount = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.LastFrame = Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.LoopedAnimation = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.MagnetAmount = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Scale = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Spin = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.Texture = (global::Lime.ITexture)ReadObject<global::Lime.ITexture>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Velocity = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.WindAmount = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__ParticleModifier(ClassDef def)
		{
			var result = new global::Lime.ParticleModifier();
			Read_Lime__ParticleModifier(def, result);
			return result;
		}

		private void Read_Lime__ParticlesMagnet(ClassDef def, object obj)
		{
			var result = (global::Lime.ParticlesMagnet)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Attenuation = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Rotation = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Shape = (global::Lime.EmitterShape)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.Strength = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Visible = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__ParticlesMagnet(ClassDef def)
		{
			var result = new global::Lime.ParticlesMagnet();
			Read_Lime__ParticlesMagnet(def, result);
			return result;
		}

		private void Read_Lime__PointObject(ClassDef def, object obj)
		{
			var result = (global::Lime.PointObject)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__PointObject(ClassDef def)
		{
			var result = new global::Lime.PointObject();
			Read_Lime__PointObject(def, result);
			return result;
		}

		private void Read_Lime__Slider(ClassDef def, object obj)
		{
			var result = (global::Lime.Slider)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.RangeMax = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.RangeMin = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Rotation = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.Step = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Value = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				result.Visible = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Slider(ClassDef def)
		{
			var result = new global::Lime.Slider();
			Read_Lime__Slider(def, result);
			return result;
		}

		private void Read_Lime__SplineGear(ClassDef def, object obj)
		{
			var result = (global::Lime.SplineGear)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.SplineId = Reader.ReadString();
				if (result.SplineId == "" && Reader.ReadBoolean()) result.SplineId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.SplineOffset = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.WidgetId = Reader.ReadString();
				if (result.WidgetId == "" && Reader.ReadBoolean()) result.WidgetId = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__SplineGear(ClassDef def)
		{
			var result = new global::Lime.SplineGear();
			Read_Lime__SplineGear(def, result);
			return result;
		}

		private void Read_Lime__SplinePoint(ClassDef def, object obj)
		{
			var result = (global::Lime.SplinePoint)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Straight = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.TangentAngle = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.TangentWeight = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__SplinePoint(ClassDef def)
		{
			var result = new global::Lime.SplinePoint();
			Read_Lime__SplinePoint(def, result);
			return result;
		}

		private void Read_Lime__RichText(ClassDef def, object obj)
		{
			var result = (global::Lime.RichText)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.HAlignment = (global::Lime.HAlignment)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.OverflowMode = (global::Lime.TextOverflowMode)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Rotation = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.Text = Reader.ReadString();
				if (result.Text == "" && Reader.ReadBoolean()) result.Text = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.VAlignment = (global::Lime.VAlignment)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				result.Visible = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
				result.WordSplitAllowed = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__RichText(ClassDef def)
		{
			var result = new global::Lime.RichText();
			Read_Lime__RichText(def, result);
			return result;
		}

		private void Read_Lime__SimpleText(ClassDef def, object obj)
		{
			var result = (global::Lime.SimpleText)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Font = (global::Lime.SerializableFont)ReadObject<global::Lime.SerializableFont>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.FontHeight = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.HAlignment = (global::Lime.HAlignment)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.OverflowMode = (global::Lime.TextOverflowMode)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Rotation = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Spacing = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
				result.Text = Reader.ReadString();
				if (result.Text == "" && Reader.ReadBoolean()) result.Text = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (25 == fd.OurIndex) {
				result.TextColor = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (26 == fd.OurIndex) {
				result.VAlignment = (global::Lime.VAlignment)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (27 == fd.OurIndex) {
				result.Visible = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (28 == fd.OurIndex) {
				result.WordSplitAllowed = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__SimpleText(ClassDef def)
		{
			var result = new global::Lime.SimpleText();
			Read_Lime__SimpleText(def, result);
			return result;
		}

		private void Read_Lime__TextStyle(ClassDef def, object obj)
		{
			var result = (global::Lime.TextStyle)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Bold = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.CastShadow = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Font = (global::Lime.SerializableFont)ReadObject<global::Lime.SerializableFont>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.ImageSize = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.ImageTexture = (global::Lime.ITexture)ReadObject<global::Lime.ITexture>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.ImageUsage = (global::Lime.TextStyle.ImageUsageEnum)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.ShadowColor = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.ShadowOffset = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Size = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.SpaceAfter = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.TextColor = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__TextStyle(ClassDef def)
		{
			var result = new global::Lime.TextStyle();
			Read_Lime__TextStyle(def, result);
			return result;
		}

		private void Read_Lime__Widget(ClassDef def, object obj)
		{
			var result = (global::Lime.Widget)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Anchors = (global::Lime.Anchors)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				var tmp1 = Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.Animation)ReadObject<global::Lime.Animation>();
						result.Animations.Add(tmp2);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				var tmp3 = Reader.ReadInt32();
				if (tmp3 >= 0) {
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.IAnimator)ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp4);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BoneArray = (global::Lime.BoneArray)ReadStruct<global::Lime.BoneArray>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Color = (global::Lime.Color4)ReadStruct<global::Lime.Color4>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = Reader.ReadString();
				if (result.ContentsPath == "" && Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Id = Reader.ReadString();
				if (result.Id == "" && Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				var tmp5 = Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.Node)ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp6);
					}
				}
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Pivot = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Position = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Rotation = Reader.ReadSingle();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Scale = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)Reader.ReadInt32();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.SilentSize = (global::Lime.Vector2)ReadStruct<global::Lime.Vector2>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.Tag = Reader.ReadString();
				if (result.Tag == "" && Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Visible = Reader.ReadBoolean();
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__Widget(ClassDef def)
		{
			var result = new global::Lime.Widget();
			Read_Lime__Widget(def, result);
			return result;
		}

		private void Read_Lime__SerializableTexture(ClassDef def, object obj)
		{
			var result = (global::Lime.SerializableTexture)obj;
			ClassDef.FieldDef fd;
			fd = def.Fields[Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.SerializationPath = Reader.ReadString();
				if (result.SerializationPath == "" && Reader.ReadBoolean()) result.SerializationPath = null;
				fd = def.Fields[Reader.ReadInt16()];
			}
			if (fd.OurIndex != ClassDef.EOF) throw Error("Unfinished object");
		}

		private object Make_Lime__SerializableTexture(ClassDef def)
		{
			var result = new global::Lime.SerializableTexture();
			Read_Lime__SerializableTexture(def, result);
			return result;
		}

		public BinaryDeserializerGen()
		{
			readFieldsCache[typeof(global::Lime.Font)] = Read_Lime__Font;
			readFieldsCache[typeof(global::Lime.SerializableSample)] = Read_Lime__SerializableSample;
			readFieldsCache[typeof(global::Lime.FontChar)] = Read_Lime__FontChar;
			readFieldsCache[typeof(global::Lime.SerializableFont)] = Read_Lime__SerializableFont;
			readFieldsCache[typeof(global::Lime.GeometryBuffer)] = Read_Lime__GeometryBuffer;
			readFieldsCache[typeof(global::Lime.Camera3D)] = Read_Lime__Camera3D;
			readFieldsCache[typeof(global::Lime.Material)] = Read_Lime__Material;
			readFieldsCache[typeof(global::Lime.Mesh3D)] = Read_Lime__Mesh3D;
			readFieldsCache[typeof(global::Lime.Submesh3D)] = Read_Lime__Submesh3D;
			readFieldsCache[typeof(global::Lime.Node3D)] = Read_Lime__Node3D;
			readFieldsCache[typeof(global::Lime.Spline3D)] = Read_Lime__Spline3D;
			readFieldsCache[typeof(global::Lime.Spline3D.Point)] = Read_Lime__Spline3D__Point;
			readFieldsCache[typeof(global::Lime.Viewport3D)] = Read_Lime__Viewport3D;
			readFieldsCache[typeof(global::Lime.Animation)] = Read_Lime__Animation;
			readFieldsCache[typeof(global::Lime.Spline)] = Read_Lime__Spline;
			readFieldsCache[typeof(global::Lime.LinearLayout)] = Read_Lime__LinearLayout;
			readFieldsCache[typeof(global::Lime.Animator<global::System.String>)] = Read_Lime__Animator_String;
			readFieldsCache[typeof(global::Lime.Animator<global::System.Int32>)] = Read_Lime__Animator_Int32;
			readFieldsCache[typeof(global::Lime.Animator<global::System.Boolean>)] = Read_Lime__Animator_Boolean;
			readFieldsCache[typeof(global::Lime.Animator<global::Lime.Blending>)] = Read_Lime__Animator_Blending;
			readFieldsCache[typeof(global::Lime.Animator<global::Lime.ITexture>)] = Read_Lime__Animator_ITexture;
			readFieldsCache[typeof(global::Lime.Animator<global::Lime.NumericRange>)] = Read_Lime__Animator_NumericRange;
			readFieldsCache[typeof(global::Lime.Animator<global::Lime.Vector2>)] = Read_Lime__Animator_Vector2;
			readFieldsCache[typeof(global::Lime.Animator<global::Lime.Color4>)] = Read_Lime__Animator_Color4;
			readFieldsCache[typeof(global::Lime.Animator<global::System.Single>)] = Read_Lime__Animator_Single;
			readFieldsCache[typeof(global::Lime.Animator<global::Lime.EmitterShape>)] = Read_Lime__Animator_EmitterShape;
			readFieldsCache[typeof(global::Lime.Animator<global::Lime.AudioAction>)] = Read_Lime__Animator_AudioAction;
			readFieldsCache[typeof(global::Lime.Animator<global::Lime.SerializableSample>)] = Read_Lime__Animator_SerializableSample;
			readFieldsCache[typeof(global::Lime.Animator<global::Lime.HAlignment>)] = Read_Lime__Animator_HAlignment;
			readFieldsCache[typeof(global::Lime.Animator<global::Lime.VAlignment>)] = Read_Lime__Animator_VAlignment;
			readFieldsCache[typeof(global::Lime.Animator<global::Lime.MovieAction>)] = Read_Lime__Animator_MovieAction;
			readFieldsCache[typeof(global::Lime.Animator<global::Lime.ShaderId>)] = Read_Lime__Animator_ShaderId;
			readFieldsCache[typeof(global::Lime.Animator<global::Lime.Vector3>)] = Read_Lime__Animator_Vector3;
			readFieldsCache[typeof(global::Lime.Animator<global::Lime.Quaternion>)] = Read_Lime__Animator_Quaternion;
			readFieldsCache[typeof(global::Lime.Animator<global::Lime.EmissionType>)] = Read_Lime__Animator_EmissionType;
			readFieldsCache[typeof(global::Lime.NumericAnimator)] = Read_Lime__NumericAnimator;
			readFieldsCache[typeof(global::Lime.Vector2Animator)] = Read_Lime__Vector2Animator;
			readFieldsCache[typeof(global::Lime.Color4Animator)] = Read_Lime__Color4Animator;
			readFieldsCache[typeof(global::Lime.QuaternionAnimator)] = Read_Lime__QuaternionAnimator;
			readFieldsCache[typeof(global::Lime.Vector3Animator)] = Read_Lime__Vector3Animator;
			readFieldsCache[typeof(global::Lime.Matrix44Animator)] = Read_Lime__Matrix44Animator;
			readFieldsCache[typeof(global::Lime.Keyframe<global::System.String>)] = Read_Lime__Keyframe_String;
			readFieldsCache[typeof(global::Lime.Keyframe<global::System.Int32>)] = Read_Lime__Keyframe_Int32;
			readFieldsCache[typeof(global::Lime.Keyframe<global::System.Boolean>)] = Read_Lime__Keyframe_Boolean;
			readFieldsCache[typeof(global::Lime.Keyframe<global::Lime.Blending>)] = Read_Lime__Keyframe_Blending;
			readFieldsCache[typeof(global::Lime.Keyframe<global::Lime.ITexture>)] = Read_Lime__Keyframe_ITexture;
			readFieldsCache[typeof(global::Lime.Keyframe<global::Lime.NumericRange>)] = Read_Lime__Keyframe_NumericRange;
			readFieldsCache[typeof(global::Lime.Keyframe<global::Lime.Vector2>)] = Read_Lime__Keyframe_Vector2;
			readFieldsCache[typeof(global::Lime.Keyframe<global::Lime.Color4>)] = Read_Lime__Keyframe_Color4;
			readFieldsCache[typeof(global::Lime.Keyframe<global::System.Single>)] = Read_Lime__Keyframe_Single;
			readFieldsCache[typeof(global::Lime.Keyframe<global::Lime.EmitterShape>)] = Read_Lime__Keyframe_EmitterShape;
			readFieldsCache[typeof(global::Lime.Keyframe<global::Lime.AudioAction>)] = Read_Lime__Keyframe_AudioAction;
			readFieldsCache[typeof(global::Lime.Keyframe<global::Lime.SerializableSample>)] = Read_Lime__Keyframe_SerializableSample;
			readFieldsCache[typeof(global::Lime.Keyframe<global::Lime.HAlignment>)] = Read_Lime__Keyframe_HAlignment;
			readFieldsCache[typeof(global::Lime.Keyframe<global::Lime.VAlignment>)] = Read_Lime__Keyframe_VAlignment;
			readFieldsCache[typeof(global::Lime.Keyframe<global::Lime.MovieAction>)] = Read_Lime__Keyframe_MovieAction;
			readFieldsCache[typeof(global::Lime.Keyframe<global::Lime.ShaderId>)] = Read_Lime__Keyframe_ShaderId;
			readFieldsCache[typeof(global::Lime.Keyframe<global::Lime.Vector3>)] = Read_Lime__Keyframe_Vector3;
			readFieldsCache[typeof(global::Lime.Keyframe<global::Lime.Quaternion>)] = Read_Lime__Keyframe_Quaternion;
			readFieldsCache[typeof(global::Lime.Keyframe<global::Lime.EmissionType>)] = Read_Lime__Keyframe_EmissionType;
			readFieldsCache[typeof(global::Lime.Keyframe<global::Lime.Matrix44>)] = Read_Lime__Keyframe_Matrix44;
			readFieldsCache[typeof(global::Lime.Audio)] = Read_Lime__Audio;
			readFieldsCache[typeof(global::Lime.SkinningWeights)] = Read_Lime__SkinningWeights;
			readFieldsCache[typeof(global::Lime.Bone)] = Read_Lime__Bone;
			readFieldsCache[typeof(global::Lime.Button)] = Read_Lime__Button;
			readFieldsCache[typeof(global::Lime.DistortionMesh)] = Read_Lime__DistortionMesh;
			readFieldsCache[typeof(global::Lime.DistortionMeshPoint)] = Read_Lime__DistortionMeshPoint;
			readFieldsCache[typeof(global::Lime.Frame)] = Read_Lime__Frame;
			readFieldsCache[typeof(global::Lime.Image)] = Read_Lime__Image;
			readFieldsCache[typeof(global::Lime.ImageCombiner)] = Read_Lime__ImageCombiner;
			readFieldsCache[typeof(global::Lime.Marker)] = Read_Lime__Marker;
			readFieldsCache[typeof(global::Lime.Movie)] = Read_Lime__Movie;
			readFieldsCache[typeof(global::Lime.NineGrid)] = Read_Lime__NineGrid;
			readFieldsCache[typeof(global::Lime.Node)] = Read_Lime__Node;
			readFieldsCache[typeof(global::Lime.ParticleEmitter)] = Read_Lime__ParticleEmitter;
			readFieldsCache[typeof(global::Lime.ParticleModifier)] = Read_Lime__ParticleModifier;
			readFieldsCache[typeof(global::Lime.ParticlesMagnet)] = Read_Lime__ParticlesMagnet;
			readFieldsCache[typeof(global::Lime.PointObject)] = Read_Lime__PointObject;
			readFieldsCache[typeof(global::Lime.Slider)] = Read_Lime__Slider;
			readFieldsCache[typeof(global::Lime.SplineGear)] = Read_Lime__SplineGear;
			readFieldsCache[typeof(global::Lime.SplinePoint)] = Read_Lime__SplinePoint;
			readFieldsCache[typeof(global::Lime.RichText)] = Read_Lime__RichText;
			readFieldsCache[typeof(global::Lime.SimpleText)] = Read_Lime__SimpleText;
			readFieldsCache[typeof(global::Lime.TextStyle)] = Read_Lime__TextStyle;
			readFieldsCache[typeof(global::Lime.Widget)] = Read_Lime__Widget;
			readFieldsCache[typeof(global::Lime.SerializableTexture)] = Read_Lime__SerializableTexture;
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
