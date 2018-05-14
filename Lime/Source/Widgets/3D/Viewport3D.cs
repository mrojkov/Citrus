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
	[TangerineNodeBuilder("BuildForTangerine")]
	[AllowedChildrenTypes(typeof(Node3D), typeof(SplineGear3D))]
	public class Viewport3D : Widget
	{
		public interface IZSorterParams
		{
			float CalcDistanceToCamera(Camera3D camera);
			bool Opaque { get; }
		}
		
		private float frame;
		private List<RenderItem> opaqueList = new List<RenderItem>();
		private List<RenderItem> transparentList = new List<RenderItem>();
		private RenderChain renderChain = new RenderChain();

		[YuzuMember]
		public NodeReference<Camera3D> CameraRef { get; set; }

		[YuzuMember]
		public NodeReference<LightSource> LightSourceRef { get; set; }

		public Camera3D Camera => CameraRef?.GetNode(this);
		public LightSource LightSource => LightSourceRef?.GetNode(this);

#if DEBUG
		[TangerineInspect]
		public NodeReference<Image> DebugShadowMapImageRef { get; set; }
		public Image DebugShadowMapImage => DebugShadowMapImageRef?.GetNode(this.Parent);
#endif

		[YuzuMember]
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
#if DEBUG
			DebugShadowMapImageRef = new NodeReference<Image>("ShadowMap");
#endif
		}

#if DEBUG
		public override void Update(float delta)
		{
			base.Update(delta);

			if (DebugShadowMapImage != null) {
				DebugShadowMapImage.Texture = LightSource?.ShadowMap;
			}
		}
#endif

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

		public override void Render()
		{
			if (Camera == null) {
				return;
			}
			AdjustCameraAspectRatio();
			foreach (var node in Nodes) {
				node.RenderChainBuilder?.AddToRenderChain(renderChain);
			}
			var oldWorld = Renderer.World;
			var oldView = Renderer.View;
			var oldProj = Renderer.Projection;
			var oldDepthState = Renderer.DepthState;
			var oldCullMode = Renderer.CullMode;
			Renderer.Flush();
			Renderer.Clear(ClearOptions.DepthBuffer);
			Renderer.View = Camera.View;
			Renderer.Projection = TransformProjection(Renderer.Projection);
			for (var i = 0; i < RenderChain.LayerCount; i++) {
				var layer = renderChain.Layers[i];
				if (layer == null || layer.Count == 0) {
					continue;
				}
				foreach (var item in layer) {
					var p = item.Node as IZSorterParams;
					if (p == null) {
						continue;
					}
					var list = p.Opaque ? opaqueList : transparentList;
					list.Add(new RenderItem {
						Node = item.Node,
						Presenter = item.Presenter,
						Distance = p.CalcDistanceToCamera(Camera)
					});
				}
				Renderer.DepthState = DepthState.DepthReadWrite;
				SortAndFlushList(opaqueList, RenderOrderComparers.FrontToBack);
				Renderer.DepthState = DepthState.DepthRead;
				SortAndFlushList(transparentList, RenderOrderComparers.BackToFront);
			}
			renderChain.Clear();
			Renderer.Clear(ClearOptions.DepthBuffer);
			Renderer.World = oldWorld;
			Renderer.View = oldView;
			Renderer.Projection = oldProj;
			Renderer.DepthState = oldDepthState;
			Renderer.CullMode = oldCullMode;
		}

		public override Node Clone()
		{
			var vp = (Viewport3D)base.Clone();
			vp.opaqueList = new List<RenderItem>();
			vp.transparentList = new List<RenderItem>();
			vp.CameraRef = CameraRef?.Clone();
			vp.LightSourceRef = LightSourceRef?.Clone();
			return vp;
		}

		private void SortAndFlushList(List<RenderItem> items, IComparer<RenderItem> comparer)
		{
			items.Sort(comparer);
			foreach (var i in items) {
				i.Presenter.Render(i.Node);
			}
			items.Clear();
		}

		public Matrix44 TransformProjection(Matrix44 orthoProjection)
		{
			orthoProjection.M33 = 1; // Discard Z normalization, since it comes from the camera projection matrix
			orthoProjection.M43 = 0;
			var p =
				// Transform from <-1, 1> normalized coordinates to the widget space
				Matrix44.CreateScale(Width / 2, -Height / 2, 1) *
				Matrix44.CreateTranslation(Width / 2, Height / 2, 0) *
				(Matrix44)LocalToWorldTransform * orthoProjection;
			if (Camera != null) {
				return Camera.Projection * p;
			} else {
				return p;
			}
		}

		public void InvalidateMaterials()
		{
			foreach (var mesh in Descendants.OfType<Mesh3D>().SelectMany((m) => m.Submeshes)) {
				mesh.Material.Invalidate();
			}
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

		private static class RenderOrderComparers
		{
			public static readonly BackToFrontComparer BackToFront = new BackToFrontComparer();
			public static readonly FrontToBackComparer FrontToBack = new FrontToBackComparer();
		}

		private class BackToFrontComparer : Comparer<RenderItem>
		{
			public override int Compare(RenderItem x, RenderItem y)
			{
				return x.Distance.CompareTo(y.Distance);
			}
		}

		private class FrontToBackComparer : Comparer<RenderItem>
		{
			public override int Compare(RenderItem x, RenderItem y)
			{
				return y.Distance.CompareTo(x.Distance);
			}
		}

		private struct RenderItem
		{
			public Node Node;
			public IPresenter Presenter;
			public float Distance;
		}
	}
}