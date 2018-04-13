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

		public string AnimationKey { get; set; }

		public string MarkerId { get; set; }
	}

	public class SceneAnimations
	{
		public List<AnimationData> List = new List<AnimationData>();

		public SceneAnimations(IntPtr scene)
		{
			var animationStacksPointer = FbxSceneGetAnimations(scene);
			if (animationStacksPointer == IntPtr.Zero) return;
			var strct = animationStacksPointer.ToStruct<SizedArray>();
			var animationStacks = strct.GetData<AnimationStack>();
			if (animationStacks.Length == 0)
				return;
			for (var stackIndex = 0; stackIndex < animationStacks.Length; stackIndex++) {
				var layers = animationStacks[stackIndex].Layers.ToStruct<SizedArray>().GetData<AnimationLayer>();
				if (layers.Length == 0)
					continue;
				for (var layerIndex = 0; layerIndex < layers.Length; layerIndex++) {
					var animations = layers[layerIndex].Animations.ToStruct<SizedArray>().GetData<Animation>();
					for (var animationIndex = 0; animationIndex < animations.Length; animationIndex++) {
						var data = new FbxImporter.AnimationData();
						List.Add(data);
						// Set animationStack as MarkerId name
						data.MarkerId = animationStacks[stackIndex].Name;
						data.AnimationKey = animations[animationIndex].Id;
						data.PositionKeys = animations[animationIndex].PositionKeys
							.ToStruct<SizedArray>()
							.GetData<Keyframe>()
							.Select(key => new Keyframe<Vector3>(AnimationUtils.SecondsToFrames(key.Time), key.Data.ToStruct<Vec3>().ToLime()))
							.ToList();

						data.RotationKeys = animations[animationIndex].RotationKeys
							.ToStruct<SizedArray>()
							.GetData<Keyframe>()
							.Select(key => new Keyframe<Quaternion>(AnimationUtils.SecondsToFrames(key.Time), key.Data.ToStruct<Vec4>().ToLimeQuaternion()))
							.ToList();

						data.ScaleKeys = animations[animationIndex].ScaleKeys
							.ToStruct<SizedArray>()
							.GetData<Keyframe>()
							.Select(key => new Keyframe<Vector3>(AnimationUtils.SecondsToFrames(key.Time), key.Data.ToStruct<Vec3>().ToLime()))
							.ToList();
					}
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

			public IntPtr Layers;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = ImportConfig.Charset)]
		private class AnimationLayer
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
