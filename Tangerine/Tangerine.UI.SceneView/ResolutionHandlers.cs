using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class ResolutionPreviewHandler : DocumentCommandHandler
	{
		public override bool GetChecked() => Document.Current.ResolutionPreview.Enabled;

		public override void ExecuteTransaction() => Execute(Document.Current, !Document.Current.ResolutionPreview.Enabled);

		public static void Execute(Document document, bool enable)
		{
			var resolutionPreview = document.ResolutionPreview;
			resolutionPreview.Enabled = enable;
			Execute(document, resolutionPreview);
		}

		public static void Execute(Document document, ResolutionPreview resolutionPreview)
		{
			if (!resolutionPreview.Enabled && !document.ResolutionPreview.Enabled) {
				return;
			}
			if (!(document.RootNode is Widget)) {
				if (document.ResolutionPreview.Enabled) {
					resolutionPreview.Enabled = false;
					PerformResolutionPreviewOperation(resolutionPreview);
				}
				return;
			}

			var requiredSave = true;
			if (resolutionPreview.Enabled) {
				if (resolutionPreview.Preset == null) {
					resolutionPreview.Preset = ProjectPreferences.Instance.Resolutions.First();
					resolutionPreview.IsPortrait = !ProjectPreferences.Instance.IsLandscapeDefault;
				}
			} else {
				resolutionPreview.Preset = ProjectPreferences.Instance.Resolutions.First();
				resolutionPreview.IsPortrait = !ProjectPreferences.Instance.IsLandscapeDefault;
				requiredSave = false;
			}
			PerformResolutionPreviewOperation(resolutionPreview, requiredSave);
			Application.InvalidateWindows();
		}

		private static void PerformResolutionPreviewOperation(ResolutionPreview resolutionPreview, bool requiredSave = true)
		{
			using (Document.Current.History.BeginTransaction()) {
				ResolutionPreviewOperation.Perform(resolutionPreview, requiredSave);
				Document.Current.History.CommitTransaction();
			}
		}
	}

	public class ResolutionChangerHandler : DocumentCommandHandler
	{
		private static ProjectPreferences Preferences => ProjectPreferences.Instance;

		private readonly bool isReverse;

		public ResolutionChangerHandler(bool isReverse = false)
		{
			this.isReverse = isReverse;
		}

		public override void ExecuteTransaction()
		{
			var document = Document.Current;
			if (!(document.RootNode is Widget)) {
				ResolutionPreviewHandler.Execute(Document.Current, enable: false);
				return;
			}

			var resolutions = Preferences.Resolutions;
			var resolutionPreview = document.ResolutionPreview;
			resolutionPreview.Enabled = true;
			if (resolutionPreview.Preset == null) {
				resolutionPreview.IsPortrait = !ProjectPreferences.Instance.IsLandscapeDefault;
				resolutionPreview.Preset = !isReverse ? resolutions.First() : resolutions.Last();
			} else {
				var index = ((List<ResolutionPreset>)resolutions).IndexOf(resolutionPreview.Preset);
				var shift = document.ResolutionPreview.Enabled ? (!isReverse ? 1 : -1) : 0;
				index = Mathf.Wrap(index + shift, 0, resolutions.Count - 1);
				resolutionPreview.Preset = resolutions[index];
			}
			ResolutionPreviewHandler.Execute(Document.Current, resolutionPreview);
		}
	}

	public class ResolutionOrientationHandler : DocumentCommandHandler
	{
		private static ProjectPreferences Preferences => ProjectPreferences.Instance;

		public override void ExecuteTransaction()
		{
			var document = Document.Current;
			if (!(document.RootNode is Widget)) {
				ResolutionPreviewHandler.Execute(Document.Current, enable: false);
				return;
			}

			var resolutionPreview = document.ResolutionPreview;
			resolutionPreview.Enabled = true;
			if (resolutionPreview.Preset == null) {
				resolutionPreview.IsPortrait = !ProjectPreferences.Instance.IsLandscapeDefault;
				resolutionPreview.Preset = Preferences.Resolutions.First();
			} else {
				resolutionPreview.IsPortrait = !resolutionPreview.IsPortrait;
			}
			ResolutionPreviewHandler.Execute(Document.Current, resolutionPreview);
		}
	}

	public class ResolutionPreviewOperation : Operation
	{
		private static bool ResolutionPreviewMode
		{
			set {
				var document = Document.Current;
				if (!document.ResolutionPreview.Enabled && value) {
					document.Saving += DocumentOnSaving;
				} else if (document.ResolutionPreview.Enabled && !value) {
					document.Saving -= DocumentOnSaving;
				}
				document.ResolutionPreview = new ResolutionPreview {
					Enabled = value,
					Preset = document.ResolutionPreview.Preset,
					IsPortrait = document.ResolutionPreview.IsPortrait
				};
			}
		}

		private static ResolutionPreview DisabledResolutionPreview => new ResolutionPreview {
			Enabled = false,
			Preset = ProjectPreferences.Instance.Resolutions.First(),
			IsPortrait = !ProjectPreferences.Instance.IsLandscapeDefault
		};

		private readonly ResolutionPreview resolutionPreview;
		private readonly bool requiredSave;

		public override bool IsChangingDocument => false;

		private static void DocumentOnSaving(Document document) => Perform(DisabledResolutionPreview, requiredSave: false);

		public static void Perform(ResolutionPreview resolutionPreview, bool requiredSave = true) =>
			DocumentHistory.Current.Perform(new ResolutionPreviewOperation(resolutionPreview, requiredSave));

		public ResolutionPreviewOperation(ResolutionPreview resolutionPreview, bool requiredSave)
		{
			this.resolutionPreview = resolutionPreview;
			this.requiredSave = requiredSave;
		}

		public class Processor : OperationProcessor<ResolutionPreviewOperation>
		{
			protected override void InternalRedo(ResolutionPreviewOperation op) => ApplyResolutionPreset(op.resolutionPreview, op.requiredSave);
			protected override void InternalUndo(ResolutionPreviewOperation op) => ApplyResolutionPreset(DisabledResolutionPreview, requiredSave: false);

			private static void ApplyResolutionPreset(ResolutionPreview resolutionPreview, bool requiredSave)
			{
				if (!(Document.Current.RootNode is Widget rootNode)) {
					return;
				}
				ResolutionPreviewMode = resolutionPreview.Enabled;
				if (requiredSave) {
					Document.Current.ResolutionPreview = resolutionPreview;
				}

				var defaultResolution = ProjectPreferences.Instance.DefaultResolution;
				Vector2 resolution;
				if (resolutionPreview.IsPortrait) {
					resolution = new Vector2(
						defaultResolution.LandscapeValue.Y,
						defaultResolution.LandscapeValue.Y * (resolutionPreview.Preset.LandscapeValue.X / resolutionPreview.Preset.LandscapeValue.Y)
					);
				} else {
					resolution = new Vector2(
						defaultResolution.LandscapeValue.Y * (resolutionPreview.Preset.LandscapeValue.X / resolutionPreview.Preset.LandscapeValue.Y),
						defaultResolution.LandscapeValue.Y
					);
				}
				rootNode.Size = resolution;
				ApplyResolutionMarkers(rootNode, resolutionPreview.Preset, resolutionPreview.IsPortrait);
				ApplyLocalizationMarkers(rootNode);
			}

			private static void ApplyResolutionMarkers(Node rootNode, ResolutionPreset resolutionPreset, bool isPortrait)
			{
				var markers = resolutionPreset.GetMarkers(isPortrait);

				void ApplyResolutionMarkerToNode(Node node)
				{
					foreach (var animation in node.Animations) {
						foreach (var marker in markers) {
							if (animation.TryRun(marker)) {
								break;
							}
						}
					}
				}

				ApplyResolutionMarkerToNode(rootNode);
				foreach (var descendant in rootNode.Descendants) {
					ApplyResolutionMarkerToNode(descendant);
				}
			}

			private static void ApplyLocalizationMarkers(Node rootNode)
			{
				var marker = $"@{Project.Current.Localization.Code}";
				void Apply(Node node)
				{
					foreach (var animation in node.Animations) {
						animation.TryRun(marker);
					}
				}
				Apply(rootNode);
				foreach (var descendant in rootNode.Descendants) {
					Apply(descendant);
				}
			}
		}
	}
}
