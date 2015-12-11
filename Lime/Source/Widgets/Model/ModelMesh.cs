using System;
using System.Collections.Generic;
using System.Linq;
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

		public bool ZTestEnabled { get; set; }
		public bool ZWriteEnabled { get; set; }
		public bool SkipRender { get; set; }

		public ModelMesh()
		{
			Submeshes = new List<ModelSubmesh>();
			ZTestEnabled = true;
			ZWriteEnabled = true;
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
				Renderer.ZTestEnabled = ZTestEnabled;
				Renderer.ZWriteEnabled = ZWriteEnabled;
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
			var result = base.HitTest(ray);
			//var sphereInWorldSpace = BoundingSphere;
			//sphereInWorldSpace.Center *= GlobalTransform;
			//Vector3 scale = GlobalTransform.Scale;
			//sphereInWorldSpace.Radius *= Math.Max(Math.Abs(scale.X), Math.Max(Math.Abs(scale.Y), Math.Abs(scale.Z)));

			float? d = null;
			if (HitTestTarget) {
				var sphereInWorldSpace = BoundingSphere.CreateFromPoints(Submeshes.SelectMany(sm => sm.Geometry.Vertices).Select(v => v * GlobalTransform));
				//sphereInWorldSpace = sphereInWorldSpace.Transform(GlobalTransform);
				d = ray.Intersects(ref sphereInWorldSpace);
			}
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