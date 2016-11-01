using System;
using System.Linq;
using Lime;

namespace Tangerine.Core.Operations
{
	public class DistortionMeshProcessor : IOperationProcessor
	{
		public void Do(IOperation op)
		{
			var mesh = 
				(op as InsertNode)?.Node as DistortionMesh ??
				(op as SetProperty)?.Obj as DistortionMesh;
			if (mesh != null)
				RestorePointsIfNeeded(mesh);
		}

		public void Redo(IOperation op) { }
		public void Undo(IOperation op) { }

		void RestorePointsIfNeeded(DistortionMesh mesh)
		{
			if (ValidateMeshPoints(mesh))
				return;
			foreach (var point in mesh.Nodes.ToList()) {
				UnlinkNode.Perform(point);
			}
			for (int i = 0; i <= mesh.NumRows; i++) {
				for (int j = 0; j <= mesh.NumCols; j++) {
					var pos = new Vector2((float)j / mesh.NumCols, (float)i / mesh.NumRows);
					var point = new DistortionMeshPoint {
						Id = $"{i};{j}",
						Color = Color4.White,
						UV = pos,
						Position = pos
					};
					InsertNode.Perform(mesh, mesh.Nodes.Count, point);
				}
			}
		}

		bool ValidateMeshPoints(DistortionMesh mesh)
		{
			if ((mesh.NumRows + 1) * (mesh.NumCols + 1) != mesh.Nodes.Count)
				return false;
			int t = 0;
			for (int i = 0; i <= mesh.NumRows; i++) {
				for (int j = 0; j <= mesh.NumCols; j++) {
					if (t >= mesh.Nodes.Count || mesh.Nodes[t].Id != $"{i};{j}")
						return false;
					t++;
				}
			}
			return true;
		}
	}
}
