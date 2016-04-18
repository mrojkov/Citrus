#if OPENAL
#if !MONOMAC
using OpenTK.Audio.OpenAL;
#else
using MonoMac.OpenAL;
#endif
#endif

namespace Lime
{
	/// <summary>
	/// Звук. Прежде чем управлять воспроизведением, убедитесь, что он загрузился (загружается из другого потока).
	/// Для этого проверяйте свойство IsLoading
	/// </summary>
	public class Sound
	{
		/// <summary>
		/// Конструктор. Изначально звук проигрываться не будет, потому что в качестве звукового канала ему установлен NullAudioChannel.
		/// Используйте AudioSystem.Play для быстрого создания и проигрывания звуков
		/// </summary>
		public Sound()
		{
			Channel = NullAudioChannel.Instance;
		}

		/// <summary>
		/// Аудиоканал, которому принадлежит этот звук
		/// </summary>
		public IAudioChannel Channel { get; internal set; }

		/// <summary>
		/// Для звука применима операция Bump
		/// </summary>
		public bool IsBumpable { get; set; }

		/// <summary>
		/// Возвращает true, если звук все еще загружается
		/// </summary>
		public bool IsLoading { get; internal set; }

		/// <summary>
		/// Возвращает true, если звук доигрался до конца, либо был остановлен методом Stop
		/// </summary>
		public bool IsStopped { get { return Channel.State == AudioChannelState.Stopped; } }

		/// <summary>
		/// Громкость. От 0 до 1.
		/// </summary>
		public float Volume
		{
			get { return Channel.Volume; }
			set { Channel.Volume = value; }
		}

		/// <summary>
		/// Высота звука
		/// </summary>
		public float Pitch
		{
			get { return Channel.Pitch; }
			set { Channel.Pitch = value; }
		}

		/// <summary>
		/// Смещение. -1 - слева, 1 - справа, 0 - посередине
		/// </summary>
		public float Pan
		{
			get { return Channel.Pan; }
			set { Channel.Pan = value; }
		}

		/// <summary>
		/// Проиграть звук с момента его остановки методом Stop
		/// </summary>
		/// <param name="fadeinTime">Время плавного нарастания громкости в секундах</param>
		public void Resume(float fadeinTime = 0)
		{
			EnsureLoaded();
			Channel.Resume(fadeinTime);
		}

		/// <summary>
		/// Остановить проигрывание звука
		/// </summary>
		/// <param name="fadeoutTime">Время плавного уменьшения громкости в секундах</param>
		public void Stop(float fadeoutTime = 0)
		{
			EnsureLoaded();
			Channel.Stop(fadeoutTime);
		}

		/// <summary>
		/// Оповестить звук, что он все еще нужен. Звуки, которых долго не оповещали, автоматически останавливаются
		/// </summary>
		public void Bump() { Channel.Bump(); }

		private void EnsureLoaded()
		{
			if (IsLoading) {
				throw new System.InvalidOperationException("The sound is being loaded");
			}
		}
	}
}
