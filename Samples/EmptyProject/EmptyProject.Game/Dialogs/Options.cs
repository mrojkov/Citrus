using System;
using System.Reflection;
using EmptyProject.Scenes.Common;
using Lime;

namespace EmptyProject.Dialogs
{
	public class Options : Dialog<Scenes.Options>
	{
		public Options()
		{
			CreateCheckBox(Scene._MusicGroup.CheckGroup, nameof(MusicEnabled));
			CreateCheckBox(Scene._SoundGroup.CheckGroup, nameof(SoundEnabled));
			CreateCheckBox(Scene._VoiceGroup.CheckGroup, nameof(VoiceEnabled));
			CreateCheckBox(Scene._FullScreenGroup.CheckGroup, nameof(Fullscreen));
			Scene._BtnOk.It.Clicked = Close;
		}

		private void CreateCheckBox(CheckGroup checkGroup, string propertyName, bool autoUpdate = false)
		{
			var property = GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Static);
			var getValue = new Func<bool>(() => (bool) property.GetValue(this));
			var toggle = new Action(() => property.SetValue(this, !getValue()));
			CreateCheckBox(checkGroup, getValue, toggle, autoUpdate);
		}

		private static void CreateCheckBox(CheckGroup checkGroup,
			Func<bool> getValue, Action toggleValue, bool autoUpdate = false)
		{
			var check = checkGroup._Check;
			RunConditionalAnimation(getValue, check.RunAnimationChecked, check.RunAnimationUnchecked);
			checkGroup._BtnCheck.It.Clicked = () => {
				toggleValue.SafeInvoke();
				RunConditionalAnimation(getValue, check.RunAnimationCheck, check.RunAnimationUncheck);
			};
			if (autoUpdate) {
				checkGroup.It.Updating += delta => {
					if (check.It.IsStopped)
						RunConditionalAnimation(getValue, check.RunAnimationChecked, check.RunAnimationUnchecked);
				};
			}
		}

		private static void RunConditionalAnimation(Func<bool> getValue, Func<Frame> trueAnimation, Func<Frame> falseAnimation)
		{
			if (getValue())
				trueAnimation();
			else
				falseAnimation();
		}

		protected override void Closing()
		{
			base.Closing();

			The.AppData.Save();
		}

		private static bool MusicEnabled
		{
			get { return The.SoundManager.MusicVolume > 0; }
			set { The.SoundManager.MusicVolume = value ? 1.0f : 0; }
		}

		private static bool SoundEnabled
		{
			get { return The.SoundManager.SfxVolume > 0; }
			set { The.SoundManager.SfxVolume = value ? 1.0f : 0; }
		}

		private static bool VoiceEnabled
		{
			get { return The.SoundManager.VoiceVolume > 0; }
			set { The.SoundManager.VoiceVolume = value ? 1.0f : 0; }
		}

		private static bool Fullscreen
		{
			get { return The.Window.Fullscreen; }
			set { The.Window.Fullscreen = value; }
		}
	}
}
