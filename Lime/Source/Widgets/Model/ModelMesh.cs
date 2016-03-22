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
		internal Matrix44 WorldViewProj;
		internal Matrix44[] SharedBoneTransforms = new Matrix44[] { };
		private bool invalidBones;

		[ProtoMember(1)]
		public ModelSubmeshCollection Submeshes { get; private set; }

		[ProtoMember(2)]
		public BoundingSphere BoundingSphere { get; set; }

		[ProtoMember(3)]
		public List<ModelNode> Bones { get; private set; }

		[ProtoMember(4)]
		public List<Matrix44> BoneBindPoseInverses { get; private set; }

		public bool ZTestEnabled { get; set; }
		public bool ZWriteEnabled { get; set; }
		public bool SkipRender { get; set; }

		public ModelMesh()
		{
			Submeshes = new ModelSubmeshCollection(this);
			Bones = new List<ModelNode>();
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
			WorldViewProj = world * Renderer.Projection;
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
						var validBone = skeletonRoot.TryFind<ModelNode>(Bones[i].Id);
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
				d = ray.Intersects(ref sphereInWorldSpace);
			}
			if (d.HasValue && d.Value < result.Distance) {
				result = new MeshHitTestResult() { Distance = d.Value, Mesh = this };
			}
			return result;
		}
	}

	[ProtoContract]
	public class ModelSubmesh : IModelRenderObject
	{
		private static Matrix44[] sharedBoneTransforms = new Matrix44[] { };

		[ProtoMember(1)]
		public ModelMaterial Material { get; set; }

		[ProtoMember(2)]
		public Mesh Geometry { get; set; }

		[ProtoMember(3)]
		public List<int> BoneIndices { get; private set; }

		public ModelMesh ModelMesh;

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

		public ModelSubmesh()
		{
			BoneIndices = new List<int>();
		}

		public void Render()
		{
			Renderer.ZTestEnabled = ModelMesh.ZTestEnabled;
			Renderer.ZWriteEnabled = ModelMesh.ZWriteEnabled;
			var materialExternals = new ModelMaterialExternals {
				WorldViewProj = ModelMesh.WorldViewProj
			};
			if (BoneIndices.Count > 0) {
				if (sharedBoneTransforms.Length < BoneIndices.Count) {
					sharedBoneTransforms = new Matrix44[BoneIndices.Count];
				}
				for (var i = 0; i < BoneIndices.Count; i++) {
					sharedBoneTransforms[i] = ModelMesh.SharedBoneTransforms[BoneIndices[i]];
				}
				materialExternals.Caps |= ModelMaterialCap.Skin;
				materialExternals.Bones = sharedBoneTransforms;
				materialExternals.BoneCount = BoneIndices.Count;
			}
			Material.Apply(ref materialExternals);
			Geometry.Render(0, Geometry.Indices.Length);
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