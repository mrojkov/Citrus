using System;
using Lime;
using System.Collections.Generic;
using ProtoBuf;

namespace Lemon
{
	[ProtoContract]
	public enum EmitterShape
	{
		[ProtoEnum]
		Point,
        [ProtoEnum]
		Line,
		[ProtoEnum]
		Ellipse,
		[ProtoEnum]
		Area,
	};

    [Flags]
	[ProtoContract]
	public enum EmissionType
	{
		[ProtoEnum]
		None,
		[ProtoEnum]
		Inner = 1,
		[ProtoEnum]
		Outer = 2,
	}
	
	[ProtoContract]
	public enum ParticlesLinkage
	{
		[ProtoEnum]
		Root,
		[ProtoEnum]
		Parent,
		[ProtoEnum]
		Other
	}

    [ProtoContract]
	public partial class ParticleEmitter : Widget
	{
        [ProtoContract]
		public class Particle
		{
            [ProtoMember(1)]
			public int ModifierIndex;
			// Position of particle with random motion.
            [ProtoMember(2)]
			public Vector2 FullPosition;
			// Position if particle without random motion.
            [ProtoMember(3)]
			public Vector2 RegularPosition;
			// Motion direction with random motion (in degrees).
            [ProtoMember(4)]
			public float FullDirection;
			// Motion direction without random motion (in degrees).
            [ProtoMember(5)]
			public float RegularDirection;
			// Veclocity of motion.
            [ProtoMember(6)]
			public float Velocity;
			// Velocity of changing motion direction (degrees/sec).
            [ProtoMember(7)]
			public float AngularVelocity;
			// Direction of particle windage (0 - right, 90 - down).
            [ProtoMember(8)]
			public float WindDirection;
			// Velocity of particle windage. 
            [ProtoMember(9)]
			public float WindAmount;
			// Direction of gravity (0 - right, 90 - down)
            [ProtoMember(10)]
			public float GravityDirection;
			// Strength of gravity.
            [ProtoMember(11)]
			public float GravityAmount;
			// Acceleration of gravity (calculated thru gravityAmount).
            [ProtoMember(12)]
			public float GravityAcceleration;
			// Velocity of the particle caused by gravity (calculated thru gravityAcceleration).
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
			// Velocity of particle rotation (degrees/sec).
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
			// Current offset of spline beginning (0..1).
            [ProtoMember(30)]
			public float RandomSplineOffset;
			// Current texture of the particle.
            [ProtoMember(31)]
			public float TextureIndex;
		};

		public static bool EnabledGlobally = true;

		/// <summary>
		/// Whether particles never die.
		/// </summary>
        [ProtoMember(1)]
		public bool ImmortalParticles;

		/// <summary>
		/// Shape of emitter.
		/// </summary>
        [ProtoMember(2)]
		public EmitterShape Shape { get; set; }

		/// <summary>
		/// Type of emission.
		/// </summary>
        [ProtoMember(3)]
		public EmissionType EmissionType { get; set; }

		/// <summary>
		/// Type of particles linkage.
		/// </summary>
        [ProtoMember(4)]
		public ParticlesLinkage ParticlesLinkage;

		/// <summary>
		/// A name of widget particles moving with (ParticlesLinkage = Other).
		/// </summary>
        [ProtoMember(5)]
		public string LinkageWidgetName;

		/// <summary>
		/// Number of spawned particles per second.
		/// </summary>
        [ProtoMember(6)]
		public float Number { get; set; }

		/// <summary>
		/// Offset of initial state emitter in seconds to make an effect of "particles explosion".
		/// </summary>
        [ProtoMember(7)]
		public float TimeShift;

		/// <summary>
		/// General speed of effect.
		/// </summary>
        [ProtoMember(8)]
		public float Speed { get; set; }

		/// <summary>
		/// Whether particles are oriented along track.
		/// </summary>
        [ProtoMember(9)]
		public bool AlongPathOrientation { get; set; }

		/// <summary>
		/// Specifiecs direction of particles windage (0 - right, 90 - down) .
		/// </summary>
        [ProtoMember(10)]
		public NumericRange WindDirection { get; set; }

		/// <summary>
		/// Specifiecs strength of particles windage. 
		/// </summary>
        [ProtoMember(11)]
		public NumericRange WindAmount { get; set; }

		/// <summary>
		/// Specifiecs direction of gravitation (0 - right, 90 - down) .
		/// </summary>
        [ProtoMember(12)]
		public NumericRange GravityDirection { get; set; }

		/// <summary>
		/// Specifiecs strength of gravitation. 
		/// </summary>
        [ProtoMember(13)]
		public NumericRange GravityAmount { get; set; }

		/// <summary>
		/// Specifiecs strength of magnets gravitation. 
		/// </summary>
        [ProtoMember(14)]
		public NumericRange MagnetAmount { get; set; }

		/// <summary>
		/// Specifies rotation of particle in degrees.
		/// </summary>
        [ProtoMember(15)]
		public NumericRange Orientation { get; set; }

		/// <summary>
		/// Specifies direction of particles motion (degrees).
		/// </summary>
        [ProtoMember(16)]
		public NumericRange Direction { get; set; }

		/// <summary>
		/// Specifies lifetime of particles in seconds.
		/// </summary>
        [ProtoMember(17)]
		public NumericRange Lifetime { get; set; }

		/// <summary>
		/// Specifies zoom of particles.
		/// </summary>
        [ProtoMember(18)]
		public NumericRange Zoom { get; set; }

		/// <summary>
		/// Specifies aspect ratio of particles (width/height).
		/// </summary>
        [ProtoMember(19)]
		public NumericRange AspectRatio { get; set; }

		/// <summary>
		/// Specifies velocity of particles.
		/// </summary>
        [ProtoMember(20)]
		public NumericRange Velocity { get; set; }

		/// <summary>
		/// Specifies velocity of rotation particle around the center.
		/// </summary>
        [ProtoMember(21)]
		public NumericRange Spin { get; set; }

		/// <summary>
		/// Specifies velocity of changing particle direction (degrees/second).
		/// </summary>
        [ProtoMember(22)]
		public NumericRange AngularVelocity { get; set; }

		/// <summary>
		/// Specifies radius within random motion has occurred.
		/// </summary>
        [ProtoMember(23)]
		public NumericRange RandomMotionRadius { get; set; }

		/// <summary>
		/// Specifies velocity of random motion.
		/// </summary>
        [ProtoMember(24)]
		public NumericRange RandomMotionSpeed { get; set; }

		/// <summary>
		/// Coefficient of random motion trajectory flatness.
		/// </summary>
        [ProtoMember(25)]
		public float RandomMotionAspectRatio { get; set; }

		/// <summary>
		/// Specifies an angle of control point rotation, in order to get new a control point.
		/// </summary>
        [ProtoMember(26)]
		public NumericRange RandomMotionRotation { get; set; }

        [ProtoMember(27)]
		public bool firstUpdate = true;
		
		[ProtoMember(28)]
		public float pendingParticles;
		
		[ProtoMember(29)]
		public readonly LinkedList<Particle> particles = new LinkedList<Particle> ();
		
		public ParticleEmitter ()
		{
			Shape = EmitterShape.Point;
			EmissionType = EmissionType.Outer;
			ParticlesLinkage = ParticlesLinkage.Parent;
			Number = 100;
			Speed = 1;
			Orientation = new NumericRange (0, 360);
			Direction = new NumericRange (0, 360);
			WindDirection = new NumericRange (0, 0);
			WindAmount = new NumericRange (0, 0);
			GravityDirection = new NumericRange (90, 0);
			GravityAmount = new NumericRange (0, 0);
			MagnetAmount = new NumericRange (0, 0);
			Lifetime = new NumericRange (1, 0);
			Zoom = new NumericRange (1, 0);
			AspectRatio = new NumericRange (1, 0);
			Velocity = new NumericRange (100, 0);
			Spin = new NumericRange (0, 0);
			AngularVelocity = new NumericRange (0, 0);
			RandomMotionRadius = new NumericRange (20, 0);
			RandomMotionSpeed = new NumericRange (0, 0);
			RandomMotionAspectRatio = 1;
			RandomMotionRotation = new NumericRange (0, 360);
			AlongPathOrientation = false;
			TimeShift = 0;
			ImmortalParticles = false;
		}

		Widget GetBasicWidget ()
		{
			switch (ParticlesLinkage) {
			case ParticlesLinkage.Parent:
				return (Parent != null) ? Parent.Widget : null;
			case ParticlesLinkage.Other:
				{
					Node node = Parent;
					while (node != null) {
						if (node.Id == LinkageWidgetName)
							return node.Widget;
						node = node.Parent;
					}
					return null;
				}
			case ParticlesLinkage.Root:
			default:
				{
					return (Parent != null) ? GetRoot ().Widget : null;
				}
			}
		}
	
		public static int TotalParticles = 0;
		public static bool GloballyEnabled = true;
	
		void UpdateHelper (int delta)
		{
			float deltaSec = delta * Speed / 1000.0f;
			if (ImmortalParticles) {
				if (TimeShift > 0)
					pendingParticles += Number * deltaSec / TimeShift;
				else
					pendingParticles = Number;
				pendingParticles = Math.Min (pendingParticles, Number - particles.Count);

				while (particles.Count > Number) {
					TotalParticles--;
					particles.RemoveLast ();
				}
			} else {
				pendingParticles += Number * deltaSec;
			}

			while (pendingParticles >= 1f) {
				Particle particle = new Particle ();
				if (GloballyEnabled && InitializeParticle (particle)) {
					TotalParticles++;
					AdvanceParticle (particle, 0);
					particles.AddLast (particle);
				}
				pendingParticles -= 1;
			}

			EnumerateMagnets ();

			LinkedListNode<Particle > node = particles.First;
			for (; node != null; node = node.Next) {
				Particle particle = node.Value;
				AdvanceParticle (particle, deltaSec);
				if (!ImmortalParticles && particle.Age > particle.Lifetime) {
					LinkedListNode<Particle > n = node.Next;
					particles.Remove (node);
					TotalParticles--;
					node = n;
					if (node == null)
						break;
				}
			}
		}

		public override void Update (int delta)
		{
			base.Update (delta);
			if (firstUpdate) {
				firstUpdate = false;
				const int ModellingStep = 40;
				delta = Math.Max (delta, (int)(TimeShift * 1000));
				while (delta >= ModellingStep) {
					UpdateHelper (ModellingStep);
					delta -= ModellingStep;
				}
				if (delta > 0)
					UpdateHelper (delta);
			} else
				UpdateHelper (delta);
		}

		Vector2 GenerateRandomMotionControlPoint (ref float rayDirection)
		{
			rayDirection += RandomMotionRotation.UniformRandomNumber ();
			Vector2 result = Vector2.CosSin (Utils.DegreesToRadians * rayDirection);
			NumericRange radius = RandomMotionRadius;
			if (radius.Variation == 0)
				radius.Variation = radius.Median;
			result *= Math.Abs (radius.NormalRandomNumber ());
			if (RandomMotionAspectRatio != 1f && RandomMotionAspectRatio > 0f) {
				result.X *= RandomMotionAspectRatio;
				result.Y /= RandomMotionAspectRatio;
			}
			return result;
		}

		bool InitializeParticle (Particle p)
		{
			if (Nodes.Count == 0)
				return false;

			// Calculating particle initial orientation & color
			Color4 color = Color;
			Matrix32 transform = LocalMatrix;

			Widget basicWidget = GetBasicWidget ();
			if (basicWidget != null) {
				for (Node node = Parent; node != basicWidget; node = node.Parent) {
					if (node.Widget != null) {
						transform *= node.Widget.LocalMatrix;
						color *= node.Widget.Color;
					}
				}
			}
			float emitterScaleAmount = 1;
			Vector2 emitterScale = new Vector2 ();
			emitterScale.X = transform.U.Length;
			emitterScale.Y = transform.V.Length;
			float crossProduct = Vector2.CrossProduct (transform.U, transform.V);
			if (crossProduct < 0.0f)
				emitterScale.Y = -emitterScale.Y;
			emitterScaleAmount = (float)Math.Sqrt (Math.Abs (crossProduct));
			float emitterAngle = transform.U.Atan2 * Utils.RadiansToDegrees;

			NumericRange aspectRatioVariationPair = new NumericRange (0, Math.Max (0.0f, AspectRatio.Variation));
			float zoom = Zoom.NormalRandomNumber ();
			float aspectRatio = Math.Max (0.00001f, AspectRatio.Median *
                (1 + Math.Abs (aspectRatioVariationPair.NormalRandomNumber ())) /
                (1 + Math.Abs (aspectRatioVariationPair.NormalRandomNumber ())));

			p.TextureIndex = 0.0f;
			p.Velocity = Velocity.NormalRandomNumber () * emitterScaleAmount;
			p.ScaleInitial = Vector2.Scale (emitterScale, new Vector2 (zoom * aspectRatio, zoom / aspectRatio));
			p.ScaleCurrent = p.ScaleInitial;
			p.WindDirection = WindDirection.UniformRandomNumber ();
			p.WindAmount = WindAmount.NormalRandomNumber () * emitterScaleAmount;
			p.GravityVelocity = 0.0f;
			p.GravityAcceleration = 0.0f;
			p.GravityAmount = GravityAmount.NormalRandomNumber () * emitterScaleAmount;
			p.GravityDirection = GravityDirection.NormalRandomNumber ();
			p.MagnetAmountInitial = MagnetAmount.NormalRandomNumber ();
			p.Lifetime = Math.Max (Lifetime.NormalRandomNumber (), 0.1f);
			p.Age = 0.0f;
			p.AngularVelocity = AngularVelocity.NormalRandomNumber ();
			p.Angle = Orientation.UniformRandomNumber () + emitterAngle;
			p.Spin = Spin.NormalRandomNumber ();
			p.ColorInitial = color;
			p.ColorCurrent = color;
			p.RandomRayDirection = (new NumericRange (0, 360)).UniformRandomNumber ();
			p.RandomSplineVertex0 = GenerateRandomMotionControlPoint (ref p.RandomRayDirection);
			p.RandomSplineVertex1 = Vector2.Zero;
			p.RandomSplineVertex2 = GenerateRandomMotionControlPoint (ref p.RandomRayDirection);
			p.RandomSplineVertex3 = GenerateRandomMotionControlPoint (ref p.RandomRayDirection);
			p.RandomMotionSpeed = RandomMotionSpeed.NormalRandomNumber ();
			p.RandomSplineOffset = 0;

			Vector2 position;
			switch (Shape) {
			case EmitterShape.Point:
				position = 0.5f * Size;
				p.RegularDirection = Direction.UniformRandomNumber () + emitterAngle - 90.0f;
				break;
			case EmitterShape.Line:
				position = new Vector2 (Utils.Random () * Size.X, Size.Y * 0.5f);
				p.RegularDirection = Direction.UniformRandomNumber () + emitterAngle - 90.0f;
				break;
			case EmitterShape.Ellipse:
				{
					float angle = Utils.Random () * 2 * Utils.Pi;
					Vector2 sincos = Vector2.CosSin (angle);
					position = 0.5f * Vector2.Scale ((sincos + Vector2.One), Size);
					p.RegularDirection = Direction.UniformRandomNumber () + emitterAngle - 90 + angle;
				}
				break;
			case EmitterShape.Area:
				position.X = Utils.Random () * Size.X;
				position.Y = Utils.Random () * Size.Y;
				p.RegularDirection = Direction.UniformRandomNumber () + emitterAngle - 90.0f;
				break;
			default:
				throw new Lime.Exception ("Unknown emitter shape");
			}

			p.RegularPosition = transform.TransformVector (position);
			p.ModifierIndex = -1;
			for (int counter = 0; counter < 10; counter++) {
				int i = Utils.Random (Nodes.Count);
				ParticleModifier modifier = Nodes [i] as ParticleModifier;
				if (modifier != null) {
					p.ModifierIndex = i;
					break;
				}
			}
			if (p.ModifierIndex < 0)
				return false;

			if (EmissionType == EmissionType.Inner)
				p.RegularDirection += 180;
			else if ((EmissionType & EmissionType.Inner) != 0) {
				if (Utils.RandomFlag ())
					p.RegularDirection += 180;
			} else if (EmissionType == 0)
				return false;

			p.FullDirection = p.RegularDirection;
			p.FullPosition = p.RegularPosition;
			return true;
		}

		bool AdvanceParticle (Particle p, float delta)
		{
			var modifier = Nodes [p.ModifierIndex] as ParticleModifier;
			int duration = modifier.Animators.GetOverallDuration ();
			if (duration > 0)
				modifier.Animators.Apply ((int)(p.Age / p.Lifetime * duration));

			p.Age += delta;
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
				p.TextureIndex += delta * Math.Max (0, modifier.AnimationFps);
				if (modifier.LoopedAnimation) {
					float upLimit = modifier.LastFrame + 1.0f;
					while (p.TextureIndex > upLimit)
						p.TextureIndex -= upLimit - modifier.FirstFrame;
				} else
					p.TextureIndex = Math.Min (p.TextureIndex, modifier.LastFrame);
				p.TextureIndex = Math.Max (p.TextureIndex, modifier.FirstFrame);
			} else {
				p.TextureIndex -= delta * Math.Max (0, modifier.AnimationFps);
				if (modifier.LoopedAnimation) {
					float downLimit = modifier.LastFrame - 1f;
					while (p.TextureIndex < downLimit)
						p.TextureIndex += modifier.FirstFrame - downLimit;
				} else
					p.TextureIndex = Math.Max (p.TextureIndex, modifier.LastFrame);
				p.TextureIndex = Math.Min (p.TextureIndex, modifier.FirstFrame);
			}

			// Updating other properties of a particle.
			var windDirection = Vector2.CosSin (Utils.DegreesToRadians * p.WindDirection);
			float windVelocity = p.WindAmount * modifier.WindAmount;

			var gravityDirection = Vector2.CosSin (Utils.DegreesToRadians * p.GravityDirection);

			var direction = Vector2.CosSin (Utils.DegreesToRadians * p.RegularDirection);
			float velocity = p.Velocity * modifier.Velocity;

			p.RegularDirection += p.AngularVelocity * modifier.AngularVelocity * delta;
			p.GravityAcceleration += p.GravityAmount * modifier.GravityAmount * delta;
			p.GravityVelocity += p.GravityAcceleration * delta;

			p.RegularPosition += velocity * delta * direction;
			p.RegularPosition += windVelocity * delta * windDirection;
			p.RegularPosition += p.GravityVelocity * delta * gravityDirection;

			p.Angle += p.Spin * modifier.Spin * delta;
			p.ScaleCurrent.X = p.ScaleInitial.X * modifier.Scale * modifier.AspectRatio;
			p.ScaleCurrent.Y = p.ScaleInitial.Y * modifier.Scale / Math.Max (0.0001f, modifier.AspectRatio);

			p.ColorCurrent = p.ColorInitial * modifier.Color;

			p.MagnetAmountCurrent = p.MagnetAmountInitial * modifier.MagnetAmount;

			ApplyMagnetsToParticle (p, delta);

			Vector2 positionOnSpline = Vector2.Zero;
			if (p.RandomMotionSpeed > 0.0f) {
				p.RandomSplineOffset += delta * p.RandomMotionSpeed;

				if (p.RandomSplineOffset >= 1.0f) {
					p.RandomSplineOffset = 0.0f;
					p.RandomSplineVertex0 = p.RandomSplineVertex1;
					p.RandomSplineVertex1 = p.RandomSplineVertex2;
					p.RandomSplineVertex2 = p.RandomSplineVertex3;
					p.RandomSplineVertex3 = GenerateRandomMotionControlPoint (ref p.RandomRayDirection);
				}

				positionOnSpline = Utils.CatmullRomSpline (p.RandomSplineOffset,
                    p.RandomSplineVertex0, p.RandomSplineVertex1,
                    p.RandomSplineVertex2, p.RandomSplineVertex3);
			}

			Vector2 previousPosition = p.FullPosition;
			p.FullPosition = p.RegularPosition + positionOnSpline;

			if (AlongPathOrientation) {
				Vector2 deltaPos = p.FullPosition - previousPosition;
				if (deltaPos.SquaredLength > 0.00001f)
					p.FullDirection = deltaPos.Atan2 * Utils.RadiansToDegrees;
			}
			return true;
		}

		static readonly Vector2[] rect = new Vector2[4] { 
            new Vector2(0, 0), 
            new Vector2(1, 0), 
            new Vector2(1, 1), 
            new Vector2(0, 1) 
        };
		static Renderer.Vertex[] quad = new Renderer.Vertex[4];

		void RenderParticle (Particle p, Color4 color)
		{
			color = color * p.ColorCurrent;
			if (Color.A > 0) {
				var modifier = Nodes [p.ModifierIndex] as ParticleModifier;
				PersistentTexture texture = modifier.GetTexture ((int)p.TextureIndex - 1);
				float angle = p.Angle;
				if (AlongPathOrientation)
					angle += p.FullDirection;
				Vector2 particleSize = Vector2.Scale (p.ScaleCurrent, (Vector2)texture.ImageSize);
				Matrix32 rotation = Matrix32.Rotation (Utils.DegreesToRadians * angle);
				for (int i = 0; i < 4; i++) {
					quad [i].Pos = Vector2.Scale (particleSize, rect [i] - Vector2.Half) * rotation + p.FullPosition;
					quad [i].UV1 = rect [i];
					quad [i].Color = color;
				}
				Renderer.Instance.DrawTriangleFan (texture, quad, 4);
			}
		}

		public override void Render ()
		{
			Matrix32 matrix = Matrix32.Identity;
			Color4 color = Color4.White;
			Widget basicWidget = GetBasicWidget ();
			if (basicWidget != null) {
				matrix = basicWidget.WorldMatrix;
				color = basicWidget.WorldColor;
			}

			Renderer.Instance.WorldMatrix = matrix;
			Renderer.Instance.Blending = WorldBlending;

			LinkedListNode<Particle > node = particles.First;
			for (; node != null; node = node.Next) {
				Particle particle = node.Value;
				RenderParticle (particle, color);
			}
		}
	}
}
