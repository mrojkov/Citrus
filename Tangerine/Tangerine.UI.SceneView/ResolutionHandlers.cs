using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class ResolutionPreviewHandler : DocumentCommandHandler
	{
		private static ProjectPreferences Preferences => ProjectPreferences.Instance;

		public static bool Enabled
		{
			get => Document.Current.ResolutionPreview.Enable;
			set {
				if (!Document.Current.ResolutionPreview.Enable && value) {
					Document.Current.Saving += DocumentOnSaving;
				} else if (Document.Current.ResolutionPreview.Enable && !value) {
					Document.Current.Saving -= DocumentOnSaving;
				}
				Document.Current.ResolutionPreview.Enable = value;
			}
		}

		public override bool GetChecked() => Enabled;

		public override void Execute() => Execute(Document.Current, !Enabled);

		public static void Execute(Document document, bool enable)
		{
			var rootNode = document.RootNode as Widget;
			if (rootNode == null) {
				Enabled = false;
				return;
			}

			Enabled = enable;
			if (Enabled) {
				if (document.ResolutionPreview.Preset == null) {
					document.ResolutionPreview.Preset = Preferences.Resolutions.First();
				}
				ApplyResolutionPreset(rootNode, document.ResolutionPreview.Preset, document.ResolutionPreview.IsPortrait);
			} else {
				ApplyResolutionPreset(rootNode, Preferences.Resolutions.First(), isPortrait: false);
			}
			Application.InvalidateWindows();
		}

		private static void ApplyResolutionPreset(Node node, ResolutionPreset resolutionPreset, bool isPortrait)
		{
			var defaultResolution = ProjectPreferences.Instance.DefaultResolution;
			Vector2 resolution;
			if (isPortrait) {
				resolution = new Vector2(
					defaultResolution.LandscapeValue.Y,
					defaultResolution.LandscapeValue.Y * (resolutionPreset.LandscapeValue.X / resolutionPreset.LandscapeValue.Y)
				);
			} else {
				resolution = new Vector2(
					defaultResolution.LandscapeValue.Y * (resolutionPreset.LandscapeValue.X / resolutionPreset.LandscapeValue.Y),
					defaultResolution.LandscapeValue.Y
				);
			}
			Core.Operations.SetProperty.Perform(node, nameof(Widget.Size), resolution);
			ApplyResolutionAnimations(node, resolutionPreset, isPortrait);
		}

		private static void ApplyResolutionAnimations(Node node, ResolutionPreset resolutionPreset, bool isPortrait)
		{
			var animations = resolutionPreset.GetAnimations(isPortrait);
			ApplyResolutionAnimation(node, animations);
			foreach (var descendant in node.Descendants) {
				ApplyResolutionAnimation(descendant, animations);
			}
		}

		private static void ApplyResolutionAnimation(Node node, IEnumerable<string> animations)
		{
			foreach (var animation in animations) {
				if (node.TryRunAnimation(animation)) {
					break;
				}
			}
		}

		private static void DocumentOnSaving(Document document) => Execute(document, false);
	}

	public class ResolutionChangerHandler : DocumentCommandHandler
	{
		private static ProjectPreferences Preferences => ProjectPreferences.Instance;

		private readonly bool isReverse;

		public ResolutionChangerHandler(bool isReverse = false)
		{
			this.isReverse = isReverse;
		}

		public override void Execute()
		{
			var document = Document.Current;
			var rootNode = document.RootNode as Widget;
			if (rootNode == null) {
				ResolutionPreviewHandler.Enabled = false;
				return;
			}

			var resolutions = Preferences.Resolutions;
			if (document.ResolutionPreview.Preset == null) {
				document.ResolutionPreview.IsPortrait = false;
				document.ResolutionPreview.Preset = !isReverse ? resolutions.First() : resolutions.Last();
			} else {
				var index = ((List<ResolutionPreset>)resolutions).IndexOf(document.ResolutionPreview.Preset);
				var shift = ResolutionPreviewHandler.Enabled ? (!isReverse ? 1 : -1) : 0;
				index = Mathf.Wrap(index + shift, 0, resolutions.Count - 1);
				document.ResolutionPreview.Preset = resolutions[index];
			}
			ResolutionPreviewHandler.Execute(Document.Current, enable: true);
		}
	}

	public class ResolutionOrientationHandler : DocumentCommandHandler
	{
		private static ProjectPreferences Preferences => ProjectPreferences.Instance;

		public override void Execute()
		{
			var document = Document.Current;
			var rootNode = document.RootNode as Widget;
			if (rootNode == null) {
				ResolutionPreviewHandler.Enabled = false;
				return;
			}

			if (document.ResolutionPreview.Preset == null) {
				document.ResolutionPreview.IsPortrait = false;
				document.ResolutionPreview.Preset = Preferences.Resolutions.First();
			} else {
				document.ResolutionPreview.IsPortrait = !document.ResolutionPreview.IsPortrait;
			}
			ResolutionPreviewHandler.Execute(Document.Current, enable: true);
		}
	}
}
