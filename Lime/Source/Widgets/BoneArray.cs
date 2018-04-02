using System;
using Yuzu;

namespace Lime
{
	public struct BoneArray
	{
		public struct Entry
		{
			[YuzuMember]
			public float Rotation;

			[YuzuMember]
			public float Length;

			[YuzuMember]
			public Vector2 Joint;

			[YuzuMember]
			public Vector2 Tip;

			/// <summary>
			/// Bone's transformation relative its reference position.
			/// </summary>
			[YuzuMember]
			public Matrix32 RelativeTransform;
		}

		const int InitialSize = 10;

		void EnsureArraySize(int size)
		{
			if (items == null)
				items = new Entry[InitialSize];
			if (size > items.Length)
				Array.Resize<Entry>(ref items, size);
		}

		public Entry this[int index] {
			get {
				EnsureArraySize(index + 1);
				return items[index];
			}
			set {
				EnsureArraySize(index + 1);
				items[index] = value;
			}
		}

		void ApplyBone(BoneWeight bw, Vector2 sourceVector, ref Vector2 resultVector, ref float overallWeight)
		{
			if (items != null && bw.Index > 0 && bw.Index < items.Length) {
				BoneArray.Entry e = items[bw.Index];
				resultVector += bw.Weight * e.RelativeTransform.TransformVector(sourceVector);
				overallWeight += bw.Weight;
			}
		}

		public Vector2 ApplySkinningToVector(Vector2 vector, SkinningWeights weights)
		{
			Vector2 result = Vector2.Zero;
			float overallWeight = 0;
			ApplyBone(weights.Bone0, vector, ref result, ref overallWeight);
			ApplyBone(weights.Bone1, vector, ref result, ref overallWeight);
			ApplyBone(weights.Bone2, vector, ref result, ref overallWeight);
			ApplyBone(weights.Bone3, vector, ref result, ref overallWeight);
			if (overallWeight < 0)
				result = vector;
			else if (overallWeight >= 0 && overallWeight < 1)
				result += (1 - overallWeight) * vector;
			else {
				result.X /= overallWeight;
				result.Y /= overallWeight;
			}
			return result;
		}

		public Matrix32 CalcWeightedRelativeTransform(SkinningWeights weights)
		{
			Matrix32 skinningMatrix = new Matrix32();

			skinningMatrix.T = ApplySkinningToVector(Vector2.Zero, weights);
			skinningMatrix.U = ApplySkinningToVector(Vector2.Right, weights) - skinningMatrix.T;
			skinningMatrix.V = ApplySkinningToVector(Vector2.Down, weights) - skinningMatrix.T;
				
			return skinningMatrix;
		}

		[YuzuMember]
		public Entry[] items;
	}
}
