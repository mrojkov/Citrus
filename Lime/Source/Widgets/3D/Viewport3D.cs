using System.Collections.Generic;
using Yuzu;
using System;
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
	public class Viewport3D : Widget
	{
		private Camera3D camera;
		private float frame;
		private List<RenderItem> opaqueList;
		private List<RenderItem> transparentList;
		private bool zSortEnabled;
		private RenderChain renderChain = new RenderChain();

		public bool ZSortEnabled
		{
			get { return zSortEnabled; }
			set
			{
				if (value == zSortEnabled) {
					return;
				}
				if (value) {
					opaqueList = new List<RenderItem>();
					transparentList = new List<RenderItem>();
				} else {
					opaqueList = null;
					transparentList = null;
				}
				zSortEnabled = value;
			}
		}

		public Camera3D Camera
		{
			get { return camera; }
			set
			{
				camera = value;
				AdjustCameraAspectRatio();
			}
		}

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

		protected override void OnSizeChanged(Vector2 sizeDelta)
		{
			base.OnSizeChanged(sizeDelta);
			AdjustCameraAspectRatio();
		}

		private void AdjustCameraAspectRatio()
		{
			if (camera != null) {
				camera.AspectRatio = Width / Height;
			}
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (!GloballyVisible) {
				return;
			}
			AddSelfToRenderChain(chain);
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
				args.Ray = ScreenPointToRay(args.Point);
				args.Distance = float.MaxValue;
				foreach (var node in Nodes) {
					node.AddToRenderChain(renderChain);
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
			foreach (var node in Nodes) {
				node.AddToRenderChain(renderChain);
			}
			var oldWorld = Renderer.World;
			var oldView = Renderer.View;
			var oldProj = Renderer.Projection;
			var oldZTestEnabled = Renderer.ZTestEnabled;
			var oldZWriteEnabled = Renderer.ZWriteEnabled;
			var oldCullMode = Renderer.CullMode;
			Renderer.Flush();
			Renderer.View = Camera.View;
			Renderer.Projection = TransformProjection(Renderer.Projection);
			Renderer.ZTestEnabled = true;
			if (ZSortEnabled) {
				for (var i = 0; i < RenderChain.LayerCount; i++) {
					var layer = renderChain.Layers[i];
					if (layer == null || layer.Count == 0) {
						continue;
					}
					for (var j = 0; j < layer.Count; j++) {
						var node = layer[j].Node.AsNode3D;
						var list = node.Opaque ? opaqueList : transparentList;
						list.Add(new RenderItem {
							Node = node,
							Distance = node.CalcDistanceToCamera(camera)
						});
					}
					Renderer.ZWriteEnabled = true;
					SortAndFlushList(opaqueList, RenderOrderComparers.FrontToBack);
					Renderer.ZWriteEnabled = false;
					SortAndFlushList(transparentList, RenderOrderComparers.BackToFront);
				}
				renderChain.Clear();
			} else {
				Renderer.ZWriteEnabled = true;
				renderChain.RenderAndClear();
			}
			Renderer.Clear(ClearTarget.DepthBuffer);
			Renderer.World = oldWorld;
			Renderer.View = oldView;
			Renderer.Projection = oldProj;
			Renderer.ZTestEnabled = oldZTestEnabled;
			Renderer.ZWriteEnabled = oldZWriteEnabled;
			Renderer.CullMode = oldCullMode;
		}

		private void SortAndFlushList(List<RenderItem> items, IComparer<RenderItem> comparer)
		{
			items.Sort(comparer);
			for (var i = 0; i < items.Count; i++) {
				var node = items[i].Node;
				node.Presenter.Render(node);
			}
			items.Clear();
		}

		private Matrix44 TransformProjection(Matrix44 orthoProjection)
		{
			orthoProjection.M33 = 1; // Discard Z normalization, since it comes from the camera projection matrix
			orthoProjection.M43 = 0;
			return Camera.Projection *
				// Transform from <-1, 1> normalized coordinates to the widget space
				Matrix44.CreateScale(new Vector3(Width / 2, -Height / 2, 1)) *
				Matrix44.CreateTranslation(new Vector3(Width / 2, Height / 2, 0)) *
				(Matrix44)LocalToWorldTransform * orthoProjection;
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
			var zNear = Camera.NearClipPlane;
			var zFar = Camera.FarClipPlane;
			var z = 2f * (zNear * zFar) / ((zFar - zNear) * pt.Z - zNear - zFar);
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
			var zNear = Camera.NearClipPlane;
			var zFar = Camera.FarClipPlane;
			var z = (pt.Z * (zFar + zNear) + 2f * zFar * zNear) / (pt.Z * (zFar - zNear));
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
			public Node3D Node;
			public float Distance;
		}
	}
}