using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class ModelMesh : ModelNode
	{
		private static Matrix44[] sharedBoneTransforms = new Matrix44[] { };
		private bool invalidBones;

		[ProtoMember(1)]
		public List<ModelSubmesh> Submeshes { get; private set; }

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
			Submeshes = new List<ModelSubmesh>();
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

		public override void Render()
		{
			if (SkipRender) {
				return;
			}
			//if (firsttimeRender) {
			//	firsttimeRender = false;
			//	var vp = GetViewport();
			//	foreach (var sm in Submeshes) {
			//		for (var i = 0; i < sm.Bones.Count; i++) {
			//			sm.Bones[i] = vp.Find<ModelNode>(sm.Bones[i].Id);
			//		}
			//	}
			//}
			ValidateBones();
			var world = GlobalTransform;
			var worldInverse = world.CalcInverted();
			if (sharedBoneTransforms.Length < Bones.Count) {
				sharedBoneTransforms = new Matrix44[Bones.Count];
			}
			for (var i = 0; i < Bones.Count; i++) {
				sharedBoneTransforms[i] = BoneBindPoseInverses[i] * Bones[i].GlobalTransform * worldInverse;
			}
			var worldViewProj = world * Renderer.Projection;
			foreach (var sm in Submeshes) {
				Renderer.ZTestEnabled = ZTestEnabled;
				Renderer.ZWriteEnabled = ZWriteEnabled;
				sm.Render(worldViewProj, sharedBoneTransforms);
				//Renderer.Projection = GlobalTransform * viewProjection;
				//PlatformRenderer.SetTexture(sm.Material.DiffuseTexture, 0);
				//PlatformRenderer.SetTexture(null, 1);
				//PlatformRenderer.SetShader(ShaderId.Diffuse, null);
				//PlatformRenderer.SetBlending(Blending.Alpha);
				//sm.Geometry.Render(0, sm.Geometry.Vertices.Length);
				//Renderer.Projection = viewProjection;
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
	public class ModelSubmesh
	{
		private static Matrix44[] sharedBoneTransforms = new Matrix44[] { };

		[ProtoMember(1)]
		public ModelMaterial Material { get; set; }

		[ProtoMember(2)]
		public Mesh Geometry { get; set; }

		[ProtoMember(3)]
		public List<int> BoneIndices { get; private set; }

		public ModelSubmesh()
		{
			BoneIndices = new List<int>();
		}

		public void Render(Matrix44 worldViewProj, Matrix44[] boneTransforms)
		{

			var materialExternals = new ModelMaterialExternals {
				WorldViewProj = worldViewProj
			};
			if (BoneIndices.Count > 0) {
				if (sharedBoneTransforms.Length < BoneIndices.Count) {
					sharedBoneTransforms = new Matrix44[BoneIndices.Count];
				}
				for (var i = 0; i < BoneIndices.Count; i++) {
					sharedBoneTransforms[i] = boneTransforms[BoneIndices[i]];
				}
				materialExternals.Caps |= ModelMaterialCap.Skin;
				materialExternals.Bones = sharedBoneTransforms;
				materialExternals.BoneCount = BoneIndices.Count;
			}
			Material.Apply(ref materialExternals);
			Geometry.Render(0, Geometry.Vertices.Length);
		}
	}
}