using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	[AllowedComponentOwnerTypes(typeof(SimpleText), typeof(RichText))]
	class SignedDistanceFieldComponent : NodeBehavior
	{
		private const string GroupFont = "01. Face";
		private const string GroupOutline = "02. Outline";
		private const float MinimumSoftness = 0f;
		private const float MaximumSoftness = 1f;
		private const float MinimumDilate = -0.5f;
		private const float MaximumDilate = 0.5f;
		private const float MinimumThickness = 0f;
		private const float MaximumThickness = 1f;

		internal SDFMaterialProvider SDFMaterialProvider { get; private set; } = new SDFMaterialProvider();
		internal SDFOutlineMaterialProvider OutlineMaterialProvider { get; private set; } = new SDFOutlineMaterialProvider();

		private SDFPresenter presenter = new SDFPresenter();
		private SDFRenderChainBuilder renderChainBuilder = new SDFRenderChainBuilder();
		private float softness = 0f;
		private float outlineSoftness = 0f;
		private float dilate = 0f;
		private float thickness = 0f;

		[YuzuMember]
		[TangerineGroup(GroupFont)]
		public float Softness
		{
			get => softness;
			set => softness = Mathf.Clamp(value, MinimumSoftness, MaximumSoftness);
		}

		[YuzuMember]
		[TangerineGroup(GroupFont)]
		public float Dilate
		{
			get => dilate;
			set => dilate = Mathf.Clamp(value, MinimumDilate, MaximumDilate);
		}

		[YuzuMember]
		[TangerineGroup(GroupOutline)]
		public bool OutlineEnabled { get; set; }

		[YuzuMember]
		[TangerineGroup(GroupOutline)]
		public Color4 OutlineColor { get; set; } = new Color4(0, 0, 0, 255);

		[YuzuMember]
		[TangerineGroup(GroupOutline)]
		public float Thickness
		{
			get => thickness;
			set => thickness = Mathf.Clamp(value, MinimumThickness, MaximumThickness);
		}
		[YuzuMember]
		[TangerineGroup(GroupOutline)]
		public float OutlineSoftness
		{
			get => outlineSoftness;
			set => outlineSoftness = Mathf.Clamp(value, MinimumSoftness, MaximumSoftness);
		}

		public void GetOwnerRenderObjects(RenderChain renderChain, RenderObjectList roObjects)
		{
			DettachFromNode(Owner);
			Owner.AddToRenderChain(renderChain);
			renderChain.GetRenderObjects(roObjects);
			AttachToNode(Owner);
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
			node.Presenter = presenter;
			node.RenderChainBuilder = renderChainBuilder;
			renderChainBuilder.Owner = node.AsWidget;
		}

		private void DettachFromNode(Node node)
		{
			node.RenderChainBuilder = node;
			node.Presenter = DefaultPresenter.Instance;
			renderChainBuilder.Owner = null;
		}

		public override NodeComponent Clone()
		{
			var clone = (SignedDistanceFieldComponent)base.Clone();
			clone.presenter = (SDFPresenter)presenter.Clone();
			clone.renderChainBuilder = (SDFRenderChainBuilder)renderChainBuilder.Clone(null);
			clone.SDFMaterialProvider = (SDFMaterialProvider)SDFMaterialProvider.Clone();
			clone.OutlineMaterialProvider = (SDFOutlineMaterialProvider)OutlineMaterialProvider.Clone();
			return clone;
		}
	}
}
