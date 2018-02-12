using System;

namespace Lime
{
	public interface IMaterial
	{
		int PassCount { get; }
		void Apply(int pass);
		IMaterial Clone();
		void Invalidate();
	}

	public interface IMaterialLightning
	{
		bool ProcessLightning { get; set; }
		void SetLightData(LightSource light);
	}

	public interface IMaterialShadowReciever : IMaterialLightning
	{
		bool RecieveShadows { get; set; }
	}

	public interface IMaterialSkin
	{
		bool SkinEnabled { get; set; }
		void SetBones(Matrix44[] boneTransforms, int boneCount);
	}

	public interface IMaterialFog
	{
		FogMode FogMode { get; set; }
		Color4 FogColor { get; set; }
		float FogStart { get; set; }
		float FogEnd { get; set; }
		float FogDensity { get; set; }
	}

	public enum FogMode
	{
		None,
		Linear,
		Exp,
		ExpSquared
	}
}