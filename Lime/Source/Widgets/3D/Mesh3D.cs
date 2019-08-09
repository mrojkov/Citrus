using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Yuzu;

namespace Lime
{
	public class Mesh3D : Node3D
	{
		[YuzuCompact]
		[StructLayout(LayoutKind.Sequential, Size = 16)]
		public struct BlendIndices : IEquatable<BlendIndices>
		{
			private float index0;
			private float index1;
			private float index2;
			private float index3;

			[YuzuMember("0")]
			public byte Index0 { get => (byte)index0; set => index0 = value; }

			[YuzuMember("1")]
			public byte Index1 { get => (byte)index1; set => index1 = value; }

			[YuzuMember("2")]
			public byte Index2 { get => (byte)index2; set => index2 = value; }

			[YuzuMember("3")]
			public byte Index3 { get => (byte)index3; set => index3 = value; }

			public bool Equals(BlendIndices other)
			{
				return index0 == other.index0 &&
					index1 == other.index1 &&
					index2 == other.index2 &&
					index3 == other.index3;
			}

			public override int GetHashCode()
			{
				unchecked {
					var hashCode = index0.GetHashCode();
					hashCode = (hashCode * 397) ^ index1.GetHashCode();
					hashCode = (hashCode * 397) ^ index2.GetHashCode();
					hashCode = (hashCode * 397) ^ index3.GetHashCode();
					return hashCode;
				}
			}
		}

		[YuzuCompact]
		[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
		public struct BlendWeights : IEquatable<BlendWeights>
		{
			[YuzuMember("0")]
			public float Weight0;

			[YuzuMember("1")]
			public float Weight1;

			[YuzuMember("2")]
			public float Weight2;

			[YuzuMember("3")]
			public float Weight3;

			public bool Equals(BlendWeights other)
			{
				return Weight0 == other.Weight0 &&
					Weight1 == other.Weight1 &&
					Weight2 == other.Weight2 &&
					Weight3 == other.Weight3;
			}

			public override int GetHashCode()
			{
				unchecked {
					var hashCode = Weight0.GetHashCode();
					hashCode = (hashCode * 397) ^ Weight1.GetHashCode();
					hashCode = (hashCode * 397) ^ Weight2.GetHashCode();
					hashCode = (hashCode * 397) ^ Weight3.GetHashCode();
					return hashCode;
				}
			}
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 48)]
		public struct Vertex : IEquatable<Vertex>
		{
			[YuzuMember("0")]
			public Vector3 Pos;

			[YuzuMember("1")]
			public Color4 Color;

			[YuzuMember("2")]
			public Vector2 UV1;

			[YuzuMember("3")]
			public BlendIndices BlendIndices;

			[YuzuMember("4")]
			public BlendWeights BlendWeights;

			[YuzuMember("5")]
			public Vector3 Normal;

			[YuzuMember("6")]
			public Vector3 Tangent;

			public bool Equals(Vertex other)
			{
				return Pos == other.Pos &&
					Color == other.Color &&
					UV1 == other.UV1 &&
					BlendIndices.Equals(other.BlendIndices) &&
					BlendWeights.Equals(other.BlendWeights) &&
					Normal == other.Normal &&
					Tangent == other.Tangent;
			}

			public override int GetHashCode()
			{
				unchecked {
					var hashCode = Pos.GetHashCode();
					hashCode = (hashCode * 397) ^ Color.GetHashCode();
					hashCode = (hashCode * 397) ^ UV1.GetHashCode();
					hashCode = (hashCode * 397) ^ BlendIndices.GetHashCode();
					hashCode = (hashCode * 397) ^ BlendWeights.GetHashCode();
					hashCode = (hashCode * 397) ^ Normal.GetHashCode();
					hashCode = (hashCode * 397) ^ Tangent.GetHashCode();
					return hashCode;
				}
			}
		}

		[TangerineIgnore]
		[YuzuMember]
		public Submesh3DCollection Submeshes { get; private set; }

		[YuzuMember]
		public BoundingSphere BoundingSphere { get; set; }

		[YuzuMember]
		public CullMode CullMode { get; set; }

		[YuzuMember]
		public Vector3 Center { get; set; }

		public SkinningMode skinningMode;

		[YuzuMember]
		public SkinningMode SkinningMode
		{
			get => skinningMode;
			set {
				if (skinningMode != value) {
					skinningMode = value;
				}
			}
		}

		public bool SkipRender { get; set; }

		public Vector3 GlobalCenter
		{
			get { return GlobalTransform.TransformVector(Center); }
		}

		public virtual Action Clicked { get; set; }

		public bool CastShadow { get; set; }
		public bool RecieveShadow { get; set; }
		public bool ProcessLightning { get; set; }

		public Mesh3D()
		{
			Presenter = DefaultPresenter.Instance;
			Submeshes = new Submesh3DCollection(this);
			CullMode = CullMode.Front;
		}

		internal protected override bool PartialHitTest (ref HitTestArgs args)
		{
			float distance;
			if (!HitTestTarget) {
				return false;
			}
			if (!HitTestBoundingSphere(args.Ray, out distance) || distance > args.Distance) {
				return false;
			}
			if (!HitTestGeometry(args.Ray, out distance) || distance > args.Distance) {
				return false;
			}
			args.Node = this;
			args.Distance = distance;
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
			ray = ray.Transform(GlobalTransformInverse);
			foreach (var submesh in Submeshes) {
				var vertices = submesh.Mesh.Vertices;
				for (int i = 0; i <= vertices.Length - 3; i += 3) {
					var d = ray.IntersectsTriangle(vertices[i].Pos, vertices[i + 1].Pos, vertices[i + 2].Pos);
					if (d != null && d.Value < distance) {
						distance = d.Value;
						hit = true;
					}
				}
			}
			return hit;
		}

		private float CalcDistanceToCamera(Camera3D camera)
		{
			return camera.View.TransformVector(GlobalCenter).Z;
		}

		public void RecalcBounds()
		{
			BoundingSphere = BoundingSphere.CreateFromPoints(GetVertexPositions());
		}

		public void RecalcCenter()
		{
			Center = Vector3.Zero;
			var n = 0;
			foreach (var vp in GetVertexPositions()) {
				Center += vp;
				n++;
			}
			Center /= n;
		}

		private IEnumerable<Vector3> GetVertexPositions()
		{
			foreach (var sm in Submeshes) {
				foreach (var v in sm.Mesh.Vertices) {
					yield return v.Pos;
				}
			}
		}

		public override void Update(float delta)
		{
			base.Update(delta);

			if (Clicked != null) {
				HandleClick();
			}
		}

		private void HandleClick()
		{
			if (!CommonWindow.Current.Input.WasKeyReleased(Key.Mouse0) || !IsMouseOver()) {
				return;
			}
			if (WidgetInput.Filter != null && !WidgetInput.Filter(Viewport, Key.Mouse0)) {
				return;
			}
			if (!Viewport.IsMouseOverThisOrDescendant()) {
				return;
			}
			Clicked();
		}

		protected override Node CloneInternal()
		{
			var clone = base.CloneInternal() as Mesh3D;
			clone.Submeshes = Submeshes.Clone(clone);
			clone.BoundingSphere = BoundingSphere;
			clone.Center = Center;
			clone.SkinningMode = SkinningMode;
			clone.CullMode = CullMode;
			clone.SkipRender = SkipRender;
			return clone;
		}

		public override void Dispose()
		{
			Submeshes.Clear();
			base.Dispose();
		}

		protected internal override Lime.RenderObject GetRenderObject()
		{
			if (SkipRender) {
				return null;
			}
			var ro = RenderObjectPool<RenderObject>.Acquire();
			ro.World = GlobalTransform;
			ro.SkinningMode = SkinningMode;
			ro.WorldInverse = GlobalTransformInverse;
			ro.CullMode = CullMode;
			ro.ColorFactor = GlobalColor;
			ro.Opaque = Opaque;
			ro.DistanceToCamera = CalcDistanceToCamera(Viewport.Camera);
			var totalBoneCount = 0;
			foreach (var submesh in Submeshes) {
				totalBoneCount += submesh.Bones.Count;
			}
			if (ro.Bones == null || ro.Bones.Length < totalBoneCount) {
				ro.Bones = new Matrix44[totalBoneCount];
				ro.BoneBindPoses = new Matrix44[totalBoneCount];
			}
			var firstBone = 0;
			foreach (var submesh in Submeshes) {
				for (var i = 0; i < submesh.Bones.Count; i++) {
					ro.Bones[firstBone + i] = submesh.Bones[i].GlobalTransform;
					ro.BoneBindPoses[firstBone + i] = submesh.BoneBindPoses[i];
				}
				ro.Meshes.Add(submesh.Mesh);
				ro.Materials.Add(submesh.Material);
				ro.Submeshes.Add(new SubmeshRenderData {
					Mesh = ro.Meshes.Count - 1,
					Material = ro.Materials.Count - 1,
					FirstBone = firstBone,
					BoneCount = submesh.Bones.Count
				});
				firstBone += submesh.Bones.Count;
			}
			return ro;
		}

		private static void DecomposeToDoubleQuaternions(Matrix44 globalTransform, out Vector4 a, out Vector4 b)
		{
			var r = globalTransform.Rotation;
			var t = globalTransform.Translation;
			var dualPart = new Quaternion(t.X, t.Y, t.Z, 0) * r * 0.5f;
			a = new Vector4(r.X, r.Y, r.Z, r.W);
			b = new Vector4(dualPart.X, dualPart.Y, dualPart.Z, dualPart.W);
		}

		private class RenderObject : RenderObject3D
		{
			private static Matrix44[] boneTransforms = new Matrix44[0];
			private static Vector4[] dualQuaternionPartA = new Vector4[0];
			private static Vector4[] dualQuaternionPartB = new Vector4[0];

			public Matrix44 World;
			public Matrix44 WorldInverse;
			public CullMode CullMode;
			public SkinningMode SkinningMode;
			public Color4 ColorFactor;
			public Matrix44[] Bones;
			public Matrix44[] BoneBindPoses;
			public List<Mesh<Vertex>> Meshes = new List<Mesh<Vertex>>();
			public List<IMaterial> Materials = new List<IMaterial>();
			public List<SubmeshRenderData> Submeshes = new List<SubmeshRenderData>();


			protected override void OnRelease()
			{
				Meshes.Clear();
				Materials.Clear();
				Submeshes.Clear();
			}

			public override void Render()
			{
				Renderer.PushState(
					RenderState.World |
					RenderState.CullMode |
					RenderState.ColorFactor);
				Renderer.World = World;
				Renderer.CullMode = CullMode;
				Renderer.ColorFactor = ColorFactor;
				if (SkinningMode == SkinningMode.Linear) {
					foreach (var submesh in Submeshes) {
						var mesh = Meshes[submesh.Mesh];
						var material = Materials[submesh.Material];
						var skin = material as IMaterialSkin;
						if (skin != null) {
							if (boneTransforms.Length < submesh.BoneCount ||
							    boneTransforms.Length < submesh.BoneCount) {
								boneTransforms = new Matrix44[submesh.BoneCount];
							}
							for (var i = 0; i < submesh.BoneCount; i++) {
								var bone = submesh.FirstBone + i;
								boneTransforms[i] = BoneBindPoses[bone] * Bones[bone] * WorldInverse;
							}
							skin.SkinEnabled = submesh.BoneCount > 0;
							skin.SkinningMode = SkinningMode;
							skin.SetBones(boneTransforms, submesh.BoneCount);
						}
						for (var i = 0; i < material.PassCount; i++) {
							material.Apply(i);
							mesh.DrawIndexed(0, mesh.Indices.Length);
						}
						Renderer.PolyCount3d += mesh.Indices.Length / 3;
					}
				} else if (SkinningMode == SkinningMode.DualQuaternion) {
					foreach (var submesh in Submeshes) {
						var mesh = Meshes[submesh.Mesh];
						var material = Materials[submesh.Material];
						var skin = material as IMaterialSkin;
						if (skin != null) {
							if (dualQuaternionPartA.Length < submesh.BoneCount ||
								dualQuaternionPartB.Length < submesh.BoneCount) {
								dualQuaternionPartA = new Vector4[submesh.BoneCount];
								dualQuaternionPartB = new Vector4[submesh.BoneCount];
							}
							for (var i = 0; i < submesh.BoneCount; i++) {
								var bone = submesh.FirstBone + i;
								DecomposeToDoubleQuaternions(BoneBindPoses[bone] * Bones[bone] * WorldInverse,
									out dualQuaternionPartA[i], out dualQuaternionPartB[i]);
							}
							skin.SkinEnabled = submesh.BoneCount > 0;
							skin.SkinningMode = SkinningMode;
							skin.SetBones(dualQuaternionPartA, dualQuaternionPartB, submesh.BoneCount);
						}
						for (var i = 0; i < material.PassCount; i++) {
							material.Apply(i);
							mesh.DrawIndexed(0, mesh.Indices.Length);
						}
						Renderer.PolyCount3d += mesh.Indices.Length / 3;
					}
				}
				Renderer.PopState();
			}
		}

		private struct SubmeshRenderData
		{
			public int Mesh;
			public int Material;
			public int FirstBone;
			public int BoneCount;
		}
	}

	public class Submesh3D
	{
		[YuzuMember]
		public IMaterial Material = new CommonMaterial();

		[YuzuMember]
		public Mesh<Mesh3D.Vertex> Mesh { get; set; }

		[YuzuMember]
		public List<Matrix44> BoneBindPoses { get; private set; }

		[YuzuMember]
		public List<string> BoneNames { get; private set; }
		public List<Node3D> Bones { get; private set; }

		public Mesh3D Owner { get; internal set; }

		public Submesh3D()
		{
			BoneBindPoses = new List<Matrix44>();
			BoneNames = new List<string>();
			Bones = new List<Node3D>();
		}

		public void RebuildSkeleton()
		{
			RebuildSkeleton(Owner.FindModel());
		}

		internal void RebuildSkeleton(Model3D model)
		{
			Bones.Clear();
			foreach (var boneName in BoneNames) {
				Bones.Add(model.Find<Node3D>(boneName));
			}
		}

		public Submesh3D Clone()
		{
			var clone = new Submesh3D();
			clone.Mesh = Mesh;
			clone.BoneNames = new List<string>(BoneNames);
			clone.BoneBindPoses = new List<Matrix44>(BoneBindPoses);
			clone.Material = Material.Clone();
			clone.Owner = null;
			return clone;
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
			item.Owner = owner;
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

		public Submesh3DCollection Clone(Mesh3D owner)
		{
			var clone = new Submesh3DCollection(owner);
			for (int i = 0; i < Count; i++) {
				clone.Add(this[i].Clone());
			}
			return clone;
		}
	}
}
