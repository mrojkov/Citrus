using Lime;

namespace Tangerine.UI.SceneView
{

	public class AccumulativeRotationHelper
	{
		private readonly float rotationInitial;
		private float rotationPrevious;
		private float rotationAccumulated;

		public AccumulativeRotationHelper(float rotationInitial, float rotationPrevious)
		{
			this.rotationInitial = rotationInitial;
			this.rotationPrevious = Mathf.Wrap180(rotationPrevious);
		}

		public float Rotation
		{
			get { return rotationInitial + rotationAccumulated; }
		}

		public void Rotate(float rotation)
		{
			rotation = Mathf.Wrap180(rotation);
			
			float rotationDelta = Mathf.Wrap180(rotation - rotationPrevious);
			rotationPrevious = rotation;

			rotationAccumulated += rotationDelta;
		}

	}
	
}
