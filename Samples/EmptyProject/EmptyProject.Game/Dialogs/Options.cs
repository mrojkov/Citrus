using System;
using Lime;

namespace EmptyProject.Dialogs
{
	public class Options : Dialog
	{
		public Options()
			: base("Shell/Options")
		{
			CreateCheckBox(Root["NoMusicCheckGroup"],
				() => MusicEnabled,
				() => { MusicEnabled = !MusicEnabled; }
			);

			CreateCheckBox(Root["NoSoundCheckGroup"],
				() => SoundEnabled,
				() => { SoundEnabled = !SoundEnabled; }
			);

			CreateCheckBox(Root["NoVoiceCheckGroup"],
				() => VoiceEnabled,
				() => { VoiceEnabled = !VoiceEnabled; }
			);

			CreateCheckBox(Root["FullScreenCheckGroup"],
				() => The.Window.Fullscreen,
				() => { The.Window.Fullscreen = !The.Window.Fullscreen; },
				autoUpdate: true
			);

			Root["BtnOk"].Clicked = Close;
		}

		public override void Close()
		{
			The.AppData.Save();
			base.Close();
		}

		static void CreateCheckBox(Widget root, Func<bool> onGetValue, Action onToggleValue, bool autoUpdate = false)
		{
			var check = root["Check"];
			check.RunAnimation(onGetValue() ? "Checked" : "Unchecked");
			root["BtnCheck"].Clicked = () => {
				onToggleValue?.Invoke();
				check.RunAnimation(onGetValue() ? "Check" : "Uncheck");
			};
			if (autoUpdate) {
				root.Updating += (delta) => {
					if (check.IsStopped)
						check.RunAnimation(onGetValue() ? "Checked" : "Unchecked");
				};
			}
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
	}

}
