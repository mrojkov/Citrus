using Yuzu;
using Lime.SignedDistanceField;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Lime
{
	public class ShadowParams
	{
		internal enum Type
		{
			Base,
			Inner,
		}

		private const float MinimumSoftness = 0f;
		private const float MaximumSoftness = 45f;
		private const float MinimumDilate = -40f;
		private const float MaximumDilate = 40f;

		private float softness = 0f;
		private float dilate = 0f;
		private int offsetX;
		private int offsetY;
		private Color4 color = Color4.Black;

		[YuzuMember]
		public bool Enabled { get; set; } = true;

		[YuzuMember]
		public Color4 Color
		{
			get => color;
			set {
				if (color != value) {
					materialProvider = null;
				}
				color = value;
			}
		}

		[YuzuMember]
		public int OffsetX
		{
			get => offsetX;
			set {
				if (offsetX != value) {
					materialProvider = null;
				}
				offsetX = value;
			}
		}

		[YuzuMember]
		public int OffsetY
		{
			get => offsetY;
			set {
				if (offsetY != value) {
					materialProvider = null;
				}
				offsetY = value;
			}
		}

		[YuzuMember]
		public float Softness
		{
			get => softness;
			set {
				var clamped = Mathf.Clamp(value, MinimumSoftness, MaximumSoftness);
				if (softness != clamped) {
					materialProvider = null;
				}
				softness = clamped;
			}
		}

		[YuzuMember]
		public float Dilate
		{
			get => dilate;
			set {
				var clamped = Mathf.Clamp(value, MinimumDilate, MaximumDilate);
				if (dilate != clamped) {
					materialProvider = null;
				}
				dilate = clamped;
			}
		}

		[TangerineIgnore]
		internal SignedDistanceFieldComponent OwnerComponent;

		[TangerineIgnore]
		internal Type ShadowType;

		private Sprite.IMaterialProvider materialProvider;

		[TangerineIgnore]
		internal Sprite.IMaterialProvider MaterialProvider
		{
			get {
				Invalidate();
				return materialProvider;
			}
		}

		public void InvalidateMaterial()
		{
			materialProvider = null;
		}

		private void Invalidate()
		{
			if (materialProvider != null) {
				return;
			}

			switch (ShadowType) {
				case Type.Base:
					var shadowKey = new SDFShadowMaterialKey() {
						Dilate = Dilate,
						Softness = Softness,
						Color = Color,
						Offset = new Vector2(offsetX, offsetY) * 0.1f
					};
					materialProvider = SDFMaterialProviderPool.Instance.GetShadowProvider(shadowKey);
					break;
				case Type.Inner:
					var innerShadowKey = new SDFInnerShadowMaterialKey() {
						Dilate = Dilate,
						TextDilate = OwnerComponent.Dilate,
						Softness = Softness,
						Color = Color,
						Offset = new Vector2(offsetX, offsetY) * 0.0001f
					};
					materialProvider = SDFMaterialProviderPool.Instance.GetInnerShadowProvider(innerShadowKey);
					break;
				default:
					break;
			}
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
		private Color4 outlineColor = Color4.Black;
		private bool gradientEnabled;
		private ColorGradient gradient = new ColorGradient(Color4.White, Color4.Black);
		private float gradientAngle;

		private SDFMaterialProvider materialProvider;

		[TangerineIgnore]
		internal SDFMaterialProvider MaterialProvider
		{
			get
			{
				Invalidate();
				return materialProvider;
			}
		}

		[YuzuMember]
		[TangerineGroup(GroupFont)]
		public float Dilate
		{
			get => dilate;
			set {
				var clamped =  Mathf.Clamp(value, MinimumDilate, MaximumDilate);
				if (dilate != clamped) {
					materialProvider = null;
					foreach (var shadow in InnerShadows) {
						shadow.InvalidateMaterial();
					}
				}
				dilate = clamped;
			}
		}

		[YuzuMember]
		[TangerineGroup(GroupOutline)]
		public Color4 OutlineColor
		{
			get => outlineColor;
			set {
				if (outlineColor != value) {
					materialProvider = null;
				}
				outlineColor = value;
			}
		}

		[YuzuMember]
		[TangerineGroup(GroupOutline)]
		public float Thickness
		{
			get => thickness;
			set {
				var clamped = Mathf.Clamp(value, MinimumThickness, MaximumThickness);
				if (thickness != clamped) {
					materialProvider = null;
				}
				thickness = clamped;
			}
		}

		[YuzuMember]
		[TangerineGroup(GroupGradient)]
		public bool GradientEnabled
		{
			get => gradientEnabled;
			set {
				if (gradientEnabled != value) {
					materialProvider = null;
				}
				gradientEnabled = value;
			}
		}

		[YuzuMember]
		[TangerineGroup(GroupGradient)]
		public ColorGradient Gradient
		{
			get => gradient;
			set {
				if (gradient.GetHashCode() != value.GetHashCode()) {
					materialProvider = null;
				}
				gradient = value;
			}
		}

		[YuzuMember]
		[TangerineGroup(GroupGradient)]
		public float GradientAngle
		{
			get => gradientAngle;
			set {
				if (gradientAngle != value) {
					materialProvider = null;
				}
				gradientAngle = value;
			}
		}

		[YuzuMember]
		[TangerineGroup(GroupShadow)]
		public List<ShadowParams> Shadows { get; set; }

		[YuzuMember]
		[TangerineGroup(GroupShadow)]
		public InnerShadowCollection InnerShadows { get; private set; }

		[YuzuMember]
		[TangerineGroup(GroupShadow)]
		public List<ShadowParams> Overlays { get; set; }

		public SignedDistanceFieldComponent()
		{
			InnerShadows = new InnerShadowCollection(this);
			Invalidate();
		}

		protected override void OnOwnerChanged(Node oldOwner)
		{
			base.OnOwnerChanged(oldOwner);
			if (oldOwner != null) {
				DettachFromNode(oldOwner);
			}
			if (Owner != null) {
				AttachToNode(Owner);
			}
		}

		private void AttachToNode(Node node)
		{
			if (node is SimpleText) {
				node.Presenter = new SDFSimpleTextPresenter();
			}
		}

		private void DettachFromNode(Node node)
		{
			if (node is SimpleText) {
				node.Presenter = DefaultPresenter.Instance;
			}
		}

		private void Invalidate()
		{
			if (materialProvider != null) {
				return;
			}
			var key = new SDFMaterialKey() {
				Dilate = Dilate,
				Thickness = Thickness,
				OutlineColor = OutlineColor,
				GradientEnabled = GradientEnabled,
				Gradient = Gradient,
				GradientAngle = GradientAngle
			};

			materialProvider = SDFMaterialProviderPool.Instance.GetProvider(key);
		}

		public override NodeComponent Clone()
		{
			var clone = (SignedDistanceFieldComponent)base.Clone();
			return clone;
		}

		public class InnerShadowCollection : ObservableCollection<ShadowParams>
		{
			private readonly SignedDistanceFieldComponent owner;

			public InnerShadowCollection(SignedDistanceFieldComponent owner)
			{
				this.owner = owner;
			}

			protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
			{
				if (e.Action == NotifyCollectionChangedAction.Add) {
					foreach (var item in e.NewItems) {
						var shadow = item as ShadowParams;
						shadow.OwnerComponent = owner;
						shadow.ShadowType = ShadowParams.Type.Inner;
					}
				}
				base.OnCollectionChanged(e);
			}
		}
	}
}
