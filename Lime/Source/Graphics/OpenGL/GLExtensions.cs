#if OPENGL
using System;

#if iOS || ANDROID || WIN
using OpenTK.Graphics.ES20;
using GLStencilOp = OpenTK.Graphics.ES20.StencilOp;
#else
using OpenTK.Graphics.OpenGL;
using GLStencilOp = OpenTK.Graphics.OpenGL.StencilOp;
#endif

#if iOS || ANDROID
using PrimitiveType = OpenTK.Graphics.ES20.BeginMode;
#elif MAC || MONOMAC
using PrimitiveType = OpenTK.Graphics.OpenGL.BeginMode;
#endif

namespace Lime
{
	internal static class GLExtensions
	{
		public static BlendEquationMode ToGLBlendEquationMode(this BlendFunc value)
		{
			switch (value) {
				case BlendFunc.Add:
					return BlendEquationMode.FuncAdd;
				case BlendFunc.Subtract:
					return BlendEquationMode.FuncSubtract;
				case BlendFunc.ReverseSubtract:
					return BlendEquationMode.FuncReverseSubtract;
				default:
					throw new NotSupportedException();
			}
		}

		public static BlendingFactorSrc ToGLBlendingFactorSrc(this Blend value)
		{
			switch (value) {
				case Blend.One:
					return (BlendingFactorSrc)All.One;
				case Blend.Zero:
					return (BlendingFactorSrc)All.Zero;
				case Blend.Factor:
					return (BlendingFactorSrc)All.ConstantColor;
				case Blend.SrcColor:
					return (BlendingFactorSrc)All.SrcColor;
				case Blend.SrcAlpha:
					return (BlendingFactorSrc)All.SrcAlpha;
				case Blend.SrcAlphaSaturation:
					return (BlendingFactorSrc)All.SrcAlphaSaturate;
				case Blend.DstColor:
					return (BlendingFactorSrc)All.DstColor;
				case Blend.DstAlpha:
					return (BlendingFactorSrc)All.DstAlpha;
				case Blend.InverseFactor:
					return (BlendingFactorSrc)All.OneMinusConstantColor;
				case Blend.InverseSrcColor:
					return (BlendingFactorSrc)All.OneMinusSrcColor;
				case Blend.InverseSrcAlpha:
					return (BlendingFactorSrc)All.OneMinusSrcAlpha;
				case Blend.InverseDstColor:
					return (BlendingFactorSrc)All.OneMinusDstColor;
				case Blend.InverseDstAlpha:
					return (BlendingFactorSrc)All.OneMinusDstAlpha;
				default:
					throw new NotSupportedException();
			}
		}

		public static BlendingFactorDest ToGLBlendingFactorDest(this Blend value)
		{
			switch (value) {
				case Blend.One:
					return (BlendingFactorDest)All.One;
				case Blend.Zero:
					return (BlendingFactorDest)All.Zero;
				case Blend.Factor:
					return (BlendingFactorDest)All.ConstantColor;
				case Blend.SrcColor:
					return (BlendingFactorDest)All.SrcColor;
				case Blend.SrcAlpha:
					return (BlendingFactorDest)All.SrcAlpha;
				case Blend.SrcAlphaSaturation:
					return (BlendingFactorDest)All.SrcAlphaSaturate;
				case Blend.DstColor:
					return (BlendingFactorDest)All.DstColor;
				case Blend.DstAlpha:
					return (BlendingFactorDest)All.DstAlpha;
				case Blend.InverseFactor:
					return (BlendingFactorDest)All.OneMinusConstantColor;
				case Blend.InverseSrcColor:
					return (BlendingFactorDest)All.OneMinusSrcColor;
				case Blend.InverseSrcAlpha:
					return (BlendingFactorDest)All.OneMinusSrcAlpha;
				case Blend.InverseDstColor:
					return (BlendingFactorDest)All.OneMinusDstColor;
				case Blend.InverseDstAlpha:
					return (BlendingFactorDest)All.OneMinusDstAlpha;
				default:
					throw new NotSupportedException();
			}
		}

		public static DepthFunction ToGLDepthFunction(this CompareFunc value)
		{
			switch (value) {
				case CompareFunc.Always:
					return DepthFunction.Always;
				case CompareFunc.Never:
					return DepthFunction.Never;
				case CompareFunc.Less:
					return DepthFunction.Less;
				case CompareFunc.LessEqual:
					return DepthFunction.Lequal;
				case CompareFunc.Greater:
					return DepthFunction.Greater;
				case CompareFunc.GreaterEqual:
					return DepthFunction.Gequal;
				case CompareFunc.Equal:
					return DepthFunction.Equal;
				case CompareFunc.NotEqual:
					return DepthFunction.Notequal;
				default:
					throw new NotSupportedException();
			}
		}

		public static StencilFunction ToGLStencilFunction(this CompareFunc value)
		{
			switch (value) {
				case CompareFunc.Always:
					return StencilFunction.Always;
				case CompareFunc.Never:
					return StencilFunction.Never;
				case CompareFunc.Less:
					return StencilFunction.Less;
				case CompareFunc.LessEqual:
					return StencilFunction.Lequal;
				case CompareFunc.Greater:
					return StencilFunction.Greater;
				case CompareFunc.GreaterEqual:
					return StencilFunction.Gequal;
				case CompareFunc.Equal:
					return StencilFunction.Equal;
				case CompareFunc.NotEqual:
					return StencilFunction.Notequal;
				default:
					throw new NotSupportedException();
			}
		}

		public static GLStencilOp ToGLStencilOp(this StencilOp value)
		{
			switch (value) {
				case StencilOp.Keep:
					return GLStencilOp.Keep;
				case StencilOp.Zero:
					return GLStencilOp.Zero;
				case StencilOp.Replace:
					return GLStencilOp.Replace;
				case StencilOp.Increment:
					return GLStencilOp.IncrWrap;
				case StencilOp.IncrementSaturation:
					return GLStencilOp.Incr;
				case StencilOp.Decrement:
					return GLStencilOp.DecrWrap;
				case StencilOp.DecrementSaturation:
					return GLStencilOp.Decr;
				case StencilOp.Invert:
					return GLStencilOp.Invert;
				default:
					throw new NotSupportedException();
			}
		}

		public static FrontFaceDirection ToGLFrontFaceDirection(this FrontFace value)
		{
			switch (value) {
				case FrontFace.CW:
					return FrontFaceDirection.Cw;
				case FrontFace.CCW:
					return FrontFaceDirection.Ccw;
				default:
					throw new NotSupportedException();
			}
		}

		public static DrawElementsType ToGLDrawElementsType(this IndexFormat value)
		{
			switch (value) {
				case IndexFormat.Index16Bits:
					return DrawElementsType.UnsignedShort;
				default:
					throw new NotSupportedException();
			}
		}

		public static PrimitiveType ToGLPrimitiveType(this PrimitiveTopology value)
		{
			switch (value) {
				case PrimitiveTopology.TriangleList:
					return PrimitiveType.Triangles;
				case PrimitiveTopology.TriangleStrip:
					return PrimitiveType.TriangleStrip;
				default:
					throw new NotSupportedException();
			}
		}

		public static GLVertexAttributeFormat ToGLVertexAttributeFormat(this VertexInputElementFormat value)
		{
			switch (value) {
				case VertexInputElementFormat.Byte1:
					return new GLVertexAttributeFormat(VertexAttribPointerType.Byte, 1, false);
				case VertexInputElementFormat.Byte1Norm:
					return new GLVertexAttributeFormat(VertexAttribPointerType.Byte, 1, true);
				case VertexInputElementFormat.Byte2:
					return new GLVertexAttributeFormat(VertexAttribPointerType.Byte, 2, false);
				case VertexInputElementFormat.Byte2Norm:
					return new GLVertexAttributeFormat(VertexAttribPointerType.Byte, 2, true);
				case VertexInputElementFormat.Byte4:
					return new GLVertexAttributeFormat(VertexAttribPointerType.Byte, 4, false);
				case VertexInputElementFormat.Byte4Norm:
					return new GLVertexAttributeFormat(VertexAttribPointerType.Byte, 4, true);
				case VertexInputElementFormat.Short1:
					return new GLVertexAttributeFormat(VertexAttribPointerType.Short, 1, false);
				case VertexInputElementFormat.Short1Norm:
					return new GLVertexAttributeFormat(VertexAttribPointerType.Short, 1, true);
				case VertexInputElementFormat.Short2:
					return new GLVertexAttributeFormat(VertexAttribPointerType.Short, 2, false);
				case VertexInputElementFormat.Short2Norm:
					return new GLVertexAttributeFormat(VertexAttribPointerType.Short, 2, true);
				case VertexInputElementFormat.Short4:
					return new GLVertexAttributeFormat(VertexAttribPointerType.Short, 4, false);
				case VertexInputElementFormat.Short4Norm:
					return new GLVertexAttributeFormat(VertexAttribPointerType.Short, 4, true);
				case VertexInputElementFormat.UByte1:
					return new GLVertexAttributeFormat(VertexAttribPointerType.UnsignedByte, 1, false);
				case VertexInputElementFormat.UByte1Norm:
					return new GLVertexAttributeFormat(VertexAttribPointerType.UnsignedByte, 1, true);
				case VertexInputElementFormat.UByte2:
					return new GLVertexAttributeFormat(VertexAttribPointerType.UnsignedByte, 2, false);
				case VertexInputElementFormat.UByte2Norm:
					return new GLVertexAttributeFormat(VertexAttribPointerType.UnsignedByte, 2, true);
				case VertexInputElementFormat.UByte4:
					return new GLVertexAttributeFormat(VertexAttribPointerType.UnsignedByte, 4, false);
				case VertexInputElementFormat.UByte4Norm:
					return new GLVertexAttributeFormat(VertexAttribPointerType.UnsignedByte, 4, true);
				case VertexInputElementFormat.UShort1:
					return new GLVertexAttributeFormat(VertexAttribPointerType.UnsignedShort, 1, false);
				case VertexInputElementFormat.UShort1Norm:
					return new GLVertexAttributeFormat(VertexAttribPointerType.UnsignedShort, 1, true);
				case VertexInputElementFormat.UShort2:
					return new GLVertexAttributeFormat(VertexAttribPointerType.UnsignedShort, 2, false);
				case VertexInputElementFormat.UShort2Norm:
					return new GLVertexAttributeFormat(VertexAttribPointerType.UnsignedShort, 2, true);
				case VertexInputElementFormat.UShort4:
					return new GLVertexAttributeFormat(VertexAttribPointerType.UnsignedShort, 4, false);
				case VertexInputElementFormat.UShort4Norm:
					return new GLVertexAttributeFormat(VertexAttribPointerType.UnsignedShort, 4, true);
				case VertexInputElementFormat.Float1:
					return new GLVertexAttributeFormat(VertexAttribPointerType.Float, 1, false);
				case VertexInputElementFormat.Float2:
					return new GLVertexAttributeFormat(VertexAttribPointerType.Float, 2, false);
				case VertexInputElementFormat.Float3:
					return new GLVertexAttributeFormat(VertexAttribPointerType.Float, 3, false);
				case VertexInputElementFormat.Float4:
					return new GLVertexAttributeFormat(VertexAttribPointerType.Float, 4, false);
				default:
					throw new NotSupportedException();
			}
		}
	}

	internal struct GLVertexAttributeFormat
	{
		public VertexAttribPointerType Type;
		public int NumberOfElements;
		public bool Normalized;

		public GLVertexAttributeFormat(VertexAttribPointerType type, int numberOfElements, bool normalized)
		{
			Type = type;
			NumberOfElements = numberOfElements;
			Normalized = normalized;
		}
	}
}

#endif