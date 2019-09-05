using System;

namespace Lime
{
	public interface IMaterial
	{
		string Id { get; set; }
		int PassCount { get; }
		void Apply(int pass);
		void Invalidate();
	}

	public enum SkinningMode
	{
		Default,
		Linear,
		DualQuaternion
	}

	public interface IMaterialSkin
	{
		bool SkinEnabled { get; set; }
		SkinningMode SkinningMode { get; set; }
		void SetBones(Matrix44[] boneTransforms, int boneCount);
		void SetBones(Vector4[] dualQuaternionPartA, Vector4[] dualQuaternionPartB, int boneCount);
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
