using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lime;
using System.IO;

namespace EmptyProject
{
	public class SoundManager
	{
		public static SoundManager Instance = new SoundManager();

		public SoundManager()
		{
			The.World.Updated += Update;
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
			string path = "Audio/Sounds/" + soundFile;
			var sound = AudioSystem.Play(path, AudioChannelGroup.Effects, looped, priority, 0, false, volume, pan);
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

		public string Music { get; private set; }
		public string Ambient { get; private set; }

		public Sound PlayVoice(string name, bool looping = false, float priority = 2.0f, float fadeinTime = 0f, bool paused = false, float volume = 1f, float pan = 0f, float pitch = 1f)
		{
			return AudioSystem.Play(
				GetVoiceFullPath(name),
				AudioChannelGroup.Voice,
				looping,
				priority,
				fadeinTime,
				paused,
				volume,
				pan,
				pitch
			);
		}

		public static string GetVoiceFullPath(string name)
		{
#if CHINA
			return "Audio/VO.CN/" + name;
#else
			return "Audio/VO/" + name;
#endif
		}

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
				} else {
					ambientInstance = null;
				}
				return oldAmbient;
			}
			return Ambient;
		}

		public string PlayMusic(string musicName, bool looped = true, float volume = 1.0f)
		{
			if (Music != musicName) {
				string oldMusic = Music;
				Music = musicName;
				if (jingleInstance == null) {
					if (musicInstance != null) {
						musicInstance.Stop(0.5f);
					}
					if (musicName != null) {
						musicInstance = Lime.AudioSystem.PlayMusic("Audio/Music/" + musicName, looped, 100, looped ? 0.5f : 0, false, volume);
					} else {
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
