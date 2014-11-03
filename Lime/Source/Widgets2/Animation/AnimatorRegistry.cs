using System;
using System.Collections.Generic;
using Lime;

namespace Lime.Widgets2
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
			Add(typeof(float), typeof(NumericAnimator));
			Add(typeof(Color4), typeof(Color4Animator));
			Add(typeof(string), typeof(Animator<string>));
			Add(typeof(bool), typeof(Animator<bool>));
			Add(typeof(NumericRange), typeof(Animator<NumericRange>));
			Add(typeof(Blending), typeof(Animator<Blending>));
			Add(typeof(ShaderId), typeof(Animator<ShaderId>));
			Add(typeof(ITexture), typeof(Animator<ITexture>));
			Add(typeof(SerializableSample), typeof(Animator<SerializableSample>));
			Add(typeof(EmitterShape), typeof(Animator<EmitterShape>));
			Add(typeof(AudioAction), typeof(Animator<AudioAction>));
			Add(typeof(MovieAction), typeof(Animator<MovieAction>));
			Add(typeof(HAlignment), typeof(Animator<HAlignment>));
			Add(typeof(VAlignment), typeof(Animator<VAlignment>));
		}
		
		Dictionary<Type, Type> map = new Dictionary<Type, Type>();
	}
}