using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lime;
using System.Reflection;
using System.IO;

namespace EmptyProject
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
				() => The.Application.FullScreen,
				() => { The.Application.FullScreen = !The.Application.FullScreen; },
				autoUpdate: true
			);

			Root["BtnOk"].Clicked = Close;
		}

		public override void Close()
		{
			The.ApplicationData.Save();
			base.Close();
		}

		static void CreateCheckBox(Widget root, Func<bool> onGetValue, Action onToggleValue, bool autoUpdate = false)
		{
			Widget check = root["Check"];
			check.RunAnimation(onGetValue() ? "Checked" : "Unchecked");
			root["BtnCheck"].Clicked = () => {
				onToggleValue.SafeInvoke();
				check.RunAnimation(onGetValue() ? "Check" : "Uncheck");
			};
			if (autoUpdate) {
				root.Updating += (delta) => {
					if (check.IsStopped)
						check.RunAnimation(onGetValue() ? "Checked" : "Unchecked");
				};
			}
		}

		static bool MusicEnabled {
			get { return AudioSystem.GetGroupVolume(AudioChannelGroup.Music) > 0; }
			set { AudioSystem.SetGroupVolume(AudioChannelGroup.Music, value ? 1.0f : 0); }
		}

		static bool SoundEnabled
		{
			get { return AudioSystem.GetGroupVolume(AudioChannelGroup.Effects) > 0; }
			set { AudioSystem.SetGroupVolume(AudioChannelGroup.Effects, value ? 1.0f : 0); }
		}

		static bool VoiceEnabled
		{
			get { return AudioSystem.GetGroupVolume(AudioChannelGroup.Voice) > 0; }
			set { AudioSystem.SetGroupVolume(AudioChannelGroup.Voice, value ? 1.0f : 0); }
		}
	}

}
