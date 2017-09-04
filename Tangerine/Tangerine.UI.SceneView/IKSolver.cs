using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public static class IKSolver
	{
		public const float DistanceTolerance = 0.001f;

		public static List<Tuple<Bone, float>> SolveFor(Bone bone, Vector2 targetPos)
		{
			var lst = new List<Tuple<Bone, float>>();
			List<Bone> boneChain;
			List<IKPoint> points;
			List<float> distances;
			Prepare(bone, out boneChain, out points, out distances);

			if (Vector2.Distance(points[0].Position, targetPos) - distances.Sum() > DistanceTolerance) {
				for (var i = 0; i < distances.Count; i++) {
					var dst = Vector2.Distance(targetPos, points[i].Position);
					var lambda = distances[i] / dst;
					points[i + 1].Position = (1 - lambda) * points[i].Position + lambda * targetPos;
				}
			} else {
				var b = points[0];
				var dist = Vector2.Distance(points.Last().Position, targetPos);
				while (dist > DistanceTolerance) {
					points[points.Count - 1].Position = targetPos;

					for (var i = distances.Count - 1; i >= 0; i--) {
						var dst = Vector2.Distance(points[i + 1].Position, points[i].Position);
						var lambda = distances[i] / dst;
						points[i].Position = (1 - lambda) * points[i + 1].Position + lambda * points[i].Position;
					}
					points[0] = b;
					for (var i = 0; i < distances.Count; i++) {
						var dst = Vector2.Distance(points[i + 1].Position, points[i].Position);
						var lambda = distances[i] / dst;
						points[i + 1].Position = (1 - lambda) * points[i].Position + lambda * points[i + 1].Position;
					}
					dist = Vector2.Distance(points.Last().Position, targetPos);
				}
			}
			var prevBone = boneChain[0].Parent.AsWidget.BoneArray[boneChain[0].BaseIndex];
			var prev = prevBone.Tip - prevBone.Joint;
			for (var i = 0; i < points.Count - 1; i++) {
				float angle;
				if (boneChain[i].BaseIndex == 0) {
					angle = (points[i + 1].Position - points[i].Position).Atan2Deg;
				} else if (i == 0) {
					var b = boneChain[i].Parent.AsWidget.BoneArray[boneChain[i].BaseIndex];
					angle = Vector2.AngleDeg(b.Tip - b.Joint, points[i + 1].Position - points[i].Position);
				} else {
					angle = Vector2.AngleDeg(prev, points[i + 1].Position - points[i].Position) + points[i].AngleOffset;
				}
				prev = points[i + 1].Position - points[i].Position;
				lst.Add(new Tuple<Bone, float>(boneChain[i], angle - points[i + 1].AngleOffset));
			}
			return lst;
		}

		private static void Prepare(Bone bone, out List<Bone> boneChain, out List<IKPoint> points, out List<float> distances)
		{
			boneChain = new List<Bone>();
			points = new List<IKPoint>();
			distances = new List<float>();

			var entries = bone.Parent.AsWidget.BoneArray;
			points.Add(
				new IKPoint {
					Position = entries[bone.Index].Tip,
				});
			do {
				boneChain.Add(bone);
				distances.Add((entries[bone.Index].Joint - points.Last().Position).Length);
				points.Add(
					new IKPoint {
						Position = entries[bone.Index].Joint,
						AngleOffset = GetAngleOffset(bone),
					});
				if (bone.IKStopper) {
					break;
				}
				bone = bone.Parent.Nodes.GetBone(bone.BaseIndex);
			} while (bone != null);

			boneChain.Reverse();
			points.Reverse();
			distances.Reverse();
		}

		private static float GetAngleOffset(Bone bone)
		{
			if (bone.BaseIndex == 0) {
				return 0f;
			}
			var a = bone.Parent.AsWidget.BoneArray[bone.Index];
			var b = bone.Parent.AsWidget.BoneArray[bone.BaseIndex];
			return Vector2.AngleDeg(b.Tip - b.Joint, a.Joint - b.Joint);
		}

		private class IKPoint
		{
			public Vector2 Position { get; set; }
			public float AngleOffset { get; set; }
		}
	}
}
