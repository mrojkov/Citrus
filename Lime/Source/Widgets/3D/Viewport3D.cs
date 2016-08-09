using System.Collections.Generic;
using ProtoBuf;
using Yuzu;
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
	[ProtoContract]
	public class Viewport3D : Widget
	{
		private Camera3D camera;
		private float frame;
		private List<IRenderObject3D> renderQueue;
		private List<Mesh3D> modelMeshes;
		private bool zSortEnabled;
		private RenderChain renderChain = new RenderChain();

		public bool ZSortEnabled
		{
			get { return zSortEnabled; }
			set
			{
				if (value) {
					if (!zSortEnabled) {
						renderQueue = new List<IRenderObject3D>();
						modelMeshes = new List<Mesh3D>();
					}
				} else {
					renderQueue = null;
					modelMeshes = null;
				}
				zSortEnabled = value;
			}
		}

		private DepthComparer depthComparer = new DepthComparer();

		public Camera3D Camera
		{
			get { return camera; }
			set
			{
				camera = value;
				depthComparer.Camera = camera;
				AdjustCameraAspectRatio();
			}
		}

		[ProtoMember(1)]
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

		private class DepthComparer : IComparer<IRenderObject3D>
		{
			public Camera3D Camera;

			public int Compare(IRenderObject3D a, IRenderObject3D b)
			{
				float da = (a.Center * Camera.View).Z;
				float db = (b.Center * Camera.View).Z;
				return da.CompareTo(db);
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
			var oldCullMode = Renderer.CullMode;
			var oldZTestEnabled = Renderer.ZTestEnabled;
			var oldZWriteEnabled = Renderer.ZWriteEnabled;
			Renderer.Flush();
			Renderer.PushProjectionMatrix();
			Renderer.Projection = TransformProjection(Renderer.Projection);
			WidgetContext.Current.CurrentCamera = Camera;
#if UNITY
			MaterialFactory.ThreeDimensionalRendering = true;
#else
			Renderer.CullMode = CullMode.CullClockwise;
#endif
			if (ZSortEnabled) {
				for (int i = 0; i < RenderChain.LayerCount; i++) {
					var layer = renderChain.Layers[i];
					if (layer == null || layer.Count == 0) {
						continue;
					}
					renderQueue.Clear();
					modelMeshes.Clear();
					foreach (var t in layer) {
						var mm = t.Node as Mesh3D;
						if (mm != null) {
							if (!mm.SkipRender) {
								modelMeshes.Add(mm);
								foreach (var sm in mm.Submeshes) {
									renderQueue.Add(sm);
								}
							}
						}
						var wa = t.Node as WidgetAdapter3D;
						if (wa != null) {
							renderQueue.Add(wa);
						}
					}
					renderQueue.Sort(depthComparer);
					foreach (var mm in modelMeshes) {
						mm.PrepareToRender();
					}
					foreach (var sm in renderQueue) {
						sm.Render();
					}
				}
				renderChain.Clear();
			} else {
				renderChain.RenderAndClear();
			}
#if UNITY
			MaterialFactory.ThreeDimensionalRendering = false;
#else
			Renderer.CullMode = oldCullMode;
#endif
			Renderer.Clear(ClearTarget.DepthBuffer);
			Renderer.PopProjectionMatrix();
			Renderer.ZTestEnabled = oldZTestEnabled;
			Renderer.ZWriteEnabled = oldZWriteEnabled;
			WidgetContext.Current.CurrentCamera = Camera;
		}

		private Matrix44 TransformProjection(Matrix44 orthoProjection)
		{
			orthoProjection.M33 = 1; // Discard Z normalization, since it comes from the camera projection matrix
			orthoProjection.M43 = 0;
			return Camera.ViewProjection *
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
	}
}