using System;
using Yuzu;

namespace Lime
{
	[TangerineRegisterNode(Order = 9)]
	public class ParticlesMagnet : Widget
	{
		public ParticlesMagnet()
		{
			RenderChainBuilder = null;
			Shape = EmitterShape.Area;
			Attenuation = 0;
			Strength = 1000;
		}

		[YuzuMember]
		[TangerineKeyframeColor(27)]
		public EmitterShape Shape { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(28)]
		public float Attenuation { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(29)]
		public float Strength { get; set; }
	}

	public partial class ParticleEmitter : Widget
	{
		struct MagnetData
		{
			public ParticlesMagnet Magnet;
			public Matrix32 PrecalcTransitionMatrix;
			public Matrix32 PrecalcInvTransitionMatrix;
		}

		const int MaxMagnets = 20;
		int numMagnets;
		static MagnetData[] magnets = new MagnetData[MaxMagnets];

		void EnumerateMagnets()
		{
			numMagnets = 0;
			if (Parent == null)
				return;
			foreach (Node node in Parent.Nodes) {
				ParticlesMagnet magnet = node as ParticlesMagnet;
				if (magnet != null) {
					if (numMagnets >= MaxMagnets)
						break;
					Matrix32 transform = magnet.CalcLocalToParentTransform();
					Widget basicWidget = GetBasicWidget();
					if (basicWidget != null) {
						for (Node n = Parent; n != basicWidget; n = n.Parent) {
							if (n.AsWidget != null)
								transform *= n.AsWidget.CalcLocalToParentTransform();
						}
					}
					magnets[numMagnets++] = new MagnetData {
						Magnet = magnet,
						PrecalcTransitionMatrix = transform.CalcInversed(),
						PrecalcInvTransitionMatrix = transform
					};
				}
			}
		}

		void ApplyMagnetsToParticle(Particle p, float delta)
		{
			for (int i = 0; i < numMagnets; i++)
				ApplyMagnetToParticle(p, magnets[i], delta);
		}

		void ApplyMagnetToParticle(Particle p, MagnetData magnetData, float delta)
		{
			ParticlesMagnet magnet = magnetData.Magnet;
			Vector2 targetPosition = p.RegularPosition;
			targetPosition = magnetData.PrecalcTransitionMatrix.TransformVector(targetPosition);
			switch(magnet.Shape) {
			case EmitterShape.Area:
				// Looking for point of magnet's edge, in the direction of particle is moving.
				if (targetPosition.X > 0 && targetPosition.X < magnet.Size.X &&
					targetPosition.X > 0 && targetPosition.Y < magnet.Size.Y) {
					// Particle is got inside magnet, move it outside.
					float d0 = targetPosition.X;
					float d1 = targetPosition.Y;
					float d2 = magnet.Size.X - targetPosition.X;
					float d3 = magnet.Size.Y - targetPosition.Y;
					if (d0 < d1 && d0 < d2 && d0 < d3)
						targetPosition.X = 0;
					else if (d1 < d0 && d1 < d2 && d1 < d3)
						targetPosition.Y = 0;
					else if (d2 < d0 && d2 < d1 && d2 < d3)
						targetPosition.X = magnet.Size.X;
					else
						targetPosition.Y = magnet.Size.Y;
				} else {
					targetPosition.X = Mathf.Clamp(targetPosition.X, 0, magnet.Size.X);
					targetPosition.Y = Mathf.Clamp(targetPosition.Y, 0, magnet.Size.Y);
				}
				break;
			case EmitterShape.Ellipse:
				if (Math.Abs(magnet.Size.Y) > 1e-5) {
					Vector2 center = 0.5f * magnet.Size;
					float k = magnet.Size.X / magnet.Size.Y;
					targetPosition -= center;
					targetPosition.Y *= k;
					targetPosition = targetPosition.Normalized;
					targetPosition *= magnet.Size.X * 0.5f;
					targetPosition.Y /= k;
					targetPosition += center;
				} else
					targetPosition = 0.5f * magnet.Size;
				break;
			case EmitterShape.Line:
				targetPosition.Y = 0.5f * magnet.Size.Y;
				if (targetPosition.X < 0.0f)
					targetPosition.X = 0.0f;
				else if (targetPosition.X > magnet.Size.X)
					targetPosition.X = magnet.Size.X;
				break;
			case EmitterShape.Point:
				targetPosition = 0.5f * magnet.Size;
				break;
			}
			targetPosition = magnetData.PrecalcInvTransitionMatrix.TransformVector(targetPosition);

			Vector2 direction = targetPosition - p.RegularPosition;
			float squaredDistance = direction.SqrLength;
			direction = direction.Normalized;

			float magnetStrength = magnet.Strength;
			if (magnet.Attenuation > 0.0001f) {
				magnetStrength /= (float)Math.Pow(squaredDistance, magnet.Attenuation * 0.5f);
			}

			float t = magnetStrength * p.MagnetAmountCurrent * delta;
			if (t * t > squaredDistance) {
				t = (float)Math.Sqrt(squaredDistance);
			}
			p.RegularPosition += direction * t;
		}
	}
}
