using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Orange.FbxImporter
{
	public class AnimationData
	{
		public List<Keyframe<Vector3>> PositionKeys { get; set; }

		public List<Keyframe<Vector3>> ScaleKeys { get; set; }

		public List<Keyframe<Quaternion>> RotationKeys { get; set; }

		public string TargetNodeId { get; set; }

		public string AnimationStackName { get; set; }
	}

	public class FbxSceneAnimations
	{
		public List<AnimationData> List = new List<AnimationData>();

		public FbxSceneAnimations(IntPtr scene)
		{
			var animationStacksPointer = FbxSceneGetAnimations(scene);
			if (animationStacksPointer == IntPtr.Zero) return;
			var strct = animationStacksPointer.ToStruct<SizedArray>();
			var animationStacks = strct.GetData<AnimationStack>();
			if (animationStacks.Length == 0) return;
			foreach (var animationStack in animationStacks) {
				var animations = animationStack.Animations.ToStruct<SizedArray>().GetData<Animation>();
				foreach (var animation in animations) {
					var data = new AnimationData();
					List.Add(data);
					data.AnimationStackName = animationStack.Name;
					data.TargetNodeId = animation.Id;
					data.PositionKeys = animation.PositionKeys
						.ToStruct<SizedArray>()
						.GetData<Keyframe>()
						.Select(key => new Keyframe<Vector3>(AnimationUtils.SecondsToFrames(key.Time), key.Data.ToStruct<Vec3>().ToLime()))
						.ToList();
					data.RotationKeys = animation.RotationKeys
						.ToStruct<SizedArray>()
						.GetData<Keyframe>()
						.Select(key => new Keyframe<Quaternion>(AnimationUtils.SecondsToFrames(key.Time), key.Data.ToStruct<Vec4>().ToLimeQuaternion()))
						.ToList();
					data.ScaleKeys = animation.ScaleKeys
						.ToStruct<SizedArray>()
						.GetData<Keyframe>()
						.Select(key => new Keyframe<Vector3>(AnimationUtils.SecondsToFrames(key.Time), key.Data.ToStruct<Vec3>().ToLime()))
						.ToList();
				}
			}
		}

		#region Pinvokes

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr FbxSceneGetAnimations(IntPtr pScene);

		#endregion

		#region MarshalingStructures

		[StructLayout(LayoutKind.Sequential, CharSet = ImportConfig.Charset)]
		private class AnimationStack
		{
			public string Name;

			public IntPtr Animations;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = ImportConfig.Charset)]
		private class Animation
		{
			public string Id;

			public IntPtr PositionKeys;

			public IntPtr ScaleKeys;

			public IntPtr RotationKeys;
		}

		[StructLayout(LayoutKind.Sequential)]
		private class Keyframe
		{
			public double Time;

			public IntPtr Data;
		}

		#endregion
	}
}
