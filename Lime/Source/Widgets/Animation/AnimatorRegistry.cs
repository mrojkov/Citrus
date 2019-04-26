using System;
using System.Collections.Generic;

namespace Lime
{
	public sealed class AnimatorRegistry
	{
		public static AnimatorRegistry Instance { get; } = new AnimatorRegistry();

		public void Add(Type propertyType, IAnimatorFactory factory) => map.Add(propertyType, factory);

		public IAnimator CreateAnimator(Type propertyType)
		{
			return GetFactory(propertyType).CreateAnimator();
		}

		public IEasedAnimator CreateEasedAnimator(Type propertyType)
		{
			return GetFactory(propertyType).CreateEasedAnimator();
		}

		public IChainedAnimator CreateChainedAnimator(Type propertyType)
		{
			return GetFactory(propertyType).CreateChainedAnimator();
		}

		public IBlendedAnimator CreateBlendedAnimator(Type propertyType)
		{
			return GetFactory(propertyType).CreateBlendedAnimator();
		}

		private IAnimatorFactory GetFactory(Type propertyType)
		{
			if (!map.TryGetValue(propertyType, out var factory)) {
				if (propertyType.IsEnum) {
					factory = new EnumAnimatorFactory(propertyType);
					Add(propertyType, factory);
				} else {
					throw new Exception("Can't find animator type for property of " + propertyType.Name);
				}
			}
			return factory;
		}

		public AnimatorRegistry()
		{
			Add(typeof(Vector2), new Vector2AnimatorFactory());
			Add(typeof(Vector3), new Vector3AnimatorFactory());
			Add(typeof(Quaternion), new QuaternionAnimatorFactory());
			Add(typeof(Matrix44), new Matrix44AnimatorFactory());
			Add(typeof(float), new NumericAnimatorFactory());
			Add(typeof(Color4), new Color4AnimatorFactory());
			Add(typeof(string), new AnimatorFactory<string>());
			Add(typeof(int), new IntAnimatorFactory());
			Add(typeof(bool), new AnimatorFactory<bool>());
			Add(typeof(NumericRange), new NumericRangeAnimatorFactory());
			Add(typeof(Blending), new AnimatorFactory<Blending>());
			Add(typeof(ShaderId), new AnimatorFactory<ShaderId>());
			Add(typeof(ITexture), new AnimatorFactory<ITexture>());
			Add(typeof(RenderTarget), new AnimatorFactory<RenderTarget>());
			Add(typeof(ClipMethod), new AnimatorFactory<ClipMethod>());
			Add(typeof(SerializableSample), new AnimatorFactory<SerializableSample>());
			Add(typeof(SerializableFont), new AnimatorFactory<SerializableFont>());
			Add(typeof(EmitterShape), new AnimatorFactory<EmitterShape>());
			Add(typeof(AudioAction), new AnimatorFactory<AudioAction>());
			Add(typeof(MovieAction), new AnimatorFactory<MovieAction>());
			Add(typeof(Anchors), new AnimatorFactory<Anchors>());
			Add(typeof(EmissionType), new AnimatorFactory<EmissionType>());
			Add(typeof(ParticlesLinkage), new AnimatorFactory<ParticlesLinkage>());
			Add(typeof(TextOverflowMode), new AnimatorFactory<TextOverflowMode>());
			Add(typeof(NodeReference<Widget>), new AnimatorFactory<NodeReference<Widget>>());
			Add(typeof(NodeReference<Spline>), new AnimatorFactory<NodeReference<Spline>>());
			Add(typeof(NodeReference<Node3D>), new AnimatorFactory<NodeReference<Node3D>>());
			Add(typeof(NodeReference<Spline3D>), new AnimatorFactory<NodeReference<Spline3D>>());
			Add(typeof(NodeReference<Camera3D>), new AnimatorFactory<NodeReference<Camera3D>>());
			Add(typeof(LayoutDirection), new AnimatorFactory<LayoutDirection>());
			Add(typeof(Thickness), new ThicknessAnimatorFactory());
			Add(typeof(HAlignment), new AnimatorFactory<HAlignment>());
			Add(typeof(VAlignment), new AnimatorFactory<VAlignment>());
			Add(typeof(Alignment), new AnimatorFactory<Alignment>());
		}

		public bool Contains(Type propertyType) => propertyType.IsEnum || map.ContainsKey(propertyType);

		private Dictionary<Type, IAnimatorFactory> map = new Dictionary<Type, IAnimatorFactory>();

		public IEnumerable<Type> EnumerateRegisteredTypes()
		{
			foreach (var kv in map) {
				yield return kv.Key;
			}
		}

		public interface IAnimatorFactory
		{
			IAnimator CreateAnimator();
			IEasedAnimator CreateEasedAnimator();
			IChainedAnimator CreateChainedAnimator();
			IBlendedAnimator CreateBlendedAnimator();
		}

		public class AnimatorFactory<T> : IAnimatorFactory
		{
			public virtual IAnimator CreateAnimator() => new Animator<T>();
			public virtual IEasedAnimator CreateEasedAnimator() => new EasedAnimator<T>();
			public virtual IChainedAnimator CreateChainedAnimator() => new ChainedAnimator<T>();
			public virtual IBlendedAnimator CreateBlendedAnimator() => new BlendedAnimator<T>();
		}

		private class Vector2AnimatorFactory : AnimatorFactory<Vector2>
		{
			public override IAnimator CreateAnimator() => new Vector2Animator();
			public override IBlendedAnimator CreateBlendedAnimator() => new Vector2BlendedAnimator();
		}

		private class Vector3AnimatorFactory : AnimatorFactory<Vector3>
		{
			public override IAnimator CreateAnimator() => new Vector3Animator();
			public override IBlendedAnimator CreateBlendedAnimator() => new Vector2BlendedAnimator();
		}

		private class QuaternionAnimatorFactory : AnimatorFactory<Quaternion>
		{
			public override IAnimator CreateAnimator() => new QuaternionAnimator();
			public override IBlendedAnimator CreateBlendedAnimator() => new QuaternionBlendedAnimator();
		}

		private class Matrix44AnimatorFactory : AnimatorFactory<Matrix44>
		{
			public override IAnimator CreateAnimator() => new Matrix44Animator();
			public override IBlendedAnimator CreateBlendedAnimator() => new Matrix44BlendedAnimator();
		}

		private class NumericAnimatorFactory : AnimatorFactory<float>
		{
			public override IAnimator CreateAnimator() => new NumericAnimator();
			public override IBlendedAnimator CreateBlendedAnimator() => new NumericBlendedAnimator();
		}

		private class Color4AnimatorFactory : AnimatorFactory<Color4>
		{
			public override IAnimator CreateAnimator() => new Color4Animator();
			public override IBlendedAnimator CreateBlendedAnimator() => new Color4BlendedAnimator();
		}

		private class IntAnimatorFactory : AnimatorFactory<Color4>
		{
			public override IAnimator CreateAnimator() => new IntAnimator();
			public override IBlendedAnimator CreateBlendedAnimator() => new IntBlendedAnimator();
		}

		private class NumericRangeAnimatorFactory : AnimatorFactory<NumericRange>
		{
			public override IAnimator CreateAnimator() => new NumericRangeAnimator();
			public override IBlendedAnimator CreateBlendedAnimator() => new NumericRangeBlendedAnimator();
		}

		private class ThicknessAnimatorFactory : AnimatorFactory<ThicknessAnimator>
		{
			public override IAnimator CreateAnimator() => new ThicknessAnimator();
			public override IBlendedAnimator CreateBlendedAnimator() => new ThicknessBlendedAnimator();
		}

		public class EnumAnimatorFactory : IAnimatorFactory
		{
			private readonly Func<IAbstractAnimator> animatorFactory;
			private readonly Func<IAbstractAnimator> easedAnimatorFactory;
			private readonly Func<IAbstractAnimator> chainedAnimatorFactory;
			private readonly Func<IAbstractAnimator> blendedAnimatorFactory;

			public EnumAnimatorFactory(Type enumType)
			{
				animatorFactory = CreateDelegate(typeof(Animator<>), enumType);
				easedAnimatorFactory = CreateDelegate(typeof(EasedAnimator<>), enumType);
				chainedAnimatorFactory = CreateDelegate(typeof(ChainedAnimator<>), enumType);
				blendedAnimatorFactory = CreateDelegate(typeof(BlendedAnimator<>), enumType);
			}

			private Func<IAbstractAnimator> CreateDelegate(Type animatorType, Type enumType)
			{
				var t = animatorType.MakeGenericType(enumType);
				var ctr = t.GetConstructor(Type.EmptyTypes);
				return () => ctr.Invoke(new object[] { }) as IAbstractAnimator;
			}

			public IAnimator CreateAnimator() => (IAnimator)animatorFactory();
			public IEasedAnimator CreateEasedAnimator() => (IEasedAnimator)easedAnimatorFactory();
			public IChainedAnimator CreateChainedAnimator() => (IChainedAnimator)chainedAnimatorFactory();
			public IBlendedAnimator CreateBlendedAnimator() => (IBlendedAnimator)blendedAnimatorFactory();
		}
	}
}
