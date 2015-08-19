using System;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class ModelCamera : ModelNode
	{
		private float fieldOfView;
		private float aspectRatio;
		private float nearClipPlane;
		private float farClipPlane;
		private Matrix44 view;
		private Matrix44 projection;
		private bool projectionDirty;

		[ProtoMember(1)]
		public float FieldOfView
		{
			get { return fieldOfView; }
			set
			{
				if (fieldOfView != value) {
					fieldOfView = value;
					projectionDirty = true;
				}
			}
		}

		[ProtoMember(2)]
		public float AspectRatio
		{
			get { return aspectRatio; }
			set
			{
				if (aspectRatio != value) {
					aspectRatio = value;
					projectionDirty = true;
				}
			}
		}

		[ProtoMember(3)]
		public float NearClipPlane
		{
			get { return nearClipPlane; }
            set
			{
				if (nearClipPlane != value) {
					nearClipPlane = value;
					projectionDirty = true;
				}
			}
		}

		[ProtoMember(4)]
		public float FarClipPlane
		{
			get { return farClipPlane; }
			set
			{
				if (farClipPlane != value) {
					farClipPlane = value;
					projectionDirty = true;
				}
			}
		}

		public Matrix44 ViewProjection
		{
			get { return View * Projection; }
		}

		public Matrix44 View
		{
			get { RecalcDirtyGlobals(); return view; }
		}

		public Matrix44 Projection
		{
			get
			{
				if (projectionDirty) {
					projectionDirty = false;
					projection = Matrix44.CreatePerspectiveFieldOfView(
						fieldOfView / aspectRatio,
						aspectRatio,
						nearClipPlane,
						farClipPlane
					);
				}
				return projection;
			}
		}

		protected override void RecalcDirtyGlobalsUsingParents()
		{
			base.RecalcDirtyGlobalsUsingParents();
			if ((DirtyMask & DirtyFlags.Transform) != 0) {
				view = globalTransform.CalcInverted();
			}
		}
	}
}