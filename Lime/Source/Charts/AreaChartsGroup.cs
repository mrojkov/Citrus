using System;

namespace Lime
{
	public class AreaChartsGroup<TEnum> : ChartsGroup where TEnum : Enum
	{
		private float[] accumulatedHeights;

		public AreaChartsGroup(int controlPointsCount) : base(Enum.GetNames(typeof(TEnum)), controlPointsCount)
		{
			accumulatedHeights = new float[controlPointsCount];
		}

		public void SetActive(TEnum index, bool value) => SetActive(Convert.ToInt32(index), value);

		public override void PushControlPoints(float[] points)
		{
			float maxY = 0;
			int submeshIndex = 0;
			foreach (var submesh in submeshes) {
				for (int i = 0; i < controlPointsCount - 1; i++) {
					submesh.Points[i] = submesh.Points[i + 1];
				}
				maxY = Mathf.Max(maxY, points[submeshIndex]);
				submesh.Points[controlPointsCount - 1] = points[submeshIndex];
				submeshIndex++;
			}
		}

		public override void UpdateVertices()
		{
			for (int i = 0; i < accumulatedHeights.Length; i++) {
				accumulatedHeights[i] = 0;
			}
			int vi = ChartsVerticesOffset;
			int parity = 0;
			foreach (var submesh in submeshes) {
				if (submesh.Enable) {
					int step = (1 - parity) * 2 - 1;
					int j = parity * (submesh.Points.Length - 1);
					for (int i = vi; i < vi + submeshVerticesCount; i += 2, j += step) {
						float ah = accumulatedHeights[j];
						float point = submesh.Points[j];
						accumulatedHeights[j] += point;

						float x = ControlPointsSpacing * j;
						vertices[i].X = x;
						vertices[i].Y = ChartMaxHeight - (ah + point * parity);
						vertices[i].Z = submesh.ColorIndex;
						vertices[i + 1].X = x;
						vertices[i + 1].Y = ChartMaxHeight - (ah + point * (1 - parity));
						vertices[i + 1].Z = submesh.ColorIndex;
					}
					vi += submeshVerticesCount;
					parity = (parity + 1) % 2;
				}
			}
			Mesh.Vertices = vertices;
			Mesh.DirtyFlags |= MeshDirtyFlags.Vertices;
		}
	}
}
