using System;
using System.Collections;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	/// <summary>
	/// WaveComponent applies the wave deformation to its owner.
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
		/// Whether the waving is clamped withing the widget bounds.
		/// </summary>
		[YuzuMember]
		public bool IsClamped
		{
			get => CustomMaterial.IsClamped;
			set => CustomMaterial.IsClamped = value;
		}

		/// <summary>
		/// Wave type
		/// </summary>
		[YuzuMember]
		public WaveType WaveType
		{
			get => CustomMaterial.Type;
			set => CustomMaterial.Type = value;
		}

		/// <summary>
		/// Denotes the fixed point on the waving surface. Top-left: (0, 0), right-bottom: (1, 1)
		/// </summary>
		[YuzuMember]
		public Vector2 Pivot
		{
			get => CustomMaterial.Pivot;
			set => CustomMaterial.Pivot = value;
		}

		/// <summary>
		/// Phase of waving along x and y axes.
		/// (0, 0) -- initial state, (1, 1) -- one full cycle, (2, 2) -- two cycles, etc.
		/// </summary>
		[YuzuMember]
		public Vector2 Phase
		{
			get => CustomMaterial.Phase;
			set => CustomMaterial.Phase = value;
		}

		/// <summary>
		/// The frequency of the wave along x and y axes.
		/// </summary>
		[YuzuMember]
		public Vector2 Frequency
		{
			get => CustomMaterial.Frequency;
			set => CustomMaterial.Frequency = value;
		}

		/// <summary>
		/// The strength of the wave along x and y axes.
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
			if (oldOwner != null && updateUVOnTextureChangeTask != null) {
				oldOwner.Tasks.Remove(updateUVOnTextureChangeTask);
			}
			if (Owner != null) {
				var image = (Image)Owner;
				updateUVOnTextureChangeTask = image.Tasks.Add(UpdateUVOnTextureChange(image));
				UpdateUV(image);
			}
		}

		private void UpdateUV(Image owner)
		{
			var uv0 = owner.UV0;
			var uv1 = owner.UV1;
			owner.Texture.TransformUVCoordinatesToAtlasSpace(ref uv0);
			owner.Texture.TransformUVCoordinatesToAtlasSpace(ref uv1);
			CustomMaterial.UV0 = uv0;
			CustomMaterial.UV1 = uv1;
		}

		private Task updateUVOnTextureChangeTask;

		private IEnumerator<object> UpdateUVOnTextureChange(Image owner)
		{
			ITexture texture = null;
			while (true) {
				if (owner.Texture != texture) {
					texture = owner.Texture;
					UpdateUV(owner);
				}
				yield return null;
			}
		}
	}
}
