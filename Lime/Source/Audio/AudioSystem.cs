namespace Lime
{
	/// <summary>
	/// Аудиосистема. Управляет проигрыванием всех звуков
	/// </summary>
	public static class AudioSystem
	{
		static readonly float[] groupVolumes = new float[3] {1, 1, 1};

		public static void Initialize()
		{
			PlatformAudioSystem.Initialize();
		}

		public static void Terminate()
		{
			PlatformAudioSystem.Terminate();
		}

		/// <summary>
		/// Если установить это свойство в false, то все звуки поставятся на паузу
		/// </summary>
		public static bool Active
		{
			get { return PlatformAudioSystem.Active; }
			set { PlatformAudioSystem.Active = value; }
		}

		/// <summary>
		/// Возвращает громкость для указанной группы звуковых каналов. От 0 до 1
		/// </summary>
		public static float GetGroupVolume(AudioChannelGroup group)
		{
			return groupVolumes[(int)group];
		}

		/// <summary>
		/// Задает громкость для указанной группы звуковых каналов. От 0 до 1
		/// </summary>
		public static float SetGroupVolume(AudioChannelGroup group, float value)
		{
			float oldVolume = groupVolumes[(int)group];
			value = Mathf.Clamp(value, 0, 1);
			groupVolumes[(int)group] = value;
			PlatformAudioSystem.SetGroupVolume(group, value);
			return oldVolume;
		}

		/// <summary>
		/// Ставит на паузу все звуки указанной группы звуковых каналов
		/// </summary>
		public static void PauseGroup(AudioChannelGroup group)
		{
			PlatformAudioSystem.PauseGroup(group);
		}

		/// <summary>
		/// Отменяет паузу для всех звуков указанной группы
		/// </summary>
		public static void ResumeGroup(AudioChannelGroup group)
		{
			PlatformAudioSystem.ResumeGroup(group);
		}

		/// <summary>
		/// Ставит на паузу все звуки
		/// </summary>
		public static void PauseAll()
		{
			PlatformAudioSystem.PauseAll();
		}

		/// <summary>
		/// Отменяет паузу для всех звуков
		/// </summary>
		public static void ResumeAll()
		{
			PlatformAudioSystem.ResumeAll();
		}

		/// <summary>
		/// Оповещает все звуки о том, что они все еще должны проигрываться.
		/// Звуки, которые долго не оповещали, останавливаются. Используется для остановки звуков,
		/// если приложение зависло или что-то долго делает
		/// </summary>
		public static void BumpAll()
		{
			PlatformAudioSystem.BumpAll();
		}

		/// <summary>
		/// Останавливает звуки указанной группы
		/// </summary>
		/// <param name="fadeoutTime">Время плавного затухания звука в секундах</param>
		public static void StopGroup(AudioChannelGroup group, float fadeoutTime = 0)
		{
			PlatformAudioSystem.StopGroup(group, fadeoutTime);
		}

		/// <summary>
		/// Обновляет состояние аудиосистемы
		/// </summary>
		public static void Update()
		{
			PlatformAudioSystem.Update();
		}

		/// <summary>
		/// Создает звук с заданными параметрами
		/// </summary>
		/// <param name="path">Путь звука в бандле относительно папки Audio (имя файла без расширения)</param>
		/// <param name="group">Группа звуковых эффектов для этого звука</param>
		/// <param name="looping">Зацикленное проигрывание</param>
		/// <param name="priority">Если не будет хватать числа свободных аудиоканалов, то каналы с более низким приоритетом будут уступать ресурсы каналу этого звука</param>
		/// <param name="fadeinTime">Время плавного нарастания громкости в момент начала воспроизведения</param>
		/// <param name="paused">Изначально звук будет на паузе</param>
		/// <param name="volume">Громкость. От 0 до 1</param>
		/// <param name="pan">Смещение. 0 - слева, 1 - справа, 0.5 - посередине</param>
		/// <param name="pitch">Высота звука</param>
		public static Sound Play(string path, AudioChannelGroup group, bool looping = false, float priority = 0.5f, float fadeinTime = 0f, bool paused = false, float volume = 1f, float pan = 0f, float pitch = 1f)
		{
			if (group == AudioChannelGroup.Music && CommandLineArgs.NoMusic) {
				return new Sound();
			}
			return PlatformAudioSystem.Play(path, group, looping, priority, fadeinTime, paused, volume, pan, pitch);
		}

		/// <summary>
		/// Создает звук с заданными параметрами и назначает его каналу музыки
		/// </summary>
		/// <param name="path">Путь звука в бандле относительно папки Audio (имя файла без расширения)</param>
		/// <param name="group">Группа звуковых эффектов для этого звука</param>
		/// <param name="looping">Зацикленное проигрывание</param>
		/// <param name="priority">Если не будет хватать числа свободных аудиоканалов, то каналы с более низким приоритетом будут уступать ресурсы каналу этого звука</param>
		/// <param name="fadeinTime">Время плавного нарастания громкости в момент начала воспроизведения</param>
		/// <param name="paused">Изначально звук будет на паузе</param>
		/// <param name="volume">Громкость. От 0 до 1</param>
		/// <param name="pan">Смещение. 0 - слева, 1 - справа, 0.5 - посередине</param>
		/// <param name="pitch">Высота звука</param>
		public static Sound PlayMusic(string path, bool looping = true, float priority = 100f, float fadeinTime = 0.5f, bool paused = false, float volume = 1f, float pan = 0f, float pitch = 1f)
		{
			return Play(path, AudioChannelGroup.Music, looping, priority, fadeinTime, paused, volume, pan, pitch);
		}

		/// <summary>
		/// Создает звук с заданными параметрами и назначает его каналу эффектов
		/// </summary>
		/// <param name="path">Путь звука в бандле относительно папки Audio (имя файла без расширения)</param>
		/// <param name="group">Группа звуковых эффектов для этого звука</param>
		/// <param name="looping">Зацикленное проигрывание</param>
		/// <param name="priority">Если не будет хватать числа свободных аудиоканалов, то каналы с более низким приоритетом будут уступать ресурсы каналу этого звука</param>
		/// <param name="fadeinTime">Время плавного нарастания громкости в момент начала воспроизведения</param>
		/// <param name="paused">Изначально звук будет на паузе</param>
		/// <param name="volume">Громкость. От 0 до 1</param>
		/// <param name="pan">Смещение. 0 - слева, 1 - справа, 0.5 - посередине</param>
		/// <param name="pitch">Высота звука</param>
		public static Sound PlayEffect(string path, bool looping = false, float priority = 0.5f, float fadeinTime = 0f, bool paused = false, float volume = 1f, float pan = 0f, float pitch = 1f)
		{
			return Play(path, AudioChannelGroup.Effects, looping, priority, fadeinTime, paused, volume, pan, pitch);
		}
	}
}