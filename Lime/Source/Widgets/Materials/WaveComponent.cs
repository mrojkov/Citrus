using Yuzu;

namespace Lime
{
	/// <summary>
	/// WaveComponent applies wave deformation to its owner.
	/// </summary>
	[TangerineRegisterComponent]
	public class WaveComponent : MaterialComponent<WaveMaterial>
	{
		/// <summary>
		/// Material's blend mode.
		/// </summary>
		[YuzuMember]
		public Blending Blending
		{
			get => CustomMaterial.Blending;
			set => CustomMaterial.Blending = value;
		}

		/// <summary>
		/// The origin of the wave. Valid range: (0, 0)..(1, 1)
		/// </summary>
		[YuzuMember]
		public Vector2 Point
		{
			get => CustomMaterial.Point;
			set => CustomMaterial.Point = value;
		}

		/// <summary>
		/// The wave phase alongside x and y axes.
		/// (0, 0) -- initial state, (1, 0) -- one full cycle, (2, 2) -- two cycles, etc.
		/// </summary>
		[YuzuMember]
		public Vector2 Phase
		{
			get => CustomMaterial.Phase;
			set => CustomMaterial.Phase = value;
		}

		/// <summary>
		/// The frequency of the wave alongside x and y axes.
		/// </summary>
		[YuzuMember]
		public Vector2 Frequency
		{
			get => CustomMaterial.Frequency;
			set => CustomMaterial.Frequency = value;
		}

		/// <summary>
		/// The strength for the wave alongside x and y axes.
		/// </summary>
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
				var uv0 = image.UV0;
				var uv1 = image.UV1;
				image.Texture.TransformUVCoordinatesToAtlasSpace(ref uv0);
				image.Texture.TransformUVCoordinatesToAtlasSpace(ref uv1);
				CustomMaterial.UV0 = uv0;
				CustomMaterial.UV1 = uv1;
			}
		}

	}
}
