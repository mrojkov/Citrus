using Yuzu;
using Lime.SignedDistanceField;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections;

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
		private const float MinimumSoftness = 0f;
		private const float MaximumSoftness = 40f;
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

		private float softness = 0f;
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
		public float Softness
		{
			get => softness;
			set {
				var clamped = Mathf.Clamp(value, MinimumDilate, MaximumDilate);
				if (softness != clamped) {
					materialProvider = null;
					foreach (var shadow in InnerShadows) {
						shadow.InvalidateMaterial();
					}
				}
				softness = clamped;
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
		public ShadowCollection Shadows { get; private set; }

		[YuzuMember]
		[TangerineGroup(GroupShadow)]
		public ShadowCollection InnerShadows { get; private set; }

		[YuzuMember]
		[TangerineGroup(GroupShadow)]
		public ShadowCollection Overlays { get; private set; }

		public SignedDistanceFieldComponent()
		{
			Shadows = new ShadowCollection(this, ShadowParams.Type.Base);
			InnerShadows = new ShadowCollection(this, ShadowParams.Type.Inner);
			Overlays = new ShadowCollection(this, ShadowParams.Type.Base);
			Invalidate();
		}

		private void Invalidate()
		{
			if (materialProvider != null) {
				return;
			}
			var key = new SDFMaterialKey() {
				Softness = Softness,
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

		public class ShadowCollection : IList<ShadowParams>
		{
			private readonly List<ShadowParams> list = new List<ShadowParams>();
			private readonly SignedDistanceFieldComponent owner;
			private readonly ShadowParams.Type type;

			internal ShadowCollection(SignedDistanceFieldComponent owner, ShadowParams.Type shadowType)
			{
				this.owner = owner;
				type = shadowType;
			}

			private void RuntimeChecksBeforeInsertion(ShadowParams shadowParams)
			{
				if (shadowParams.OwnerComponent != null) {
					throw new Lime.Exception("Can't adopt a ShadowParams twice.");
				}
			}

			public ShadowParams this[int index]
			{
				get
				{
					return list[index];
				}
				set {
					RuntimeChecksBeforeInsertion(value);
					value.OwnerComponent = owner;
					var oldNode = list[index];
					oldNode.OwnerComponent = null;
					list[index] = value;
				}
			}

			public int Count => list.Count;

			public bool IsReadOnly => false;

			public void Add(ShadowParams item)
			{
				RuntimeChecksBeforeInsertion(item);
				item.OwnerComponent = owner;
				item.ShadowType = type;
				list.Add(item);
			}

			public void Clear()
			{
				foreach (var item in list) {
					item.OwnerComponent = null;
				}
				list.Clear();
			}

			public bool Contains(ShadowParams item)
			{
				return IndexOf(item) >= 0;
			}

			public void CopyTo(ShadowParams[] array, int arrayIndex)
			{
				list.CopyTo(array, arrayIndex);
			}

			public int IndexOf(ShadowParams item)
			{
				return list.IndexOf(item);
			}

			public void Insert(int index, ShadowParams item)
			{
				RuntimeChecksBeforeInsertion(item);
				item.OwnerComponent = owner;
				item.ShadowType = type;
				list.Insert(index, item);
			}

			public bool Remove(ShadowParams item)
			{
				int index = IndexOf(item);
				if (index >= 0) {
					RemoveAt(index);
					return true;
				}
				return false;
			}

			public void RemoveAt(int index)
			{
				list[index].OwnerComponent = null;
				list.RemoveAt(index);
			}

			public List<ShadowParams>.Enumerator GetEnumerator() => list.GetEnumerator();

			IEnumerator<ShadowParams> IEnumerable<ShadowParams>.GetEnumerator() => GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}
