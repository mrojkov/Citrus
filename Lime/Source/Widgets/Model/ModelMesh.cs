using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class ModelMesh : ModelNode
	{
		[ProtoMember(1)]
		public ModelSubmeshCollection Submeshes { get; private set; }

		[ProtoMember(2)]
		public BoundingSphere BoundingSphere { get; set; }

		public bool ZTestEnabled { get; set; }
		public bool ZWriteEnabled { get; set; }
		public bool SkipRender { get; set; }

		public ModelMesh()
		{
			Submeshes = new ModelSubmeshCollection(this);
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
				RenderSubmesh(sm);
			}
		}

		public void RenderSubmesh(ModelSubmesh sm)
		{
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

		public ModelMesh ModelMesh;

		private Vector3? center;
		public Vector3 Center {
			get {
				if (center == null) {
					center = Vector3.Zero;
					int n = Geometry.Vertices.Length;
					for (int i = 0; i < n; i++) {
						center += Geometry.Vertices[i];
					}
					center /= n;
				}
				return center.Value;
			}
			private set { center = value; }
		}
	}

	public class ModelSubmeshCollection : IList<ModelSubmesh>
	{
		private ModelMesh owner;
		private List<ModelSubmesh> list = new List<ModelSubmesh>();

		public ModelSubmeshCollection() { }
		public ModelSubmeshCollection(ModelMesh owner)
		{
			this.owner = owner;
		}

		public IEnumerator<ModelSubmesh> GetEnumerator()
		{
			return list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(ModelSubmesh item)
		{
			item.ModelMesh = owner;
			list.Add(item);
		}

		public void Clear()
		{
			list.Clear();
		}

		public bool Contains(ModelSubmesh item)
		{
			return list.Contains(item);
		}

		public void CopyTo(ModelSubmesh[] array, int arrayIndex)
		{
			list.CopyTo(array, arrayIndex);
		}

		public bool Remove(ModelSubmesh item)
		{
			return list.Remove(item);
		}

		public int Count { get { return list.Count; } }
		public bool IsReadOnly { get { return false; } }
		public int IndexOf(ModelSubmesh item)
		{
			return list.IndexOf(item);
		}

		public void Insert(int index, ModelSubmesh item)
		{
			list.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			list.RemoveAt(index);
		}

		public ModelSubmesh this[int index]
		{
			get { return list[index]; }
			set { list[index] = value; }
		}
	}
}