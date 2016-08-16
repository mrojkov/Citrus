using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Yuzu;
using Yuzu.Json;


namespace GeneratedDeserializersJSON.Lime
{
	class Font_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Font_JsonDeserializer Instance = new Font_JsonDeserializer();

		public Font_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Font>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Font(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Font)obj;
			if ("About" == name) {
				result.About = RequireString();
				name = GetNextName(false);
			}
			if ("CharCollection" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp1 = GeneratedDeserializersJSON.Lime.FontChar_JsonDeserializer.Instance.FromReaderTyped<global::Lime.FontChar>(Reader);
						((global::System.Collections.Generic.ICollection<global::Lime.FontChar>)result.CharCollection).Add(tmp1);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("Textures" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.ITexture_JsonDeserializer.Instance.FromReaderInterface<global::Lime.ITexture>(Reader);
						result.Textures.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class SerializableSample_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SerializableSample_JsonDeserializer Instance = new SerializableSample_JsonDeserializer();

		public SerializableSample_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.SerializableSample>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.SerializableSample(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.SerializableSample)obj;
			if ("SerializationPath" == name) {
				result.SerializationPath = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class KerningPair_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new KerningPair_JsonDeserializer Instance = new KerningPair_JsonDeserializer();

		public KerningPair_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.KerningPair>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.KerningPair(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.KerningPair)obj;
			if ("Char" == name) {
				result.Char = RequireChar();
				name = GetNextName(false);
			}
			if ("Kerning" == name) {
				result.Kerning = RequireSingle();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class FontChar_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new FontChar_JsonDeserializer Instance = new FontChar_JsonDeserializer();

		public FontChar_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.FontChar>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.FontChar(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.FontChar)obj;
			if ("ACWidths" == name) {
				result.ACWidths = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Char" == name) {
				result.Char = RequireChar();
				name = GetNextName(false);
			}
			if ("Height" == name) {
				result.Height = RequireSingle();
				name = GetNextName(false);
			}
			if ("KerningPairs" == name) {
				result.KerningPairs = RequireOrNull('[') ? null : new global::System.Collections.Generic.List<global::Lime.KerningPair>();
				if (result.KerningPairs != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.KerningPair_JsonDeserializer.Instance.FromReaderTyped<global::Lime.KerningPair>(Reader);
							result.KerningPairs.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TextureIndex" == name) {
				result.TextureIndex = RequireInt();
				name = GetNextName(false);
			}
			if ("UV0" == name) {
				result.UV0 = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("UV1" == name) {
				result.UV1 = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("VerticalOffset" == name) {
				result.VerticalOffset = RequireInt();
				name = GetNextName(false);
			}
			if ("Width" == name) {
				result.Width = RequireSingle();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class ITexture_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new ITexture_JsonDeserializer Instance = new ITexture_JsonDeserializer();

		public ITexture_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderInterface<global::Lime.ITexture>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return null;
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.ITexture)obj;
			if ("SerializationPath" == name) {
				result.SerializationPath = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class SerializableFont_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SerializableFont_JsonDeserializer Instance = new SerializableFont_JsonDeserializer();

		public SerializableFont_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.SerializableFont>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.SerializableFont(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.SerializableFont)obj;
			if ("Name" == name) {
				result.Name = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class BlendIndices_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new BlendIndices_JsonDeserializer Instance = new BlendIndices_JsonDeserializer();

		public BlendIndices_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.BlendIndices>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.BlendIndices(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.BlendIndices)obj;
			if ("Index0" == name) {
				result.Index0 = checked((byte)RequireUInt());
				name = GetNextName(false);
			}
			if ("Index1" == name) {
				result.Index1 = checked((byte)RequireUInt());
				name = GetNextName(false);
			}
			if ("Index2" == name) {
				result.Index2 = checked((byte)RequireUInt());
				name = GetNextName(false);
			}
			if ("Index3" == name) {
				result.Index3 = checked((byte)RequireUInt());
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}

		protected override object ReadFieldsCompact(object obj)
		{
			var result = (global::Lime.BlendIndices)obj;
			result.Index0 = checked((byte)RequireUInt());
			Require(',');
			result.Index1 = checked((byte)RequireUInt());
			Require(',');
			result.Index2 = checked((byte)RequireUInt());
			Require(',');
			result.Index3 = checked((byte)RequireUInt());
			Require(']');
			return result;
		}
	}

	class BlendWeights_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new BlendWeights_JsonDeserializer Instance = new BlendWeights_JsonDeserializer();

		public BlendWeights_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.BlendWeights>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.BlendWeights(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.BlendWeights)obj;
			if ("Weight0" == name) {
				result.Weight0 = RequireSingle();
				name = GetNextName(false);
			}
			if ("Weight1" == name) {
				result.Weight1 = RequireSingle();
				name = GetNextName(false);
			}
			if ("Weight2" == name) {
				result.Weight2 = RequireSingle();
				name = GetNextName(false);
			}
			if ("Weight3" == name) {
				result.Weight3 = RequireSingle();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}

		protected override object ReadFieldsCompact(object obj)
		{
			var result = (global::Lime.BlendWeights)obj;
			result.Weight0 = RequireSingle();
			Require(',');
			result.Weight1 = RequireSingle();
			Require(',');
			result.Weight2 = RequireSingle();
			Require(',');
			result.Weight3 = RequireSingle();
			Require(']');
			return result;
		}
	}

	class GeometryBuffer_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new GeometryBuffer_JsonDeserializer Instance = new GeometryBuffer_JsonDeserializer();

		public GeometryBuffer_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.GeometryBuffer>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.GeometryBuffer(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.GeometryBuffer)obj;
			if ("BlendIndices" == name) {
				result.BlendIndices = RequireOrNull('[') ? null : new global::Lime.BlendIndices[0];
				if (result.BlendIndices != null) {
					if (SkipSpacesCarefully() != ']') {
						var tmp1 = new global::Lime.BlendIndices[RequireUInt()];
						for(int tmp2 = 0; tmp2 < tmp1.Length; ++tmp2) {
							Require(',');
							tmp1[tmp2] = GeneratedDeserializersJSON.Lime.BlendIndices_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BlendIndices>(Reader);
						}
						result.BlendIndices = tmp1;
					}
					Require(']');
				}
				name = GetNextName(false);
			}
			if ("BlendWeights" == name) {
				result.BlendWeights = RequireOrNull('[') ? null : new global::Lime.BlendWeights[0];
				if (result.BlendWeights != null) {
					if (SkipSpacesCarefully() != ']') {
						var tmp3 = new global::Lime.BlendWeights[RequireUInt()];
						for(int tmp4 = 0; tmp4 < tmp3.Length; ++tmp4) {
							Require(',');
							tmp3[tmp4] = GeneratedDeserializersJSON.Lime.BlendWeights_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BlendWeights>(Reader);
						}
						result.BlendWeights = tmp3;
					}
					Require(']');
				}
				name = GetNextName(false);
			}
			if ("Colors" == name) {
				result.Colors = RequireOrNull('[') ? null : new global::Lime.Color4[0];
				if (result.Colors != null) {
					if (SkipSpacesCarefully() != ']') {
						var tmp5 = new global::Lime.Color4[RequireUInt()];
						for(int tmp6 = 0; tmp6 < tmp5.Length; ++tmp6) {
							Require(',');
							tmp5[tmp6] = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
						}
						result.Colors = tmp5;
					}
					Require(']');
				}
				name = GetNextName(false);
			}
			if ("Indices" == name) {
				result.Indices = RequireOrNull('[') ? null : new global::System.UInt16[0];
				if (result.Indices != null) {
					if (SkipSpacesCarefully() != ']') {
						var tmp7 = new global::System.UInt16[RequireUInt()];
						for(int tmp8 = 0; tmp8 < tmp7.Length; ++tmp8) {
							Require(',');
							tmp7[tmp8] = checked((ushort)RequireUInt());
						}
						result.Indices = tmp7;
					}
					Require(']');
				}
				name = GetNextName(false);
			}
			if ("UV1" == name) {
				result.UV1 = RequireOrNull('[') ? null : new global::Lime.Vector2[0];
				if (result.UV1 != null) {
					if (SkipSpacesCarefully() != ']') {
						var tmp9 = new global::Lime.Vector2[RequireUInt()];
						for(int tmp10 = 0; tmp10 < tmp9.Length; ++tmp10) {
							Require(',');
							tmp9[tmp10] = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
						}
						result.UV1 = tmp9;
					}
					Require(']');
				}
				name = GetNextName(false);
			}
			if ("UV2" == name) {
				result.UV2 = RequireOrNull('[') ? null : new global::Lime.Vector2[0];
				if (result.UV2 != null) {
					if (SkipSpacesCarefully() != ']') {
						var tmp11 = new global::Lime.Vector2[RequireUInt()];
						for(int tmp12 = 0; tmp12 < tmp11.Length; ++tmp12) {
							Require(',');
							tmp11[tmp12] = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
						}
						result.UV2 = tmp11;
					}
					Require(']');
				}
				name = GetNextName(false);
			}
			if ("UV3" == name) {
				result.UV3 = RequireOrNull('[') ? null : new global::Lime.Vector2[0];
				if (result.UV3 != null) {
					if (SkipSpacesCarefully() != ']') {
						var tmp13 = new global::Lime.Vector2[RequireUInt()];
						for(int tmp14 = 0; tmp14 < tmp13.Length; ++tmp14) {
							Require(',');
							tmp13[tmp14] = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
						}
						result.UV3 = tmp13;
					}
					Require(']');
				}
				name = GetNextName(false);
			}
			if ("UV4" == name) {
				result.UV4 = RequireOrNull('[') ? null : new global::Lime.Vector2[0];
				if (result.UV4 != null) {
					if (SkipSpacesCarefully() != ']') {
						var tmp15 = new global::Lime.Vector2[RequireUInt()];
						for(int tmp16 = 0; tmp16 < tmp15.Length; ++tmp16) {
							Require(',');
							tmp15[tmp16] = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
						}
						result.UV4 = tmp15;
					}
					Require(']');
				}
				name = GetNextName(false);
			}
			if ("Vertices" == name) {
				result.Vertices = RequireOrNull('[') ? null : new global::Lime.Vector3[0];
				if (result.Vertices != null) {
					if (SkipSpacesCarefully() != ']') {
						var tmp17 = new global::Lime.Vector3[RequireUInt()];
						for(int tmp18 = 0; tmp18 < tmp17.Length; ++tmp18) {
							Require(',');
							tmp17[tmp18] = GeneratedDeserializersJSON.Lime.Vector3_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector3>(Reader);
						}
						result.Vertices = tmp17;
					}
					Require(']');
				}
				name = GetNextName(false);
			}
			Require('}');
			result.AfterDeserialization();
			return result;
		}
	}

	class TextureAtlasElement__Params_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new TextureAtlasElement__Params_JsonDeserializer Instance = new TextureAtlasElement__Params_JsonDeserializer();

		public TextureAtlasElement__Params_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.TextureAtlasElement.Params>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.TextureAtlasElement.Params(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.TextureAtlasElement.Params)obj;
			if ("AtlasPath" == name) {
				result.AtlasPath = RequireString();
				name = GetNextName(false);
			}
			if ("AtlasRect" == name) {
				result.AtlasRect = GeneratedDeserializersJSON.Lime.IntRectangle_JsonDeserializer.Instance.FromReaderTyped<global::Lime.IntRectangle>(Reader);
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class BitSet32_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new BitSet32_JsonDeserializer Instance = new BitSet32_JsonDeserializer();

		public BitSet32_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.BitSet32>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.BitSet32(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.BitSet32)obj;
			if ("Value" == name) {
				result.Value = RequireUInt();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}

		protected override object ReadFieldsCompact(object obj)
		{
			var result = (global::Lime.BitSet32)obj;
			result.Value = RequireUInt();
			Require(']');
			return result;
		}
	}

	class BoundingSphere_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new BoundingSphere_JsonDeserializer Instance = new BoundingSphere_JsonDeserializer();

		public BoundingSphere_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.BoundingSphere>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.BoundingSphere(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.BoundingSphere)obj;
			if ("Center" == name) {
				result.Center = GeneratedDeserializersJSON.Lime.Vector3_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector3>(Reader);
				name = GetNextName(false);
			}
			if ("Radius" == name) {
				result.Radius = RequireSingle();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Color4_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Color4_JsonDeserializer Instance = new Color4_JsonDeserializer();

		public Color4_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Color4>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Color4(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Color4)obj;
			if ("ABGR" == name) {
				result.ABGR = RequireUInt();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}

		protected override object ReadFieldsCompact(object obj)
		{
			var result = (global::Lime.Color4)obj;
			result.ABGR = RequireUInt();
			Require(']');
			return result;
		}
	}

	class IntRectangle_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new IntRectangle_JsonDeserializer Instance = new IntRectangle_JsonDeserializer();

		public IntRectangle_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.IntRectangle>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.IntRectangle(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.IntRectangle)obj;
			if ("A" == name) {
				result.A = GeneratedDeserializersJSON.Lime.IntVector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.IntVector2>(Reader);
				name = GetNextName(false);
			}
			if ("B" == name) {
				result.B = GeneratedDeserializersJSON.Lime.IntVector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.IntVector2>(Reader);
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}

		protected override object ReadFieldsCompact(object obj)
		{
			var result = (global::Lime.IntRectangle)obj;
			result.A = GeneratedDeserializersJSON.Lime.IntVector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.IntVector2>(Reader);
			Require(',');
			result.B = GeneratedDeserializersJSON.Lime.IntVector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.IntVector2>(Reader);
			Require(']');
			return result;
		}
	}

	class IntVector2_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new IntVector2_JsonDeserializer Instance = new IntVector2_JsonDeserializer();

		public IntVector2_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.IntVector2>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.IntVector2(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.IntVector2)obj;
			if ("X" == name) {
				result.X = RequireInt();
				name = GetNextName(false);
			}
			if ("Y" == name) {
				result.Y = RequireInt();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}

		protected override object ReadFieldsCompact(object obj)
		{
			var result = (global::Lime.IntVector2)obj;
			result.X = RequireInt();
			Require(',');
			result.Y = RequireInt();
			Require(']');
			return result;
		}
	}

	class Matrix32_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Matrix32_JsonDeserializer Instance = new Matrix32_JsonDeserializer();

		public Matrix32_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Matrix32>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Matrix32(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Matrix32)obj;
			if ("T" == name) {
				result.T = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("U" == name) {
				result.U = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("V" == name) {
				result.V = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}

		protected override object ReadFieldsCompact(object obj)
		{
			var result = (global::Lime.Matrix32)obj;
			result.T = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
			Require(',');
			result.U = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
			Require(',');
			result.V = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
			Require(']');
			return result;
		}
	}

	class Matrix44_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Matrix44_JsonDeserializer Instance = new Matrix44_JsonDeserializer();

		public Matrix44_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Matrix44>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Matrix44(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Matrix44)obj;
			if ("M11" == name) {
				result.M11 = RequireSingle();
				name = GetNextName(false);
			}
			if ("M12" == name) {
				result.M12 = RequireSingle();
				name = GetNextName(false);
			}
			if ("M13" == name) {
				result.M13 = RequireSingle();
				name = GetNextName(false);
			}
			if ("M14" == name) {
				result.M14 = RequireSingle();
				name = GetNextName(false);
			}
			if ("M21" == name) {
				result.M21 = RequireSingle();
				name = GetNextName(false);
			}
			if ("M22" == name) {
				result.M22 = RequireSingle();
				name = GetNextName(false);
			}
			if ("M23" == name) {
				result.M23 = RequireSingle();
				name = GetNextName(false);
			}
			if ("M24" == name) {
				result.M24 = RequireSingle();
				name = GetNextName(false);
			}
			if ("M31" == name) {
				result.M31 = RequireSingle();
				name = GetNextName(false);
			}
			if ("M32" == name) {
				result.M32 = RequireSingle();
				name = GetNextName(false);
			}
			if ("M33" == name) {
				result.M33 = RequireSingle();
				name = GetNextName(false);
			}
			if ("M34" == name) {
				result.M34 = RequireSingle();
				name = GetNextName(false);
			}
			if ("M41" == name) {
				result.M41 = RequireSingle();
				name = GetNextName(false);
			}
			if ("M42" == name) {
				result.M42 = RequireSingle();
				name = GetNextName(false);
			}
			if ("M43" == name) {
				result.M43 = RequireSingle();
				name = GetNextName(false);
			}
			if ("M44" == name) {
				result.M44 = RequireSingle();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}

		protected override object ReadFieldsCompact(object obj)
		{
			var result = (global::Lime.Matrix44)obj;
			result.M11 = RequireSingle();
			Require(',');
			result.M12 = RequireSingle();
			Require(',');
			result.M13 = RequireSingle();
			Require(',');
			result.M14 = RequireSingle();
			Require(',');
			result.M21 = RequireSingle();
			Require(',');
			result.M22 = RequireSingle();
			Require(',');
			result.M23 = RequireSingle();
			Require(',');
			result.M24 = RequireSingle();
			Require(',');
			result.M31 = RequireSingle();
			Require(',');
			result.M32 = RequireSingle();
			Require(',');
			result.M33 = RequireSingle();
			Require(',');
			result.M34 = RequireSingle();
			Require(',');
			result.M41 = RequireSingle();
			Require(',');
			result.M42 = RequireSingle();
			Require(',');
			result.M43 = RequireSingle();
			Require(',');
			result.M44 = RequireSingle();
			Require(']');
			return result;
		}
	}

	class NumericRange_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new NumericRange_JsonDeserializer Instance = new NumericRange_JsonDeserializer();

		public NumericRange_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.NumericRange>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.NumericRange(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.NumericRange)obj;
			if ("Dispersion" == name) {
				result.Dispersion = RequireSingle();
				name = GetNextName(false);
			}
			if ("Median" == name) {
				result.Median = RequireSingle();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Plane_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Plane_JsonDeserializer Instance = new Plane_JsonDeserializer();

		public Plane_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Plane>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Plane(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Plane)obj;
			if ("D" == name) {
				result.D = RequireSingle();
				name = GetNextName(false);
			}
			if ("Normal" == name) {
				result.Normal = GeneratedDeserializersJSON.Lime.Vector3_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector3>(Reader);
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Quaternion_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Quaternion_JsonDeserializer Instance = new Quaternion_JsonDeserializer();

		public Quaternion_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Quaternion>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Quaternion(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Quaternion)obj;
			if ("W" == name) {
				result.W = RequireSingle();
				name = GetNextName(false);
			}
			if ("X" == name) {
				result.X = RequireSingle();
				name = GetNextName(false);
			}
			if ("Y" == name) {
				result.Y = RequireSingle();
				name = GetNextName(false);
			}
			if ("Z" == name) {
				result.Z = RequireSingle();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}

		protected override object ReadFieldsCompact(object obj)
		{
			var result = (global::Lime.Quaternion)obj;
			result.W = RequireSingle();
			Require(',');
			result.X = RequireSingle();
			Require(',');
			result.Y = RequireSingle();
			Require(',');
			result.Z = RequireSingle();
			Require(']');
			return result;
		}
	}

	class Ray_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Ray_JsonDeserializer Instance = new Ray_JsonDeserializer();

		public Ray_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Ray>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Ray(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Ray)obj;
			if ("Direction" == name) {
				result.Direction = GeneratedDeserializersJSON.Lime.Vector3_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector3>(Reader);
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector3_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector3>(Reader);
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Rectangle_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Rectangle_JsonDeserializer Instance = new Rectangle_JsonDeserializer();

		public Rectangle_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Rectangle>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Rectangle(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Rectangle)obj;
			if ("A" == name) {
				result.A = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("B" == name) {
				result.B = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}

		protected override object ReadFieldsCompact(object obj)
		{
			var result = (global::Lime.Rectangle)obj;
			result.A = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
			Require(',');
			result.B = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
			Require(']');
			return result;
		}
	}

	class Size_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Size_JsonDeserializer Instance = new Size_JsonDeserializer();

		public Size_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Size>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Size(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Size)obj;
			if ("Height" == name) {
				result.Height = RequireInt();
				name = GetNextName(false);
			}
			if ("Width" == name) {
				result.Width = RequireInt();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}

		protected override object ReadFieldsCompact(object obj)
		{
			var result = (global::Lime.Size)obj;
			result.Height = RequireInt();
			Require(',');
			result.Width = RequireInt();
			Require(']');
			return result;
		}
	}

	class Vector2_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Vector2_JsonDeserializer Instance = new Vector2_JsonDeserializer();

		public Vector2_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Vector2>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Vector2(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Vector2)obj;
			if ("X" == name) {
				result.X = RequireSingle();
				name = GetNextName(false);
			}
			if ("Y" == name) {
				result.Y = RequireSingle();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}

		protected override object ReadFieldsCompact(object obj)
		{
			var result = (global::Lime.Vector2)obj;
			result.X = RequireSingle();
			Require(',');
			result.Y = RequireSingle();
			Require(']');
			return result;
		}
	}

	class Vector3_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Vector3_JsonDeserializer Instance = new Vector3_JsonDeserializer();

		public Vector3_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Vector3>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Vector3(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Vector3)obj;
			if ("X" == name) {
				result.X = RequireSingle();
				name = GetNextName(false);
			}
			if ("Y" == name) {
				result.Y = RequireSingle();
				name = GetNextName(false);
			}
			if ("Z" == name) {
				result.Z = RequireSingle();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}

		protected override object ReadFieldsCompact(object obj)
		{
			var result = (global::Lime.Vector3)obj;
			result.X = RequireSingle();
			Require(',');
			result.Y = RequireSingle();
			Require(',');
			result.Z = RequireSingle();
			Require(']');
			return result;
		}
	}

	class Vector4_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Vector4_JsonDeserializer Instance = new Vector4_JsonDeserializer();

		public Vector4_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Vector4>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Vector4(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Vector4)obj;
			if ("W" == name) {
				result.W = RequireSingle();
				name = GetNextName(false);
			}
			if ("X" == name) {
				result.X = RequireSingle();
				name = GetNextName(false);
			}
			if ("Y" == name) {
				result.Y = RequireSingle();
				name = GetNextName(false);
			}
			if ("Z" == name) {
				result.Z = RequireSingle();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}

		protected override object ReadFieldsCompact(object obj)
		{
			var result = (global::Lime.Vector4)obj;
			result.W = RequireSingle();
			Require(',');
			result.X = RequireSingle();
			Require(',');
			result.Y = RequireSingle();
			Require(',');
			result.Z = RequireSingle();
			Require(']');
			return result;
		}
	}

	class Camera3D_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Camera3D_JsonDeserializer Instance = new Camera3D_JsonDeserializer();

		public Camera3D_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Camera3D>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Camera3D(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Camera3D)obj;
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("AspectRatio" == name) {
				result.AspectRatio = RequireSingle();
				name = GetNextName(false);
			}
			if ("Color" == name) {
				result.Color = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("FarClipPlane" == name) {
				result.FarClipPlane = RequireSingle();
				name = GetNextName(false);
			}
			if ("FieldOfView" == name) {
				result.FieldOfView = RequireSingle();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("NearClipPlane" == name) {
				result.NearClipPlane = RequireSingle();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector3_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector3>(Reader);
				name = GetNextName(false);
			}
			if ("Rotation" == name) {
				result.Rotation = GeneratedDeserializersJSON.Lime.Quaternion_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Quaternion>(Reader);
				name = GetNextName(false);
			}
			if ("Scale" == name) {
				result.Scale = GeneratedDeserializersJSON.Lime.Vector3_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector3>(Reader);
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("Visible" == name) {
				result.Visible = RequireBool();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Material_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Material_JsonDeserializer Instance = new Material_JsonDeserializer();

		public Material_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Material>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Material(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Material)obj;
			if ("DiffuseColor" == name) {
				result.DiffuseColor = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("DiffuseTexture" == name) {
				result.DiffuseTexture = GeneratedDeserializersJSON.Lime.ITexture_JsonDeserializer.Instance.FromReaderInterface<global::Lime.ITexture>(Reader);
				name = GetNextName(false);
			}
			if ("EmissiveColor" == name) {
				result.EmissiveColor = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("FogColor" == name) {
				result.FogColor = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("FogDensity" == name) {
				result.FogDensity = RequireSingle();
				name = GetNextName(false);
			}
			if ("FogEnd" == name) {
				result.FogEnd = RequireSingle();
				name = GetNextName(false);
			}
			if ("FogMode" == name) {
				result.FogMode = (global::Lime.FogMode)RequireInt();
				name = GetNextName(false);
			}
			if ("FogStart" == name) {
				result.FogStart = RequireSingle();
				name = GetNextName(false);
			}
			if ("Name" == name) {
				result.Name = RequireString();
				name = GetNextName(false);
			}
			if ("Opacity" == name) {
				result.Opacity = RequireSingle();
				name = GetNextName(false);
			}
			if ("OpacityTexture" == name) {
				result.OpacityTexture = GeneratedDeserializersJSON.Lime.ITexture_JsonDeserializer.Instance.FromReaderInterface<global::Lime.ITexture>(Reader);
				name = GetNextName(false);
			}
			if ("SpecularColor" == name) {
				result.SpecularColor = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("SpecularPower" == name) {
				result.SpecularPower = RequireSingle();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Mesh3D_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Mesh3D_JsonDeserializer Instance = new Mesh3D_JsonDeserializer();

		public Mesh3D_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Mesh3D>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Mesh3D(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Mesh3D)obj;
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("BoneBindPoseInverses" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp3 = GeneratedDeserializersJSON.Lime.Matrix44_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Matrix44>(Reader);
						result.BoneBindPoseInverses.Add(tmp3);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("Bones" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp4 = GeneratedDeserializersJSON.Lime.Node3D_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node3D>(Reader);
						result.Bones.Add(tmp4);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("BoundingSphere" == name) {
				result.BoundingSphere = GeneratedDeserializersJSON.Lime.BoundingSphere_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoundingSphere>(Reader);
				name = GetNextName(false);
			}
			if ("Color" == name) {
				result.Color = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp5 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp5);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector3_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector3>(Reader);
				name = GetNextName(false);
			}
			if ("Rotation" == name) {
				result.Rotation = GeneratedDeserializersJSON.Lime.Quaternion_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Quaternion>(Reader);
				name = GetNextName(false);
			}
			if ("Scale" == name) {
				result.Scale = GeneratedDeserializersJSON.Lime.Vector3_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector3>(Reader);
				name = GetNextName(false);
			}
			if ("Submeshes" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp6 = GeneratedDeserializersJSON.Lime.Submesh3D_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Submesh3D>(Reader);
						result.Submeshes.Add(tmp6);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("Visible" == name) {
				result.Visible = RequireBool();
				name = GetNextName(false);
			}
			Require('}');
			result.AfterDeserialization();
			return result;
		}
	}

	class Submesh3D_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Submesh3D_JsonDeserializer Instance = new Submesh3D_JsonDeserializer();

		public Submesh3D_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Submesh3D>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Submesh3D(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Submesh3D)obj;
			if ("BoneIndices" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp1 = RequireInt();
						result.BoneIndices.Add(tmp1);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("Geometry" == name) {
				GeneratedDeserializersJSON.Lime.GeometryBuffer_JsonDeserializer.Instance.FromReader(result.Geometry, Reader);
				name = GetNextName(false);
			}
			if ("Material" == name) {
				result.Material = GeneratedDeserializersJSON.Lime.Material_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Material>(Reader);
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Node3D_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Node3D_JsonDeserializer Instance = new Node3D_JsonDeserializer();

		public Node3D_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Node3D>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Node3D(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Node3D)obj;
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("Color" == name) {
				result.Color = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector3_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector3>(Reader);
				name = GetNextName(false);
			}
			if ("Rotation" == name) {
				result.Rotation = GeneratedDeserializersJSON.Lime.Quaternion_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Quaternion>(Reader);
				name = GetNextName(false);
			}
			if ("Scale" == name) {
				result.Scale = GeneratedDeserializersJSON.Lime.Vector3_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector3>(Reader);
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("Visible" == name) {
				result.Visible = RequireBool();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Spline3D_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Spline3D_JsonDeserializer Instance = new Spline3D_JsonDeserializer();

		public Spline3D_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Spline3D>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Spline3D(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Spline3D)obj;
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("Closed" == name) {
				result.Closed = RequireBool();
				name = GetNextName(false);
			}
			if ("Color" == name) {
				result.Color = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Points" == name) {
				result.Points = RequireOrNull('[') ? null : new global::System.Collections.Generic.List<global::Lime.Spline3D.Point>();
				if (result.Points != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp4 = GeneratedDeserializersJSON.Lime.Spline3D__Point_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Spline3D.Point>(Reader);
							result.Points.Add(tmp4);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector3_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector3>(Reader);
				name = GetNextName(false);
			}
			if ("Rotation" == name) {
				result.Rotation = GeneratedDeserializersJSON.Lime.Quaternion_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Quaternion>(Reader);
				name = GetNextName(false);
			}
			if ("Scale" == name) {
				result.Scale = GeneratedDeserializersJSON.Lime.Vector3_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector3>(Reader);
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("Visible" == name) {
				result.Visible = RequireBool();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Spline3D__Point_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Spline3D__Point_JsonDeserializer Instance = new Spline3D__Point_JsonDeserializer();

		public Spline3D__Point_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Spline3D.Point>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Spline3D.Point(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Spline3D.Point)obj;
			if ("InterpolationMode" == name) {
				result.InterpolationMode = (global::Lime.Spline3D.InterpolationMode)RequireInt();
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector3_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector3>(Reader);
				name = GetNextName(false);
			}
			if ("TangentA" == name) {
				result.TangentA = GeneratedDeserializersJSON.Lime.Vector3_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector3>(Reader);
				name = GetNextName(false);
			}
			if ("TangentB" == name) {
				result.TangentB = GeneratedDeserializersJSON.Lime.Vector3_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector3>(Reader);
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Viewport3D_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Viewport3D_JsonDeserializer Instance = new Viewport3D_JsonDeserializer();

		public Viewport3D_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Viewport3D>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Viewport3D(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Viewport3D)obj;
			if ("Anchors" == name) {
				result.Anchors = (global::Lime.Anchors)RequireInt();
				name = GetNextName(false);
			}
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("Blending" == name) {
				result.Blending = (global::Lime.Blending)RequireInt();
				name = GetNextName(false);
			}
			if ("BoneArray" == name) {
				result.BoneArray = GeneratedDeserializersJSON.Lime.BoneArray_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoneArray>(Reader);
				name = GetNextName(false);
			}
			if ("Color" == name) {
				result.Color = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("Frame" == name) {
				result.Frame = RequireSingle();
				name = GetNextName(false);
			}
			if ("HitTestMethod" == name) {
				result.HitTestMethod = (global::Lime.HitTestMethod)RequireInt();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Pivot" == name) {
				result.Pivot = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Rotation" == name) {
				result.Rotation = RequireSingle();
				name = GetNextName(false);
			}
			if ("Scale" == name) {
				result.Scale = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Shader" == name) {
				result.Shader = (global::Lime.ShaderId)RequireInt();
				name = GetNextName(false);
			}
			if ("SilentSize" == name) {
				result.SilentSize = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("SkinningWeights" == name) {
				result.SkinningWeights = GeneratedDeserializersJSON.Lime.SkinningWeights_JsonDeserializer.Instance.FromReaderTyped<global::Lime.SkinningWeights>(Reader);
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("Visible" == name) {
				result.Visible = RequireBool();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Animation_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Animation_JsonDeserializer Instance = new Animation_JsonDeserializer();

		public Animation_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Animation>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Animation(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Animation)obj;
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Markers" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp1 = GeneratedDeserializersJSON.Lime.Marker_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Marker>(Reader);
						result.Markers.Add(tmp1);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Spline_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Spline_JsonDeserializer Instance = new Spline_JsonDeserializer();

		public Spline_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Spline>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Spline(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Spline)obj;
			if ("Anchors" == name) {
				result.Anchors = (global::Lime.Anchors)RequireInt();
				name = GetNextName(false);
			}
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("Blending" == name) {
				result.Blending = (global::Lime.Blending)RequireInt();
				name = GetNextName(false);
			}
			if ("BoneArray" == name) {
				result.BoneArray = GeneratedDeserializersJSON.Lime.BoneArray_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoneArray>(Reader);
				name = GetNextName(false);
			}
			if ("Color" == name) {
				result.Color = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("HitTestMethod" == name) {
				result.HitTestMethod = (global::Lime.HitTestMethod)RequireInt();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Pivot" == name) {
				result.Pivot = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Rotation" == name) {
				result.Rotation = RequireSingle();
				name = GetNextName(false);
			}
			if ("Scale" == name) {
				result.Scale = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Shader" == name) {
				result.Shader = (global::Lime.ShaderId)RequireInt();
				name = GetNextName(false);
			}
			if ("SilentSize" == name) {
				result.SilentSize = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("SkinningWeights" == name) {
				result.SkinningWeights = GeneratedDeserializersJSON.Lime.SkinningWeights_JsonDeserializer.Instance.FromReaderTyped<global::Lime.SkinningWeights>(Reader);
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("Visible" == name) {
				result.Visible = RequireBool();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class LinearLayout_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new LinearLayout_JsonDeserializer Instance = new LinearLayout_JsonDeserializer();

		public LinearLayout_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.LinearLayout>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.LinearLayout(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.LinearLayout)obj;
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("Horizontal" == name) {
				result.Horizontal = RequireBool();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("ProcessHidden" == name) {
				result.ProcessHidden = RequireBool();
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Animator_String_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Animator_String_JsonDeserializer Instance = new Animator_String_JsonDeserializer();

		public Animator_String_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Animator<global::System.String>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Animator<global::System.String>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Animator<global::System.String>)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::System.String>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_String_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::System.String>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Animator_Int32_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Animator_Int32_JsonDeserializer Instance = new Animator_Int32_JsonDeserializer();

		public Animator_Int32_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Animator<global::System.Int32>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Animator<global::System.Int32>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Animator<global::System.Int32>)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::System.Int32>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_Int32_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::System.Int32>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Animator_Boolean_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Animator_Boolean_JsonDeserializer Instance = new Animator_Boolean_JsonDeserializer();

		public Animator_Boolean_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Animator<global::System.Boolean>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Animator<global::System.Boolean>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Animator<global::System.Boolean>)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::System.Boolean>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_Boolean_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::System.Boolean>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Animator_Blending_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Animator_Blending_JsonDeserializer Instance = new Animator_Blending_JsonDeserializer();

		public Animator_Blending_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Animator<global::Lime.Blending>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Animator<global::Lime.Blending>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Animator<global::Lime.Blending>)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::Lime.Blending>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_Blending_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::Lime.Blending>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Animator_ITexture_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Animator_ITexture_JsonDeserializer Instance = new Animator_ITexture_JsonDeserializer();

		public Animator_ITexture_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Animator<global::Lime.ITexture>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Animator<global::Lime.ITexture>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Animator<global::Lime.ITexture>)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::Lime.ITexture>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_ITexture_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::Lime.ITexture>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Animator_NumericRange_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Animator_NumericRange_JsonDeserializer Instance = new Animator_NumericRange_JsonDeserializer();

		public Animator_NumericRange_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Animator<global::Lime.NumericRange>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Animator<global::Lime.NumericRange>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Animator<global::Lime.NumericRange>)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::Lime.NumericRange>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_NumericRange_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::Lime.NumericRange>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Animator_Vector2_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Animator_Vector2_JsonDeserializer Instance = new Animator_Vector2_JsonDeserializer();

		public Animator_Vector2_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Animator<global::Lime.Vector2>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Animator<global::Lime.Vector2>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Animator<global::Lime.Vector2>)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::Lime.Vector2>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::Lime.Vector2>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Animator_Color4_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Animator_Color4_JsonDeserializer Instance = new Animator_Color4_JsonDeserializer();

		public Animator_Color4_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Animator<global::Lime.Color4>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Animator<global::Lime.Color4>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Animator<global::Lime.Color4>)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::Lime.Color4>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::Lime.Color4>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Animator_Single_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Animator_Single_JsonDeserializer Instance = new Animator_Single_JsonDeserializer();

		public Animator_Single_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Animator<global::System.Single>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Animator<global::System.Single>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Animator<global::System.Single>)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::System.Single>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_Single_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::System.Single>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Animator_EmitterShape_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Animator_EmitterShape_JsonDeserializer Instance = new Animator_EmitterShape_JsonDeserializer();

		public Animator_EmitterShape_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Animator<global::Lime.EmitterShape>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Animator<global::Lime.EmitterShape>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Animator<global::Lime.EmitterShape>)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::Lime.EmitterShape>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_EmitterShape_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::Lime.EmitterShape>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Animator_AudioAction_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Animator_AudioAction_JsonDeserializer Instance = new Animator_AudioAction_JsonDeserializer();

		public Animator_AudioAction_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Animator<global::Lime.AudioAction>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Animator<global::Lime.AudioAction>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Animator<global::Lime.AudioAction>)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::Lime.AudioAction>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_AudioAction_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::Lime.AudioAction>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Animator_SerializableSample_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Animator_SerializableSample_JsonDeserializer Instance = new Animator_SerializableSample_JsonDeserializer();

		public Animator_SerializableSample_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Animator<global::Lime.SerializableSample>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Animator<global::Lime.SerializableSample>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Animator<global::Lime.SerializableSample>)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::Lime.SerializableSample>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_SerializableSample_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::Lime.SerializableSample>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Animator_HAlignment_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Animator_HAlignment_JsonDeserializer Instance = new Animator_HAlignment_JsonDeserializer();

		public Animator_HAlignment_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Animator<global::Lime.HAlignment>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Animator<global::Lime.HAlignment>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Animator<global::Lime.HAlignment>)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::Lime.HAlignment>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_HAlignment_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::Lime.HAlignment>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Animator_VAlignment_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Animator_VAlignment_JsonDeserializer Instance = new Animator_VAlignment_JsonDeserializer();

		public Animator_VAlignment_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Animator<global::Lime.VAlignment>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Animator<global::Lime.VAlignment>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Animator<global::Lime.VAlignment>)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::Lime.VAlignment>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_VAlignment_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::Lime.VAlignment>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Animator_MovieAction_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Animator_MovieAction_JsonDeserializer Instance = new Animator_MovieAction_JsonDeserializer();

		public Animator_MovieAction_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Animator<global::Lime.MovieAction>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Animator<global::Lime.MovieAction>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Animator<global::Lime.MovieAction>)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::Lime.MovieAction>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_MovieAction_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::Lime.MovieAction>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Animator_ShaderId_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Animator_ShaderId_JsonDeserializer Instance = new Animator_ShaderId_JsonDeserializer();

		public Animator_ShaderId_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Animator<global::Lime.ShaderId>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Animator<global::Lime.ShaderId>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Animator<global::Lime.ShaderId>)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::Lime.ShaderId>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_ShaderId_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::Lime.ShaderId>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Animator_Vector3_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Animator_Vector3_JsonDeserializer Instance = new Animator_Vector3_JsonDeserializer();

		public Animator_Vector3_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Animator<global::Lime.Vector3>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Animator<global::Lime.Vector3>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Animator<global::Lime.Vector3>)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::Lime.Vector3>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_Vector3_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::Lime.Vector3>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Animator_Quaternion_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Animator_Quaternion_JsonDeserializer Instance = new Animator_Quaternion_JsonDeserializer();

		public Animator_Quaternion_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Animator<global::Lime.Quaternion>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Animator<global::Lime.Quaternion>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Animator<global::Lime.Quaternion>)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::Lime.Quaternion>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_Quaternion_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::Lime.Quaternion>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Animator_EmissionType_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Animator_EmissionType_JsonDeserializer Instance = new Animator_EmissionType_JsonDeserializer();

		public Animator_EmissionType_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Animator<global::Lime.EmissionType>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Animator<global::Lime.EmissionType>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Animator<global::Lime.EmissionType>)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::Lime.EmissionType>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_EmissionType_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::Lime.EmissionType>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class NumericAnimator_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new NumericAnimator_JsonDeserializer Instance = new NumericAnimator_JsonDeserializer();

		public NumericAnimator_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.NumericAnimator>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.NumericAnimator(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.NumericAnimator)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::System.Single>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_Single_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::System.Single>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Vector2Animator_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Vector2Animator_JsonDeserializer Instance = new Vector2Animator_JsonDeserializer();

		public Vector2Animator_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Vector2Animator>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Vector2Animator(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Vector2Animator)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::Lime.Vector2>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::Lime.Vector2>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Color4Animator_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Color4Animator_JsonDeserializer Instance = new Color4Animator_JsonDeserializer();

		public Color4Animator_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Color4Animator>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Color4Animator(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Color4Animator)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::Lime.Color4>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::Lime.Color4>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class QuaternionAnimator_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new QuaternionAnimator_JsonDeserializer Instance = new QuaternionAnimator_JsonDeserializer();

		public QuaternionAnimator_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.QuaternionAnimator>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.QuaternionAnimator(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.QuaternionAnimator)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::Lime.Quaternion>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_Quaternion_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::Lime.Quaternion>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Vector3Animator_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Vector3Animator_JsonDeserializer Instance = new Vector3Animator_JsonDeserializer();

		public Vector3Animator_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Vector3Animator>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Vector3Animator(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Vector3Animator)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::Lime.Vector3>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_Vector3_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::Lime.Vector3>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Matrix44Animator_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Matrix44Animator_JsonDeserializer Instance = new Matrix44Animator_JsonDeserializer();

		public Matrix44Animator_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Matrix44Animator>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Matrix44Animator(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Matrix44Animator)obj;
			if ("AnimationId" == name) {
				result.AnimationId = RequireString();
				name = GetNextName(false);
			}
			if ("ReadonlyKeys" == name) {
				result.ReadonlyKeys = RequireOrNull('[') ? null : new global::Lime.KeyframeCollection<global::Lime.Matrix44>();
				if (result.ReadonlyKeys != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Keyframe_Matrix44_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Keyframe<global::Lime.Matrix44>>(Reader);
							result.ReadonlyKeys.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("TargetProperty" == name) {
				result.TargetProperty = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Keyframe_String_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Keyframe_String_JsonDeserializer Instance = new Keyframe_String_JsonDeserializer();

		public Keyframe_String_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Keyframe<global::System.String>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Keyframe<global::System.String>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Keyframe<global::System.String>)obj;
			if ("Frame" == name) {
				result.Frame = RequireInt();
				name = GetNextName(false);
			}
			if ("Function" == name) {
				result.Function = (global::Lime.KeyFunction)RequireInt();
				name = GetNextName(false);
			}
			if ("Value" == name) {
				result.Value = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Keyframe_Int32_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Keyframe_Int32_JsonDeserializer Instance = new Keyframe_Int32_JsonDeserializer();

		public Keyframe_Int32_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Keyframe<global::System.Int32>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Keyframe<global::System.Int32>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Keyframe<global::System.Int32>)obj;
			if ("Frame" == name) {
				result.Frame = RequireInt();
				name = GetNextName(false);
			}
			if ("Function" == name) {
				result.Function = (global::Lime.KeyFunction)RequireInt();
				name = GetNextName(false);
			}
			if ("Value" == name) {
				result.Value = RequireInt();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Keyframe_Boolean_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Keyframe_Boolean_JsonDeserializer Instance = new Keyframe_Boolean_JsonDeserializer();

		public Keyframe_Boolean_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Keyframe<global::System.Boolean>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Keyframe<global::System.Boolean>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Keyframe<global::System.Boolean>)obj;
			if ("Frame" == name) {
				result.Frame = RequireInt();
				name = GetNextName(false);
			}
			if ("Function" == name) {
				result.Function = (global::Lime.KeyFunction)RequireInt();
				name = GetNextName(false);
			}
			if ("Value" == name) {
				result.Value = RequireBool();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Keyframe_Blending_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Keyframe_Blending_JsonDeserializer Instance = new Keyframe_Blending_JsonDeserializer();

		public Keyframe_Blending_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Keyframe<global::Lime.Blending>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Keyframe<global::Lime.Blending>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Keyframe<global::Lime.Blending>)obj;
			if ("Frame" == name) {
				result.Frame = RequireInt();
				name = GetNextName(false);
			}
			if ("Function" == name) {
				result.Function = (global::Lime.KeyFunction)RequireInt();
				name = GetNextName(false);
			}
			if ("Value" == name) {
				result.Value = (global::Lime.Blending)RequireInt();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Keyframe_ITexture_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Keyframe_ITexture_JsonDeserializer Instance = new Keyframe_ITexture_JsonDeserializer();

		public Keyframe_ITexture_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Keyframe<global::Lime.ITexture>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Keyframe<global::Lime.ITexture>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Keyframe<global::Lime.ITexture>)obj;
			if ("Frame" == name) {
				result.Frame = RequireInt();
				name = GetNextName(false);
			}
			if ("Function" == name) {
				result.Function = (global::Lime.KeyFunction)RequireInt();
				name = GetNextName(false);
			}
			if ("Value" == name) {
				result.Value = GeneratedDeserializersJSON.Lime.ITexture_JsonDeserializer.Instance.FromReaderInterface<global::Lime.ITexture>(Reader);
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Keyframe_NumericRange_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Keyframe_NumericRange_JsonDeserializer Instance = new Keyframe_NumericRange_JsonDeserializer();

		public Keyframe_NumericRange_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Keyframe<global::Lime.NumericRange>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Keyframe<global::Lime.NumericRange>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Keyframe<global::Lime.NumericRange>)obj;
			if ("Frame" == name) {
				result.Frame = RequireInt();
				name = GetNextName(false);
			}
			if ("Function" == name) {
				result.Function = (global::Lime.KeyFunction)RequireInt();
				name = GetNextName(false);
			}
			if ("Value" == name) {
				result.Value = GeneratedDeserializersJSON.Lime.NumericRange_JsonDeserializer.Instance.FromReaderTyped<global::Lime.NumericRange>(Reader);
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Keyframe_Vector2_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Keyframe_Vector2_JsonDeserializer Instance = new Keyframe_Vector2_JsonDeserializer();

		public Keyframe_Vector2_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Keyframe<global::Lime.Vector2>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Keyframe<global::Lime.Vector2>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Keyframe<global::Lime.Vector2>)obj;
			if ("Frame" == name) {
				result.Frame = RequireInt();
				name = GetNextName(false);
			}
			if ("Function" == name) {
				result.Function = (global::Lime.KeyFunction)RequireInt();
				name = GetNextName(false);
			}
			if ("Value" == name) {
				result.Value = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Keyframe_Color4_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Keyframe_Color4_JsonDeserializer Instance = new Keyframe_Color4_JsonDeserializer();

		public Keyframe_Color4_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Keyframe<global::Lime.Color4>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Keyframe<global::Lime.Color4>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Keyframe<global::Lime.Color4>)obj;
			if ("Frame" == name) {
				result.Frame = RequireInt();
				name = GetNextName(false);
			}
			if ("Function" == name) {
				result.Function = (global::Lime.KeyFunction)RequireInt();
				name = GetNextName(false);
			}
			if ("Value" == name) {
				result.Value = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Keyframe_Single_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Keyframe_Single_JsonDeserializer Instance = new Keyframe_Single_JsonDeserializer();

		public Keyframe_Single_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Keyframe<global::System.Single>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Keyframe<global::System.Single>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Keyframe<global::System.Single>)obj;
			if ("Frame" == name) {
				result.Frame = RequireInt();
				name = GetNextName(false);
			}
			if ("Function" == name) {
				result.Function = (global::Lime.KeyFunction)RequireInt();
				name = GetNextName(false);
			}
			if ("Value" == name) {
				result.Value = RequireSingle();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Keyframe_EmitterShape_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Keyframe_EmitterShape_JsonDeserializer Instance = new Keyframe_EmitterShape_JsonDeserializer();

		public Keyframe_EmitterShape_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Keyframe<global::Lime.EmitterShape>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Keyframe<global::Lime.EmitterShape>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Keyframe<global::Lime.EmitterShape>)obj;
			if ("Frame" == name) {
				result.Frame = RequireInt();
				name = GetNextName(false);
			}
			if ("Function" == name) {
				result.Function = (global::Lime.KeyFunction)RequireInt();
				name = GetNextName(false);
			}
			if ("Value" == name) {
				result.Value = (global::Lime.EmitterShape)RequireInt();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Keyframe_AudioAction_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Keyframe_AudioAction_JsonDeserializer Instance = new Keyframe_AudioAction_JsonDeserializer();

		public Keyframe_AudioAction_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Keyframe<global::Lime.AudioAction>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Keyframe<global::Lime.AudioAction>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Keyframe<global::Lime.AudioAction>)obj;
			if ("Frame" == name) {
				result.Frame = RequireInt();
				name = GetNextName(false);
			}
			if ("Function" == name) {
				result.Function = (global::Lime.KeyFunction)RequireInt();
				name = GetNextName(false);
			}
			if ("Value" == name) {
				result.Value = (global::Lime.AudioAction)RequireInt();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Keyframe_SerializableSample_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Keyframe_SerializableSample_JsonDeserializer Instance = new Keyframe_SerializableSample_JsonDeserializer();

		public Keyframe_SerializableSample_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Keyframe<global::Lime.SerializableSample>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Keyframe<global::Lime.SerializableSample>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Keyframe<global::Lime.SerializableSample>)obj;
			if ("Frame" == name) {
				result.Frame = RequireInt();
				name = GetNextName(false);
			}
			if ("Function" == name) {
				result.Function = (global::Lime.KeyFunction)RequireInt();
				name = GetNextName(false);
			}
			if ("Value" == name) {
				result.Value = GeneratedDeserializersJSON.Lime.SerializableSample_JsonDeserializer.Instance.FromReaderTyped<global::Lime.SerializableSample>(Reader);
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Keyframe_HAlignment_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Keyframe_HAlignment_JsonDeserializer Instance = new Keyframe_HAlignment_JsonDeserializer();

		public Keyframe_HAlignment_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Keyframe<global::Lime.HAlignment>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Keyframe<global::Lime.HAlignment>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Keyframe<global::Lime.HAlignment>)obj;
			if ("Frame" == name) {
				result.Frame = RequireInt();
				name = GetNextName(false);
			}
			if ("Function" == name) {
				result.Function = (global::Lime.KeyFunction)RequireInt();
				name = GetNextName(false);
			}
			if ("Value" == name) {
				result.Value = (global::Lime.HAlignment)RequireInt();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Keyframe_VAlignment_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Keyframe_VAlignment_JsonDeserializer Instance = new Keyframe_VAlignment_JsonDeserializer();

		public Keyframe_VAlignment_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Keyframe<global::Lime.VAlignment>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Keyframe<global::Lime.VAlignment>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Keyframe<global::Lime.VAlignment>)obj;
			if ("Frame" == name) {
				result.Frame = RequireInt();
				name = GetNextName(false);
			}
			if ("Function" == name) {
				result.Function = (global::Lime.KeyFunction)RequireInt();
				name = GetNextName(false);
			}
			if ("Value" == name) {
				result.Value = (global::Lime.VAlignment)RequireInt();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Keyframe_MovieAction_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Keyframe_MovieAction_JsonDeserializer Instance = new Keyframe_MovieAction_JsonDeserializer();

		public Keyframe_MovieAction_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Keyframe<global::Lime.MovieAction>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Keyframe<global::Lime.MovieAction>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Keyframe<global::Lime.MovieAction>)obj;
			if ("Frame" == name) {
				result.Frame = RequireInt();
				name = GetNextName(false);
			}
			if ("Function" == name) {
				result.Function = (global::Lime.KeyFunction)RequireInt();
				name = GetNextName(false);
			}
			if ("Value" == name) {
				result.Value = (global::Lime.MovieAction)RequireInt();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Keyframe_ShaderId_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Keyframe_ShaderId_JsonDeserializer Instance = new Keyframe_ShaderId_JsonDeserializer();

		public Keyframe_ShaderId_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Keyframe<global::Lime.ShaderId>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Keyframe<global::Lime.ShaderId>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Keyframe<global::Lime.ShaderId>)obj;
			if ("Frame" == name) {
				result.Frame = RequireInt();
				name = GetNextName(false);
			}
			if ("Function" == name) {
				result.Function = (global::Lime.KeyFunction)RequireInt();
				name = GetNextName(false);
			}
			if ("Value" == name) {
				result.Value = (global::Lime.ShaderId)RequireInt();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Keyframe_Vector3_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Keyframe_Vector3_JsonDeserializer Instance = new Keyframe_Vector3_JsonDeserializer();

		public Keyframe_Vector3_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Keyframe<global::Lime.Vector3>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Keyframe<global::Lime.Vector3>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Keyframe<global::Lime.Vector3>)obj;
			if ("Frame" == name) {
				result.Frame = RequireInt();
				name = GetNextName(false);
			}
			if ("Function" == name) {
				result.Function = (global::Lime.KeyFunction)RequireInt();
				name = GetNextName(false);
			}
			if ("Value" == name) {
				result.Value = GeneratedDeserializersJSON.Lime.Vector3_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector3>(Reader);
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Keyframe_Quaternion_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Keyframe_Quaternion_JsonDeserializer Instance = new Keyframe_Quaternion_JsonDeserializer();

		public Keyframe_Quaternion_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Keyframe<global::Lime.Quaternion>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Keyframe<global::Lime.Quaternion>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Keyframe<global::Lime.Quaternion>)obj;
			if ("Frame" == name) {
				result.Frame = RequireInt();
				name = GetNextName(false);
			}
			if ("Function" == name) {
				result.Function = (global::Lime.KeyFunction)RequireInt();
				name = GetNextName(false);
			}
			if ("Value" == name) {
				result.Value = GeneratedDeserializersJSON.Lime.Quaternion_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Quaternion>(Reader);
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Keyframe_EmissionType_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Keyframe_EmissionType_JsonDeserializer Instance = new Keyframe_EmissionType_JsonDeserializer();

		public Keyframe_EmissionType_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Keyframe<global::Lime.EmissionType>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Keyframe<global::Lime.EmissionType>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Keyframe<global::Lime.EmissionType>)obj;
			if ("Frame" == name) {
				result.Frame = RequireInt();
				name = GetNextName(false);
			}
			if ("Function" == name) {
				result.Function = (global::Lime.KeyFunction)RequireInt();
				name = GetNextName(false);
			}
			if ("Value" == name) {
				result.Value = (global::Lime.EmissionType)RequireInt();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Keyframe_Matrix44_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Keyframe_Matrix44_JsonDeserializer Instance = new Keyframe_Matrix44_JsonDeserializer();

		public Keyframe_Matrix44_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Keyframe<global::Lime.Matrix44>>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Keyframe<global::Lime.Matrix44>(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Keyframe<global::Lime.Matrix44>)obj;
			if ("Frame" == name) {
				result.Frame = RequireInt();
				name = GetNextName(false);
			}
			if ("Function" == name) {
				result.Function = (global::Lime.KeyFunction)RequireInt();
				name = GetNextName(false);
			}
			if ("Value" == name) {
				result.Value = GeneratedDeserializersJSON.Lime.Matrix44_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Matrix44>(Reader);
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Audio_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Audio_JsonDeserializer Instance = new Audio_JsonDeserializer();

		public Audio_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Audio>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Audio(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Audio)obj;
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("Bumpable" == name) {
				result.Bumpable = RequireBool();
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("FadeTime" == name) {
				result.FadeTime = RequireSingle();
				name = GetNextName(false);
			}
			if ("Group" == name) {
				result.Group = (global::Lime.AudioChannelGroup)RequireInt();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Looping" == name) {
				result.Looping = RequireBool();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Pan" == name) {
				result.Pan = RequireSingle();
				name = GetNextName(false);
			}
			if ("Pitch" == name) {
				result.Pitch = RequireSingle();
				name = GetNextName(false);
			}
			if ("Priority" == name) {
				result.Priority = RequireSingle();
				name = GetNextName(false);
			}
			if ("Sample" == name) {
				result.Sample = GeneratedDeserializersJSON.Lime.SerializableSample_JsonDeserializer.Instance.FromReaderTyped<global::Lime.SerializableSample>(Reader);
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("Volume" == name) {
				result.Volume = RequireSingle();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class BoneWeight_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new BoneWeight_JsonDeserializer Instance = new BoneWeight_JsonDeserializer();

		public BoneWeight_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.BoneWeight>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.BoneWeight(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.BoneWeight)obj;
			if ("Index" == name) {
				result.Index = RequireInt();
				name = GetNextName(false);
			}
			if ("Weight" == name) {
				result.Weight = RequireSingle();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}

		protected override object ReadFieldsCompact(object obj)
		{
			var result = (global::Lime.BoneWeight)obj;
			result.Index = RequireInt();
			Require(',');
			result.Weight = RequireSingle();
			Require(']');
			return result;
		}
	}

	class SkinningWeights_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SkinningWeights_JsonDeserializer Instance = new SkinningWeights_JsonDeserializer();

		public SkinningWeights_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.SkinningWeights>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.SkinningWeights(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.SkinningWeights)obj;
			if ("Bone0" == name) {
				result.Bone0 = GeneratedDeserializersJSON.Lime.BoneWeight_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoneWeight>(Reader);
				name = GetNextName(false);
			}
			if ("Bone1" == name) {
				result.Bone1 = GeneratedDeserializersJSON.Lime.BoneWeight_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoneWeight>(Reader);
				name = GetNextName(false);
			}
			if ("Bone2" == name) {
				result.Bone2 = GeneratedDeserializersJSON.Lime.BoneWeight_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoneWeight>(Reader);
				name = GetNextName(false);
			}
			if ("Bone3" == name) {
				result.Bone3 = GeneratedDeserializersJSON.Lime.BoneWeight_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoneWeight>(Reader);
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}

		protected override object ReadFieldsCompact(object obj)
		{
			var result = (global::Lime.SkinningWeights)obj;
			result.Bone0 = GeneratedDeserializersJSON.Lime.BoneWeight_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoneWeight>(Reader);
			Require(',');
			result.Bone1 = GeneratedDeserializersJSON.Lime.BoneWeight_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoneWeight>(Reader);
			Require(',');
			result.Bone2 = GeneratedDeserializersJSON.Lime.BoneWeight_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoneWeight>(Reader);
			Require(',');
			result.Bone3 = GeneratedDeserializersJSON.Lime.BoneWeight_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoneWeight>(Reader);
			Require(']');
			return result;
		}
	}

	class Bone_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Bone_JsonDeserializer Instance = new Bone_JsonDeserializer();

		public Bone_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Bone>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Bone(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Bone)obj;
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("BaseIndex" == name) {
				result.BaseIndex = RequireInt();
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("EffectiveRadius" == name) {
				result.EffectiveRadius = RequireSingle();
				name = GetNextName(false);
			}
			if ("FadeoutZone" == name) {
				result.FadeoutZone = RequireSingle();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("IKStopper" == name) {
				result.IKStopper = RequireBool();
				name = GetNextName(false);
			}
			if ("Index" == name) {
				result.Index = RequireInt();
				name = GetNextName(false);
			}
			if ("Length" == name) {
				result.Length = RequireSingle();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("RefLength" == name) {
				result.RefLength = RequireSingle();
				name = GetNextName(false);
			}
			if ("RefPosition" == name) {
				result.RefPosition = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("RefRotation" == name) {
				result.RefRotation = RequireSingle();
				name = GetNextName(false);
			}
			if ("Rotation" == name) {
				result.Rotation = RequireSingle();
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class BoneArray_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new BoneArray_JsonDeserializer Instance = new BoneArray_JsonDeserializer();

		public BoneArray_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.BoneArray>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.BoneArray(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.BoneArray)obj;
			if ("items" == name) {
				result.items = RequireOrNull('[') ? null : new global::Lime.BoneArray.Entry[0];
				if (result.items != null) {
					if (SkipSpacesCarefully() != ']') {
						var tmp1 = new global::Lime.BoneArray.Entry[RequireUInt()];
						for(int tmp2 = 0; tmp2 < tmp1.Length; ++tmp2) {
							Require(',');
							tmp1[tmp2] = GeneratedDeserializersJSON.Lime.BoneArray__Entry_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoneArray.Entry>(Reader);
						}
						result.items = tmp1;
					}
					Require(']');
				}
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class BoneArray__Entry_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new BoneArray__Entry_JsonDeserializer Instance = new BoneArray__Entry_JsonDeserializer();

		public BoneArray__Entry_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.BoneArray.Entry>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.BoneArray.Entry(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.BoneArray.Entry)obj;
			if ("Joint" == name) {
				result.Joint = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Length" == name) {
				result.Length = RequireSingle();
				name = GetNextName(false);
			}
			if ("RelativeTransform" == name) {
				result.RelativeTransform = GeneratedDeserializersJSON.Lime.Matrix32_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Matrix32>(Reader);
				name = GetNextName(false);
			}
			if ("Rotation" == name) {
				result.Rotation = RequireSingle();
				name = GetNextName(false);
			}
			if ("Tip" == name) {
				result.Tip = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Button_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Button_JsonDeserializer Instance = new Button_JsonDeserializer();

		public Button_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Button>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Button(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Button)obj;
			if ("Anchors" == name) {
				result.Anchors = (global::Lime.Anchors)RequireInt();
				name = GetNextName(false);
			}
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("Blending" == name) {
				result.Blending = (global::Lime.Blending)RequireInt();
				name = GetNextName(false);
			}
			if ("BoneArray" == name) {
				result.BoneArray = GeneratedDeserializersJSON.Lime.BoneArray_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoneArray>(Reader);
				name = GetNextName(false);
			}
			if ("Color" == name) {
				result.Color = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("Draggable" == name) {
				result.Draggable = RequireBool();
				name = GetNextName(false);
			}
			if ("Enabled" == name) {
				result.Enabled = RequireBool();
				name = GetNextName(false);
			}
			if ("HitTestMethod" == name) {
				result.HitTestMethod = (global::Lime.HitTestMethod)RequireInt();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Pivot" == name) {
				result.Pivot = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Rotation" == name) {
				result.Rotation = RequireSingle();
				name = GetNextName(false);
			}
			if ("Scale" == name) {
				result.Scale = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Shader" == name) {
				result.Shader = (global::Lime.ShaderId)RequireInt();
				name = GetNextName(false);
			}
			if ("SilentSize" == name) {
				result.SilentSize = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("SkinningWeights" == name) {
				result.SkinningWeights = GeneratedDeserializersJSON.Lime.SkinningWeights_JsonDeserializer.Instance.FromReaderTyped<global::Lime.SkinningWeights>(Reader);
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("Text" == name) {
				result.Text = RequireString();
				name = GetNextName(false);
			}
			if ("Visible" == name) {
				result.Visible = RequireBool();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class DistortionMesh_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new DistortionMesh_JsonDeserializer Instance = new DistortionMesh_JsonDeserializer();

		public DistortionMesh_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.DistortionMesh>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.DistortionMesh(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.DistortionMesh)obj;
			if ("Anchors" == name) {
				result.Anchors = (global::Lime.Anchors)RequireInt();
				name = GetNextName(false);
			}
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("Blending" == name) {
				result.Blending = (global::Lime.Blending)RequireInt();
				name = GetNextName(false);
			}
			if ("BoneArray" == name) {
				result.BoneArray = GeneratedDeserializersJSON.Lime.BoneArray_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoneArray>(Reader);
				name = GetNextName(false);
			}
			if ("Color" == name) {
				result.Color = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("HitTestMethod" == name) {
				result.HitTestMethod = (global::Lime.HitTestMethod)RequireInt();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("NumCols" == name) {
				result.NumCols = RequireInt();
				name = GetNextName(false);
			}
			if ("NumRows" == name) {
				result.NumRows = RequireInt();
				name = GetNextName(false);
			}
			if ("Pivot" == name) {
				result.Pivot = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Rotation" == name) {
				result.Rotation = RequireSingle();
				name = GetNextName(false);
			}
			if ("Scale" == name) {
				result.Scale = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Shader" == name) {
				result.Shader = (global::Lime.ShaderId)RequireInt();
				name = GetNextName(false);
			}
			if ("SilentSize" == name) {
				result.SilentSize = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("SkinningWeights" == name) {
				result.SkinningWeights = GeneratedDeserializersJSON.Lime.SkinningWeights_JsonDeserializer.Instance.FromReaderTyped<global::Lime.SkinningWeights>(Reader);
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("Texture" == name) {
				result.Texture = GeneratedDeserializersJSON.Lime.ITexture_JsonDeserializer.Instance.FromReaderInterface<global::Lime.ITexture>(Reader);
				name = GetNextName(false);
			}
			if ("Visible" == name) {
				result.Visible = RequireBool();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class DistortionMeshPoint_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new DistortionMeshPoint_JsonDeserializer Instance = new DistortionMeshPoint_JsonDeserializer();

		public DistortionMeshPoint_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.DistortionMeshPoint>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.DistortionMeshPoint(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.DistortionMeshPoint)obj;
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("Color" == name) {
				result.Color = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Offset" == name) {
				result.Offset = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("SkinningWeights" == name) {
				result.SkinningWeights = GeneratedDeserializersJSON.Lime.SkinningWeights_JsonDeserializer.Instance.FromReaderTyped<global::Lime.SkinningWeights>(Reader);
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("UV" == name) {
				result.UV = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Frame_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Frame_JsonDeserializer Instance = new Frame_JsonDeserializer();

		public Frame_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Frame>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Frame(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Frame)obj;
			if ("Anchors" == name) {
				result.Anchors = (global::Lime.Anchors)RequireInt();
				name = GetNextName(false);
			}
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("Blending" == name) {
				result.Blending = (global::Lime.Blending)RequireInt();
				name = GetNextName(false);
			}
			if ("BoneArray" == name) {
				result.BoneArray = GeneratedDeserializersJSON.Lime.BoneArray_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoneArray>(Reader);
				name = GetNextName(false);
			}
			if ("Color" == name) {
				result.Color = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("HitTestMethod" == name) {
				result.HitTestMethod = (global::Lime.HitTestMethod)RequireInt();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Pivot" == name) {
				result.Pivot = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("RenderTarget" == name) {
				result.RenderTarget = (global::Lime.RenderTarget)RequireInt();
				name = GetNextName(false);
			}
			if ("Rotation" == name) {
				result.Rotation = RequireSingle();
				name = GetNextName(false);
			}
			if ("Scale" == name) {
				result.Scale = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Shader" == name) {
				result.Shader = (global::Lime.ShaderId)RequireInt();
				name = GetNextName(false);
			}
			if ("SilentSize" == name) {
				result.SilentSize = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("SkinningWeights" == name) {
				result.SkinningWeights = GeneratedDeserializersJSON.Lime.SkinningWeights_JsonDeserializer.Instance.FromReaderTyped<global::Lime.SkinningWeights>(Reader);
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("Visible" == name) {
				result.Visible = RequireBool();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Image_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Image_JsonDeserializer Instance = new Image_JsonDeserializer();

		public Image_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Image>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Image(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Image)obj;
			if ("Anchors" == name) {
				result.Anchors = (global::Lime.Anchors)RequireInt();
				name = GetNextName(false);
			}
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("Blending" == name) {
				result.Blending = (global::Lime.Blending)RequireInt();
				name = GetNextName(false);
			}
			if ("BoneArray" == name) {
				result.BoneArray = GeneratedDeserializersJSON.Lime.BoneArray_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoneArray>(Reader);
				name = GetNextName(false);
			}
			if ("Color" == name) {
				result.Color = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("HitTestMethod" == name) {
				result.HitTestMethod = (global::Lime.HitTestMethod)RequireInt();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Pivot" == name) {
				result.Pivot = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Rotation" == name) {
				result.Rotation = RequireSingle();
				name = GetNextName(false);
			}
			if ("Scale" == name) {
				result.Scale = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Shader" == name) {
				result.Shader = (global::Lime.ShaderId)RequireInt();
				name = GetNextName(false);
			}
			if ("SilentSize" == name) {
				result.SilentSize = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("SkinningWeights" == name) {
				result.SkinningWeights = GeneratedDeserializersJSON.Lime.SkinningWeights_JsonDeserializer.Instance.FromReaderTyped<global::Lime.SkinningWeights>(Reader);
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("Texture" == name) {
				result.Texture = GeneratedDeserializersJSON.Lime.ITexture_JsonDeserializer.Instance.FromReaderInterface<global::Lime.ITexture>(Reader);
				name = GetNextName(false);
			}
			if ("UV0" == name) {
				result.UV0 = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("UV1" == name) {
				result.UV1 = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Visible" == name) {
				result.Visible = RequireBool();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class ImageCombiner_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new ImageCombiner_JsonDeserializer Instance = new ImageCombiner_JsonDeserializer();

		public ImageCombiner_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.ImageCombiner>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.ImageCombiner(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.ImageCombiner)obj;
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("Blending" == name) {
				result.Blending = (global::Lime.Blending)RequireInt();
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("Enabled" == name) {
				result.Enabled = RequireBool();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Shader" == name) {
				result.Shader = (global::Lime.ShaderId)RequireInt();
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Marker_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Marker_JsonDeserializer Instance = new Marker_JsonDeserializer();

		public Marker_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Marker>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Marker(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Marker)obj;
			if ("Action" == name) {
				result.Action = (global::Lime.MarkerAction)RequireInt();
				name = GetNextName(false);
			}
			if ("Frame" == name) {
				result.Frame = RequireInt();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("JumpTo" == name) {
				result.JumpTo = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Movie_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Movie_JsonDeserializer Instance = new Movie_JsonDeserializer();

		public Movie_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Movie>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Movie(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Movie)obj;
			if ("Anchors" == name) {
				result.Anchors = (global::Lime.Anchors)RequireInt();
				name = GetNextName(false);
			}
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("Blending" == name) {
				result.Blending = (global::Lime.Blending)RequireInt();
				name = GetNextName(false);
			}
			if ("BoneArray" == name) {
				result.BoneArray = GeneratedDeserializersJSON.Lime.BoneArray_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoneArray>(Reader);
				name = GetNextName(false);
			}
			if ("Color" == name) {
				result.Color = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("HitTestMethod" == name) {
				result.HitTestMethod = (global::Lime.HitTestMethod)RequireInt();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Looped" == name) {
				result.Looped = RequireBool();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Path" == name) {
				result.Path = RequireString();
				name = GetNextName(false);
			}
			if ("Pivot" == name) {
				result.Pivot = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Rotation" == name) {
				result.Rotation = RequireSingle();
				name = GetNextName(false);
			}
			if ("Scale" == name) {
				result.Scale = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Shader" == name) {
				result.Shader = (global::Lime.ShaderId)RequireInt();
				name = GetNextName(false);
			}
			if ("SilentSize" == name) {
				result.SilentSize = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("SkinningWeights" == name) {
				result.SkinningWeights = GeneratedDeserializersJSON.Lime.SkinningWeights_JsonDeserializer.Instance.FromReaderTyped<global::Lime.SkinningWeights>(Reader);
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("Visible" == name) {
				result.Visible = RequireBool();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class NineGrid_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new NineGrid_JsonDeserializer Instance = new NineGrid_JsonDeserializer();

		public NineGrid_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.NineGrid>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.NineGrid(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.NineGrid)obj;
			if ("Anchors" == name) {
				result.Anchors = (global::Lime.Anchors)RequireInt();
				name = GetNextName(false);
			}
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("Blending" == name) {
				result.Blending = (global::Lime.Blending)RequireInt();
				name = GetNextName(false);
			}
			if ("BoneArray" == name) {
				result.BoneArray = GeneratedDeserializersJSON.Lime.BoneArray_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoneArray>(Reader);
				name = GetNextName(false);
			}
			if ("BottomOffset" == name) {
				result.BottomOffset = RequireSingle();
				name = GetNextName(false);
			}
			if ("Color" == name) {
				result.Color = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("HitTestMethod" == name) {
				result.HitTestMethod = (global::Lime.HitTestMethod)RequireInt();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("LeftOffset" == name) {
				result.LeftOffset = RequireSingle();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Pivot" == name) {
				result.Pivot = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("RightOffset" == name) {
				result.RightOffset = RequireSingle();
				name = GetNextName(false);
			}
			if ("Rotation" == name) {
				result.Rotation = RequireSingle();
				name = GetNextName(false);
			}
			if ("Scale" == name) {
				result.Scale = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Shader" == name) {
				result.Shader = (global::Lime.ShaderId)RequireInt();
				name = GetNextName(false);
			}
			if ("SilentSize" == name) {
				result.SilentSize = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("SkinningWeights" == name) {
				result.SkinningWeights = GeneratedDeserializersJSON.Lime.SkinningWeights_JsonDeserializer.Instance.FromReaderTyped<global::Lime.SkinningWeights>(Reader);
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("Texture" == name) {
				result.Texture = GeneratedDeserializersJSON.Lime.ITexture_JsonDeserializer.Instance.FromReaderInterface<global::Lime.ITexture>(Reader);
				name = GetNextName(false);
			}
			if ("TopOffset" == name) {
				result.TopOffset = RequireSingle();
				name = GetNextName(false);
			}
			if ("Visible" == name) {
				result.Visible = RequireBool();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Node_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Node_JsonDeserializer Instance = new Node_JsonDeserializer();

		public Node_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Node>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Node(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Node)obj;
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class ParticleEmitter_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new ParticleEmitter_JsonDeserializer Instance = new ParticleEmitter_JsonDeserializer();

		public ParticleEmitter_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.ParticleEmitter>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.ParticleEmitter(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.ParticleEmitter)obj;
			if ("AlongPathOrientation" == name) {
				result.AlongPathOrientation = RequireBool();
				name = GetNextName(false);
			}
			if ("Anchors" == name) {
				result.Anchors = (global::Lime.Anchors)RequireInt();
				name = GetNextName(false);
			}
			if ("AngularVelocity" == name) {
				result.AngularVelocity = GeneratedDeserializersJSON.Lime.NumericRange_JsonDeserializer.Instance.FromReaderTyped<global::Lime.NumericRange>(Reader);
				name = GetNextName(false);
			}
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("AspectRatio" == name) {
				result.AspectRatio = GeneratedDeserializersJSON.Lime.NumericRange_JsonDeserializer.Instance.FromReaderTyped<global::Lime.NumericRange>(Reader);
				name = GetNextName(false);
			}
			if ("Blending" == name) {
				result.Blending = (global::Lime.Blending)RequireInt();
				name = GetNextName(false);
			}
			if ("BoneArray" == name) {
				result.BoneArray = GeneratedDeserializersJSON.Lime.BoneArray_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoneArray>(Reader);
				name = GetNextName(false);
			}
			if ("Color" == name) {
				result.Color = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("Direction" == name) {
				result.Direction = GeneratedDeserializersJSON.Lime.NumericRange_JsonDeserializer.Instance.FromReaderTyped<global::Lime.NumericRange>(Reader);
				name = GetNextName(false);
			}
			if ("EmissionType" == name) {
				result.EmissionType = (global::Lime.EmissionType)RequireInt();
				name = GetNextName(false);
			}
			if ("GravityAmount" == name) {
				result.GravityAmount = GeneratedDeserializersJSON.Lime.NumericRange_JsonDeserializer.Instance.FromReaderTyped<global::Lime.NumericRange>(Reader);
				name = GetNextName(false);
			}
			if ("GravityDirection" == name) {
				result.GravityDirection = GeneratedDeserializersJSON.Lime.NumericRange_JsonDeserializer.Instance.FromReaderTyped<global::Lime.NumericRange>(Reader);
				name = GetNextName(false);
			}
			if ("HitTestMethod" == name) {
				result.HitTestMethod = (global::Lime.HitTestMethod)RequireInt();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("ImmortalParticles" == name) {
				result.ImmortalParticles = RequireBool();
				name = GetNextName(false);
			}
			if ("Lifetime" == name) {
				result.Lifetime = GeneratedDeserializersJSON.Lime.NumericRange_JsonDeserializer.Instance.FromReaderTyped<global::Lime.NumericRange>(Reader);
				name = GetNextName(false);
			}
			if ("LinkageWidgetName" == name) {
				result.LinkageWidgetName = RequireString();
				name = GetNextName(false);
			}
			if ("MagnetAmount" == name) {
				result.MagnetAmount = GeneratedDeserializersJSON.Lime.NumericRange_JsonDeserializer.Instance.FromReaderTyped<global::Lime.NumericRange>(Reader);
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Number" == name) {
				result.Number = RequireSingle();
				name = GetNextName(false);
			}
			if ("Orientation" == name) {
				result.Orientation = GeneratedDeserializersJSON.Lime.NumericRange_JsonDeserializer.Instance.FromReaderTyped<global::Lime.NumericRange>(Reader);
				name = GetNextName(false);
			}
			if ("ParticlesLinkage" == name) {
				result.ParticlesLinkage = (global::Lime.ParticlesLinkage)RequireInt();
				name = GetNextName(false);
			}
			if ("Pivot" == name) {
				result.Pivot = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("RandomMotionAspectRatio" == name) {
				result.RandomMotionAspectRatio = RequireSingle();
				name = GetNextName(false);
			}
			if ("RandomMotionRadius" == name) {
				result.RandomMotionRadius = GeneratedDeserializersJSON.Lime.NumericRange_JsonDeserializer.Instance.FromReaderTyped<global::Lime.NumericRange>(Reader);
				name = GetNextName(false);
			}
			if ("RandomMotionRotation" == name) {
				result.RandomMotionRotation = GeneratedDeserializersJSON.Lime.NumericRange_JsonDeserializer.Instance.FromReaderTyped<global::Lime.NumericRange>(Reader);
				name = GetNextName(false);
			}
			if ("RandomMotionSpeed" == name) {
				result.RandomMotionSpeed = GeneratedDeserializersJSON.Lime.NumericRange_JsonDeserializer.Instance.FromReaderTyped<global::Lime.NumericRange>(Reader);
				name = GetNextName(false);
			}
			if ("Rotation" == name) {
				result.Rotation = RequireSingle();
				name = GetNextName(false);
			}
			if ("Scale" == name) {
				result.Scale = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Shader" == name) {
				result.Shader = (global::Lime.ShaderId)RequireInt();
				name = GetNextName(false);
			}
			if ("Shape" == name) {
				result.Shape = (global::Lime.EmitterShape)RequireInt();
				name = GetNextName(false);
			}
			if ("SilentSize" == name) {
				result.SilentSize = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("SkinningWeights" == name) {
				result.SkinningWeights = GeneratedDeserializersJSON.Lime.SkinningWeights_JsonDeserializer.Instance.FromReaderTyped<global::Lime.SkinningWeights>(Reader);
				name = GetNextName(false);
			}
			if ("Speed" == name) {
				result.Speed = RequireSingle();
				name = GetNextName(false);
			}
			if ("Spin" == name) {
				result.Spin = GeneratedDeserializersJSON.Lime.NumericRange_JsonDeserializer.Instance.FromReaderTyped<global::Lime.NumericRange>(Reader);
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("TimeShift" == name) {
				result.TimeShift = RequireSingle();
				name = GetNextName(false);
			}
			if ("Velocity" == name) {
				result.Velocity = GeneratedDeserializersJSON.Lime.NumericRange_JsonDeserializer.Instance.FromReaderTyped<global::Lime.NumericRange>(Reader);
				name = GetNextName(false);
			}
			if ("Visible" == name) {
				result.Visible = RequireBool();
				name = GetNextName(false);
			}
			if ("WindAmount" == name) {
				result.WindAmount = GeneratedDeserializersJSON.Lime.NumericRange_JsonDeserializer.Instance.FromReaderTyped<global::Lime.NumericRange>(Reader);
				name = GetNextName(false);
			}
			if ("WindDirection" == name) {
				result.WindDirection = GeneratedDeserializersJSON.Lime.NumericRange_JsonDeserializer.Instance.FromReaderTyped<global::Lime.NumericRange>(Reader);
				name = GetNextName(false);
			}
			if ("Zoom" == name) {
				result.Zoom = GeneratedDeserializersJSON.Lime.NumericRange_JsonDeserializer.Instance.FromReaderTyped<global::Lime.NumericRange>(Reader);
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class ParticleModifier_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new ParticleModifier_JsonDeserializer Instance = new ParticleModifier_JsonDeserializer();

		public ParticleModifier_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.ParticleModifier>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.ParticleModifier(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.ParticleModifier)obj;
			if ("AngularVelocity" == name) {
				result.AngularVelocity = RequireSingle();
				name = GetNextName(false);
			}
			if ("AnimationFps" == name) {
				result.AnimationFps = RequireSingle();
				name = GetNextName(false);
			}
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("AspectRatio" == name) {
				result.AspectRatio = RequireSingle();
				name = GetNextName(false);
			}
			if ("Color" == name) {
				result.Color = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("FirstFrame" == name) {
				result.FirstFrame = RequireInt();
				name = GetNextName(false);
			}
			if ("GravityAmount" == name) {
				result.GravityAmount = RequireSingle();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("LastFrame" == name) {
				result.LastFrame = RequireInt();
				name = GetNextName(false);
			}
			if ("LoopedAnimation" == name) {
				result.LoopedAnimation = RequireBool();
				name = GetNextName(false);
			}
			if ("MagnetAmount" == name) {
				result.MagnetAmount = RequireSingle();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Scale" == name) {
				result.Scale = RequireSingle();
				name = GetNextName(false);
			}
			if ("Spin" == name) {
				result.Spin = RequireSingle();
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("Texture" == name) {
				result.Texture = GeneratedDeserializersJSON.Lime.ITexture_JsonDeserializer.Instance.FromReaderInterface<global::Lime.ITexture>(Reader);
				name = GetNextName(false);
			}
			if ("Velocity" == name) {
				result.Velocity = RequireSingle();
				name = GetNextName(false);
			}
			if ("WindAmount" == name) {
				result.WindAmount = RequireSingle();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class ParticlesMagnet_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new ParticlesMagnet_JsonDeserializer Instance = new ParticlesMagnet_JsonDeserializer();

		public ParticlesMagnet_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.ParticlesMagnet>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.ParticlesMagnet(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.ParticlesMagnet)obj;
			if ("Anchors" == name) {
				result.Anchors = (global::Lime.Anchors)RequireInt();
				name = GetNextName(false);
			}
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("Attenuation" == name) {
				result.Attenuation = RequireSingle();
				name = GetNextName(false);
			}
			if ("Blending" == name) {
				result.Blending = (global::Lime.Blending)RequireInt();
				name = GetNextName(false);
			}
			if ("BoneArray" == name) {
				result.BoneArray = GeneratedDeserializersJSON.Lime.BoneArray_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoneArray>(Reader);
				name = GetNextName(false);
			}
			if ("Color" == name) {
				result.Color = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("HitTestMethod" == name) {
				result.HitTestMethod = (global::Lime.HitTestMethod)RequireInt();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Pivot" == name) {
				result.Pivot = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Rotation" == name) {
				result.Rotation = RequireSingle();
				name = GetNextName(false);
			}
			if ("Scale" == name) {
				result.Scale = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Shader" == name) {
				result.Shader = (global::Lime.ShaderId)RequireInt();
				name = GetNextName(false);
			}
			if ("Shape" == name) {
				result.Shape = (global::Lime.EmitterShape)RequireInt();
				name = GetNextName(false);
			}
			if ("SilentSize" == name) {
				result.SilentSize = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("SkinningWeights" == name) {
				result.SkinningWeights = GeneratedDeserializersJSON.Lime.SkinningWeights_JsonDeserializer.Instance.FromReaderTyped<global::Lime.SkinningWeights>(Reader);
				name = GetNextName(false);
			}
			if ("Strength" == name) {
				result.Strength = RequireSingle();
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("Visible" == name) {
				result.Visible = RequireBool();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class PointObject_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new PointObject_JsonDeserializer Instance = new PointObject_JsonDeserializer();

		public PointObject_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.PointObject>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.PointObject(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.PointObject)obj;
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("SkinningWeights" == name) {
				result.SkinningWeights = GeneratedDeserializersJSON.Lime.SkinningWeights_JsonDeserializer.Instance.FromReaderTyped<global::Lime.SkinningWeights>(Reader);
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Slider_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Slider_JsonDeserializer Instance = new Slider_JsonDeserializer();

		public Slider_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Slider>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Slider(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Slider)obj;
			if ("Anchors" == name) {
				result.Anchors = (global::Lime.Anchors)RequireInt();
				name = GetNextName(false);
			}
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("Blending" == name) {
				result.Blending = (global::Lime.Blending)RequireInt();
				name = GetNextName(false);
			}
			if ("BoneArray" == name) {
				result.BoneArray = GeneratedDeserializersJSON.Lime.BoneArray_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoneArray>(Reader);
				name = GetNextName(false);
			}
			if ("Color" == name) {
				result.Color = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("HitTestMethod" == name) {
				result.HitTestMethod = (global::Lime.HitTestMethod)RequireInt();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Pivot" == name) {
				result.Pivot = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("RangeMax" == name) {
				result.RangeMax = RequireSingle();
				name = GetNextName(false);
			}
			if ("RangeMin" == name) {
				result.RangeMin = RequireSingle();
				name = GetNextName(false);
			}
			if ("Rotation" == name) {
				result.Rotation = RequireSingle();
				name = GetNextName(false);
			}
			if ("Scale" == name) {
				result.Scale = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Shader" == name) {
				result.Shader = (global::Lime.ShaderId)RequireInt();
				name = GetNextName(false);
			}
			if ("SilentSize" == name) {
				result.SilentSize = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("SkinningWeights" == name) {
				result.SkinningWeights = GeneratedDeserializersJSON.Lime.SkinningWeights_JsonDeserializer.Instance.FromReaderTyped<global::Lime.SkinningWeights>(Reader);
				name = GetNextName(false);
			}
			if ("Step" == name) {
				result.Step = RequireSingle();
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("Value" == name) {
				result.Value = RequireSingle();
				name = GetNextName(false);
			}
			if ("Visible" == name) {
				result.Visible = RequireBool();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class SplineGear_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SplineGear_JsonDeserializer Instance = new SplineGear_JsonDeserializer();

		public SplineGear_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.SplineGear>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.SplineGear(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.SplineGear)obj;
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("SplineId" == name) {
				result.SplineId = RequireString();
				name = GetNextName(false);
			}
			if ("SplineOffset" == name) {
				result.SplineOffset = RequireSingle();
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("WidgetId" == name) {
				result.WidgetId = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class SplinePoint_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SplinePoint_JsonDeserializer Instance = new SplinePoint_JsonDeserializer();

		public SplinePoint_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.SplinePoint>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.SplinePoint(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.SplinePoint)obj;
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("SkinningWeights" == name) {
				result.SkinningWeights = GeneratedDeserializersJSON.Lime.SkinningWeights_JsonDeserializer.Instance.FromReaderTyped<global::Lime.SkinningWeights>(Reader);
				name = GetNextName(false);
			}
			if ("Straight" == name) {
				result.Straight = RequireBool();
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("TangentAngle" == name) {
				result.TangentAngle = RequireSingle();
				name = GetNextName(false);
			}
			if ("TangentWeight" == name) {
				result.TangentWeight = RequireSingle();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class RichText_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new RichText_JsonDeserializer Instance = new RichText_JsonDeserializer();

		public RichText_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.RichText>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.RichText(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.RichText)obj;
			if ("Anchors" == name) {
				result.Anchors = (global::Lime.Anchors)RequireInt();
				name = GetNextName(false);
			}
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("Blending" == name) {
				result.Blending = (global::Lime.Blending)RequireInt();
				name = GetNextName(false);
			}
			if ("BoneArray" == name) {
				result.BoneArray = GeneratedDeserializersJSON.Lime.BoneArray_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoneArray>(Reader);
				name = GetNextName(false);
			}
			if ("Color" == name) {
				result.Color = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("HAlignment" == name) {
				result.HAlignment = (global::Lime.HAlignment)RequireInt();
				name = GetNextName(false);
			}
			if ("HitTestMethod" == name) {
				result.HitTestMethod = (global::Lime.HitTestMethod)RequireInt();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("OverflowMode" == name) {
				result.OverflowMode = (global::Lime.TextOverflowMode)RequireInt();
				name = GetNextName(false);
			}
			if ("Pivot" == name) {
				result.Pivot = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Rotation" == name) {
				result.Rotation = RequireSingle();
				name = GetNextName(false);
			}
			if ("Scale" == name) {
				result.Scale = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Shader" == name) {
				result.Shader = (global::Lime.ShaderId)RequireInt();
				name = GetNextName(false);
			}
			if ("SilentSize" == name) {
				result.SilentSize = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("SkinningWeights" == name) {
				result.SkinningWeights = GeneratedDeserializersJSON.Lime.SkinningWeights_JsonDeserializer.Instance.FromReaderTyped<global::Lime.SkinningWeights>(Reader);
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("Text" == name) {
				result.Text = RequireString();
				name = GetNextName(false);
			}
			if ("VAlignment" == name) {
				result.VAlignment = (global::Lime.VAlignment)RequireInt();
				name = GetNextName(false);
			}
			if ("Visible" == name) {
				result.Visible = RequireBool();
				name = GetNextName(false);
			}
			if ("WordSplitAllowed" == name) {
				result.WordSplitAllowed = RequireBool();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class SimpleText_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SimpleText_JsonDeserializer Instance = new SimpleText_JsonDeserializer();

		public SimpleText_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.SimpleText>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.SimpleText(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.SimpleText)obj;
			if ("Anchors" == name) {
				result.Anchors = (global::Lime.Anchors)RequireInt();
				name = GetNextName(false);
			}
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("Blending" == name) {
				result.Blending = (global::Lime.Blending)RequireInt();
				name = GetNextName(false);
			}
			if ("BoneArray" == name) {
				result.BoneArray = GeneratedDeserializersJSON.Lime.BoneArray_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoneArray>(Reader);
				name = GetNextName(false);
			}
			if ("Color" == name) {
				result.Color = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("Font" == name) {
				result.Font = GeneratedDeserializersJSON.Lime.SerializableFont_JsonDeserializer.Instance.FromReaderTyped<global::Lime.SerializableFont>(Reader);
				name = GetNextName(false);
			}
			if ("FontHeight" == name) {
				result.FontHeight = RequireSingle();
				name = GetNextName(false);
			}
			if ("HAlignment" == name) {
				result.HAlignment = (global::Lime.HAlignment)RequireInt();
				name = GetNextName(false);
			}
			if ("HitTestMethod" == name) {
				result.HitTestMethod = (global::Lime.HitTestMethod)RequireInt();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("OverflowMode" == name) {
				result.OverflowMode = (global::Lime.TextOverflowMode)RequireInt();
				name = GetNextName(false);
			}
			if ("Pivot" == name) {
				result.Pivot = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Rotation" == name) {
				result.Rotation = RequireSingle();
				name = GetNextName(false);
			}
			if ("Scale" == name) {
				result.Scale = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Shader" == name) {
				result.Shader = (global::Lime.ShaderId)RequireInt();
				name = GetNextName(false);
			}
			if ("SilentSize" == name) {
				result.SilentSize = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("SkinningWeights" == name) {
				result.SkinningWeights = GeneratedDeserializersJSON.Lime.SkinningWeights_JsonDeserializer.Instance.FromReaderTyped<global::Lime.SkinningWeights>(Reader);
				name = GetNextName(false);
			}
			if ("Spacing" == name) {
				result.Spacing = RequireSingle();
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("Text" == name) {
				result.Text = RequireString();
				name = GetNextName(false);
			}
			if ("TextColor" == name) {
				result.TextColor = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("VAlignment" == name) {
				result.VAlignment = (global::Lime.VAlignment)RequireInt();
				name = GetNextName(false);
			}
			if ("Visible" == name) {
				result.Visible = RequireBool();
				name = GetNextName(false);
			}
			if ("WordSplitAllowed" == name) {
				result.WordSplitAllowed = RequireBool();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class TextStyle_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new TextStyle_JsonDeserializer Instance = new TextStyle_JsonDeserializer();

		public TextStyle_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.TextStyle>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.TextStyle(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.TextStyle)obj;
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("Bold" == name) {
				result.Bold = RequireBool();
				name = GetNextName(false);
			}
			if ("CastShadow" == name) {
				result.CastShadow = RequireBool();
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("Font" == name) {
				result.Font = GeneratedDeserializersJSON.Lime.SerializableFont_JsonDeserializer.Instance.FromReaderTyped<global::Lime.SerializableFont>(Reader);
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("ImageSize" == name) {
				result.ImageSize = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("ImageTexture" == name) {
				result.ImageTexture = GeneratedDeserializersJSON.Lime.ITexture_JsonDeserializer.Instance.FromReaderInterface<global::Lime.ITexture>(Reader);
				name = GetNextName(false);
			}
			if ("ImageUsage" == name) {
				result.ImageUsage = (global::Lime.TextStyle.ImageUsageEnum)RequireInt();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("ShadowColor" == name) {
				result.ShadowColor = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("ShadowOffset" == name) {
				result.ShadowOffset = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Size" == name) {
				result.Size = RequireSingle();
				name = GetNextName(false);
			}
			if ("SpaceAfter" == name) {
				result.SpaceAfter = RequireSingle();
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("TextColor" == name) {
				result.TextColor = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class Widget_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new Widget_JsonDeserializer Instance = new Widget_JsonDeserializer();

		public Widget_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.Widget>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.Widget(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.Widget)obj;
			if ("Anchors" == name) {
				result.Anchors = (global::Lime.Anchors)RequireInt();
				name = GetNextName(false);
			}
			if ("Animations" == name) {
				result.Animations = RequireOrNull('[') ? null : new global::Lime.AnimationList();
				if (result.Animations != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp1 = GeneratedDeserializersJSON.Lime.Animation_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Animation>(Reader);
							result.Animations.Add(tmp1);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Animators" == name) {
				Require('[');
				if (SkipSpacesCarefully() == ']') {
					Require(']');
				}
				else {
					do {
						var tmp2 = GeneratedDeserializersJSON.Lime.IAnimator_JsonDeserializer.Instance.FromReaderInterface<global::Lime.IAnimator>(Reader);
						result.Animators.Add(tmp2);
					} while (Require(']', ',') == ',');
				}
				name = GetNextName(false);
			}
			if ("Blending" == name) {
				result.Blending = (global::Lime.Blending)RequireInt();
				name = GetNextName(false);
			}
			if ("BoneArray" == name) {
				result.BoneArray = GeneratedDeserializersJSON.Lime.BoneArray_JsonDeserializer.Instance.FromReaderTyped<global::Lime.BoneArray>(Reader);
				name = GetNextName(false);
			}
			if ("Color" == name) {
				result.Color = GeneratedDeserializersJSON.Lime.Color4_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Color4>(Reader);
				name = GetNextName(false);
			}
			if ("ContentsPath" == name) {
				result.ContentsPath = RequireString();
				name = GetNextName(false);
			}
			if ("HitTestMethod" == name) {
				result.HitTestMethod = (global::Lime.HitTestMethod)RequireInt();
				name = GetNextName(false);
			}
			if ("Id" == name) {
				result.Id = RequireString();
				name = GetNextName(false);
			}
			if ("Nodes" == name) {
				result.Nodes = RequireOrNull('[') ? null : new global::Lime.NodeList();
				if (result.Nodes != null) {
					if (SkipSpacesCarefully() == ']') {
						Require(']');
					}
					else {
						do {
							var tmp3 = GeneratedDeserializersJSON.Lime.Node_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Node>(Reader);
							result.Nodes.Add(tmp3);
						} while (Require(']', ',') == ',');
					}
				}
				name = GetNextName(false);
			}
			if ("Pivot" == name) {
				result.Pivot = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Position" == name) {
				result.Position = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Rotation" == name) {
				result.Rotation = RequireSingle();
				name = GetNextName(false);
			}
			if ("Scale" == name) {
				result.Scale = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("Shader" == name) {
				result.Shader = (global::Lime.ShaderId)RequireInt();
				name = GetNextName(false);
			}
			if ("SilentSize" == name) {
				result.SilentSize = GeneratedDeserializersJSON.Lime.Vector2_JsonDeserializer.Instance.FromReaderTyped<global::Lime.Vector2>(Reader);
				name = GetNextName(false);
			}
			if ("SkinningWeights" == name) {
				result.SkinningWeights = GeneratedDeserializersJSON.Lime.SkinningWeights_JsonDeserializer.Instance.FromReaderTyped<global::Lime.SkinningWeights>(Reader);
				name = GetNextName(false);
			}
			if ("Tag" == name) {
				result.Tag = RequireString();
				name = GetNextName(false);
			}
			if ("Visible" == name) {
				result.Visible = RequireBool();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

	class IAnimator_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new IAnimator_JsonDeserializer Instance = new IAnimator_JsonDeserializer();

		public IAnimator_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderInterface<global::Lime.IAnimator>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return null;
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.IAnimator)obj;
			Require('}');
			return result;
		}
	}

	class SerializableTexture_JsonDeserializer : JsonDeserializerGenBase
	{
		public static new SerializableTexture_JsonDeserializer Instance = new SerializableTexture_JsonDeserializer();

		public SerializableTexture_JsonDeserializer()
		{
			Options.Assembly = Assembly.Load("Lime, Version=1.0.0.2, Culture=neutral, PublicKeyToken=null");
			Options.IgnoreNewFields = false;
			Options.AllowEmptyTypes = false;
			Options.ReportErrorPosition = true;
			JsonOptions.EnumAsString = false;
			JsonOptions.SaveRootClass = false;
			JsonOptions.IgnoreCompact = false;
			JsonOptions.Int64AsString = false;
			JsonOptions.FieldSeparator = "\n";
			JsonOptions.Indent = "\t";
			JsonOptions.ClassTag = "class";
			JsonOptions.ArrayLengthPrefix = true;
			JsonOptions.DateFormat = "O";
			JsonOptions.TimeSpanFormat = "c";
		}

		public override object FromReaderInt()
		{
			return FromReaderTyped<global::Lime.SerializableTexture>(Reader);
		}

		public override object FromReaderIntPartial(string name)
		{
			return ReadFields(new global::Lime.SerializableTexture(), name);
		}

		protected override object ReadFields(object obj, string name)
		{
			var result = (global::Lime.SerializableTexture)obj;
			if ("SerializationPath" == name) {
				result.SerializationPath = RequireString();
				name = GetNextName(false);
			}
			Require('}');
			return result;
		}
	}

}
