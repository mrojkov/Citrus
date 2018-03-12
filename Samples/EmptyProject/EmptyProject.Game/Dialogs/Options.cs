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
			var property = GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
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
				toggleValue?.Invoke();
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
			AppData.Save();
		}

		private bool MusicEnabled
		{
			get { return SoundManager.MusicVolume > 0; }
			set { SoundManager.MusicVolume = value ? 1.0f : 0; }
		}

		private bool SoundEnabled
		{
			get { return SoundManager.SfxVolume > 0; }
			set { SoundManager.SfxVolume = value ? 1.0f : 0; }
		}

		private bool VoiceEnabled
		{
			get { return SoundManager.VoiceVolume > 0; }
			set { SoundManager.VoiceVolume = value ? 1.0f : 0; }
		}

		private bool Fullscreen
		{
			get { return Window.Fullscreen; }
			set { Window.Fullscreen = value; }
		}
	}
}
