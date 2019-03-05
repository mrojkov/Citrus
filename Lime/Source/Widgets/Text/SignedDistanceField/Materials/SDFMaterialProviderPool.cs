using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lime.SignedDistanceField
{
	internal struct SDFMaterialKey : IEquatable<SDFMaterialKey>
	{
		public float Dilate;
		public float Thickness;
		public Color4 OutlineColor;
		public bool GradientEnabled;
		public ColorGradient Gradient;
		public float GradientAngle;

		public bool Equals(SDFMaterialKey other)
		{
			return Dilate.Equals(other.Dilate) &&
				Thickness.Equals(other.Thickness) &&
				OutlineColor.Equals(other.OutlineColor) &&
				GradientEnabled.Equals(other.GradientEnabled) &&
				Gradient.GetHashCode().Equals(other.Gradient.GetHashCode()) &&
				GradientAngle.Equals(other.GradientAngle);
		}

		public override int GetHashCode()
		{
			unchecked {
				int hash = (int)2166136261;
				hash = (hash * 16777619) ^ Dilate.GetHashCode();
				hash = (hash * 16777619) ^ Thickness.GetHashCode();
				hash = (hash * 16777619) ^ OutlineColor.GetHashCode();
				hash = (hash * 16777619) ^ GradientEnabled.GetHashCode();
				if (GradientEnabled) {
					hash = (hash * 16777619) ^ Gradient.GetHashCode();
					hash = (hash * 16777619) ^ GradientAngle.GetHashCode();
				}
				return hash;
			}
		}
	}

	internal struct SDFShadowMaterialKey : IEquatable<SDFShadowMaterialKey>
	{
		public float Dilate;
		public float Softness;
		public Color4 Color;
		public Vector2 Offset;

		public bool Equals(SDFShadowMaterialKey other)
		{
			return
				Dilate == other.Dilate &&
				Softness == other.Softness &&
				Color == other.Color &&
				Offset == other.Offset;
		}

		public override int GetHashCode()
		{
			unchecked {
				int hash = (int)2166136261;
				hash = (hash * 16777619) ^ Dilate.GetHashCode();
				hash = (hash * 16777619) ^ Softness.GetHashCode();
				hash = (hash * 16777619) ^ Color.GetHashCode();
				hash = (hash * 16777619) ^ Offset.GetHashCode();
				return hash;
			}
		}
	}

	internal struct SDFInnerShadowMaterialKey : IEquatable<SDFInnerShadowMaterialKey>
	{
		public float Dilate;
		public float TextDilate;
		public float Softness;
		public Color4 Color;
		public Vector2 Offset;

		public bool Equals(SDFInnerShadowMaterialKey other)
		{
			return
				Dilate == other.Dilate &&
				TextDilate == other.TextDilate &&
				Softness == other.Softness &&
				Color == other.Color &&
				Offset == other.Offset;
		}

		public override int GetHashCode()
		{
			unchecked {
				int hash = (int)2166136261;
				hash = (hash * 16777619) ^ Dilate.GetHashCode();
				hash = (hash * 16777619) ^ TextDilate.GetHashCode();
				hash = (hash * 16777619) ^ Softness.GetHashCode();
				hash = (hash * 16777619) ^ Color.GetHashCode();
				hash = (hash * 16777619) ^ Offset.GetHashCode();
				return hash;
			}
		}
	}

	internal class SDFMaterialProviderPool
	{
		private readonly Dictionary<SDFMaterialKey, SDFMaterialProvider> mainMaterialsCache = new Dictionary<SDFMaterialKey, SDFMaterialProvider>();
		private readonly Dictionary<SDFShadowMaterialKey, SDFShadowMaterialProvider> shadowMaterialsCache = new Dictionary<SDFShadowMaterialKey, SDFShadowMaterialProvider>();
		private readonly Dictionary<SDFInnerShadowMaterialKey, SDFInnerShadowMaterialProvider> innerShadowMaterialsCache = new Dictionary<SDFInnerShadowMaterialKey, SDFInnerShadowMaterialProvider>();

		public readonly static SDFMaterialProviderPool Instance = new SDFMaterialProviderPool();

		internal SDFMaterialProvider GetProvider(SDFMaterialKey key)
		{
			SDFMaterialProvider result;
			if (mainMaterialsCache.TryGetValue(key, out result)) {
				return result;
			} else {
				result = new SDFMaterialProvider();
				result.Material.Dilate = key.Dilate;
				result.Material.Thickness = key.Thickness;
				result.Material.OutlineColor = key.OutlineColor;
				result.Material.GradientEnabled = key.GradientEnabled;
				result.Material.Gradient = key.Gradient;
				result.Material.GradientAngle = key.GradientAngle;
				mainMaterialsCache.Add(key, result);
				return result;
			}
		}

		internal SDFShadowMaterialProvider GetShadowProvider(SDFShadowMaterialKey key)
		{
			SDFShadowMaterialProvider result;
			if (shadowMaterialsCache.TryGetValue(key, out result)) {
				return result;
			} else {
				result = new SDFShadowMaterialProvider();
				result.Material.Dilate = key.Dilate;
				result.Material.Softness = key.Softness;
				result.Material.Color = key.Color;
				result.Material.Offset = key.Offset;
				shadowMaterialsCache.Add(key, result);
				return result;
			}
		}

		internal SDFInnerShadowMaterialProvider GetInnerShadowProvider(SDFInnerShadowMaterialKey key)
		{
			SDFInnerShadowMaterialProvider result;
			if (innerShadowMaterialsCache.TryGetValue(key, out result)) {
				return result;
			} else {
				result = new SDFInnerShadowMaterialProvider();
				result.Material.Dilate = key.Dilate;
				result.Material.TextDilate = key.TextDilate;
				result.Material.Softness = key.Softness;
				result.Material.Color = key.Color;
				result.Material.Offset = key.Offset;
				innerShadowMaterialsCache.Add(key, result);
				return result;
			}
		}
	}
}
