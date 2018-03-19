#if OPENGL
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if iOS || ANDROID || WIN
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
#endif
using Yuzu;

namespace Lime
{
	internal interface IGLVertexBuffer
	{
		void SetAttribPointers(int[] attributes = null);
		void BufferData();
	}
	
	public class VertexBuffer<T> : IVertexBuffer<T>, IGLVertexBuffer, IGLObject where T: struct
	{
		static AttributeDescription[] descriptions = GetDescriptions().ToArray();
#if WIN || MONOMAC
		static int stride = Marshal.SizeOf(typeof(T));
#else
		static int stride = Marshal.SizeOf<T>();
#endif
		private uint vboHandle;
		private bool disposed;

		public bool Dynamic { get; set; }
		public bool Dirty { get; set; }

		public T[] Data { get; set; }

		public VertexBuffer()
		{
			Dirty = true;
			GLObjectRegistry.Instance.Add(this);
		}

		private static IEnumerable<AttributeDescription> GetDescriptions()
		{
			int offset = 0;
			var result = GetAttributeDescription(typeof(T), ref offset);
			if (result != null) {
				yield return result;
				yield break;
			}
			foreach (var field in typeof(T).GetFields()) {
				var attrs = field.GetCustomAttributes(typeof(FieldOffsetAttribute), false);
				if (attrs.Length > 0) {
					offset = (attrs[0] as FieldOffsetAttribute).Value;
				}
				result = GetAttributeDescription(field.FieldType, ref offset);
				if (result != null) {
					yield return result;
				} else {
					throw new InvalidOperationException();
				}
			}
		}
		
		private static AttributeDescription GetAttributeDescription(Type type, ref int offset)
		{
			AttributeDescription result = null;
			if (type == typeof(float)) {
				result = new AttributeDescription { AttributeType = VertexAttribPointerType.Float, ComponentCount = 1, Offset = offset };
				offset += 4;
			} else if (type == typeof(Vector2)) {
				result = new AttributeDescription { AttributeType = VertexAttribPointerType.Float, ComponentCount = 2, Offset = offset };
				offset += 8;
			} else if (type == typeof(Vector3)) {
				result = new AttributeDescription { AttributeType = VertexAttribPointerType.Float, ComponentCount = 3, Offset = offset };
				offset += 12;
			} else if (type == typeof(Color4)) {
				result = new AttributeDescription { AttributeType = VertexAttribPointerType.UnsignedByte, ComponentCount = 4, Normalized = true, Offset = offset };
				offset += 4;
			} else if (type == typeof(Mesh3D.BlendIndices)) {
				result = new AttributeDescription { AttributeType = VertexAttribPointerType.UnsignedByte, ComponentCount = 4, Offset = offset };
				offset += 4;
			} else if (type == typeof(Mesh3D.BlendWeights)) {
				result = new AttributeDescription { AttributeType = VertexAttribPointerType.Float, ComponentCount = 4, Offset = offset };
				offset += 16;
			}
			return result;
		}

		~VertexBuffer()
		{
			Dispose();
		}

		private void AllocateHandle()
		{
			var t = new int[1];
			GL.GenBuffers(1, t);
			vboHandle = (uint)t[0];
		}

		void IGLVertexBuffer.SetAttribPointers(int[] attributes)
		{
			if (vboHandle == 0) {
				AllocateHandle();
			}
			GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
			int i = 0;
			foreach (var d in descriptions) {
				var attribute = attributes[i++];
				GL.EnableVertexAttribArray(attribute);
				GL.VertexAttribPointer(attribute, d.ComponentCount, d.AttributeType, d.Normalized, stride, (IntPtr)d.Offset);
			}
		}
		
		void IGLVertexBuffer.BufferData()
		{
			if (Dirty) {
				Dirty = false;
				var usageHint = Dynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw;
				GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
				GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(stride * Data.Length), Data, usageHint);
			}
		}

		public void Dispose()
		{
			if (!disposed) {
				Discard();
				disposed = true;
			}
			GC.SuppressFinalize(this);
		}

		public void Discard()
		{
			if (vboHandle != 0) {
				var capturedVboHandle = vboHandle;
				Window.Current.InvokeOnRendering(() => {
#if !MAC && !MONOMAC
					if (OpenTK.Graphics.GraphicsContext.CurrentContext == null)
						return;
#endif
					GL.DeleteBuffers(1, new uint[] { capturedVboHandle });
				});
				vboHandle = 0;
			}
		}
		
		class AttributeDescription
		{
			public VertexAttribPointerType AttributeType;
			public int ComponentCount;
			public bool Normalized;
			public int Offset;
		}
	}
}
#endif