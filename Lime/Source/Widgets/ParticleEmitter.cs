using Lime;
using ProtoBuf;
using System;
using System.Collections.Generic;

namespace Lime
{
	/// <summary>
	/// Форма эмиттера частиц
	/// </summary>
	[ProtoContract]
	public enum EmitterShape
	{
		/// <summary>
		/// Точечный. Частицы генерируются в точке
		/// </summary>
		[ProtoEnum]
		Point,

		/// <summary>
		/// Линейный. Частицы генерируются равномерно по линии, длина которой соответствует ширине эмиттера
		/// </summary>
		[ProtoEnum]
		Line,

		/// <summary>
		/// Частицы генерируются равномерно по границам эллипса, описанного по границам эмиттера
		/// </summary>
		[ProtoEnum]
		Ellipse,

		/// <summary>
		/// Частицы генерируются равномерно по области эмиттера
		/// </summary>
		[ProtoEnum]
		Area,
	};

	/// <summary>
	/// Тип генерации частиц
	/// </summary>
	[Flags]
	[ProtoContract]
	public enum EmissionType
	{
		/// <summary>
		/// Частицы генерироваться не будут
		/// </summary>
		[ProtoEnum]
		None,

		/// <summary>
		/// Частицы генерируются внутрь области эмиттера
		/// </summary>
		[ProtoEnum]
		Inner = 1,

		/// <summary>
		/// Частицы генерируются наружу области эмиттера
		/// </summary>
		[ProtoEnum]
		Outer = 2,
	}

	/// <summary>
	/// Тип привязки частиц. Привязанные частицы двигаются вместе с объектом, к которому они привязаны
	/// </summary>
	[ProtoContract]
	public enum ParticlesLinkage
	{
		/// <summary>
		/// Частицы не привязаны ни к какому объекту (стандартное поведение)
		/// </summary>
		[ProtoEnum]
		Root,

		/// <summary>
		/// Частицы привязаны к контейнеру, в котором находится эмиттер
		/// </summary>
		[ProtoEnum]
		Parent,

		/// <summary>
		/// Частицы привязаны к объекту, указанному пользователем
		/// </summary>
		[ProtoEnum]
		Other
	}

	/// <summary>
	/// Эмиттер частиц (специальный невидимый объект, из которого вылетают частицы)
	/// </summary>
	[ProtoContract]
	public partial class ParticleEmitter : Widget
	{
		internal static System.Random Rng = new System.Random();

		/// <summary>
		/// Частица, сгенерированная эмиттером
		/// </summary>
		[ProtoContract]
		public class Particle
		{
			[ProtoMember(1)]
			public int ModifierIndex;

			public ParticleModifier Modifier;

			// Position of particle with random motion.
			[ProtoMember(2)]
			public Vector2 FullPosition;

			// Position if particle without random motion.
			[ProtoMember(3)]
			public Vector2 RegularPosition;

			// Motion direction with random motion(in degrees).
			[ProtoMember(4)]
			public float FullDirection;

			// Motion direction without random motion(in degrees).
			[ProtoMember(5)]
			public float RegularDirection;

			// Veclocity of motion.
			[ProtoMember(6)]
			public float Velocity;

			// Velocity of changing motion direction(degrees/sec).
			[ProtoMember(7)]
			public float AngularVelocity;

			// Direction of particle windage(0 - right, 90 - down).
			[ProtoMember(8)]
			public float WindDirection;

			// Velocity of particle windage.
			[ProtoMember(9)]
			public float WindAmount;

			// Direction of gravity(0 - right, 90 - down)
			[ProtoMember(10)]
			public float GravityDirection;

			// Strength of gravity.
			[ProtoMember(11)]
			public float GravityAmount;

			// Acceleration of gravity(calculated thru gravityAmount).
			[ProtoMember(12)]
			public float GravityAcceleration;

			// Velocity of the particle caused by gravity(calculated thru gravityAcceleration).
			[ProtoMember(13)]
			public float GravityVelocity;

			// Strength of magnet's gravity at the moment of particle birth.
			[ProtoMember(14)]
			public float MagnetAmountInitial;

			// Strength of magnet's gravity in the current moment.
			[ProtoMember(15)]
			public float MagnetAmountCurrent;

			// Scale of particle at the moment of particle birth.
			[ProtoMember(16)]
			public Vector2 ScaleInitial;

			// Scale of particle in the current moment.
			[ProtoMember(17)]
			public Vector2 ScaleCurrent;

			// Rotation of particle relative to its center.
			[ProtoMember(18)]
			public float Angle;

			// Velocity of particle rotation(degrees/sec).
			[ProtoMember(19)]
			public float Spin;

			// Age of particle in seconds.
			[ProtoMember(20)]
			public float Age;

			// Full life time of particle in seconds.
			[ProtoMember(21)]
			public float Lifetime;

			// Color of the particle at the moment of birth.
			[ProtoMember(22)]
			public Color4 ColorInitial;

			// Current color of the particle.
			[ProtoMember(23)]
			public Color4 ColorCurrent;

			// Velocty of random motion.
			[ProtoMember(24)]
			public float RandomMotionSpeed;

			// Splined path of random particle motion.
			[ProtoMember(25)]
			public Vector2 RandomSplineVertex0;

			[ProtoMember(26)]
			public Vector2 RandomSplineVertex1;

			[ProtoMember(27)]
			public Vector2 RandomSplineVertex2;

			[ProtoMember(28)]
			public Vector2 RandomSplineVertex3;

			// Current angle of spline control point, relative to center of random motion.
			[ProtoMember(29)]
			public float RandomRayDirection;

			// Current offset of spline beginning(0..1).
			[ProtoMember(30)]
			public float RandomSplineOffset;

			// Current texture of the particle.
			[ProtoMember(31)]
			public float TextureIndex;

			// modifier.Animators.OverallDuration / LifeTime
			[ProtoMember(32)]
			public float AgeToFrame;
		};

		public static bool EnabledGlobally = true;

		/// <summary>
		/// Сгенерированные частицы будут жить вечно. Генерация происходит один раз
		/// </summary>
		[ProtoMember(1)]
		public bool ImmortalParticles;

		/// <summary>
		/// Форма эмиттера
		/// </summary>
		[ProtoMember(2)]
		public EmitterShape Shape { get; set; }

		/// <summary>
		/// Тип генерации частиц
		/// </summary>
		[ProtoMember(3)]
		public EmissionType EmissionType { get; set; }

		/// <summary>
		///  Тип привязки частиц. Привязанные частицы двигаются вместе с объектом, к которому они привязаны
		/// </summary>
		[ProtoMember(4)]
		public ParticlesLinkage ParticlesLinkage;

		/// <summary>
		/// Имя виджета, к которому привязаны частицы (имеет смысл только при ParticlesLinkage = Other)
		/// </summary>
		[ProtoMember(5)]
		public string LinkageWidgetName;

		/// <summary>
		/// Количество сгенерированных частиц в секунду
		/// </summary>
		[ProtoMember(6)]
		public float Number { get; set; }

		/// <summary>
		/// На сколько кадров раньше эмиттер начнет генерацию.
		/// Используется для того, чтобы в 0 кадре частицы была уже сгенерирована
		/// (Например значение 3 означает, что эмиттер начнет генерацию на 3 кадра раньше)
		/// </summary>
		[ProtoMember(7)]
		public float TimeShift;

		/// <summary>
		/// Скорость жизненного цикла частицы
		/// </summary>
		[ProtoMember(8)]
		public float Speed { get; set; }

		/// <summary>
		/// Whether particles are oriented along track.
		/// </summary>
		[ProtoMember(9)]
		public bool AlongPathOrientation { get; set; }

		/// <summary>
		/// Направление ветра, градусы (0 - вправо, 90 - вниз)
		/// </summary>
		[ProtoMember(10)]
		public NumericRange WindDirection { get; set; }

		/// <summary>
		/// Сила ветра
		/// </summary>
		[ProtoMember(11)]
		public NumericRange WindAmount { get; set; }

		/// <summary>
		/// Направление гравитации, градусы (0 - вправо, 90 - вниз)
		/// </summary>
		[ProtoMember(12)]
		public NumericRange GravityDirection { get; set; }

		/// <summary>
		/// Сила гравитации
		/// </summary>
		[ProtoMember(13)]
		public NumericRange GravityAmount { get; set; }

		/// <summary>
		/// Задает силу притяжения магнита
		/// </summary>
		[ProtoMember(14)]
		public NumericRange MagnetAmount { get; set; }

		/// <summary>
		/// Задает угол поворота сгенерированных частиц
		/// </summary>
		[ProtoMember(15)]
		public NumericRange Orientation { get; set; }

		/// <summary>
		/// Угол поворота эмиттера, градусы (поворот по часовой стрелке)
		/// </summary>
		[ProtoMember(16)]
		public NumericRange Direction { get; set; }

		/// <summary>
		/// Время жизни частицы в секундах
		/// </summary>
		[ProtoMember(17)]
		public NumericRange Lifetime { get; set; }

		/// <summary>
		/// Масштаб сгенерированных частиц
		/// </summary>
		[ProtoMember(18)]
		public NumericRange Zoom { get; set; }

		/// <summary>
		/// Задает соотношение ширины и длины частицы (для создания вытянутых частиц)
		/// </summary>
		[ProtoMember(19)]
		public NumericRange AspectRatio { get; set; }

		/// <summary>
		/// Скорость движения частицы
		/// </summary>
		[ProtoMember(20)]
		public NumericRange Velocity { get; set; }

		/// <summary>
		/// Скорость вращения частиц
		/// </summary>
		[ProtoMember(21)]
		public NumericRange Spin { get; set; }

		/// <summary>
		/// Скорость изменения направления движения по кругу (градусы в секунду)
		/// </summary>
		[ProtoMember(22)]
		public NumericRange AngularVelocity { get; set; }

		/// <summary>
		/// Радиус окружности, в котором частицы хаотично двигаются
		/// </summary>
		[ProtoMember(23)]
		public NumericRange RandomMotionRadius { get; set; }

		/// <summary>
		/// Скорость хаотичного движения частиц
		/// </summary>
		[ProtoMember(24)]
		public NumericRange RandomMotionSpeed { get; set; }

		/// <summary>
		/// Коэффицент 'плоской формы' для траектории заотичного движения
		/// (задает распространение хаотичного движения по нескольким осям)
		/// </summary>
		[ProtoMember(25)]
		public float RandomMotionAspectRatio { get; set; }

		/// <summary>
		/// Скорость вращения частиц при хаотичном движении
		/// </summary>
		[ProtoMember(26)]
		public NumericRange RandomMotionRotation { get; set; }

		/// <summary>
		/// Флаг, сбрасывающийся в false после первого обновления
		/// </summary>
		[ProtoMember(27)]
		public bool firstUpdate = true;

		/// <summary>
		/// Количество частиц, которое должен сгенерировать эмиттер в цикле обновления (Update)
		/// Используется, чтобы количество сгенерированных частиц не зависело от FPS
		/// </summary>
		[ProtoMember(28)]
		public float particlesToSpawn;

		/// <summary>
		/// Список сгенерированных частиц
		/// </summary>
		[ProtoMember(29)]
		public LinkedList<Particle> particles = new LinkedList<Particle>();

		private static LinkedList<Particle> particlePool = new LinkedList<Particle>();

		public ParticleEmitter()
		{
			Shape = EmitterShape.Point;
			EmissionType = EmissionType.Outer;
			ParticlesLinkage = ParticlesLinkage.Parent;
			Number = 100;
			Speed = 1;
			Orientation = new NumericRange(0, 360);
			Direction = new NumericRange(0, 360);
			WindDirection = new NumericRange(0, 0);
			WindAmount = new NumericRange(0, 0);
			GravityDirection = new NumericRange(90, 0);
			GravityAmount = new NumericRange(0, 0);
			MagnetAmount = new NumericRange(0, 0);
			Lifetime = new NumericRange(1, 0);
			Zoom = new NumericRange(1, 0);
			AspectRatio = new NumericRange(1, 0);
			Velocity = new NumericRange(100, 0);
			Spin = new NumericRange(0, 0);
			AngularVelocity = new NumericRange(0, 0);
			RandomMotionRadius = new NumericRange(20, 0);
			RandomMotionSpeed = new NumericRange(0, 0);
			RandomMotionAspectRatio = 1;
			RandomMotionRotation = new NumericRange(0, 360);
			AlongPathOrientation = false;
			TimeShift = 0;
			ImmortalParticles = false;
		}

		/// <summary>
		/// Для ProtoBuf. Действия, выполняемые после десериализации
		/// </summary>
		[ProtoAfterDeserialization]
		public void AfterDeserialization()
		{
			// If particle was deserialized, particle.Modifier would be null. In some cases
			// particles are rendered before update, but particle.Modifier is required for
			// the render. AdvanceParticle() sets particle.Modifier.
			foreach (var particle in particles) {
				AdvanceParticle(particle, 0);
			}
		}

		public override Node DeepCloneFast()
		{
			// Do not clone particle instances
			var savedParticles = particles;
			particles = new LinkedList<Particle>();
			var clone = base.DeepCloneFast() as ParticleEmitter;
			particles = savedParticles;
			return clone;
		}

		private Widget GetBasicWidget()
		{
			switch (ParticlesLinkage) {
			case ParticlesLinkage.Parent:
				return (Parent != null && !ParentWidget.IsRenderedToTexture()) ?
					ParentWidget : null;

			case ParticlesLinkage.Other: {
				var widget = ParentWidget;
				while (widget != null) {
					if (widget.Id == LinkageWidgetName)
						return widget;
					widget = widget.ParentWidget;
				}
				return null;
			}
			case ParticlesLinkage.Root:
			default:
				return (Parent != null) ? WidgetContext.Current.Root : null;
			}
		}

		public static int NumberOfUpdatedParticles = 0;
		public static bool GloballyEnabled = true;

		private LinkedListNode<Particle> AllocParticle()
		{
			LinkedListNode<Particle> result;
			if (particlePool.Count == 0) {
				result = new LinkedListNode<Particle>(new Particle());
			} else {
				result = particlePool.First;
				particlePool.RemoveFirst();
			}
			particles.AddLast(result);
			return result;
		}

		private void FreeParticle(LinkedListNode<Particle> particleNode)
		{
			particles.Remove(particleNode);
			particlePool.AddFirst(particleNode);
		}

		private void UpdateHelper(float delta)
		{
			delta *= Speed;
			if (ImmortalParticles) {
				if (TimeShift > 0)
					particlesToSpawn += Number * delta / TimeShift;
				else
					particlesToSpawn = Number;
				particlesToSpawn = Math.Min(particlesToSpawn, Number - particles.Count);
				while (particles.Count > Number) {
					FreeParticle(particles.Last);
				}
			} else {
				particlesToSpawn += Number * delta;
			}
			while (particlesToSpawn >= 1f) {
				LinkedListNode<Particle> particleNode = AllocParticle();
				if (GloballyEnabled && Nodes.Count > 0 && InitializeParticle(particleNode.Value)) {
					AdvanceParticle(particleNode.Value, 0);
				} else {
					FreeParticle(particleNode);
				}
				particlesToSpawn -= 1;
			}
			if (MagnetAmount.Median != 0 || MagnetAmount.Dispersion != 0) {
				EnumerateMagnets();
			}
			LinkedListNode<Particle> p = particles.First;
			for (; p != null; p = p.Next) {
				Particle particle = p.Value;
				AdvanceParticle(particle, delta);
				if (!ImmortalParticles && particle.Age > particle.Lifetime) {
					LinkedListNode<Particle> n = p.Next;
					FreeParticle(p);
					p = n;
					if (p == null)
						break;
				}
			}
		}

		protected override void SelfLateUpdate(float delta)
		{
			if (firstUpdate) {
				firstUpdate = false;
				const float ModellingStep = 0.004f;
				delta = Math.Max(delta, TimeShift);
				while (delta >= ModellingStep) {
					UpdateHelper(ModellingStep);
					delta -= ModellingStep;
				}
				if (delta > 0)
					UpdateHelper(delta);
			} else
				UpdateHelper(delta);
		}

		private Vector2 GenerateRandomMotionControlPoint(ref float rayDirection)
		{
			rayDirection += RandomMotionRotation.UniformRandomNumber(Rng);
			Vector2 result = Vector2.CosSinRough(rayDirection * Mathf.DegToRad);
			NumericRange radius = RandomMotionRadius;
			if (radius.Dispersion == 0)
				radius.Dispersion = radius.Median;
			result *= Math.Abs(radius.NormalRandomNumber(Rng));
			if (RandomMotionAspectRatio != 1f && RandomMotionAspectRatio > 0f) {
				result.X *= RandomMotionAspectRatio;
				result.Y /= RandomMotionAspectRatio;
			}
			return result;
		}

		private bool InitializeParticle(Particle p)
		{
			Color4 color;
			Matrix32 transform;
			CalcInitialColorAndTransform(out color, out transform);
			float emitterScaleAmount = 1;
			Vector2 emitterScale = new Vector2();
			emitterScale.X = transform.U.Length;
			emitterScale.Y = transform.V.Length;
			float crossProduct = Vector2.CrossProduct(transform.U, transform.V);
			if (crossProduct < 0.0f)
				emitterScale.Y = -emitterScale.Y;
			emitterScaleAmount = (float)Math.Sqrt(Math.Abs(crossProduct));
			float emitterAngle = transform.U.Atan2Deg;

			NumericRange aspectRatioVariationPair = new NumericRange(0, Math.Max(0.0f, AspectRatio.Dispersion));
			float zoom = Zoom.NormalRandomNumber(Rng);
			float aspectRatio = Math.Max(0.00001f, AspectRatio.Median *
				(1 + Math.Abs(aspectRatioVariationPair.NormalRandomNumber(Rng))) /
				(1 + Math.Abs(aspectRatioVariationPair.NormalRandomNumber(Rng))));

			p.TextureIndex = 0.0f;
			p.Velocity = Velocity.NormalRandomNumber(Rng) * emitterScaleAmount;
			p.ScaleInitial = emitterScale * new Vector2(zoom * aspectRatio, zoom / aspectRatio);
			p.ScaleCurrent = p.ScaleInitial;
			p.WindDirection = WindDirection.UniformRandomNumber(Rng);
			p.WindAmount = WindAmount.NormalRandomNumber(Rng) * emitterScaleAmount;
			p.GravityVelocity = 0.0f;
			p.GravityAcceleration = 0.0f;
			p.GravityAmount = GravityAmount.NormalRandomNumber(Rng) * emitterScaleAmount;
			p.GravityDirection = GravityDirection.NormalRandomNumber(Rng);
			p.MagnetAmountInitial = MagnetAmount.NormalRandomNumber(Rng);
			p.Lifetime = Math.Max(Lifetime.NormalRandomNumber(Rng), 0.1f);
			p.Age = 0.0f;
			p.AngularVelocity = AngularVelocity.NormalRandomNumber(Rng);
			p.Angle = Orientation.UniformRandomNumber(Rng) + emitterAngle;
			p.Spin = Spin.NormalRandomNumber(Rng);
			p.ColorInitial = color;
			p.ColorCurrent = color;
			p.RandomRayDirection = (new NumericRange(0, 360)).UniformRandomNumber(Rng);
			p.RandomSplineVertex0 = GenerateRandomMotionControlPoint(ref p.RandomRayDirection);
			p.RandomSplineVertex1 = Vector2.Zero;
			p.RandomSplineVertex2 = GenerateRandomMotionControlPoint(ref p.RandomRayDirection);
			p.RandomSplineVertex3 = GenerateRandomMotionControlPoint(ref p.RandomRayDirection);
			p.RandomMotionSpeed = RandomMotionSpeed.NormalRandomNumber(Rng);
			p.RandomSplineOffset = 0;

			Vector2 position;
			switch (Shape) {
			case EmitterShape.Point:
				position = 0.5f * Size;
				p.RegularDirection = Direction.UniformRandomNumber(Rng) + emitterAngle - 90.0f;
				break;

			case EmitterShape.Line:
				position = new Vector2(Rng.RandomFloat() * Size.X, Size.Y * 0.5f);
				p.RegularDirection = Direction.UniformRandomNumber(Rng) + emitterAngle - 90.0f;
				break;

			case EmitterShape.Ellipse:
				float angle = Rng.RandomFloat(0, 360);
				Vector2 sincos = Vector2.CosSinRough(angle * Mathf.DegToRad);
				position = 0.5f * ((sincos + Vector2.One) * Size);
				p.RegularDirection = Direction.UniformRandomNumber(Rng) + emitterAngle - 90 + angle;
				break;

			case EmitterShape.Area:
				position.X = Rng.RandomFloat() * Size.X;
				position.Y = Rng.RandomFloat() * Size.Y;
				p.RegularDirection = Direction.UniformRandomNumber(Rng) + emitterAngle - 90.0f;
				break;

			default:
				throw new Lime.Exception("Invalid particle emitter shape");
			}

			p.RegularPosition = transform.TransformVector(position);
			p.ModifierIndex = -1;
			p.Modifier = null;
			for (int counter = 0; counter < 10; counter++) {
				int i = Rng.RandomInt(Nodes.Count);
				p.Modifier = Nodes[i] as ParticleModifier;
				if (p.Modifier != null) {
					p.ModifierIndex = i;
					break;
				}
			}
			if (p.ModifierIndex < 0)
				return false;

			int duration = p.Modifier.Animators.GetOverallDuration();
			p.AgeToFrame = duration / p.Lifetime;

			if (EmissionType == EmissionType.Inner)
				p.RegularDirection += 180;
			else if ((EmissionType & EmissionType.Inner) != 0) {
				if (Rng.RandomInt(2) == 0)
					p.RegularDirection += 180;
			} else if (EmissionType == 0)
				return false;

			p.FullDirection = p.RegularDirection;
			p.FullPosition = p.RegularPosition;
			return true;
		}

		private void CalcInitialColorAndTransform(out Color4 color, out Matrix32 transform)
		{
			color = Color;
			transform = CalcLocalToParentTransform();
			Widget basicWidget = GetBasicWidget();
			if (basicWidget != null) {
				for (Node node = Parent; node != null && node != basicWidget; node = node.Parent) {
					if (node.AsWidget != null) {
						transform *= node.AsWidget.CalcLocalToParentTransform();
						color *= node.AsWidget.Color;
					}
				}
			}
		}

		private bool AdvanceParticle(Particle p, float delta)
		{
			NumberOfUpdatedParticles++;
			p.Age += delta;
			// If particle was deserialized, p.Modifier would be null.
			if (p.Modifier == null) {
				p.Modifier = Nodes[p.ModifierIndex] as ParticleModifier;
			}
			var modifier = p.Modifier;

			if (p.AgeToFrame > 0) {
				p.Modifier.Animators.Apply(AnimationUtils.FramesToMsecs((int)(p.Age * p.AgeToFrame)));
			}
			if (ImmortalParticles) {
				if (p.Lifetime > 0.0f)
					p.Age = p.Age % p.Lifetime;
			}
			// Updating a particle texture index.
			if (p.TextureIndex == 0.0f)
				p.TextureIndex = (float)modifier.FirstFrame;

			if (modifier.FirstFrame == modifier.LastFrame) {
				p.TextureIndex = (float)modifier.FirstFrame;
			} else if (modifier.FirstFrame < modifier.LastFrame) {
				p.TextureIndex += delta * Math.Max(0, modifier.AnimationFps);
				if (modifier.LoopedAnimation) {
					float upLimit = modifier.LastFrame + 1.0f;
					while (p.TextureIndex > upLimit)
						p.TextureIndex -= upLimit - modifier.FirstFrame;
				} else
					p.TextureIndex = Math.Min(p.TextureIndex, modifier.LastFrame);
				p.TextureIndex = Math.Max(p.TextureIndex, modifier.FirstFrame);
			} else {
				p.TextureIndex -= delta * Math.Max(0, modifier.AnimationFps);
				if (modifier.LoopedAnimation) {
					float downLimit = modifier.LastFrame - 1f;
					while (p.TextureIndex < downLimit)
						p.TextureIndex += modifier.FirstFrame - downLimit;
				} else
					p.TextureIndex = Math.Max(p.TextureIndex, modifier.LastFrame);
				p.TextureIndex = Math.Min(p.TextureIndex, modifier.FirstFrame);
			}

			// Updating other properties of a particle.
			float windVelocity = p.WindAmount * modifier.WindAmount;
			if (windVelocity != 0) {
				var windDirection = Vector2.CosSinRough(p.WindDirection * Mathf.DegToRad);
				p.RegularPosition += windVelocity * delta * windDirection;
			}
			if (p.GravityVelocity != 0) {
				var gravityDirection = Vector2.CosSinRough(p.GravityDirection * Mathf.DegToRad);
				p.RegularPosition += p.GravityVelocity * delta * gravityDirection;
			}
			var direction = Vector2.CosSinRough(p.RegularDirection * Mathf.DegToRad);
			float velocity = p.Velocity * modifier.Velocity;

			p.RegularDirection += p.AngularVelocity * modifier.AngularVelocity * delta;
			p.GravityAcceleration += p.GravityAmount * modifier.GravityAmount * delta;
			p.GravityVelocity += p.GravityAcceleration * delta;

			p.RegularPosition += velocity * delta * direction;
			p.Angle += p.Spin * modifier.Spin * delta;

			p.ScaleCurrent = p.ScaleInitial * modifier.Scale;
			if (modifier.AspectRatio != 1f) {
				p.ScaleCurrent.X *= modifier.AspectRatio;
				p.ScaleCurrent.Y /= Math.Max(0.0001f, modifier.AspectRatio);
			}
			p.ColorCurrent = p.ColorInitial * modifier.Color;

			p.MagnetAmountCurrent = p.MagnetAmountInitial * modifier.MagnetAmount;

			ApplyMagnetsToParticle(p, delta);

			Vector2 positionOnSpline = Vector2.Zero;
			if (p.RandomMotionSpeed > 0.0f) {
				p.RandomSplineOffset += delta * p.RandomMotionSpeed;
				while (p.RandomSplineOffset >= 1.0f) {
					p.RandomSplineOffset -= 1.0f;
					p.RandomSplineVertex0 = p.RandomSplineVertex1;
					p.RandomSplineVertex1 = p.RandomSplineVertex2;
					p.RandomSplineVertex2 = p.RandomSplineVertex3;
					p.RandomSplineVertex3 = GenerateRandomMotionControlPoint(ref p.RandomRayDirection);
				}
				positionOnSpline = Mathf.CatmullRomSpline(p.RandomSplineOffset,
					p.RandomSplineVertex0, p.RandomSplineVertex1,
					p.RandomSplineVertex2, p.RandomSplineVertex3);
			}

			Vector2 previousPosition = p.FullPosition;
			p.FullPosition = p.RegularPosition + positionOnSpline;

			if (AlongPathOrientation) {
				Vector2 deltaPos = p.FullPosition - previousPosition;
				if (deltaPos.SqrLength > 0.00001f)
					p.FullDirection = deltaPos.Atan2Deg;
			}
			return true;
		}

		private void RenderParticle(Particle p, Matrix32 matrix, Color4 color)
		{
			if (p.ColorCurrent.A <= 0) {
				return;
			}
			float angle = p.Angle;
			if (AlongPathOrientation) {
				angle += p.FullDirection;
			}
			ITexture texture = p.Modifier.GetTexture((int)p.TextureIndex - 1);
			var imageSize = (Vector2)texture.ImageSize;
			var particleSize = p.ScaleCurrent * imageSize;
			var orientation = Vector2.CosSinRough(angle * Mathf.DegToRad);
			var perpendicularOrientation = new Vector2(-orientation.Y, orientation.X);
			var globalMatrix = new Matrix32 {
				U = particleSize.X * orientation,
				V = particleSize.Y * perpendicularOrientation,
				T = p.FullPosition
			};
			Renderer.Transform1 = globalMatrix * matrix;
			Renderer.DrawSprite(texture, p.ColorCurrent, -Vector2.Half, Vector2.One, Vector2.Zero, Vector2.One);
		}

		public override void Render()
		{
			Matrix32 matrix = Matrix32.Identity;
			Color4 color = Color4.White;
			Widget basicWidget = GetBasicWidget();
			if (basicWidget != null) {
				matrix = basicWidget.LocalToWorldTransform;
				color = basicWidget.GlobalColor;
			}
			Renderer.Blending = GlobalBlending;
			Renderer.Shader = GlobalShader;
			LinkedListNode<Particle> node = particles.First;
			for (; node != null; node = node.Next) {
				Particle particle = node.Value;
				RenderParticle(particle, matrix, color);
			}
		}

		public void DeleteAllParticles()
		{
			while (particles.Count > 0) {
				FreeParticle(particles.Last);
			}
		}
	}
}
