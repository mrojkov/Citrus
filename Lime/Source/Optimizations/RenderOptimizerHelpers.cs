using System;
using System.Linq;

namespace Lime.RenderOptimizer
{
	public class ImageCombinerItem
	{
		public ImageCombiner Combiner;
		public IImageCombinerArg Arg1;
		public IImageCombinerArg Arg2;

		private ImageCombinerItem(ImageCombiner combiner, IImageCombinerArg arg1, IImageCombinerArg arg2)
		{
			Combiner = combiner;
			Arg1 = arg1;
			Arg2 = arg2;
		}

		public static ImageCombinerItem TryCreate(ImageCombiner combiner)
		{
			if (combiner.Parent == null) {
				return null;
			}
			var selfIndex = combiner.Parent.Nodes.IndexOf(combiner);
			if (selfIndex >= combiner.Parent.Nodes.Count - 2) {
				return null;
			}
			var arg1 = combiner.Parent.Nodes[selfIndex + 1] as IImageCombinerArg;
			var arg2 = combiner.Parent.Nodes[selfIndex + 2] as IImageCombinerArg;
			if (arg1 == null || arg2 == null) {
				return null;
			}

			return new ImageCombinerItem(combiner, arg1, arg2);
		}
	}

	public static class DistortionMeshExtensions
	{
		public static Rectangle GetLocalAABB(this DistortionMesh mesh)
		{
			var parent = mesh.ParentWidget;

			var existsAnimation = false;
			var animationStart = int.MaxValue;
			var animationEnd = 0;
			foreach (var bone in parent.Nodes.OfType<Bone>()) {
				foreach (var animator in bone.Animators) {
					existsAnimation = true;
					animationStart = Math.Min(animationStart, animator.Keys.First().Frame);
					animationEnd = Math.Max(animationEnd, animator.Duration);
				}
			}
			if (!existsAnimation) {
				animationStart = animationEnd = parent.DefaultAnimation.Frame;
			}

			var existsPointAnimation = false;
			var pointsAnimationStart = int.MaxValue;
			var pointsAnimationEnd = 0;
			foreach (var point in mesh.Nodes.OfType<DistortionMeshPoint>()) {
				foreach (var animator in point.Animators) {
					existsPointAnimation = true;
					pointsAnimationStart = Math.Min(pointsAnimationStart, animator.Keys.First().Frame);
					pointsAnimationEnd = Math.Max(pointsAnimationEnd, animator.Duration);
				}
			}
			if (!existsPointAnimation) {
				pointsAnimationStart = pointsAnimationEnd = mesh.DefaultAnimation.Frame;
			}

			var rectangle = new Rectangle(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
			var animationFrame = parent.DefaultAnimation.Frame;
			var pointsAnimationFrame = mesh.DefaultAnimation.Frame;
			for (var frame = animationStart; frame <= animationEnd; frame++) {
				parent.DefaultAnimation.Frame = frame;

				var bones = parent.BoneArray;
				var weightsMatrix = mesh.CalcLocalToParentTransform();
				var weightsMatrixInversed = weightsMatrix.CalcInversed();

				for (var pointsFrame = pointsAnimationStart; pointsFrame <= pointsAnimationEnd; pointsFrame++) {
					mesh.DefaultAnimation.Frame = frame;

					for (var i = 0; i < mesh.Nodes.Count; i++) {
						var point = (DistortionMeshPoint)mesh.Nodes[i];
						rectangle = ExpandMeshLocalAABB(rectangle, mesh.Size, point, bones, weightsMatrix, weightsMatrixInversed);
					}
				}
			}
			parent.DefaultAnimation.Frame = animationFrame;
			mesh.DefaultAnimation.Frame = pointsAnimationFrame;

			return rectangle;
		}

		private static Rectangle ExpandMeshLocalAABB(
			Rectangle aabb,
			Vector2 size,
			DistortionMeshPoint point,
			BoneArray bones,
			Matrix32 weightsMatrix,
			Matrix32 weightsMatrixInversed
		)
		{
			var position = size * point.Position + point.Offset;
			if (point.SkinningWeights != null) {
				position = weightsMatrixInversed.TransformVector(bones.ApplySkinningToVector(
					weightsMatrix.TransformVector(position),
					point.SkinningWeights
				));
			}
			return aabb.IncludingPoint(position);
		}
	}
}
