using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class ModelMesh : ModelNode
	{
		[ProtoMember(1)]
		public List<ModelSubmesh> Submeshes { get; private set; }

		[ProtoMember(2)]
		public BoundingSphere BoundingSphere { get; set; }

		public bool SkipRender;

		public ModelMesh()
		{
			Submeshes = new List<ModelSubmesh>();
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (!GloballyVisible) {
				return;
			}
			if (Layer != 0) {
				var oldLayer = chain.SetCurrentLayer(Layer);
				for (var node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
					node.AddToRenderChain(chain);
				}
				chain.Add(this);
				chain.SetCurrentLayer(oldLayer);
			} else {
				for (var node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
					node.AddToRenderChain(chain);
				}
				chain.Add(this);
			}
		}

		public override void Render()
		{
			if (SkipRender) {
				return;
			}
			foreach (var sm in Submeshes) {
				var viewProjection = Renderer.Projection;
				Renderer.Projection = GlobalTransform * viewProjection;
				PlatformRenderer.SetTexture(sm.Material.DiffuseTexture, 0);
				PlatformRenderer.SetTexture(null, 1);
				PlatformRenderer.SetShader(ShaderId.Diffuse, null);
				PlatformRenderer.SetBlending(Blending.Alpha);
				sm.Geometry.Render(0, sm.Geometry.Vertices.Length);
				Renderer.Projection = viewProjection;
			}
		}

		public override MeshHitTestResult HitTest(Ray ray)
		{
			if (!GloballyVisible) {
				return MeshHitTestResult.Default;
			}
			var result = base.HitTest(ray);
			var sphereInWorldSpace = BoundingSphere;
			sphereInWorldSpace.Center *= globalTransform;
			Vector3 scale = globalTransform.Scale;
			sphereInWorldSpace.Radius *= Math.Max(Math.Abs(scale.X), Math.Max(Math.Abs(scale.Y), Math.Abs(scale.Z)));
			var d = ray.Intersects(sphereInWorldSpace);
			if (d.HasValue && d.Value < result.Distance) {
				result = new MeshHitTestResult() { Distance = d.Value, Mesh = this };
			}
			return result;
		}
	}

	[ProtoContract]
	public class ModelSubmesh
	{
		[ProtoMember(1)]
		public ModelMaterial Material { get; set; }

		[ProtoMember(2)]
		public Mesh Geometry { get; set; }
	}
}