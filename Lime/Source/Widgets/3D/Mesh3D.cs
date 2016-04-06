using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class Mesh3D : Node3D
	{
		internal Matrix44 WorldViewProj;
		internal Matrix44[] SharedBoneTransforms = new Matrix44[] { };
		private bool invalidBones;

		[ProtoMember(1)]
		public Submesh3DCollection Submeshes { get; private set; }

		[ProtoMember(2)]
		public BoundingSphere BoundingSphere { get; set; }

		[ProtoMember(3)]
		public List<Node3D> Bones { get; private set; }

		[ProtoMember(4)]
		public List<Matrix44> BoneBindPoseInverses { get; private set; }

		public bool ZTestEnabled { get; set; }
		public bool ZWriteEnabled { get; set; }
		public bool SkipRender { get; set; }

		public Mesh3D()
		{
			Submeshes = new Submesh3DCollection(this);
			Bones = new List<Node3D>();
			BoneBindPoseInverses = new List<Matrix44>();
			ZTestEnabled = true;
			ZWriteEnabled = true;
		}

		[ProtoAfterDeserialization]
		public void AfterDeserialization()
		{
			invalidBones = true;
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

		internal void PrepareToRender()
		{
			ValidateBones();
			var world = GlobalTransform;
			var worldInverse = world.CalcInverted();
			if (SharedBoneTransforms.Length < Bones.Count) {
				SharedBoneTransforms = new Matrix44[Bones.Count];
			}
			for (var i = 0; i < Bones.Count; i++) {
				SharedBoneTransforms[i] = BoneBindPoseInverses[i] * Bones[i].GlobalTransform * worldInverse;
			}
			WorldViewProj = Renderer.FixupWVP(world * Renderer.Projection);
		}

		public override void Render()
		{
			if (SkipRender) {
				return;
			}
			PrepareToRender();
			foreach (var sm in Submeshes) {
				sm.Render();
			}
		}

		private void ValidateBones()
		{
			if (invalidBones) {
				var success = false;
				Node skeletonRoot = this;
				while (skeletonRoot != null && skeletonRoot.AsModelNode != null) {
					var i = 0;
					while (i < Bones.Count) {
						var validBone = skeletonRoot.TryFind<Node3D>(Bones[i].Id);
						if (validBone == null) {
							break;
						}
						Bones[i] = validBone;
						i++;
					}
					success = i == Bones.Count;
					if (success) {
						break;
					}
					skeletonRoot = skeletonRoot.Parent;
				}
				if (!success) {
					throw new Lime.Exception("Skeleton for `{0}` is not found", ToString());
				}
				invalidBones = false;
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
				d = ray.Intersects(sphereInWorldSpace);
			}
			if (d.HasValue && d.Value < result.Distance) {
				result = new MeshHitTestResult() { Distance = d.Value, Mesh = this };
			}
			return result;
		}

		internal override bool PerformHitTest(Ray ray, float distanceToNearNode, out float distance)
		{
			distance = default(float);
			if (!HitTestTarget) {
				return false;
			}
			if (!HitTestBoundingSphere(ray, out distance) || distance > distanceToNearNode) {
				return false;
			}
			if (!HitTestGeometry(ray, out distance) || distance > distanceToNearNode) {
				return false;
			}
			return true;
		}

		private bool HitTestBoundingSphere(Ray ray, out float distance)
		{
			distance = default(float);
			var boundingSphereInWorldSpace = BoundingSphere.Transform(GlobalTransform);
			var d = ray.Intersects(boundingSphereInWorldSpace);
			if (d != null) {
				distance = d.Value;
				return true;
			}
			return false;
		}

		private bool HitTestGeometry(Ray ray, out float distance)
		{
			var hit = false;
			distance = float.MaxValue;
			ray = ray.Transform(GlobalTransform.CalcInverted());
			foreach (var submesh in Submeshes) {
				var vertices = submesh.Geometry.Vertices;
				for (int i = 0; i <= vertices.Length - 3; i += 3) {
					var d = ray.IntersectsTriangle(vertices[i], vertices[i + 1], vertices[i + 2]);
					if (d != null && d.Value < distance) {
						distance = d.Value;
						hit = true;
					}
				}
			}
			return hit;
		}
	}

	[ProtoContract]
	public class Submesh3D : IRenderObject3D
	{
		private static Matrix44[] sharedBoneTransforms = new Matrix44[] { };
		
		[ProtoMember(1)]
		public Material Material { get; set; }

		[ProtoMember(2)]
		public GeometryBuffer Geometry { get; set; }

		[ProtoMember(3)]
		public List<int> BoneIndices { get; private set; }

		public Mesh3D ModelMesh;

		private Vector3? center;

		public Vector3 Center
		{
			get
			{
				if (center == null) {
					center = Vector3.Zero;
					int n = Geometry.Vertices.Length;
					for (int i = 0; i < n; i++) {
						center += Geometry.Vertices[i];
					}
					center /= n;
				}
				return center.Value * ModelMesh.GlobalTransform;
			}
		}

		public Submesh3D()
		{
			BoneIndices = new List<int>();
		}

		public void Render()
		{
			Renderer.ZTestEnabled = ModelMesh.ZTestEnabled;
			Renderer.ZWriteEnabled = ModelMesh.ZWriteEnabled;
			var materialExternals = new MaterialExternals {
				WorldViewProj = ModelMesh.WorldViewProj,
				ColorFactor = ModelMesh.GlobalColor
			};
			if (BoneIndices.Count > 0) {
				if (sharedBoneTransforms.Length < BoneIndices.Count) {
					sharedBoneTransforms = new Matrix44[BoneIndices.Count];
				}
				for (var i = 0; i < BoneIndices.Count; i++) {
					sharedBoneTransforms[i] = ModelMesh.SharedBoneTransforms[BoneIndices[i]];
				}
				materialExternals.Caps |= MaterialCap.Skin;
				materialExternals.Bones = sharedBoneTransforms;
				materialExternals.BoneCount = BoneIndices.Count;
			}
			Material.Apply(ref materialExternals);
			Geometry.Render(0, Geometry.Indices.Length);
		}
	}

	public class Submesh3DCollection : IList<Submesh3D>
	{
		private Mesh3D owner;
		private List<Submesh3D> list = new List<Submesh3D>();

		public Submesh3DCollection() { }
		public Submesh3DCollection(Mesh3D owner)
		{
			this.owner = owner;
		}

		public IEnumerator<Submesh3D> GetEnumerator()
		{
			return list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(Submesh3D item)
		{
			item.ModelMesh = owner;
			list.Add(item);
		}

		public void Clear()
		{
			list.Clear();
		}

		public bool Contains(Submesh3D item)
		{
			return list.Contains(item);
		}

		public void CopyTo(Submesh3D[] array, int arrayIndex)
		{
			list.CopyTo(array, arrayIndex);
		}

		public bool Remove(Submesh3D item)
		{
			return list.Remove(item);
		}

		public int Count { get { return list.Count; } }
		public bool IsReadOnly { get { return false; } }
		public int IndexOf(Submesh3D item)
		{
			return list.IndexOf(item);
		}

		public void Insert(int index, Submesh3D item)
		{
			list.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			list.RemoveAt(index);
		}

		public Submesh3D this[int index]
		{
			get { return list[index]; }
			set { list[index] = value; }
		}
	}
}