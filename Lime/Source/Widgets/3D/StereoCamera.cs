namespace Lime
{
	public class StereoCameraEye : ICamera
	{
		private Matrix44 projection;
		public Matrix44 view;
		public IViewProjectionCalculator ViewProjCalculator { get; set; }

		public float NearClipPlane { get; set; }

		public float FarClipPlane { get; set; }

		public Matrix44 Projection
		{
			get
			{
				return ViewProjCalculator?.GetProjection(NearClipPlane, FarClipPlane) ?? projection;
			}
			set { projection = value; }
		}

		public Matrix44 View
		{
			get
			{
				return ViewProjCalculator?.GetView() ?? view;
			}
			set { view = value; }
		}

		public StereoCameraEye()
		{
			view = Matrix44.Identity;
			Projection = Matrix44.Identity;
		}
	}

	public class StereoCamera : Node3D, ICamera
	{
		private float nearClipPlane;
		private float farClipPlane;

		public float NearClipPlane
		{
			get
			{
				return nearClipPlane;
			}
			set
			{
				if (nearClipPlane != value) {
					nearClipPlane = value;
					LeftEye.NearClipPlane = value;
					RightEye.NearClipPlane = value;
				}
			}
		}

		public float FarClipPlane
		{
			get
			{
				return farClipPlane;
			}
			set
			{
				if (farClipPlane != value) {
					farClipPlane = value;
					LeftEye.FarClipPlane = value;
					RightEye.FarClipPlane = value;
				}
			}
		}

		public Matrix44 Projection => activeEye.Projection;

		public Matrix44 View => (activeEye.View.CalcInverted() * GlobalTransform).CalcInverted();

		public StereoCameraEye LeftEye;

		public StereoCameraEye RightEye;

		private StereoCameraEye activeEye;

		public void SetCurrentEye(int index)
		{
			switch (index) {
				case 0:
					activeEye = LeftEye;
					break;
				case 1:
					activeEye = RightEye;
					break;
			}
		}

		public StereoCamera()
		{
			LeftEye = new StereoCameraEye();
			RightEye = new StereoCameraEye();
			activeEye = LeftEye;
		}
	}

	public interface IViewProjectionCalculator
	{
		Matrix44 GetProjection(float farClipPlane, float NearClipPlane);
		Matrix44 GetView();
	}

	public class GenericViewProjectionCalculator : IViewProjectionCalculator
	{
		private readonly GetViewDelegate viewGetter;
		private readonly GetProjectionDelegate projGetter;

		public delegate Matrix44 GetProjectionDelegate(float nearClipPlane, float farClipPlane);

		public delegate Matrix44 GetViewDelegate();

		public GenericViewProjectionCalculator(GetProjectionDelegate projGetter, GetViewDelegate viewGetter)
		{
			this.viewGetter = viewGetter;
			this.projGetter = projGetter;
		}

		public Matrix44 GetProjection(float nearClipPlane, float farClipPlane)
		{
			return projGetter?.Invoke(nearClipPlane, farClipPlane) ?? Matrix44.Identity;
		}

		public Matrix44 GetView()
		{
			return viewGetter?.Invoke() ?? Matrix44.Identity;
		}
	}
}
