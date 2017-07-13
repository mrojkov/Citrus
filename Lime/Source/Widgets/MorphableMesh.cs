using System;
using System.Linq;
using System.Collections.Generic;
using Yuzu;
using System.Runtime.InteropServices;

namespace Lime
{
	public class DiscreteMorphableMesh : Node
	{
		public List<MorphableMesh> Meshes { get; private set; } = new List<MorphableMesh>();

		private int currMeshIndex;

		public DiscreteMorphableMesh()
		{
			Presenter = DefaultPresenter.Instance;
		}

		public override Node Clone()
		{
			var clone = (DiscreteMorphableMesh)base.Clone();
			clone.Meshes = new List<MorphableMesh>();
			foreach (var m in Meshes) {
				clone.Meshes.Add(m.Clone());
			}
			return clone;
		}

		public override void Render()
		{
			var parentWidget = Parent.AsWidget;
			var animationTime = parentWidget.AnimationTime;
			MorphableMesh mesh = null;
			while (true) {
				mesh = Meshes[currMeshIndex];
				if (animationTime < mesh.LeftBound) {
					if (currMeshIndex == 0)
						break;
					currMeshIndex--;
				} else if (animationTime < mesh.RightBound) {
					break;
				} else {
					currMeshIndex++;
				}
			}
			mesh.Render(parentWidget);
		}

		public void RefreshMeshBounds()
		{
			for (int i = 0; i < Meshes.Count; i++) {
				Meshes[i].LeftBound = Meshes[i].MorphTargets[0].Timestamp;
				Meshes[i].RightBound = i < Meshes.Count - 1 ? Meshes[i + 1].MorphTargets[0].Timestamp : int.MaxValue;
			}
		}
	}

	public class MorphableMesh
	{
		[YuzuMember]
		public IndexBuffer IndexBuffer { get; set; }

		[YuzuMember]
		public IVertexBuffer<Vector2> UVBuffer { get; set; }

		[YuzuMember]
		public readonly List<MorphTarget> MorphTargets = new List<MorphTarget>();

		[YuzuMember]
		public readonly List<RenderBatch> Batches = new List<RenderBatch>();

		[YuzuMember]
		public double LeftBound { get; set; }

		[YuzuMember]
		public double RightBound { get; set; }

		private int currentTarget;

		private static ShaderProgram shaderProgram;
		private static int morphKoeffUid;
		private static int globalColorUid;
		private static int globalTransformUid;
		private static Color4 loadedGlobalColor;

		public MorphableMesh Clone()
		{
			return (MorphableMesh)MemberwiseClone();
		}

		public void Render(Widget parentWidget)
		{
			Renderer.Flush();
			var animationTime = parentWidget.AnimationTime;
			// Find the morph targets to interpolate in between.
			MorphTarget target0, target1;
			if (MorphTargets.Count == 1) {
				target0 = target1 = MorphTargets[0];
			} else {
				while (true) {
					target0 = MorphTargets[currentTarget];
					target1 = MorphTargets[currentTarget + 1];
					if (animationTime < target0.Timestamp) {
						if (currentTarget == 0) {
							break;
						}
						currentTarget--;
					} else if (animationTime <= target1.Timestamp) {
						break;
					} else if (currentTarget >= MorphTargets.Count - 2) {
						// We got across the rightmost target.
						target0 = target1;
						break;
					} else {
						currentTarget++;
					}
				}
			}
			// Create the mesh if there is no any.
			if (target0.Mesh == null) {
				target0.Mesh = new Mesh {
					VertexBuffers = new IVertexBuffer[] { UVBuffer, target0.PosColorBuffer, target1.PosColorBuffer },
					Attributes = new[] {
						new[] { ShaderPrograms.Attributes.UV1 },
						new[] { ShaderPrograms.Attributes.Pos1, ShaderPrograms.Attributes.Color1 },
						new[] { ShaderPrograms.Attributes.Pos2, ShaderPrograms.Attributes.Color2 }
					},
					IndexBuffer = IndexBuffer
				};
			}
			// Calculate the interpolation koefficient.
			float t;
			if (animationTime <= target0.Timestamp) {
				t = 0;
			} else if (animationTime >= target1.Timestamp) {
				t = 1;
			} else {
				t = (float)((animationTime - target0.Timestamp) / (target1.Timestamp - target0.Timestamp));
			}
			// Render all of it.
			if (shaderProgram == null) {
				var options = ShaderOptions.VertexAnimation;
				shaderProgram = ShaderPrograms.Instance.GetShaderProgram(ShaderId.Diffuse, 1, options);
				morphKoeffUid = shaderProgram.GetUniformId("morphKoeff");
				globalColorUid = shaderProgram.GetUniformId("globalColor");
				globalTransformUid = shaderProgram.GetUniformId("globalTransform");
			}
			PlatformRenderer.SetBlending(Blending.Alpha);
			PlatformRenderer.SetShaderProgram(shaderProgram);
			var globalColor = parentWidget.GlobalColor;
			shaderProgram.LoadFloat(morphKoeffUid, t);
			if (loadedGlobalColor.ABGR != globalColor.ABGR) {
				loadedGlobalColor = globalColor;
				var globalColorVec4 = (globalColor.ABGR == 0xFFFFFFFF) ?
					Vector4.One : new Vector4(
						globalColor.R / 255f,
						globalColor.G / 255f,
						globalColor.B / 255f,
						globalColor.A / 255f);
				shaderProgram.LoadVector4(globalColorUid, globalColorVec4);
			}
			shaderProgram.LoadMatrix(globalTransformUid, (Matrix44)parentWidget.LocalToWorldTransform);
			foreach (var sm in Batches) {
				PlatformRenderer.SetTexture(sm.Texture, 0);
				PlatformRenderer.DrawTriangles(target0.Mesh, sm.StartIndex, sm.IndexCount);
			}
		}

		[StructLayout(LayoutKind.Explicit)] // Align structure size to 16 bytes.
		public struct PosColor
		{
			[FieldOffset(0)]
			[YuzuMember]
			public Vector2 Position;

			[FieldOffset(12)]
			[YuzuMember]
			public Color4 Color;
		}

		public class MorphTarget
		{
			[YuzuMember]
			public double Timestamp { get; set; }

			[YuzuMember]
			public IVertexBuffer<PosColor> PosColorBuffer { get; set; }

			public IMesh Mesh { get; set; }
		}

		public class RenderBatch
		{
			[YuzuMember]
			public ITexture Texture { get; set; }

			[YuzuMember]
			public Blending Blending { get; set; }

			[YuzuMember]
			public ShaderId Shader { get; set; }

			[YuzuMember]
			public int StartIndex { get; set; }

			[YuzuMember]
			public int IndexCount { get; set; }
		}
	}
}
