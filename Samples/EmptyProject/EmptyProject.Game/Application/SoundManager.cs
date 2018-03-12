using System;
using System.Collections.Generic;
using Lime;

namespace EmptyProject.Application
{
	public class SoundManager
	{
		public static readonly SoundManager Instance = new SoundManager();

		private SoundManager()
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

		private readonly Dictionary<string, DateTime> effectTimestamps = new Dictionary<string, DateTime>();

		public Sound PlaySound(string soundFile, string polyphonicGroup = null, float pan = 0, int polyphonicMinInterval = 50, float pitch = 1, float priority = 0.5f, float volume = 1.0f, bool looped = false)
		{
			if (polyphonicGroup != null) {
				DateTime value;
				var now = DateTime.Now;
				if (effectTimestamps.TryGetValue(polyphonicGroup, out value) && (now - value).Milliseconds < polyphonicMinInterval) {
					return new Sound();
				}
				effectTimestamps[polyphonicGroup] = now;
			}
			var path = soundFile;
			var sound = AudioSystem.Play("Audio/Sounds/" + path, AudioChannelGroup.Effects, looped, priority, 0, false, volume, pan);
			if (pitch != 1) {
				sound.Pitch = pitch;
			}
			return sound;
		}

		private Sound jingleInstance;

		public void PlayJingle(string soundFile)
		{
			musicInstance?.Stop(0.5f);
			jingleInstance?.Stop(0.5f);
			jingleInstance = AudioSystem.PlayMusic("Audio/Music/" + soundFile, false);
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
			var music = Music;
			Music = null;
			PlayMusic(music);
		}

		private readonly Stack<string> musicStack = new Stack<string>();

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
			public readonly Sound OldMusic;
			public readonly float Duration;
			public float TimePassed;

			public bool TimeToOccur => TimePassed >= Duration;

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
			if (Ambient == ambientName)
				return Ambient;
			var oldAmbient = Ambient;
			ambientInstance?.Stop(0.5f);
			Ambient = ambientName;
			ambientInstance = ambientName == null ? null :
				AudioSystem.PlayEffect("Audio/Ambient/" + ambientName, true, 100, 0.5f);
			return oldAmbient;
		}

		public string PlayMusic(string musicName, bool looped = true, float volume = 1.0f)
		{
			const float fadeoutDuration = 0.5f;

			if (Music == musicName)
				return Music;
			var oldMusic = Music;
			Music = musicName;
			if (jingleInstance != null)
				return oldMusic;
			if (musicInstance != null) {
				musicChanged = new MusicChangedEvent(musicInstance, fadeoutDuration);
			}
			musicInstance = musicName == null ? null :
				AudioSystem.PlayMusic("Audio/Music/" + musicName, looped, 100, fadeoutDuration, false, volume);
			return oldMusic;
		}

		public string StopMusic(float fadeoutTime = 0)
		{
			if (Music == null)
				return Music;
			var oldMusic = Music;
			Music = null;
			if (jingleInstance != null)
				return oldMusic;
			musicInstance?.Stop(fadeoutTime);
			musicInstance = null;
			return oldMusic;
		}
	}
}
