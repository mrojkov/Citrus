using System;
using System.Collections.Generic;
using Lime;

namespace Lime
{
	public sealed class AnimatorRegistry
	{
		static readonly AnimatorRegistry instance = new AnimatorRegistry ();

		public static AnimatorRegistry Instance {
			get { return instance; }
		}
		
		public void Add (Type propertyType, Type animatorType)
		{
			dict.Add (propertyType, animatorType);
		}
		
		public Animator CreateAnimator (Type propertyType)
		{
			Type animatorType;
			if (!dict.TryGetValue (propertyType, out animatorType))
				throw new Lime.Exception ("Can't find animator type for property of {0}", propertyType.Name);
			var ctr = animatorType.GetConstructor (System.Type.EmptyTypes);
			var animator = ctr.Invoke (new object[] {}) as Animator;
			return animator;
		}

		public AnimatorRegistry ()
		{
			Add (typeof(Vector2), typeof(Vector2Animator));
			Add (typeof(float), typeof(NumericAnimator));
			Add (typeof(Color4), typeof(Color4Animator));
			Add (typeof(string), typeof(GenericAnimator<string>));
			Add (typeof(bool), typeof(GenericAnimator<bool>));
			Add (typeof(NumericRange), typeof(GenericAnimator<NumericRange>));
			Add (typeof(Blending), typeof(GenericAnimator<Blending>));
			Add (typeof(SerializableTexture), typeof(GenericAnimator<SerializableTexture>));
			Add (typeof(SerializableSound), typeof(GenericAnimator<SerializableSound>));
			Add (typeof(EmitterShape), typeof(GenericAnimator<EmitterShape>));
			Add (typeof(AudioAction), typeof(GenericAnimator<AudioAction>));
		}
		
		Dictionary<Type, Type> dict = new Dictionary<Type, Type> ();
	}
}

