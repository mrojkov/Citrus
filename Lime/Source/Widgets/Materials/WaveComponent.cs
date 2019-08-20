using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	public class WaveComponent : MaterialComponent<WaveMaterial>
	{
		[YuzuMember]
		public Blending Blending
		{
			get => CustomMaterial.Blending;
			set => CustomMaterial.Blending = value;
		}

		[YuzuMember]
		[TangerineKeyframeColor(1)]
		public bool AutoLoopEnabled
		{
			get => CustomMaterial.AutoLoopEnabled;
			set => CustomMaterial.AutoLoopEnabled = value;
		}

		[YuzuMember]
		public float Time
		{
			get => CustomMaterial.Time;
			set => CustomMaterial.Time = value;
		}

		[YuzuMember]
		public float Frequency
		{
			get => CustomMaterial.Frequency;
			set => CustomMaterial.Frequency = value;
		}

		[YuzuMember]
		public Vector2 Point
		{
			get => CustomMaterial.Point;
			set => CustomMaterial.Point = value;
		}

		[YuzuMember]
		public Vector2 TimeSpeed
		{
			get => CustomMaterial.TimeSpeed;
			set => CustomMaterial.TimeSpeed = value;
		}

		[YuzuMember]
		public Vector2 Amplitude
		{
			get => CustomMaterial.Amplitude;
			set => CustomMaterial.Amplitude = value;
		}
		
		protected override void OnOwnerChanged(Node oldOwner)
		{
			base.OnOwnerChanged(oldOwner);
			if (Owner != null) {
				var image = (Image)Owner;
				CustomMaterial.UV0 = image.UV0;
				CustomMaterial.UV1 = image.UV1;
				image.Texture.TransformUVCoordinatesToAtlasSpace(ref CustomMaterial.UV0);
				image.Texture.TransformUVCoordinatesToAtlasSpace(ref CustomMaterial.UV1);
			}
		}

	}
}
