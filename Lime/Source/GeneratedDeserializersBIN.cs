using System;
using System.Reflection;

using Yuzu;
using Yuzu.Binary;

namespace GeneratedDeserializersBIN
{
	public class BinaryDeserializerGen: BinaryDeserializerGenBase
	{
		private static object Make_Lime__Alignment(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Alignment();
			result.X = (global::Lime.HAlignment)d.Reader.ReadInt32();
			result.Y = (global::Lime.VAlignment)d.Reader.ReadInt32();
			return result;
		}

		private static void Read_Lime__Animation(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animation)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.IsLegacy = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
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

		private static void Read_Lime__Animation__AnimationData(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animation.AnimationData)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.IAnimator)dg.ReadObject<global::Lime.IAnimator>();
						result.Animators.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animation__AnimationData(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animation.AnimationData();
			Read_Lime__Animation__AnimationData(d, def, result);
			return result;
		}

		private static void Read_Lime__AnimationBlender(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.AnimationBlender)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					while (--tmp1 >= 0) {
						var tmp2 = d.Reader.ReadString();
						if (tmp2 == "" && d.Reader.ReadBoolean()) tmp2 = null;
						var tmp3 = (global::Lime.AnimationBlending)dg.ReadObject<global::Lime.AnimationBlending>();
						result.Options.Add(tmp2, tmp3);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__AnimationBlender(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.AnimationBlender();
			Read_Lime__AnimationBlender(d, def, result);
			return result;
		}

		private static void Read_Lime__AnimationBlending(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.AnimationBlending)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.MarkersOptions = (global::System.Collections.Generic.Dictionary<global::System.String,global::Lime.MarkerBlending>)null;
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.MarkersOptions = new global::System.Collections.Generic.Dictionary<global::System.String,global::Lime.MarkerBlending>();
					while (--tmp1 >= 0) {
						var tmp2 = d.Reader.ReadString();
						if (tmp2 == "" && d.Reader.ReadBoolean()) tmp2 = null;
						var tmp3 = (global::Lime.MarkerBlending)dg.ReadObject<global::Lime.MarkerBlending>();
						result.MarkersOptions.Add(tmp2, tmp3);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Option = (global::Lime.BlendingOption)dg.ReadObject<global::Lime.BlendingOption>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__AnimationBlending(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.AnimationBlending();
			Read_Lime__AnimationBlending(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_Alignment(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.Alignment>)obj;
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
						var tmp2 = (global::Lime.Keyframe<global::Lime.Alignment>)dg.ReadObject<global::Lime.Keyframe<global::Lime.Alignment>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_Alignment(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.Alignment>();
			Read_Lime__Animator_Alignment(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_Anchors(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.Anchors>)obj;
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
						var tmp2 = (global::Lime.Keyframe<global::Lime.Anchors>)dg.ReadObject<global::Lime.Keyframe<global::Lime.Anchors>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_Anchors(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.Anchors>();
			Read_Lime__Animator_Anchors(d, def, result);
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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

		private static void Read_Lime__Animator_ClipMethod(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.ClipMethod>)obj;
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
						var tmp2 = (global::Lime.Keyframe<global::Lime.ClipMethod>)dg.ReadObject<global::Lime.Keyframe<global::Lime.ClipMethod>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_ClipMethod(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.ClipMethod>();
			Read_Lime__Animator_ClipMethod(d, def, result);
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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

		private static void Read_Lime__Animator_LayoutDirection(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.LayoutDirection>)obj;
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
						var tmp2 = (global::Lime.Keyframe<global::Lime.LayoutDirection>)dg.ReadObject<global::Lime.Keyframe<global::Lime.LayoutDirection>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_LayoutDirection(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.LayoutDirection>();
			Read_Lime__Animator_LayoutDirection(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_Matrix44(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.Matrix44>)obj;
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_Matrix44(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.Matrix44>();
			Read_Lime__Animator_Matrix44(d, def, result);
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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

		private static void Read_Lime__Animator_NodeReference_Camera3D(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.NodeReference<global::Lime.Camera3D>>)obj;
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
						var tmp2 = (global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Camera3D>>)dg.ReadObject<global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Camera3D>>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_NodeReference_Camera3D(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.NodeReference<global::Lime.Camera3D>>();
			Read_Lime__Animator_NodeReference_Camera3D(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_NodeReference_Node3D(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.NodeReference<global::Lime.Node3D>>)obj;
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
						var tmp2 = (global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Node3D>>)dg.ReadObject<global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Node3D>>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_NodeReference_Node3D(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.NodeReference<global::Lime.Node3D>>();
			Read_Lime__Animator_NodeReference_Node3D(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_NodeReference_Spline(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.NodeReference<global::Lime.Spline>>)obj;
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
						var tmp2 = (global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline>>)dg.ReadObject<global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline>>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_NodeReference_Spline(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.NodeReference<global::Lime.Spline>>();
			Read_Lime__Animator_NodeReference_Spline(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_NodeReference_Spline3D(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.NodeReference<global::Lime.Spline3D>>)obj;
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
						var tmp2 = (global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline3D>>)dg.ReadObject<global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline3D>>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_NodeReference_Spline3D(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.NodeReference<global::Lime.Spline3D>>();
			Read_Lime__Animator_NodeReference_Spline3D(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_NodeReference_Widget(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.NodeReference<global::Lime.Widget>>)obj;
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
						var tmp2 = (global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Widget>>)dg.ReadObject<global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Widget>>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_NodeReference_Widget(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.NodeReference<global::Lime.Widget>>();
			Read_Lime__Animator_NodeReference_Widget(d, def, result);
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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

		private static void Read_Lime__Animator_ParticlesLinkage(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.ParticlesLinkage>)obj;
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
						var tmp2 = (global::Lime.Keyframe<global::Lime.ParticlesLinkage>)dg.ReadObject<global::Lime.Keyframe<global::Lime.ParticlesLinkage>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_ParticlesLinkage(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.ParticlesLinkage>();
			Read_Lime__Animator_ParticlesLinkage(d, def, result);
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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

		private static void Read_Lime__Animator_RenderTarget(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.RenderTarget>)obj;
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
						var tmp2 = (global::Lime.Keyframe<global::Lime.RenderTarget>)dg.ReadObject<global::Lime.Keyframe<global::Lime.RenderTarget>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_RenderTarget(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.RenderTarget>();
			Read_Lime__Animator_RenderTarget(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_SerializableFont(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.SerializableFont>)obj;
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
						var tmp2 = (global::Lime.Keyframe<global::Lime.SerializableFont>)dg.ReadObject<global::Lime.Keyframe<global::Lime.SerializableFont>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_SerializableFont(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.SerializableFont>();
			Read_Lime__Animator_SerializableFont(d, def, result);
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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

		private static void Read_Lime__Animator_TextOverflowMode(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.TextOverflowMode>)obj;
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
						var tmp2 = (global::Lime.Keyframe<global::Lime.TextOverflowMode>)dg.ReadObject<global::Lime.Keyframe<global::Lime.TextOverflowMode>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_TextOverflowMode(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.TextOverflowMode>();
			Read_Lime__Animator_TextOverflowMode(d, def, result);
			return result;
		}

		private static void Read_Lime__Animator_Thickness(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Animator<global::Lime.Thickness>)obj;
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
						var tmp2 = (global::Lime.Keyframe<global::Lime.Thickness>)dg.ReadObject<global::Lime.Keyframe<global::Lime.Thickness>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Animator_Thickness(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Animator<global::Lime.Thickness>();
			Read_Lime__Animator_Thickness(d, def, result);
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.Continuous = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.FadeTime = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp7 = d.Reader.ReadInt32();
				if (tmp7 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp7 >= 0) {
						var tmp8 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp8);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Group = (global::Lime.AudioChannelGroup)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Looping = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				var tmp9 = d.Reader.ReadInt32();
				if (tmp9 >= 0) {
					while (--tmp9 >= 0) {
						var tmp10 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp10);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Pan = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Pitch = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Priority = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Sample = (global::Lime.SerializableSample)dg.ReadObject<global::Lime.SerializableSample>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
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

		private static void Read_Lime__BaseShadowParams(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.BaseShadowParams)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp1 = new global::Lime.Color4();
				tmp1.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp1;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Dilate = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Enabled = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.OffsetX = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.OffsetY = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Softness = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__BaseShadowParams(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.BaseShadowParams();
			Read_Lime__BaseShadowParams(d, def, result);
			return result;
		}

		private static object Make_Lime__BitSet32(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.BitSet32();
			result.Value = d.Reader.ReadUInt32();
			return result;
		}

		private static void Read_Lime__BlendingOption(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.BlendingOption)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Duration = d.Reader.ReadDouble();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__BlendingOption(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.BlendingOption();
			Read_Lime__BlendingOption(d, def, result);
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
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.EffectiveRadius = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.FadeoutZone = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp7 = d.Reader.ReadInt32();
				if (tmp7 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp7 >= 0) {
						var tmp8 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp8);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.IKStopper = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Index = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Length = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				var tmp9 = d.Reader.ReadInt32();
				if (tmp9 >= 0) {
					while (--tmp9 >= 0) {
						var tmp10 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp10);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp11 = new global::Lime.Vector2();
				tmp11.X = d.Reader.ReadSingle();
				tmp11.Y = d.Reader.ReadSingle();
				result.Position = tmp11;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.RefLength = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp12 = new global::Lime.Vector2();
				tmp12.X = d.Reader.ReadSingle();
				tmp12.Y = d.Reader.ReadSingle();
				result.RefPosition = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.RefRotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
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

		private static object Make_Lime__BoneWeight(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.BoneWeight();
			result.Index = d.Reader.ReadInt32();
			result.Weight = d.Reader.ReadSingle();
			return result;
		}

		private static object Make_Lime__BoundingSphere(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.BoundingSphere();
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				result.Center.X = d.Reader.ReadSingle();
				result.Center.Y = d.Reader.ReadSingle();
				result.Center.Z = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Radius = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
			return result;
		}

		private static object Make_Lime__Bounds(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Bounds();
			var dg = (BinaryDeserializerGen)d;
			dg.EnsureClassDef(typeof(global::Lime.Vector3));
			result.A.X = d.Reader.ReadSingle();
			result.A.Y = d.Reader.ReadSingle();
			result.A.Z = d.Reader.ReadSingle();
			dg.EnsureClassDef(typeof(global::Lime.Vector3));
			result.B.X = d.Reader.ReadSingle();
			result.B.Y = d.Reader.ReadSingle();
			result.B.Z = d.Reader.ReadSingle();
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
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Enabled = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
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
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Thickness));
				var tmp12 = new global::Lime.Thickness();
				tmp12.Left = d.Reader.ReadSingle();
				tmp12.Right = d.Reader.ReadSingle();
				tmp12.Top = d.Reader.ReadSingle();
				tmp12.Bottom = d.Reader.ReadSingle();
				result.Padding = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp13 = new global::Lime.Vector2();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				result.Pivot = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp14 = new global::Lime.Vector2();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				result.Position = tmp14;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp15 = new global::Lime.Vector2();
				tmp15.X = d.Reader.ReadSingle();
				tmp15.Y = d.Reader.ReadSingle();
				result.Scale = tmp15;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp16 = new global::Lime.Vector2();
				tmp16.X = d.Reader.ReadSingle();
				tmp16.Y = d.Reader.ReadSingle();
				result.SilentSize = tmp16;
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
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				result.Text = d.Reader.ReadString();
				if (result.Text == "" && d.Reader.ReadBoolean()) result.Text = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
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
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.FarClipPlane = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.FieldOfView = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.NearClipPlane = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Opaque = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.OrthographicSize = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				var tmp12 = new global::Lime.Vector3();
				tmp12.X = d.Reader.ReadSingle();
				tmp12.Y = d.Reader.ReadSingle();
				tmp12.Z = d.Reader.ReadSingle();
				result.Position = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.ProjectionMode = (global::Lime.CameraProjectionMode)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Quaternion));
				var tmp13 = new global::Lime.Quaternion();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				tmp13.Z = d.Reader.ReadSingle();
				tmp13.W = d.Reader.ReadSingle();
				result.Rotation = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				var tmp14 = new global::Lime.Vector3();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				tmp14.Z = d.Reader.ReadSingle();
				result.Scale = tmp14;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
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

		private static object Make_Lime__Color4(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Color4();
			result.ABGR = d.Reader.ReadUInt32();
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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

		private static void Read_Lime__CommonMaterial(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.CommonMaterial)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp1 = new global::Lime.Color4();
				tmp1.ABGR = d.Reader.ReadUInt32();
				result.DiffuseColor = tmp1;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.DiffuseTexture = (global::Lime.ITexture)dg.ReadObject<global::Lime.ITexture>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp2 = new global::Lime.Color4();
				tmp2.ABGR = d.Reader.ReadUInt32();
				result.FogColor = tmp2;
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
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.SkinEnabled = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__CommonMaterial(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.CommonMaterial();
			Read_Lime__CommonMaterial(d, def, result);
			return result;
		}

		private static void Read_Lime__DefaultLayoutCell(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.DefaultLayoutCell)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Alignment));
				var tmp1 = new global::Lime.Alignment();
				tmp1.X = (global::Lime.HAlignment)d.Reader.ReadInt32();
				tmp1.Y = (global::Lime.VAlignment)d.Reader.ReadInt32();
				result.Alignment = tmp1;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ColumnSpan = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Ignore = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.RowSpan = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp2 = new global::Lime.Vector2();
				tmp2.X = d.Reader.ReadSingle();
				tmp2.Y = d.Reader.ReadSingle();
				result.Stretch = tmp2;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__DefaultLayoutCell(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.DefaultLayoutCell();
			Read_Lime__DefaultLayoutCell(d, def, result);
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
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
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
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.NumCols = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.NumRows = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Thickness));
				var tmp12 = new global::Lime.Thickness();
				tmp12.Left = d.Reader.ReadSingle();
				tmp12.Right = d.Reader.ReadSingle();
				tmp12.Top = d.Reader.ReadSingle();
				tmp12.Bottom = d.Reader.ReadSingle();
				result.Padding = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp13 = new global::Lime.Vector2();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				result.Pivot = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp14 = new global::Lime.Vector2();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				result.Position = tmp14;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp15 = new global::Lime.Vector2();
				tmp15.X = d.Reader.ReadSingle();
				tmp15.Y = d.Reader.ReadSingle();
				result.Scale = tmp15;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp16 = new global::Lime.Vector2();
				tmp16.X = d.Reader.ReadSingle();
				tmp16.Y = d.Reader.ReadSingle();
				result.SilentSize = tmp16;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
				result.Texture = (global::Lime.ITexture)dg.ReadObject<global::Lime.ITexture>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (25 == fd.OurIndex) {
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
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp12 = new global::Lime.Vector2();
				tmp12.X = d.Reader.ReadSingle();
				tmp12.Y = d.Reader.ReadSingle();
				result.Offset = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp13 = new global::Lime.Vector2();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				result.Position = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp14 = new global::Lime.Vector2();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				result.UV = tmp14;
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

		private static void Read_Lime__EmitterShapePoint(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.EmitterShapePoint)obj;
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
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp7 = d.Reader.ReadInt32();
				if (tmp7 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp7 >= 0) {
						var tmp8 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp8);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				var tmp9 = d.Reader.ReadInt32();
				if (tmp9 >= 0) {
					while (--tmp9 >= 0) {
						var tmp10 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp10);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp11 = new global::Lime.Vector2();
				tmp11.X = d.Reader.ReadSingle();
				tmp11.Y = d.Reader.ReadSingle();
				result.Position = tmp11;
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
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__EmitterShapePoint(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.EmitterShapePoint();
			Read_Lime__EmitterShapePoint(d, def, result);
			return result;
		}

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
				result.RoundCoordinates = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
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

		private static void Read_Lime__FontChar(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.FontChar)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				result.ACWidths.X = d.Reader.ReadSingle();
				result.ACWidths.Y = d.Reader.ReadSingle();
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
				result.RgbIntensity = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.TextureIndex = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				result.UV0.X = d.Reader.ReadSingle();
				result.UV0.Y = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				result.UV1.X = d.Reader.ReadSingle();
				result.UV1.Y = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.VerticalOffset = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
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
				result.ClipChildren = (global::Lime.ClipMethod)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
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
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Thickness));
				var tmp12 = new global::Lime.Thickness();
				tmp12.Left = d.Reader.ReadSingle();
				tmp12.Right = d.Reader.ReadSingle();
				tmp12.Top = d.Reader.ReadSingle();
				tmp12.Bottom = d.Reader.ReadSingle();
				result.Padding = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp13 = new global::Lime.Vector2();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				result.Pivot = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp14 = new global::Lime.Vector2();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				result.Position = tmp14;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.RenderTarget = (global::Lime.RenderTarget)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp15 = new global::Lime.Vector2();
				tmp15.X = d.Reader.ReadSingle();
				tmp15.Y = d.Reader.ReadSingle();
				result.Scale = tmp15;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp16 = new global::Lime.Vector2();
				tmp16.X = d.Reader.ReadSingle();
				tmp16.Y = d.Reader.ReadSingle();
				result.SilentSize = tmp16;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
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

		private static void Read_Lime__GradientComponent(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.GradientComponent)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Angle = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Blending = (global::Lime.Blending)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Gradient = (global::Lime.ColorGradient)null;
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.Gradient = new global::Lime.ColorGradient();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.GradientControlPoint)dg.ReadObject<global::Lime.GradientControlPoint>();
						result.Gradient.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__GradientComponent(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.GradientComponent();
			Read_Lime__GradientComponent(d, def, result);
			return result;
		}

		private static void Read_Lime__GradientControlPoint(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.GradientControlPoint)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp1 = new global::Lime.Color4();
				tmp1.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp1;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Position = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__GradientControlPoint(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.GradientControlPoint();
			Read_Lime__GradientControlPoint(d, def, result);
			return result;
		}

		private static void Read_Lime__HBoxLayout(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.HBoxLayout)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.DefaultCell = (global::Lime.DefaultLayoutCell)dg.ReadObject<global::Lime.DefaultLayoutCell>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Direction = (global::Lime.LayoutDirection)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.IgnoreHidden = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Spacing = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__HBoxLayout(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.HBoxLayout();
			Read_Lime__HBoxLayout(d, def, result);
			return result;
		}

		private static void Read_Lime__HSLComponent(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.HSLComponent)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Hue = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Lightness = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Saturation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__HSLComponent(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.HSLComponent();
			Read_Lime__HSLComponent(d, def, result);
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
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
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
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Thickness));
				var tmp12 = new global::Lime.Thickness();
				tmp12.Left = d.Reader.ReadSingle();
				tmp12.Right = d.Reader.ReadSingle();
				tmp12.Top = d.Reader.ReadSingle();
				tmp12.Bottom = d.Reader.ReadSingle();
				result.Padding = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp13 = new global::Lime.Vector2();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				result.Pivot = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp14 = new global::Lime.Vector2();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				result.Position = tmp14;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp15 = new global::Lime.Vector2();
				tmp15.X = d.Reader.ReadSingle();
				tmp15.Y = d.Reader.ReadSingle();
				result.Scale = tmp15;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp16 = new global::Lime.Vector2();
				tmp16.X = d.Reader.ReadSingle();
				tmp16.Y = d.Reader.ReadSingle();
				result.SilentSize = tmp16;
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
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Texture = (global::Lime.ITexture)dg.ReadObject<global::Lime.ITexture>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp17 = new global::Lime.Vector2();
				tmp17.X = d.Reader.ReadSingle();
				tmp17.Y = d.Reader.ReadSingle();
				result.UV0 = tmp17;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp18 = new global::Lime.Vector2();
				tmp18.X = d.Reader.ReadSingle();
				tmp18.Y = d.Reader.ReadSingle();
				result.UV1 = tmp18;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (25 == fd.OurIndex) {
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
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Enabled = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp7 = d.Reader.ReadInt32();
				if (tmp7 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp7 >= 0) {
						var tmp8 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp8);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				var tmp9 = d.Reader.ReadInt32();
				if (tmp9 >= 0) {
					while (--tmp9 >= 0) {
						var tmp10 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp10);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
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

		private static void Read_Lime__IntAnimator(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.IntAnimator)obj;
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__IntAnimator(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.IntAnimator();
			Read_Lime__IntAnimator(d, def, result);
			return result;
		}

		private static object Make_Lime__IntRectangle(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.IntRectangle();
			var dg = (BinaryDeserializerGen)d;
			dg.EnsureClassDef(typeof(global::Lime.IntVector2));
			result.A.X = d.Reader.ReadInt32();
			result.A.Y = d.Reader.ReadInt32();
			dg.EnsureClassDef(typeof(global::Lime.IntVector2));
			result.B.X = d.Reader.ReadInt32();
			result.B.Y = d.Reader.ReadInt32();
			return result;
		}

		private static object Make_Lime__IntVector2(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.IntVector2();
			result.X = d.Reader.ReadInt32();
			result.Y = d.Reader.ReadInt32();
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

		private static void Read_Lime__Keyframe_Alignment(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.Alignment>)obj;
			var dg = (BinaryDeserializerGen)d;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			dg.EnsureClassDef(typeof(global::Lime.Alignment));
			var tmp1 = new global::Lime.Alignment();
			tmp1.X = (global::Lime.HAlignment)d.Reader.ReadInt32();
			tmp1.Y = (global::Lime.VAlignment)d.Reader.ReadInt32();
			result.Value = tmp1;
		}

		private static object Make_Lime__Keyframe_Alignment(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.Alignment>();
			Read_Lime__Keyframe_Alignment(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_Anchors(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.Anchors>)obj;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = (global::Lime.Anchors)d.Reader.ReadInt32();
		}

		private static object Make_Lime__Keyframe_Anchors(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.Anchors>();
			Read_Lime__Keyframe_Anchors(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_AudioAction(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.AudioAction>)obj;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = (global::Lime.AudioAction)d.Reader.ReadInt32();
		}

		private static object Make_Lime__Keyframe_AudioAction(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.AudioAction>();
			Read_Lime__Keyframe_AudioAction(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_Blending(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.Blending>)obj;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = (global::Lime.Blending)d.Reader.ReadInt32();
		}

		private static object Make_Lime__Keyframe_Blending(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.Blending>();
			Read_Lime__Keyframe_Blending(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_ClipMethod(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.ClipMethod>)obj;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = (global::Lime.ClipMethod)d.Reader.ReadInt32();
		}

		private static object Make_Lime__Keyframe_ClipMethod(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.ClipMethod>();
			Read_Lime__Keyframe_ClipMethod(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_Color4(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.Color4>)obj;
			var dg = (BinaryDeserializerGen)d;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			dg.EnsureClassDef(typeof(global::Lime.Color4));
			var tmp1 = new global::Lime.Color4();
			tmp1.ABGR = d.Reader.ReadUInt32();
			result.Value = tmp1;
		}

		private static object Make_Lime__Keyframe_Color4(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.Color4>();
			Read_Lime__Keyframe_Color4(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_EmissionType(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.EmissionType>)obj;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = (global::Lime.EmissionType)d.Reader.ReadInt32();
		}

		private static object Make_Lime__Keyframe_EmissionType(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.EmissionType>();
			Read_Lime__Keyframe_EmissionType(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_EmitterShape(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.EmitterShape>)obj;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = (global::Lime.EmitterShape)d.Reader.ReadInt32();
		}

		private static object Make_Lime__Keyframe_EmitterShape(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.EmitterShape>();
			Read_Lime__Keyframe_EmitterShape(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_HAlignment(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.HAlignment>)obj;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = (global::Lime.HAlignment)d.Reader.ReadInt32();
		}

		private static object Make_Lime__Keyframe_HAlignment(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.HAlignment>();
			Read_Lime__Keyframe_HAlignment(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_ITexture(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.ITexture>)obj;
			var dg = (BinaryDeserializerGen)d;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = (global::Lime.ITexture)dg.ReadObject<global::Lime.ITexture>();
		}

		private static object Make_Lime__Keyframe_ITexture(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.ITexture>();
			Read_Lime__Keyframe_ITexture(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_LayoutDirection(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.LayoutDirection>)obj;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = (global::Lime.LayoutDirection)d.Reader.ReadInt32();
		}

		private static object Make_Lime__Keyframe_LayoutDirection(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.LayoutDirection>();
			Read_Lime__Keyframe_LayoutDirection(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_Matrix44(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.Matrix44>)obj;
			var dg = (BinaryDeserializerGen)d;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			dg.EnsureClassDef(typeof(global::Lime.Matrix44));
			var tmp1 = new global::Lime.Matrix44();
			tmp1.M11 = d.Reader.ReadSingle();
			tmp1.M12 = d.Reader.ReadSingle();
			tmp1.M13 = d.Reader.ReadSingle();
			tmp1.M14 = d.Reader.ReadSingle();
			tmp1.M21 = d.Reader.ReadSingle();
			tmp1.M22 = d.Reader.ReadSingle();
			tmp1.M23 = d.Reader.ReadSingle();
			tmp1.M24 = d.Reader.ReadSingle();
			tmp1.M31 = d.Reader.ReadSingle();
			tmp1.M32 = d.Reader.ReadSingle();
			tmp1.M33 = d.Reader.ReadSingle();
			tmp1.M34 = d.Reader.ReadSingle();
			tmp1.M41 = d.Reader.ReadSingle();
			tmp1.M42 = d.Reader.ReadSingle();
			tmp1.M43 = d.Reader.ReadSingle();
			tmp1.M44 = d.Reader.ReadSingle();
			result.Value = tmp1;
		}

		private static object Make_Lime__Keyframe_Matrix44(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.Matrix44>();
			Read_Lime__Keyframe_Matrix44(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_MovieAction(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.MovieAction>)obj;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = (global::Lime.MovieAction)d.Reader.ReadInt32();
		}

		private static object Make_Lime__Keyframe_MovieAction(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.MovieAction>();
			Read_Lime__Keyframe_MovieAction(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_NodeReference_Camera3D(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Camera3D>>)obj;
			var dg = (BinaryDeserializerGen)d;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = (global::Lime.NodeReference<global::Lime.Camera3D>)dg.ReadObject<global::Lime.NodeReference<global::Lime.Camera3D>>();
		}

		private static object Make_Lime__Keyframe_NodeReference_Camera3D(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Camera3D>>();
			Read_Lime__Keyframe_NodeReference_Camera3D(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_NodeReference_Node3D(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Node3D>>)obj;
			var dg = (BinaryDeserializerGen)d;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = (global::Lime.NodeReference<global::Lime.Node3D>)dg.ReadObject<global::Lime.NodeReference<global::Lime.Node3D>>();
		}

		private static object Make_Lime__Keyframe_NodeReference_Node3D(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Node3D>>();
			Read_Lime__Keyframe_NodeReference_Node3D(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_NodeReference_Spline(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline>>)obj;
			var dg = (BinaryDeserializerGen)d;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = (global::Lime.NodeReference<global::Lime.Spline>)dg.ReadObject<global::Lime.NodeReference<global::Lime.Spline>>();
		}

		private static object Make_Lime__Keyframe_NodeReference_Spline(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline>>();
			Read_Lime__Keyframe_NodeReference_Spline(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_NodeReference_Spline3D(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline3D>>)obj;
			var dg = (BinaryDeserializerGen)d;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = (global::Lime.NodeReference<global::Lime.Spline3D>)dg.ReadObject<global::Lime.NodeReference<global::Lime.Spline3D>>();
		}

		private static object Make_Lime__Keyframe_NodeReference_Spline3D(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline3D>>();
			Read_Lime__Keyframe_NodeReference_Spline3D(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_NodeReference_Widget(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Widget>>)obj;
			var dg = (BinaryDeserializerGen)d;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = (global::Lime.NodeReference<global::Lime.Widget>)dg.ReadObject<global::Lime.NodeReference<global::Lime.Widget>>();
		}

		private static object Make_Lime__Keyframe_NodeReference_Widget(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Widget>>();
			Read_Lime__Keyframe_NodeReference_Widget(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_NumericRange(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.NumericRange>)obj;
			var dg = (BinaryDeserializerGen)d;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
		}

		private static object Make_Lime__Keyframe_NumericRange(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.NumericRange>();
			Read_Lime__Keyframe_NumericRange(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_ParticlesLinkage(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.ParticlesLinkage>)obj;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = (global::Lime.ParticlesLinkage)d.Reader.ReadInt32();
		}

		private static object Make_Lime__Keyframe_ParticlesLinkage(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.ParticlesLinkage>();
			Read_Lime__Keyframe_ParticlesLinkage(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_Quaternion(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.Quaternion>)obj;
			var dg = (BinaryDeserializerGen)d;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			dg.EnsureClassDef(typeof(global::Lime.Quaternion));
			var tmp1 = new global::Lime.Quaternion();
			tmp1.X = d.Reader.ReadSingle();
			tmp1.Y = d.Reader.ReadSingle();
			tmp1.Z = d.Reader.ReadSingle();
			tmp1.W = d.Reader.ReadSingle();
			result.Value = tmp1;
		}

		private static object Make_Lime__Keyframe_Quaternion(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.Quaternion>();
			Read_Lime__Keyframe_Quaternion(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_RenderTarget(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.RenderTarget>)obj;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = (global::Lime.RenderTarget)d.Reader.ReadInt32();
		}

		private static object Make_Lime__Keyframe_RenderTarget(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.RenderTarget>();
			Read_Lime__Keyframe_RenderTarget(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_SerializableFont(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.SerializableFont>)obj;
			var dg = (BinaryDeserializerGen)d;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = (global::Lime.SerializableFont)dg.ReadObject<global::Lime.SerializableFont>();
		}

		private static object Make_Lime__Keyframe_SerializableFont(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.SerializableFont>();
			Read_Lime__Keyframe_SerializableFont(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_SerializableSample(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.SerializableSample>)obj;
			var dg = (BinaryDeserializerGen)d;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = (global::Lime.SerializableSample)dg.ReadObject<global::Lime.SerializableSample>();
		}

		private static object Make_Lime__Keyframe_SerializableSample(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.SerializableSample>();
			Read_Lime__Keyframe_SerializableSample(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_ShaderId(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.ShaderId>)obj;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = (global::Lime.ShaderId)d.Reader.ReadInt32();
		}

		private static object Make_Lime__Keyframe_ShaderId(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.ShaderId>();
			Read_Lime__Keyframe_ShaderId(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_TextOverflowMode(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.TextOverflowMode>)obj;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = (global::Lime.TextOverflowMode)d.Reader.ReadInt32();
		}

		private static object Make_Lime__Keyframe_TextOverflowMode(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.TextOverflowMode>();
			Read_Lime__Keyframe_TextOverflowMode(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_Thickness(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.Thickness>)obj;
			var dg = (BinaryDeserializerGen)d;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			dg.EnsureClassDef(typeof(global::Lime.Thickness));
			var tmp1 = new global::Lime.Thickness();
			tmp1.Left = d.Reader.ReadSingle();
			tmp1.Right = d.Reader.ReadSingle();
			tmp1.Top = d.Reader.ReadSingle();
			tmp1.Bottom = d.Reader.ReadSingle();
			result.Value = tmp1;
		}

		private static object Make_Lime__Keyframe_Thickness(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.Thickness>();
			Read_Lime__Keyframe_Thickness(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_VAlignment(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.VAlignment>)obj;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = (global::Lime.VAlignment)d.Reader.ReadInt32();
		}

		private static object Make_Lime__Keyframe_VAlignment(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.VAlignment>();
			Read_Lime__Keyframe_VAlignment(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_Vector2(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.Vector2>)obj;
			var dg = (BinaryDeserializerGen)d;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			dg.EnsureClassDef(typeof(global::Lime.Vector2));
			var tmp1 = new global::Lime.Vector2();
			tmp1.X = d.Reader.ReadSingle();
			tmp1.Y = d.Reader.ReadSingle();
			result.Value = tmp1;
		}

		private static object Make_Lime__Keyframe_Vector2(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.Vector2>();
			Read_Lime__Keyframe_Vector2(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_Vector3(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::Lime.Vector3>)obj;
			var dg = (BinaryDeserializerGen)d;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			dg.EnsureClassDef(typeof(global::Lime.Vector3));
			var tmp1 = new global::Lime.Vector3();
			tmp1.X = d.Reader.ReadSingle();
			tmp1.Y = d.Reader.ReadSingle();
			tmp1.Z = d.Reader.ReadSingle();
			result.Value = tmp1;
		}

		private static object Make_Lime__Keyframe_Vector3(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::Lime.Vector3>();
			Read_Lime__Keyframe_Vector3(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_Boolean(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::System.Boolean>)obj;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = d.Reader.ReadBoolean();
		}

		private static object Make_Lime__Keyframe_Boolean(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::System.Boolean>();
			Read_Lime__Keyframe_Boolean(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_Int32(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::System.Int32>)obj;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = d.Reader.ReadInt32();
		}

		private static object Make_Lime__Keyframe_Int32(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::System.Int32>();
			Read_Lime__Keyframe_Int32(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_Single(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::System.Single>)obj;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = d.Reader.ReadSingle();
		}

		private static object Make_Lime__Keyframe_Single(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::System.Single>();
			Read_Lime__Keyframe_Single(d, def, result);
			return result;
		}

		private static void Read_Lime__Keyframe_String(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Keyframe<global::System.String>)obj;
			result.Frame = d.Reader.ReadInt32();
			result.Function = (global::Lime.KeyFunction)d.Reader.ReadInt32();
			result.Value = d.Reader.ReadString();
			if (result.Value == "" && d.Reader.ReadBoolean()) result.Value = null;
		}

		private static object Make_Lime__Keyframe_String(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Keyframe<global::System.String>();
			Read_Lime__Keyframe_String(d, def, result);
			return result;
		}

		private static void Read_Lime__LayoutCell(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.LayoutCell)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Alignment));
				var tmp1 = new global::Lime.Alignment();
				tmp1.X = (global::Lime.HAlignment)d.Reader.ReadInt32();
				tmp1.Y = (global::Lime.VAlignment)d.Reader.ReadInt32();
				result.Alignment = tmp1;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ColumnSpan = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Ignore = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.RowSpan = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp2 = new global::Lime.Vector2();
				tmp2.X = d.Reader.ReadSingle();
				tmp2.Y = d.Reader.ReadSingle();
				result.Stretch = tmp2;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__LayoutCell(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.LayoutCell();
			Read_Lime__LayoutCell(d, def, result);
			return result;
		}

		private static void Read_Lime__LayoutConstraints(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.LayoutConstraints)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp1 = new global::Lime.Vector2();
				tmp1.X = d.Reader.ReadSingle();
				tmp1.Y = d.Reader.ReadSingle();
				result.MaxSize = tmp1;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp2 = new global::Lime.Vector2();
				tmp2.X = d.Reader.ReadSingle();
				tmp2.Y = d.Reader.ReadSingle();
				result.MinSize = tmp2;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__LayoutConstraints(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.LayoutConstraints();
			Read_Lime__LayoutConstraints(d, def, result);
			return result;
		}

		private static void Read_Lime__LinearLayout(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.LinearLayout)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.DefaultCell = (global::Lime.DefaultLayoutCell)dg.ReadObject<global::Lime.DefaultLayoutCell>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Direction = (global::Lime.LayoutDirection)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.IgnoreHidden = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Spacing = d.Reader.ReadSingle();
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

		private static void Read_Lime__MarkerBlending(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.MarkerBlending)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Option = (global::Lime.BlendingOption)dg.ReadObject<global::Lime.BlendingOption>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.SourceMarkersOptions = (global::System.Collections.Generic.Dictionary<global::System.String,global::Lime.BlendingOption>)null;
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.SourceMarkersOptions = new global::System.Collections.Generic.Dictionary<global::System.String,global::Lime.BlendingOption>();
					while (--tmp1 >= 0) {
						var tmp2 = d.Reader.ReadString();
						if (tmp2 == "" && d.Reader.ReadBoolean()) tmp2 = null;
						var tmp3 = (global::Lime.BlendingOption)dg.ReadObject<global::Lime.BlendingOption>();
						result.SourceMarkersOptions.Add(tmp2, tmp3);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__MarkerBlending(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.MarkerBlending();
			Read_Lime__MarkerBlending(d, def, result);
			return result;
		}

		private static object Make_Lime__Matrix32(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Matrix32();
			var dg = (BinaryDeserializerGen)d;
			dg.EnsureClassDef(typeof(global::Lime.Vector2));
			result.U.X = d.Reader.ReadSingle();
			result.U.Y = d.Reader.ReadSingle();
			dg.EnsureClassDef(typeof(global::Lime.Vector2));
			result.V.X = d.Reader.ReadSingle();
			result.V.Y = d.Reader.ReadSingle();
			dg.EnsureClassDef(typeof(global::Lime.Vector2));
			result.T.X = d.Reader.ReadSingle();
			result.T.Y = d.Reader.ReadSingle();
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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

		private static void Read_Lime__Mesh_Mesh3D__Vertex(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Mesh<global::Lime.Mesh3D.Vertex>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AttributeLocations = (global::System.Int32[])null;
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					var tmp2 = new global::System.Int32[tmp1];
					for(tmp1 = 0; tmp1 < tmp2.Length; ++tmp1) {
						tmp2[tmp1] = d.Reader.ReadInt32();
					}
					result.AttributeLocations = tmp2;
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Indices = (global::System.UInt16[])null;
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					var tmp4 = new global::System.UInt16[tmp3];
					for(tmp3 = 0; tmp3 < tmp4.Length; ++tmp3) {
						tmp4[tmp3] = d.Reader.ReadUInt16();
					}
					result.Indices = tmp4;
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Topology = (global::Lime.PrimitiveTopology)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Vertices = (global::Lime.Mesh3D.Vertex[])null;
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					var tmp6 = new global::Lime.Mesh3D.Vertex[tmp5];
					for(tmp5 = 0; tmp5 < tmp6.Length; ++tmp5) {
						tmp6[tmp5] = (global::Lime.Mesh3D.Vertex)dg.ReadStruct<global::Lime.Mesh3D.Vertex>();
					}
					result.Vertices = tmp6;
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Mesh_Mesh3D__Vertex(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Mesh<global::Lime.Mesh3D.Vertex>();
			Read_Lime__Mesh_Mesh3D__Vertex(d, def, result);
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
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				var tmp5 = new global::Lime.Vector3();
				tmp5.X = d.Reader.ReadSingle();
				tmp5.Y = d.Reader.ReadSingle();
				tmp5.Z = d.Reader.ReadSingle();
				result.Center = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp6 = new global::Lime.Color4();
				tmp6.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp6;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				var tmp7 = d.Reader.ReadInt32();
				if (tmp7 >= 0) {
					while (--tmp7 >= 0) {
						var tmp8 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp8);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.CullMode = (global::Lime.CullMode)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp9 = d.Reader.ReadInt32();
				if (tmp9 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp9 >= 0) {
						var tmp10 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp10);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				var tmp11 = d.Reader.ReadInt32();
				if (tmp11 >= 0) {
					while (--tmp11 >= 0) {
						var tmp12 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp12);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.Opaque = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				var tmp13 = new global::Lime.Vector3();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				tmp13.Z = d.Reader.ReadSingle();
				result.Position = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Quaternion));
				var tmp14 = new global::Lime.Quaternion();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				tmp14.Z = d.Reader.ReadSingle();
				tmp14.W = d.Reader.ReadSingle();
				result.Rotation = tmp14;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				var tmp15 = new global::Lime.Vector3();
				tmp15.X = d.Reader.ReadSingle();
				tmp15.Y = d.Reader.ReadSingle();
				tmp15.Z = d.Reader.ReadSingle();
				result.Scale = tmp15;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				var tmp16 = d.Reader.ReadInt32();
				if (tmp16 >= 0) {
					while (--tmp16 >= 0) {
						var tmp17 = (global::Lime.Submesh3D)dg.ReadObject<global::Lime.Submesh3D>();
						result.Submeshes.Add(tmp17);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
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

		private static object Make_Lime__Mesh3D__BlendIndices(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Mesh3D.BlendIndices();
			result.Index0 = d.Reader.ReadByte();
			result.Index1 = d.Reader.ReadByte();
			result.Index2 = d.Reader.ReadByte();
			result.Index3 = d.Reader.ReadByte();
			return result;
		}

		private static object Make_Lime__Mesh3D__BlendWeights(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Mesh3D.BlendWeights();
			result.Weight0 = d.Reader.ReadSingle();
			result.Weight1 = d.Reader.ReadSingle();
			result.Weight2 = d.Reader.ReadSingle();
			result.Weight3 = d.Reader.ReadSingle();
			return result;
		}

		private static object Make_Lime__Mesh3D__Vertex(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Mesh3D.Vertex();
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				result.Pos.X = d.Reader.ReadSingle();
				result.Pos.Y = d.Reader.ReadSingle();
				result.Pos.Z = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				result.Color.ABGR = d.Reader.ReadUInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				result.UV1.X = d.Reader.ReadSingle();
				result.UV1.Y = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Mesh3D.BlendIndices));
				result.BlendIndices.Index0 = d.Reader.ReadByte();
				result.BlendIndices.Index1 = d.Reader.ReadByte();
				result.BlendIndices.Index2 = d.Reader.ReadByte();
				result.BlendIndices.Index3 = d.Reader.ReadByte();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Mesh3D.BlendWeights));
				result.BlendWeights.Weight0 = d.Reader.ReadSingle();
				result.BlendWeights.Weight1 = d.Reader.ReadSingle();
				result.BlendWeights.Weight2 = d.Reader.ReadSingle();
				result.BlendWeights.Weight3 = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				result.Normal.X = d.Reader.ReadSingle();
				result.Normal.Y = d.Reader.ReadSingle();
				result.Normal.Z = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
			return result;
		}

		private static void Read_Lime__Model3D(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Model3D)obj;
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
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Opaque = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				var tmp12 = new global::Lime.Vector3();
				tmp12.X = d.Reader.ReadSingle();
				tmp12.Y = d.Reader.ReadSingle();
				tmp12.Z = d.Reader.ReadSingle();
				result.Position = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Quaternion));
				var tmp13 = new global::Lime.Quaternion();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				tmp13.Z = d.Reader.ReadSingle();
				tmp13.W = d.Reader.ReadSingle();
				result.Rotation = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				var tmp14 = new global::Lime.Vector3();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				tmp14.Z = d.Reader.ReadSingle();
				result.Scale = tmp14;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
			result.AfterDeserialization();
		}

		private static object Make_Lime__Model3D(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Model3D();
			Read_Lime__Model3D(d, def, result);
			return result;
		}

		private static void Read_Lime__Model3DAttachment__MaterialRemap(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Model3DAttachment.MaterialRemap)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Material = (global::Lime.IMaterial)dg.ReadObject<global::Lime.IMaterial>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.SourceName = d.Reader.ReadString();
				if (result.SourceName == "" && d.Reader.ReadBoolean()) result.SourceName = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Model3DAttachment__MaterialRemap(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Model3DAttachment.MaterialRemap();
			Read_Lime__Model3DAttachment__MaterialRemap(d, def, result);
			return result;
		}

		private static void Read_Lime__Model3DAttachmentParser__MeshOptionFormat(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Model3DAttachmentParser.MeshOptionFormat)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.CullMode = d.Reader.ReadString();
				if (result.CullMode == "" && d.Reader.ReadBoolean()) result.CullMode = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.HitTestTarget = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Opaque = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Model3DAttachmentParser__MeshOptionFormat(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Model3DAttachmentParser.MeshOptionFormat();
			Read_Lime__Model3DAttachmentParser__MeshOptionFormat(d, def, result);
			return result;
		}

		private static void Read_Lime__Model3DAttachmentParser__ModelAnimationFormat(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Model3DAttachmentParser.ModelAnimationFormat)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Blending = d.Reader.ReadBoolean() ? (global::System.Int32?)null : d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.IgnoredNodes = (global::System.Collections.Generic.List<global::System.String>)null;
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.IgnoredNodes = new global::System.Collections.Generic.List<global::System.String>();
					while (--tmp1 >= 0) {
						var tmp2 = d.Reader.ReadString();
						if (tmp2 == "" && d.Reader.ReadBoolean()) tmp2 = null;
						result.IgnoredNodes.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.LastFrame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Markers = (global::System.Collections.Generic.Dictionary<global::System.String,global::Lime.Model3DAttachmentParser.ModelMarkerFormat>)null;
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					result.Markers = new global::System.Collections.Generic.Dictionary<global::System.String,global::Lime.Model3DAttachmentParser.ModelMarkerFormat>();
					while (--tmp3 >= 0) {
						var tmp4 = d.Reader.ReadString();
						if (tmp4 == "" && d.Reader.ReadBoolean()) tmp4 = null;
						var tmp5 = (global::Lime.Model3DAttachmentParser.ModelMarkerFormat)dg.ReadObject<global::Lime.Model3DAttachmentParser.ModelMarkerFormat>();
						result.Markers.Add(tmp4, tmp5);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.Nodes = (global::System.Collections.Generic.List<global::System.String>)null;
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					result.Nodes = new global::System.Collections.Generic.List<global::System.String>();
					while (--tmp6 >= 0) {
						var tmp7 = d.Reader.ReadString();
						if (tmp7 == "" && d.Reader.ReadBoolean()) tmp7 = null;
						result.Nodes.Add(tmp7);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.SourceAnimationId = d.Reader.ReadString();
				if (result.SourceAnimationId == "" && d.Reader.ReadBoolean()) result.SourceAnimationId = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.StartFrame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Model3DAttachmentParser__ModelAnimationFormat(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Model3DAttachmentParser.ModelAnimationFormat();
			Read_Lime__Model3DAttachmentParser__ModelAnimationFormat(d, def, result);
			return result;
		}

		private static void Read_Lime__Model3DAttachmentParser__ModelAttachmentFormat(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Model3DAttachmentParser.ModelAttachmentFormat)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Animations = (global::System.Collections.Generic.Dictionary<global::System.String,global::Lime.Model3DAttachmentParser.ModelAnimationFormat>)null;
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.Animations = new global::System.Collections.Generic.Dictionary<global::System.String,global::Lime.Model3DAttachmentParser.ModelAnimationFormat>();
					while (--tmp1 >= 0) {
						var tmp2 = d.Reader.ReadString();
						if (tmp2 == "" && d.Reader.ReadBoolean()) tmp2 = null;
						var tmp3 = (global::Lime.Model3DAttachmentParser.ModelAnimationFormat)dg.ReadObject<global::Lime.Model3DAttachmentParser.ModelAnimationFormat>();
						result.Animations.Add(tmp2, tmp3);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.EntryTrigger = d.Reader.ReadString();
				if (result.EntryTrigger == "" && d.Reader.ReadBoolean()) result.EntryTrigger = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Materials = (global::System.Collections.Generic.List<global::Lime.Model3DAttachment.MaterialRemap>)null;
				var tmp4 = d.Reader.ReadInt32();
				if (tmp4 >= 0) {
					result.Materials = new global::System.Collections.Generic.List<global::Lime.Model3DAttachment.MaterialRemap>();
					while (--tmp4 >= 0) {
						var tmp5 = (global::Lime.Model3DAttachment.MaterialRemap)dg.ReadObject<global::Lime.Model3DAttachment.MaterialRemap>();
						result.Materials.Add(tmp5);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.MeshOptions = (global::System.Collections.Generic.Dictionary<global::System.String,global::Lime.Model3DAttachmentParser.MeshOptionFormat>)null;
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					result.MeshOptions = new global::System.Collections.Generic.Dictionary<global::System.String,global::Lime.Model3DAttachmentParser.MeshOptionFormat>();
					while (--tmp6 >= 0) {
						var tmp7 = d.Reader.ReadString();
						if (tmp7 == "" && d.Reader.ReadBoolean()) tmp7 = null;
						var tmp8 = (global::Lime.Model3DAttachmentParser.MeshOptionFormat)dg.ReadObject<global::Lime.Model3DAttachmentParser.MeshOptionFormat>();
						result.MeshOptions.Add(tmp7, tmp8);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.NodeComponents = (global::System.Collections.Generic.Dictionary<global::System.String,global::Lime.Model3DAttachmentParser.ModelComponentsFormat>)null;
				var tmp9 = d.Reader.ReadInt32();
				if (tmp9 >= 0) {
					result.NodeComponents = new global::System.Collections.Generic.Dictionary<global::System.String,global::Lime.Model3DAttachmentParser.ModelComponentsFormat>();
					while (--tmp9 >= 0) {
						var tmp10 = d.Reader.ReadString();
						if (tmp10 == "" && d.Reader.ReadBoolean()) tmp10 = null;
						var tmp11 = (global::Lime.Model3DAttachmentParser.ModelComponentsFormat)dg.ReadObject<global::Lime.Model3DAttachmentParser.ModelComponentsFormat>();
						result.NodeComponents.Add(tmp10, tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.NodeRemovals = (global::System.Collections.Generic.List<global::System.String>)null;
				var tmp12 = d.Reader.ReadInt32();
				if (tmp12 >= 0) {
					result.NodeRemovals = new global::System.Collections.Generic.List<global::System.String>();
					while (--tmp12 >= 0) {
						var tmp13 = d.Reader.ReadString();
						if (tmp13 == "" && d.Reader.ReadBoolean()) tmp13 = null;
						result.NodeRemovals.Add(tmp13);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ScaleFactor = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.SourceAnimationIds = (global::System.Collections.Generic.List<global::System.String>)null;
				var tmp14 = d.Reader.ReadInt32();
				if (tmp14 >= 0) {
					result.SourceAnimationIds = new global::System.Collections.Generic.List<global::System.String>();
					while (--tmp14 >= 0) {
						var tmp15 = d.Reader.ReadString();
						if (tmp15 == "" && d.Reader.ReadBoolean()) tmp15 = null;
						result.SourceAnimationIds.Add(tmp15);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.UVAnimations = (global::System.Collections.Generic.List<global::Lime.Model3DAttachmentParser.UVAnimationFormat>)null;
				var tmp16 = d.Reader.ReadInt32();
				if (tmp16 >= 0) {
					result.UVAnimations = new global::System.Collections.Generic.List<global::Lime.Model3DAttachmentParser.UVAnimationFormat>();
					while (--tmp16 >= 0) {
						var tmp17 = (global::Lime.Model3DAttachmentParser.UVAnimationFormat)dg.ReadObject<global::Lime.Model3DAttachmentParser.UVAnimationFormat>();
						result.UVAnimations.Add(tmp17);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Model3DAttachmentParser__ModelAttachmentFormat(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Model3DAttachmentParser.ModelAttachmentFormat();
			Read_Lime__Model3DAttachmentParser__ModelAttachmentFormat(d, def, result);
			return result;
		}

		private static void Read_Lime__Model3DAttachmentParser__ModelComponentsFormat(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Model3DAttachmentParser.ModelComponentsFormat)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Components = (global::System.Collections.Generic.List<global::Lime.NodeComponent>)null;
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.Components = new global::System.Collections.Generic.List<global::Lime.NodeComponent>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Node = d.Reader.ReadString();
				if (result.Node == "" && d.Reader.ReadBoolean()) result.Node = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Model3DAttachmentParser__ModelComponentsFormat(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Model3DAttachmentParser.ModelComponentsFormat();
			Read_Lime__Model3DAttachmentParser__ModelComponentsFormat(d, def, result);
			return result;
		}

		private static void Read_Lime__Model3DAttachmentParser__ModelMarkerFormat(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Model3DAttachmentParser.ModelMarkerFormat)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Action = d.Reader.ReadString();
				if (result.Action == "" && d.Reader.ReadBoolean()) result.Action = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Blending = d.Reader.ReadBoolean() ? (global::System.Int32?)null : d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Frame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.JumpTarget = d.Reader.ReadString();
				if (result.JumpTarget == "" && d.Reader.ReadBoolean()) result.JumpTarget = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.SourceMarkersBlending = (global::System.Collections.Generic.Dictionary<global::System.String,global::System.Int32>)null;
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.SourceMarkersBlending = new global::System.Collections.Generic.Dictionary<global::System.String,global::System.Int32>();
					while (--tmp1 >= 0) {
						var tmp2 = d.Reader.ReadString();
						if (tmp2 == "" && d.Reader.ReadBoolean()) tmp2 = null;
						var tmp3 = d.Reader.ReadInt32();
						result.SourceMarkersBlending.Add(tmp2, tmp3);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Model3DAttachmentParser__ModelMarkerFormat(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Model3DAttachmentParser.ModelMarkerFormat();
			Read_Lime__Model3DAttachmentParser__ModelMarkerFormat(d, def, result);
			return result;
		}

		private static void Read_Lime__Model3DAttachmentParser__UVAnimationFormat(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Model3DAttachmentParser.UVAnimationFormat)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AnimateOverlay = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.AnimationSpeed = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.AnimationType = (global::Lime.Model3DAttachmentParser.UVAnimationType)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.BlendingMode = (global::Lime.Model3DAttachmentParser.UVAnimationOverlayBlending)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.DiffuseTexture = d.Reader.ReadString();
				if (result.DiffuseTexture == "" && d.Reader.ReadBoolean()) result.DiffuseTexture = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.MaskTexture = d.Reader.ReadString();
				if (result.MaskTexture == "" && d.Reader.ReadBoolean()) result.MaskTexture = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.MeshName = d.Reader.ReadString();
				if (result.MeshName == "" && d.Reader.ReadBoolean()) result.MeshName = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.OverlayTexture = d.Reader.ReadString();
				if (result.OverlayTexture == "" && d.Reader.ReadBoolean()) result.OverlayTexture = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.TileX = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.TileY = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Model3DAttachmentParser__UVAnimationFormat(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Model3DAttachmentParser.UVAnimationFormat();
			Read_Lime__Model3DAttachmentParser__UVAnimationFormat(d, def, result);
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
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
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
				result.Looped = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Thickness));
				var tmp12 = new global::Lime.Thickness();
				tmp12.Left = d.Reader.ReadSingle();
				tmp12.Right = d.Reader.ReadSingle();
				tmp12.Top = d.Reader.ReadSingle();
				tmp12.Bottom = d.Reader.ReadSingle();
				result.Padding = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Path = d.Reader.ReadString();
				if (result.Path == "" && d.Reader.ReadBoolean()) result.Path = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp13 = new global::Lime.Vector2();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				result.Pivot = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp14 = new global::Lime.Vector2();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				result.Position = tmp14;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp15 = new global::Lime.Vector2();
				tmp15.X = d.Reader.ReadSingle();
				tmp15.Y = d.Reader.ReadSingle();
				result.Scale = tmp15;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp16 = new global::Lime.Vector2();
				tmp16.X = d.Reader.ReadSingle();
				tmp16.Y = d.Reader.ReadSingle();
				result.SilentSize = tmp16;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
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
				result.BottomOffset = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
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
				result.LeftOffset = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Thickness));
				var tmp12 = new global::Lime.Thickness();
				tmp12.Left = d.Reader.ReadSingle();
				tmp12.Right = d.Reader.ReadSingle();
				tmp12.Top = d.Reader.ReadSingle();
				tmp12.Bottom = d.Reader.ReadSingle();
				result.Padding = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp13 = new global::Lime.Vector2();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				result.Pivot = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp14 = new global::Lime.Vector2();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				result.Position = tmp14;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.RightOffset = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp15 = new global::Lime.Vector2();
				tmp15.X = d.Reader.ReadSingle();
				tmp15.Y = d.Reader.ReadSingle();
				result.Scale = tmp15;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp16 = new global::Lime.Vector2();
				tmp16.X = d.Reader.ReadSingle();
				tmp16.Y = d.Reader.ReadSingle();
				result.SilentSize = tmp16;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (25 == fd.OurIndex) {
				result.Texture = (global::Lime.ITexture)dg.ReadObject<global::Lime.ITexture>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (26 == fd.OurIndex) {
				result.TopOffset = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (27 == fd.OurIndex) {
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
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Opaque = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				var tmp12 = new global::Lime.Vector3();
				tmp12.X = d.Reader.ReadSingle();
				tmp12.Y = d.Reader.ReadSingle();
				tmp12.Z = d.Reader.ReadSingle();
				result.Position = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Quaternion));
				var tmp13 = new global::Lime.Quaternion();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				tmp13.Z = d.Reader.ReadSingle();
				tmp13.W = d.Reader.ReadSingle();
				result.Rotation = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				var tmp14 = new global::Lime.Vector3();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				tmp14.Z = d.Reader.ReadSingle();
				result.Scale = tmp14;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
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

		private static void Read_Lime__NodeReference_Camera3D(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.NodeReference<global::Lime.Camera3D>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__NodeReference_Camera3D(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.NodeReference<global::Lime.Camera3D>();
			Read_Lime__NodeReference_Camera3D(d, def, result);
			return result;
		}

		private static void Read_Lime__NodeReference_Spline(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.NodeReference<global::Lime.Spline>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__NodeReference_Spline(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.NodeReference<global::Lime.Spline>();
			Read_Lime__NodeReference_Spline(d, def, result);
			return result;
		}

		private static void Read_Lime__NodeReference_Widget(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.NodeReference<global::Lime.Widget>)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__NodeReference_Widget(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.NodeReference<global::Lime.Widget>();
			Read_Lime__NodeReference_Widget(d, def, result);
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
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
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.GravityAmount = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.GravityDirection = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.ImmortalParticles = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Lifetime = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.LinkageWidgetName = d.Reader.ReadString();
				if (result.LinkageWidgetName == "" && d.Reader.ReadBoolean()) result.LinkageWidgetName = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.MagnetAmount = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				result.Number = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
				result.Orientation = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (25 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Thickness));
				var tmp12 = new global::Lime.Thickness();
				tmp12.Left = d.Reader.ReadSingle();
				tmp12.Right = d.Reader.ReadSingle();
				tmp12.Top = d.Reader.ReadSingle();
				tmp12.Bottom = d.Reader.ReadSingle();
				result.Padding = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (26 == fd.OurIndex) {
				result.ParticlesLinkage = (global::Lime.ParticlesLinkage)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (27 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp13 = new global::Lime.Vector2();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				result.Pivot = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (28 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp14 = new global::Lime.Vector2();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				result.Position = tmp14;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (29 == fd.OurIndex) {
				result.RandomMotionAspectRatio = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (30 == fd.OurIndex) {
				result.RandomMotionRadius = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (31 == fd.OurIndex) {
				result.RandomMotionRotation = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (32 == fd.OurIndex) {
				result.RandomMotionSpeed = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (33 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (34 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp15 = new global::Lime.Vector2();
				tmp15.X = d.Reader.ReadSingle();
				tmp15.Y = d.Reader.ReadSingle();
				result.Scale = tmp15;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (35 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (36 == fd.OurIndex) {
				result.Shape = (global::Lime.EmitterShape)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (37 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp16 = new global::Lime.Vector2();
				tmp16.X = d.Reader.ReadSingle();
				tmp16.Y = d.Reader.ReadSingle();
				result.SilentSize = tmp16;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (38 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (39 == fd.OurIndex) {
				result.Speed = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (40 == fd.OurIndex) {
				result.Spin = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (41 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (42 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (43 == fd.OurIndex) {
				result.TimeShift = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (44 == fd.OurIndex) {
				result.Velocity = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (45 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (46 == fd.OurIndex) {
				result.WindAmount = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (47 == fd.OurIndex) {
				result.WindDirection = (global::Lime.NumericRange)dg.ReadStruct<global::Lime.NumericRange>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (48 == fd.OurIndex) {
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
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
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
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.GravityAmount = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.LastFrame = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.LoopedAnimation = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.MagnetAmount = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp12 = new global::Lime.Vector2();
				tmp12.X = d.Reader.ReadSingle();
				tmp12.Y = d.Reader.ReadSingle();
				result.Scale = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp13 = new global::Lime.Vector2();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				result.Size = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.Spin = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.Texture = (global::Lime.ITexture)dg.ReadObject<global::Lime.ITexture>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Velocity = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
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
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
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
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Thickness));
				var tmp12 = new global::Lime.Thickness();
				tmp12.Left = d.Reader.ReadSingle();
				tmp12.Right = d.Reader.ReadSingle();
				tmp12.Top = d.Reader.ReadSingle();
				tmp12.Bottom = d.Reader.ReadSingle();
				result.Padding = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp13 = new global::Lime.Vector2();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				result.Pivot = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp14 = new global::Lime.Vector2();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				result.Position = tmp14;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp15 = new global::Lime.Vector2();
				tmp15.X = d.Reader.ReadSingle();
				tmp15.Y = d.Reader.ReadSingle();
				result.Scale = tmp15;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Shape = (global::Lime.EmitterShape)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp16 = new global::Lime.Vector2();
				tmp16.X = d.Reader.ReadSingle();
				tmp16.Y = d.Reader.ReadSingle();
				result.SilentSize = tmp16;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Strength = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (25 == fd.OurIndex) {
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
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				result.Normal.X = d.Reader.ReadSingle();
				result.Normal.Y = d.Reader.ReadSingle();
				result.Normal.Z = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
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
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp7 = d.Reader.ReadInt32();
				if (tmp7 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp7 >= 0) {
						var tmp8 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp8);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				var tmp9 = d.Reader.ReadInt32();
				if (tmp9 >= 0) {
					while (--tmp9 >= 0) {
						var tmp10 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp10);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp11 = new global::Lime.Vector2();
				tmp11.X = d.Reader.ReadSingle();
				tmp11.Y = d.Reader.ReadSingle();
				result.Position = tmp11;
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
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
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

		private static void Read_Lime__Polyline(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.Polyline)obj;
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
				result.Closed = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
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
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Thickness));
				var tmp12 = new global::Lime.Thickness();
				tmp12.Left = d.Reader.ReadSingle();
				tmp12.Right = d.Reader.ReadSingle();
				tmp12.Top = d.Reader.ReadSingle();
				tmp12.Bottom = d.Reader.ReadSingle();
				result.Padding = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp13 = new global::Lime.Vector2();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				result.Pivot = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp14 = new global::Lime.Vector2();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				result.Position = tmp14;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp15 = new global::Lime.Vector2();
				tmp15.X = d.Reader.ReadSingle();
				tmp15.Y = d.Reader.ReadSingle();
				result.Scale = tmp15;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp16 = new global::Lime.Vector2();
				tmp16.X = d.Reader.ReadSingle();
				tmp16.Y = d.Reader.ReadSingle();
				result.SilentSize = tmp16;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.StaticThickness = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
				result.Thickness = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (25 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__Polyline(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Polyline();
			Read_Lime__Polyline(d, def, result);
			return result;
		}

		private static void Read_Lime__PolylinePoint(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.PolylinePoint)obj;
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
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp7 = d.Reader.ReadInt32();
				if (tmp7 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp7 >= 0) {
						var tmp8 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp8);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				var tmp9 = d.Reader.ReadInt32();
				if (tmp9 >= 0) {
					while (--tmp9 >= 0) {
						var tmp10 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp10);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp11 = new global::Lime.Vector2();
				tmp11.X = d.Reader.ReadSingle();
				tmp11.Y = d.Reader.ReadSingle();
				result.Position = tmp11;
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
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__PolylinePoint(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.PolylinePoint();
			Read_Lime__PolylinePoint(d, def, result);
			return result;
		}

		private static void Read_Lime__PostProcessingComponent(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.PostProcessingComponent)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.BloomBrightThreshold = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp1 = new global::Lime.Color4();
				tmp1.ABGR = d.Reader.ReadUInt32();
				result.BloomColor = tmp1;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.BloomEnabled = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				var tmp2 = new global::Lime.Vector3();
				tmp2.X = d.Reader.ReadSingle();
				tmp2.Y = d.Reader.ReadSingle();
				tmp2.Z = d.Reader.ReadSingle();
				result.BloomGammaCorrection = tmp2;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.BloomShaderId = (global::Lime.BlurShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.BloomStrength = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.BloomTextureScaling = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.BlurAlphaCorrection = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp3 = new global::Lime.Color4();
				tmp3.ABGR = d.Reader.ReadUInt32();
				result.BlurBackgroundColor = tmp3;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.BlurEnabled = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.BlurRadius = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.BlurShader = (global::Lime.BlurShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.BlurTextureScaling = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.DistortionBarrelPincushion = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.DistortionBlue = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.DistortionChromaticAberration = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.DistortionEnabled = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.DistortionGreen = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.DistortionRed = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.FXAAEnabled = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.FXAALumaTreshold = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.FXAAMaxSpan = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				result.FXAAMinReduce = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
				result.FXAAMulReduce = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (25 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				var tmp4 = new global::Lime.Vector3();
				tmp4.X = d.Reader.ReadSingle();
				tmp4.Y = d.Reader.ReadSingle();
				tmp4.Z = d.Reader.ReadSingle();
				result.HSL = tmp4;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (26 == fd.OurIndex) {
				result.HSLEnabled = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (27 == fd.OurIndex) {
				result.NoiseBrightThreshold = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (28 == fd.OurIndex) {
				result.NoiseDarkThreshold = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (29 == fd.OurIndex) {
				result.NoiseEnabled = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (30 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp5 = new global::Lime.Vector2();
				tmp5.X = d.Reader.ReadSingle();
				tmp5.Y = d.Reader.ReadSingle();
				result.NoiseOffset = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (31 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp6 = new global::Lime.Vector2();
				tmp6.X = d.Reader.ReadSingle();
				tmp6.Y = d.Reader.ReadSingle();
				result.NoiseScale = tmp6;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (32 == fd.OurIndex) {
				result.NoiseSoftLight = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (33 == fd.OurIndex) {
				result.NoiseTexture = (global::Lime.ITexture)dg.ReadObject<global::Lime.ITexture>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (34 == fd.OurIndex) {
				result.OpagueRendering = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (35 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp7 = new global::Lime.Color4();
				tmp7.ABGR = d.Reader.ReadUInt32();
				result.OverallImpactColor = tmp7;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (36 == fd.OurIndex) {
				result.OverallImpactEnabled = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (37 == fd.OurIndex) {
				result.RefreshSourceRate = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (38 == fd.OurIndex) {
				result.RefreshSourceTexture = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (39 == fd.OurIndex) {
				result.SharpenEnabled = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (40 == fd.OurIndex) {
				result.SharpenLimit = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (41 == fd.OurIndex) {
				result.SharpenStep = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (42 == fd.OurIndex) {
				result.SharpenStrength = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (43 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Size));
				var tmp8 = new global::Lime.Size();
				tmp8.Width = d.Reader.ReadInt32();
				tmp8.Height = d.Reader.ReadInt32();
				result.TextureSizeLimit = tmp8;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (44 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp9 = new global::Lime.Color4();
				tmp9.ABGR = d.Reader.ReadUInt32();
				result.VignetteColor = tmp9;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (45 == fd.OurIndex) {
				result.VignetteEnabled = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (46 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp10 = new global::Lime.Vector2();
				tmp10.X = d.Reader.ReadSingle();
				tmp10.Y = d.Reader.ReadSingle();
				result.VignettePivot = tmp10;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (47 == fd.OurIndex) {
				result.VignetteRadius = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (48 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp11 = new global::Lime.Vector2();
				tmp11.X = d.Reader.ReadSingle();
				tmp11.Y = d.Reader.ReadSingle();
				result.VignetteScale = tmp11;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (49 == fd.OurIndex) {
				result.VignetteSoftness = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__PostProcessingComponent(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.PostProcessingComponent();
			Read_Lime__PostProcessingComponent(d, def, result);
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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

		private static object Make_Lime__Ray(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Ray();
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				result.Direction.X = d.Reader.ReadSingle();
				result.Direction.Y = d.Reader.ReadSingle();
				result.Direction.Z = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				result.Position.X = d.Reader.ReadSingle();
				result.Position.Y = d.Reader.ReadSingle();
				result.Position.Z = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
			return result;
		}

		private static object Make_Lime__Rectangle(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Rectangle();
			result.AX = d.Reader.ReadSingle();
			result.AY = d.Reader.ReadSingle();
			result.BX = d.Reader.ReadSingle();
			result.BY = d.Reader.ReadSingle();
			return result;
		}

		private static void Read_Lime_RenderOptimizer__ContentBox(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.RenderOptimizer.ContentBox)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Bounds));
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				result.Data.A.X = d.Reader.ReadSingle();
				result.Data.A.Y = d.Reader.ReadSingle();
				result.Data.A.Z = d.Reader.ReadSingle();
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				result.Data.B.X = d.Reader.ReadSingle();
				result.Data.B.Y = d.Reader.ReadSingle();
				result.Data.B.Z = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime_RenderOptimizer__ContentBox(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.RenderOptimizer.ContentBox();
			Read_Lime_RenderOptimizer__ContentBox(d, def, result);
			return result;
		}

		private static void Read_Lime_RenderOptimizer__ContentPlane(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.RenderOptimizer.ContentPlane)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Data = (global::Lime.Vector3[])null;
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					var tmp2 = new global::Lime.Vector3[tmp1];
					for(tmp1 = 0; tmp1 < tmp2.Length; ++tmp1) {
						tmp2[tmp1] = (global::Lime.Vector3)dg.ReadStruct<global::Lime.Vector3>();
					}
					result.Data = tmp2;
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime_RenderOptimizer__ContentPlane(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.RenderOptimizer.ContentPlane();
			Read_Lime_RenderOptimizer__ContentPlane(d, def, result);
			return result;
		}

		private static void Read_Lime_RenderOptimizer__ContentRectangle(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.RenderOptimizer.ContentRectangle)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Rectangle));
				result.Data.AX = d.Reader.ReadSingle();
				result.Data.AY = d.Reader.ReadSingle();
				result.Data.BX = d.Reader.ReadSingle();
				result.Data.BY = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime_RenderOptimizer__ContentRectangle(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.RenderOptimizer.ContentRectangle();
			Read_Lime_RenderOptimizer__ContentRectangle(d, def, result);
			return result;
		}

		private static void Read_Lime_RenderOptimizer__ContentSizeComponent(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.RenderOptimizer.ContentSizeComponent)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.Size = (global::Lime.RenderOptimizer.ContentSize)dg.ReadObject<global::Lime.RenderOptimizer.ContentSize>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime_RenderOptimizer__ContentSizeComponent(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.RenderOptimizer.ContentSizeComponent();
			Read_Lime_RenderOptimizer__ContentSizeComponent(d, def, result);
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
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.HAlignment = (global::Lime.HAlignment)d.Reader.ReadInt32();
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
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.OverflowMode = (global::Lime.TextOverflowMode)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Thickness));
				var tmp12 = new global::Lime.Thickness();
				tmp12.Left = d.Reader.ReadSingle();
				tmp12.Right = d.Reader.ReadSingle();
				tmp12.Top = d.Reader.ReadSingle();
				tmp12.Bottom = d.Reader.ReadSingle();
				result.Padding = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp13 = new global::Lime.Vector2();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				result.Pivot = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp14 = new global::Lime.Vector2();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				result.Position = tmp14;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp15 = new global::Lime.Vector2();
				tmp15.X = d.Reader.ReadSingle();
				tmp15.Y = d.Reader.ReadSingle();
				result.Scale = tmp15;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp16 = new global::Lime.Vector2();
				tmp16.X = d.Reader.ReadSingle();
				tmp16.Y = d.Reader.ReadSingle();
				result.SilentSize = tmp16;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
				result.Text = d.Reader.ReadString();
				if (result.Text == "" && d.Reader.ReadBoolean()) result.Text = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (25 == fd.OurIndex) {
				result.VAlignment = (global::Lime.VAlignment)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (26 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (27 == fd.OurIndex) {
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

		private static void Read_Lime__ShadowParams(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.ShadowParams)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp1 = new global::Lime.Color4();
				tmp1.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp1;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Dilate = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.Enabled = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.OffsetX = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.OffsetY = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Softness = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Type = (global::Lime.ShadowParams.ShadowType)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__ShadowParams(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.ShadowParams();
			Read_Lime__ShadowParams(d, def, result);
			return result;
		}

		private static void Read_Lime__SignedDistanceFieldComponent(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.SignedDistanceFieldComponent)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.BevelEnabled = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.BevelRoundness = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.BevelWidth = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Dilate = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.Gradient = (global::Lime.ColorGradient)null;
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.Gradient = new global::Lime.ColorGradient();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.GradientControlPoint)dg.ReadObject<global::Lime.GradientControlPoint>();
						result.Gradient.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.GradientAngle = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.GradientEnabled = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.InnerShadows = (global::System.Collections.Generic.List<global::Lime.BaseShadowParams>)null;
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					result.InnerShadows = new global::System.Collections.Generic.List<global::Lime.BaseShadowParams>();
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.BaseShadowParams)dg.ReadObject<global::Lime.BaseShadowParams>();
						result.InnerShadows.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.LightAngle = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.LightColor = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp6 = new global::Lime.Color4();
				tmp6.ABGR = d.Reader.ReadUInt32();
				result.OutlineColor = tmp6;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.ReflectionPower = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Shadows = (global::System.Collections.Generic.List<global::Lime.ShadowParams>)null;
				var tmp7 = d.Reader.ReadInt32();
				if (tmp7 >= 0) {
					result.Shadows = new global::System.Collections.Generic.List<global::Lime.ShadowParams>();
					while (--tmp7 >= 0) {
						var tmp8 = (global::Lime.ShadowParams)dg.ReadObject<global::Lime.ShadowParams>();
						result.Shadows.Add(tmp8);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Softness = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Thickness = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__SignedDistanceFieldComponent(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.SignedDistanceFieldComponent();
			Read_Lime__SignedDistanceFieldComponent(d, def, result);
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
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Font = (global::Lime.SerializableFont)dg.ReadObject<global::Lime.SerializableFont>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.FontHeight = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.ForceUncutText = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.GradientMapIndex = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.HAlignment = (global::Lime.HAlignment)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.HitTestMethod = (global::Lime.HitTestMethod)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.LetterSpacing = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.OverflowMode = (global::Lime.TextOverflowMode)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Thickness));
				var tmp12 = new global::Lime.Thickness();
				tmp12.Left = d.Reader.ReadSingle();
				tmp12.Right = d.Reader.ReadSingle();
				tmp12.Top = d.Reader.ReadSingle();
				tmp12.Bottom = d.Reader.ReadSingle();
				result.Padding = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp13 = new global::Lime.Vector2();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				result.Pivot = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp14 = new global::Lime.Vector2();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				result.Position = tmp14;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp15 = new global::Lime.Vector2();
				tmp15.X = d.Reader.ReadSingle();
				tmp15.Y = d.Reader.ReadSingle();
				result.Scale = tmp15;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (25 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp16 = new global::Lime.Vector2();
				tmp16.X = d.Reader.ReadSingle();
				tmp16.Y = d.Reader.ReadSingle();
				result.SilentSize = tmp16;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (26 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (27 == fd.OurIndex) {
				result.Spacing = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (28 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (29 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (30 == fd.OurIndex) {
				result.Text = d.Reader.ReadString();
				if (result.Text == "" && d.Reader.ReadBoolean()) result.Text = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (31 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp17 = new global::Lime.Color4();
				tmp17.ABGR = d.Reader.ReadUInt32();
				result.TextColor = tmp17;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (32 == fd.OurIndex) {
				result.VAlignment = (global::Lime.VAlignment)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (33 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (34 == fd.OurIndex) {
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

		private static object Make_Lime__Size(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Size();
			result.Width = d.Reader.ReadInt32();
			result.Height = d.Reader.ReadInt32();
			return result;
		}

		private static void Read_Lime__SkinningWeights(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.SkinningWeights)obj;
			var dg = (BinaryDeserializerGen)d;
			dg.EnsureClassDef(typeof(global::Lime.BoneWeight));
			result.Bone0.Index = d.Reader.ReadInt32();
			result.Bone0.Weight = d.Reader.ReadSingle();
			dg.EnsureClassDef(typeof(global::Lime.BoneWeight));
			result.Bone1.Index = d.Reader.ReadInt32();
			result.Bone1.Weight = d.Reader.ReadSingle();
			dg.EnsureClassDef(typeof(global::Lime.BoneWeight));
			result.Bone2.Index = d.Reader.ReadInt32();
			result.Bone2.Weight = d.Reader.ReadSingle();
			dg.EnsureClassDef(typeof(global::Lime.BoneWeight));
			result.Bone3.Index = d.Reader.ReadInt32();
			result.Bone3.Weight = d.Reader.ReadSingle();
		}

		private static object Make_Lime__SkinningWeights(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.SkinningWeights();
			Read_Lime__SkinningWeights(d, def, result);
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
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
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
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Thickness));
				var tmp12 = new global::Lime.Thickness();
				tmp12.Left = d.Reader.ReadSingle();
				tmp12.Right = d.Reader.ReadSingle();
				tmp12.Top = d.Reader.ReadSingle();
				tmp12.Bottom = d.Reader.ReadSingle();
				result.Padding = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp13 = new global::Lime.Vector2();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				result.Pivot = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp14 = new global::Lime.Vector2();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				result.Position = tmp14;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.RangeMax = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.RangeMin = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp15 = new global::Lime.Vector2();
				tmp15.X = d.Reader.ReadSingle();
				tmp15.Y = d.Reader.ReadSingle();
				result.Scale = tmp15;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp16 = new global::Lime.Vector2();
				tmp16.X = d.Reader.ReadSingle();
				tmp16.Y = d.Reader.ReadSingle();
				result.SilentSize = tmp16;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Step = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (25 == fd.OurIndex) {
				result.Value = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (26 == fd.OurIndex) {
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
				result.Closed = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
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
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Thickness));
				var tmp12 = new global::Lime.Thickness();
				tmp12.Left = d.Reader.ReadSingle();
				tmp12.Right = d.Reader.ReadSingle();
				tmp12.Top = d.Reader.ReadSingle();
				tmp12.Bottom = d.Reader.ReadSingle();
				result.Padding = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp13 = new global::Lime.Vector2();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				result.Pivot = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp14 = new global::Lime.Vector2();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				result.Position = tmp14;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp15 = new global::Lime.Vector2();
				tmp15.X = d.Reader.ReadSingle();
				tmp15.Y = d.Reader.ReadSingle();
				result.Scale = tmp15;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp16 = new global::Lime.Vector2();
				tmp16.X = d.Reader.ReadSingle();
				tmp16.Y = d.Reader.ReadSingle();
				result.SilentSize = tmp16;
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
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
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
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Opaque = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				var tmp12 = new global::Lime.Vector3();
				tmp12.X = d.Reader.ReadSingle();
				tmp12.Y = d.Reader.ReadSingle();
				tmp12.Z = d.Reader.ReadSingle();
				result.Position = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Quaternion));
				var tmp13 = new global::Lime.Quaternion();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				tmp13.Z = d.Reader.ReadSingle();
				tmp13.W = d.Reader.ReadSingle();
				result.Rotation = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				var tmp14 = new global::Lime.Vector3();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				tmp14.Z = d.Reader.ReadSingle();
				result.Scale = tmp14;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
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

		private static void Read_Lime__SplineGear(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.SplineGear)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.AlongPathOrientation = d.Reader.ReadBoolean();
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
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp7 = d.Reader.ReadInt32();
				if (tmp7 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp7 >= 0) {
						var tmp8 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp8);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				var tmp9 = d.Reader.ReadInt32();
				if (tmp9 >= 0) {
					while (--tmp9 >= 0) {
						var tmp10 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp10);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.SplineOffset = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.SplineRef = (global::Lime.NodeReference<global::Lime.Spline>)dg.ReadObject<global::Lime.NodeReference<global::Lime.Spline>>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.WidgetRef = (global::Lime.NodeReference<global::Lime.Widget>)dg.ReadObject<global::Lime.NodeReference<global::Lime.Widget>>();
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

		private static void Read_Lime__SplineGear3D(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.SplineGear3D)obj;
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
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp7 = d.Reader.ReadInt32();
				if (tmp7 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp7 >= 0) {
						var tmp8 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp8);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.NodeRef = (global::Lime.NodeReference<global::Lime.Node3D>)dg.ReadObject<global::Lime.NodeReference<global::Lime.Node3D>>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				var tmp9 = d.Reader.ReadInt32();
				if (tmp9 >= 0) {
					while (--tmp9 >= 0) {
						var tmp10 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp10);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.SplineOffset = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.SplineRef = (global::Lime.NodeReference<global::Lime.Spline3D>)dg.ReadObject<global::Lime.NodeReference<global::Lime.Spline3D>>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__SplineGear3D(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.SplineGear3D();
			Read_Lime__SplineGear3D(d, def, result);
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
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp7 = d.Reader.ReadInt32();
				if (tmp7 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp7 >= 0) {
						var tmp8 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp8);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				var tmp9 = d.Reader.ReadInt32();
				if (tmp9 >= 0) {
					while (--tmp9 >= 0) {
						var tmp10 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp10);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp11 = new global::Lime.Vector2();
				tmp11.X = d.Reader.ReadSingle();
				tmp11.Y = d.Reader.ReadSingle();
				result.Position = tmp11;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Straight = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.TangentAngle = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.TangentWeight = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
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

		private static void Read_Lime__SplinePoint3D(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.SplinePoint3D)obj;
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
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp7 = d.Reader.ReadInt32();
				if (tmp7 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp7 >= 0) {
						var tmp8 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp8);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Interpolation = (global::Lime.SplineInterpolation)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				var tmp9 = d.Reader.ReadInt32();
				if (tmp9 >= 0) {
					while (--tmp9 >= 0) {
						var tmp10 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp10);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				var tmp11 = new global::Lime.Vector3();
				tmp11.X = d.Reader.ReadSingle();
				tmp11.Y = d.Reader.ReadSingle();
				tmp11.Z = d.Reader.ReadSingle();
				result.Position = tmp11;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				var tmp12 = new global::Lime.Vector3();
				tmp12.X = d.Reader.ReadSingle();
				tmp12.Y = d.Reader.ReadSingle();
				tmp12.Z = d.Reader.ReadSingle();
				result.TangentA = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				var tmp13 = new global::Lime.Vector3();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				tmp13.Z = d.Reader.ReadSingle();
				result.TangentB = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__SplinePoint3D(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.SplinePoint3D();
			Read_Lime__SplinePoint3D(d, def, result);
			return result;
		}

		private static void Read_Lime__StackLayout(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.StackLayout)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.DefaultCell = (global::Lime.DefaultLayoutCell)dg.ReadObject<global::Lime.DefaultLayoutCell>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.HorizontallySizeable = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.IgnoreHidden = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.VerticallySizeable = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__StackLayout(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.StackLayout();
			Read_Lime__StackLayout(d, def, result);
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
				result.Material = (global::Lime.IMaterial)dg.ReadObject<global::Lime.IMaterial>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Mesh = (global::Lime.Mesh<global::Lime.Mesh3D.Vertex>)dg.ReadObject<global::Lime.Mesh<global::Lime.Mesh3D.Vertex>>();
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

		private static void Read_Lime__TableLayout(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.TableLayout)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.ColumnCount = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.ColumnDefaults = (global::System.Collections.Generic.List<global::Lime.DefaultLayoutCell>)null;
				var tmp1 = d.Reader.ReadInt32();
				if (tmp1 >= 0) {
					result.ColumnDefaults = new global::System.Collections.Generic.List<global::Lime.DefaultLayoutCell>();
					while (--tmp1 >= 0) {
						var tmp2 = (global::Lime.DefaultLayoutCell)dg.ReadObject<global::Lime.DefaultLayoutCell>();
						result.ColumnDefaults.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.ColumnSpacing = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.DefaultCell = (global::Lime.DefaultLayoutCell)dg.ReadObject<global::Lime.DefaultLayoutCell>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.IgnoreHidden = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.RowCount = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.RowDefaults = (global::System.Collections.Generic.List<global::Lime.DefaultLayoutCell>)null;
				var tmp3 = d.Reader.ReadInt32();
				if (tmp3 >= 0) {
					result.RowDefaults = new global::System.Collections.Generic.List<global::Lime.DefaultLayoutCell>();
					while (--tmp3 >= 0) {
						var tmp4 = (global::Lime.DefaultLayoutCell)dg.ReadObject<global::Lime.DefaultLayoutCell>();
						result.RowDefaults.Add(tmp4);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.RowSpacing = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__TableLayout(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.TableLayout();
			Read_Lime__TableLayout(d, def, result);
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
				var tmp5 = d.Reader.ReadInt32();
				if (tmp5 >= 0) {
					while (--tmp5 >= 0) {
						var tmp6 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp6);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp7 = d.Reader.ReadInt32();
				if (tmp7 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp7 >= 0) {
						var tmp8 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp8);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Font = (global::Lime.SerializableFont)dg.ReadObject<global::Lime.SerializableFont>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.GradientMapIndex = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp9 = new global::Lime.Vector2();
				tmp9.X = d.Reader.ReadSingle();
				tmp9.Y = d.Reader.ReadSingle();
				result.ImageSize = tmp9;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				result.ImageTexture = (global::Lime.ITexture)dg.ReadObject<global::Lime.ITexture>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.ImageUsage = (global::Lime.TextStyle.ImageUsageEnum)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.LetterSpacing = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp12 = new global::Lime.Color4();
				tmp12.ABGR = d.Reader.ReadUInt32();
				result.ShadowColor = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp13 = new global::Lime.Vector2();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				result.ShadowOffset = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				result.Size = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.SpaceAfter = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp14 = new global::Lime.Color4();
				tmp14.ABGR = d.Reader.ReadUInt32();
				result.TextColor = tmp14;
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
				dg.EnsureClassDef(typeof(global::Lime.IntRectangle));
				dg.EnsureClassDef(typeof(global::Lime.IntVector2));
				result.AtlasRect.A.X = d.Reader.ReadInt32();
				result.AtlasRect.A.Y = d.Reader.ReadInt32();
				dg.EnsureClassDef(typeof(global::Lime.IntVector2));
				result.AtlasRect.B.X = d.Reader.ReadInt32();
				result.AtlasRect.B.Y = d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
			return result;
		}

		private static void Read_Lime__TextureParams(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.TextureParams)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.MagFilter = (global::Lime.TextureFilter)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.MinFilter = (global::Lime.TextureFilter)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.WrapModeU = (global::Lime.TextureWrapMode)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.WrapModeV = (global::Lime.TextureWrapMode)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__TextureParams(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.TextureParams();
			Read_Lime__TextureParams(d, def, result);
			return result;
		}

		private static object Make_Lime__Thickness(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Thickness();
			result.Left = d.Reader.ReadSingle();
			result.Right = d.Reader.ReadSingle();
			result.Top = d.Reader.ReadSingle();
			result.Bottom = d.Reader.ReadSingle();
			return result;
		}

		private static void Read_Lime__ThicknessAnimator(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.ThicknessAnimator)obj;
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
						var tmp2 = (global::Lime.Keyframe<global::Lime.Thickness>)dg.ReadObject<global::Lime.Keyframe<global::Lime.Thickness>>();
						result.ReadonlyKeys.Add(tmp2);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__ThicknessAnimator(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.ThicknessAnimator();
			Read_Lime__ThicknessAnimator(d, def, result);
			return result;
		}

		private static void Read_Lime__TiledImage(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.TiledImage)obj;
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
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
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
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Thickness));
				var tmp12 = new global::Lime.Thickness();
				tmp12.Left = d.Reader.ReadSingle();
				tmp12.Right = d.Reader.ReadSingle();
				tmp12.Top = d.Reader.ReadSingle();
				tmp12.Bottom = d.Reader.ReadSingle();
				result.Padding = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp13 = new global::Lime.Vector2();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				result.Pivot = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp14 = new global::Lime.Vector2();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				result.Position = tmp14;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp15 = new global::Lime.Vector2();
				tmp15.X = d.Reader.ReadSingle();
				tmp15.Y = d.Reader.ReadSingle();
				result.Scale = tmp15;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp16 = new global::Lime.Vector2();
				tmp16.X = d.Reader.ReadSingle();
				tmp16.Y = d.Reader.ReadSingle();
				result.SilentSize = tmp16;
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
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Texture = (global::Lime.ITexture)dg.ReadObject<global::Lime.ITexture>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp17 = new global::Lime.Vector2();
				tmp17.X = d.Reader.ReadSingle();
				tmp17.Y = d.Reader.ReadSingle();
				result.TileOffset = tmp17;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
				result.TileRounding = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (25 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp18 = new global::Lime.Vector2();
				tmp18.X = d.Reader.ReadSingle();
				tmp18.Y = d.Reader.ReadSingle();
				result.TileSize = tmp18;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (26 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__TiledImage(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.TiledImage();
			Read_Lime__TiledImage(d, def, result);
			return result;
		}

		private static void Read_Lime__VBoxLayout(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.VBoxLayout)obj;
			var dg = (BinaryDeserializerGen)d;
			ReaderClassDef.FieldDef fd;
			fd = def.Fields[d.Reader.ReadInt16()];
			if (1 == fd.OurIndex) {
				result.DefaultCell = (global::Lime.DefaultLayoutCell)dg.ReadObject<global::Lime.DefaultLayoutCell>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (2 == fd.OurIndex) {
				result.Direction = (global::Lime.LayoutDirection)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (3 == fd.OurIndex) {
				result.IgnoreHidden = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				result.Spacing = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__VBoxLayout(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.VBoxLayout();
			Read_Lime__VBoxLayout(d, def, result);
			return result;
		}

		private static object Make_Lime__Vector2(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Vector2();
			result.X = d.Reader.ReadSingle();
			result.Y = d.Reader.ReadSingle();
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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

		private static object Make_Lime__Vector3(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Vector3();
			result.X = d.Reader.ReadSingle();
			result.Y = d.Reader.ReadSingle();
			result.Z = d.Reader.ReadSingle();
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
				result.TargetPropertyPath = d.Reader.ReadString();
				if (result.TargetPropertyPath == "" && d.Reader.ReadBoolean()) result.TargetPropertyPath = null;
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

		private static object Make_Lime__Vector4(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.Vector4();
			result.X = d.Reader.ReadSingle();
			result.Y = d.Reader.ReadSingle();
			result.Z = d.Reader.ReadSingle();
			result.W = d.Reader.ReadSingle();
			return result;
		}

		private static void Read_Lime__VideoPlayer(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.VideoPlayer)obj;
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
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
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
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Thickness));
				var tmp12 = new global::Lime.Thickness();
				tmp12.Left = d.Reader.ReadSingle();
				tmp12.Right = d.Reader.ReadSingle();
				tmp12.Top = d.Reader.ReadSingle();
				tmp12.Bottom = d.Reader.ReadSingle();
				result.Padding = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp13 = new global::Lime.Vector2();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				result.Pivot = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp14 = new global::Lime.Vector2();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				result.Position = tmp14;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp15 = new global::Lime.Vector2();
				tmp15.X = d.Reader.ReadSingle();
				tmp15.Y = d.Reader.ReadSingle();
				result.Scale = tmp15;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp16 = new global::Lime.Vector2();
				tmp16.X = d.Reader.ReadSingle();
				tmp16.Y = d.Reader.ReadSingle();
				result.SilentSize = tmp16;
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
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Texture = (global::Lime.ITexture)dg.ReadObject<global::Lime.ITexture>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp17 = new global::Lime.Vector2();
				tmp17.X = d.Reader.ReadSingle();
				tmp17.Y = d.Reader.ReadSingle();
				result.UV0 = tmp17;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp18 = new global::Lime.Vector2();
				tmp18.X = d.Reader.ReadSingle();
				tmp18.Y = d.Reader.ReadSingle();
				result.UV1 = tmp18;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (25 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__VideoPlayer(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.VideoPlayer();
			Read_Lime__VideoPlayer(d, def, result);
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
				result.CameraRef = (global::Lime.NodeReference<global::Lime.Camera3D>)dg.ReadObject<global::Lime.NodeReference<global::Lime.Camera3D>>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				result.Frame = d.Reader.ReadSingle();
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
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Thickness));
				var tmp12 = new global::Lime.Thickness();
				tmp12.Left = d.Reader.ReadSingle();
				tmp12.Right = d.Reader.ReadSingle();
				tmp12.Top = d.Reader.ReadSingle();
				tmp12.Bottom = d.Reader.ReadSingle();
				result.Padding = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp13 = new global::Lime.Vector2();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				result.Pivot = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp14 = new global::Lime.Vector2();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				result.Position = tmp14;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp15 = new global::Lime.Vector2();
				tmp15.X = d.Reader.ReadSingle();
				tmp15.Y = d.Reader.ReadSingle();
				result.Scale = tmp15;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (19 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (20 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp16 = new global::Lime.Vector2();
				tmp16.X = d.Reader.ReadSingle();
				tmp16.Y = d.Reader.ReadSingle();
				result.SilentSize = tmp16;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (21 == fd.OurIndex) {
				result.SkinningWeights = (global::Lime.SkinningWeights)dg.ReadObject<global::Lime.SkinningWeights>();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (23 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (24 == fd.OurIndex) {
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
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
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
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Thickness));
				var tmp12 = new global::Lime.Thickness();
				tmp12.Left = d.Reader.ReadSingle();
				tmp12.Right = d.Reader.ReadSingle();
				tmp12.Top = d.Reader.ReadSingle();
				tmp12.Bottom = d.Reader.ReadSingle();
				result.Padding = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp13 = new global::Lime.Vector2();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				result.Pivot = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp14 = new global::Lime.Vector2();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				result.Position = tmp14;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Rotation = d.Reader.ReadSingle();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (16 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp15 = new global::Lime.Vector2();
				tmp15.X = d.Reader.ReadSingle();
				tmp15.Y = d.Reader.ReadSingle();
				result.Scale = tmp15;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (17 == fd.OurIndex) {
				result.Shader = (global::Lime.ShaderId)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (18 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector2));
				var tmp16 = new global::Lime.Vector2();
				tmp16.X = d.Reader.ReadSingle();
				tmp16.Y = d.Reader.ReadSingle();
				result.SilentSize = tmp16;
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
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (22 == fd.OurIndex) {
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

		private static void Read_Lime__WidgetAdapter3D(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			var result = (global::Lime.WidgetAdapter3D)obj;
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
				dg.EnsureClassDef(typeof(global::Lime.Color4));
				var tmp5 = new global::Lime.Color4();
				tmp5.ABGR = d.Reader.ReadUInt32();
				result.Color = tmp5;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (4 == fd.OurIndex) {
				var tmp6 = d.Reader.ReadInt32();
				if (tmp6 >= 0) {
					while (--tmp6 >= 0) {
						var tmp7 = (global::Lime.NodeComponent)dg.ReadObject<global::Lime.NodeComponent>();
						result.Components.Add(tmp7);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (5 == fd.OurIndex) {
				result.ContentsPath = d.Reader.ReadString();
				if (result.ContentsPath == "" && d.Reader.ReadBoolean()) result.ContentsPath = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (6 == fd.OurIndex) {
				result.Folders = (global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>)null;
				var tmp8 = d.Reader.ReadInt32();
				if (tmp8 >= 0) {
					result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
					while (--tmp8 >= 0) {
						var tmp9 = (global::Lime.Folder.Descriptor)dg.ReadObject<global::Lime.Folder.Descriptor>();
						result.Folders.Add(tmp9);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (7 == fd.OurIndex) {
				result.Id = d.Reader.ReadString();
				if (result.Id == "" && d.Reader.ReadBoolean()) result.Id = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (8 == fd.OurIndex) {
				var tmp10 = d.Reader.ReadInt32();
				if (tmp10 >= 0) {
					while (--tmp10 >= 0) {
						var tmp11 = (global::Lime.Node)dg.ReadObject<global::Lime.Node>();
						result.Nodes.Add(tmp11);
					}
				}
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (9 == fd.OurIndex) {
				result.Opaque = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (10 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				var tmp12 = new global::Lime.Vector3();
				tmp12.X = d.Reader.ReadSingle();
				tmp12.Y = d.Reader.ReadSingle();
				tmp12.Z = d.Reader.ReadSingle();
				result.Position = tmp12;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (11 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Quaternion));
				var tmp13 = new global::Lime.Quaternion();
				tmp13.X = d.Reader.ReadSingle();
				tmp13.Y = d.Reader.ReadSingle();
				tmp13.Z = d.Reader.ReadSingle();
				tmp13.W = d.Reader.ReadSingle();
				result.Rotation = tmp13;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (12 == fd.OurIndex) {
				dg.EnsureClassDef(typeof(global::Lime.Vector3));
				var tmp14 = new global::Lime.Vector3();
				tmp14.X = d.Reader.ReadSingle();
				tmp14.Y = d.Reader.ReadSingle();
				tmp14.Z = d.Reader.ReadSingle();
				result.Scale = tmp14;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (13 == fd.OurIndex) {
				result.Tag = d.Reader.ReadString();
				if (result.Tag == "" && d.Reader.ReadBoolean()) result.Tag = null;
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (14 == fd.OurIndex) {
				result.TangerineFlags = (global::Lime.TangerineFlags)d.Reader.ReadInt32();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (15 == fd.OurIndex) {
				result.Visible = d.Reader.ReadBoolean();
				fd = def.Fields[d.Reader.ReadInt16()];
			}
			if (fd.OurIndex != ReaderClassDef.EOF) throw dg.Error("Unfinished object");
		}

		private static object Make_Lime__WidgetAdapter3D(BinaryDeserializer d, ReaderClassDef def)
		{
			var result = new global::Lime.WidgetAdapter3D();
			Read_Lime__WidgetAdapter3D(d, def, result);
			return result;
		}

		static BinaryDeserializerGen()
		{
			readCache[typeof(global::Lime.Animation)] = Read_Lime__Animation;
			readCache[typeof(global::Lime.Animation.AnimationData)] = Read_Lime__Animation__AnimationData;
			readCache[typeof(global::Lime.AnimationBlender)] = Read_Lime__AnimationBlender;
			readCache[typeof(global::Lime.AnimationBlending)] = Read_Lime__AnimationBlending;
			readCache[typeof(global::Lime.Animator<global::Lime.Alignment>)] = Read_Lime__Animator_Alignment;
			readCache[typeof(global::Lime.Animator<global::Lime.Anchors>)] = Read_Lime__Animator_Anchors;
			readCache[typeof(global::Lime.Animator<global::Lime.AudioAction>)] = Read_Lime__Animator_AudioAction;
			readCache[typeof(global::Lime.Animator<global::Lime.Blending>)] = Read_Lime__Animator_Blending;
			readCache[typeof(global::Lime.Animator<global::Lime.ClipMethod>)] = Read_Lime__Animator_ClipMethod;
			readCache[typeof(global::Lime.Animator<global::Lime.Color4>)] = Read_Lime__Animator_Color4;
			readCache[typeof(global::Lime.Animator<global::Lime.EmissionType>)] = Read_Lime__Animator_EmissionType;
			readCache[typeof(global::Lime.Animator<global::Lime.EmitterShape>)] = Read_Lime__Animator_EmitterShape;
			readCache[typeof(global::Lime.Animator<global::Lime.HAlignment>)] = Read_Lime__Animator_HAlignment;
			readCache[typeof(global::Lime.Animator<global::Lime.ITexture>)] = Read_Lime__Animator_ITexture;
			readCache[typeof(global::Lime.Animator<global::Lime.LayoutDirection>)] = Read_Lime__Animator_LayoutDirection;
			readCache[typeof(global::Lime.Animator<global::Lime.Matrix44>)] = Read_Lime__Animator_Matrix44;
			readCache[typeof(global::Lime.Animator<global::Lime.MovieAction>)] = Read_Lime__Animator_MovieAction;
			readCache[typeof(global::Lime.Animator<global::Lime.NodeReference<global::Lime.Camera3D>>)] = Read_Lime__Animator_NodeReference_Camera3D;
			readCache[typeof(global::Lime.Animator<global::Lime.NodeReference<global::Lime.Node3D>>)] = Read_Lime__Animator_NodeReference_Node3D;
			readCache[typeof(global::Lime.Animator<global::Lime.NodeReference<global::Lime.Spline>>)] = Read_Lime__Animator_NodeReference_Spline;
			readCache[typeof(global::Lime.Animator<global::Lime.NodeReference<global::Lime.Spline3D>>)] = Read_Lime__Animator_NodeReference_Spline3D;
			readCache[typeof(global::Lime.Animator<global::Lime.NodeReference<global::Lime.Widget>>)] = Read_Lime__Animator_NodeReference_Widget;
			readCache[typeof(global::Lime.Animator<global::Lime.NumericRange>)] = Read_Lime__Animator_NumericRange;
			readCache[typeof(global::Lime.Animator<global::Lime.ParticlesLinkage>)] = Read_Lime__Animator_ParticlesLinkage;
			readCache[typeof(global::Lime.Animator<global::Lime.Quaternion>)] = Read_Lime__Animator_Quaternion;
			readCache[typeof(global::Lime.Animator<global::Lime.RenderTarget>)] = Read_Lime__Animator_RenderTarget;
			readCache[typeof(global::Lime.Animator<global::Lime.SerializableFont>)] = Read_Lime__Animator_SerializableFont;
			readCache[typeof(global::Lime.Animator<global::Lime.SerializableSample>)] = Read_Lime__Animator_SerializableSample;
			readCache[typeof(global::Lime.Animator<global::Lime.ShaderId>)] = Read_Lime__Animator_ShaderId;
			readCache[typeof(global::Lime.Animator<global::Lime.TextOverflowMode>)] = Read_Lime__Animator_TextOverflowMode;
			readCache[typeof(global::Lime.Animator<global::Lime.Thickness>)] = Read_Lime__Animator_Thickness;
			readCache[typeof(global::Lime.Animator<global::Lime.VAlignment>)] = Read_Lime__Animator_VAlignment;
			readCache[typeof(global::Lime.Animator<global::Lime.Vector2>)] = Read_Lime__Animator_Vector2;
			readCache[typeof(global::Lime.Animator<global::Lime.Vector3>)] = Read_Lime__Animator_Vector3;
			readCache[typeof(global::Lime.Animator<global::System.Boolean>)] = Read_Lime__Animator_Boolean;
			readCache[typeof(global::Lime.Animator<global::System.Int32>)] = Read_Lime__Animator_Int32;
			readCache[typeof(global::Lime.Animator<global::System.Single>)] = Read_Lime__Animator_Single;
			readCache[typeof(global::Lime.Animator<global::System.String>)] = Read_Lime__Animator_String;
			readCache[typeof(global::Lime.Audio)] = Read_Lime__Audio;
			readCache[typeof(global::Lime.BaseShadowParams)] = Read_Lime__BaseShadowParams;
			readCache[typeof(global::Lime.BlendingOption)] = Read_Lime__BlendingOption;
			readCache[typeof(global::Lime.Bone)] = Read_Lime__Bone;
			readCache[typeof(global::Lime.Button)] = Read_Lime__Button;
			readCache[typeof(global::Lime.Camera3D)] = Read_Lime__Camera3D;
			readCache[typeof(global::Lime.Color4Animator)] = Read_Lime__Color4Animator;
			readCache[typeof(global::Lime.CommonMaterial)] = Read_Lime__CommonMaterial;
			readCache[typeof(global::Lime.DefaultLayoutCell)] = Read_Lime__DefaultLayoutCell;
			readCache[typeof(global::Lime.DistortionMesh)] = Read_Lime__DistortionMesh;
			readCache[typeof(global::Lime.DistortionMeshPoint)] = Read_Lime__DistortionMeshPoint;
			readCache[typeof(global::Lime.EmitterShapePoint)] = Read_Lime__EmitterShapePoint;
			readCache[typeof(global::Lime.Font)] = Read_Lime__Font;
			readCache[typeof(global::Lime.FontChar)] = Read_Lime__FontChar;
			readCache[typeof(global::Lime.Frame)] = Read_Lime__Frame;
			readCache[typeof(global::Lime.GradientComponent)] = Read_Lime__GradientComponent;
			readCache[typeof(global::Lime.GradientControlPoint)] = Read_Lime__GradientControlPoint;
			readCache[typeof(global::Lime.HBoxLayout)] = Read_Lime__HBoxLayout;
			readCache[typeof(global::Lime.HSLComponent)] = Read_Lime__HSLComponent;
			readCache[typeof(global::Lime.Image)] = Read_Lime__Image;
			readCache[typeof(global::Lime.ImageCombiner)] = Read_Lime__ImageCombiner;
			readCache[typeof(global::Lime.IntAnimator)] = Read_Lime__IntAnimator;
			readCache[typeof(global::Lime.Keyframe<global::Lime.Alignment>)] = Read_Lime__Keyframe_Alignment;
			readCache[typeof(global::Lime.Keyframe<global::Lime.Anchors>)] = Read_Lime__Keyframe_Anchors;
			readCache[typeof(global::Lime.Keyframe<global::Lime.AudioAction>)] = Read_Lime__Keyframe_AudioAction;
			readCache[typeof(global::Lime.Keyframe<global::Lime.Blending>)] = Read_Lime__Keyframe_Blending;
			readCache[typeof(global::Lime.Keyframe<global::Lime.ClipMethod>)] = Read_Lime__Keyframe_ClipMethod;
			readCache[typeof(global::Lime.Keyframe<global::Lime.Color4>)] = Read_Lime__Keyframe_Color4;
			readCache[typeof(global::Lime.Keyframe<global::Lime.EmissionType>)] = Read_Lime__Keyframe_EmissionType;
			readCache[typeof(global::Lime.Keyframe<global::Lime.EmitterShape>)] = Read_Lime__Keyframe_EmitterShape;
			readCache[typeof(global::Lime.Keyframe<global::Lime.HAlignment>)] = Read_Lime__Keyframe_HAlignment;
			readCache[typeof(global::Lime.Keyframe<global::Lime.ITexture>)] = Read_Lime__Keyframe_ITexture;
			readCache[typeof(global::Lime.Keyframe<global::Lime.LayoutDirection>)] = Read_Lime__Keyframe_LayoutDirection;
			readCache[typeof(global::Lime.Keyframe<global::Lime.Matrix44>)] = Read_Lime__Keyframe_Matrix44;
			readCache[typeof(global::Lime.Keyframe<global::Lime.MovieAction>)] = Read_Lime__Keyframe_MovieAction;
			readCache[typeof(global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Camera3D>>)] = Read_Lime__Keyframe_NodeReference_Camera3D;
			readCache[typeof(global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Node3D>>)] = Read_Lime__Keyframe_NodeReference_Node3D;
			readCache[typeof(global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline>>)] = Read_Lime__Keyframe_NodeReference_Spline;
			readCache[typeof(global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline3D>>)] = Read_Lime__Keyframe_NodeReference_Spline3D;
			readCache[typeof(global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Widget>>)] = Read_Lime__Keyframe_NodeReference_Widget;
			readCache[typeof(global::Lime.Keyframe<global::Lime.NumericRange>)] = Read_Lime__Keyframe_NumericRange;
			readCache[typeof(global::Lime.Keyframe<global::Lime.ParticlesLinkage>)] = Read_Lime__Keyframe_ParticlesLinkage;
			readCache[typeof(global::Lime.Keyframe<global::Lime.Quaternion>)] = Read_Lime__Keyframe_Quaternion;
			readCache[typeof(global::Lime.Keyframe<global::Lime.RenderTarget>)] = Read_Lime__Keyframe_RenderTarget;
			readCache[typeof(global::Lime.Keyframe<global::Lime.SerializableFont>)] = Read_Lime__Keyframe_SerializableFont;
			readCache[typeof(global::Lime.Keyframe<global::Lime.SerializableSample>)] = Read_Lime__Keyframe_SerializableSample;
			readCache[typeof(global::Lime.Keyframe<global::Lime.ShaderId>)] = Read_Lime__Keyframe_ShaderId;
			readCache[typeof(global::Lime.Keyframe<global::Lime.TextOverflowMode>)] = Read_Lime__Keyframe_TextOverflowMode;
			readCache[typeof(global::Lime.Keyframe<global::Lime.Thickness>)] = Read_Lime__Keyframe_Thickness;
			readCache[typeof(global::Lime.Keyframe<global::Lime.VAlignment>)] = Read_Lime__Keyframe_VAlignment;
			readCache[typeof(global::Lime.Keyframe<global::Lime.Vector2>)] = Read_Lime__Keyframe_Vector2;
			readCache[typeof(global::Lime.Keyframe<global::Lime.Vector3>)] = Read_Lime__Keyframe_Vector3;
			readCache[typeof(global::Lime.Keyframe<global::System.Boolean>)] = Read_Lime__Keyframe_Boolean;
			readCache[typeof(global::Lime.Keyframe<global::System.Int32>)] = Read_Lime__Keyframe_Int32;
			readCache[typeof(global::Lime.Keyframe<global::System.Single>)] = Read_Lime__Keyframe_Single;
			readCache[typeof(global::Lime.Keyframe<global::System.String>)] = Read_Lime__Keyframe_String;
			readCache[typeof(global::Lime.LayoutCell)] = Read_Lime__LayoutCell;
			readCache[typeof(global::Lime.LayoutConstraints)] = Read_Lime__LayoutConstraints;
			readCache[typeof(global::Lime.LinearLayout)] = Read_Lime__LinearLayout;
			readCache[typeof(global::Lime.Marker)] = Read_Lime__Marker;
			readCache[typeof(global::Lime.MarkerBlending)] = Read_Lime__MarkerBlending;
			readCache[typeof(global::Lime.Matrix44Animator)] = Read_Lime__Matrix44Animator;
			readCache[typeof(global::Lime.Mesh<global::Lime.Mesh3D.Vertex>)] = Read_Lime__Mesh_Mesh3D__Vertex;
			readCache[typeof(global::Lime.Mesh3D)] = Read_Lime__Mesh3D;
			readCache[typeof(global::Lime.Model3D)] = Read_Lime__Model3D;
			readCache[typeof(global::Lime.Model3DAttachment.MaterialRemap)] = Read_Lime__Model3DAttachment__MaterialRemap;
			readCache[typeof(global::Lime.Model3DAttachmentParser.MeshOptionFormat)] = Read_Lime__Model3DAttachmentParser__MeshOptionFormat;
			readCache[typeof(global::Lime.Model3DAttachmentParser.ModelAnimationFormat)] = Read_Lime__Model3DAttachmentParser__ModelAnimationFormat;
			readCache[typeof(global::Lime.Model3DAttachmentParser.ModelAttachmentFormat)] = Read_Lime__Model3DAttachmentParser__ModelAttachmentFormat;
			readCache[typeof(global::Lime.Model3DAttachmentParser.ModelComponentsFormat)] = Read_Lime__Model3DAttachmentParser__ModelComponentsFormat;
			readCache[typeof(global::Lime.Model3DAttachmentParser.ModelMarkerFormat)] = Read_Lime__Model3DAttachmentParser__ModelMarkerFormat;
			readCache[typeof(global::Lime.Model3DAttachmentParser.UVAnimationFormat)] = Read_Lime__Model3DAttachmentParser__UVAnimationFormat;
			readCache[typeof(global::Lime.Movie)] = Read_Lime__Movie;
			readCache[typeof(global::Lime.NineGrid)] = Read_Lime__NineGrid;
			readCache[typeof(global::Lime.Node3D)] = Read_Lime__Node3D;
			readCache[typeof(global::Lime.NodeReference<global::Lime.Camera3D>)] = Read_Lime__NodeReference_Camera3D;
			readCache[typeof(global::Lime.NodeReference<global::Lime.Spline>)] = Read_Lime__NodeReference_Spline;
			readCache[typeof(global::Lime.NodeReference<global::Lime.Widget>)] = Read_Lime__NodeReference_Widget;
			readCache[typeof(global::Lime.NumericAnimator)] = Read_Lime__NumericAnimator;
			readCache[typeof(global::Lime.ParticleEmitter)] = Read_Lime__ParticleEmitter;
			readCache[typeof(global::Lime.ParticleModifier)] = Read_Lime__ParticleModifier;
			readCache[typeof(global::Lime.ParticlesMagnet)] = Read_Lime__ParticlesMagnet;
			readCache[typeof(global::Lime.PointObject)] = Read_Lime__PointObject;
			readCache[typeof(global::Lime.Polyline)] = Read_Lime__Polyline;
			readCache[typeof(global::Lime.PolylinePoint)] = Read_Lime__PolylinePoint;
			readCache[typeof(global::Lime.PostProcessingComponent)] = Read_Lime__PostProcessingComponent;
			readCache[typeof(global::Lime.QuaternionAnimator)] = Read_Lime__QuaternionAnimator;
			readCache[typeof(global::Lime.RenderOptimizer.ContentBox)] = Read_Lime_RenderOptimizer__ContentBox;
			readCache[typeof(global::Lime.RenderOptimizer.ContentPlane)] = Read_Lime_RenderOptimizer__ContentPlane;
			readCache[typeof(global::Lime.RenderOptimizer.ContentRectangle)] = Read_Lime_RenderOptimizer__ContentRectangle;
			readCache[typeof(global::Lime.RenderOptimizer.ContentSizeComponent)] = Read_Lime_RenderOptimizer__ContentSizeComponent;
			readCache[typeof(global::Lime.RichText)] = Read_Lime__RichText;
			readCache[typeof(global::Lime.SerializableFont)] = Read_Lime__SerializableFont;
			readCache[typeof(global::Lime.SerializableSample)] = Read_Lime__SerializableSample;
			readCache[typeof(global::Lime.SerializableTexture)] = Read_Lime__SerializableTexture;
			readCache[typeof(global::Lime.ShadowParams)] = Read_Lime__ShadowParams;
			readCache[typeof(global::Lime.SignedDistanceFieldComponent)] = Read_Lime__SignedDistanceFieldComponent;
			readCache[typeof(global::Lime.SimpleText)] = Read_Lime__SimpleText;
			readCache[typeof(global::Lime.SkinningWeights)] = Read_Lime__SkinningWeights;
			readCache[typeof(global::Lime.Slider)] = Read_Lime__Slider;
			readCache[typeof(global::Lime.Spline)] = Read_Lime__Spline;
			readCache[typeof(global::Lime.Spline3D)] = Read_Lime__Spline3D;
			readCache[typeof(global::Lime.SplineGear)] = Read_Lime__SplineGear;
			readCache[typeof(global::Lime.SplineGear3D)] = Read_Lime__SplineGear3D;
			readCache[typeof(global::Lime.SplinePoint)] = Read_Lime__SplinePoint;
			readCache[typeof(global::Lime.SplinePoint3D)] = Read_Lime__SplinePoint3D;
			readCache[typeof(global::Lime.StackLayout)] = Read_Lime__StackLayout;
			readCache[typeof(global::Lime.Submesh3D)] = Read_Lime__Submesh3D;
			readCache[typeof(global::Lime.TableLayout)] = Read_Lime__TableLayout;
			readCache[typeof(global::Lime.TextStyle)] = Read_Lime__TextStyle;
			readCache[typeof(global::Lime.TextureParams)] = Read_Lime__TextureParams;
			readCache[typeof(global::Lime.ThicknessAnimator)] = Read_Lime__ThicknessAnimator;
			readCache[typeof(global::Lime.TiledImage)] = Read_Lime__TiledImage;
			readCache[typeof(global::Lime.VBoxLayout)] = Read_Lime__VBoxLayout;
			readCache[typeof(global::Lime.Vector2Animator)] = Read_Lime__Vector2Animator;
			readCache[typeof(global::Lime.Vector3Animator)] = Read_Lime__Vector3Animator;
			readCache[typeof(global::Lime.VideoPlayer)] = Read_Lime__VideoPlayer;
			readCache[typeof(global::Lime.Viewport3D)] = Read_Lime__Viewport3D;
			readCache[typeof(global::Lime.Widget)] = Read_Lime__Widget;
			readCache[typeof(global::Lime.WidgetAdapter3D)] = Read_Lime__WidgetAdapter3D;
			makeCache[typeof(global::Lime.Alignment)] = Make_Lime__Alignment;
			makeCache[typeof(global::Lime.Animation)] = Make_Lime__Animation;
			makeCache[typeof(global::Lime.Animation.AnimationData)] = Make_Lime__Animation__AnimationData;
			makeCache[typeof(global::Lime.AnimationBlender)] = Make_Lime__AnimationBlender;
			makeCache[typeof(global::Lime.AnimationBlending)] = Make_Lime__AnimationBlending;
			makeCache[typeof(global::Lime.Animator<global::Lime.Alignment>)] = Make_Lime__Animator_Alignment;
			makeCache[typeof(global::Lime.Animator<global::Lime.Anchors>)] = Make_Lime__Animator_Anchors;
			makeCache[typeof(global::Lime.Animator<global::Lime.AudioAction>)] = Make_Lime__Animator_AudioAction;
			makeCache[typeof(global::Lime.Animator<global::Lime.Blending>)] = Make_Lime__Animator_Blending;
			makeCache[typeof(global::Lime.Animator<global::Lime.ClipMethod>)] = Make_Lime__Animator_ClipMethod;
			makeCache[typeof(global::Lime.Animator<global::Lime.Color4>)] = Make_Lime__Animator_Color4;
			makeCache[typeof(global::Lime.Animator<global::Lime.EmissionType>)] = Make_Lime__Animator_EmissionType;
			makeCache[typeof(global::Lime.Animator<global::Lime.EmitterShape>)] = Make_Lime__Animator_EmitterShape;
			makeCache[typeof(global::Lime.Animator<global::Lime.HAlignment>)] = Make_Lime__Animator_HAlignment;
			makeCache[typeof(global::Lime.Animator<global::Lime.ITexture>)] = Make_Lime__Animator_ITexture;
			makeCache[typeof(global::Lime.Animator<global::Lime.LayoutDirection>)] = Make_Lime__Animator_LayoutDirection;
			makeCache[typeof(global::Lime.Animator<global::Lime.Matrix44>)] = Make_Lime__Animator_Matrix44;
			makeCache[typeof(global::Lime.Animator<global::Lime.MovieAction>)] = Make_Lime__Animator_MovieAction;
			makeCache[typeof(global::Lime.Animator<global::Lime.NodeReference<global::Lime.Camera3D>>)] = Make_Lime__Animator_NodeReference_Camera3D;
			makeCache[typeof(global::Lime.Animator<global::Lime.NodeReference<global::Lime.Node3D>>)] = Make_Lime__Animator_NodeReference_Node3D;
			makeCache[typeof(global::Lime.Animator<global::Lime.NodeReference<global::Lime.Spline>>)] = Make_Lime__Animator_NodeReference_Spline;
			makeCache[typeof(global::Lime.Animator<global::Lime.NodeReference<global::Lime.Spline3D>>)] = Make_Lime__Animator_NodeReference_Spline3D;
			makeCache[typeof(global::Lime.Animator<global::Lime.NodeReference<global::Lime.Widget>>)] = Make_Lime__Animator_NodeReference_Widget;
			makeCache[typeof(global::Lime.Animator<global::Lime.NumericRange>)] = Make_Lime__Animator_NumericRange;
			makeCache[typeof(global::Lime.Animator<global::Lime.ParticlesLinkage>)] = Make_Lime__Animator_ParticlesLinkage;
			makeCache[typeof(global::Lime.Animator<global::Lime.Quaternion>)] = Make_Lime__Animator_Quaternion;
			makeCache[typeof(global::Lime.Animator<global::Lime.RenderTarget>)] = Make_Lime__Animator_RenderTarget;
			makeCache[typeof(global::Lime.Animator<global::Lime.SerializableFont>)] = Make_Lime__Animator_SerializableFont;
			makeCache[typeof(global::Lime.Animator<global::Lime.SerializableSample>)] = Make_Lime__Animator_SerializableSample;
			makeCache[typeof(global::Lime.Animator<global::Lime.ShaderId>)] = Make_Lime__Animator_ShaderId;
			makeCache[typeof(global::Lime.Animator<global::Lime.TextOverflowMode>)] = Make_Lime__Animator_TextOverflowMode;
			makeCache[typeof(global::Lime.Animator<global::Lime.Thickness>)] = Make_Lime__Animator_Thickness;
			makeCache[typeof(global::Lime.Animator<global::Lime.VAlignment>)] = Make_Lime__Animator_VAlignment;
			makeCache[typeof(global::Lime.Animator<global::Lime.Vector2>)] = Make_Lime__Animator_Vector2;
			makeCache[typeof(global::Lime.Animator<global::Lime.Vector3>)] = Make_Lime__Animator_Vector3;
			makeCache[typeof(global::Lime.Animator<global::System.Boolean>)] = Make_Lime__Animator_Boolean;
			makeCache[typeof(global::Lime.Animator<global::System.Int32>)] = Make_Lime__Animator_Int32;
			makeCache[typeof(global::Lime.Animator<global::System.Single>)] = Make_Lime__Animator_Single;
			makeCache[typeof(global::Lime.Animator<global::System.String>)] = Make_Lime__Animator_String;
			makeCache[typeof(global::Lime.Audio)] = Make_Lime__Audio;
			makeCache[typeof(global::Lime.BaseShadowParams)] = Make_Lime__BaseShadowParams;
			makeCache[typeof(global::Lime.BitSet32)] = Make_Lime__BitSet32;
			makeCache[typeof(global::Lime.BlendingOption)] = Make_Lime__BlendingOption;
			makeCache[typeof(global::Lime.Bone)] = Make_Lime__Bone;
			makeCache[typeof(global::Lime.BoneArray)] = Make_Lime__BoneArray;
			makeCache[typeof(global::Lime.BoneWeight)] = Make_Lime__BoneWeight;
			makeCache[typeof(global::Lime.BoundingSphere)] = Make_Lime__BoundingSphere;
			makeCache[typeof(global::Lime.Bounds)] = Make_Lime__Bounds;
			makeCache[typeof(global::Lime.Button)] = Make_Lime__Button;
			makeCache[typeof(global::Lime.Camera3D)] = Make_Lime__Camera3D;
			makeCache[typeof(global::Lime.Color4)] = Make_Lime__Color4;
			makeCache[typeof(global::Lime.Color4Animator)] = Make_Lime__Color4Animator;
			makeCache[typeof(global::Lime.CommonMaterial)] = Make_Lime__CommonMaterial;
			makeCache[typeof(global::Lime.DefaultLayoutCell)] = Make_Lime__DefaultLayoutCell;
			makeCache[typeof(global::Lime.DistortionMesh)] = Make_Lime__DistortionMesh;
			makeCache[typeof(global::Lime.DistortionMeshPoint)] = Make_Lime__DistortionMeshPoint;
			makeCache[typeof(global::Lime.EmitterShapePoint)] = Make_Lime__EmitterShapePoint;
			makeCache[typeof(global::Lime.Font)] = Make_Lime__Font;
			makeCache[typeof(global::Lime.FontChar)] = Make_Lime__FontChar;
			makeCache[typeof(global::Lime.Frame)] = Make_Lime__Frame;
			makeCache[typeof(global::Lime.GradientComponent)] = Make_Lime__GradientComponent;
			makeCache[typeof(global::Lime.GradientControlPoint)] = Make_Lime__GradientControlPoint;
			makeCache[typeof(global::Lime.HBoxLayout)] = Make_Lime__HBoxLayout;
			makeCache[typeof(global::Lime.HSLComponent)] = Make_Lime__HSLComponent;
			makeCache[typeof(global::Lime.Image)] = Make_Lime__Image;
			makeCache[typeof(global::Lime.ImageCombiner)] = Make_Lime__ImageCombiner;
			makeCache[typeof(global::Lime.IntAnimator)] = Make_Lime__IntAnimator;
			makeCache[typeof(global::Lime.IntRectangle)] = Make_Lime__IntRectangle;
			makeCache[typeof(global::Lime.IntVector2)] = Make_Lime__IntVector2;
			makeCache[typeof(global::Lime.KerningPair)] = Make_Lime__KerningPair;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.Alignment>)] = Make_Lime__Keyframe_Alignment;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.Anchors>)] = Make_Lime__Keyframe_Anchors;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.AudioAction>)] = Make_Lime__Keyframe_AudioAction;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.Blending>)] = Make_Lime__Keyframe_Blending;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.ClipMethod>)] = Make_Lime__Keyframe_ClipMethod;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.Color4>)] = Make_Lime__Keyframe_Color4;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.EmissionType>)] = Make_Lime__Keyframe_EmissionType;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.EmitterShape>)] = Make_Lime__Keyframe_EmitterShape;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.HAlignment>)] = Make_Lime__Keyframe_HAlignment;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.ITexture>)] = Make_Lime__Keyframe_ITexture;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.LayoutDirection>)] = Make_Lime__Keyframe_LayoutDirection;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.Matrix44>)] = Make_Lime__Keyframe_Matrix44;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.MovieAction>)] = Make_Lime__Keyframe_MovieAction;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Camera3D>>)] = Make_Lime__Keyframe_NodeReference_Camera3D;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Node3D>>)] = Make_Lime__Keyframe_NodeReference_Node3D;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline>>)] = Make_Lime__Keyframe_NodeReference_Spline;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline3D>>)] = Make_Lime__Keyframe_NodeReference_Spline3D;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Widget>>)] = Make_Lime__Keyframe_NodeReference_Widget;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.NumericRange>)] = Make_Lime__Keyframe_NumericRange;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.ParticlesLinkage>)] = Make_Lime__Keyframe_ParticlesLinkage;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.Quaternion>)] = Make_Lime__Keyframe_Quaternion;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.RenderTarget>)] = Make_Lime__Keyframe_RenderTarget;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.SerializableFont>)] = Make_Lime__Keyframe_SerializableFont;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.SerializableSample>)] = Make_Lime__Keyframe_SerializableSample;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.ShaderId>)] = Make_Lime__Keyframe_ShaderId;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.TextOverflowMode>)] = Make_Lime__Keyframe_TextOverflowMode;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.Thickness>)] = Make_Lime__Keyframe_Thickness;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.VAlignment>)] = Make_Lime__Keyframe_VAlignment;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.Vector2>)] = Make_Lime__Keyframe_Vector2;
			makeCache[typeof(global::Lime.Keyframe<global::Lime.Vector3>)] = Make_Lime__Keyframe_Vector3;
			makeCache[typeof(global::Lime.Keyframe<global::System.Boolean>)] = Make_Lime__Keyframe_Boolean;
			makeCache[typeof(global::Lime.Keyframe<global::System.Int32>)] = Make_Lime__Keyframe_Int32;
			makeCache[typeof(global::Lime.Keyframe<global::System.Single>)] = Make_Lime__Keyframe_Single;
			makeCache[typeof(global::Lime.Keyframe<global::System.String>)] = Make_Lime__Keyframe_String;
			makeCache[typeof(global::Lime.LayoutCell)] = Make_Lime__LayoutCell;
			makeCache[typeof(global::Lime.LayoutConstraints)] = Make_Lime__LayoutConstraints;
			makeCache[typeof(global::Lime.LinearLayout)] = Make_Lime__LinearLayout;
			makeCache[typeof(global::Lime.Marker)] = Make_Lime__Marker;
			makeCache[typeof(global::Lime.MarkerBlending)] = Make_Lime__MarkerBlending;
			makeCache[typeof(global::Lime.Matrix32)] = Make_Lime__Matrix32;
			makeCache[typeof(global::Lime.Matrix44)] = Make_Lime__Matrix44;
			makeCache[typeof(global::Lime.Matrix44Animator)] = Make_Lime__Matrix44Animator;
			makeCache[typeof(global::Lime.Mesh<global::Lime.Mesh3D.Vertex>)] = Make_Lime__Mesh_Mesh3D__Vertex;
			makeCache[typeof(global::Lime.Mesh3D)] = Make_Lime__Mesh3D;
			makeCache[typeof(global::Lime.Mesh3D.BlendIndices)] = Make_Lime__Mesh3D__BlendIndices;
			makeCache[typeof(global::Lime.Mesh3D.BlendWeights)] = Make_Lime__Mesh3D__BlendWeights;
			makeCache[typeof(global::Lime.Mesh3D.Vertex)] = Make_Lime__Mesh3D__Vertex;
			makeCache[typeof(global::Lime.Model3D)] = Make_Lime__Model3D;
			makeCache[typeof(global::Lime.Model3DAttachment.MaterialRemap)] = Make_Lime__Model3DAttachment__MaterialRemap;
			makeCache[typeof(global::Lime.Model3DAttachmentParser.MeshOptionFormat)] = Make_Lime__Model3DAttachmentParser__MeshOptionFormat;
			makeCache[typeof(global::Lime.Model3DAttachmentParser.ModelAnimationFormat)] = Make_Lime__Model3DAttachmentParser__ModelAnimationFormat;
			makeCache[typeof(global::Lime.Model3DAttachmentParser.ModelAttachmentFormat)] = Make_Lime__Model3DAttachmentParser__ModelAttachmentFormat;
			makeCache[typeof(global::Lime.Model3DAttachmentParser.ModelComponentsFormat)] = Make_Lime__Model3DAttachmentParser__ModelComponentsFormat;
			makeCache[typeof(global::Lime.Model3DAttachmentParser.ModelMarkerFormat)] = Make_Lime__Model3DAttachmentParser__ModelMarkerFormat;
			makeCache[typeof(global::Lime.Model3DAttachmentParser.UVAnimationFormat)] = Make_Lime__Model3DAttachmentParser__UVAnimationFormat;
			makeCache[typeof(global::Lime.Movie)] = Make_Lime__Movie;
			makeCache[typeof(global::Lime.NineGrid)] = Make_Lime__NineGrid;
			makeCache[typeof(global::Lime.Node3D)] = Make_Lime__Node3D;
			makeCache[typeof(global::Lime.NodeReference<global::Lime.Camera3D>)] = Make_Lime__NodeReference_Camera3D;
			makeCache[typeof(global::Lime.NodeReference<global::Lime.Spline>)] = Make_Lime__NodeReference_Spline;
			makeCache[typeof(global::Lime.NodeReference<global::Lime.Widget>)] = Make_Lime__NodeReference_Widget;
			makeCache[typeof(global::Lime.NumericAnimator)] = Make_Lime__NumericAnimator;
			makeCache[typeof(global::Lime.NumericRange)] = Make_Lime__NumericRange;
			makeCache[typeof(global::Lime.ParticleEmitter)] = Make_Lime__ParticleEmitter;
			makeCache[typeof(global::Lime.ParticleModifier)] = Make_Lime__ParticleModifier;
			makeCache[typeof(global::Lime.ParticlesMagnet)] = Make_Lime__ParticlesMagnet;
			makeCache[typeof(global::Lime.Plane)] = Make_Lime__Plane;
			makeCache[typeof(global::Lime.PointObject)] = Make_Lime__PointObject;
			makeCache[typeof(global::Lime.Polyline)] = Make_Lime__Polyline;
			makeCache[typeof(global::Lime.PolylinePoint)] = Make_Lime__PolylinePoint;
			makeCache[typeof(global::Lime.PostProcessingComponent)] = Make_Lime__PostProcessingComponent;
			makeCache[typeof(global::Lime.Quaternion)] = Make_Lime__Quaternion;
			makeCache[typeof(global::Lime.QuaternionAnimator)] = Make_Lime__QuaternionAnimator;
			makeCache[typeof(global::Lime.Ray)] = Make_Lime__Ray;
			makeCache[typeof(global::Lime.Rectangle)] = Make_Lime__Rectangle;
			makeCache[typeof(global::Lime.RenderOptimizer.ContentBox)] = Make_Lime_RenderOptimizer__ContentBox;
			makeCache[typeof(global::Lime.RenderOptimizer.ContentPlane)] = Make_Lime_RenderOptimizer__ContentPlane;
			makeCache[typeof(global::Lime.RenderOptimizer.ContentRectangle)] = Make_Lime_RenderOptimizer__ContentRectangle;
			makeCache[typeof(global::Lime.RenderOptimizer.ContentSizeComponent)] = Make_Lime_RenderOptimizer__ContentSizeComponent;
			makeCache[typeof(global::Lime.RichText)] = Make_Lime__RichText;
			makeCache[typeof(global::Lime.SerializableFont)] = Make_Lime__SerializableFont;
			makeCache[typeof(global::Lime.SerializableSample)] = Make_Lime__SerializableSample;
			makeCache[typeof(global::Lime.SerializableTexture)] = Make_Lime__SerializableTexture;
			makeCache[typeof(global::Lime.ShadowParams)] = Make_Lime__ShadowParams;
			makeCache[typeof(global::Lime.SignedDistanceFieldComponent)] = Make_Lime__SignedDistanceFieldComponent;
			makeCache[typeof(global::Lime.SimpleText)] = Make_Lime__SimpleText;
			makeCache[typeof(global::Lime.Size)] = Make_Lime__Size;
			makeCache[typeof(global::Lime.SkinningWeights)] = Make_Lime__SkinningWeights;
			makeCache[typeof(global::Lime.Slider)] = Make_Lime__Slider;
			makeCache[typeof(global::Lime.Spline)] = Make_Lime__Spline;
			makeCache[typeof(global::Lime.Spline3D)] = Make_Lime__Spline3D;
			makeCache[typeof(global::Lime.SplineGear)] = Make_Lime__SplineGear;
			makeCache[typeof(global::Lime.SplineGear3D)] = Make_Lime__SplineGear3D;
			makeCache[typeof(global::Lime.SplinePoint)] = Make_Lime__SplinePoint;
			makeCache[typeof(global::Lime.SplinePoint3D)] = Make_Lime__SplinePoint3D;
			makeCache[typeof(global::Lime.StackLayout)] = Make_Lime__StackLayout;
			makeCache[typeof(global::Lime.Submesh3D)] = Make_Lime__Submesh3D;
			makeCache[typeof(global::Lime.TableLayout)] = Make_Lime__TableLayout;
			makeCache[typeof(global::Lime.TextStyle)] = Make_Lime__TextStyle;
			makeCache[typeof(global::Lime.TextureAtlasElement.Params)] = Make_Lime__TextureAtlasElement__Params;
			makeCache[typeof(global::Lime.TextureParams)] = Make_Lime__TextureParams;
			makeCache[typeof(global::Lime.Thickness)] = Make_Lime__Thickness;
			makeCache[typeof(global::Lime.ThicknessAnimator)] = Make_Lime__ThicknessAnimator;
			makeCache[typeof(global::Lime.TiledImage)] = Make_Lime__TiledImage;
			makeCache[typeof(global::Lime.VBoxLayout)] = Make_Lime__VBoxLayout;
			makeCache[typeof(global::Lime.Vector2)] = Make_Lime__Vector2;
			makeCache[typeof(global::Lime.Vector2Animator)] = Make_Lime__Vector2Animator;
			makeCache[typeof(global::Lime.Vector3)] = Make_Lime__Vector3;
			makeCache[typeof(global::Lime.Vector3Animator)] = Make_Lime__Vector3Animator;
			makeCache[typeof(global::Lime.Vector4)] = Make_Lime__Vector4;
			makeCache[typeof(global::Lime.VideoPlayer)] = Make_Lime__VideoPlayer;
			makeCache[typeof(global::Lime.Viewport3D)] = Make_Lime__Viewport3D;
			makeCache[typeof(global::Lime.Widget)] = Make_Lime__Widget;
			makeCache[typeof(global::Lime.WidgetAdapter3D)] = Make_Lime__WidgetAdapter3D;
		}
	}
}
