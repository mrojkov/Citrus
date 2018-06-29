using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Yuzu;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Inspector
{
	public class InspectorPropertyRegistry
	{
		public readonly List<RegistryItem> Items;

		public static readonly InspectorPropertyRegistry Instance = new InspectorPropertyRegistry();

		static bool AllowChildren(PropertyEditorParams context)
		{
			return context.Objects.Any(o => NodeCompositionValidator.CanHaveChildren(o.GetType()));
		}

		InspectorPropertyRegistry()
		{
			Items = new List<RegistryItem>();
			AddEditor(c => c.PropertyName == "ContentsPath", c => AllowChildren(c) ? new ContentsPathPropertyEditor(c) : null);
			AddEditor(c => c.PropertyName == "Trigger", c => AllowChildren(c) ? new TriggerPropertyEditor(c) : null);
			AddEditor(typeof(Vector2), c => new Vector2PropertyEditor(c));
			AddEditor(typeof(Vector3), c => new Vector3PropertyEditor(c));
			AddEditor(typeof(Quaternion), c => new QuaternionPropertyEditor(c));
			AddEditor(typeof(NumericRange), c => new NumericRangePropertyEditor(c));
			AddEditor(c => c.PropertyName == "Text", c => new StringPropertyEditor(c, multiline: true));
			AddEditor(typeof(string), c => new StringPropertyEditor(c));
			AddEditor(typeof(float), c => new FloatPropertyEditor(c));
			AddEditor(typeof(bool), c => new BooleanPropertyEditor(c));
			AddEditor(typeof(int), c => new IntPropertyEditor(c));
			AddEditor(typeof(Color4), c => new Color4PropertyEditor(c));
			AddEditor(typeof(Anchors), c => new AnchorsPropertyEditor(c));
			AddEditor(typeof(Blending), c => new BlendingPropertyEditor(c));
			AddEditor(typeof(ShaderId), c => new EnumPropertyEditor<ShaderId>(c));
			AddEditor(typeof(RenderTarget), c => new RenderTargetPropertyEditor(c));
			AddEditor(typeof(ClipMethod), c => new EnumPropertyEditor<ClipMethod>(c));
			AddEditor(typeof(CameraProjectionMode), c => new EnumPropertyEditor<CameraProjectionMode>(c));
			AddEditor(c => {
				return
					c.Objects.Count == 1 &&
					c.PropertyInfo.PropertyType == typeof(ITexture) &&
					c.PropertyInfo.GetValue(c.Objects[0])?.GetType() == typeof(RenderTexture);
			}, c => new RenderTexturePropertyEditor(c));
			AddEditor(typeof(ITexture), c => new TexturePropertyEditor(c));
			AddEditor(typeof(SerializableSample), c => new AudioSamplePropertyEditor(c));
			AddEditor(typeof(SerializableFont), c => new FontPropertyEditor(c));
			AddEditor(typeof(HAlignment), c => new EnumPropertyEditor<HAlignment>(c));
			AddEditor(typeof(VAlignment), c => new EnumPropertyEditor<VAlignment>(c));
			AddEditor(typeof(AudioAction), c => new EnumPropertyEditor<AudioAction>(c));
			AddEditor(typeof(MovieAction), c => new EnumPropertyEditor<MovieAction>(c));
			AddEditor(typeof(EmitterShape), c => new EnumPropertyEditor<EmitterShape>(c));
			AddEditor(typeof(EmissionType), c => new EnumPropertyEditor<EmissionType>(c));
			AddEditor(typeof(ParticlesLinkage), c => new EnumPropertyEditor<ParticlesLinkage>(c));
			AddEditor(typeof(TextOverflowMode), c => new EnumPropertyEditor<TextOverflowMode>(c));
			AddEditor(typeof(ShadowMapTextureQuality), c => new EnumPropertyEditor<ShadowMapTextureQuality>(c));
			AddEditor(typeof(NodeReference<Camera3D>), c => new NodeReferencePropertyEditor<Camera3D>(c));
			AddEditor(typeof(NodeReference<LightSource>), c => new NodeReferencePropertyEditor<LightSource>(c));
			AddEditor(typeof(NodeReference<Image>), c => new NodeReferencePropertyEditor<Image>(c));
			AddEditor(typeof(NodeReference<Spline>), c => new NodeReferencePropertyEditor<Spline>(c));
			AddEditor(typeof(NodeReference<Widget>), c => new NodeReferencePropertyEditor<Widget>(c));
			AddEditor(typeof(NodeReference<Node3D>), c => new NodeReferencePropertyEditor<Node3D>(c));
			AddEditor(typeof(NodeReference<Spline3D>), c => new NodeReferencePropertyEditor<Spline3D>(c));
			AddEditor(typeof(SkinningWeights), c => new SkinningWeightsPropertyEditor(c));
			AddEditor(typeof(CullMode), c => new EnumPropertyEditor<CullMode>(c));
		}

		void AddEditor(Type type, PropertyEditorBuilder builder)
		{
			Items.Add(new RegistryItem(c => c.PropertyInfo.PropertyType == type, builder));
		}

		void AddEditor(Func<PropertyEditorParams, bool> condition, PropertyEditorBuilder builder)
		{
			Items.Add(new RegistryItem(condition, builder));
		}

		public class RegistryItem
		{
			public readonly Func<PropertyEditorParams, bool> Condition;
			public readonly PropertyEditorBuilder Builder;

			public RegistryItem(Func<PropertyEditorParams, bool> condition, PropertyEditorBuilder builder)
			{
				Condition = condition;
				Builder = builder;
			}
		}
	}
}
