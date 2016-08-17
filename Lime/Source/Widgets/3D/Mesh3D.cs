using System.Collections;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	public class Mesh3D : Node3D
	{
		internal Matrix44 WorldView;
		internal Matrix44 WorldViewProj;
		internal Matrix44[] SharedBoneTransforms = new Matrix44[] { };
		private bool invalidBones;

		[YuzuMember]
		public Submesh3DCollection Submeshes { get; private set; }

		[YuzuMember]
		public BoundingSphere BoundingSphere { get; set; }

		[YuzuMember]
		public List<Node3D> Bones { get; private set; }

		[YuzuMember]
		public List<Matrix44> BoneBindPoseInverses { get; private set; }

		public CullMode CullMode { get; set; }
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
			CullMode = CullMode.CullClockwise;
		}

		[YuzuAfterDeserialization]
		public void AfterDeserialization()
		{
			invalidBones = true;
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (!GloballyVisible) {
				return;
			}
			AddContentsToRenderChain(chain);
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
			WorldView = world * WidgetContext.Current.CurrentCamera.View;
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
				while (skeletonRoot != null && skeletonRoot.AsNode3D != null) {
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
			ray = ray.Transform(GlobalTransform.CalcInverted());
			foreach (var submesh in Submeshes) {
				var vertices = submesh.ReadOnlyGeometry.Vertices;
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

		public override Node Clone()
		{
			var clone = base.Clone() as Mesh3D;
			clone.Submeshes = Submeshes.Clone(clone);
			clone.Bones = Toolbox.Clone(Bones);
			clone.BoneBindPoseInverses = Toolbox.Clone(BoneBindPoseInverses);
			clone.SharedBoneTransforms = new Matrix44[] { };
			clone.invalidBones = true;
			return clone;
		}
	}

	public class Submesh3D : IRenderObject3D
	{
		private static Matrix44[] sharedBoneTransforms = new Matrix44[] { };

		public GeometryBufferReference GeometryReference = new GeometryBufferReference(new GeometryBuffer());
		public GeometryBuffer ReadOnlyGeometry { get { return GeometryReference.Target; } }

		[YuzuMember]
		public Material Material { get; set; }

		[YuzuMember]
		public GeometryBuffer Geometry
		{
			get
			{
				if (GeometryReference.Counter > 1) {
					GeometryReference.Counter--;
					GeometryReference = new GeometryBufferReference(GeometryReference.Target.Clone());
				}
				return GeometryReference.Target;
			}
		}

		[YuzuMember]
		public List<int> BoneIndices { get; private set; }

		public Mesh3D ModelMesh;

		private Vector3? center;

		public Vector3 Center
		{
			get
			{
				if (center == null) {
					center = Vector3.Zero;
					int n = ReadOnlyGeometry.Vertices.Length;
					for (int i = 0; i < n; i++) {
						center += ReadOnlyGeometry.Vertices[i];
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

		~Submesh3D()
		{
			GeometryReference.Counter--;
		}

		public void Render()
		{
			Renderer.ZTestEnabled = ModelMesh.ZTestEnabled;
			Renderer.ZWriteEnabled = ModelMesh.ZWriteEnabled;
			Renderer.CullMode = ModelMesh.CullMode;
			var materialExternals = new MaterialExternals {
				WorldView = ModelMesh.WorldView,
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
			ReadOnlyGeometry.Render(0, ReadOnlyGeometry.Indices.Length);
		}

		public Submesh3D Clone()
		{
			GeometryReference.Counter++;
			var clone = MemberwiseClone() as Submesh3D;
			clone.GeometryReference = GeometryReference;
			clone.BoneIndices = Toolbox.Clone(BoneIndices);
			clone.Material = Material.Clone();
			clone.ModelMesh = null;
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

		public Submesh3DCollection Clone(Mesh3D owner)
		{
			var clone = new Submesh3DCollection(owner);
			for (int i = 0; i < Count; i++) {
				clone.Add(this[i].Clone());
			}
			return clone;
		}
	}

	public class GeometryBufferReference
	{
		public int Counter;
		public GeometryBuffer Target;

		public GeometryBufferReference(GeometryBuffer target)
		{
			Target = target;
			Counter = 1;
		}


	}

}