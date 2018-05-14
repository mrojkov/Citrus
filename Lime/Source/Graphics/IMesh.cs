using System;
using Yuzu;

namespace Lime
{
	public interface IMesh
	{
		IMesh ShallowClone();
		void Draw(int startVertex, int vertexCount);
		void DrawIndexed(int startIndex, int indexCount, int baseVertex = 0);
	}

	public enum MeshDirtyFlags
	{
		None = 0,
		Vertices = 1 << 0,
		Indices = 1 << 1,
		AttributeLocations = 1 << 2,
		VerticesIndices = Vertices | Indices,
		All = Vertices | Indices | AttributeLocations,
	}
}
