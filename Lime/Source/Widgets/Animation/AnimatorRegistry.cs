using System;
using System.Collections.Generic;
using Lime;

namespace Lime
{
	public sealed class AnimatorRegistry
	{
		static readonly AnimatorRegistry instance = new AnimatorRegistry();

		public static AnimatorRegistry Instance {
			get { return instance; }
		}
		
		public void Add(Type propertyType, Type animatorType)
		{
			map.Add(propertyType, animatorType);
		}
		
		public IAnimator CreateAnimator(Type propertyType)
		{
			Type animatorType;
			if (!map.TryGetValue(propertyType, out animatorType))
				throw new Lime.Exception("Can't find animator type for property of {0}", propertyType.Name);
			var ctr = animatorType.GetConstructor(System.Type.EmptyTypes);
			var animator = ctr.Invoke(new object[] {}) as IAnimator;
			return animator;
		}

		public AnimatorRegistry()
		{
			Add(typeof(Vector2), typeof(Vector2Animator));
			Add(typeof(Vector3), typeof(Vector3Animator));
			Add(typeof(Quaternion), typeof(QuaternionAnimator));
			Add(typeof(Matrix44), typeof(Matrix44Animator));
			Add(typeof(float), typeof(NumericAnimator));
			Add(typeof(Color4), typeof(Color4Animator));
			Add(typeof(string), typeof(Animator<string>));
			Add(typeof(bool), typeof(Animator<bool>));
			Add(typeof(NumericRange), typeof(Animator<NumericRange>));
			Add(typeof(Blending), typeof(Animator<Blending>));
			Add(typeof(ShaderId), typeof(Animator<ShaderId>));
			Add(typeof(ITexture), typeof(Animator<ITexture>));
			Add(typeof(RenderTarget), typeof(Animator<RenderTarget>));
			Add(typeof(ClipMethod), typeof(Animator<ClipMethod>));
			Add(typeof(SerializableSample), typeof(Animator<SerializableSample>));
			Add(typeof(SerializableFont), typeof(Animator<SerializableFont>));
			Add(typeof(EmitterShape), typeof(Animator<EmitterShape>));
			Add(typeof(AudioAction), typeof(Animator<AudioAction>));
			Add(typeof(MovieAction), typeof(Animator<MovieAction>));
			Add(typeof(HAlignment), typeof(Animator<HAlignment>));
			Add(typeof(VAlignment), typeof(Animator<VAlignment>));
			Add(typeof(Anchors), typeof(Animator<Anchors>));
			Add(typeof(EmissionType), typeof(Animator<EmissionType>));
			Add(typeof(ParticlesLinkage), typeof(Animator<ParticlesLinkage>));
			Add(typeof(TextOverflowMode), typeof(Animator<TextOverflowMode>));
			Add(typeof(NodeReference<Widget>), typeof(Animator<NodeReference<Widget>>));
			Add(typeof(NodeReference<Spline>), typeof(Animator<NodeReference<Spline>>));
			Add(typeof(NodeReference<Node3D>), typeof(Animator<NodeReference<Node3D>>));
			Add(typeof(NodeReference<Spline3D>), typeof(Animator<NodeReference<Spline3D>>));
			Add(typeof(NodeReference<Camera3D>), typeof(Animator<NodeReference<Camera3D>>));
		}
		
		Dictionary<Type, Type> map = new Dictionary<Type, Type>();
	}
}