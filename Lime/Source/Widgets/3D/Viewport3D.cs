using System.Collections.Generic;
using Yuzu;
using System;
using System.Linq;
#if OPENGL
#if !MAC && !MONOMAC
using OpenTK.Graphics.ES20;
#elif MAC
using OpenTK.Graphics.OpenGL;
#elif MONOMAC
using MonoMac.OpenGL;
#endif

#endif

namespace Lime
{
	[TangerineRegisterNode(CanBeRoot = true, Order = 20)]
	[TangerineNodeBuilder("BuildForTangerine")]
	[TangerineAllowedChildrenTypes(typeof(Node3D), typeof(SplineGear3D))]
	[TangerineVisualHintGroup("/All/Nodes/Containers")]
	public class Viewport3D : Widget
	{
		private float frame;
		private RenderChain renderChain = new RenderChain();

		[YuzuMember]
		[TangerineKeyframeColor(28)]
		public NodeReference<Camera3D> CameraRef { get; set; }

		public Camera3D Camera => CameraRef?.GetNode(this);

		[YuzuMember]
		[TangerineKeyframeColor(31)]
		public float Frame
		{
			get { return frame; }
			set
			{
				frame = value;
				RefreshChildren();
			}
		}

		public Viewport3D()
		{
			Presenter = DefaultPresenter.Instance;
		}

		private void BuildForTangerine()
		{
			var camera = new Camera3D {
				Id = "DefaultCamera",
				Position = new Vector3(0, 0, 10),
				FarClipPlane = 10000,
				NearClipPlane = 0.001f,
				FieldOfView = 1.0f,
				AspectRatio = 1.3f,
				OrthographicSize = 1.0f
			};
			Nodes.Add(camera);
			CameraRef = new NodeReference<Camera3D>(camera.Id);
		}

		protected override void OnSizeChanged(Vector2 sizeDelta)
		{
			base.OnSizeChanged(sizeDelta);
			AdjustCameraAspectRatio();
		}

		private void AdjustCameraAspectRatio()
		{
			if (Camera != null) {
				Camera.AspectRatio = Width / Height;
			}
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (GloballyVisible && ClipRegionTest(chain.ClipRegion)) {
				AddSelfToRenderChain(chain, Layer);
			}
		}

		private void RefreshChildren()
		{
			var time = (int)(Frame * 1000 / 16);
			foreach (var n in Nodes) {
				n.AnimationTime = time;
			}
		}

		internal protected override bool PartialHitTest(ref HitTestArgs args)
		{
			try {
				if (Camera == null) {
					return false;
				}
				args.Ray = ScreenPointToRay(args.Point);
				args.Distance = float.MaxValue;
				foreach (var node in Nodes) {
					node.RenderChainBuilder?.AddToRenderChain(renderChain);
				}
				var layers = renderChain.Layers;
				for (var i = layers.Length - 1; i >= 0; i--) {
					var list = layers[i];
					if (list == null || list.Count == 0) {
						continue;
					}
					var hit = false;
					for (var j = 0; j < list.Count; j++) {
						var t = list[j];
						hit |= t.Presenter.PartialHitTest(t.Node, ref args);
					}
					if (hit) {
						return true;
					}
				}
				return false;
			} finally {
				renderChain.Clear();
			}
		}

		public override Node Clone()
		{
			var vp = (Viewport3D)base.Clone();
			vp.CameraRef = CameraRef?.Clone();
			return vp;
		}

		public Vector3 WorldToScreenPoint(Vector3 pt)
		{
			pt = WorldToViewportPoint(pt);
			return new Vector3(LocalToWorldTransform.TransformVector((Vector2)pt), pt.Z);
		}

		public Vector3 WorldToViewportPoint(Vector3 pt)
		{
			pt = Camera.ViewProjection.ProjectVector(pt) * new Vector3(1, -1, 1);
			var xy = ((Vector2)pt + Vector2.One) * Size * Vector2.Half;
			var z = Camera.Projection.CalcInverted().ProjectVector(pt).Z;
			return new Vector3(xy, z);
		}

		public Vector3 ScreenToWorldPoint(Vector3 pt)
		{
			var xy = LocalToWorldTransform.CalcInversed().TransformVector((Vector2)pt);
			return ViewportToWorldPoint(new Vector3(xy, pt.Z));
		}

		public Vector3 ViewportToWorldPoint(Vector3 pt)
		{
			return Camera.ViewProjection.CalcInverted().ProjectVector(ViewportToNDCPoint(pt));
		}

		public Ray ScreenPointToRay(Vector2 pt)
		{
			return ScreenPointToRay(new Vector3(pt, Camera.NearClipPlane));
		}

		public Ray ScreenPointToRay(Vector3 pt)
		{
			var xy = LocalToWorldTransform.CalcInversed().TransformVector((Vector2)pt);
			return ViewportPointToRay(new Vector3(xy, pt.Z));
		}

		public Ray ViewportPointToRay(Vector2 pt)
		{
			return ViewportPointToRay(new Vector3(pt, Camera.NearClipPlane));
		}

		public Ray ViewportPointToRay(Vector3 pt)
		{
			var xy = (Vector2)pt;
			var invViewProj = Camera.ViewProjection.CalcInverted();
			var a = invViewProj.ProjectVector(ViewportToNDCPoint(pt));
			var b = invViewProj.ProjectVector(ViewportToNDCPoint(new Vector3(xy, Camera.FarClipPlane)));
			return new Ray {
				Position = a,
				Direction = (a - b).Normalized
			};
		}

		private Vector3 ViewportToNDCPoint(Vector3 pt)
		{
			var xy = (Vector2)pt / Size * 2f - Vector2.One;
			var z = Camera.Projection.ProjectVector(pt).Z;
			return new Vector3(xy, z) * new Vector3(1, -1, 1);
		}

		protected internal override Lime.RenderObject GetRenderObject()
		{
			if (Camera == null) {
				return null;
			}
			AdjustCameraAspectRatio();
			var ro = RenderObjectPool<RenderObject>.Acquire();
			ro.Width = Width;
			ro.Height = Height;
			ro.Transform = LocalToWorldTransform;
			ro.View = Camera.View;
			ro.Projection = Camera.Projection;
			try {
				foreach (var node in Nodes) {
					node.RenderChainBuilder?.AddToRenderChain(renderChain);
				}
				for (var i = 0; i < RenderChain.LayerCount; i++) {
					var layer = renderChain.Layers[i];
					if (layer == null || layer.Count == 0) {
						continue;
					}
					var first = ro.Objects.Count;
					foreach (var item in layer) {
						var renderObject = item.Presenter.GetRenderObject(item.Node);
						if (renderObject != null) {
							ro.Objects.Add(renderObject);
						}
					}
					ro.Layers.Add(new RenderLayer {
						FirstObject = first,
						ObjectCount = ro.Objects.Count - first
					});
				}
			} finally {
				renderChain.Clear();
			}
			return ro;
		}

		private class RenderObject : Lime.RenderObject
		{
			private List<RenderObject3D> opaqueObjects = new List<RenderObject3D>();
			private List<RenderObject3D> transparentObjects = new List<RenderObject3D>();

			public float Width;
			public float Height;
			public Matrix32 Transform;
			public Matrix44 View;
			public Matrix44 Projection;
			public RenderObjectList Objects = new RenderObjectList();
			public List<RenderLayer> Layers = new List<RenderLayer>();

			protected override void OnRelease()
			{
				Objects.Clear();
				Layers.Clear();
			}

			public override void Render()
			{
				Renderer.Flush();
				Renderer.PushState(
					RenderState.World |
					RenderState.View |
					RenderState.Projection |
					RenderState.DepthState |
					RenderState.CullMode);
				Renderer.View = View;
				Renderer.Projection = MakeProjection(Width, Height, Transform, Projection, Renderer.Projection);
				foreach (var layer in Layers) {
					try {
						for (var i = 0; i < layer.ObjectCount; i++) {
							var obj = (RenderObject3D)Objects[layer.FirstObject + i];
							var list = obj.Opaque ? opaqueObjects : transparentObjects;
							list.Add(obj);
						}
						Renderer.DepthState = DepthState.DepthReadWrite;
						opaqueObjects.Sort(RenderOrderComparers.FrontToBack);
						foreach (var obj in opaqueObjects) {
							obj.Render();
						}
						Renderer.DepthState = DepthState.DepthRead;
						transparentObjects.Sort(RenderOrderComparers.BackToFront);
						foreach (var obj in transparentObjects) {
							obj.Render();
						}
					} finally {
						opaqueObjects.Clear();
						transparentObjects.Clear();
					}
				}
				Renderer.Clear(ClearOptions.DepthBuffer);
				Renderer.PopState();
			}

			private Matrix44 TransformProjection(Matrix44 orthoProjection)
			{
				orthoProjection.M33 = 1; // Discard Z normalization, since it comes from the camera projection matrix
				orthoProjection.M43 = 0;
				return Projection *
					// Transform from <-1, 1> normalized coordinates to the widget space
					Matrix44.CreateScale(Width / 2, -Height / 2, 1) *
					Matrix44.CreateTranslation(Width / 2, Height / 2, 0) *
					(Matrix44)Transform * orthoProjection;
			}
		}

		public static Matrix44 MakeProjection(
			float width, float height, Matrix32 transform,
			Matrix44 cameraProjection, Matrix44 orthoProjection)
		{
			orthoProjection.M33 = 1; // Discard Z normalization, since it comes from the camera projection matrix
			orthoProjection.M43 = 0;
			return cameraProjection *
				// Transform from <-1, 1> normalized coordinates to the widget space
				Matrix44.CreateScale(width / 2, -height / 2, 1) *
				Matrix44.CreateTranslation(width / 2, height / 2, 0) *
				(Matrix44)transform * orthoProjection;
		}

		private struct RenderLayer
		{
			public int FirstObject;
			public int ObjectCount;
		}

		private static class RenderOrderComparers
		{
			public static readonly BackToFrontComparer BackToFront = new BackToFrontComparer();
			public static readonly FrontToBackComparer FrontToBack = new FrontToBackComparer();
		}

		private class BackToFrontComparer : Comparer<RenderObject3D>
		{
			public override int Compare(RenderObject3D x, RenderObject3D y)
			{
				return x.DistanceToCamera.CompareTo(y.DistanceToCamera);
			}
		}

		private class FrontToBackComparer : Comparer<RenderObject3D>
		{
			public override int Compare(RenderObject3D x, RenderObject3D y)
			{
				return y.DistanceToCamera.CompareTo(x.DistanceToCamera);
			}
		}
	}
}
