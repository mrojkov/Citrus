using System;
using Yuzu;

namespace Lime
{
	public interface IMesh
	{
		[YuzuMember]
		IIndexBuffer IndexBuffer { get; set; }

		[YuzuMember]
		IVertexBuffer[] VertexBuffers { get; set; }

		// FIXME: Yuzu doesn't support jagged arrays for now
		// [YuzuMember]
		int[][] Attributes { get; set; }
		
		IMesh ShallowClone();
	}
	
	public interface IVertexBuffer : IDisposable
	{
		[YuzuMember]
		bool Dynamic { get; set; }
		
		bool Dirty { get; set; }
	}

	public interface IVertexBuffer<T> : IVertexBuffer where T : struct
	{
		[YuzuMember]
		T[] Data { get; set; }
	}
	
	public interface IIndexBuffer : IDisposable
	{
		[YuzuMember]
		bool Dynamic { get; set; }

		[YuzuMember]
		ushort[] Data { get; set; }

		bool Dirty { get; set; }
	}
}
