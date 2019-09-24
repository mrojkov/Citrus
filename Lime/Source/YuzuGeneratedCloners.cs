using System;

using Yuzu.Clone;

namespace YuzuGenerated
{
	public class LimeCloner: ClonerGenBase
	{
		private static global::Lime.Alignment Clone_Lime__Alignment(Cloner cl, object src) =>
			(global::Lime.Alignment)src;

		protected static global::Lime.AlphaIntensityComponent Clone_Lime__AlphaIntensityComponent(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.AlphaIntensityComponent))
				return (global::Lime.AlphaIntensityComponent)cl.DeepObject(src);
			var s = (global::Lime.AlphaIntensityComponent)src;
			var result = new global::Lime.AlphaIntensityComponent();
			result.Brightness = s.Brightness;
			result.Color = s.Color;
			result.MaskTexture = (global::Lime.ITexture)cl.DeepObject(s.MaskTexture);
			result.Radius = s.Radius;
			return result;
		}

		protected static global::Lime.AlphaIntensityMaterial Clone_Lime__AlphaIntensityMaterial(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.AlphaIntensityMaterial))
				return (global::Lime.AlphaIntensityMaterial)cl.DeepObject(src);
			var s = (global::Lime.AlphaIntensityMaterial)src;
			var result = new global::Lime.AlphaIntensityMaterial();
			result.Brightness = s.Brightness;
			result.Color = s.Color;
			result.MaskTexture = (global::Lime.ITexture)cl.DeepObject(s.MaskTexture);
			result.Radius = s.Radius;
			return result;
		}

		protected static global::Lime.Animation Clone_Lime__Animation(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animation))
				return (global::Lime.Animation)cl.DeepObject(src);
			var s = (global::Lime.Animation)src;
			var result = new global::Lime.Animation();
			result.ApplyZeroPose = s.ApplyZeroPose;
			result.ContentsPath = s.ContentsPath;
			result.Id = s.Id;
			result.IsCompound = s.IsCompound;
			result.IsLegacy = s.IsLegacy;
			if (s.Markers != null && result.Markers != null) {
				foreach (var tmp1 in s.Markers)
					result.Markers.Add(Clone_Lime__Marker(cl, tmp1));
			}
			if (s.Tracks != null && result.Tracks != null) {
				foreach (var tmp2 in s.Tracks)
					result.Tracks.Add(Clone_Lime__AnimationTrack(cl, tmp2));
			}
			return result;
		}

		protected static global::Lime.AnimationBlender Clone_Lime__AnimationBlender(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.AnimationBlender))
				return (global::Lime.AnimationBlender)cl.DeepObject(src);
			var s = (global::Lime.AnimationBlender)src;
			var result = new global::Lime.AnimationBlender();
			if (s.Options != null) {
				foreach (var tmp1 in s.Options)
					result.Options.Add(tmp1.Key, Clone_Lime__AnimationBlending(cl, tmp1.Value));
			}
			return result;
		}

		protected static global::Lime.AnimationBlending Clone_Lime__AnimationBlending(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.AnimationBlending))
				return (global::Lime.AnimationBlending)cl.DeepObject(src);
			var s = (global::Lime.AnimationBlending)src;
			var result = new global::Lime.AnimationBlending();
			if (s.MarkersOptions != null) {
				result.MarkersOptions = new global::System.Collections.Generic.Dictionary<string, global::Lime.MarkerBlending>();
				foreach (var tmp1 in s.MarkersOptions)
					result.MarkersOptions.Add(tmp1.Key, Clone_Lime__MarkerBlending(cl, tmp1.Value));
			}
			result.Option = Clone_Lime__BlendingOption(cl, s.Option);
			return result;
		}

		protected static global::Lime.AnimationClip Clone_Lime__AnimationClip(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.AnimationClip))
				return (global::Lime.AnimationClip)cl.DeepObject(src);
			var s = (global::Lime.AnimationClip)src;
			var result = new global::Lime.AnimationClip();
			result.AnimationId = s.AnimationId;
			result.BeginFrame = s.BeginFrame;
			result.EndFrame = s.EndFrame;
			result.InFrame = s.InFrame;
			result.PostExtrapolation = s.PostExtrapolation;
			result.Reversed = s.Reversed;
			result.Speed = s.Speed;
			return result;
		}

		protected static global::Lime.Animation.AnimationData Clone_Lime__Animation__AnimationData(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animation.AnimationData))
				return (global::Lime.Animation.AnimationData)cl.DeepObject(src);
			var s = (global::Lime.Animation.AnimationData)src;
			var result = new global::Lime.Animation.AnimationData();
			if (s.Animators != null && result.Animators != null) {
				var tmp2 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp1 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp2(tmp1));
			}
			return result;
		}

		protected static global::Lime.AnimationTrack Clone_Lime__AnimationTrack(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.AnimationTrack))
				return (global::Lime.AnimationTrack)cl.DeepObject(src);
			var s = (global::Lime.AnimationTrack)src;
			var result = new global::Lime.AnimationTrack();
			if (s.Animators != null && result.Animators != null) {
				var tmp2 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp1 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp2(tmp1));
			}
			if (s.Clips != null && result.Clips != null) {
				foreach (var tmp3 in s.Clips)
					result.Clips.Add(Clone_Lime__AnimationClip(cl, tmp3));
			}
			result.Id = s.Id;
			result.TangerineFlags = s.TangerineFlags;
			result.Weight = s.Weight;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.Alignment> Clone_Lime__Animator_Alignment(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.Alignment>))
				return (global::Lime.Animator<global::Lime.Alignment>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.Alignment>)src;
			var result = new global::Lime.Animator<global::Lime.Alignment>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.Anchors> Clone_Lime__Animator_Anchors(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.Anchors>))
				return (global::Lime.Animator<global::Lime.Anchors>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.Anchors>)src;
			var result = new global::Lime.Animator<global::Lime.Anchors>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.AudioAction> Clone_Lime__Animator_AudioAction(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.AudioAction>))
				return (global::Lime.Animator<global::Lime.AudioAction>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.AudioAction>)src;
			var result = new global::Lime.Animator<global::Lime.AudioAction>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.Blending> Clone_Lime__Animator_Blending(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.Blending>))
				return (global::Lime.Animator<global::Lime.Blending>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.Blending>)src;
			var result = new global::Lime.Animator<global::Lime.Blending>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.ClipMethod> Clone_Lime__Animator_ClipMethod(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.ClipMethod>))
				return (global::Lime.Animator<global::Lime.ClipMethod>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.ClipMethod>)src;
			var result = new global::Lime.Animator<global::Lime.ClipMethod>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.Color4> Clone_Lime__Animator_Color4(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.Color4>))
				return (global::Lime.Animator<global::Lime.Color4>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.Color4>)src;
			var result = new global::Lime.Animator<global::Lime.Color4>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.EmissionType> Clone_Lime__Animator_EmissionType(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.EmissionType>))
				return (global::Lime.Animator<global::Lime.EmissionType>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.EmissionType>)src;
			var result = new global::Lime.Animator<global::Lime.EmissionType>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.EmitterShape> Clone_Lime__Animator_EmitterShape(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.EmitterShape>))
				return (global::Lime.Animator<global::Lime.EmitterShape>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.EmitterShape>)src;
			var result = new global::Lime.Animator<global::Lime.EmitterShape>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.HAlignment> Clone_Lime__Animator_HAlignment(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.HAlignment>))
				return (global::Lime.Animator<global::Lime.HAlignment>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.HAlignment>)src;
			var result = new global::Lime.Animator<global::Lime.HAlignment>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.ITexture> Clone_Lime__Animator_ITexture(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.ITexture>))
				return (global::Lime.Animator<global::Lime.ITexture>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.ITexture>)src;
			var result = new global::Lime.Animator<global::Lime.ITexture>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.LayoutDirection> Clone_Lime__Animator_LayoutDirection(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.LayoutDirection>))
				return (global::Lime.Animator<global::Lime.LayoutDirection>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.LayoutDirection>)src;
			var result = new global::Lime.Animator<global::Lime.LayoutDirection>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.Matrix44> Clone_Lime__Animator_Matrix44(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.Matrix44>))
				return (global::Lime.Animator<global::Lime.Matrix44>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.Matrix44>)src;
			var result = new global::Lime.Animator<global::Lime.Matrix44>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.MovieAction> Clone_Lime__Animator_MovieAction(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.MovieAction>))
				return (global::Lime.Animator<global::Lime.MovieAction>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.MovieAction>)src;
			var result = new global::Lime.Animator<global::Lime.MovieAction>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.NodeReference<global::Lime.Camera3D>> Clone_Lime__Animator_NodeReference_Camera3D(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.NodeReference<global::Lime.Camera3D>>))
				return (global::Lime.Animator<global::Lime.NodeReference<global::Lime.Camera3D>>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.NodeReference<global::Lime.Camera3D>>)src;
			var result = new global::Lime.Animator<global::Lime.NodeReference<global::Lime.Camera3D>>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.NodeReference<global::Lime.Node3D>> Clone_Lime__Animator_NodeReference_Node3D(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.NodeReference<global::Lime.Node3D>>))
				return (global::Lime.Animator<global::Lime.NodeReference<global::Lime.Node3D>>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.NodeReference<global::Lime.Node3D>>)src;
			var result = new global::Lime.Animator<global::Lime.NodeReference<global::Lime.Node3D>>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.NodeReference<global::Lime.Spline>> Clone_Lime__Animator_NodeReference_Spline(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.NodeReference<global::Lime.Spline>>))
				return (global::Lime.Animator<global::Lime.NodeReference<global::Lime.Spline>>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.NodeReference<global::Lime.Spline>>)src;
			var result = new global::Lime.Animator<global::Lime.NodeReference<global::Lime.Spline>>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.NodeReference<global::Lime.Spline3D>> Clone_Lime__Animator_NodeReference_Spline3D(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.NodeReference<global::Lime.Spline3D>>))
				return (global::Lime.Animator<global::Lime.NodeReference<global::Lime.Spline3D>>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.NodeReference<global::Lime.Spline3D>>)src;
			var result = new global::Lime.Animator<global::Lime.NodeReference<global::Lime.Spline3D>>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.NodeReference<global::Lime.Widget>> Clone_Lime__Animator_NodeReference_Widget(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.NodeReference<global::Lime.Widget>>))
				return (global::Lime.Animator<global::Lime.NodeReference<global::Lime.Widget>>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.NodeReference<global::Lime.Widget>>)src;
			var result = new global::Lime.Animator<global::Lime.NodeReference<global::Lime.Widget>>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.NumericRange> Clone_Lime__Animator_NumericRange(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.NumericRange>))
				return (global::Lime.Animator<global::Lime.NumericRange>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.NumericRange>)src;
			var result = new global::Lime.Animator<global::Lime.NumericRange>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.ParticlesLinkage> Clone_Lime__Animator_ParticlesLinkage(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.ParticlesLinkage>))
				return (global::Lime.Animator<global::Lime.ParticlesLinkage>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.ParticlesLinkage>)src;
			var result = new global::Lime.Animator<global::Lime.ParticlesLinkage>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.Quaternion> Clone_Lime__Animator_Quaternion(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.Quaternion>))
				return (global::Lime.Animator<global::Lime.Quaternion>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.Quaternion>)src;
			var result = new global::Lime.Animator<global::Lime.Quaternion>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.RenderTarget> Clone_Lime__Animator_RenderTarget(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.RenderTarget>))
				return (global::Lime.Animator<global::Lime.RenderTarget>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.RenderTarget>)src;
			var result = new global::Lime.Animator<global::Lime.RenderTarget>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.SerializableFont> Clone_Lime__Animator_SerializableFont(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.SerializableFont>))
				return (global::Lime.Animator<global::Lime.SerializableFont>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.SerializableFont>)src;
			var result = new global::Lime.Animator<global::Lime.SerializableFont>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.SerializableSample> Clone_Lime__Animator_SerializableSample(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.SerializableSample>))
				return (global::Lime.Animator<global::Lime.SerializableSample>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.SerializableSample>)src;
			var result = new global::Lime.Animator<global::Lime.SerializableSample>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.ShaderId> Clone_Lime__Animator_ShaderId(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.ShaderId>))
				return (global::Lime.Animator<global::Lime.ShaderId>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.ShaderId>)src;
			var result = new global::Lime.Animator<global::Lime.ShaderId>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.TextOverflowMode> Clone_Lime__Animator_TextOverflowMode(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.TextOverflowMode>))
				return (global::Lime.Animator<global::Lime.TextOverflowMode>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.TextOverflowMode>)src;
			var result = new global::Lime.Animator<global::Lime.TextOverflowMode>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.Thickness> Clone_Lime__Animator_Thickness(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.Thickness>))
				return (global::Lime.Animator<global::Lime.Thickness>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.Thickness>)src;
			var result = new global::Lime.Animator<global::Lime.Thickness>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.VAlignment> Clone_Lime__Animator_VAlignment(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.VAlignment>))
				return (global::Lime.Animator<global::Lime.VAlignment>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.VAlignment>)src;
			var result = new global::Lime.Animator<global::Lime.VAlignment>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.Vector2> Clone_Lime__Animator_Vector2(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.Vector2>))
				return (global::Lime.Animator<global::Lime.Vector2>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.Vector2>)src;
			var result = new global::Lime.Animator<global::Lime.Vector2>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<global::Lime.Vector3> Clone_Lime__Animator_Vector3(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<global::Lime.Vector3>))
				return (global::Lime.Animator<global::Lime.Vector3>)cl.DeepObject(src);
			var s = (global::Lime.Animator<global::Lime.Vector3>)src;
			var result = new global::Lime.Animator<global::Lime.Vector3>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<bool> Clone_Lime__Animator_Boolean(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<bool>))
				return (global::Lime.Animator<bool>)cl.DeepObject(src);
			var s = (global::Lime.Animator<bool>)src;
			var result = new global::Lime.Animator<bool>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<int> Clone_Lime__Animator_Int32(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<int>))
				return (global::Lime.Animator<int>)cl.DeepObject(src);
			var s = (global::Lime.Animator<int>)src;
			var result = new global::Lime.Animator<int>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<float> Clone_Lime__Animator_Single(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<float>))
				return (global::Lime.Animator<float>)cl.DeepObject(src);
			var s = (global::Lime.Animator<float>)src;
			var result = new global::Lime.Animator<float>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Animator<string> Clone_Lime__Animator_String(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Animator<string>))
				return (global::Lime.Animator<string>)cl.DeepObject(src);
			var s = (global::Lime.Animator<string>)src;
			var result = new global::Lime.Animator<string>();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Node.AssetBundlePathComponent Clone_Lime__Node__AssetBundlePathComponent(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Node.AssetBundlePathComponent))
				return (global::Lime.Node.AssetBundlePathComponent)cl.DeepObject(src);
			var s = (global::Lime.Node.AssetBundlePathComponent)src;
			var result = new global::Lime.Node.AssetBundlePathComponent();
			result.Path = s.Path;
			return result;
		}

		protected static global::Lime.Audio Clone_Lime__Audio(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Audio))
				return (global::Lime.Audio)cl.DeepObject(src);
			var s = (global::Lime.Audio)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.Audio();
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			result.Continuous = s.Continuous;
			result.FadeTime = s.FadeTime;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.Group = s.Group;
			result.Id = s.Id;
			result.Looping = s.Looping;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Pan = s.Pan;
			result.Pitch = s.Pitch;
			result.Priority = s.Priority;
			result.Sample = Clone_Lime__SerializableSample(cl, s.Sample);
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.Volume = s.Volume;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.AudioRandomizerComponent Clone_Lime__AudioRandomizerComponent(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.AudioRandomizerComponent))
				return (global::Lime.AudioRandomizerComponent)cl.DeepObject(src);
			var s = (global::Lime.AudioRandomizerComponent)src;
			var result = new global::Lime.AudioRandomizerComponent();
			result.Pitch = s.Pitch;
			if (s.Samples != null && result.Samples != null) {
				foreach (var tmp1 in s.Samples)
					result.Samples.Add(Clone_Lime__SerializableSample(cl, tmp1));
			}
			result.Volume = s.Volume;
			return result;
		}

		private static global::Lime.BezierEasing Clone_Lime__BezierEasing(Cloner cl, object src) =>
			(global::Lime.BezierEasing)src;

		private static global::Lime.BitSet32 Clone_Lime__BitSet32(Cloner cl, object src) =>
			(global::Lime.BitSet32)src;

		private static global::Lime.Mesh3D.BlendIndices Clone_Lime__Mesh3D__BlendIndices(Cloner cl, object src) =>
			(global::Lime.Mesh3D.BlendIndices)src;

		protected static global::Lime.BlendingOption Clone_Lime__BlendingOption(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.BlendingOption))
				return (global::Lime.BlendingOption)cl.DeepObject(src);
			var s = (global::Lime.BlendingOption)src;
			var result = new global::Lime.BlendingOption();
			result.Duration = s.Duration;
			return result;
		}

		private static global::Lime.Mesh3D.BlendWeights Clone_Lime__Mesh3D__BlendWeights(Cloner cl, object src) =>
			(global::Lime.Mesh3D.BlendWeights)src;

		protected static global::Lime.BloomMaterial Clone_Lime__BloomMaterial(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.BloomMaterial))
				return (global::Lime.BloomMaterial)cl.DeepObject(src);
			var s = (global::Lime.BloomMaterial)src;
			var result = new global::Lime.BloomMaterial();
			result.BrightThreshold = s.BrightThreshold;
			result.InversedGammaCorrection = s.InversedGammaCorrection;
			return result;
		}

		protected static global::Lime.BlurMaterial Clone_Lime__BlurMaterial(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.BlurMaterial))
				return (global::Lime.BlurMaterial)cl.DeepObject(src);
			var s = (global::Lime.BlurMaterial)src;
			var result = new global::Lime.BlurMaterial();
			result.AlphaCorrection = s.AlphaCorrection;
			result.Blending = s.Blending;
			result.BlurShaderId = s.BlurShaderId;
			result.Dir = s.Dir;
			result.Opaque = s.Opaque;
			result.Radius = s.Radius;
			result.Step = s.Step;
			return result;
		}

		protected static global::Lime.Bone Clone_Lime__Bone(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Bone))
				return (global::Lime.Bone)cl.DeepObject(src);
			var s = (global::Lime.Bone)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.Bone();
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.BaseIndex = s.BaseIndex;
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			result.EffectiveRadius = s.EffectiveRadius;
			result.FadeoutZone = s.FadeoutZone;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.IKStopper = s.IKStopper;
			result.Id = s.Id;
			result.Index = s.Index;
			result.Length = s.Length;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Position = s.Position;
			result.RefLength = s.RefLength;
			result.RefPosition = s.RefPosition;
			result.RefRotation = s.RefRotation;
			result.Rotation = s.Rotation;
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.BoneArray Clone_Lime__BoneArray(Cloner cl, object src)
		{
			var s = (global::Lime.BoneArray)src;
			var result = new global::Lime.BoneArray();
			if (s.items != null) {
				result.items = new global::Lime.BoneArray.Entry[s.items.Length];
				Array.Copy(s.items, result.items, s.items.Length);
			}
			return result;
		}

		private static object Clone_Lime__BoneArray_obj(Cloner cl, object src) =>
			Clone_Lime__BoneArray(cl, src);

		private static global::Lime.BoneWeight Clone_Lime__BoneWeight(Cloner cl, object src) =>
			(global::Lime.BoneWeight)src;

		private static global::Lime.BoundingSphere Clone_Lime__BoundingSphere(Cloner cl, object src) =>
			(global::Lime.BoundingSphere)src;

		private static global::Lime.Bounds Clone_Lime__Bounds(Cloner cl, object src) =>
			(global::Lime.Bounds)src;

		protected static global::Lime.Button Clone_Lime__Button(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Button))
				return (global::Lime.Button)cl.DeepObject(src);
			var s = (global::Lime.Button)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.Button();
			result.Anchors = s.Anchors;
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Blending = s.Blending;
			if (s.IsNotDecorated()) {
				result.Color = s.Color;
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			result.Enabled = s.Enabled;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.HitTestMethod = s.HitTestMethod;
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Padding = s.Padding;
			result.Pivot = s.Pivot;
			result.Position = s.Position;
			result.Rotation = s.Rotation;
			result.Scale = s.Scale;
			result.Shader = s.Shader;
			result.SilentSize = s.SilentSize;
			result.SkinningWeights = Clone_Lime__SkinningWeights(cl, s.SkinningWeights);
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.Text = s.Text;
			result.Visible = s.Visible;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.Camera3D Clone_Lime__Camera3D(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Camera3D))
				return (global::Lime.Camera3D)cl.DeepObject(src);
			var s = (global::Lime.Camera3D)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.Camera3D();
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.AspectRatio = s.AspectRatio;
			result.Color = s.Color;
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			result.FarClipPlane = s.FarClipPlane;
			result.FieldOfView = s.FieldOfView;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.Id = s.Id;
			result.NearClipPlane = s.NearClipPlane;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Opaque = s.Opaque;
			result.OrthographicSize = s.OrthographicSize;
			result.Position = s.Position;
			result.ProjectionMode = s.ProjectionMode;
			result.Rotation = s.Rotation;
			result.Scale = s.Scale;
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.Visible = s.Visible;
			s.OnAfterSerialization();
			return result;
		}

		private static global::Lime.Color4 Clone_Lime__Color4(Cloner cl, object src) =>
			(global::Lime.Color4)src;

		protected static global::Lime.Color4Animator Clone_Lime__Color4Animator(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Color4Animator))
				return (global::Lime.Color4Animator)cl.DeepObject(src);
			var s = (global::Lime.Color4Animator)src;
			var result = new global::Lime.Color4Animator();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.ColorCorrectionMaterial Clone_Lime__ColorCorrectionMaterial(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.ColorCorrectionMaterial))
				return (global::Lime.ColorCorrectionMaterial)cl.DeepObject(src);
			var s = (global::Lime.ColorCorrectionMaterial)src;
			var result = new global::Lime.ColorCorrectionMaterial();
			result.Blending = s.Blending;
			result.Brightness = s.Brightness;
			result.Contrast = s.Contrast;
			result.HSL = s.HSL;
			result.Opaque = s.Opaque;
			return result;
		}

		protected static global::Lime.CommonMaterial Clone_Lime__CommonMaterial(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.CommonMaterial))
				return (global::Lime.CommonMaterial)cl.DeepObject(src);
			var s = (global::Lime.CommonMaterial)src;
			var result = new global::Lime.CommonMaterial();
			result.Blending = s.Blending;
			result.DiffuseColor = s.DiffuseColor;
			result.DiffuseTexture = (global::Lime.ITexture)cl.DeepObject(s.DiffuseTexture);
			result.FogColor = s.FogColor;
			result.FogDensity = s.FogDensity;
			result.FogEnd = s.FogEnd;
			result.FogMode = s.FogMode;
			result.FogStart = s.FogStart;
			result.Id = s.Id;
			result.SkinEnabled = s.SkinEnabled;
			result.SkinningMode = s.SkinningMode;
			return result;
		}

		protected static global::Lime.RenderOptimizer.ContentBox Clone_Lime_RenderOptimizer__ContentBox(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.RenderOptimizer.ContentBox))
				return (global::Lime.RenderOptimizer.ContentBox)cl.DeepObject(src);
			var s = (global::Lime.RenderOptimizer.ContentBox)src;
			var result = new global::Lime.RenderOptimizer.ContentBox();
			result.Data = s.Data;
			return result;
		}

		protected static global::Lime.RenderOptimizer.ContentPlane Clone_Lime_RenderOptimizer__ContentPlane(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.RenderOptimizer.ContentPlane))
				return (global::Lime.RenderOptimizer.ContentPlane)cl.DeepObject(src);
			var s = (global::Lime.RenderOptimizer.ContentPlane)src;
			var result = new global::Lime.RenderOptimizer.ContentPlane();
			if (s.Data != null) {
				result.Data = new global::Lime.Vector3[s.Data.Length];
				Array.Copy(s.Data, result.Data, s.Data.Length);
			}
			return result;
		}

		protected static global::Lime.RenderOptimizer.ContentRectangle Clone_Lime_RenderOptimizer__ContentRectangle(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.RenderOptimizer.ContentRectangle))
				return (global::Lime.RenderOptimizer.ContentRectangle)cl.DeepObject(src);
			var s = (global::Lime.RenderOptimizer.ContentRectangle)src;
			var result = new global::Lime.RenderOptimizer.ContentRectangle();
			result.Data = s.Data;
			return result;
		}

		protected static global::Lime.RenderOptimizer.ContentSizeComponent Clone_Lime_RenderOptimizer__ContentSizeComponent(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.RenderOptimizer.ContentSizeComponent))
				return (global::Lime.RenderOptimizer.ContentSizeComponent)cl.DeepObject(src);
			var s = (global::Lime.RenderOptimizer.ContentSizeComponent)src;
			var result = new global::Lime.RenderOptimizer.ContentSizeComponent();
			result.Size = (global::Lime.RenderOptimizer.ContentSize)cl.DeepObject(s.Size);
			return result;
		}

		protected static global::Lime.DefaultLayoutCell Clone_Lime__DefaultLayoutCell(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.DefaultLayoutCell))
				return (global::Lime.DefaultLayoutCell)cl.DeepObject(src);
			var s = (global::Lime.DefaultLayoutCell)src;
			var result = new global::Lime.DefaultLayoutCell();
			result.Alignment = s.Alignment;
			result.ColumnSpan = s.ColumnSpan;
			result.Ignore = s.Ignore;
			result.RowSpan = s.RowSpan;
			result.Stretch = s.Stretch;
			return result;
		}

		protected static global::Lime.DissolveComponent Clone_Lime__DissolveComponent(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.DissolveComponent))
				return (global::Lime.DissolveComponent)cl.DeepObject(src);
			var s = (global::Lime.DissolveComponent)src;
			var result = new global::Lime.DissolveComponent();
			result.Brightness = s.Brightness;
			result.Color = s.Color;
			result.MaskTexture = (global::Lime.ITexture)cl.DeepObject(s.MaskTexture);
			result.Range = s.Range;
			return result;
		}

		protected static global::Lime.DissolveMaterial Clone_Lime__DissolveMaterial(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.DissolveMaterial))
				return (global::Lime.DissolveMaterial)cl.DeepObject(src);
			var s = (global::Lime.DissolveMaterial)src;
			var result = new global::Lime.DissolveMaterial();
			result.Brightness = s.Brightness;
			result.Color = s.Color;
			result.MaskTexture = (global::Lime.ITexture)cl.DeepObject(s.MaskTexture);
			result.Range = s.Range;
			return result;
		}

		protected static global::Lime.DistortionMaterial Clone_Lime__DistortionMaterial(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.DistortionMaterial))
				return (global::Lime.DistortionMaterial)cl.DeepObject(src);
			var s = (global::Lime.DistortionMaterial)src;
			var result = new global::Lime.DistortionMaterial();
			result.BarrelPincushion = s.BarrelPincushion;
			result.Blending = s.Blending;
			result.Blue = s.Blue;
			result.ChromaticAberration = s.ChromaticAberration;
			result.Green = s.Green;
			result.Opaque = s.Opaque;
			result.Red = s.Red;
			return result;
		}

		protected static global::Lime.DistortionMesh Clone_Lime__DistortionMesh(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.DistortionMesh))
				return (global::Lime.DistortionMesh)cl.DeepObject(src);
			var s = (global::Lime.DistortionMesh)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.DistortionMesh();
			result.Anchors = s.Anchors;
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Blending = s.Blending;
			if (s.IsNotDecorated()) {
				result.Color = s.Color;
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.HitTestMethod = s.HitTestMethod;
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.NumCols = s.NumCols;
			result.NumRows = s.NumRows;
			result.Padding = s.Padding;
			result.Pivot = s.Pivot;
			result.Position = s.Position;
			result.Rotation = s.Rotation;
			result.Scale = s.Scale;
			result.Shader = s.Shader;
			result.SilentSize = s.SilentSize;
			result.SkinningWeights = Clone_Lime__SkinningWeights(cl, s.SkinningWeights);
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.Texture = (global::Lime.ITexture)cl.DeepObject(s.Texture);
			result.Visible = s.Visible;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.DistortionMeshPoint Clone_Lime__DistortionMeshPoint(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.DistortionMeshPoint))
				return (global::Lime.DistortionMeshPoint)cl.DeepObject(src);
			var s = (global::Lime.DistortionMeshPoint)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.DistortionMeshPoint();
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Color = s.Color;
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Offset = s.Offset;
			result.Position = s.Position;
			result.SkinningWeights = Clone_Lime__SkinningWeights(cl, s.SkinningWeights);
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.UV = s.UV;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.EmitterShapePoint Clone_Lime__EmitterShapePoint(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.EmitterShapePoint))
				return (global::Lime.EmitterShapePoint)cl.DeepObject(src);
			var s = (global::Lime.EmitterShapePoint)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.EmitterShapePoint();
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Position = s.Position;
			result.SkinningWeights = Clone_Lime__SkinningWeights(cl, s.SkinningWeights);
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.Font Clone_Lime__Font(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Font))
				return (global::Lime.Font)cl.DeepObject(src);
			var s = (global::Lime.Font)src;
			var result = new global::Lime.Font();
			result.About = s.About;
			if (s.CharCollection != null && result.CharCollection != null) {
				foreach (var tmp1 in s.CharCollection)
					((global::System.Collections.Generic.ICollection<global::Lime.FontChar>)result.CharCollection).Add(Clone_Lime__FontChar(cl, tmp1));
			}
			result.RoundCoordinates = s.RoundCoordinates;
			if (s.Textures != null && result.Textures != null) {
				var tmp3 = cl.GetCloner<global::Lime.ITexture>();
				foreach (var tmp2 in s.Textures)
					result.Textures.Add((global::Lime.ITexture)tmp3(tmp2));
			}
			return result;
		}

		protected static global::Lime.FontChar Clone_Lime__FontChar(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.FontChar))
				return (global::Lime.FontChar)cl.DeepObject(src);
			var s = (global::Lime.FontChar)src;
			var result = new global::Lime.FontChar();
			result.ACWidths = s.ACWidths;
			result.Char = s.Char;
			result.Height = s.Height;
			if (s.KerningPairs != null) {
				result.KerningPairs = new global::System.Collections.Generic.List<global::Lime.KerningPair>();
				foreach (var tmp1 in s.KerningPairs)
					result.KerningPairs.Add(tmp1);
			}
			result.RgbIntensity = s.RgbIntensity;
			result.TextureIndex = s.TextureIndex;
			result.UV0 = s.UV0;
			result.UV1 = s.UV1;
			result.VerticalOffset = s.VerticalOffset;
			result.Width = s.Width;
			return result;
		}

		protected static global::Lime.Frame Clone_Lime__Frame(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Frame))
				return (global::Lime.Frame)cl.DeepObject(src);
			var s = (global::Lime.Frame)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.Frame();
			result.Anchors = s.Anchors;
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Blending = s.Blending;
			result.ClipChildren = s.ClipChildren;
			if (s.IsNotDecorated()) {
				result.Color = s.Color;
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.HitTestMethod = s.HitTestMethod;
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Padding = s.Padding;
			result.Pivot = s.Pivot;
			result.Position = s.Position;
			result.RenderTarget = s.RenderTarget;
			result.Rotation = s.Rotation;
			result.Scale = s.Scale;
			result.Shader = s.Shader;
			result.SilentSize = s.SilentSize;
			result.SkinningWeights = Clone_Lime__SkinningWeights(cl, s.SkinningWeights);
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.Visible = s.Visible;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.FXAAMaterial Clone_Lime__FXAAMaterial(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.FXAAMaterial))
				return (global::Lime.FXAAMaterial)cl.DeepObject(src);
			var s = (global::Lime.FXAAMaterial)src;
			var result = new global::Lime.FXAAMaterial();
			result.Blending = s.Blending;
			result.LumaTreshold = s.LumaTreshold;
			result.MaxSpan = s.MaxSpan;
			result.MinReduce = s.MinReduce;
			result.MulReduce = s.MulReduce;
			result.Opaque = s.Opaque;
			result.TexelStep = s.TexelStep;
			return result;
		}

		protected static global::Lime.GradientComponent Clone_Lime__GradientComponent(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.GradientComponent))
				return (global::Lime.GradientComponent)cl.DeepObject(src);
			var s = (global::Lime.GradientComponent)src;
			var result = new global::Lime.GradientComponent();
			result.Angle = s.Angle;
			result.BlendMode = s.BlendMode;
			if (s.Gradient != null) {
				result.Gradient = new global::Lime.ColorGradient();
				foreach (var tmp1 in s.Gradient)
					result.Gradient.Add(Clone_Lime__GradientControlPoint(cl, tmp1));
			}
			return result;
		}

		protected static global::Lime.GradientControlPoint Clone_Lime__GradientControlPoint(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.GradientControlPoint))
				return (global::Lime.GradientControlPoint)cl.DeepObject(src);
			var s = (global::Lime.GradientControlPoint)src;
			var result = new global::Lime.GradientControlPoint();
			result.Color = s.Color;
			result.Position = s.Position;
			return result;
		}

		protected static global::Lime.GradientMaterial Clone_Lime__GradientMaterial(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.GradientMaterial))
				return (global::Lime.GradientMaterial)cl.DeepObject(src);
			var s = (global::Lime.GradientMaterial)src;
			var result = new global::Lime.GradientMaterial();
			result.Angle = s.Angle;
			result.BlendMode = s.BlendMode;
			if (s.Gradient != null) {
				result.Gradient = new global::Lime.ColorGradient();
				foreach (var tmp1 in s.Gradient)
					result.Gradient.Add(Clone_Lime__GradientControlPoint(cl, tmp1));
			}
			return result;
		}

		protected static global::Lime.HBoxLayout Clone_Lime__HBoxLayout(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.HBoxLayout))
				return (global::Lime.HBoxLayout)cl.DeepObject(src);
			var s = (global::Lime.HBoxLayout)src;
			var result = new global::Lime.HBoxLayout();
			result.DefaultCell = Clone_Lime__DefaultLayoutCell(cl, s.DefaultCell);
			result.Direction = s.Direction;
			result.IgnoreHidden = s.IgnoreHidden;
			result.Spacing = s.Spacing;
			return result;
		}

		protected static global::Lime.HSLComponent Clone_Lime__HSLComponent(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.HSLComponent))
				return (global::Lime.HSLComponent)cl.DeepObject(src);
			var s = (global::Lime.HSLComponent)src;
			var result = new global::Lime.HSLComponent();
			result.Hue = s.Hue;
			result.Lightness = s.Lightness;
			result.Saturation = s.Saturation;
			return result;
		}

		protected static global::Lime.Image Clone_Lime__Image(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Image))
				return (global::Lime.Image)cl.DeepObject(src);
			var s = (global::Lime.Image)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.Image();
			result.Anchors = s.Anchors;
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Blending = s.Blending;
			if (s.IsNotDecorated()) {
				result.Color = s.Color;
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.HitTestMethod = s.HitTestMethod;
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Padding = s.Padding;
			result.Pivot = s.Pivot;
			result.Position = s.Position;
			result.Rotation = s.Rotation;
			result.Scale = s.Scale;
			result.Shader = s.Shader;
			result.SilentSize = s.SilentSize;
			result.SkinningWeights = Clone_Lime__SkinningWeights(cl, s.SkinningWeights);
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			if (s.IsNotRenderTexture()) {
				result.Texture = (global::Lime.ITexture)cl.DeepObject(s.Texture);
			}
			result.UV0 = s.UV0;
			result.UV1 = s.UV1;
			result.Visible = s.Visible;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.ImageCombiner Clone_Lime__ImageCombiner(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.ImageCombiner))
				return (global::Lime.ImageCombiner)cl.DeepObject(src);
			var s = (global::Lime.ImageCombiner)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.ImageCombiner();
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Blending = s.Blending;
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			result.Enabled = s.Enabled;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Operation = s.Operation;
			result.Shader = s.Shader;
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.IntAnimator Clone_Lime__IntAnimator(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.IntAnimator))
				return (global::Lime.IntAnimator)cl.DeepObject(src);
			var s = (global::Lime.IntAnimator)src;
			var result = new global::Lime.IntAnimator();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		private static global::Lime.IntRectangle Clone_Lime__IntRectangle(Cloner cl, object src) =>
			(global::Lime.IntRectangle)src;

		private static global::Lime.IntVector2 Clone_Lime__IntVector2(Cloner cl, object src) =>
			(global::Lime.IntVector2)src;

		private static global::Lime.KerningPair Clone_Lime__KerningPair(Cloner cl, object src) =>
			(global::Lime.KerningPair)src;

		protected static global::Lime.Keyframe<global::Lime.Alignment> Clone_Lime__Keyframe_Alignment(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.Alignment>))
				return (global::Lime.Keyframe<global::Lime.Alignment>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.Alignment>)src;
			var result = new global::Lime.Keyframe<global::Lime.Alignment>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.Anchors> Clone_Lime__Keyframe_Anchors(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.Anchors>))
				return (global::Lime.Keyframe<global::Lime.Anchors>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.Anchors>)src;
			var result = new global::Lime.Keyframe<global::Lime.Anchors>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.AudioAction> Clone_Lime__Keyframe_AudioAction(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.AudioAction>))
				return (global::Lime.Keyframe<global::Lime.AudioAction>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.AudioAction>)src;
			var result = new global::Lime.Keyframe<global::Lime.AudioAction>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.Blending> Clone_Lime__Keyframe_Blending(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.Blending>))
				return (global::Lime.Keyframe<global::Lime.Blending>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.Blending>)src;
			var result = new global::Lime.Keyframe<global::Lime.Blending>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.ClipMethod> Clone_Lime__Keyframe_ClipMethod(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.ClipMethod>))
				return (global::Lime.Keyframe<global::Lime.ClipMethod>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.ClipMethod>)src;
			var result = new global::Lime.Keyframe<global::Lime.ClipMethod>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.Color4> Clone_Lime__Keyframe_Color4(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.Color4>))
				return (global::Lime.Keyframe<global::Lime.Color4>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.Color4>)src;
			var result = new global::Lime.Keyframe<global::Lime.Color4>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.EmissionType> Clone_Lime__Keyframe_EmissionType(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.EmissionType>))
				return (global::Lime.Keyframe<global::Lime.EmissionType>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.EmissionType>)src;
			var result = new global::Lime.Keyframe<global::Lime.EmissionType>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.EmitterShape> Clone_Lime__Keyframe_EmitterShape(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.EmitterShape>))
				return (global::Lime.Keyframe<global::Lime.EmitterShape>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.EmitterShape>)src;
			var result = new global::Lime.Keyframe<global::Lime.EmitterShape>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.HAlignment> Clone_Lime__Keyframe_HAlignment(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.HAlignment>))
				return (global::Lime.Keyframe<global::Lime.HAlignment>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.HAlignment>)src;
			var result = new global::Lime.Keyframe<global::Lime.HAlignment>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.ITexture> Clone_Lime__Keyframe_ITexture(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.ITexture>))
				return (global::Lime.Keyframe<global::Lime.ITexture>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.ITexture>)src;
			var result = new global::Lime.Keyframe<global::Lime.ITexture>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = (global::Lime.ITexture)cl.DeepObject(s.Value);
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.LayoutDirection> Clone_Lime__Keyframe_LayoutDirection(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.LayoutDirection>))
				return (global::Lime.Keyframe<global::Lime.LayoutDirection>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.LayoutDirection>)src;
			var result = new global::Lime.Keyframe<global::Lime.LayoutDirection>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.Matrix44> Clone_Lime__Keyframe_Matrix44(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.Matrix44>))
				return (global::Lime.Keyframe<global::Lime.Matrix44>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.Matrix44>)src;
			var result = new global::Lime.Keyframe<global::Lime.Matrix44>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.MovieAction> Clone_Lime__Keyframe_MovieAction(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.MovieAction>))
				return (global::Lime.Keyframe<global::Lime.MovieAction>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.MovieAction>)src;
			var result = new global::Lime.Keyframe<global::Lime.MovieAction>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Camera3D>> Clone_Lime__Keyframe_NodeReference_Camera3D(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Camera3D>>))
				return (global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Camera3D>>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Camera3D>>)src;
			var result = new global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Camera3D>>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = Clone_Lime__NodeReference_Camera3D(cl, s.Value);
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Node3D>> Clone_Lime__Keyframe_NodeReference_Node3D(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Node3D>>))
				return (global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Node3D>>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Node3D>>)src;
			var result = new global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Node3D>>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = (global::Lime.NodeReference<global::Lime.Node3D>)cl.DeepObject(s.Value);
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline>> Clone_Lime__Keyframe_NodeReference_Spline(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline>>))
				return (global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline>>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline>>)src;
			var result = new global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline>>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = Clone_Lime__NodeReference_Spline(cl, s.Value);
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline3D>> Clone_Lime__Keyframe_NodeReference_Spline3D(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline3D>>))
				return (global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline3D>>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline3D>>)src;
			var result = new global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline3D>>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = (global::Lime.NodeReference<global::Lime.Spline3D>)cl.DeepObject(s.Value);
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Widget>> Clone_Lime__Keyframe_NodeReference_Widget(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Widget>>))
				return (global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Widget>>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Widget>>)src;
			var result = new global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Widget>>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = Clone_Lime__NodeReference_Widget(cl, s.Value);
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.NumericRange> Clone_Lime__Keyframe_NumericRange(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.NumericRange>))
				return (global::Lime.Keyframe<global::Lime.NumericRange>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.NumericRange>)src;
			var result = new global::Lime.Keyframe<global::Lime.NumericRange>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.ParticlesLinkage> Clone_Lime__Keyframe_ParticlesLinkage(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.ParticlesLinkage>))
				return (global::Lime.Keyframe<global::Lime.ParticlesLinkage>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.ParticlesLinkage>)src;
			var result = new global::Lime.Keyframe<global::Lime.ParticlesLinkage>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.Quaternion> Clone_Lime__Keyframe_Quaternion(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.Quaternion>))
				return (global::Lime.Keyframe<global::Lime.Quaternion>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.Quaternion>)src;
			var result = new global::Lime.Keyframe<global::Lime.Quaternion>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.RenderTarget> Clone_Lime__Keyframe_RenderTarget(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.RenderTarget>))
				return (global::Lime.Keyframe<global::Lime.RenderTarget>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.RenderTarget>)src;
			var result = new global::Lime.Keyframe<global::Lime.RenderTarget>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.SerializableFont> Clone_Lime__Keyframe_SerializableFont(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.SerializableFont>))
				return (global::Lime.Keyframe<global::Lime.SerializableFont>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.SerializableFont>)src;
			var result = new global::Lime.Keyframe<global::Lime.SerializableFont>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = Clone_Lime__SerializableFont(cl, s.Value);
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.SerializableSample> Clone_Lime__Keyframe_SerializableSample(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.SerializableSample>))
				return (global::Lime.Keyframe<global::Lime.SerializableSample>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.SerializableSample>)src;
			var result = new global::Lime.Keyframe<global::Lime.SerializableSample>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = Clone_Lime__SerializableSample(cl, s.Value);
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.ShaderId> Clone_Lime__Keyframe_ShaderId(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.ShaderId>))
				return (global::Lime.Keyframe<global::Lime.ShaderId>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.ShaderId>)src;
			var result = new global::Lime.Keyframe<global::Lime.ShaderId>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.TextOverflowMode> Clone_Lime__Keyframe_TextOverflowMode(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.TextOverflowMode>))
				return (global::Lime.Keyframe<global::Lime.TextOverflowMode>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.TextOverflowMode>)src;
			var result = new global::Lime.Keyframe<global::Lime.TextOverflowMode>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.Thickness> Clone_Lime__Keyframe_Thickness(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.Thickness>))
				return (global::Lime.Keyframe<global::Lime.Thickness>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.Thickness>)src;
			var result = new global::Lime.Keyframe<global::Lime.Thickness>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.VAlignment> Clone_Lime__Keyframe_VAlignment(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.VAlignment>))
				return (global::Lime.Keyframe<global::Lime.VAlignment>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.VAlignment>)src;
			var result = new global::Lime.Keyframe<global::Lime.VAlignment>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.Vector2> Clone_Lime__Keyframe_Vector2(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.Vector2>))
				return (global::Lime.Keyframe<global::Lime.Vector2>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.Vector2>)src;
			var result = new global::Lime.Keyframe<global::Lime.Vector2>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<global::Lime.Vector3> Clone_Lime__Keyframe_Vector3(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<global::Lime.Vector3>))
				return (global::Lime.Keyframe<global::Lime.Vector3>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<global::Lime.Vector3>)src;
			var result = new global::Lime.Keyframe<global::Lime.Vector3>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<bool> Clone_Lime__Keyframe_Boolean(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<bool>))
				return (global::Lime.Keyframe<bool>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<bool>)src;
			var result = new global::Lime.Keyframe<bool>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<int> Clone_Lime__Keyframe_Int32(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<int>))
				return (global::Lime.Keyframe<int>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<int>)src;
			var result = new global::Lime.Keyframe<int>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<float> Clone_Lime__Keyframe_Single(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<float>))
				return (global::Lime.Keyframe<float>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<float>)src;
			var result = new global::Lime.Keyframe<float>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.Keyframe<string> Clone_Lime__Keyframe_String(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Keyframe<string>))
				return (global::Lime.Keyframe<string>)cl.DeepObject(src);
			var s = (global::Lime.Keyframe<string>)src;
			var result = new global::Lime.Keyframe<string>();
			result.Frame = s.Frame;
			result.PackedParams = s.PackedParams;
			result.Value = s.Value;
			return result;
		}

		protected static global::Lime.LayoutCell Clone_Lime__LayoutCell(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.LayoutCell))
				return (global::Lime.LayoutCell)cl.DeepObject(src);
			var s = (global::Lime.LayoutCell)src;
			var result = new global::Lime.LayoutCell();
			result.Alignment = s.Alignment;
			result.ColumnSpan = s.ColumnSpan;
			result.Ignore = s.Ignore;
			result.RowSpan = s.RowSpan;
			result.Stretch = s.Stretch;
			return result;
		}

		protected static global::Lime.LayoutConstraints Clone_Lime__LayoutConstraints(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.LayoutConstraints))
				return (global::Lime.LayoutConstraints)cl.DeepObject(src);
			var s = (global::Lime.LayoutConstraints)src;
			var result = new global::Lime.LayoutConstraints();
			result.MaxSize = s.MaxSize;
			result.MinSize = s.MinSize;
			return result;
		}

		protected static global::Lime.LinearLayout Clone_Lime__LinearLayout(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.LinearLayout))
				return (global::Lime.LinearLayout)cl.DeepObject(src);
			var s = (global::Lime.LinearLayout)src;
			var result = new global::Lime.LinearLayout();
			result.DefaultCell = Clone_Lime__DefaultLayoutCell(cl, s.DefaultCell);
			result.Direction = s.Direction;
			result.IgnoreHidden = s.IgnoreHidden;
			result.Spacing = s.Spacing;
			return result;
		}

		protected static global::Lime.Marker Clone_Lime__Marker(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Marker))
				return (global::Lime.Marker)cl.DeepObject(src);
			var s = (global::Lime.Marker)src;
			var result = new global::Lime.Marker();
			result.Action = s.Action;
			result.BezierEasing = s.BezierEasing;
			result.Frame = s.Frame;
			result.Id = s.Id;
			result.JumpTo = s.JumpTo;
			return result;
		}

		protected static global::Lime.MarkerBlending Clone_Lime__MarkerBlending(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.MarkerBlending))
				return (global::Lime.MarkerBlending)cl.DeepObject(src);
			var s = (global::Lime.MarkerBlending)src;
			var result = new global::Lime.MarkerBlending();
			result.Option = Clone_Lime__BlendingOption(cl, s.Option);
			if (s.SourceMarkersOptions != null) {
				result.SourceMarkersOptions = new global::System.Collections.Generic.Dictionary<string, global::Lime.BlendingOption>();
				foreach (var tmp1 in s.SourceMarkersOptions)
					result.SourceMarkersOptions.Add(tmp1.Key, Clone_Lime__BlendingOption(cl, tmp1.Value));
			}
			return result;
		}

		protected static global::Lime.Model3DAttachment.MaterialRemap Clone_Lime__Model3DAttachment__MaterialRemap(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Model3DAttachment.MaterialRemap))
				return (global::Lime.Model3DAttachment.MaterialRemap)cl.DeepObject(src);
			var s = (global::Lime.Model3DAttachment.MaterialRemap)src;
			var result = new global::Lime.Model3DAttachment.MaterialRemap();
			result.Material = (global::Lime.IMaterial)cl.DeepObject(s.Material);
			result.SourceName = s.SourceName;
			return result;
		}

		private static global::Lime.Matrix32 Clone_Lime__Matrix32(Cloner cl, object src) =>
			(global::Lime.Matrix32)src;

		private static global::Lime.Matrix44 Clone_Lime__Matrix44(Cloner cl, object src) =>
			(global::Lime.Matrix44)src;

		protected static global::Lime.Matrix44Animator Clone_Lime__Matrix44Animator(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Matrix44Animator))
				return (global::Lime.Matrix44Animator)cl.DeepObject(src);
			var s = (global::Lime.Matrix44Animator)src;
			var result = new global::Lime.Matrix44Animator();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.Mesh<global::Lime.Mesh3D.Vertex> Clone_Lime__Mesh_Mesh3D__Vertex(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Mesh<global::Lime.Mesh3D.Vertex>))
				return (global::Lime.Mesh<global::Lime.Mesh3D.Vertex>)cl.DeepObject(src);
			var s = (global::Lime.Mesh<global::Lime.Mesh3D.Vertex>)src;
			var result = new global::Lime.Mesh<global::Lime.Mesh3D.Vertex>();
			if (s.AttributeLocations != null) {
				result.AttributeLocations = new int[s.AttributeLocations.Length];
				Array.Copy(s.AttributeLocations, result.AttributeLocations, s.AttributeLocations.Length);
			}
			if (s.Indices != null) {
				result.Indices = new ushort[s.Indices.Length];
				Array.Copy(s.Indices, result.Indices, s.Indices.Length);
			}
			result.Topology = s.Topology;
			if (s.Vertices != null) {
				result.Vertices = new global::Lime.Mesh3D.Vertex[s.Vertices.Length];
				Array.Copy(s.Vertices, result.Vertices, s.Vertices.Length);
			}
			return result;
		}

		protected static global::Lime.Mesh3D Clone_Lime__Mesh3D(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Mesh3D))
				return (global::Lime.Mesh3D)cl.DeepObject(src);
			var s = (global::Lime.Mesh3D)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.Mesh3D();
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.BoundingSphere = s.BoundingSphere;
			result.Center = s.Center;
			result.Color = s.Color;
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			result.CullMode = s.CullMode;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Opaque = s.Opaque;
			result.Position = s.Position;
			result.Rotation = s.Rotation;
			result.Scale = s.Scale;
			result.SkinningMode = s.SkinningMode;
			if (s.Submeshes != null && result.Submeshes != null) {
				foreach (var tmp11 in s.Submeshes)
					result.Submeshes.Add(Clone_Lime__Submesh3D(cl, tmp11));
			}
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.Visible = s.Visible;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.Model3DAttachmentParser.MeshOptionFormat Clone_Lime__Model3DAttachmentParser__MeshOptionFormat(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Model3DAttachmentParser.MeshOptionFormat))
				return (global::Lime.Model3DAttachmentParser.MeshOptionFormat)cl.DeepObject(src);
			var s = (global::Lime.Model3DAttachmentParser.MeshOptionFormat)src;
			var result = new global::Lime.Model3DAttachmentParser.MeshOptionFormat();
			result.CullMode = s.CullMode;
			result.DisableMerging = s.DisableMerging;
			result.HitTestTarget = s.HitTestTarget;
			result.Opaque = s.Opaque;
			result.SkinningMode = s.SkinningMode;
			return result;
		}

		protected static global::Lime.Model3D Clone_Lime__Model3D(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Model3D))
				return (global::Lime.Model3D)cl.DeepObject(src);
			var s = (global::Lime.Model3D)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.Model3D();
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Color = s.Color;
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Opaque = s.Opaque;
			result.Position = s.Position;
			result.Rotation = s.Rotation;
			result.Scale = s.Scale;
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.Visible = s.Visible;
			s.OnAfterSerialization();
			result.AfterDeserialization();
			return result;
		}

		protected static global::Lime.Model3DAttachmentParser.ModelAnimationFormat Clone_Lime__Model3DAttachmentParser__ModelAnimationFormat(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Model3DAttachmentParser.ModelAnimationFormat))
				return (global::Lime.Model3DAttachmentParser.ModelAnimationFormat)cl.DeepObject(src);
			var s = (global::Lime.Model3DAttachmentParser.ModelAnimationFormat)src;
			var result = new global::Lime.Model3DAttachmentParser.ModelAnimationFormat();
			result.Blending = s.Blending;
			if (s.IgnoredNodes != null) {
				result.IgnoredNodes = new global::System.Collections.Generic.List<string>();
				foreach (var tmp1 in s.IgnoredNodes)
					result.IgnoredNodes.Add(tmp1);
			}
			if (s.ShouldSerializeLastFrame()) {
				result.LastFrame = s.LastFrame;
			}
			if (s.Markers != null) {
				result.Markers = new global::System.Collections.Generic.Dictionary<string, global::Lime.Model3DAttachmentParser.ModelMarkerFormat>();
				foreach (var tmp2 in s.Markers)
					result.Markers.Add(tmp2.Key, Clone_Lime__Model3DAttachmentParser__ModelMarkerFormat(cl, tmp2.Value));
			}
			if (s.Nodes != null) {
				result.Nodes = new global::System.Collections.Generic.List<string>();
				foreach (var tmp3 in s.Nodes)
					result.Nodes.Add(tmp3);
			}
			result.SourceAnimationId = s.SourceAnimationId;
			if (s.ShouldSerializeStartFrame()) {
				result.StartFrame = s.StartFrame;
			}
			return result;
		}

		protected static global::Lime.Model3DAttachmentParser.ModelAttachmentFormat Clone_Lime__Model3DAttachmentParser__ModelAttachmentFormat(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Model3DAttachmentParser.ModelAttachmentFormat))
				return (global::Lime.Model3DAttachmentParser.ModelAttachmentFormat)cl.DeepObject(src);
			var s = (global::Lime.Model3DAttachmentParser.ModelAttachmentFormat)src;
			var result = new global::Lime.Model3DAttachmentParser.ModelAttachmentFormat();
			if (s.Animations != null) {
				result.Animations = new global::System.Collections.Generic.Dictionary<string, global::Lime.Model3DAttachmentParser.ModelAnimationFormat>();
				foreach (var tmp1 in s.Animations)
					result.Animations.Add(tmp1.Key, Clone_Lime__Model3DAttachmentParser__ModelAnimationFormat(cl, tmp1.Value));
			}
			result.EntryTrigger = s.EntryTrigger;
			if (s.Materials != null) {
				result.Materials = new global::System.Collections.Generic.List<global::Lime.Model3DAttachment.MaterialRemap>();
				foreach (var tmp2 in s.Materials)
					result.Materials.Add(Clone_Lime__Model3DAttachment__MaterialRemap(cl, tmp2));
			}
			if (s.MeshOptions != null) {
				result.MeshOptions = new global::System.Collections.Generic.Dictionary<string, global::Lime.Model3DAttachmentParser.MeshOptionFormat>();
				foreach (var tmp3 in s.MeshOptions)
					result.MeshOptions.Add(tmp3.Key, Clone_Lime__Model3DAttachmentParser__MeshOptionFormat(cl, tmp3.Value));
			}
			if (s.NodeComponents != null) {
				result.NodeComponents = new global::System.Collections.Generic.Dictionary<string, global::Lime.Model3DAttachmentParser.ModelComponentsFormat>();
				foreach (var tmp4 in s.NodeComponents)
					result.NodeComponents.Add(tmp4.Key, Clone_Lime__Model3DAttachmentParser__ModelComponentsFormat(cl, tmp4.Value));
			}
			if (s.NodeRemovals != null) {
				result.NodeRemovals = new global::System.Collections.Generic.List<string>();
				foreach (var tmp5 in s.NodeRemovals)
					result.NodeRemovals.Add(tmp5);
			}
			result.ScaleFactor = s.ScaleFactor;
			if (s.SourceAnimationIds != null) {
				result.SourceAnimationIds = new global::System.Collections.Generic.List<string>();
				foreach (var tmp6 in s.SourceAnimationIds)
					result.SourceAnimationIds.Add(tmp6);
			}
			if (s.UVAnimations != null) {
				result.UVAnimations = new global::System.Collections.Generic.List<global::Lime.Model3DAttachmentParser.UVAnimationFormat>();
				foreach (var tmp7 in s.UVAnimations)
					result.UVAnimations.Add(Clone_Lime__Model3DAttachmentParser__UVAnimationFormat(cl, tmp7));
			}
			return result;
		}

		protected static global::Lime.Model3DAttachmentParser.ModelComponentsFormat Clone_Lime__Model3DAttachmentParser__ModelComponentsFormat(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Model3DAttachmentParser.ModelComponentsFormat))
				return (global::Lime.Model3DAttachmentParser.ModelComponentsFormat)cl.DeepObject(src);
			var s = (global::Lime.Model3DAttachmentParser.ModelComponentsFormat)src;
			var result = new global::Lime.Model3DAttachmentParser.ModelComponentsFormat();
			if (s.Components != null) {
				result.Components = new global::System.Collections.Generic.List<global::Lime.NodeComponent>();
				var tmp2 = cl.GetCloner<global::Lime.NodeComponent>();
				foreach (var tmp1 in s.Components)
					result.Components.Add((global::Lime.NodeComponent)tmp2(tmp1));
			}
			result.IsRoot = s.IsRoot;
			result.Node = s.Node;
			return result;
		}

		protected static global::Lime.Model3DAttachmentParser.ModelMarkerFormat Clone_Lime__Model3DAttachmentParser__ModelMarkerFormat(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Model3DAttachmentParser.ModelMarkerFormat))
				return (global::Lime.Model3DAttachmentParser.ModelMarkerFormat)cl.DeepObject(src);
			var s = (global::Lime.Model3DAttachmentParser.ModelMarkerFormat)src;
			var result = new global::Lime.Model3DAttachmentParser.ModelMarkerFormat();
			result.Action = s.Action;
			result.Blending = s.Blending;
			result.Frame = s.Frame;
			result.JumpTarget = s.JumpTarget;
			if (s.SourceMarkersBlending != null) {
				result.SourceMarkersBlending = new global::System.Collections.Generic.Dictionary<string, int>();
				foreach (var tmp1 in s.SourceMarkersBlending)
					result.SourceMarkersBlending.Add(tmp1.Key, tmp1.Value);
			}
			return result;
		}

		protected static global::Lime.Movie Clone_Lime__Movie(Cloner cl, object src)
		{
			if (src == null) return null;
			var s = (global::Lime.Movie)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.Movie();
			result.Anchors = s.Anchors;
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Blending = s.Blending;
			if (s.IsNotDecorated()) {
				result.Color = s.Color;
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.HitTestMethod = s.HitTestMethod;
			result.Id = s.Id;
			result.Looped = s.Looped;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Padding = s.Padding;
			result.Path = s.Path;
			result.Pivot = s.Pivot;
			result.Position = s.Position;
			result.Rotation = s.Rotation;
			result.Scale = s.Scale;
			result.Shader = s.Shader;
			result.SilentSize = s.SilentSize;
			result.SkinningWeights = Clone_Lime__SkinningWeights(cl, s.SkinningWeights);
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.Visible = s.Visible;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.NineGrid Clone_Lime__NineGrid(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.NineGrid))
				return (global::Lime.NineGrid)cl.DeepObject(src);
			var s = (global::Lime.NineGrid)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.NineGrid();
			result.Anchors = s.Anchors;
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Blending = s.Blending;
			result.BottomOffset = s.BottomOffset;
			if (s.IsNotDecorated()) {
				result.Color = s.Color;
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.HitTestMethod = s.HitTestMethod;
			result.Id = s.Id;
			result.LeftOffset = s.LeftOffset;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Padding = s.Padding;
			result.Pivot = s.Pivot;
			result.Position = s.Position;
			result.RightOffset = s.RightOffset;
			result.Rotation = s.Rotation;
			result.Scale = s.Scale;
			result.Shader = s.Shader;
			result.SilentSize = s.SilentSize;
			result.SkinningWeights = Clone_Lime__SkinningWeights(cl, s.SkinningWeights);
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.Texture = (global::Lime.ITexture)cl.DeepObject(s.Texture);
			result.TopOffset = s.TopOffset;
			result.Visible = s.Visible;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.Node3D Clone_Lime__Node3D(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Node3D))
				return (global::Lime.Node3D)cl.DeepObject(src);
			var s = (global::Lime.Node3D)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.Node3D();
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Color = s.Color;
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Opaque = s.Opaque;
			result.Position = s.Position;
			result.Rotation = s.Rotation;
			result.Scale = s.Scale;
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.Visible = s.Visible;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.NodeReference<global::Lime.Camera3D> Clone_Lime__NodeReference_Camera3D(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.NodeReference<global::Lime.Camera3D>))
				return (global::Lime.NodeReference<global::Lime.Camera3D>)cl.DeepObject(src);
			var s = (global::Lime.NodeReference<global::Lime.Camera3D>)src;
			var result = new global::Lime.NodeReference<global::Lime.Camera3D>();
			result.Id = s.Id;
			return result;
		}

		protected static global::Lime.NodeReference<global::Lime.Spline> Clone_Lime__NodeReference_Spline(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.NodeReference<global::Lime.Spline>))
				return (global::Lime.NodeReference<global::Lime.Spline>)cl.DeepObject(src);
			var s = (global::Lime.NodeReference<global::Lime.Spline>)src;
			var result = new global::Lime.NodeReference<global::Lime.Spline>();
			result.Id = s.Id;
			return result;
		}

		protected static global::Lime.NodeReference<global::Lime.Widget> Clone_Lime__NodeReference_Widget(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.NodeReference<global::Lime.Widget>))
				return (global::Lime.NodeReference<global::Lime.Widget>)cl.DeepObject(src);
			var s = (global::Lime.NodeReference<global::Lime.Widget>)src;
			var result = new global::Lime.NodeReference<global::Lime.Widget>();
			result.Id = s.Id;
			return result;
		}

		protected static global::Lime.NoiseMaterial Clone_Lime__NoiseMaterial(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.NoiseMaterial))
				return (global::Lime.NoiseMaterial)cl.DeepObject(src);
			var s = (global::Lime.NoiseMaterial)src;
			var result = new global::Lime.NoiseMaterial();
			result.BrightThreshold = s.BrightThreshold;
			result.DarkThreshold = s.DarkThreshold;
			result.Opaque = s.Opaque;
			result.SoftLight = s.SoftLight;
			return result;
		}

		protected static global::Lime.NumericAnimator Clone_Lime__NumericAnimator(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.NumericAnimator))
				return (global::Lime.NumericAnimator)cl.DeepObject(src);
			var s = (global::Lime.NumericAnimator)src;
			var result = new global::Lime.NumericAnimator();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		private static global::Lime.NumericRange Clone_Lime__NumericRange(Cloner cl, object src) =>
			(global::Lime.NumericRange)src;

		protected static global::Lime.NumericRangeAnimator Clone_Lime__NumericRangeAnimator(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.NumericRangeAnimator))
				return (global::Lime.NumericRangeAnimator)cl.DeepObject(src);
			var s = (global::Lime.NumericRangeAnimator)src;
			var result = new global::Lime.NumericRangeAnimator();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		private static global::Lime.TextureAtlasElement.Params Clone_Lime__TextureAtlasElement__Params(Cloner cl, object src) =>
			(global::Lime.TextureAtlasElement.Params)src;

		protected static global::Lime.ParticleEmitter Clone_Lime__ParticleEmitter(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.ParticleEmitter))
				return (global::Lime.ParticleEmitter)cl.DeepObject(src);
			var s = (global::Lime.ParticleEmitter)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.ParticleEmitter();
			result.AlongPathOrientation = s.AlongPathOrientation;
			result.Anchors = s.Anchors;
			result.AngularVelocity = s.AngularVelocity;
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.AspectRatio = s.AspectRatio;
			result.Blending = s.Blending;
			if (s.IsNotDecorated()) {
				result.Color = s.Color;
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			result.Direction = s.Direction;
			result.EmissionType = s.EmissionType;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.GravityAmount = s.GravityAmount;
			result.GravityDirection = s.GravityDirection;
			result.HitTestMethod = s.HitTestMethod;
			result.Id = s.Id;
			result.ImmortalParticles = s.ImmortalParticles;
			result.Lifetime = s.Lifetime;
			result.LinkageWidgetName = s.LinkageWidgetName;
			result.MagnetAmount = s.MagnetAmount;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Number = s.Number;
			result.Orientation = s.Orientation;
			result.Padding = s.Padding;
			result.ParticlesLinkage = s.ParticlesLinkage;
			result.Pivot = s.Pivot;
			result.Position = s.Position;
			result.RandomMotionAspectRatio = s.RandomMotionAspectRatio;
			result.RandomMotionRadius = s.RandomMotionRadius;
			result.RandomMotionRotation = s.RandomMotionRotation;
			result.RandomMotionSpeed = s.RandomMotionSpeed;
			result.Rotation = s.Rotation;
			result.Scale = s.Scale;
			result.Shader = s.Shader;
			result.Shape = s.Shape;
			result.SilentSize = s.SilentSize;
			result.SkinningWeights = Clone_Lime__SkinningWeights(cl, s.SkinningWeights);
			result.Speed = s.Speed;
			result.Spin = s.Spin;
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.TimeShift = s.TimeShift;
			result.Velocity = s.Velocity;
			result.Visible = s.Visible;
			result.WindAmount = s.WindAmount;
			result.WindDirection = s.WindDirection;
			result.Zoom = s.Zoom;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.ParticleModifier Clone_Lime__ParticleModifier(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.ParticleModifier))
				return (global::Lime.ParticleModifier)cl.DeepObject(src);
			var s = (global::Lime.ParticleModifier)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.ParticleModifier();
			result.AngularVelocity = s.AngularVelocity;
			result.AnimationFps = s.AnimationFps;
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Color = s.Color;
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			result.FirstFrame = s.FirstFrame;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.GravityAmount = s.GravityAmount;
			result.Id = s.Id;
			result.LastFrame = s.LastFrame;
			result.LoopedAnimation = s.LoopedAnimation;
			result.MagnetAmount = s.MagnetAmount;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Scale = s.Scale;
			result.Size = s.Size;
			result.Spin = s.Spin;
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.Texture = (global::Lime.ITexture)cl.DeepObject(s.Texture);
			result.Velocity = s.Velocity;
			result.WindAmount = s.WindAmount;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.ParticlesMagnet Clone_Lime__ParticlesMagnet(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.ParticlesMagnet))
				return (global::Lime.ParticlesMagnet)cl.DeepObject(src);
			var s = (global::Lime.ParticlesMagnet)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.ParticlesMagnet();
			result.Anchors = s.Anchors;
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Attenuation = s.Attenuation;
			result.Blending = s.Blending;
			if (s.IsNotDecorated()) {
				result.Color = s.Color;
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.HitTestMethod = s.HitTestMethod;
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Padding = s.Padding;
			result.Pivot = s.Pivot;
			result.Position = s.Position;
			result.Rotation = s.Rotation;
			result.Scale = s.Scale;
			result.Shader = s.Shader;
			result.Shape = s.Shape;
			result.SilentSize = s.SilentSize;
			result.SkinningWeights = Clone_Lime__SkinningWeights(cl, s.SkinningWeights);
			result.Strength = s.Strength;
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.Visible = s.Visible;
			s.OnAfterSerialization();
			return result;
		}

		private static global::Lime.Plane Clone_Lime__Plane(Cloner cl, object src) =>
			(global::Lime.Plane)src;

		protected static global::Lime.PointObject Clone_Lime__PointObject(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.PointObject))
				return (global::Lime.PointObject)cl.DeepObject(src);
			var s = (global::Lime.PointObject)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.PointObject();
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Position = s.Position;
			result.SkinningWeights = Clone_Lime__SkinningWeights(cl, s.SkinningWeights);
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.Polyline Clone_Lime__Polyline(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Polyline))
				return (global::Lime.Polyline)cl.DeepObject(src);
			var s = (global::Lime.Polyline)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.Polyline();
			result.Anchors = s.Anchors;
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Blending = s.Blending;
			result.Closed = s.Closed;
			if (s.IsNotDecorated()) {
				result.Color = s.Color;
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.HitTestMethod = s.HitTestMethod;
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Padding = s.Padding;
			result.Pivot = s.Pivot;
			result.Position = s.Position;
			result.Rotation = s.Rotation;
			result.Scale = s.Scale;
			result.Shader = s.Shader;
			result.SilentSize = s.SilentSize;
			result.SkinningWeights = Clone_Lime__SkinningWeights(cl, s.SkinningWeights);
			result.StaticThickness = s.StaticThickness;
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.Thickness = s.Thickness;
			result.Visible = s.Visible;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.PolylinePoint Clone_Lime__PolylinePoint(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.PolylinePoint))
				return (global::Lime.PolylinePoint)cl.DeepObject(src);
			var s = (global::Lime.PolylinePoint)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.PolylinePoint();
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Position = s.Position;
			result.SkinningWeights = Clone_Lime__SkinningWeights(cl, s.SkinningWeights);
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.PostProcessingComponent Clone_Lime__PostProcessingComponent(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.PostProcessingComponent))
				return (global::Lime.PostProcessingComponent)cl.DeepObject(src);
			var s = (global::Lime.PostProcessingComponent)src;
			var result = new global::Lime.PostProcessingComponent();
			result.BloomBrightThreshold = s.BloomBrightThreshold;
			result.BloomColor = s.BloomColor;
			result.BloomEnabled = s.BloomEnabled;
			result.BloomGammaCorrection = s.BloomGammaCorrection;
			result.BloomShaderId = s.BloomShaderId;
			result.BloomStrength = s.BloomStrength;
			result.BloomTextureScaling = s.BloomTextureScaling;
			result.BlurAlphaCorrection = s.BlurAlphaCorrection;
			result.BlurBackgroundColor = s.BlurBackgroundColor;
			result.BlurEnabled = s.BlurEnabled;
			result.BlurRadius = s.BlurRadius;
			result.BlurShader = s.BlurShader;
			result.BlurTextureScaling = s.BlurTextureScaling;
			result.DistortionBarrelPincushion = s.DistortionBarrelPincushion;
			result.DistortionBlue = s.DistortionBlue;
			result.DistortionChromaticAberration = s.DistortionChromaticAberration;
			result.DistortionEnabled = s.DistortionEnabled;
			result.DistortionGreen = s.DistortionGreen;
			result.DistortionRed = s.DistortionRed;
			result.FXAAEnabled = s.FXAAEnabled;
			result.FXAALumaTreshold = s.FXAALumaTreshold;
			result.FXAAMaxSpan = s.FXAAMaxSpan;
			result.FXAAMinReduce = s.FXAAMinReduce;
			result.FXAAMulReduce = s.FXAAMulReduce;
			result.HSL = s.HSL;
			result.HSLEnabled = s.HSLEnabled;
			result.NoiseBrightThreshold = s.NoiseBrightThreshold;
			result.NoiseDarkThreshold = s.NoiseDarkThreshold;
			result.NoiseEnabled = s.NoiseEnabled;
			result.NoiseOffset = s.NoiseOffset;
			result.NoiseScale = s.NoiseScale;
			result.NoiseSoftLight = s.NoiseSoftLight;
			if (s.RequiredSerializeNoiseTexture()) {
				result.NoiseTexture = (global::Lime.ITexture)cl.DeepObject(s.NoiseTexture);
			}
			result.OpagueRendering = s.OpagueRendering;
			result.OverallImpactColor = s.OverallImpactColor;
			result.OverallImpactEnabled = s.OverallImpactEnabled;
			result.RefreshSourceRate = s.RefreshSourceRate;
			result.RefreshSourceTexture = s.RefreshSourceTexture;
			result.SharpenEnabled = s.SharpenEnabled;
			result.SharpenLimit = s.SharpenLimit;
			result.SharpenStep = s.SharpenStep;
			result.SharpenStrength = s.SharpenStrength;
			result.TextureSizeLimit = s.TextureSizeLimit;
			result.VignetteColor = s.VignetteColor;
			result.VignetteEnabled = s.VignetteEnabled;
			result.VignettePivot = s.VignettePivot;
			result.VignetteRadius = s.VignetteRadius;
			result.VignetteScale = s.VignetteScale;
			result.VignetteSoftness = s.VignetteSoftness;
			return result;
		}

		private static global::Lime.Quaternion Clone_Lime__Quaternion(Cloner cl, object src) =>
			(global::Lime.Quaternion)src;

		protected static global::Lime.QuaternionAnimator Clone_Lime__QuaternionAnimator(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.QuaternionAnimator))
				return (global::Lime.QuaternionAnimator)cl.DeepObject(src);
			var s = (global::Lime.QuaternionAnimator)src;
			var result = new global::Lime.QuaternionAnimator();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		private static global::Lime.Ray Clone_Lime__Ray(Cloner cl, object src) =>
			(global::Lime.Ray)src;

		private static global::Lime.Rectangle Clone_Lime__Rectangle(Cloner cl, object src) =>
			(global::Lime.Rectangle)src;

		protected static global::Lime.RichText Clone_Lime__RichText(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.RichText))
				return (global::Lime.RichText)cl.DeepObject(src);
			var s = (global::Lime.RichText)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.RichText();
			result.Anchors = s.Anchors;
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Blending = s.Blending;
			if (s.IsNotDecorated()) {
				result.Color = s.Color;
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.HAlignment = s.HAlignment;
			result.HitTestMethod = s.HitTestMethod;
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.OverflowMode = s.OverflowMode;
			result.Padding = s.Padding;
			result.Pivot = s.Pivot;
			result.Position = s.Position;
			result.Rotation = s.Rotation;
			result.Scale = s.Scale;
			result.Shader = s.Shader;
			result.SilentSize = s.SilentSize;
			result.SkinningWeights = Clone_Lime__SkinningWeights(cl, s.SkinningWeights);
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.Text = s.Text;
			result.VAlignment = s.VAlignment;
			result.Visible = s.Visible;
			result.WordSplitAllowed = s.WordSplitAllowed;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.SignedDistanceField.SDFInnerShadowMaterial Clone_Lime_SignedDistanceField__SDFInnerShadowMaterial(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.SignedDistanceField.SDFInnerShadowMaterial))
				return (global::Lime.SignedDistanceField.SDFInnerShadowMaterial)cl.DeepObject(src);
			var s = (global::Lime.SignedDistanceField.SDFInnerShadowMaterial)src;
			var result = new global::Lime.SignedDistanceField.SDFInnerShadowMaterial();
			result.Blending = s.Blending;
			result.Color = s.Color;
			result.Dilate = s.Dilate;
			result.Offset = s.Offset;
			result.Softness = s.Softness;
			result.TextDilate = s.TextDilate;
			return result;
		}

		protected static global::Lime.SignedDistanceField.SDFShadowMaterial Clone_Lime_SignedDistanceField__SDFShadowMaterial(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.SignedDistanceField.SDFShadowMaterial))
				return (global::Lime.SignedDistanceField.SDFShadowMaterial)cl.DeepObject(src);
			var s = (global::Lime.SignedDistanceField.SDFShadowMaterial)src;
			var result = new global::Lime.SignedDistanceField.SDFShadowMaterial();
			result.Blending = s.Blending;
			result.Color = s.Color;
			result.Dilate = s.Dilate;
			result.Softness = s.Softness;
			return result;
		}

		protected static global::Lime.SerializableCompoundFont Clone_Lime__SerializableCompoundFont(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.SerializableCompoundFont))
				return (global::Lime.SerializableCompoundFont)cl.DeepObject(src);
			var s = (global::Lime.SerializableCompoundFont)src;
			var result = new global::Lime.SerializableCompoundFont();
			if (s.FontNames != null && result.FontNames != null) {
				foreach (var tmp1 in s.FontNames)
					result.FontNames.Add(tmp1);
			}
			return result;
		}

		protected static global::Lime.SerializableFont Clone_Lime__SerializableFont(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.SerializableFont))
				return (global::Lime.SerializableFont)cl.DeepObject(src);
			var s = (global::Lime.SerializableFont)src;
			var result = new global::Lime.SerializableFont();
			result.Name = s.Name;
			return result;
		}

		protected static global::Lime.SerializableSample Clone_Lime__SerializableSample(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.SerializableSample))
				return (global::Lime.SerializableSample)cl.DeepObject(src);
			var s = (global::Lime.SerializableSample)src;
			var result = new global::Lime.SerializableSample();
			result.SerializationPath = s.SerializationPath;
			return result;
		}

		protected static global::Lime.SerializableTexture Clone_Lime__SerializableTexture(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.SerializableTexture))
				return (global::Lime.SerializableTexture)cl.DeepObject(src);
			var s = (global::Lime.SerializableTexture)src;
			var result = new global::Lime.SerializableTexture();
			result.SerializationPath = s.SerializationPath;
			return result;
		}

		protected static global::Lime.ShadowParams Clone_Lime__ShadowParams(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.ShadowParams))
				return (global::Lime.ShadowParams)cl.DeepObject(src);
			var s = (global::Lime.ShadowParams)src;
			var result = new global::Lime.ShadowParams();
			result.Color = s.Color;
			result.Dilate = s.Dilate;
			result.Enabled = s.Enabled;
			result.OffsetX = s.OffsetX;
			result.OffsetY = s.OffsetY;
			result.Softness = s.Softness;
			return result;
		}

		protected static global::Lime.SharpenMaterial Clone_Lime__SharpenMaterial(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.SharpenMaterial))
				return (global::Lime.SharpenMaterial)cl.DeepObject(src);
			var s = (global::Lime.SharpenMaterial)src;
			var result = new global::Lime.SharpenMaterial();
			result.Blending = s.Blending;
			result.Limit = s.Limit;
			result.Opaque = s.Opaque;
			result.Step = s.Step;
			result.Strength = s.Strength;
			return result;
		}

		protected static global::Lime.SignedDistanceFieldComponent Clone_Lime__SignedDistanceFieldComponent(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.SignedDistanceFieldComponent))
				return (global::Lime.SignedDistanceFieldComponent)cl.DeepObject(src);
			var s = (global::Lime.SignedDistanceFieldComponent)src;
			var result = new global::Lime.SignedDistanceFieldComponent();
			result.Dilate = s.Dilate;
			if (s.Gradient != null) {
				result.Gradient = new global::Lime.ColorGradient();
				foreach (var tmp1 in s.Gradient)
					result.Gradient.Add(Clone_Lime__GradientControlPoint(cl, tmp1));
			}
			result.GradientAngle = s.GradientAngle;
			result.GradientEnabled = s.GradientEnabled;
			if (s.InnerShadows != null && result.InnerShadows != null) {
				foreach (var tmp2 in s.InnerShadows)
					result.InnerShadows.Add(Clone_Lime__ShadowParams(cl, tmp2));
			}
			result.OutlineColor = s.OutlineColor;
			if (s.Overlays != null && result.Overlays != null) {
				foreach (var tmp3 in s.Overlays)
					result.Overlays.Add(Clone_Lime__ShadowParams(cl, tmp3));
			}
			if (s.Shadows != null && result.Shadows != null) {
				foreach (var tmp4 in s.Shadows)
					result.Shadows.Add(Clone_Lime__ShadowParams(cl, tmp4));
			}
			result.Softness = s.Softness;
			result.Thickness = s.Thickness;
			return result;
		}

		protected static global::Lime.SignedDistanceField.SignedDistanceFieldMaterial Clone_Lime_SignedDistanceField__SignedDistanceFieldMaterial(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.SignedDistanceField.SignedDistanceFieldMaterial))
				return (global::Lime.SignedDistanceField.SignedDistanceFieldMaterial)cl.DeepObject(src);
			var s = (global::Lime.SignedDistanceField.SignedDistanceFieldMaterial)src;
			var result = new global::Lime.SignedDistanceField.SignedDistanceFieldMaterial();
			result.Blending = s.Blending;
			result.Dilate = s.Dilate;
			if (s.Gradient != null) {
				result.Gradient = new global::Lime.ColorGradient();
				foreach (var tmp1 in s.Gradient)
					result.Gradient.Add(Clone_Lime__GradientControlPoint(cl, tmp1));
			}
			result.GradientAngle = s.GradientAngle;
			result.GradientEnabled = s.GradientEnabled;
			result.OutlineColor = s.OutlineColor;
			result.Softness = s.Softness;
			result.Thickness = s.Thickness;
			return result;
		}

		protected static global::Lime.SimpleText Clone_Lime__SimpleText(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.SimpleText))
				return (global::Lime.SimpleText)cl.DeepObject(src);
			var s = (global::Lime.SimpleText)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.SimpleText();
			result.Anchors = s.Anchors;
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Blending = s.Blending;
			if (s.IsNotDecorated()) {
				result.Color = s.Color;
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.Font = Clone_Lime__SerializableFont(cl, s.Font);
			result.FontHeight = s.FontHeight;
			result.ForceUncutText = s.ForceUncutText;
			result.GradientMapIndex = s.GradientMapIndex;
			result.HAlignment = s.HAlignment;
			result.HitTestMethod = s.HitTestMethod;
			result.Id = s.Id;
			result.LetterSpacing = s.LetterSpacing;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.OverflowMode = s.OverflowMode;
			result.Padding = s.Padding;
			result.Pivot = s.Pivot;
			result.Position = s.Position;
			result.Rotation = s.Rotation;
			result.Scale = s.Scale;
			result.Shader = s.Shader;
			result.SilentSize = s.SilentSize;
			result.SkinningWeights = Clone_Lime__SkinningWeights(cl, s.SkinningWeights);
			result.Spacing = s.Spacing;
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.Text = s.Text;
			result.TextColor = s.TextColor;
			result.VAlignment = s.VAlignment;
			result.Visible = s.Visible;
			result.WordSplitAllowed = s.WordSplitAllowed;
			s.OnAfterSerialization();
			return result;
		}

		private static global::Lime.Size Clone_Lime__Size(Cloner cl, object src) =>
			(global::Lime.Size)src;

		protected static global::Lime.SkinningWeights Clone_Lime__SkinningWeights(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.SkinningWeights))
				return (global::Lime.SkinningWeights)cl.DeepObject(src);
			var s = (global::Lime.SkinningWeights)src;
			var result = new global::Lime.SkinningWeights();
			result.Bone0 = s.Bone0;
			result.Bone1 = s.Bone1;
			result.Bone2 = s.Bone2;
			result.Bone3 = s.Bone3;
			return result;
		}

		protected static global::Lime.Slider Clone_Lime__Slider(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Slider))
				return (global::Lime.Slider)cl.DeepObject(src);
			var s = (global::Lime.Slider)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.Slider();
			result.Anchors = s.Anchors;
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Blending = s.Blending;
			if (s.IsNotDecorated()) {
				result.Color = s.Color;
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.HitTestMethod = s.HitTestMethod;
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Padding = s.Padding;
			result.Pivot = s.Pivot;
			result.Position = s.Position;
			result.RangeMax = s.RangeMax;
			result.RangeMin = s.RangeMin;
			result.Rotation = s.Rotation;
			result.Scale = s.Scale;
			result.Shader = s.Shader;
			result.SilentSize = s.SilentSize;
			result.SkinningWeights = Clone_Lime__SkinningWeights(cl, s.SkinningWeights);
			result.Step = s.Step;
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.Value = s.Value;
			result.Visible = s.Visible;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.Spline Clone_Lime__Spline(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Spline))
				return (global::Lime.Spline)cl.DeepObject(src);
			var s = (global::Lime.Spline)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.Spline();
			result.Anchors = s.Anchors;
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Blending = s.Blending;
			result.Closed = s.Closed;
			if (s.IsNotDecorated()) {
				result.Color = s.Color;
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.HitTestMethod = s.HitTestMethod;
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Padding = s.Padding;
			result.Pivot = s.Pivot;
			result.Position = s.Position;
			result.Rotation = s.Rotation;
			result.Scale = s.Scale;
			result.Shader = s.Shader;
			result.SilentSize = s.SilentSize;
			result.SkinningWeights = Clone_Lime__SkinningWeights(cl, s.SkinningWeights);
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.Visible = s.Visible;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.Spline3D Clone_Lime__Spline3D(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Spline3D))
				return (global::Lime.Spline3D)cl.DeepObject(src);
			var s = (global::Lime.Spline3D)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.Spline3D();
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Closed = s.Closed;
			result.Color = s.Color;
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Opaque = s.Opaque;
			result.Position = s.Position;
			result.Rotation = s.Rotation;
			result.Scale = s.Scale;
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.Visible = s.Visible;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.SplineGear Clone_Lime__SplineGear(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.SplineGear))
				return (global::Lime.SplineGear)cl.DeepObject(src);
			var s = (global::Lime.SplineGear)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.SplineGear();
			result.AlongPathOrientation = s.AlongPathOrientation;
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.SplineOffset = s.SplineOffset;
			result.SplineRef = Clone_Lime__NodeReference_Spline(cl, s.SplineRef);
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.WidgetRef = Clone_Lime__NodeReference_Widget(cl, s.WidgetRef);
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.SplineGear3D Clone_Lime__SplineGear3D(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.SplineGear3D))
				return (global::Lime.SplineGear3D)cl.DeepObject(src);
			var s = (global::Lime.SplineGear3D)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.SplineGear3D();
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.Id = s.Id;
			result.NodeRef = (global::Lime.NodeReference<global::Lime.Node3D>)cl.DeepObject(s.NodeRef);
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.SplineOffset = s.SplineOffset;
			result.SplineRef = (global::Lime.NodeReference<global::Lime.Spline3D>)cl.DeepObject(s.SplineRef);
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.SplinePoint Clone_Lime__SplinePoint(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.SplinePoint))
				return (global::Lime.SplinePoint)cl.DeepObject(src);
			var s = (global::Lime.SplinePoint)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.SplinePoint();
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Position = s.Position;
			result.SkinningWeights = Clone_Lime__SkinningWeights(cl, s.SkinningWeights);
			result.Straight = s.Straight;
			result.Tag = s.Tag;
			result.TangentAngle = s.TangentAngle;
			result.TangentWeight = s.TangentWeight;
			result.TangerineFlags = s.TangerineFlags;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.SplinePoint3D Clone_Lime__SplinePoint3D(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.SplinePoint3D))
				return (global::Lime.SplinePoint3D)cl.DeepObject(src);
			var s = (global::Lime.SplinePoint3D)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.SplinePoint3D();
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.Id = s.Id;
			result.Interpolation = s.Interpolation;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Position = s.Position;
			result.Tag = s.Tag;
			result.TangentA = s.TangentA;
			result.TangentB = s.TangentB;
			result.TangerineFlags = s.TangerineFlags;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.StackLayout Clone_Lime__StackLayout(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.StackLayout))
				return (global::Lime.StackLayout)cl.DeepObject(src);
			var s = (global::Lime.StackLayout)src;
			var result = new global::Lime.StackLayout();
			result.DefaultCell = Clone_Lime__DefaultLayoutCell(cl, s.DefaultCell);
			result.HorizontallySizeable = s.HorizontallySizeable;
			result.IgnoreHidden = s.IgnoreHidden;
			result.VerticallySizeable = s.VerticallySizeable;
			return result;
		}

		protected static global::Lime.Submesh3D Clone_Lime__Submesh3D(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Submesh3D))
				return (global::Lime.Submesh3D)cl.DeepObject(src);
			var s = (global::Lime.Submesh3D)src;
			var result = new global::Lime.Submesh3D();
			if (s.BoneBindPoses != null && result.BoneBindPoses != null) {
				foreach (var tmp1 in s.BoneBindPoses)
					result.BoneBindPoses.Add(tmp1);
			}
			if (s.BoneNames != null && result.BoneNames != null) {
				foreach (var tmp2 in s.BoneNames)
					result.BoneNames.Add(tmp2);
			}
			result.Material = (global::Lime.IMaterial)cl.DeepObject(s.Material);
			result.Mesh = s.Mesh;
			return result;
		}

		protected static global::Lime.TableLayout Clone_Lime__TableLayout(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.TableLayout))
				return (global::Lime.TableLayout)cl.DeepObject(src);
			var s = (global::Lime.TableLayout)src;
			var result = new global::Lime.TableLayout();
			result.ColumnCount = s.ColumnCount;
			if (s.ColumnDefaults != null) {
				result.ColumnDefaults = new global::System.Collections.Generic.List<global::Lime.DefaultLayoutCell>();
				foreach (var tmp1 in s.ColumnDefaults)
					result.ColumnDefaults.Add(Clone_Lime__DefaultLayoutCell(cl, tmp1));
			}
			result.ColumnSpacing = s.ColumnSpacing;
			result.DefaultCell = Clone_Lime__DefaultLayoutCell(cl, s.DefaultCell);
			result.IgnoreHidden = s.IgnoreHidden;
			result.RowCount = s.RowCount;
			if (s.RowDefaults != null) {
				result.RowDefaults = new global::System.Collections.Generic.List<global::Lime.DefaultLayoutCell>();
				foreach (var tmp2 in s.RowDefaults)
					result.RowDefaults.Add(Clone_Lime__DefaultLayoutCell(cl, tmp2));
			}
			result.RowSpacing = s.RowSpacing;
			return result;
		}

		protected static global::Lime.TextStyle Clone_Lime__TextStyle(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.TextStyle))
				return (global::Lime.TextStyle)cl.DeepObject(src);
			var s = (global::Lime.TextStyle)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.TextStyle();
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Bold = s.Bold;
			result.CastShadow = s.CastShadow;
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.Font = Clone_Lime__SerializableFont(cl, s.Font);
			result.GradientMapIndex = s.GradientMapIndex;
			result.Id = s.Id;
			result.ImageSize = s.ImageSize;
			result.ImageTexture = (global::Lime.ITexture)cl.DeepObject(s.ImageTexture);
			result.ImageUsage = s.ImageUsage;
			result.LetterSpacing = s.LetterSpacing;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.ShadowColor = s.ShadowColor;
			result.ShadowOffset = s.ShadowOffset;
			result.Size = s.Size;
			result.SpaceAfter = s.SpaceAfter;
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.TextColor = s.TextColor;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.TextureParams Clone_Lime__TextureParams(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.TextureParams))
				return (global::Lime.TextureParams)cl.DeepObject(src);
			var s = (global::Lime.TextureParams)src;
			var result = new global::Lime.TextureParams();
			result.MagFilter = s.MagFilter;
			result.MinFilter = s.MinFilter;
			result.MipmapMode = s.MipmapMode;
			result.WrapModeU = s.WrapModeU;
			result.WrapModeV = s.WrapModeV;
			return result;
		}

		private static global::Lime.Thickness Clone_Lime__Thickness(Cloner cl, object src) =>
			(global::Lime.Thickness)src;

		protected static global::Lime.ThicknessAnimator Clone_Lime__ThicknessAnimator(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.ThicknessAnimator))
				return (global::Lime.ThicknessAnimator)cl.DeepObject(src);
			var s = (global::Lime.ThicknessAnimator)src;
			var result = new global::Lime.ThicknessAnimator();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		protected static global::Lime.TiledImage Clone_Lime__TiledImage(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.TiledImage))
				return (global::Lime.TiledImage)cl.DeepObject(src);
			var s = (global::Lime.TiledImage)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.TiledImage();
			result.Anchors = s.Anchors;
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Blending = s.Blending;
			if (s.IsNotDecorated()) {
				result.Color = s.Color;
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.HitTestMethod = s.HitTestMethod;
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Padding = s.Padding;
			result.Pivot = s.Pivot;
			result.Position = s.Position;
			result.Rotation = s.Rotation;
			result.Scale = s.Scale;
			result.Shader = s.Shader;
			result.SilentSize = s.SilentSize;
			result.SkinningWeights = Clone_Lime__SkinningWeights(cl, s.SkinningWeights);
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			if (s.IsNotRenderTexture()) {
				result.Texture = (global::Lime.ITexture)cl.DeepObject(s.Texture);
			}
			result.TileOffset = s.TileOffset;
			result.TileRounding = s.TileRounding;
			result.TileSize = s.TileSize;
			result.Visible = s.Visible;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.TwistComponent Clone_Lime__TwistComponent(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.TwistComponent))
				return (global::Lime.TwistComponent)cl.DeepObject(src);
			var s = (global::Lime.TwistComponent)src;
			var result = new global::Lime.TwistComponent();
			result.Angle = s.Angle;
			return result;
		}

		protected static global::Lime.TwistMaterial Clone_Lime__TwistMaterial(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.TwistMaterial))
				return (global::Lime.TwistMaterial)cl.DeepObject(src);
			var s = (global::Lime.TwistMaterial)src;
			var result = new global::Lime.TwistMaterial();
			result.Angle = s.Angle;
			return result;
		}

		protected static global::Lime.Model3DAttachmentParser.UVAnimationFormat Clone_Lime__Model3DAttachmentParser__UVAnimationFormat(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Model3DAttachmentParser.UVAnimationFormat))
				return (global::Lime.Model3DAttachmentParser.UVAnimationFormat)cl.DeepObject(src);
			var s = (global::Lime.Model3DAttachmentParser.UVAnimationFormat)src;
			var result = new global::Lime.Model3DAttachmentParser.UVAnimationFormat();
			result.AnimateOverlay = s.AnimateOverlay;
			result.AnimationSpeed = s.AnimationSpeed;
			result.AnimationType = s.AnimationType;
			result.BlendingMode = s.BlendingMode;
			result.DiffuseTexture = s.DiffuseTexture;
			result.MaskTexture = s.MaskTexture;
			result.MeshName = s.MeshName;
			result.OverlayTexture = s.OverlayTexture;
			result.TileX = s.TileX;
			result.TileY = s.TileY;
			return result;
		}

		protected static global::Lime.VBoxLayout Clone_Lime__VBoxLayout(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.VBoxLayout))
				return (global::Lime.VBoxLayout)cl.DeepObject(src);
			var s = (global::Lime.VBoxLayout)src;
			var result = new global::Lime.VBoxLayout();
			result.DefaultCell = Clone_Lime__DefaultLayoutCell(cl, s.DefaultCell);
			result.Direction = s.Direction;
			result.IgnoreHidden = s.IgnoreHidden;
			result.Spacing = s.Spacing;
			return result;
		}

		private static global::Lime.Vector2 Clone_Lime__Vector2(Cloner cl, object src) =>
			(global::Lime.Vector2)src;

		protected static global::Lime.Vector2Animator Clone_Lime__Vector2Animator(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Vector2Animator))
				return (global::Lime.Vector2Animator)cl.DeepObject(src);
			var s = (global::Lime.Vector2Animator)src;
			var result = new global::Lime.Vector2Animator();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		private static global::Lime.Vector3 Clone_Lime__Vector3(Cloner cl, object src) =>
			(global::Lime.Vector3)src;

		protected static global::Lime.Vector3Animator Clone_Lime__Vector3Animator(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Vector3Animator))
				return (global::Lime.Vector3Animator)cl.DeepObject(src);
			var s = (global::Lime.Vector3Animator)src;
			var result = new global::Lime.Vector3Animator();
			result.AnimationId = s.AnimationId;
			result.ReadonlyKeys = s.ReadonlyKeys;
			result.TargetPropertyPath = s.TargetPropertyPath;
			return result;
		}

		private static global::Lime.Vector4 Clone_Lime__Vector4(Cloner cl, object src) =>
			(global::Lime.Vector4)src;

		private static global::Lime.Mesh3D.Vertex Clone_Lime__Mesh3D__Vertex(Cloner cl, object src) =>
			(global::Lime.Mesh3D.Vertex)src;

		protected static global::Lime.VideoPlayer Clone_Lime__VideoPlayer(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.VideoPlayer))
				return (global::Lime.VideoPlayer)cl.DeepObject(src);
			var s = (global::Lime.VideoPlayer)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.VideoPlayer();
			result.Anchors = s.Anchors;
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Blending = s.Blending;
			if (s.IsNotDecorated()) {
				result.Color = s.Color;
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.HitTestMethod = s.HitTestMethod;
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Padding = s.Padding;
			result.Pivot = s.Pivot;
			result.Position = s.Position;
			result.Rotation = s.Rotation;
			result.Scale = s.Scale;
			result.Shader = s.Shader;
			result.SilentSize = s.SilentSize;
			result.SkinningWeights = Clone_Lime__SkinningWeights(cl, s.SkinningWeights);
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			if (s.IsNotRenderTexture()) {
				result.Texture = (global::Lime.ITexture)cl.DeepObject(s.Texture);
			}
			result.UV0 = s.UV0;
			result.UV1 = s.UV1;
			result.Visible = s.Visible;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.Viewport3D Clone_Lime__Viewport3D(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Viewport3D))
				return (global::Lime.Viewport3D)cl.DeepObject(src);
			var s = (global::Lime.Viewport3D)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.Viewport3D();
			result.Anchors = s.Anchors;
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Blending = s.Blending;
			result.CameraRef = Clone_Lime__NodeReference_Camera3D(cl, s.CameraRef);
			if (s.IsNotDecorated()) {
				result.Color = s.Color;
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.Frame = s.Frame;
			result.HitTestMethod = s.HitTestMethod;
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Padding = s.Padding;
			result.Pivot = s.Pivot;
			result.Position = s.Position;
			result.Rotation = s.Rotation;
			result.Scale = s.Scale;
			result.Shader = s.Shader;
			result.SilentSize = s.SilentSize;
			result.SkinningWeights = Clone_Lime__SkinningWeights(cl, s.SkinningWeights);
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.Visible = s.Visible;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.VignetteMaterial Clone_Lime__VignetteMaterial(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.VignetteMaterial))
				return (global::Lime.VignetteMaterial)cl.DeepObject(src);
			var s = (global::Lime.VignetteMaterial)src;
			var result = new global::Lime.VignetteMaterial();
			result.Blending = s.Blending;
			result.Color = s.Color;
			result.Radius = s.Radius;
			result.Softness = s.Softness;
			result.UV1 = s.UV1;
			result.UVOffset = s.UVOffset;
			return result;
		}

		protected static global::Lime.WaveComponent Clone_Lime__WaveComponent(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.WaveComponent))
				return (global::Lime.WaveComponent)cl.DeepObject(src);
			var s = (global::Lime.WaveComponent)src;
			var result = new global::Lime.WaveComponent();
			result.Amplitude = s.Amplitude;
			result.AutoLoopEnabled = s.AutoLoopEnabled;
			result.Frequency = s.Frequency;
			result.Point = s.Point;
			result.Time = s.Time;
			result.TimeSpeed = s.TimeSpeed;
			return result;
		}

		protected static global::Lime.WaveMaterial Clone_Lime__WaveMaterial(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.WaveMaterial))
				return (global::Lime.WaveMaterial)cl.DeepObject(src);
			var s = (global::Lime.WaveMaterial)src;
			var result = new global::Lime.WaveMaterial();
			result.Amplitude = s.Amplitude;
			result.AutoLoopEnabled = s.AutoLoopEnabled;
			result.Frequency = s.Frequency;
			result.Point = s.Point;
			result.Time = s.Time;
			result.TimeSpeed = s.TimeSpeed;
			return result;
		}

		protected static global::Lime.Widget Clone_Lime__Widget(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.Widget))
				return (global::Lime.Widget)cl.DeepObject(src);
			var s = (global::Lime.Widget)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.Widget();
			result.Anchors = s.Anchors;
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Blending = s.Blending;
			if (s.IsNotDecorated()) {
				result.Color = s.Color;
			}
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.HitTestMethod = s.HitTestMethod;
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Padding = s.Padding;
			result.Pivot = s.Pivot;
			result.Position = s.Position;
			result.Rotation = s.Rotation;
			result.Scale = s.Scale;
			result.Shader = s.Shader;
			result.SilentSize = s.SilentSize;
			result.SkinningWeights = Clone_Lime__SkinningWeights(cl, s.SkinningWeights);
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.Visible = s.Visible;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.WidgetAdapter3D Clone_Lime__WidgetAdapter3D(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.WidgetAdapter3D))
				return (global::Lime.WidgetAdapter3D)cl.DeepObject(src);
			var s = (global::Lime.WidgetAdapter3D)src;
			s.OnBeforeSerialization();
			var result = new global::Lime.WidgetAdapter3D();
			if (s.NeedSerializeAnimations()) {
				if (s.Animations != null && result.Animations != null) {
					foreach (var tmp1 in s.Animations)
						result.Animations.Add(Clone_Lime__Animation(cl, tmp1));
				}
			}
			if (s.Animators != null && result.Animators != null) {
				var tmp3 = cl.GetCloner<global::Lime.IAnimator>();
				foreach (var tmp2 in s.Animators)
					result.Animators.Add((global::Lime.IAnimator)tmp3(tmp2));
			}
			result.Color = s.Color;
			if (s.Components != null && result.Components != null) {
				var tmp5 = cl.GetCloner<global::Lime.NodeComponent>();
				int tmp6 = 0;
				foreach (var tmp4 in s.Components) {
					if (s.Components.SerializeItemIf(tmp6++, tmp4))
						result.Components.Add((global::Lime.NodeComponent)tmp5(tmp4));
				}
			}
			result.ContentsPath = s.ContentsPath;
			if (s.Folders != null) {
				result.Folders = new global::System.Collections.Generic.List<global::Lime.Folder.Descriptor>();
				var tmp8 = cl.GetCloner<global::Lime.Folder.Descriptor>();
				foreach (var tmp7 in s.Folders)
					result.Folders.Add((global::Lime.Folder.Descriptor)tmp8(tmp7));
			}
			result.Id = s.Id;
			if (s.ShouldSerializeNodes()) {
				if (s.Nodes != null && result.Nodes != null) {
					var tmp10 = cl.GetCloner<global::Lime.Node>();
					foreach (var tmp9 in s.Nodes)
						result.Nodes.Add((global::Lime.Node)tmp10(tmp9));
				}
			}
			result.Opaque = s.Opaque;
			result.Position = s.Position;
			result.Rotation = s.Rotation;
			result.Scale = s.Scale;
			result.Tag = s.Tag;
			result.TangerineFlags = s.TangerineFlags;
			result.Visible = s.Visible;
			s.OnAfterSerialization();
			return result;
		}

		protected static global::Lime.VideoDecoder.YUVtoRGBMaterial Clone_Lime__VideoDecoder__YUVtoRGBMaterial(Cloner cl, object src)
		{
			if (src == null) return null;
			if (src.GetType() != typeof(global::Lime.VideoDecoder.YUVtoRGBMaterial))
				return (global::Lime.VideoDecoder.YUVtoRGBMaterial)cl.DeepObject(src);
			var s = (global::Lime.VideoDecoder.YUVtoRGBMaterial)src;
			var result = new global::Lime.VideoDecoder.YUVtoRGBMaterial();
			result.Strength = s.Strength;
			return result;
		}

		static LimeCloner()
		{
			clonerCache[typeof(global::Lime.Alignment)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.AlphaIntensityComponent)] = Clone_Lime__AlphaIntensityComponent;
			clonerCache[typeof(global::Lime.AlphaIntensityMaterial)] = Clone_Lime__AlphaIntensityMaterial;
			clonerCache[typeof(global::Lime.Animation)] = Clone_Lime__Animation;
			clonerCache[typeof(global::Lime.AnimationBlender)] = Clone_Lime__AnimationBlender;
			clonerCache[typeof(global::Lime.AnimationBlending)] = Clone_Lime__AnimationBlending;
			clonerCache[typeof(global::Lime.AnimationClip)] = Clone_Lime__AnimationClip;
			clonerCache[typeof(global::Lime.Animation.AnimationData)] = Clone_Lime__Animation__AnimationData;
			clonerCache[typeof(global::Lime.AnimationTrack)] = Clone_Lime__AnimationTrack;
			clonerCache[typeof(global::Lime.Animator<global::Lime.Alignment>)] = Clone_Lime__Animator_Alignment;
			clonerCache[typeof(global::Lime.Animator<global::Lime.Anchors>)] = Clone_Lime__Animator_Anchors;
			clonerCache[typeof(global::Lime.Animator<global::Lime.AudioAction>)] = Clone_Lime__Animator_AudioAction;
			clonerCache[typeof(global::Lime.Animator<global::Lime.Blending>)] = Clone_Lime__Animator_Blending;
			clonerCache[typeof(global::Lime.Animator<global::Lime.ClipMethod>)] = Clone_Lime__Animator_ClipMethod;
			clonerCache[typeof(global::Lime.Animator<global::Lime.Color4>)] = Clone_Lime__Animator_Color4;
			clonerCache[typeof(global::Lime.Animator<global::Lime.EmissionType>)] = Clone_Lime__Animator_EmissionType;
			clonerCache[typeof(global::Lime.Animator<global::Lime.EmitterShape>)] = Clone_Lime__Animator_EmitterShape;
			clonerCache[typeof(global::Lime.Animator<global::Lime.HAlignment>)] = Clone_Lime__Animator_HAlignment;
			clonerCache[typeof(global::Lime.Animator<global::Lime.ITexture>)] = Clone_Lime__Animator_ITexture;
			clonerCache[typeof(global::Lime.Animator<global::Lime.LayoutDirection>)] = Clone_Lime__Animator_LayoutDirection;
			clonerCache[typeof(global::Lime.Animator<global::Lime.Matrix44>)] = Clone_Lime__Animator_Matrix44;
			clonerCache[typeof(global::Lime.Animator<global::Lime.MovieAction>)] = Clone_Lime__Animator_MovieAction;
			clonerCache[typeof(global::Lime.Animator<global::Lime.NodeReference<global::Lime.Camera3D>>)] = Clone_Lime__Animator_NodeReference_Camera3D;
			clonerCache[typeof(global::Lime.Animator<global::Lime.NodeReference<global::Lime.Node3D>>)] = Clone_Lime__Animator_NodeReference_Node3D;
			clonerCache[typeof(global::Lime.Animator<global::Lime.NodeReference<global::Lime.Spline>>)] = Clone_Lime__Animator_NodeReference_Spline;
			clonerCache[typeof(global::Lime.Animator<global::Lime.NodeReference<global::Lime.Spline3D>>)] = Clone_Lime__Animator_NodeReference_Spline3D;
			clonerCache[typeof(global::Lime.Animator<global::Lime.NodeReference<global::Lime.Widget>>)] = Clone_Lime__Animator_NodeReference_Widget;
			clonerCache[typeof(global::Lime.Animator<global::Lime.NumericRange>)] = Clone_Lime__Animator_NumericRange;
			clonerCache[typeof(global::Lime.Animator<global::Lime.ParticlesLinkage>)] = Clone_Lime__Animator_ParticlesLinkage;
			clonerCache[typeof(global::Lime.Animator<global::Lime.Quaternion>)] = Clone_Lime__Animator_Quaternion;
			clonerCache[typeof(global::Lime.Animator<global::Lime.RenderTarget>)] = Clone_Lime__Animator_RenderTarget;
			clonerCache[typeof(global::Lime.Animator<global::Lime.SerializableFont>)] = Clone_Lime__Animator_SerializableFont;
			clonerCache[typeof(global::Lime.Animator<global::Lime.SerializableSample>)] = Clone_Lime__Animator_SerializableSample;
			clonerCache[typeof(global::Lime.Animator<global::Lime.ShaderId>)] = Clone_Lime__Animator_ShaderId;
			clonerCache[typeof(global::Lime.Animator<global::Lime.TextOverflowMode>)] = Clone_Lime__Animator_TextOverflowMode;
			clonerCache[typeof(global::Lime.Animator<global::Lime.Thickness>)] = Clone_Lime__Animator_Thickness;
			clonerCache[typeof(global::Lime.Animator<global::Lime.VAlignment>)] = Clone_Lime__Animator_VAlignment;
			clonerCache[typeof(global::Lime.Animator<global::Lime.Vector2>)] = Clone_Lime__Animator_Vector2;
			clonerCache[typeof(global::Lime.Animator<global::Lime.Vector3>)] = Clone_Lime__Animator_Vector3;
			clonerCache[typeof(global::Lime.Animator<bool>)] = Clone_Lime__Animator_Boolean;
			clonerCache[typeof(global::Lime.Animator<int>)] = Clone_Lime__Animator_Int32;
			clonerCache[typeof(global::Lime.Animator<float>)] = Clone_Lime__Animator_Single;
			clonerCache[typeof(global::Lime.Animator<string>)] = Clone_Lime__Animator_String;
			clonerCache[typeof(global::Lime.Node.AssetBundlePathComponent)] = Clone_Lime__Node__AssetBundlePathComponent;
			clonerCache[typeof(global::Lime.Audio)] = Clone_Lime__Audio;
			clonerCache[typeof(global::Lime.AudioRandomizerComponent)] = Clone_Lime__AudioRandomizerComponent;
			clonerCache[typeof(global::Lime.BezierEasing)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.BitSet32)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.Mesh3D.BlendIndices)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.BlendingOption)] = Clone_Lime__BlendingOption;
			clonerCache[typeof(global::Lime.Mesh3D.BlendWeights)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.BloomMaterial)] = Clone_Lime__BloomMaterial;
			clonerCache[typeof(global::Lime.BlurMaterial)] = Clone_Lime__BlurMaterial;
			clonerCache[typeof(global::Lime.Bone)] = Clone_Lime__Bone;
			clonerCache[typeof(global::Lime.BoneArray)] = Clone_Lime__BoneArray_obj;
			clonerCache[typeof(global::Lime.BoneWeight)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.BoundingSphere)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.Bounds)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.Button)] = Clone_Lime__Button;
			clonerCache[typeof(global::Lime.Camera3D)] = Clone_Lime__Camera3D;
			clonerCache[typeof(global::Lime.Color4)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.Color4Animator)] = Clone_Lime__Color4Animator;
			clonerCache[typeof(global::Lime.ColorCorrectionMaterial)] = Clone_Lime__ColorCorrectionMaterial;
			clonerCache[typeof(global::Lime.CommonMaterial)] = Clone_Lime__CommonMaterial;
			clonerCache[typeof(global::Lime.RenderOptimizer.ContentBox)] = Clone_Lime_RenderOptimizer__ContentBox;
			clonerCache[typeof(global::Lime.RenderOptimizer.ContentPlane)] = Clone_Lime_RenderOptimizer__ContentPlane;
			clonerCache[typeof(global::Lime.RenderOptimizer.ContentRectangle)] = Clone_Lime_RenderOptimizer__ContentRectangle;
			clonerCache[typeof(global::Lime.RenderOptimizer.ContentSizeComponent)] = Clone_Lime_RenderOptimizer__ContentSizeComponent;
			clonerCache[typeof(global::Lime.DefaultLayoutCell)] = Clone_Lime__DefaultLayoutCell;
			clonerCache[typeof(global::Lime.DissolveComponent)] = Clone_Lime__DissolveComponent;
			clonerCache[typeof(global::Lime.DissolveMaterial)] = Clone_Lime__DissolveMaterial;
			clonerCache[typeof(global::Lime.DistortionMaterial)] = Clone_Lime__DistortionMaterial;
			clonerCache[typeof(global::Lime.DistortionMesh)] = Clone_Lime__DistortionMesh;
			clonerCache[typeof(global::Lime.DistortionMeshPoint)] = Clone_Lime__DistortionMeshPoint;
			clonerCache[typeof(global::Lime.EmitterShapePoint)] = Clone_Lime__EmitterShapePoint;
			clonerCache[typeof(global::Lime.Font)] = Clone_Lime__Font;
			clonerCache[typeof(global::Lime.FontChar)] = Clone_Lime__FontChar;
			clonerCache[typeof(global::Lime.Frame)] = Clone_Lime__Frame;
			clonerCache[typeof(global::Lime.FXAAMaterial)] = Clone_Lime__FXAAMaterial;
			clonerCache[typeof(global::Lime.GradientComponent)] = Clone_Lime__GradientComponent;
			clonerCache[typeof(global::Lime.GradientControlPoint)] = Clone_Lime__GradientControlPoint;
			clonerCache[typeof(global::Lime.GradientMaterial)] = Clone_Lime__GradientMaterial;
			clonerCache[typeof(global::Lime.HBoxLayout)] = Clone_Lime__HBoxLayout;
			clonerCache[typeof(global::Lime.HSLComponent)] = Clone_Lime__HSLComponent;
			clonerCache[typeof(global::Lime.Image)] = Clone_Lime__Image;
			clonerCache[typeof(global::Lime.ImageCombiner)] = Clone_Lime__ImageCombiner;
			clonerCache[typeof(global::Lime.IntAnimator)] = Clone_Lime__IntAnimator;
			clonerCache[typeof(global::Lime.IntRectangle)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.IntVector2)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.KerningPair)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.Alignment>)] = Clone_Lime__Keyframe_Alignment;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.Anchors>)] = Clone_Lime__Keyframe_Anchors;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.AudioAction>)] = Clone_Lime__Keyframe_AudioAction;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.Blending>)] = Clone_Lime__Keyframe_Blending;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.ClipMethod>)] = Clone_Lime__Keyframe_ClipMethod;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.Color4>)] = Clone_Lime__Keyframe_Color4;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.EmissionType>)] = Clone_Lime__Keyframe_EmissionType;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.EmitterShape>)] = Clone_Lime__Keyframe_EmitterShape;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.HAlignment>)] = Clone_Lime__Keyframe_HAlignment;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.ITexture>)] = Clone_Lime__Keyframe_ITexture;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.LayoutDirection>)] = Clone_Lime__Keyframe_LayoutDirection;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.Matrix44>)] = Clone_Lime__Keyframe_Matrix44;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.MovieAction>)] = Clone_Lime__Keyframe_MovieAction;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Camera3D>>)] = Clone_Lime__Keyframe_NodeReference_Camera3D;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Node3D>>)] = Clone_Lime__Keyframe_NodeReference_Node3D;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline>>)] = Clone_Lime__Keyframe_NodeReference_Spline;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Spline3D>>)] = Clone_Lime__Keyframe_NodeReference_Spline3D;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.NodeReference<global::Lime.Widget>>)] = Clone_Lime__Keyframe_NodeReference_Widget;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.NumericRange>)] = Clone_Lime__Keyframe_NumericRange;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.ParticlesLinkage>)] = Clone_Lime__Keyframe_ParticlesLinkage;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.Quaternion>)] = Clone_Lime__Keyframe_Quaternion;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.RenderTarget>)] = Clone_Lime__Keyframe_RenderTarget;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.SerializableFont>)] = Clone_Lime__Keyframe_SerializableFont;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.SerializableSample>)] = Clone_Lime__Keyframe_SerializableSample;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.ShaderId>)] = Clone_Lime__Keyframe_ShaderId;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.TextOverflowMode>)] = Clone_Lime__Keyframe_TextOverflowMode;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.Thickness>)] = Clone_Lime__Keyframe_Thickness;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.VAlignment>)] = Clone_Lime__Keyframe_VAlignment;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.Vector2>)] = Clone_Lime__Keyframe_Vector2;
			clonerCache[typeof(global::Lime.Keyframe<global::Lime.Vector3>)] = Clone_Lime__Keyframe_Vector3;
			clonerCache[typeof(global::Lime.Keyframe<bool>)] = Clone_Lime__Keyframe_Boolean;
			clonerCache[typeof(global::Lime.Keyframe<int>)] = Clone_Lime__Keyframe_Int32;
			clonerCache[typeof(global::Lime.Keyframe<float>)] = Clone_Lime__Keyframe_Single;
			clonerCache[typeof(global::Lime.Keyframe<string>)] = Clone_Lime__Keyframe_String;
			clonerCache[typeof(global::Lime.LayoutCell)] = Clone_Lime__LayoutCell;
			clonerCache[typeof(global::Lime.LayoutConstraints)] = Clone_Lime__LayoutConstraints;
			clonerCache[typeof(global::Lime.LinearLayout)] = Clone_Lime__LinearLayout;
			clonerCache[typeof(global::Lime.Marker)] = Clone_Lime__Marker;
			clonerCache[typeof(global::Lime.MarkerBlending)] = Clone_Lime__MarkerBlending;
			clonerCache[typeof(global::Lime.Model3DAttachment.MaterialRemap)] = Clone_Lime__Model3DAttachment__MaterialRemap;
			clonerCache[typeof(global::Lime.Matrix32)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.Matrix44)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.Matrix44Animator)] = Clone_Lime__Matrix44Animator;
			clonerCache[typeof(global::Lime.Mesh<global::Lime.Mesh3D.Vertex>)] = Clone_Lime__Mesh_Mesh3D__Vertex;
			clonerCache[typeof(global::Lime.Mesh3D)] = Clone_Lime__Mesh3D;
			clonerCache[typeof(global::Lime.Model3DAttachmentParser.MeshOptionFormat)] = Clone_Lime__Model3DAttachmentParser__MeshOptionFormat;
			clonerCache[typeof(global::Lime.Model3D)] = Clone_Lime__Model3D;
			clonerCache[typeof(global::Lime.Model3DAttachmentParser.ModelAnimationFormat)] = Clone_Lime__Model3DAttachmentParser__ModelAnimationFormat;
			clonerCache[typeof(global::Lime.Model3DAttachmentParser.ModelAttachmentFormat)] = Clone_Lime__Model3DAttachmentParser__ModelAttachmentFormat;
			clonerCache[typeof(global::Lime.Model3DAttachmentParser.ModelComponentsFormat)] = Clone_Lime__Model3DAttachmentParser__ModelComponentsFormat;
			clonerCache[typeof(global::Lime.Model3DAttachmentParser.ModelMarkerFormat)] = Clone_Lime__Model3DAttachmentParser__ModelMarkerFormat;
			clonerCache[typeof(global::Lime.Movie)] = Clone_Lime__Movie;
			clonerCache[typeof(global::Lime.NineGrid)] = Clone_Lime__NineGrid;
			clonerCache[typeof(global::Lime.Node3D)] = Clone_Lime__Node3D;
			clonerCache[typeof(global::Lime.NodeReference<global::Lime.Camera3D>)] = Clone_Lime__NodeReference_Camera3D;
			clonerCache[typeof(global::Lime.NodeReference<global::Lime.Spline>)] = Clone_Lime__NodeReference_Spline;
			clonerCache[typeof(global::Lime.NodeReference<global::Lime.Widget>)] = Clone_Lime__NodeReference_Widget;
			clonerCache[typeof(global::Lime.NoiseMaterial)] = Clone_Lime__NoiseMaterial;
			clonerCache[typeof(global::Lime.NumericAnimator)] = Clone_Lime__NumericAnimator;
			clonerCache[typeof(global::Lime.NumericRange)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.NumericRangeAnimator)] = Clone_Lime__NumericRangeAnimator;
			clonerCache[typeof(global::Lime.TextureAtlasElement.Params)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.ParticleEmitter)] = Clone_Lime__ParticleEmitter;
			clonerCache[typeof(global::Lime.ParticleModifier)] = Clone_Lime__ParticleModifier;
			clonerCache[typeof(global::Lime.ParticlesMagnet)] = Clone_Lime__ParticlesMagnet;
			clonerCache[typeof(global::Lime.Plane)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.PointObject)] = Clone_Lime__PointObject;
			clonerCache[typeof(global::Lime.Polyline)] = Clone_Lime__Polyline;
			clonerCache[typeof(global::Lime.PolylinePoint)] = Clone_Lime__PolylinePoint;
			clonerCache[typeof(global::Lime.PostProcessingComponent)] = Clone_Lime__PostProcessingComponent;
			clonerCache[typeof(global::Lime.Quaternion)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.QuaternionAnimator)] = Clone_Lime__QuaternionAnimator;
			clonerCache[typeof(global::Lime.Ray)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.Rectangle)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.RichText)] = Clone_Lime__RichText;
			clonerCache[typeof(global::Lime.SignedDistanceField.SDFInnerShadowMaterial)] = Clone_Lime_SignedDistanceField__SDFInnerShadowMaterial;
			clonerCache[typeof(global::Lime.SignedDistanceField.SDFShadowMaterial)] = Clone_Lime_SignedDistanceField__SDFShadowMaterial;
			clonerCache[typeof(global::Lime.SerializableCompoundFont)] = Clone_Lime__SerializableCompoundFont;
			clonerCache[typeof(global::Lime.SerializableFont)] = Clone_Lime__SerializableFont;
			clonerCache[typeof(global::Lime.SerializableSample)] = Clone_Lime__SerializableSample;
			clonerCache[typeof(global::Lime.SerializableTexture)] = Clone_Lime__SerializableTexture;
			clonerCache[typeof(global::Lime.ShadowParams)] = Clone_Lime__ShadowParams;
			clonerCache[typeof(global::Lime.SharpenMaterial)] = Clone_Lime__SharpenMaterial;
			clonerCache[typeof(global::Lime.SignedDistanceFieldComponent)] = Clone_Lime__SignedDistanceFieldComponent;
			clonerCache[typeof(global::Lime.SignedDistanceField.SignedDistanceFieldMaterial)] = Clone_Lime_SignedDistanceField__SignedDistanceFieldMaterial;
			clonerCache[typeof(global::Lime.SimpleText)] = Clone_Lime__SimpleText;
			clonerCache[typeof(global::Lime.Size)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.SkinningWeights)] = Clone_Lime__SkinningWeights;
			clonerCache[typeof(global::Lime.Slider)] = Clone_Lime__Slider;
			clonerCache[typeof(global::Lime.Spline)] = Clone_Lime__Spline;
			clonerCache[typeof(global::Lime.Spline3D)] = Clone_Lime__Spline3D;
			clonerCache[typeof(global::Lime.SplineGear)] = Clone_Lime__SplineGear;
			clonerCache[typeof(global::Lime.SplineGear3D)] = Clone_Lime__SplineGear3D;
			clonerCache[typeof(global::Lime.SplinePoint)] = Clone_Lime__SplinePoint;
			clonerCache[typeof(global::Lime.SplinePoint3D)] = Clone_Lime__SplinePoint3D;
			clonerCache[typeof(global::Lime.StackLayout)] = Clone_Lime__StackLayout;
			clonerCache[typeof(global::Lime.Submesh3D)] = Clone_Lime__Submesh3D;
			clonerCache[typeof(global::Lime.TableLayout)] = Clone_Lime__TableLayout;
			clonerCache[typeof(global::Lime.TextStyle)] = Clone_Lime__TextStyle;
			clonerCache[typeof(global::Lime.TextureParams)] = Clone_Lime__TextureParams;
			clonerCache[typeof(global::Lime.Thickness)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.ThicknessAnimator)] = Clone_Lime__ThicknessAnimator;
			clonerCache[typeof(global::Lime.TiledImage)] = Clone_Lime__TiledImage;
			clonerCache[typeof(global::Lime.TwistComponent)] = Clone_Lime__TwistComponent;
			clonerCache[typeof(global::Lime.TwistMaterial)] = Clone_Lime__TwistMaterial;
			clonerCache[typeof(global::Lime.Model3DAttachmentParser.UVAnimationFormat)] = Clone_Lime__Model3DAttachmentParser__UVAnimationFormat;
			clonerCache[typeof(global::Lime.VBoxLayout)] = Clone_Lime__VBoxLayout;
			clonerCache[typeof(global::Lime.Vector2)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.Vector2Animator)] = Clone_Lime__Vector2Animator;
			clonerCache[typeof(global::Lime.Vector3)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.Vector3Animator)] = Clone_Lime__Vector3Animator;
			clonerCache[typeof(global::Lime.Vector4)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.Mesh3D.Vertex)] = ValueCopyCloner;
			clonerCache[typeof(global::Lime.VideoPlayer)] = Clone_Lime__VideoPlayer;
			clonerCache[typeof(global::Lime.Viewport3D)] = Clone_Lime__Viewport3D;
			clonerCache[typeof(global::Lime.VignetteMaterial)] = Clone_Lime__VignetteMaterial;
			clonerCache[typeof(global::Lime.WaveComponent)] = Clone_Lime__WaveComponent;
			clonerCache[typeof(global::Lime.WaveMaterial)] = Clone_Lime__WaveMaterial;
			clonerCache[typeof(global::Lime.Widget)] = Clone_Lime__Widget;
			clonerCache[typeof(global::Lime.WidgetAdapter3D)] = Clone_Lime__WidgetAdapter3D;
			clonerCache[typeof(global::Lime.VideoDecoder.YUVtoRGBMaterial)] = Clone_Lime__VideoDecoder__YUVtoRGBMaterial;
		}
	}
}
