using System;
using System.Linq;
using System.Collections.Generic;

namespace Lime
{
	public class DiscreteMorphableMesh : Node
	{
		public readonly List<MorphableMesh> Meshes = new List<MorphableMesh>();

		public override void Render()
		{
			var parentWidget = Parent.AsWidget;
			for (int i = 0; i < Meshes.Count; i++) {
				var mesh = Meshes[i];
				var nextMeshTimestamp = i < Meshes.Count - 1 ? Meshes[i + 1].MorphTargets[0].Timestamp : int.MaxValue;
				if (parentWidget.AnimationTime < nextMeshTimestamp) {
					mesh.Render(parentWidget);
					break;
				}
			}
		}
	}

	public class MorphableMesh
	{
		public GeometryBuffer Geometry { get; set; }
		public readonly List<MorphTarget> MorphTargets = new List<MorphTarget>();
		public readonly List<RenderBatch> Batches = new List<RenderBatch>();

		public void Render(Widget parentWidget)
		{
			Renderer.Flush();
			var savedProjection = Renderer.Projection;
			try {
				Renderer.Projection = (Matrix44)parentWidget.LocalToWorldTransform * Renderer.Projection;
				var animationTime = parentWidget.AnimationTime;
				MorphTarget target0, target1;
				target0 = target1 = MorphTargets[MorphTargets.Count - 1]; 
				for (int i = 1; i < MorphTargets.Count; i++) {
					if (animationTime < MorphTargets[i].Timestamp) {
						target0 = MorphTargets[i - 1];
						target1 = MorphTargets[i];
						break;
					}
				}
				target0.Geometry.UploadVertices(0);
				target1.Geometry.UploadVertices(1);
				Geometry.UploadVertices(0);
				Geometry.UploadIndices();
				float t;
				if (animationTime <= target0.Timestamp) {
					t = 0;
				} else if (animationTime >= target1.Timestamp) {
					t = 1;
				} else {
					t = (animationTime - target0.Timestamp) / (float)(target1.Timestamp - target0.Timestamp);
				}
				var globalColor = parentWidget.GlobalColor;
				var globalColorVec4 = (globalColor.ABGR == 0xFFFFFFFF) ?
					Vector4.One : new Vector4(globalColor.R / 255f, globalColor.G / 255f, globalColor.B / 255f, globalColor.A / 255f);
				foreach (var sm in Batches) {
					PlatformRenderer.SetTexture(sm.Texture1, 0);
					PlatformRenderer.SetTexture(sm.Texture2, 1);
					var program = PlatformRenderer.SetShader(sm.Shader, null, ShaderFlags.VertexAnimation);
					var morphKoeffUid = program.GetUniformId("morphKoeff");
					PlatformRenderer.SetBlending(sm.Blending);
					program.LoadFloat(morphKoeffUid, t);
					var globalColorUid = program.GetUniformId("globalColor");
					program.LoadVector4(globalColorUid, globalColorVec4); 
					Geometry.Render(sm.StartIndex, sm.IndexCount, uploadVertices: false);
				}
			} finally {
				Renderer.Projection = savedProjection;
			}
		}

		public class MorphTarget
		{
			public GeometryBuffer Geometry { get; set; }
			public int Timestamp { get; set; }
		}

		public class RenderBatch
		{
			public ITexture Texture1 { get; set; }
			public ITexture Texture2 { get; set; }
			public Blending Blending { get; set; }
			public ShaderId Shader { get; set; }
			public int StartIndex { get; set; }
			public int IndexCount { get; set; }
		}
	}
}
