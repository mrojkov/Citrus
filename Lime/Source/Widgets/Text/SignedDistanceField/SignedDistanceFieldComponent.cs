using Yuzu;
using Lime.SignedDistanceField;
using System.Collections.Generic;

namespace Lime
{
	public class ShadowParams
	{
		private const float MinimumSoftness = 0f;
		private const float MaximumSoftness = 45f;
		private const float MinimumDilate = -40f;
		private const float MaximumDilate = 40f;

		private float softness = 0f;
		private float dilate = 0f;

		[YuzuMember]
		public bool Enabled { get; set; } = true;

		[YuzuMember]
		public Color4 Color { get; set; } = Color4.Black;

		[YuzuMember]
		public int OffsetX { get; set; }

		[YuzuMember]
		public int OffsetY { get; set; }

		[YuzuMember]
		public float Softness
		{
			get => softness;
			set => softness = Mathf.Clamp(value, MinimumSoftness, MaximumSoftness);
		}

		[YuzuMember]
		public float Dilate
		{
			get => dilate;
			set => dilate = Mathf.Clamp(value, MinimumDilate, MaximumDilate);
		}
	}

	[TangerineRegisterComponent]
	[AllowedComponentOwnerTypes(typeof(SimpleText), typeof(TextStyle))]
	public class SignedDistanceFieldComponent : NodeComponent
	{
		private const string GroupFont = "01. Face";
		private const string GroupOutline = "02. Outline";
		private const string GroupGradient = "03. Gradient";
		private const string GroupShadow = "04. Shadows";
		private const float MinimumDilate = -30f;
		private const float MaximumDilate = 30f;
		private const float MinimumThickness = 0f;
		private const float MaximumThickness = 30f;
		private const float MinimumLightAngle = 0f;
		private const float MaximumLightAngle = 360f;
		private const float MinimumReflectionPower = 0f;
		private const float MaximumReflectionPower = 100f;
		private const float MinimumBevelRoundness = 0f;
		private const float MaximumBevelRoundness = 5f;
		private const float MinimumBevelWidth = 0f;
		private const float MaximumBevelWidth = 30f;

		private float dilate = 0f;
		private float thickness = 0f;

		[YuzuMember]
		[TangerineGroup(GroupFont)]
		public float Dilate
		{
			get => dilate;
			set => dilate = Mathf.Clamp(value, MinimumDilate, MaximumDilate);
		}

		[YuzuMember]
		[TangerineGroup(GroupOutline)]
		public Color4 OutlineColor { get; set; } = Color4.Black;

		[YuzuMember]
		[TangerineGroup(GroupOutline)]
		public float Thickness
		{
			get => thickness;
			set => thickness = Mathf.Clamp(value, MinimumThickness, MaximumThickness);
		}

		[YuzuMember]
		[TangerineGroup(GroupGradient)]
		public bool GradientEnabled { get; set; }

		[YuzuMember]
		[TangerineGroup(GroupGradient)]
		public ColorGradient Gradient { get; set; } = new ColorGradient(Color4.White, Color4.Black);

		[YuzuMember]
		[TangerineGroup(GroupGradient)]
		public float GradientAngle { get; set; }

		[YuzuMember]
		[TangerineGroup(GroupShadow)]
		public List<ShadowParams> Shadows { get; set; }

		[YuzuMember]
		[TangerineGroup(GroupShadow)]
		public List<ShadowParams> InnerShadows { get; set; }

		[YuzuMember]
		[TangerineGroup(GroupShadow)]
		public List<ShadowParams> Overlays { get; set; }

		protected override void OnOwnerChanged(Node oldOwner)
		{
			
		}

		public override NodeComponent Clone()
		{
			var clone = (SignedDistanceFieldComponent)base.Clone();
			return clone;
		}
	}
}
