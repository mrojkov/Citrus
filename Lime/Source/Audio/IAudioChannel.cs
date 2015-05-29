using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	/// <summary>
	/// Состояния звукового канала
	/// </summary>
	public enum AudioChannelState
	{
		Initial,
		Playing,
		Stopped,
		Paused
	}

	/// <summary>
	/// Группы звуковых каналов
	/// </summary>
	[ProtoContract]
	public enum AudioChannelGroup
	{
		[ProtoEnum]
		Effects,
		[ProtoEnum]
		Music,
		[ProtoEnum]
		Voice
	}

	/// <summary>
	/// Интерфейс звукового канала
	/// </summary>
	public interface IAudioChannel
	{
		/// <summary>
		/// Текущее состояние
		/// </summary>
		AudioChannelState State { get; }

		/// <summary>
		/// Группа
		/// </summary>
		AudioChannelGroup Group { get; set; }

		/// <summary>
		/// Смещение. 0 - слева, 1 - справа, 0.5 - посередине
		/// </summary>
		float Pan { get; set; }

		/// <summary>
		/// Проиграть звук с момента его остановки методом Stop
		/// </summary>
		/// <param name="fadeinTime">Время плавного нарастания громкости в секундах</param>
		void Resume(float fadeinTime = 0);

		/// <summary>
		/// Остановить проигрывание звука
		/// </summary>
		/// <param name="fadeoutTime">Время плавного уменьшения громкости в секундах</param>
		void Stop(float fadeoutTime = 0);

		/// <summary>
		/// Громкость. От 0 до 1.
		/// </summary>
		float Volume { get; set; }

		/// <summary>
		/// Высота звука
		/// </summary>
		float Pitch { get; set; }

		/// <summary>
		/// Путь к файлу, из которого был загружен звук
		/// </summary>
		string SamplePath { get; set; }

		/// <summary>
		/// Звук, назначенный этому каналу
		/// </summary>
		Sound Sound { get; }

		/// <summary>
		/// Оповестить звук, что он все еще нужен. Звуки, которых долго не оповещали, автоматически останавливаются
		/// </summary>
		void Bump();
	}

	/// <summary>
	/// Звуковой канал, не проигрывающий никаких звуков
	/// </summary>
	public class NullAudioChannel : IAudioChannel
	{
		public static NullAudioChannel Instance = new NullAudioChannel();

		public AudioChannelState State { get { return AudioChannelState.Stopped; } }
		public AudioChannelGroup Group { get; set; }
		public float Pan { get { return 0; } set { } }
		public void Resume(float fadeinTime = 0) {}
		public void Stop(float fadeoutTime = 0) {}
		public float Volume { get { return 0; } set { } }
		public float Pitch { get { return 1; } set { } }
		public void Bump() {}
		public string SamplePath { get; set; }
		public Sound Sound { get { return null; } }
	}
}
