using System;
using System.Text.RegularExpressions;
using OpenTK.Graphics.ES20;

namespace Lime.Graphics.Platform.OpenGL
{
	internal static class GLHelper
	{
		[System.Diagnostics.Conditional("DEBUG")]
		public static void CheckGLErrors()
		{
			var errorCode = GL.GetError();
			if (errorCode != ErrorCode.NoError) {
				throw new GLErrorException(errorCode);
			}
		}

		public static ShaderType GetGLShaderType(ShaderStageMask stage)
		{
			switch (stage) {
				case ShaderStageMask.Vertex:
					return ShaderType.VertexShader;
				case ShaderStageMask.Fragment:
					return ShaderType.FragmentShader;
				default:
					throw new ArgumentException(nameof(stage));
			}
		}

		public static void GetGLTextureFormat(Format format, out All glInternalFormat, out All glFormat, out All glType)
		{
			const All CompressedRgb8Etc2 = (All)37492;
			const All CompressedRgb8PunchthroughAlpha1Etc2 = (All)37494;
			const All CompressedRgba8Etc2Eac = (All)37496;
			glInternalFormat = 0;
			glFormat = 0;
			glType = 0;
			switch (format) {
				case Format.R8G8B8_UNorm:
					glInternalFormat = All.Rgb;
					glFormat = All.Rgb;
					glType = All.UnsignedByte;
					break;
				case Format.R8G8B8A8_UNorm:
					glInternalFormat = All.Rgba;
					glFormat = All.Rgba;
					glType = All.UnsignedByte;
					break;
				case Format.R5G6B5_UNorm_Pack16:
					glInternalFormat = All.Rgb;
					glFormat = All.Rgb;
					glType = All.UnsignedShort565;
					break;
				case Format.R5G5B5A1_UNorm_Pack16:
					glInternalFormat = All.Rgba;
					glFormat = All.Rgba;
					glType = All.UnsignedShort5551;
					break;
				case Format.R4G4B4A4_UNorm_Pack16:
					glInternalFormat = All.Rgba;
					glFormat = All.Rgba;
					glType = All.UnsignedShort4444;
					break;
				case Format.BC1_RGB_UNorm_Block:
					glInternalFormat = All.CompressedRgbS3tcDxt1Ext;
					break;
				case Format.BC1_RGBA_UNorm_Block:
					glInternalFormat = All.CompressedRgbaS3tcDxt1Ext;
					break;
				case Format.BC2_UNorm_Block:
					glInternalFormat = All.CompressedRgbaS3tcDxt3Ext;
					break;
				case Format.BC3_UNorm_Block:
					glInternalFormat = All.CompressedRgbaS3tcDxt5Ext;
					break;
				case Format.ETC1_R8G8B8_UNorm_Block:
					glInternalFormat = All.Etc1Rgb8Oes;
					break;
				case Format.ETC2_R8G8B8_UNorm_Block:
					glInternalFormat = CompressedRgb8Etc2;
					break;
				case Format.ETC2_R8G8B8A1_UNorm_Block:
					glInternalFormat = CompressedRgb8PunchthroughAlpha1Etc2;
					break;
				case Format.ETC2_R8G8B8A8_UNorm_Block:
					glInternalFormat = CompressedRgba8Etc2Eac;
					break;
				case Format.PVRTC1_2Bpp_UNorm_Block:
					glInternalFormat = All.CompressedRgbaPvrtc2Bppv1Img;
					break;
				case Format.PVRTC1_4Bpp_UNorm_Block:
					glInternalFormat = All.CompressedRgbaPvrtc4Bppv1Img;
					break;
				case Format.PVRTC2_2Bpp_UNorm_Block:
					glInternalFormat = All.CompressedRgbaPvrtc2Bppv2Img;
					break;
				case Format.PVRTC2_4Bpp_UNorm_Block:
					glInternalFormat = All.CompressedRgbaPvrtc4Bppv2Img;
					break;
				default:
					throw new ArgumentException(nameof(format));
			}
		}

		public static All GetGLTextureFilter(TextureFilter filter, TextureMipmapMode mipmapMode)
		{
			switch (filter) {
				case TextureFilter.Linear:
					switch (mipmapMode) {
						case TextureMipmapMode.Linear:
							return All.LinearMipmapLinear;
						case TextureMipmapMode.Nearest:
							return All.LinearMipmapNearest;
						default:
							throw new ArgumentException(nameof(mipmapMode));
					}
				case TextureFilter.Nearest:
					switch (mipmapMode) {
						case TextureMipmapMode.Linear:
							return All.NearestMipmapLinear;
						case TextureMipmapMode.Nearest:
							return All.NearestMipmapNearest;
						default:
							throw new ArgumentException(nameof(mipmapMode));
					}
				default:
					throw new ArgumentException(nameof(filter));
			}
		}

		public static All GetGLTextureFilter(TextureFilter filter)
		{
			switch (filter) {
				case TextureFilter.Linear:
					return All.Linear;
				case TextureFilter.Nearest:
					return All.Nearest;
				default:
					throw new ArgumentException(nameof(filter));
			}
		}

		public static All GetGLTextureWrapMode(TextureWrapMode mode)
		{
			switch (mode) {
				case TextureWrapMode.Clamp:
					return All.ClampToEdge;
				case TextureWrapMode.Repeat:
					return All.Repeat;
				case TextureWrapMode.MirroredRepeat:
					return All.MirroredRepeat;
				default:
					throw new ArgumentException(nameof(mode));
			}
		}

		public static void GetGLVertexAttribFormat(Format format, out All glType, out int glSize, out bool glNormalized)
		{
			switch (format) {
				case Format.R8_SInt:
					glType = All.Byte;
					glSize = 1;
					glNormalized = false;
					break;
				case Format.R8_SNorm:
					glType = All.Byte;
					glSize = 1;
					glNormalized = true;
					break;
				case Format.R8_UInt:
					glType = All.UnsignedByte;
					glSize = 1;
					glNormalized = false;
					break;
				case Format.R8_UNorm:
					glType = All.UnsignedByte;
					glSize = 1;
					glNormalized = true;
					break;
				case Format.R8G8_SInt:
					glType = All.Byte;
					glSize = 2;
					glNormalized = false;
					break;
				case Format.R8G8_SNorm:
					glType = All.Byte;
					glSize = 2;
					glNormalized = true;
					break;
				case Format.R8G8_UInt:
					glType = All.UnsignedByte;
					glSize = 2;
					glNormalized = false;
					break;
				case Format.R8G8_UNorm:
					glType = All.UnsignedByte;
					glSize = 2;
					glNormalized = true;
					break;
				case Format.R8G8B8_SInt:
					glType = All.Byte;
					glSize = 3;
					glNormalized = false;
					break;
				case Format.R8G8B8_SNorm:
					glType = All.Byte;
					glSize = 3;
					glNormalized = true;
					break;
				case Format.R8G8B8_UInt:
					glType = All.UnsignedByte;
					glSize = 3;
					glNormalized = false;
					break;
				case Format.R8G8B8_UNorm:
					glType = All.UnsignedByte;
					glSize = 3;
					glNormalized = true;
					break;
				case Format.R8G8B8A8_SInt:
					glType = All.Byte;
					glSize = 4;
					glNormalized = false;
					break;
				case Format.R8G8B8A8_SNorm:
					glType = All.Byte;
					glSize = 4;
					glNormalized = true;
					break;
				case Format.R8G8B8A8_UInt:
					glType = All.UnsignedByte;
					glSize = 4;
					glNormalized = false;
					break;
				case Format.R8G8B8A8_UNorm:
					glType = All.UnsignedByte;
					glSize = 4;
					glNormalized = true;
					break;
				case Format.R32_SFloat:
					glType = All.Float;
					glSize = 1;
					glNormalized = false;
					break;
				case Format.R32G32_SFloat:
					glType = All.Float;
					glSize = 2;
					glNormalized = false;
					break;
				case Format.R32G32B32_SFloat:
					glType = All.Float;
					glSize = 3;
					glNormalized = false;
					break;
				case Format.R32G32B32A32_SFloat:
					glType = All.Float;
					glSize = 4;
					glNormalized = false;
					break;
				default:
					throw new ArgumentException(nameof(format));
			}
		}

		public static All GetGLBlendEquationMode(BlendFunc func)
		{
			switch (func) {
				case BlendFunc.Add:
					return All.FuncAdd;
				case BlendFunc.Subtract:
					return All.FuncSubtract;
				case BlendFunc.ReverseSubtract:
					return All.FuncReverseSubtract;
				default:
					throw new ArgumentException(nameof(func));
			}
		}

		public static All GetGLBlendFactor(Blend blend)
		{
			switch (blend) {
				case Blend.One:
					return All.One;
				case Blend.Zero:
					return All.Zero;
				case Blend.Factor:
					return All.ConstantColor;
				case Blend.SrcColor:
					return All.SrcColor;
				case Blend.SrcAlpha:
					return All.SrcAlpha;
				case Blend.SrcAlphaSaturation:
					return All.SrcAlphaSaturate;
				case Blend.DstColor:
					return All.DstColor;
				case Blend.DstAlpha:
					return All.DstAlpha;
				case Blend.InverseFactor:
					return All.OneMinusConstantColor;
				case Blend.InverseSrcColor:
					return All.OneMinusSrcColor;
				case Blend.InverseSrcAlpha:
					return All.OneMinusSrcAlpha;
				case Blend.InverseDstColor:
					return All.OneMinusDstColor;
				case Blend.InverseDstAlpha:
					return All.OneMinusDstAlpha;
				default:
					throw new ArgumentException(nameof(blend));
			}
		}

		public static All GetGLCompareFunc(CompareFunc func)
		{
			switch (func) {
				case CompareFunc.Always:
					return All.Always;
				case CompareFunc.Never:
					return All.Never;
				case CompareFunc.Less:
					return All.Less;
				case CompareFunc.LessEqual:
					return All.Lequal;
				case CompareFunc.Greater:
					return All.Greater;
				case CompareFunc.GreaterEqual:
					return All.Gequal;
				case CompareFunc.Equal:
					return All.Equal;
				case CompareFunc.NotEqual:
					return All.Notequal;
				default:
					throw new ArgumentException(nameof(func));
			}
		}

		public static All GetGLStencilOp(StencilOp op)
		{
			switch (op) {
				case StencilOp.Keep:
					return All.Keep;
				case StencilOp.Zero:
					return All.Zero;
				case StencilOp.Replace:
					return All.Replace;
				case StencilOp.Invert:
					return All.Invert;
				case StencilOp.Increment:
					return All.IncrWrap;
				case StencilOp.IncrementSaturation:
					return All.Incr;
				case StencilOp.Decrement:
					return All.DecrWrap;
				case StencilOp.DecrementSaturation:
					return All.Decr;
				default:
					throw new ArgumentException(nameof(op));
			}
		}

		public static All GetGLFrontFaceDirection(FrontFace face)
		{
			switch (face) {
				case FrontFace.CW:
					return All.Cw;
				case FrontFace.CCW:
					return All.Ccw;
				default:
					throw new ArgumentException(nameof(face));
			}
		}

		public static All GetGLCullFaceMode(CullMode mode)
		{
			switch (mode) {
				case CullMode.Back:
					return All.Back;
				case CullMode.Front:
					return All.Front;
				default:
					throw new ArgumentException(nameof(mode));
			}
		}

		public static All GetGLPrimitiveType(PrimitiveTopology topology)
		{
			switch (topology) {
				case PrimitiveTopology.TriangleList:
					return All.Triangles;
				case PrimitiveTopology.TriangleStrip:
					return All.TriangleStrip;
				default:
					throw new ArgumentException(nameof(topology));
			}
		}

		public static All GetGLDrawElementsType(IndexFormat format)
		{
			switch (format) {
				case IndexFormat.Index16Bits:
					return All.UnsignedShort;
				default:
					throw new ArgumentException(nameof(format));
			}
		}

		public static void ParseGLVersion(string s, out int major, out int minor, out bool esProfile)
		{
			var match = Regex.Match(s, @"OpenGL ES (\d+)\.(\d+).*");
			if (match.Success) {
				major = int.Parse(match.Groups[1].Value);
				minor = int.Parse(match.Groups[2].Value);
				esProfile = true;
				return;
			}
			match = Regex.Match(s, @"(\d+)\.(\d+).*");
			if (match.Success) {
				major = int.Parse(match.Groups[1].Value);
				minor = int.Parse(match.Groups[2].Value);
				esProfile = false;
				return;
			}
			throw new ArgumentException(nameof(s));
		}
	}

	internal class GLErrorException : System.Exception
	{
		public GLErrorException(ErrorCode errorCode) : base($"OpenGL error: {errorCode}")
		{
		}
	}
}
