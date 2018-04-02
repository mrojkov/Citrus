namespace Lime
{
	public interface ICamera
	{
		Matrix44 View { get; }

		Matrix44 Projection { get; }

		float NearClipPlane { get; set; }

		float FarClipPlane { get; set; }
	}
}
