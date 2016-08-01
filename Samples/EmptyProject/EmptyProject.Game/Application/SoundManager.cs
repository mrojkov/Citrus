using System;
using System.Collections.Generic;
using Lime;

namespace EmptyProject.Application
{
	public class SoundManager
	{
		public static SoundManager Instance = new SoundManager();

		public SoundManager()
		{
			The.World.Updated += Update;

			SfxVolume = AppData.Instance.EffectsVolume;
			MusicVolume = AppData.Instance.MusicVolume;
		}

		public float SfxVolume
		{
			get { return AudioSystem.GetGroupVolume(AudioChannelGroup.Effects); }
			set { AudioSystem.SetGroupVolume(AudioChannelGroup.Effects, value); }
		}

		public float MusicVolume
		{
			get { return AudioSystem.GetGroupVolume(AudioChannelGroup.Music); }
			set { AudioSystem.SetGroupVolume(AudioChannelGroup.Music, value); }
		}

		public float VoiceVolume
		{
			get { return AudioSystem.GetGroupVolume(AudioChannelGroup.Voice); }
			set { AudioSystem.SetGroupVolume(AudioChannelGroup.Voice, value); }
		}

		Dictionary<string, DateTime> effectTimestamps = new Dictionary<string, DateTime>();

		public Sound PlaySound(string soundFile, string polyphonicGroup = null, float pan = 0, int polyphonicMinInterval = 50, float pitch = 1, float priority = 0.5f, float volume = 1.0f, bool looped = false)
		{
			if (polyphonicGroup != null) {
				DateTime value;
				DateTime now = DateTime.Now;
				if (effectTimestamps.TryGetValue(polyphonicGroup, out value) && (now - value).Milliseconds < polyphonicMinInterval) {
					return new Sound();
				}
				effectTimestamps[polyphonicGroup] = now;
			}
			string path = soundFile;
			var sound = AudioSystem.Play("Audio/Sounds/" + path, AudioChannelGroup.Effects, looped, priority, 0, false, volume, pan);
			if (pitch != 1) {
				sound.Pitch = pitch;
			}
			return sound;
		}

		private Sound jingleInstance;

		public void PlayJingle(string soundFile)
		{
			if (musicInstance != null) {
				musicInstance.Stop(0.5f);
			}
			if (jingleInstance != null) {
				jingleInstance.Stop(0.5f);
			}
			jingleInstance = Lime.AudioSystem.PlayMusic("Audio/Music/" + soundFile, false, 100, 0.5f);
		}

		public void StopJingle()
		{
			if (jingleInstance != null) {
				jingleInstance.Stop(0.5f);
				jingleInstance = null;
				RestartMusic();
			}
		}

		public void Update(float delta)
		{
			if (jingleInstance != null) {
				if (jingleInstance.IsStopped) {
					jingleInstance = null;
					RestartMusic();
				}
			}

			if (musicChanged != null) {
				musicChanged.TimePassed += delta;

				if (musicChanged.TimeToOccur) {
					musicChanged.OldMusic.Stop(musicChanged.Duration);

					musicChanged = null;
				}
			}
		}

		private void RestartMusic()
		{
			string music = Music;
			Music = null;
			PlayMusic(music);
		}

		private Stack<string> musicStack = new Stack<string>();

		public void PushMusic()
		{
			musicStack.Push(Music);
		}

		public void PopMusic()
		{
			PlayMusic(musicStack.Pop());
		}

		private Sound musicInstance;
		private Sound ambientInstance;

		private sealed class MusicChangedEvent
		{
			public Sound OldMusic;
			public float Duration;
			public float TimePassed;

			public bool TimeToOccur { get { return TimePassed >= Duration; } }

			public MusicChangedEvent(Sound oldMusic, float duration)
			{
				OldMusic = oldMusic;
				Duration = duration;
				TimePassed = 0f;
			}
		}

		private MusicChangedEvent musicChanged;

		public string Music { get; private set; }
		public string Ambient { get; private set; }

		public string PlayAmbient(string ambientName)
		{
			if (Ambient != ambientName) {
				string oldAmbient = Ambient;
				if (ambientInstance != null) {
					ambientInstance.Stop(0.5f);
				}
				Ambient = ambientName;
				if (ambientName != null) {
					ambientInstance = Lime.AudioSystem.PlayEffect("Audio/Ambient/" + ambientName, true, 100, 0.5f);
				}
				else {
					ambientInstance = null;
				}
				return oldAmbient;
			}
			return Ambient;
		}

		public string PlayMusic(string musicName, bool looped = true, float volume = 1.0f)
		{
			const float fadeoutDuration = 0.5f;

			if (Music != musicName) {
				string oldMusic = Music;
				Music = musicName;
				if (jingleInstance == null) {
					if (musicInstance != null) {
						musicChanged = new MusicChangedEvent(musicInstance, fadeoutDuration);
					}
					if (musicName != null) {
						musicInstance = Lime.AudioSystem.PlayMusic("Audio/Music/" + musicName, looped, 100, fadeoutDuration, false, volume);
					}
					else {
						musicInstance = null;
					}
				}
				return oldMusic;
			}
			return Music;
		}

		public string StopMusic(float fadeoutTime = 0)
		{
			if (Music != null) {
				string oldMusic = Music;
				Music = null;
				if (jingleInstance == null) {
					if (musicInstance != null) {
						musicInstance.Stop(fadeoutTime);
					}
					musicInstance = null;
				}
				return oldMusic;
			}
			return Music;
		}
	}
}
