using System.Collections;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	public class Mesh3D : Node3D
	{
		private static Matrix44[] sharedBoneTransforms = new Matrix44[0];

		[YuzuMember]
		public Submesh3DCollection Submeshes { get; private set; }

		[YuzuMember]
		public BoundingSphere BoundingSphere { get; set; }

		public CullMode CullMode { get; set; }
		public bool ZTestEnabled { get; set; }
		public bool ZWriteEnabled { get; set; }
		public bool SkipRender { get; set; }

		public Mesh3D()
		{
			Submeshes = new Submesh3DCollection(this);
			ZTestEnabled = true;
			ZWriteEnabled = true;
			CullMode = CullMode.CullClockwise;
		}

		public override void Render()
		{
			if (SkipRender) {
				return;
			}
			var world = GlobalTransform;
			var worldInverse = world.CalcInverted();
			var materialExternals = new MaterialExternals {
				WorldView = GlobalTransform * Renderer.View,
				WorldViewProj = Renderer.FixupWVP(world * Renderer.ViewProjection),
				ColorFactor = GlobalColor
			};
			Renderer.ZTestEnabled = ZTestEnabled;
			Renderer.ZWriteEnabled = ZWriteEnabled;
			Renderer.CullMode = CullMode;
			foreach (var sm in Submeshes) {
				if (sm.Bones.Count > 0) {
					if (sharedBoneTransforms.Length < sm.Bones.Count) {
						sharedBoneTransforms = new Matrix44[sm.Bones.Count];
					}
					for (var i = 0; i < sm.Bones.Count; i++) {
						sharedBoneTransforms[i] = sm.BoneBindPoses[i] * sm.Bones[i].GlobalTransform * worldInverse;
					}
					materialExternals.Caps |= MaterialCap.Skin;
					materialExternals.Bones = sharedBoneTransforms;
					materialExternals.BoneCount = sm.Bones.Count;
				}
				sm.Material.Apply(ref materialExternals);
				sm.ReadOnlyGeometry.Render(0, sm.ReadOnlyGeometry.Indices.Length);
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
			return clone;
		}
	}

	public class Submesh3D
	{
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
		public List<Matrix44> BoneBindPoses { get; private set; }

		[YuzuMember]
		public List<string> BoneNames { get; private set; }
		public List<Node3D> Bones { get; private set; }

		public Mesh3D Owner;

		public Submesh3D()
		{
			BoneBindPoses = new List<Matrix44>();
			BoneNames = new List<string>();
			Bones = new List<Node3D>();
		}

		~Submesh3D()
		{
			GeometryReference.Counter--;
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
			GeometryReference.Counter++;
			var clone = new Submesh3D();
			clone.GeometryReference = GeometryReference;
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